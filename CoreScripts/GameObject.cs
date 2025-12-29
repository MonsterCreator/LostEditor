using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
    public partial class GameObject : Node2D
    {
        [Export] public Polygon2D shapeObj;
        [Export] public CollisionPolygon2D collisionShapeObj;

        [Export] public string Name = "Base name";
        [Export] public float startTime;
        [Export] public float endTime;
        
        // ВСЕ ОБЯЗАТЕЛЬНЫЕ СВОЙСТВА ДОЛЖНЫ БЫТЬ ОБЪЯВЛЕНЫ
        public List<KeyframePosX> keyframePosX { get; set; } = new();
        public List<KeyframePosY> keyframePosY { get; set; } = new();
        public List<KeyframeSizeX> keyframeSizeX { get; set; } = new();
        public List<KeyframeSizeY> keyframeSizeY { get; set; } = new();
        public List<KeyframeRotation> keyframeRotation { get; set; } = new();
        public List<KeyframeColor> keyframeColor { get; set; } = new();
    }
    
    public enum EndTimeMode {
        FixedTime,          // Фиксированное время (то, что мы делали раньше)
        LastKeyframe,       // Строго по последнему ключу
        LastKeyframeOffset, // Последний ключ + N секунд
        GlobalTime          // Игнорирует всё (бесконечно или до конца таймлайна)
    }
}