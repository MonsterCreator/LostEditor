using Godot;
using LostEditor;
using System;
using System.Collections.Generic;

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
                // 1. ЛОГИКА ВЫДЕЛЕНИЯ
                if (!Input.IsKeyPressed(Key.Ctrl) && !Input.IsKeyPressed(Key.Shift) && !_selectedItems.Contains(point))
                {
                    DeselectAll();
                }
                
                Select(point);
                keyframesPanel.UpdateKeyframeData(point.KeyframeData);

                // 2. ИНИЦИАЛИЗАЦИЯ ПЕРЕТАСКИВАНИЯ (ВАЖНО!)
                _isDragging = true;
                // Запоминаем глобальную позицию мыши в момент клика
                _startDragMouseX = point.GetGlobalMousePosition().X;
                
                // Запоминаем изначальное время каждого выделенного кейфрейма
                _startKeyframeTimes.Clear();
                foreach (var item in _selectedItems)
                {
                    _startKeyframeTimes[item] = item.KeyframeData.Time;
                }

                GetViewport().SetInputAsHandled();
            }
            else
            {
                // 3. ЗАВЕРШЕНИЕ ПЕРЕТАСКИВАНИЯ
                if (_isDragging)
                {
                    _isDragging = false;
                    FinalizeDrag(); // Тут будет сортировка
                }
            }
        }
        // ВАЖНО: Мышь может двигаться быстро, используем GetGlobalMousePosition
        else if (@event is InputEventMouseMotion mm && _isDragging)
        {
            // Передаем текущую позицию мыши, а не Relative
            MoveSelected(mm.GlobalPosition.X);
        }
    }

	private void Select(KeyframePoint point)
    {
        if (!_selectedItems.Contains(point))
        {
            _selectedItems.Add(point);
            point.SetSelected(true);
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
	}

	private void MoveSelected(float currentMouseX) {
		float mouseDiffX = currentMouseX - _startDragMouseX;
		float timeOffset = mouseDiffX / _pixelsPerSecond;
        GD.Print($"cmX: {currentMouseX} sdmX: {_startDragMouseX} mdX: {mouseDiffX} toffset: {timeOffset}");

		foreach (var item in _selectedItems) {
			if (_startKeyframeTimes.TryGetValue(item, out float startTime)) {
				float newTime = Math.Max(0, startTime + timeOffset);

				
				// Обновляем данные
				if (item.KeyframeData is Keyframe<float> kf) kf.Time = newTime;
				
				// Обновляем визуал (точное соответствие PPS)
				item.Position = new Vector2(newTime * _pixelsPerSecond, item.Position.Y);
			}
		}
	}

	private void FinalizeDrag()
    {
        // Здесь мы должны вызвать сортировку в GameObject, когда мышь отпущена.
        // Поскольку KeyframePoint ссылается на данные напрямую, данные уже изменены.
        // Нам нужно просто сказать системе: "Эй, пересортируй списки!"
        
        // Пример (псевдокод, так как зависит от твоей архитектуры):
        // var currentObj = SceneManager.SelectedObject;
        // currentObj.SortKeyframes();
        GD.Print("Drag finished. Keyframes should be sorted now.");
    }

	public void DeleteSelected()
    {
        foreach(var item in _selectedItems)
        {
            // Тут нужно удалить данные из списка в GameObject!
            // Это сложность: KeyframePoint должен знать, из какого он списка, 
            // либо вызывать Action<IKeyframe> OnDeleteRequest
            item.QueueFree();
        }
        _selectedItems.Clear();
    }
}
