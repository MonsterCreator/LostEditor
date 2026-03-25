using Godot;
using System;
using System.Collections.Generic;
using LostEditor;

public partial class TimelineObjectController : Node
{
    [Export] public Editor editor;
    [Export] public Control[] Rows;

    [Export] public LevelColorData levelColors;
    [Export] public TimelineController timelineController;
    [Export] public WorkPanel workPanel;
    [Export] public ScrollContainerHorController scrollContainerHor;
    [Export] public SelectionManager selectionManager;
    [Export] public DebugEditorManager debugEditorManager;
    [Export] public Node ViewportObj;

    [ExportGroup("Prefabs")]
    [Export] public PackedScene GameObjectScene;
    [Export] public PackedScene TimelineBlockScene;
    [Export] public ObjectManager objectManager;
    


    
    public bool IsDragging = false;

    public bool hasMoved = false; // Флаг, чтобы отличить клик от драга
    
    // Аккумуляторы для движения
    private float _mouseYAccumulator = 0f;
    private const float RowThreshold = 30f; // Чувствительность смены строк

    private int deselectIndexDebug = 0;

    private TimelineBlock _lastClickedBlock = null;
    private bool _wasCtrlOnDragStart = false;
    public List<TimelineBlock> activeBlocks = new();

    public void CreateObjectButtonPressed() => CreateObject();

    public void CreateObject()
    {
        if (Rows == null || Rows.Length == 0) return;

        var newSceneObj = GameObjectScene.Instantiate<GameObject>();
        LoadDefaultObjectData(newSceneObj);

        // Кэшируем время окончания на основе созданных ключей
        

        ViewportObj.AddChild(newSceneObj);
        objectManager.RegisterObject(newSceneObj);

        var block = TimelineBlockScene.Instantiate<TimelineBlock>();
        block.Setup(newSceneObj, timelineController, this, selectionManager);
        block.Data = newSceneObj;
        block.editor = editor;
        
        Rows[0].AddChild(block);
        activeBlocks.Add(block);

        newSceneObj.RecalculateEndTime();
        block.UpdateVisual();
    }

    public TimelineBlock GetSelectedBlock()
    {
        if(activeBlocks != null && activeBlocks.Count == 1) return activeBlocks[0];
        else return null;
    }

    private void LoadDefaultObjectData(GameObject obj)
    {
        float start = timelineController.timelineTime;
        
        obj.startTime = start;
        obj.endTimeMode = EndTimeMode.FixedTime;
        obj.endTime = 3f;

        obj.keyframePositionX.Add(new Keyframe<float>()
        {
            kType = KeyframeType.PositionX,
            Time = 0f,
            EasingType = EasingType.Linear,
            Value = 0f
        });
        obj.keyframePositionY.Add(new Keyframe<float>()
        {
            kType = KeyframeType.PositionY,
            Time = 0f,
            EasingType = EasingType.Linear,
            Value = 0f
        });

        obj.keyframeScaleX.Add(new Keyframe<float>()
        {
            kType = KeyframeType.ScaleX,
            Time = 0f,
            EasingType = EasingType.Linear,
            Value = 10f
        });

        obj.keyframeScaleY.Add(new Keyframe<float>()
        {
            kType = KeyframeType.ScaleY,
            Time = 0f,
            EasingType = EasingType.Linear,
            Value = 10f
        });

        obj.keyframeRotation.Add(new Keyframe<float>()
        {
            kType = KeyframeType.Rotation,
            Time = 0f,
            EasingType = EasingType.Linear,
            Value = 0f
        });

        LevelColor levelColor = levelColors.Colors[0]; // LevelColor
        var oc = new ObjectColor();
        oc.SetBaseLevelColor(levelColor);
        obj.keyframeColor.Add(new Keyframe<ObjectColor>()
        {
            kType = KeyframeType.Color,
            Time = 0f,
            EasingType = EasingType.Linear,
            Value = oc
        });


        
    }

    public override void _Input(InputEvent @event)
    {
        if (IsDragging)
            HandleDragMotion(@event);

        // ← ЭТОТ БЛОК ПОТЕРЯЛСЯ — он завершает перетаскивание
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
        {
            bool hadMoved = hasMoved;

            if (IsDragging) FinalizeDrag();

            if (!hadMoved && GetBlockUnderMouse() == null && IsMouseOverObjectRows())
            {
                selectionManager.DeselectAll();
                workPanel.OpenPanel(WorkPanelType.NoPanel);
            }
        }

        if (@event is InputEventKey ek && ek.Pressed && IsMouseOverObjectRows())
        {
            bool ctrl = Input.IsKeyPressed(Key.Ctrl);
            if (ctrl)
            {
                if (ek.Keycode == Key.C)
                {
                    CopySelectedObjects();
                    GetViewport().SetInputAsHandled();
                    return;
                }
                else if (ek.Keycode == Key.X)
                {
                    CopySelectedObjects();
                    var toDelete = new List<TimelineBlock>(selectionManager.SelectedBlocks);
                    foreach (var block in toDelete) DeleteBlock(block);
                    GetViewport().SetInputAsHandled();
                    return;
                }
                else if (ek.Keycode == Key.V)
                {
                    PasteObjects();
                    GetViewport().SetInputAsHandled();
                    return;
                }
            }
        }

        HandleDeletion(@event);
    }

