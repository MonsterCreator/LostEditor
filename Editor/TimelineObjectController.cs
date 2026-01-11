using Godot;
using System;
using System.Collections.Generic;
using LostEditor;

public partial class TimelineObjectController : Node
{
    [Export] public Editor editor;
    [Export] public Control[] Rows;


    
    public bool IsDragging = false;

    public bool hasMoved = false; // Флаг, чтобы отличить клик от драга
    
    // Аккумуляторы для движения
    private float _mouseYAccumulator = 0f;
    private const float RowThreshold = 30f; // Чувствительность смены строк

    private int deselectIndexDebug = 0;

    private TimelineBlock _lastClickedBlock = null;
    public List<TimelineBlock> activeBlocks = new();

    public void CreateObjectButtonPressed() => CreateObject();

    public void CreateObject()
    {
        if (Rows == null || Rows.Length == 0) return;

        var newSceneObj = editor.GameObjectScene.Instantiate<GameObject>();
        float start = editor.timelineController.timelineTime;
        float duration = 5.0f;
        newSceneObj.startTime = start;
        newSceneObj.endTime = start + duration;

        editor.ViewportObj.AddChild(newSceneObj);
        editor.objectManager.RegisterObject(newSceneObj);

        var block = editor.TimelineBlockScene.Instantiate<TimelineBlock>();
        block.Data = newSceneObj;
        block.editor = editor;
        
        Rows[0].AddChild(block);
        activeBlocks.Add(block);

        block.UpdateVisual(editor.timelineController.PixelsPerSecond);
    }

    public override void _Input(InputEvent @event)
    {
        // 1. Если мы уже тащим - нам не нужно искать блоки под мышью.
        // Мы просто обрабатываем движение.
        if (IsDragging)
        {
            HandleDragMotion(@event);
        }

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
        {
            if(editor.timeLineObjectControl.GetBlockUnderMouse() == null && hasMoved) editor.selection.DeselectAll();
            
            if (IsDragging) FinalizeDrag();
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
        IsDragging = true;
        editor.debugEditorManager.OverrideText(1,"StartDraggingBlock, IsDragging: True (TimelineBlockController,FinalizeDrag)");
        hasMoved = false;
        _lastClickedBlock = block;
        _mouseYAccumulator = 0f;
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
        float pps = editor.timelineController.PixelsPerSecond;
        if (pps <= 0) return; 

        float timeDelta = deltaX / pps;

        // Проверка границ: если хоть один блок вылетит за пределы, отменяем всё движение
        foreach (var b in editor.selection.SelectedBlocks)
        {
            float newStart = b.Data.startTime + timeDelta;
            float newEnd = b.Data.endTime + timeDelta;
            if (newStart < 0 || newEnd > editor.timelineController.timelineMaxTime) return;
        }
        
        // Применяем
        editor.selection.SelectedBlocks.ForEach(b => {
            b.Data.startTime += timeDelta;
            b.Data.endTime += timeDelta;
            b.UpdateVisual(pps);
        });
    }

    private void FinalizeDrag()
    {
        // Если мышь не двигалась, это был просто клик (выделение)
        if (!hasMoved && _lastClickedBlock != null)
        {
            editor.debugEditorManager.OverrideText(4,"FinalizeDrag Мышь была просто нажата без перемещения.");
            bool isCtrl = Input.IsKeyPressed(Key.Ctrl);
            
            // Если Ctrl не зажат, и мы кликнули по уже выделенному блоку (без движения),
            // то по логике UI это должно сбросить остальные выделения и оставить только этот
            if (!isCtrl && editor.selection.SelectedBlocks.Count > 1 && editor.selection.SelectedBlocks.Contains(_lastClickedBlock))
            {
                editor.selection.DeselectAll();
                editor.selection.HandleSelection(_lastClickedBlock, false);
            }
            // Стандартная обработка (Ctrl или клик по невыделенному) уже произошла в StartDraggingBlock,
            // но можно добавить специфичную логику здесь.
        }

        IsDragging = false;
        editor.debugEditorManager.OverrideText(1,"StartDraggingBlock, IsDragging: Flase (TimelineBlockController,FinalizeDrag)");
        _lastClickedBlock = null;
        _mouseYAccumulator = 0f;
        UpdateAllBlocks(); // Финальное обновление для выравнивания
    }

    private bool MoveGroup(int dir)
    {
        // 1. Проверяем возможность движения для ВСЕЙ группы
        foreach (var block in editor.selection.SelectedBlocks)
        {
            Node currentRow = block.GetParent();
            int currentIndex = Array.IndexOf(Rows, currentRow);
            int targetIndex = currentIndex + dir;
            
            // Если хоть один блок упирается в границы - отменяем движение всей группы
            if (targetIndex < 0 || targetIndex >= Rows.Length) return false;
        }

        // 2. Двигаем
        foreach (var block in editor.selection.SelectedBlocks)
        {
            Node currentRow = block.GetParent();
            int currentIndex = Array.IndexOf(Rows, currentRow);
            
            currentRow.RemoveChild(block);
            Rows[currentIndex + dir].AddChild(block);
            
            // Хак для Godot при смене родителя
            block.UpdateVisual(editor.timelineController.PixelsPerSecond);
        }
        return true;
    }

    // Метод обработки удаления (без изменений)
    private bool HandleDeletion(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Delete)
        {
            if (editor.selection.SelectedBlocks.Count > 0)
            {
                var toDelete = new List<TimelineBlock>(editor.selection.SelectedBlocks);
                foreach (var block in toDelete) DeleteBlock(block);
                GetViewport().SetInputAsHandled();
                return true;
            }
        }
        return false;
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
    
    // Остальные вспомогательные методы (HandleBlockSelection, DeleteBlock и т.д.)
    // ... (можно оставить как были в твоем коде) ...
    public void DeleteBlock(TimelineBlock block)
    {
        activeBlocks.Remove(block);
        editor.selection.SelectedBlocks.Remove(block);
        editor.objectManager.objects.Remove(block.Data);
        block.Data.QueueFree();
        block.QueueFree();
    }
    public void UpdateAllBlocks() => activeBlocks.ForEach(b => b.UpdateVisual(editor.timelineController.PixelsPerSecond));
    public void HandleBlockSelection(TimelineBlock block, bool isCtrl) => editor.selection.HandleSelection(block, isCtrl);
}