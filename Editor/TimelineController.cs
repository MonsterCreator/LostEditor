using Godot;
using System;

[GlobalClass, Tool]
public partial class TimelineController : Node
{
    [Signal] public delegate void TimeChangedEventHandler(float newTime);
    [Signal] public delegate void ZoomChangedEventHandler(float newPixelsPerSecond);

    [Export] public float PixelsPerSecond = 100f;
    [Export] public float MaxTime = 60f;
    [Export] public Label DebugLabel;
    
    // Текущее время
    public float CurrentTime { get; private set; } = 0f;
    public bool IsPlaying { get; set; } = false;

    public override void _PhysicsProcess(double delta)
    {
        DebugLabel.Text = PixelsPerSecond.ToString();
        // В режиме игры увеличиваем время
        if (!Engine.IsEditorHint() && IsPlaying)
        {
            SetTime(CurrentTime + (float)delta);
        }
    }

    public void SetTime(float time)
    {
        // Ограничиваем время
        CurrentTime = Mathf.Clamp(time, 0, MaxTime);
        EmitSignal(SignalName.TimeChanged, CurrentTime);
    }

    public void SetZoom(float newPPS)
    {
        PixelsPerSecond = Mathf.Clamp(newPPS, 10f, 2000f);
        EmitSignal(SignalName.ZoomChanged, PixelsPerSecond);
    }
}