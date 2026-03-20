using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
    public partial class GameObject : Node2D
    {
        [Export] public Polygon2D shapeObj;
        [Export] public CollisionPolygon2D collisionShapeObj;

        public ObjectColor objectColor = new ObjectColor();

        public event Action OnDataChanged;
        public event Action OnEndTimeChanged;

        private string _name;
        public string name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
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
                    RecalculateEndTime();
                    OnDataChanged?.Invoke();
                    OnEndTimeChanged?.Invoke();
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
                    if(endTimeMode == EndTimeMode.LastKeyframeOffset) OnEndTimeChanged?.Invoke();
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
                    OnEndTimeChanged?.Invoke();
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
            GD.Print(maxKeyTime);
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
            if (keyframePositionX.Count > 0) maxT = Math.Max(maxT, keyframePositionX[^1].Time);
            if (keyframePositionY.Count > 0) maxT = Math.Max(maxT, keyframePositionY[^1].Time);
            if (keyframeScaleX.Count > 0) maxT = Math.Max(maxT, keyframeScaleX[^1].Time);
            if (keyframeScaleY.Count > 0) maxT = Math.Max(maxT, keyframeScaleY[^1].Time);
            if (keyframeRotation.Count > 0) maxT = Math.Max(maxT, keyframeRotation[^1].Time);
            if (keyframeColor.Count > 0) maxT = Math.Max(maxT, keyframeColor[^1].Time);
            return maxT;
        }

        public void SortKeyframes() 
        {
            keyframePositionX.Sort((a, b) => a.Time.CompareTo(b.Time));
            keyframePositionY.Sort((a, b) => a.Time.CompareTo(b.Time));
            // ... для остальных типов
            OnDataChanged?.Invoke(); 
        }

        public List<Keyframe<float>> keyframePositionX { get; set; } = new();
        public List<Keyframe<float>> keyframePositionY { get; set; } = new();
        public List<Keyframe<float>> keyframeScaleX { get; set; } = new();
        public List<Keyframe<float>> keyframeScaleY { get; set; } = new();
        public List<Keyframe<float>> keyframeRotation { get; set; } = new();
        public List<Keyframe<ObjectColor>> keyframeColor { get; set; } = new();
    }
    
    public enum EndTimeMode {
        NoEndTime,
        FixedTime,
        LastKeyframe,
        LastKeyframeOffset, 
        GlobalTime          
    }

}