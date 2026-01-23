 using Godot;

using System;

using System.Collections.Generic;

using System.ComponentModel;

using System.Runtime.CompilerServices;



namespace LostEditor;



public partial class Trigger : Node, INotifyPropertyChanged

{

    public event PropertyChangedEventHandler PropertyChanged;


    // Универсальный метод для установки значений

    protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)

    {

        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        field = value;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }


    private TriggerType _triggerType;

    public TriggerType triggerType { get => _triggerType; set => SetField(ref _triggerType, value); }


    private float _startTime;

    public float startTime { get => _startTime; set => SetField(ref _startTime, value); }


    private float _endTime;

    public float endTime { get => _endTime; set => SetField(ref _endTime, value); }


    private EasingType _easingType;

    public EasingType EasingType { get => _easingType; set => SetField(ref _easingType, value); }

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
    private bool _isCameraZoomActive;
    public bool IsCameraZoomActive 
    { 
        get => _isCameraZoomActive; 
        set => SetField(ref _isCameraZoomActive, value); 
    }

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
    private bool _isCameraRotationActive;
    public bool IsCameraRotationActive 
    { 
        get => _isCameraRotationActive; 
        set => SetField(ref _isCameraRotationActive, value); 
    }

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

// ТРИГГЕР ИЗМЕНЕНИЯ ЦВЕТА
public partial class TriggerColorChange : Trigger
{
    private List<int> _colorId;
    public List<int> ColorId 
    { 
        get => _colorId; 
        set => SetField(ref _colorId, value); 
    }
}