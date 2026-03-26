using Godot;
using LostEditor;
using System;
using System.Collections.Generic;

/*
    TimelineKeyframeControlSystem - класс управляющий кейфреймами на таймлайне
    (выделение, удаление, сброс выделения, перемещение выделенных)
*/

public partial class TimelineKeyframeControlSystem : Node
{
    [Export] KeyframesPanelMain keyframesPanel;
	private List<KeyframePoint> _selectedItems = new();

	private bool _isSelected = false;
	private bool _isDragging = false;
	private bool _hasMoved = false;

	private float _pixelsPerSecond = 100f;

	private float _startDragMouseX;
	private Dictionary<KeyframePoint, float> _startKeyframeTimes = new();

	public void SetPixelsPerSecond(float pps) => _pixelsPerSecond = pps;

    public KeyframePoint GetSelectedKeyframe()
    {
        if(_selectedItems[0] != null) return _selectedItems[0];
        else return null;
        
    }

    public void HandleKeyframeInput(KeyframePoint point, InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                _hasMoved = false; // Сбрасываем флаг движения при каждом новом клике
                
                bool isCtrl = Input.IsKeyPressed(Key.Ctrl) || Input.IsKeyPressed(Key.Shift);
                bool isAlreadySelected = _selectedItems.Contains(point);

                // Если кликаем БЕЗ Ctrl по НЕВЫДЕЛЕННОМУ — сбрасываем всё сразу и выделяем этот
                if (!isCtrl && !isAlreadySelected)
                {
                    DeselectAll();
                    Select(point);
                }
                // Если с Ctrl — просто добавляем/убираем из выделения
                else if (isCtrl)
                {
                    if (isAlreadySelected) 
                    {
                        // Опционально: можно реализовать деселект при повторном клике с Ctrl
                        // Но пока просто оставим выбранным для драга
                    }
                    else Select(point);
                }
                // Если кликаем БЕЗ Ctrl по УЖЕ ВЫДЕЛЕННОМУ — ничего не делаем (ждем: будет драг или просто клик?)

                keyframesPanel.UpdateKeyframeData(point.KeyframeData);

                // Инициализация драга
                _isDragging = true;
                _startDragMouseX = point.GetGlobalMousePosition().X;
                _startKeyframeTimes.Clear();
                foreach (var item in _selectedItems)
                {
                    _startKeyframeTimes[item] = item.KeyframeData.Time;
                }

                GetViewport().SetInputAsHandled();
            }
            else // Кнопка отпущена
            {
                if (_isDragging)
                {
                    // ЛОГИКА ОДИНОЧНОГО КЛИКА:
                    // Если мы не двигали мышь и не был нажат Ctrl
                    if (!_hasMoved && !Input.IsKeyPressed(Key.Ctrl) && !Input.IsKeyPressed(Key.Shift))
                    {
                        // Сбрасываем всё и выделяем только тот, по которому кликнули в конце
                        DeselectAll();
                        Select(point);
                    }

                    if (_hasMoved)
                    {
                        FinalizeDrag();
                    }
                    
                    _isDragging = false;
                    _hasMoved = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion mm && _isDragging)
        {
            // Считаем движением только если мышь сместилась хотя бы на пару пикселей (защита от микро-дрожания)
            if (Math.Abs(mm.GlobalPosition.X - _startDragMouseX) > 2f)
            {
                _hasMoved = true;
                MoveSelected(mm.GlobalPosition.X);
            }
        }
    }

    public void RequestCreateKeyframe(KeyframeType type, float localMouseX)
    {
        // 1. Получаем текущий объект (нужен доступ к _currentObj из KeyframesPanelMain)
        // Лучше всего сделать свойство CurrentObject в KeyframesPanelMain публичным
        var currentObj = keyframesPanel.GetCurrentObject(); // <-- Сделай это свойство public в KeyframesPanelMain
        
        if (currentObj == null) 
        {
            GD.PrintErr("Нет выбранного объекта для создания ключа!");
            return;
        }

        // 2. Вычисляем время на основе позиции мыши и зума (PPS)
        // Используем Math.Max, чтобы время не было отрицательным
        float time = Math.Max(0, localMouseX / _pixelsPerSecond);

        // 3. Создаем данные
        CreateKeyframeData(currentObj, type, time);
    }

