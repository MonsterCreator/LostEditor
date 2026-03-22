using Godot;
using Godot.NativeInterop;
using LostEditor; // Чтобы видеть GameObject

public partial class TimelineBlock : Panel
{
    [Export] public Button button;
    [Export] public Label objectName;
    public GameObject Data;

    private TimelineObjectController _timelineObjController;
    private TimelineController _timelineController;
    private SelectionManager _selectionManager;

    private float pps => _timelineController.PixelsPerSecond;

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
        Data.OnEndTimeChanged += UpdateVisual;
        
        
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

    private void ButtonPressed() // сигнал pressed
    {
        if (editor == null || Data == null) return;
        // Вся логика выделения в StartDraggingBlock / FinalizeDrag.
        // Здесь только финальное обновление визуала.
        UpdateVisual();
    }



    private void ButtonDown() // сигнал button_down
    {
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
        UpdateVisual();
    }
    public void UpdateVisual()
    {
        if (Data == null) return;
        
        if (objectName != null) objectName.Text = Data.name;

        // 1. Получаем ЧИСТУЮ локальную длительность объекта
        float duration = Data.cachedEndTime;

        if(Data.endTimeMode == EndTimeMode.GlobalTime) duration = Data.cachedEndTime - Data.startTime;
        
        // 2. Обработка исключения для бесконечных объектов
        if (Data.endTimeMode == EndTimeMode.NoEndTime) 
        {
            // Для бесконечных объектов задаем фиксированную визуальную длину (например, 10 сек)
            duration = 10.0f; 
        }

        // 3. Расчет позиции и размера
        // Позиция X зависит от глобального начала
        float posX = Data.startTime * pps;
        
        // Ширина зависит ТОЛЬКО от длительности (duration)
        float width = duration * pps;

        // Защита: минимум 10 пикселей ширины для кликабельности
        width = Mathf.Max(width, 10f); 

        // Применяем значения
        Position = new Vector2(posX, Position.Y);
        Size = new Vector2(width, Size.Y);

        SelfModulate = IsSelected ? selectedColor : defaultColor;
    }

    
}