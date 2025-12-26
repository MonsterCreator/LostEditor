using Godot;
using LostEditor; // Чтобы видеть GameObject

public partial class TimelineBlock : ColorRect
{
    public GameObject Data;
    public bool IsSelected = false;
    
    // Для перемещения
    private bool _isDragging = false;
    private Vector2 _dragStartPos;
    private float _startTimeAtDrag;
    private int _startRowIndex;

    // Ссылка на редактор для уведомлений
    public Editor EditorRef;

    public override void _Ready()
    {
        // Включаем фильтр мыши, чтобы блок ловил клики
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (EditorRef == null || Data == null) return;

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                // Просто передаем управление Редактору
                EditorRef.HandleBlockSelection(this, Input.IsKeyPressed(Key.Ctrl));
                GetViewport().SetInputAsHandled(); 
            }
        }
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