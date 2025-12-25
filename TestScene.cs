using Godot;
using System;

public partial class TestScene : Node2D
{
    double time = 0f;
    [Export] public Label debugLabel;
    [Export] public Label debugLabel2;

    [Export] public Polygon2D pol1;
    [Export] public Polygon2D pol2;

    public override void _Ready()
    {
        pol2.Polygon = (Vector2[])pol1.Polygon.Clone();
    }

    public override void _Process(double delta)
    {

        debugLabel.Text = Engine.GetFramesPerSecond().ToString();


    }

    
}
