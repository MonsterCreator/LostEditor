using Godot;
using LostEditor;


public partial class KeyframeScrollContainer : ScrollContainer
{
    [Export] public KeyframesPanelMain keyframesPanel;
    [Export] public float ScrollSpeed = 30f;

	
    public override void _Ready()
    {
        FollowFocus = false;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mb) return;
        if (mb.ButtonIndex != MouseButton.WheelUp && mb.ButtonIndex != MouseButton.WheelDown) return;

        float direction = mb.ButtonIndex == MouseButton.WheelUp ? -1f : 1f;

        // Shift + колесо = зум
        if (Input.IsKeyPressed(Key.Shift))
        {
            if (keyframesPanel == null) return;
            float factor = mb.ButtonIndex == MouseButton.WheelUp ? 1.15f : 0.87f;
            keyframesPanel.ApplyZoom(factor, this);
            AcceptEvent();
            return;
        }

        // Без модификатора = горизонтальный скролл
        ScrollHorizontal += (int)(direction * ScrollSpeed);
        AcceptEvent();
    }
	
}