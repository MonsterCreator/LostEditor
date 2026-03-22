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
    public Action OnAllDeselected; // ← новое: для закрытия панели из контроллера

    private TriggerBlock _lastSelectedTriggerBlock;
    public TriggerBlock LastSelectedTriggerBlock
    {
        get => _lastSelectedTriggerBlock;
        private set
        {
            _lastSelectedTriggerBlock = value;
            OnNewBlockSelected?.Invoke(_lastSelectedTriggerBlock);
        }
    }

    // Drag state
    private bool _isDragging = false;
    private bool _hasMoved = false;
    private bool _wasCtrlOnDragStart = false; // ← захватываем Ctrl в момент нажатия
    private TriggerBlock _lastClickedBlock = null;
    private float _dragStartX = 0f;
    private Dictionary<Trigger, float> _initialDragTimes = new();

    public override void _Ready()
    {
        if (TimelineTriggerPanel != null)
            TimelineTriggerPanel.GuiInput += OnPanelGuiInput;
    }

    public override void _Input(InputEvent @event)
    {
        // Удаление
        if (@event is InputEventKey ek && ek.Pressed && ek.Keycode == Key.Delete)
        {
            if (SelectedBlocks.Count > 0 &&
                TimelineTriggerPanel.GetGlobalRect().HasPoint(GetViewport().GetMousePosition()))
            {
                DeleteSelectedBlocks();
                GetViewport().SetInputAsHandled();
            }
        }

        if (!_isDragging) return;

        if (@event is InputEventMouseMotion mm)
        {
            _hasMoved = true;
            if (Input.IsKeyPressed(Key.Shift))
                ProcessVerticalMove(mm.Relative.Y);
            else
                ProcessHorizontalMove();
            GetViewport().SetInputAsHandled();
        }

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
        {
            FinalizeDrag();
        }
    }

    // Клик по фону панели (пустое место) — сбрасываем выделение
    private void OnPanelGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            if (!_isDragging && !Input.IsKeyPressed(Key.Ctrl) && !Input.IsKeyPressed(Key.Shift))
            {
                DeselectAll();
            }
        }
    }

    public void SubscribeToBlock(TriggerBlock block)
    {
        block.OnInputEvent += (b, @event) =>
        {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                StartDragging(b);
                GetViewport().SetInputAsHandled();
            }
        };
    }

    public void UnsubscribeFromBlock(TriggerBlock block)
    {
        if (SelectedBlocks.Contains(block)) Deselect(block);
    }

    // Точка входа — аналог StartDraggingBlock у объектов
    private void StartDragging(TriggerBlock block)
    {
        _isDragging = true;
        _hasMoved = false;
        _lastClickedBlock = block;
        _dragStartX = GetViewport().GetMousePosition().X;
        _mouseYAccumulator = 0f;
        _wasCtrlOnDragStart = Input.IsKeyPressed(Key.Ctrl) || Input.IsKeyPressed(Key.Shift);

        if (_wasCtrlOnDragStart)
        {
            // Ctrl/Shift: переключаем блок в мультивыделении
            if (SelectedBlocks.Contains(block))
                Deselect(block);
            else
                Select(block, append: true);
        }
        else if (!SelectedBlocks.Contains(block))
        {
            // Клик по невыделенному без модификатора — выделяем только его
            Select(block, append: false);
        }
        // Если блок уже выделен без Ctrl — не трогаем, чтобы тащить всю группу

        _initialDragTimes.Clear();
        foreach (var sb in SelectedBlocks)
        {
            var data = sb.GetTriggerData();
            if (data != null) _initialDragTimes[data] = data.startTime;
        }
    }

    // Аналог FinalizeDrag у объектов
    private void FinalizeDrag()
    {
        if (!_hasMoved && _lastClickedBlock != null)
        {
            // Простой клик (без движения, без Ctrl) по блоку внутри группы
            // → схлопнуть выделение до одного этого блока
            if (!_wasCtrlOnDragStart
                && SelectedBlocks.Count > 1
                && SelectedBlocks.Contains(_lastClickedBlock))
            {
                Select(_lastClickedBlock, append: false);
            }
        }

        _isDragging = false;
        _hasMoved = false;
        _wasCtrlOnDragStart = false;
        _lastClickedBlock = null;
        _initialDragTimes.Clear();
        _mouseYAccumulator = 0f;
    }

    private void ProcessHorizontalMove()
    {
        float currentMouseX = GetViewport().GetMousePosition().X;
        float pixelDiff = currentMouseX - _dragStartX;
        float pps = timelineController.PixelsPerSecond;
        if (pps <= 0) return;

        float timeDiff = pixelDiff / pps;
        foreach (var kvp in _initialDragTimes)
            kvp.Key.startTime = Mathf.Max(0, kvp.Value + timeDiff);
    }

    private void ProcessVerticalMove(float deltaY)
    {
        _mouseYAccumulator += deltaY;
        while (Mathf.Abs(_mouseYAccumulator) >= RowThreshold)
        {
            int direction = _mouseYAccumulator > 0 ? 1 : -1;
            if (MoveGroup(direction))
                _mouseYAccumulator -= RowThreshold * direction;
            else
            {
                _mouseYAccumulator = 0;
                break;
            }
        }
    }

    private bool MoveGroup(int dir)
    {
        var rows = triggerController.Rows;
        if (rows == null || rows.Length == 0) return false;

        foreach (var block in SelectedBlocks)
        {
            int idx = Array.IndexOf(rows, block.GetParent());
            if (idx + dir < 0 || idx + dir >= rows.Length) return false;
        }
        foreach (var block in SelectedBlocks)
        {
            int idx = Array.IndexOf(rows, block.GetParent());
            block.GetParent().RemoveChild(block);
            rows[idx + dir].AddChild(block);
            block.UpdateBlockVisual();
        }
        return true;
    }

    public void Select(TriggerBlock block, bool append)
    {
        if (!append) DeselectAll();
        if (!SelectedBlocks.Contains(block))
        {
            SelectedBlocks.Add(block);
            block.IsSelected = true;
            LastSelectedTriggerBlock = block; // → OnNewBlockSelected → открывает панель
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

    public void DeselectAll()
    {
        foreach (var block in SelectedBlocks) block.IsSelected = false;
        SelectedBlocks.Clear();
        OnAllDeselected?.Invoke(); // → закрывает панель
    }

    private void DeleteSelectedBlocks()
    {
        var blocksToDelete = new List<TriggerBlock>(SelectedBlocks);
        foreach (var block in blocksToDelete)
            triggerController.DeleteTriggerBlock(block);
        DeselectAll();
    }
}