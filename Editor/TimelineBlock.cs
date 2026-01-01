using Godot;
using LostEditor;

public partial class TimelineBlock : ColorRect
{
    public GameObject Data;
    public bool IsSelected = false;
    
    // Ссылка на редактор
    public Editor EditorRef;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _GuiInput(InputEvent @event)
    {
        // ПРОВЕРКА: Убедитесь, что в Editor.cs поле InputSys публичное!
        if (EditorRef == null || EditorRef.InputSys == null || Data == null) return;

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                // ИСПРАВЛЕНО: Вызываем метод у InputHandler вместо старого HandleBlockSelection
                EditorRef.InputSys.OnBlockClicked(this);
                
                GetViewport().SetInputAsHandled(); 
            }
        }
    }

    public void UpdateVisual(float pps)
    {
        if (Data == null) return;
        
        float posX = Data.startTime * pps;
        float width = (Data.endTime - Data.startTime) * pps;

        // Обновляем только X и Width. Y меняется при смене строки (Row)
        Position = new Vector2(posX, Position.Y);
        Size = new Vector2(width, Size.Y);

        Color = IsSelected ? new Color(0.8f, 0.8f, 1.0f) : new Color(0.4f, 0.4f, 0.4f);
    }
}