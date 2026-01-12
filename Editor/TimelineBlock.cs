using Godot;
using LostEditor; // Чтобы видеть GameObject

public partial class TimelineBlock : Panel
{
    [Export] public Button button;
    public GameObject Data;

    private TimelineObjectController _timelineObjController;
    private TimelineController _timelineController;
    private SelectionManager _selectionManager;

    [Export] private Color defaultColor;
    [Export] private Color selectedColor;

    public void Setup
    (
        GameObject data, 
        TimelineController tlc, 
        TimelineObjectController tloc, 
        SelectionManager sm
    )
    {
        // Отписываемся от старого, если был (чтобы избежать утечек памяти)
        if (Data != null) Data.OnDataChanged -= OnDataUpdate;
        Data = data;
        _timelineObjController = tloc;
        _timelineController = tlc;
        _selectionManager = sm;

        // Подписываемся на изменения
        Data.OnDataChanged += OnDataUpdate;
    }


    private void OnDataUpdate()
    {
        // ВАЖНО: Тут нужно знать текущий pps (pixels per second).
        // Обычно pps хранится в EditorManager или TimelineController (синглтон или ссылка).
        UpdateVisual(_timelineController.PixelsPerSecond); 
    }

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

        if(!_timelineObjController.hasMoved)
        {
            if(Input.IsKeyPressed(Key.Ctrl))
            {
                _timelineObjController.HandleBlockSelection(this,true);
            }
            else if(_timelineObjController.GetBlockUnderMouse() != null)
            {
                //editor.timeLineObjectControl.HandleBlockSelection(this,false);
                _selectionManager.SelectBlock(this);
                IsSelected = true;
            }
            else
            {
                _selectionManager.DeselectAll();
            }
            GD.Print("BUTTON PRESSED");
        }
        else _timelineObjController.hasMoved = false;
        

        UpdateVisual(_timelineController.PixelsPerSecond);
        
    }

// В файле TimelineBlock.cs

    private void ButtonDown() // Сигнал button_down
    {
        // Сообщаем контроллеру, что начали тащить ЭТОТ блок
        // Контроллер сам выставит IsDragging = true и обработает выделение
        _timelineObjController.StartDraggingBlock(this);
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
        UpdateVisual(_timelineController.PixelsPerSecond);
    }
    public void UpdateVisual(float pps)
    {
        if (Data == null) return;
        
        float posX = Data.startTime * pps;
        float width = (Data.endTime - Data.startTime) * pps;

        Position = new Vector2(posX, Position.Y);
        Size = new Vector2(width, Size.Y);

        // Визуальное выделение (например, белая рамка или изменение цвета)
        SelfModulate = IsSelected ? selectedColor : defaultColor;
    }

    
}