using Godot;
using System;

public partial class TestScene : Node2D
{
    double time = 0f;
    [Export] public Label debugLabel;
    public override void _Process(double delta)
    {

        debugLabel.Text = Engine.GetFramesPerSecond().ToString();


    }

    
}
