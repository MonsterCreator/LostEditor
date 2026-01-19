using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
    public partial class GameObject : Node2D
    {
        [Export] public Polygon2D shapeObj;
        [Export] public CollisionPolygon2D collisionShapeObj;

        public event Action OnDataChanged;

        private string _name;
        public string name
        {
            get => _name;
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
        

        private float _startTime;
        public float startTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    // Пересчитываем, так как LastKeyframe зависит от startTime
                    RecalculateEndTime(); 
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
                    RecalculateEndTime();
                    OnDataChanged?.Invoke(); 
                }
            }
        }

        private float _endTimeOffset;
        public float endTimeOffset
        {
            get => _endTimeOffset;
            set
            {
                if (_endTimeOffset != value)
                {
                    _endTimeOffset = value;
                    // Сообщаем всем подписчикам, что данные изменились
                    RecalculateEndTime();
                    OnDataChanged?.Invoke(); 
                }
            }
        }

        private EndTimeMode _endTimeMode;
        public EndTimeMode endTimeMode
        {
            get => _endTimeMode;
            set
            {
                if (_endTimeMode != value)
                {
                    _endTimeMode = value;
                    // Сообщаем всем подписчикам, что данные изменились
                    RecalculateEndTime();
                    OnDataChanged?.Invoke(); 
                }
            }
        }


        public float cachedEndTime { get; private set; }
        
        public override void _Ready()
        {
            // Принудительно пересчитываем время окончания при старте,
            // чтобы убедиться, что все ключи учтены.
            RecalculateEndTime();
        }

        // Вызываем этот метод всякий раз, когда меняются списки ключей или режим времени
        public void RecalculateEndTime()
        {
            float maxKeyTime = GetMaxKeyframeTime();

            cachedEndTime = _endTimeMode switch
            {
                EndTimeMode.NoEndTime => float.MaxValue,
                EndTimeMode.FixedTime => endTime, 
                EndTimeMode.LastKeyframe => maxKeyTime,
                EndTimeMode.LastKeyframeOffset => maxKeyTime + endTimeOffset,
                
                // ОБНОВЛЕНО: Используем endTime как абсолютную точку на таймлайне
                EndTimeMode.GlobalTime => endTime, 
                
                _ => endTime
            };

            OnDataChanged?.Invoke();
        }

        private float GetMaxKeyframeTime()
        {
            float maxT = 0f;
            // Делаем проверку только если в списке есть ключи
            if (keyframePosX.Count > 0) maxT = Math.Max(maxT, keyframePosX[^1].Time);
            if (keyframePosY.Count > 0) maxT = Math.Max(maxT, keyframePosY[^1].Time);
            if (keyframeSizeX.Count > 0) maxT = Math.Max(maxT, keyframeSizeX[^1].Time);
            if (keyframeSizeY.Count > 0) maxT = Math.Max(maxT, keyframeSizeY[^1].Time);
            if (keyframeRotation.Count > 0) maxT = Math.Max(maxT, keyframeRotation[^1].Time);
            if (keyframeColor.Count > 0) maxT = Math.Max(maxT, keyframeColor[^1].Time);
            return maxT;
        }

        public void SortKeyframes() 
        {
            keyframePosX.Sort((a, b) => a.Time.CompareTo(b.Time));
            keyframePosY.Sort((a, b) => a.Time.CompareTo(b.Time));
            // ... для остальных типов
            OnDataChanged?.Invoke(); 
        }

        
        
        // ВСЕ ОБЯЗАТЕЛЬНЫЕ СВОЙСТВА ДОЛЖНЫ БЫТЬ ОБЪЯВЛЕНЫ
        public List<Keyframe<float>> keyframePosX { get; set; } = new();
        public List<Keyframe<float>> keyframePosY { get; set; } = new();
        public List<Keyframe<float>> keyframeSizeX { get; set; } = new();
        public List<Keyframe<float>> keyframeSizeY { get; set; } = new();
        public List<Keyframe<float>> keyframeRotation { get; set; } = new();
        public List<Keyframe<Color>> keyframeColor { get; set; } = new();
    }
    
    public enum EndTimeMode {
        NoEndTime,
        FixedTime,          // Фиксированное время (то, что мы делали раньше)
        LastKeyframe,       // Строго по последнему ключу
        LastKeyframeOffset, // Последний ключ + N секунд
        GlobalTime          // Игнорирует всё (бесконечно или до конца таймлайна)
    }

}