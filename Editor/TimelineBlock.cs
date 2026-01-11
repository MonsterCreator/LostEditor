using Godot;
using LostEditor; // Чтобы видеть GameObject

public partial class TimelineBlock : ColorRect
{
    [Export] public Button button;
    public GameObject Data;
    public bool IsSelected = false;
    
    // Для перемещения

    private Vector2 _dragStartPos;
    private float _startTimeAtDrag;
    private int _startRowIndex;

    // Ссылка на редактор для уведомлений
    public Editor editor;

    public override void _Ready()
    {
        // Включаем фильтр мыши, чтобы блок ловил клики
        MouseFilter = MouseFilterEnum.Stop;
    }

    /*
    public override void _GuiInput(InputEvent @event)
    {
        if (editor == null || Data == null) return;
        
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    // Устанавливаем состояние перетаскивания
                    editor.timeLineObjectControl.IsDragging = true;
                    editor.debugEditorManager.OverrideText(1,"перетаскивание: True (TimelineBlock, _GuiInput)");
                    GetViewport().SetInputAsHandled();
                }
                else if (editor.timeLineObjectControl.IsDragging)
                {
                    // Не сбрасываем состояние здесь, это сделает TimelineObjectController
                    GetViewport().SetInputAsHandled();
                }
            }
        }
    }
    */

    private void ButtonPressed() //signal connected
    {
        if (editor == null || Data == null) return;

        if(!editor.timeLineObjectControl.hasMoved)
        {
            if(Input.IsKeyPressed(Key.Ctrl))
            {
                editor.timeLineObjectControl.HandleBlockSelection(this,true);
            }
            else if(editor.timeLineObjectControl.GetBlockUnderMouse() != null)
            {
                //editor.timeLineObjectControl.HandleBlockSelection(this,false);
                editor.selection.SelectBlock(this);
                IsSelected = true;
            }
            else
            {
                editor.selection.DeselectAll();
            }
            GD.Print("BUTTON PRESSED");
        }
        else editor.timeLineObjectControl.hasMoved = false;
        

        UpdateVisual(editor.timelineController.PixelsPerSecond);
        
    }

// В файле TimelineBlock.cs

    private void ButtonDown() // Сигнал button_down
    {
        // Сообщаем контроллеру, что начали тащить ЭТОТ блок
        // Контроллер сам выставит IsDragging = true и обработает выделение
        editor.timeLineObjectControl.StartDraggingBlock(this);
    }

    private void ButtonUp() // Сигнал button_up
    {
        // Сброс происходит глобально в Controller._Input, 
        // но можно продублировать для надежности, если мышь была над кнопкой
        //editor.timeLineObjectControl.IsDragging = false; 
    }

    public void DeselectBlock()
    {
        IsSelected = false;
        UpdateVisual(editor.timelineController.PixelsPerSecond);
    }
    public void UpdateVisual(float pps)
    {
        if (Data == null) return;
        
        float posX = Data.startTime * pps;
        float width = (Data.endTime - Data.startTime) * pps;

        Position = new Vector2(posX, Position.Y);
        Size = new Vector2(width, Size.Y);

        // Визуальное выделение (например, белая рамка или изменение цвета)
        Color = IsSelected ? new Color(0.8f, 0.8f, 1.0f) : new Color(0.4f, 0.4f, 0.4f);
    }

    
}