    private void CreateKeyframeData(GameObject obj, KeyframeType type, float time)
    {
        IKeyframe newKeyframeData = null;

        // --- 1. ЛОГИКА НАСЛЕДОВАНИЯ ---
        switch (type)
        {
            case KeyframeType.PositionX:
                var prevPX = GetPreviousKeyframe(obj.keyframePositionX, time);
                var kfPX = new Keyframe<float> { 
                    Time = time, 
                    kType = type,
                    // Если есть предыдущий — берем его значение, иначе 0 (или текущее X объекта)
                    Value = prevPX != null ? prevPX.Value : obj.Position.X, 
                    EasingType = prevPX != null ? prevPX.EasingType : EasingType.Linear 
                };
                obj.keyframePositionX.Add(kfPX);
                ResolveOverlapsAndSort(obj.keyframePositionX);
                newKeyframeData = kfPX;
                break;

            case KeyframeType.PositionY:
                var prevPY = GetPreviousKeyframe(obj.keyframePositionY, time);
                var kfPY = new Keyframe<float> { 
                    Time = time, kType = type,
                    Value = prevPY != null ? prevPY.Value : obj.Position.Y,
                    EasingType = prevPY != null ? prevPY.EasingType : EasingType.Linear 
                };
                obj.keyframePositionY.Add(kfPY);
                ResolveOverlapsAndSort(obj.keyframePositionY);
                newKeyframeData = kfPY;
                break;

            case KeyframeType.ScaleX:
                var prevSX = GetPreviousKeyframe(obj.keyframeScaleX, time);
                var kfSX = new Keyframe<float> { 
                    Time = time, kType = type,
                    Value = prevSX != null ? prevSX.Value : obj.Scale.X,
                    EasingType = prevSX != null ? prevSX.EasingType : EasingType.Linear 
                };
                obj.keyframeScaleX.Add(kfSX);
                ResolveOverlapsAndSort(obj.keyframeScaleX);
                newKeyframeData = kfSX;
                break;

            case KeyframeType.ScaleY:
                var prevSY = GetPreviousKeyframe(obj.keyframeScaleY, time);
                var kfSY = new Keyframe<float> { 
                    Time = time, kType = type,
                    Value = prevSY != null ? prevSY.Value : obj.Scale.Y,
                    EasingType = prevSY != null ? prevSY.EasingType : EasingType.Linear 
                };
                obj.keyframeScaleY.Add(kfSY);
                ResolveOverlapsAndSort(obj.keyframeScaleY);
                newKeyframeData = kfSY;
                break;

            case KeyframeType.Rotation:
                var prevR = GetPreviousKeyframe(obj.keyframeRotation, time);
                var kfR = new Keyframe<float> { 
                    Time = time, kType = type,
                    // Важно: если используем градусы, берем RotationDegrees
                    Value = prevR != null ? prevR.Value : obj.RotationDegrees,
                    EasingType = prevR != null ? prevR.EasingType : EasingType.Linear 
                };
                obj.keyframeRotation.Add(kfR);
                ResolveOverlapsAndSort(obj.keyframeRotation);
                newKeyframeData = kfR;
                break;

            case KeyframeType.Color:
                var prevC = GetPreviousKeyframe(obj.keyframeColor, time);
                    ObjectColor newColorValue;
                    if (prevC != null && prevC.Value != null)
                        newColorValue = prevC.Value.Clone();    // ИСПРАВЛЕНО: копия, не ссылка
                    else if (obj.objectColor != null)
                        newColorValue = obj.objectColor.Clone(); // ИСПРАВЛЕНО: копия, не ссылка
                    else
                        newColorValue = new ObjectColor();
                
                    var kfC = new Keyframe<ObjectColor> { 
                        Time = time,
                        kType = type,
                        Value = newColorValue,
                        EasingType = prevC != null ? prevC.EasingType : EasingType.Linear
                    };
                    obj.keyframeColor.Add(kfC);
                    ResolveOverlapsAndSort(obj.keyframeColor);
                    newKeyframeData = kfC;
                    break;
        }

        if (newKeyframeData == null) return;

        obj.RecalculateEndTime();
        obj.animCache.IsDirty = true; // ← ДОБАВИТЬ

        DeselectAll();
        keyframesPanel.LoadKeyframesToPanel(obj);

        // --- 3. АВТО-ВЫДЕЛЕНИЕ НОВОГО КЛЮЧА ---
        // Ищем среди созданных визуальных объектов тот, который привязан к нашим новым данным
        var allPoints = keyframesPanel.GetAllKeyframePoints();
        foreach (var point in allPoints)
        {
            if (point.KeyframeData == newKeyframeData)
            {
                Select(point);
                // Также обновляем инспектор данными этого ключа
                keyframesPanel.UpdateKeyframeData(point.KeyframeData);
                break;
            }
        }
        GD.Print($"Создан ключ {type} на времени {time}, данные унаследованы.");
    }



