using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LostEditor;

public partial class TimelineTriggerSelectionManager : Node
{
    [Export] public TimelineTriggerController triggerController;
    [Export] public TimelineController timelineController;
    [Export] public Control TimelineTriggerPanel;

    private float _mouseYAccumulator = 0f;
    private const float RowThreshold = 30f; 

    public List<TriggerBlock> SelectedBlocks { get; private set; } = new();

	public Action<TriggerBlock> OnNewBlockSelected;
	private TriggerBlock _lastSelectedTriggerBlock;
	public TriggerBlock LastSelectedTriggerBlock
	{
		get => _lastSelectedTriggerBlock;
		set
		{
			_lastSelectedTriggerBlock = value;
			OnNewBlockSelected?.Invoke(_lastSelectedTriggerBlock);
		}
	}

    private bool _isDragging = false;
    private bool _hasMoved = false;
    private float _dragStartX = 0f;
    private Dictionary<Trigger, float> _initialDragTimes = new Dictionary<Trigger, float>();

    public override void _Ready()
    {
        if (TimelineTriggerPanel != null)
            TimelineTriggerPanel.GuiInput += OnPanelGuiInput;
    }

    // ВАЖНО: Обработка движения перенесена в глобальный _Input
    public override void _Input(InputEvent @event)
    {
        // Обработка удаления
        if (@event is InputEventKey ek && ek.Pressed && ek.Keycode == Key.Delete)
        {
            // Проверяем, есть ли выделенные блоки и находится ли мышь над панелью
            if (SelectedBlocks.Count > 0 && TimelineTriggerPanel.GetGlobalRect().HasPoint(GetViewport().GetMousePosition()))
            {
                DeleteSelectedBlocks();
                GetViewport().SetInputAsHandled(); // Поглощаем ввод
            }
        }

        if (!_isDragging) return;

        if (@event is InputEventMouseMotion mm)
        {
            _hasMoved = true;
            
            if (Input.IsKeyPressed(Key.Shift))
            {
                // Вертикальное движение (используем относительное смещение кадра)
                ProcessVerticalMove(mm.Relative.Y);
            }
            else
            {
                // Горизонтальное движение
                ProcessHorizontalMove();
            }
            
            // Помечаем событие как обработанное, чтобы не дергать другие элементы
            GetViewport().SetInputAsHandled();
        }

    
        if (!_isDragging) return;
        
        // Завершение перетаскивания при отпускании кнопки в любом месте экрана
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
        {
            OnGlobalReleased();
        }
    }

    private void OnPanelGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            if (!Input.IsKeyPressed(Key.Ctrl) && !Input.IsKeyPressed(Key.Shift))
            {
                DeselectAll();
            }
        }
    }

    // Теперь блоки только инициируют начало драга
    public void SubscribeToBlock(TriggerBlock block)
    {
        block.OnInputEvent += (b, @event) => 
        {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                OnBlockClicked(b);
                // Захватываем ввод, чтобы панель под блоком не получила клик
                GetViewport().SetInputAsHandled();
            }
        };
    }

    public void UnsubscribeFromBlock(TriggerBlock block)
    {
        if (SelectedBlocks.Contains(block)) Deselect(block);
    }

    private void OnBlockClicked(TriggerBlock block)
    {
        _isDragging = true;
        _hasMoved = false;
        _dragStartX = GetViewport().GetMousePosition().X;
        _mouseYAccumulator = 0f;

        bool isModifier = Input.IsKeyPressed(Key.Ctrl) || Input.IsKeyPressed(Key.Shift);

        if (!isModifier && !SelectedBlocks.Contains(block))
        {
            DeselectAll();
        }

        if (!SelectedBlocks.Contains(block))
        {
            Select(block, isModifier);
        }

        _initialDragTimes.Clear();
        foreach (var sb in SelectedBlocks)
        {
            var data = sb.GetTriggerData();
            if (data != null) _initialDragTimes[data] = data.startTime;
        }
    }

	private void ProcessHorizontalMove()
	{
		float currentMouseX = GetViewport().GetMousePosition().X;
		float pixelDiff = currentMouseX - _dragStartX;
		float pps = timelineController.PixelsPerSecond;
		if (pps <= 0) return;
		
		float timeDiff = pixelDiff / pps;

		foreach (var kvp in _initialDragTimes)
		{
			Trigger trigger = kvp.Key;

			trigger.startTime = Mathf.Max(0, kvp.Value + timeDiff);
		}
	}

    private void ProcessVerticalMove(float deltaY)
    {
        _mouseYAccumulator += deltaY;

        // Пока накоплено больше порога — двигаем блоки по строкам
        while (Mathf.Abs(_mouseYAccumulator) >= RowThreshold)
        {
            int direction = _mouseYAccumulator > 0 ? 1 : -1;
            
            if (MoveGroup(direction))
            {
                // Вычитаем ровно один шаг из аккумулятора
                _mouseYAccumulator -= RowThreshold * direction;
            }
            else
            {
                // Если уперлись в край, сбрасываем остаток, чтобы не "давить" в стенку
                _mouseYAccumulator = 0;
                break;
            }
        }
    }

    private bool MoveGroup(int dir)
    {
        var rows = triggerController.Rows;
        if (rows == null || rows.Length == 0) return false;

        // 1. Валидация всей группы
        foreach (var block in SelectedBlocks)
        {
            int currentIndex = Array.IndexOf(rows, block.GetParent());
            int targetIndex = currentIndex + dir;
            if (targetIndex < 0 || targetIndex >= rows.Length) return false;
        }

        // 2. Перемещение (теперь выполняется только если валидация прошла для всех)
        foreach (var block in SelectedBlocks)
        {
            int currentIndex = Array.IndexOf(rows, block.GetParent());
            block.GetParent().RemoveChild(block);
            rows[currentIndex + dir].AddChild(block);
            block.UpdateBlockVisual();
        }
        return true;
    }

    private void OnGlobalReleased()
    {
        _isDragging = false;
        _initialDragTimes.Clear();
        _mouseYAccumulator = 0f;
        GD.Print("Drag завершен");
    }

    public void Select(TriggerBlock block, bool append)
    {
        if (!append) DeselectAll();
        if (!SelectedBlocks.Contains(block))
        {
			LastSelectedTriggerBlock = block;
            SelectedBlocks.Add(block);
            block.IsSelected = true;
        }
    }

    public void Deselect(TriggerBlock block)
    {
        if (SelectedBlocks.Contains(block))
        {
            block.IsSelected = false;
            SelectedBlocks.Remove(block);
        }
    }

    private void DeleteSelectedBlocks()
    {
        // Клонируем список, так как оригинальный будет изменяться при удалении
        var blocksToDelete = new List<TriggerBlock>(SelectedBlocks);
        
        foreach (var block in blocksToDelete)
        {
            // Вызываем удаление в основном контроллере
            triggerController.DeleteTriggerBlock(block);
        }
        
        // Очищаем список выделения (на всякий случай, хотя DeleteTriggerBlock должен это делать)
        DeselectAll();
        GD.Print($"Удалено блоков: {blocksToDelete.Count}");
    }

    public void DeselectAll()
    {
        foreach (var block in SelectedBlocks) block.IsSelected = false;
        SelectedBlocks.Clear();
    }
}