    // Метод чисто для обработки движения мыши во время перетаскивания
    private void HandleDragMotion(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            hasMoved = true;
            
            // Логика перемещения
            if (Input.IsKeyPressed(Key.Shift))
            {
                ProcessVerticalMove(motion.Relative.Y);
            }
            else
            {
                ProcessHorizontalMove(motion.Relative.X);
            }
            GetViewport().SetInputAsHandled();
        }
    }

    // Вызывается из TimelineBlock при нажатии кнопки (ButtonDown)
    // Это точка ВХОДА в режим перетаскивания
    public void StartDraggingBlock(TimelineBlock block)
    {
        if (IsDragging) return;

        IsDragging = true;
        hasMoved = false;
        _lastClickedBlock = block;
        _mouseYAccumulator = 0f;
        _wasCtrlOnDragStart = Input.IsKeyPressed(Key.Ctrl);

        if (_wasCtrlOnDragStart)
        {
            selectionManager.HandleSelection(block, true);
        }
        else if (!selectionManager.SelectedBlocks.Contains(block))
        {
            selectionManager.SelectBlock(block, clearFirst: true);
        }
    }

    private void ProcessVerticalMove(float deltaY)
    {
        _mouseYAccumulator += deltaY;

        // Пока накопленное смещение больше порога, двигаем блоки
        // Используем while, чтобы при резком движении можно было перескочить сразу 2-3 строки
        while (Mathf.Abs(_mouseYAccumulator) > RowThreshold)
        {
            int direction = _mouseYAccumulator > 0 ? 1 : -1;
            
            // Пытаемся сдвинуть
            bool moved = MoveGroup(direction);
            
            // Уменьшаем аккумулятор на величину шага
            // ВАЖНО: не обнуляем, а вычитаем, чтобы сохранить плавность
            _mouseYAccumulator -= RowThreshold * direction;

            if (moved)
            {
                UpdateAllBlocks();
            }
            else
            {
                // Если двигаться некуда (уперлись в край), сбрасываем остаток, 
                // чтобы не копить бесконечное давление в стену
                _mouseYAccumulator = 0; 
                break;
            }
        }
    }

    private void ProcessHorizontalMove(float deltaX)
    {
        float pps = timelineController.PixelsPerSecond;
        if (pps <= 0) return; 
        float timeDelta = deltaX / pps;

        // Проверка границ
        foreach (var b in selectionManager.SelectedBlocks)
        {
            float newStart = b.Data.startTime + timeDelta;
            float duration = b.Data.cachedEndTime - b.Data.startTime;
            float newEnd = newStart + duration;

            if (newStart < 0) return;
            // Если это GlobalTime, объект не может начаться позже своего глобального конца
            if (b.Data.endTimeMode == EndTimeMode.GlobalTime && newStart >= b.Data.endTime) return;
        }
        
        foreach (var b in selectionManager.SelectedBlocks)
        {
            b.Data.startTime += timeDelta;

            // В FixedTime двигаем конец вместе со стартом
            /*
            if (b.Data.endTimeMode == EndTimeMode.FixedTime)
            {
                b.Data.endTime += timeDelta;
            }
            */
            // В GlobalTime НЕ трогаем b.Data.endTime, чтобы он остался на 5.0с

            b.Data.RecalculateEndTime();
            b.UpdateVisual();
        }
    }

    private void FinalizeDrag()
    {
        if (!hasMoved && _lastClickedBlock != null)
        {
            // Простой клик (без движения) по блоку внутри группы без Ctrl →
            // схлопнуть мультивыделение до одного этого блока
            if (!_wasCtrlOnDragStart // ← используем сохранённое, не читаем Ctrl снова
                && selectionManager.SelectedBlocks.Count > 1
                && selectionManager.SelectedBlocks.Contains(_lastClickedBlock))
            {
                selectionManager.SelectBlock(_lastClickedBlock, clearFirst: true);
            }
        }

        workPanel.OpenPanel(WorkPanelType.ObjectEdit);

        IsDragging = false;
        hasMoved = false;
        _wasCtrlOnDragStart = false;
        _lastClickedBlock = null;
        _mouseYAccumulator = 0f;
        UpdateAllBlocks();
    }

    private bool MoveGroup(int dir)
    {
        // 1. Проверяем возможность движения для ВСЕЙ группы
        foreach (var block in selectionManager.SelectedBlocks)
        {
            Node currentRow = block.GetParent();
            int currentIndex = Array.IndexOf(Rows, currentRow);
            int targetIndex = currentIndex + dir;
            
            // Если хоть один блок упирается в границы - отменяем движение всей группы
            if (targetIndex < 0 || targetIndex >= Rows.Length) return false;
        }

        // 2. Двигаем
        foreach (var block in selectionManager.SelectedBlocks)
        {
            Node currentRow = block.GetParent();
            int currentIndex = Array.IndexOf(Rows, currentRow);
            
            currentRow.RemoveChild(block);
            Rows[currentIndex + dir].AddChild(block);
            
            // Хак для Godot при смене родителя
            block.UpdateVisual();
        }
        return true;
    }

    // Метод обработки удаления (без изменений)
    private bool HandleDeletion(InputEvent @event)
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        if (@event is InputEventKey keyEvent && keyEvent.Pressed
            && keyEvent.Keycode == Key.Delete
            && IsMouseOverObjectRows())
        {
            if (selectionManager.SelectedBlocks.Count > 0)
            {
                var toDelete = new List<TimelineBlock>(selectionManager.SelectedBlocks);
                foreach (var block in toDelete) DeleteBlock(block);
                GetViewport().SetInputAsHandled();
                return true;
            }
        }
        return false;
    }

    // ───── Копирование ────────────────────────────────────────────────────────
    private void CopySelectedObjects()
    {
        if (selectionManager.SelectedBlocks.Count == 0) return;

        _objectClipboard.Clear();

        foreach (var block in selectionManager.SelectedBlocks)
        {
            var obj = block.Data;
            var entry = new ObjectClipboardEntry
            {
                Name              = obj.name,
                OriginalStartTime = obj.startTime,
                EndTime           = obj.endTime,
                EndTimeOffset     = obj.endTimeOffset,
                EndTimeMode       = obj.endTimeMode,
                ObjColor          = obj.objectColor?.Clone(),
            };

            // Глубокое копирование float-кейфреймов
            static Keyframe<float> CloneFloat(Keyframe<float> kf) =>
                new Keyframe<float> { kType = kf.kType, Time = kf.Time, Value = kf.Value, EasingType = kf.EasingType };

            foreach (var kf in obj.keyframePositionX) entry.KeyframePositionX.Add(CloneFloat(kf));
            foreach (var kf in obj.keyframePositionY) entry.KeyframePositionY.Add(CloneFloat(kf));
            foreach (var kf in obj.keyframeScaleX)    entry.KeyframeScaleX.Add(CloneFloat(kf));
            foreach (var kf in obj.keyframeScaleY)    entry.KeyframeScaleY.Add(CloneFloat(kf));
            foreach (var kf in obj.keyframeRotation)  entry.KeyframeRotation.Add(CloneFloat(kf));

            foreach (var kf in obj.keyframeColor)
                entry.KeyframeColor.Add(new Keyframe<ObjectColor>
                {
                    kType      = kf.kType,
                    Time       = kf.Time,
                    Value      = kf.Value?.Clone(), // глубокая копия ObjectColor
                    EasingType = kf.EasingType
                });

            _objectClipboard.Add(entry);
        }

        GD.Print($"[ObjectClipboard] Скопировано {_objectClipboard.Count} объектов.");
    }

    // ───── Вставка ────────────────────────────────────────────────────────────
    private void PasteObjects()
    {
        if (_objectClipboard.Count == 0) return;

        // Самый ранний startTime в буфере — он встаёт точно на курсор таймлайна
        float minTime = float.MaxValue;
        foreach (var entry in _objectClipboard)
            if (entry.OriginalStartTime < minTime) minTime = entry.OriginalStartTime;

        float pasteTime = timelineController.timelineTime;

        var pastedBlocks = new List<TimelineBlock>();

        foreach (var entry in _objectClipboard)
        {
            float newStartTime = pasteTime + (entry.OriginalStartTime - minTime);

            // 1. Создаём новый GameObject
            var newObj = GameObjectScene.Instantiate<GameObject>();

            // Сначала заполняем кейфреймы — до установки startTime,
            // чтобы RecalculateEndTime работала правильно
            static Keyframe<float> CloneFloat(Keyframe<float> kf) =>
                new Keyframe<float> { kType = kf.kType, Time = kf.Time, Value = kf.Value, EasingType = kf.EasingType };

            foreach (var kf in entry.KeyframePositionX) newObj.keyframePositionX.Add(CloneFloat(kf));
            foreach (var kf in entry.KeyframePositionY) newObj.keyframePositionY.Add(CloneFloat(kf));
            foreach (var kf in entry.KeyframeScaleX)    newObj.keyframeScaleX.Add(CloneFloat(kf));
            foreach (var kf in entry.KeyframeScaleY)    newObj.keyframeScaleY.Add(CloneFloat(kf));
            foreach (var kf in entry.KeyframeRotation)  newObj.keyframeRotation.Add(CloneFloat(kf));

            foreach (var kf in entry.KeyframeColor)
                newObj.keyframeColor.Add(new Keyframe<ObjectColor>
                {
                    kType      = kf.kType,
                    Time       = kf.Time,
                    Value      = kf.Value?.Clone(),
                    EasingType = kf.EasingType
                });

            // 2. Устанавливаем базовые свойства
            newObj.name         = entry.Name + " (copy)";
            newObj.startTime    = newStartTime;
            newObj.endTimeMode  = entry.EndTimeMode;
            newObj.endTime      = entry.EndTime;
            newObj.endTimeOffset = entry.EndTimeOffset;
            if (entry.ObjColor != null) newObj.objectColor = entry.ObjColor.Clone();

            newObj.RecalculateEndTime();

            // 3. Добавляем в сцену и регистрируем
            ViewportObj.AddChild(newObj);
            objectManager.RegisterObject(newObj);

            // 4. Создаём визуальный блок
            var block = TimelineBlockScene.Instantiate<TimelineBlock>();
            block.Setup(newObj, timelineController, this, selectionManager);
            block.Data   = newObj;
            block.editor = editor;

            Rows[0].AddChild(block);
            activeBlocks.Add(block);
            block.UpdateVisual();

            pastedBlocks.Add(block);
        }

        // 5. Выделяем вставленные блоки (сначала сброс, потом мультивыделение)
        selectionManager.DeselectAll();
        foreach (var block in pastedBlocks)
            selectionManager.HandleSelection(block, true);

        GD.Print($"[ObjectClipboard] Вставлено {pastedBlocks.Count} объектов на время {pasteTime:F3}.");
    }

    public TimelineBlock GetBlockUnderMouse()
    {
        Vector2 mouseGlobalPos = GetViewport().GetMousePosition();
            foreach (var row in Rows)
            {
                foreach (Node child in row.GetChildren())
                {
                    if (child is TimelineBlock block)
                    {
                        Rect2 blockRect = new Rect2(block.GlobalPosition, block.Size);
                        if (blockRect.HasPoint(mouseGlobalPos))
                        {
                            return block;
                        }
                    }
                }
            }
        return null;
    } 

    private bool IsMouseOverObjectRows()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        foreach (var row in Rows)
        {
            // IsVisibleInTree() = false когда вкладка объектов неактивна,
            // даже если GetGlobalRect() геометрически совпадает с триггер-строками
            if (row.IsVisibleInTree() && row.GetGlobalRect().HasPoint(mousePos))
                return true;
        }
        return false;
    }
    
    // Остальные вспомогательные методы (HandleBlockSelection, DeleteBlock и т.д.)
    // ... (можно оставить как были в твоем коде) ...
    public void DeleteBlock(TimelineBlock block)
    {
        activeBlocks.Remove(block);
        selectionManager.SelectedBlocks.Remove(block);
        objectManager.objects.Remove(block.Data);
        block.Data.QueueFree();
        block.QueueFree();
    }
    public void UpdateAllBlocks() => activeBlocks.ForEach(b => b.UpdateVisual());
    public void HandleBlockSelection(TimelineBlock block, bool isCtrl) => selectionManager.HandleSelection(block, isCtrl);

    private class ObjectClipboardEntry
    {
        public string Name;
        public float OriginalStartTime; // для расчёта смещения при вставке
        public float EndTime;
        public float EndTimeOffset;
        public EndTimeMode EndTimeMode;
        public ObjectColor ObjColor;

        // Глубокие копии всех кейфрейм-списков
        public List<Keyframe<float>> KeyframePositionX = new();
        public List<Keyframe<float>> KeyframePositionY = new();
        public List<Keyframe<float>> KeyframeScaleX    = new();
        public List<Keyframe<float>> KeyframeScaleY    = new();
        public List<Keyframe<float>> KeyframeRotation  = new();
        public List<Keyframe<ObjectColor>> KeyframeColor = new();
    }

    private readonly List<ObjectClipboardEntry> _objectClipboard = new();
}

