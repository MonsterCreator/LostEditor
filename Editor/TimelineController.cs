using Godot;
using System;

public partial class TimelineController : Node
{
    public float currentTime;
    [Export] public float maxTime;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        currentTime += (float)delta;
    }

}
