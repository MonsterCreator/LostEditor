using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
    public partial class GameObject : Node2D
    {
        [Export] public Polygon2D shapeObj;
        [Export] public CollisionPolygon2D collisionShapeObj;

        public float startTime { get; set; }
        public float endTime { get; set; }
        
        // ВСЕ ОБЯЗАТЕЛЬНЫЕ СВОЙСТВА ДОЛЖНЫ БЫТЬ ОБЪЯВЛЕНЫ
        public List<KeyframePosX> keyframePosX { get; set; } = new();
        public List<KeyframePosY> keyframePosY { get; set; } = new();
        public List<KeyframeSizeX> keyframeSizeX { get; set; } = new();
        public List<KeyframeSizeY> keyframeSizeY { get; set; } = new();
        public List<KeyframeRotation> keyframeRotation { get; set; } = new();
        public List<KeyframeColor> keyframeColor { get; set; } = new();
    }
}