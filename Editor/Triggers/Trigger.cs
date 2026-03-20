 using Godot;

using System;

using System.Collections.Generic;

using System.ComponentModel;

using System.Runtime.CompilerServices;



namespace LostEditor;



public partial class Trigger : Node, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private bool _isAdditive;
    [Export] public bool IsAdditive { get => _isAdditive; set => SetField(ref _isAdditive, value); }

    protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        GD.Print($"[Trigger Object] Свойство {propertyName} изменилось на {value}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private TriggerType _triggerType;
    public TriggerType triggerType { get => _triggerType; set => SetField(ref _triggerType, value); }

    private float _startTime;
    public float startTime { get => _startTime; set => SetField(ref _startTime, value); }

    private float _endTime; // В вашем коде это используется как Длительность
    public float endTime { get => _endTime; set => SetField(ref _endTime, value); }

    private EasingType _easingType;
    public EasingType EasingType { get => _easingType; set => SetField(ref _easingType, value); }

    /// <summary>
    /// Возвращает прогресс триггера от 0.0 до 1.0 с учетом типа сглаживания.
    /// </summary>
    public float GetProgress(double currentTime)
    {
        // Если длительность (endTime) <= 0, триггер срабатывает мгновенно
        if (endTime <= 0) return (float)currentTime >= startTime ? 1f : 0f;

        // Рассчитываем линейный прогресс
        float t = (float)(currentTime - startTime) / endTime;
        t = Math.Clamp(t, 0f, 1f);

        // Применяем Easing (интерполяцию)
        return Interpolate(t, EasingType);
    }

    // Временный метод интерполяции, пока вы не наполните свой класс Easings
    private float Interpolate(float t, EasingType type)
    {
        switch (type)
        {
            case EasingType.Instant: return t >= 1.0f ? 1.0f : 0.0f;
            case EasingType.Linear: return t;
            case EasingType.InQuad: return t * t;
            case EasingType.OutQuad: return t * (2 - t);
            case EasingType.InOutQuad: return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
            // Добавьте остальные типы из вашего enum по мере необходимости
            default: return t; 
        }
    }
}


public partial class TriggerCameraPosition : Trigger

{

    private bool _isCameraPositionXActive;

    public bool IsCameraPositionXActive { get => _isCameraPositionXActive; set => SetField(ref _isCameraPositionXActive, value); }


    private float _cameraPositionX;

    public float CameraPositionX { get => _cameraPositionX; set => SetField(ref _cameraPositionX, value); }


    private bool _isCameraPositionYActive;

    public bool IsCameraPositionYActive { get => _isCameraPositionYActive; set => SetField(ref _isCameraPositionYActive, value); }


    private float _cameraPositionY;

    public float CameraPositionY { get => _cameraPositionY; set => SetField(ref _cameraPositionY, value); }

} 

// ТРИГГЕР ЗУМА
public partial class TriggerCameraZoom : Trigger
{
    private float _cameraZoom;
    public float CameraZoom 
    { 
        get => _cameraZoom; 
        set => SetField(ref _cameraZoom, value); 
    }
}

// ТРИГГЕР РОТАЦИИ
public partial class TriggerCameraRotation : Trigger
{
    private float _cameraRotation;
    public float CameraRotation 
    { 
        get => _cameraRotation; 
        set => SetField(ref _cameraRotation, value); 
    }
}

// ТРИГГЕР ТРЯСКИ КАМЕРЫ
public partial class TriggerCameraShake : Trigger
{
    private bool _isCameraShakeStrenghtActive;
    public bool IsCameraShakeStrenghtActive 
    { 
        get => _isCameraShakeStrenghtActive; 
        set => SetField(ref _isCameraShakeStrenghtActive, value); 
    }

    private float _cameraShakeStrenght;
    public float CameraShakeStrenght 
    { 
        get => _cameraShakeStrenght; 
        set => SetField(ref _cameraShakeStrenght, value); 
    }

    private bool _isCameraShakeSpeedActive;
    public bool IsCameraShakeSpeedActive 
    { 
        get => _isCameraShakeSpeedActive; 
        set => SetField(ref _isCameraShakeSpeedActive, value); 
    }

    private float _cameraShakeSpeed;
    public float CameraShakeSpeed 
    { 
        get => _cameraShakeSpeed; 
        set => SetField(ref _cameraShakeSpeed, value); 
    }

    private bool _isCameraShakeXActive;
    public bool IsCameraShakeXActive 
    { 
        get => _isCameraShakeXActive; 
        set => SetField(ref _isCameraShakeXActive, value); 
    }

    private float _cameraShakeX;
    public float CameraShakeX 
    { 
        get => _cameraShakeX; 
        set => SetField(ref _cameraShakeX, value); 
    }

    private bool _isCameraShakeYActive;
    public bool IsCameraShakeYActive 
    { 
        get => _isCameraShakeYActive; 
        set => SetField(ref _isCameraShakeYActive, value); 
    }

    private float _cameraShakeY;
    public float CameraShakeY 
    { 
        get => _cameraShakeY; 
        set => SetField(ref _cameraShakeY, value); 
    }
}

