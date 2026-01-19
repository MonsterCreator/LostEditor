using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
public interface IKeyframe
    {
        // Событие, на которое будут подписываться UI-элементы или системы
        event Action OnDataChanged;
        event Action OnTimeChanged;

        object Value { get; set; }
        float Time { get; set; }
        EasingType EasingType { get; set; } // Добавил set, чтобы можно было менять
        KeyframeType kType { get; }
    }

    public partial class Keyframe<T> : IKeyframe
    {
        public event Action OnDataChanged;
        public event Action OnTimeChanged;

        private float _time;
        private T _value;
        private EasingType _easingType;
        private KeyframeType _kType;

        public float Time
        {
            get => _time;
            set
            {
                // Проверка на равенство, чтобы не спамить событиями, если значение не изменилось
                if (EqualityComparer<float>.Default.Equals(_time, value)) return;
                _time = value;
                OnTimeChanged?.Invoke();
                NotifyListeners();
            }
        }

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;
                _value = value;
                NotifyListeners();
            }
        }

        public EasingType EasingType
        {
            get => _easingType;
            set
            {
                if (_easingType == value) return;
                _easingType = value;
                NotifyListeners();
            }
        }

        public KeyframeType kType
        {
            get => _kType;
            set => _kType = value; // Тип обычно не меняется в процессе, но можно добавить Notify
        }

        // Явная реализация интерфейса для object Value
        object IKeyframe.Value
        {
            get => Value;
            set => Value = (T)value;
        }

        // Вспомогательный метод для вызова события
        private void NotifyListeners()
        {
            OnDataChanged?.Invoke();
        }
    }


    public enum KeyframeType
    {
        PositionX = 0,
        PositionY = 1,
        ScaleX = 2,
        ScaleY = 3,
        Rotation = 4,
        Color = 5,
        Custom = 6

    }

}