    private void ResolveOverlapsAndSort<T>(List<Keyframe<T>> list)
    {
        // Если список пуст или в нем 1 элемент, сортировать нечего
        if (list == null || list.Count <= 1) return;

        // 1. Сортируем список по времени (Time)
        // Используем встроенную сортировку List<T>
        list.Sort((a, b) => a.Time.CompareTo(b.Time));

        // 2. Устраняем наложения (если время совпадает)
        // Минимальный шаг, чтобы ключи не сливались
        float epsilon = 0.001f; 

        for (int i = 1; i < list.Count; i++)
        {
            var prev = list[i - 1];
            var curr = list[i];

            // Если текущий ключ по времени раньше или равен предыдущему
            if (curr.Time <= prev.Time)
            {
                // Сдвигаем текущий ключ чуть вперед относительно предыдущего
                curr.Time = prev.Time + epsilon;
            }
        }
    }

	private void Select(KeyframePoint point)
    {
        if (!_selectedItems.Contains(point))
        {
            _selectedItems.Add(point);
            point.SetSelected(true);
            int typeIndex = keyframesPanel.GetKeyframeTypeIndex(point.KeyframeData.kType);
            keyframesPanel.animationPanel.KeyframeDataTabContainer.CurrentTab = typeIndex + 1;
        }
    }

	public void DeselectAll()
	{
		foreach (var item in _selectedItems)
		{
			// Проверяем, существует ли объект в памяти Godot и не помечен ли на удаление
			if (GodotObject.IsInstanceValid(item))
			{
				item.SetSelected(false);
			}
		}
		_selectedItems.Clear();
        keyframesPanel.animationPanel.KeyframeDataTabContainer.CurrentTab = 0;
	}

    private void MoveSelected(float currentMouseX)
    {
        float mouseDiffX = currentMouseX - _startDragMouseX;
        float timeOffset = mouseDiffX / _pixelsPerSecond;

        foreach (var item in _selectedItems)
        {
            if (_startKeyframeTimes.TryGetValue(item, out float startTime))
            {
                float newTime = Math.Max(0, startTime + timeOffset);
                item.KeyframeData.Time = newTime;
                item.Position = new Vector2(newTime * _pixelsPerSecond, item.Position.Y);
            }
        }

        var currentObj = keyframesPanel.GetCurrentObject();
        if (currentObj == null) return;

        // Определяем какие списки затронуты и сортируем их,
        // чтобы инкрементальный поиск GetCurrentIndex работал корректно
        bool sortPosX = false, sortPosY = false;
        bool sortScaX = false, sortScaY = false;
        bool sortRot  = false, sortCol  = false;

        foreach (var item in _selectedItems)
        {
            switch (item.KeyframeData.kType)
            {
                case KeyframeType.PositionX: sortPosX = true; break;
                case KeyframeType.PositionY: sortPosY = true; break;
                case KeyframeType.ScaleX:    sortScaX = true; break;
                case KeyframeType.ScaleY:    sortScaY = true; break;
                case KeyframeType.Rotation:  sortRot  = true; break;
                case KeyframeType.Color:     sortCol  = true; break;
            }
        }

        if (sortPosX) currentObj.keyframePositionX.Sort((a, b) => a.Time.CompareTo(b.Time));
        if (sortPosY) currentObj.keyframePositionY.Sort((a, b) => a.Time.CompareTo(b.Time));
        if (sortScaX) currentObj.keyframeScaleX.Sort((a, b)    => a.Time.CompareTo(b.Time));
        if (sortScaY) currentObj.keyframeScaleY.Sort((a, b)    => a.Time.CompareTo(b.Time));
        if (sortRot)  currentObj.keyframeRotation.Sort((a, b)  => a.Time.CompareTo(b.Time));
        if (sortCol)  currentObj.keyframeColor.Sort((a, b)     => a.Time.CompareTo(b.Time));

        // Сбрасываем кэш индексов — список изменил порядок,
        // инкрементальный поиск должен стартовать заново
        currentObj.animCache.IsDirty = true;
    }

