using Godot;
using System;
using System.Collections.Generic;

public partial class GameObject : Node2D
{
    [Export] public Polygon2D shapeObj;
    [Export] public CollisionPolygon2D collisionShapeObj;

    public float startTime { get; set; }
    public float endTime { get; set; }
    public List<KeyframePosX> keyframePosX { get; set; }
    public List<KeyframePosY> keyframePosY { get; set; }


}
