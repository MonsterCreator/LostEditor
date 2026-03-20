using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices; 

namespace LostEditor;

public partial class CameraPositionTriggerPanel : Control, ITriggerPanel, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private TriggerCameraPosition _currentTrigger;

    [Export] public BaseDataTriggerPanel baseDataTriggerPanel;
    [Export] public InputLineEdit camPosXLineEdit;
    [Export] public CheckButton camPosXCheckButton;
    [Export] public InputLineEdit camPosYLineEdit;
    [Export] public CheckButton camPosYCheckButton;
    [Export] public EaseTypePanel easeTypePanel;

    public void Bind(TriggerCameraPosition trigger)
    {
        if (trigger == null)
        {
            GD.PrintErr("!!! ПОПЫТКА ПРИВЯЗАТЬ NULL ТРИГГЕР !!!");
            return;
        }
        
        _currentTrigger = trigger;
        GD.Print($"[Panel] Привязан триггер: {trigger.GetHashCode()}. Текущее X: {trigger.CameraPositionX}");
        
        LoadDataToPanel(trigger);
    }


    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }



    public void LoadDataToPanel(Trigger abstarctTrigger)
    {
        var trigger = abstarctTrigger as TriggerCameraPosition;

        if (trigger == null) 
        {
            GD.PrintErr("[CameraPanel] Передан неверный тип триггера!");
            return;
        }

        // --- ГЛАВНОЕ ИСПРАВЛЕНИЕ ---
        // Сохраняем ссылку на триггер в переменную класса, 
        // чтобы обработчики событий знали, что менять.
        _currentTrigger = trigger; 
        // ---------------------------

        GD.Print("ЗАГРУЗКА ДАННЫХ В ПАНЕЛЬ");

        camPosXCheckButton.ButtonPressed = trigger.IsCameraPositionXActive;
        camPosXLineEdit.SetValueWithoutNotify(trigger.CameraPositionX);
        camPosYCheckButton.ButtonPressed = trigger.IsCameraPositionYActive;
        camPosYLineEdit.SetValueWithoutNotify(trigger.CameraPositionY);
        easeTypePanel.SetType(trigger.EasingType);
    }

    public void SubscribePanelChanges()
    {
        if (_currentTrigger == null) return;
        GD.Print("ПОДПИСКА НА СОБЫТИЯ ПАНЕЛИ");

        // Подписываемся на события
        camPosXLineEdit.DataChanged += OnCamPosXChanged;
        camPosXCheckButton.Toggled += OnCamPosXActiveToggled;
        camPosYLineEdit.DataChanged += OnCamPosYChanged;
        camPosYCheckButton.Toggled += OnCamPosYActiveToggled;
        easeTypePanel.OnEaseTypeSelectedAction += OnEaseTypeSelected;
    }

    public void UnsubscribePanelChanges()
    {
        camPosXLineEdit.DataChanged -= OnCamPosXChanged;
        camPosXCheckButton.Toggled -= OnCamPosXActiveToggled;
        camPosYLineEdit.DataChanged -= OnCamPosYChanged;
        camPosYCheckButton.Toggled -= OnCamPosYActiveToggled;
        easeTypePanel.OnEaseTypeSelectedAction -= OnEaseTypeSelected;
    }
    private void OnCamPosXChanged(Variant newData) // Предполагаю double, судя по (float)newData
    {
        GD.Print("ПОЛЕ ПОЗИЦИИ Х ИЗМЕНЕНО");
        if (_currentTrigger != null)
        {
            _currentTrigger.CameraPositionX = (float)newData;
            OnPropertyChanged(nameof(camPosXLineEdit));
        }
    }

    private void OnCamPosXActiveToggled(bool toggled)
    {
        if (_currentTrigger != null)
        {
            _currentTrigger.IsCameraPositionXActive = toggled;
            OnPropertyChanged(nameof(camPosXCheckButton));
        }
    }

    private void OnCamPosYChanged(Variant newData)
    {
        if (_currentTrigger != null)
        {
            _currentTrigger.CameraPositionY = (float)newData;
            OnPropertyChanged(nameof(camPosYLineEdit));
        }
    }

    private void OnCamPosYActiveToggled(bool toggled)
    {
        if (_currentTrigger != null)
        {
            _currentTrigger.IsCameraPositionYActive = toggled;
            OnPropertyChanged(nameof(camPosYCheckButton));
        }
    }

    private void OnEaseTypeSelected(int index)
    {
        if (_currentTrigger != null)
        {
            _currentTrigger.EasingType = (EasingType)index;
            OnPropertyChanged(nameof(easeTypePanel));
        }
    }
}