	private void FinalizeDrag()
    {
        var currentObj = keyframesPanel.GetCurrentObject();
        if (currentObj == null || _selectedItems.Count == 0) return;

        HashSet<IKeyframe> selectedData = new();
        foreach (var point in _selectedItems)
            selectedData.Add(point.KeyframeData);

        _selectedItems.Clear();

        SortAndFixList(currentObj.keyframePositionX);
        SortAndFixList(currentObj.keyframePositionY);
        SortAndFixList(currentObj.keyframeScaleX);
        SortAndFixList(currentObj.keyframeScaleY);
        SortAndFixList(currentObj.keyframeRotation);
        SortAndFixList(currentObj.keyframeColor);

        currentObj.RecalculateEndTime();
        currentObj.animCache.IsDirty = true; // ← ДОБАВИТЬ

        keyframesPanel.LoadKeyframesToPanel(currentObj);

        var newPoints = keyframesPanel.GetAllKeyframePoints();
        foreach (var point in newPoints)
        {
            if (selectedData.Contains(point.KeyframeData))
                Select(point);
        }

        if (_selectedItems.Count > 0)
            keyframesPanel.UpdateKeyframeData(_selectedItems[0].KeyframeData);

        GD.Print("Drag finished. Selection and Inspector synced.");
    }

    private Keyframe<T> GetPreviousKeyframe<T>(List<Keyframe<T>> list, float time)
    {
        if (list == null || list.Count == 0) return null;
        
        Keyframe<T> bestMatch = null;
        foreach (var kf in list)
        {
            // Ищем ключ, который ближе всего к новому времени, но стоит ДО него
            if (kf.Time <= time)
            {
                if (bestMatch == null || kf.Time > bestMatch.Time)
                    bestMatch = kf;
            }
        }
        return bestMatch;
    }

