using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
    public partial class GameObject : Node2D
    {
        [Export] public Polygon2D shapeObj;
        [Export] public CollisionPolygon2D collisionShapeObj;

        private string _name;
        public string name
        {
            get => name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    // Сообщаем всем подписчикам, что данные изменились
                    OnDataChanged?.Invoke(); 
                }
            }
        }
        public event Action OnDataChanged;

        private float _startTime;
        public float startTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    // Сообщаем всем подписчикам, что данные изменились
                    OnDataChanged?.Invoke(); 
                }
            }
        }
        private float _endTime;
        public float endTime
        {
            get => _endTime;
            set
            {
                if (_endTime != value)
                {
                    _endTime = value;
                    // Сообщаем всем подписчикам, что данные изменились
                    OnDataChanged?.Invoke(); 
                }
            }
        }


        
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