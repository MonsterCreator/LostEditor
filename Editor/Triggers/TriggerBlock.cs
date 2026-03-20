 using Godot;

using System;

using System.Linq;

using System.Collections.Generic;

using System.ComponentModel;

using System.Runtime.CompilerServices;


namespace LostEditor;


public partial class TriggerBlock : Panel

{

    public event Action<TriggerBlock, InputEvent> OnInputEvent;


    [Export] public TextureRect triggerIcon;

    [Export] public Texture2D[] TriggerIconDataArray;

    [Export] public Control DataLeft;

    [Export] public Control DataRight;

    [Export] public Color[] TriggerTypeColor;

    private float _pps = 100;
    protected Trigger _trigger;




    private bool _isSelected = false;

    public bool IsSelected

    {

        get => _isSelected;

        set

        {

            _isSelected = value;

            UpdateSelectionVisual();

        }

    }

    public Trigger GetTriggerData()

    {

        return _trigger;

    }


    public override void _GuiInput(InputEvent @event)

    {

        base._GuiInput(@event);

        // Передаем себя и событие наверх.

        // Manager подпишется на это событие при создании блока.

        OnInputEvent?.Invoke(this, @event);

       

        // Опционально: поглощаем клик, чтобы он не прошел на таймлайн под блоком

        if (@event is InputEventMouseButton)
        {
            AcceptEvent();
        }
    }

    public void Setup(Trigger trigger, TimelineController controller)
    {
        controller.OnPPSChanged += OnPPSChanged;
        if (_trigger != null) _trigger.PropertyChanged -= OnTriggerPropertyChanged;

        _trigger = trigger;
        _trigger.PropertyChanged += OnTriggerPropertyChanged;
        
        // Передаем текущий PPS для первой отрисовки
        UpdateBlockVisual(); 
    }

    private void OnPPSChanged(float newPPS) 
    {
        _pps = newPPS;
        GD.Print($"PPS IS {newPPS}");
        UpdateBlockVisual();
    }


    private void OnTriggerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Trigger.startTime):
            case nameof(Trigger.endTime):
                UpdateBlockVisual(); // Обновляем позицию/размер
                break;
            case nameof(Trigger.triggerType):
                UpdateIcon();
                UpdateSelectionVisual();
                break;
        }
    }


    public void ChangeTriggerType()
    {
        switch (_trigger.triggerType)
        {
            case TriggerType.CameraPosition: GD.Print("TriggerType CameraPos selected"); break;

            case TriggerType.CameraZoom: GD.Print("TriggerType CameraZoom selected"); break;

            case TriggerType.CameraRotation: GD.Print("TriggerType CameraRotation selected"); break;

            case TriggerType.CameraShake: GD.Print("TriggerType CameraShake selected"); break;

            case TriggerType.ColorChange: GD.Print("TriggerType ColorChange selected"); break;

            default: return;
        }
    }

    public void Unsubscribe(TimelineController tController)
    {
        tController.OnPPSChanged -= OnPPSChanged;
        _trigger.PropertyChanged -= OnTriggerPropertyChanged;
    }

    public void OnIconChanged(int iconID)
    {
        triggerIcon.Texture = TriggerIconDataArray[iconID];
    }

    
    protected void ClearData()
    {

        DataLeft.GetChildren().ToList().ForEach(child => child.QueueFree());

        DataRight.GetChildren().ToList().ForEach(child => child.QueueFree());

    }


    public void UpdateBlockVisual()
    {
        if (_trigger == null) return;

        // Рассчитываем ширину на основе PPS
        float targetWidth = _trigger.endTime * _pps;
        if(targetWidth < 200) targetWidth = 200;
        
        // 1. Устанавливаем позицию (работает, если родитель — просто Control, а не Container)
        Position = new Vector2(_trigger.startTime * _pps, Position.Y);

        // 2. Устанавливаем размер
        // Используем Size для мгновенного обновления и CustomMinimumSize для контейнеров
        Vector2 newSize = new Vector2(targetWidth, Size.Y);
        CustomMinimumSize = newSize; 
        Size = newSize;

        UpdateIcon();
        UpdateSelectionVisual();
    }


    private void UpdateIcon()
    {
        triggerIcon.Texture = TriggerIconDataArray[(int)_trigger.triggerType];
    }


    private void UpdateSelectionVisual()
    {

        if(_isSelected) GD.Print(_isSelected);

        Color baseColor = TriggerTypeColor[(int)_trigger.triggerType];

        SelfModulate = _isSelected

            ? Color.FromHsv(baseColor.H, baseColor.S, Math.Clamp(baseColor.V + 0.2f, 0f, 1f))

            : baseColor;

    }

    // Добавьте этот метод в класс TriggerBlock
    public void SwapTriggerData(Trigger newTrigger)
    {
        // 1. Отписываемся от старого триггера
        if (_trigger != null)
        {
            _trigger.PropertyChanged -= OnTriggerPropertyChanged;
        }

        // 2. Меняем ссылку
        _trigger = newTrigger;

        // 3. Подписываемся на новый
        if (_trigger != null)
        {
            _trigger.PropertyChanged += OnTriggerPropertyChanged;
        }

        // 4. Обновляем визуал (иконку, цвет)
        UpdateIcon();
        UpdateSelectionVisual();
        UpdateBlockVisual(); // Обновить ширину/позицию
        
        // Если у вас есть специфичный контент в блоке (DataRight), его тоже надо обновить
        // Но так как TriggerBlock - это общий класс, специфику обновлять сложнее. 
        // Обычно при смене типа просто очищают превью данных.
        ClearData(); 
    }

}


public partial class TriggerBlockCameraPosition: TriggerBlock
{
    public TriggerCameraPosition trigger;
    public void UpdateVisualContent(float posX, float posY)
    {
        ClearData();

        var label = new Label();

        DataRight.AddChild(label);

        label.Text = $"X {posX} Y {posY}";
    }
}