    // УНИВЕРСАЛЬНЫЙ МЕТОД СОРТИРОВКИ И ИСПРАВЛЕНИЯ
    private void SortAndFixList<T>(List<Keyframe<T>> list)
    {
        if (list == null || list.Count <= 1) return;

        // Шаг 1: Сортировка по времени
        list.Sort((a, b) => a.Time.CompareTo(b.Time));

        // Шаг 2: Устранение полных совпадений времени (Overlap fix)
        // Минимальный шаг между кадрами (например, 0.001 сек)
        float epsilon = 0.001f; 

        for (int i = 1; i < list.Count; i++)
        {
            var prev = list[i - 1];
            var curr = list[i];

            // Если текущий ключ встал раньше или ровно на место предыдущего
            // (такое возможно, если мы перетащили группу ключей назад)
            if (curr.Time <= prev.Time)
            {
                // Сдвигаем текущий ключ чуть вперед
                curr.Time = prev.Time + epsilon;
                
                // Важный момент: если мы изменили время в данных, нужно обновить и UI,
                // если бы мы не перезагружали панель целиком. 
                // Но так как мы делаем LoadKeyframesToPanel, это учтется.
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey ek || !ek.Pressed) return;

        bool ctrl = Input.IsKeyPressed(Key.Ctrl);

        // Delete / Backspace — без проверки зоны (как было)
        if (ek.Keycode == Key.Delete || ek.Keycode == Key.Backspace)
        {
            if (_selectedItems.Count > 0)
            {
                DeleteSelected();
                GetViewport().SetInputAsHandled();
            }
            return;
        }

        // Ctrl+C / Ctrl+X / Ctrl+V — только если панель активна
        if (!ctrl || !IsKeyframePanelActive()) return;

        if (ek.Keycode == Key.C)
        {
            CopySelected();
            GetViewport().SetInputAsHandled();
        }
        else if (ek.Keycode == Key.X)
        {
            CopySelected();
            DeleteSelected(); // вырезка = копия + немедленное удаление
            GetViewport().SetInputAsHandled();
        }
        else if (ek.Keycode == Key.V)
        {
            PasteFromClipboard();
            GetViewport().SetInputAsHandled();
        }
    }

    private bool IsKeyframePanelActive()
    {
        return keyframesPanel != null &&
            keyframesPanel.GetGlobalRect().HasPoint(GetViewport().GetMousePosition());
    }

    // ───── Копирование ───────────────────────────────────────────────────
    private void CopySelected()
    {
        if (_selectedItems.Count == 0) return;

        _clipboard.Clear();

        foreach (var point in _selectedItems)
        {
            var data = point.KeyframeData;
            var entry = new ClipboardEntry
            {
                Type        = data.kType,
                OriginalTime = data.Time,
                EasingType  = data.EasingType,
            };

            if (data is Keyframe<float> floatKf)
                entry.FloatValue = floatKf.Value;
            else if (data is Keyframe<ObjectColor> colorKf)
                entry.ColorValue = colorKf.Value?.Clone(); // глубокая копия

            _clipboard.Add(entry);
        }

        GD.Print($"[Clipboard] Скопировано {_clipboard.Count} кейфреймов.");
    }

    // ───── Вставка ───────────────────────────────────────────────────────
    private void PasteFromClipboard()
    {
        if (_clipboard.Count == 0) return;

        var currentObj = keyframesPanel.GetCurrentObject();
        if (currentObj == null) return;

        // Время маркера в локальном времени объекта (то, что показывает слайдер)
        float pasteTime = (float)keyframesPanel.slider.Value;

        // Находим самый ранний ключ в буфере — он встанет ровно на pasteTime
        float minTime = float.MaxValue;
        foreach (var entry in _clipboard)
            if (entry.OriginalTime < minTime) minTime = entry.OriginalTime;

        // Создаём новые кейфреймы со смещёнными временами
        var pastedData = new List<IKeyframe>();

        foreach (var entry in _clipboard)
        {
            float newTime = pasteTime + (entry.OriginalTime - minTime);

            switch (entry.Type)
            {
                case KeyframeType.PositionX:
                {
                    var kf = new Keyframe<float>
                        { kType = entry.Type, Time = newTime,
                        Value = entry.FloatValue, EasingType = entry.EasingType };
                    currentObj.keyframePositionX.Add(kf);
                    pastedData.Add(kf);
                    break;
                }
                case KeyframeType.PositionY:
                {
                    var kf = new Keyframe<float>
                        { kType = entry.Type, Time = newTime,
                        Value = entry.FloatValue, EasingType = entry.EasingType };
                    currentObj.keyframePositionY.Add(kf);
                    pastedData.Add(kf);
                    break;
                }
                case KeyframeType.ScaleX:
                {
                    var kf = new Keyframe<float>
                        { kType = entry.Type, Time = newTime,
                        Value = entry.FloatValue, EasingType = entry.EasingType };
                    currentObj.keyframeScaleX.Add(kf);
                    pastedData.Add(kf);
                    break;
                }
                case KeyframeType.ScaleY:
                {
                    var kf = new Keyframe<float>
                        { kType = entry.Type, Time = newTime,
                        Value = entry.FloatValue, EasingType = entry.EasingType };
                    currentObj.keyframeScaleY.Add(kf);
                    pastedData.Add(kf);
                    break;
                }
                case KeyframeType.Rotation:
                {
                    var kf = new Keyframe<float>
                        { kType = entry.Type, Time = newTime,
                        Value = entry.FloatValue, EasingType = entry.EasingType };
                    currentObj.keyframeRotation.Add(kf);
                    pastedData.Add(kf);
                    break;
                }
                case KeyframeType.Color:
                {
                    var kf = new Keyframe<ObjectColor>
                        { kType = entry.Type, Time = newTime,
                        Value = entry.ColorValue?.Clone(), EasingType = entry.EasingType };
                    currentObj.keyframeColor.Add(kf);
                    pastedData.Add(kf);
                    break;
                }
            }
        }

        // Сортируем все списки после добавления
        SortAndFixList(currentObj.keyframePositionX);
        SortAndFixList(currentObj.keyframePositionY);
        SortAndFixList(currentObj.keyframeScaleX);
        SortAndFixList(currentObj.keyframeScaleY);
        SortAndFixList(currentObj.keyframeRotation);
        SortAndFixList(currentObj.keyframeColor);

        currentObj.RecalculateEndTime();

        // Перезагружаем UI
        DeselectAll();
        keyframesPanel.LoadKeyframesToPanel(currentObj);

        // Выделяем вставленные кейфреймы
        var pastedSet = new HashSet<IKeyframe>(pastedData);
        foreach (var point in keyframesPanel.GetAllKeyframePoints())
        {
            if (pastedSet.Contains(point.KeyframeData))
                Select(point);
        }

        // Обновляем инспектор первым из вставленных
        if (_selectedItems.Count > 0)
            keyframesPanel.UpdateKeyframeData(_selectedItems[0].KeyframeData);

        GD.Print($"[Clipboard] Вставлено {pastedData.Count} кейфреймов на время {pasteTime:F3}.");
    }

	public void DeleteSelected()
    {
        var currentObj = keyframesPanel.GetCurrentObject();
        if (currentObj == null || _selectedItems.Count == 0) return;

        var itemsToDelete = _selectedItems.ToArray();

        foreach (var item in itemsToDelete)
        {
            if (!GodotObject.IsInstanceValid(item)) continue;

            var data = item.KeyframeData;

            switch (data.kType)
            {
                case KeyframeType.PositionX:
                    if (data is Keyframe<float> kfX) currentObj.keyframePositionX.Remove(kfX);
                    break;
                case KeyframeType.PositionY:
                    if (data is Keyframe<float> kfY) currentObj.keyframePositionY.Remove(kfY);
                    break;
                case KeyframeType.ScaleX:
                    if (data is Keyframe<float> kfSX) currentObj.keyframeScaleX.Remove(kfSX);
                    break;
                case KeyframeType.ScaleY:
                    if (data is Keyframe<float> kfSY) currentObj.keyframeScaleY.Remove(kfSY);
                    break;
                case KeyframeType.Rotation:
                    if (data is Keyframe<float> kfR) currentObj.keyframeRotation.Remove(kfR);
                    break;
                case KeyframeType.Color:
                    if (data is Keyframe<ObjectColor> kfC) currentObj.keyframeColor.Remove(kfC);
                    break;
            }

            item.QueueFree();
        }

        _selectedItems.Clear();
        currentObj.RecalculateEndTime();
        currentObj.animCache.IsDirty = true; // ← ДОБАВИТЬ

        keyframesPanel.LoadKeyframesToPanel(currentObj);
    }

    private struct ClipboardEntry
    {
        public KeyframeType Type;
        public float OriginalTime; // время в момент копирования — для расчёта смещений
        public float FloatValue;
        public ObjectColor ColorValue; // глубокая копия для Color-кейфреймов
        public EasingType EasingType;
    }

    private readonly List<ClipboardEntry> _clipboard = new();
}



