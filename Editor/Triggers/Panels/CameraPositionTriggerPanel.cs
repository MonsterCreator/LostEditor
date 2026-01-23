using Godot;
using System;
using System.ComponentModel;
// ОШИБКА 2 и 3: Нужно добавить этот using для CallerMemberName
using System.Runtime.CompilerServices; 

namespace LostEditor;

public partial class CameraPositionTriggerPanel : Control, INotifyPropertyChanged
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
        
        LoadData(trigger);
    }

    public override void _Ready()
    {
        // --- Поле X ---
        camPosXLineEdit.DataChanged += (newData) => {
            GD.Print($"[UI] Изменено значение X: {newData}");
            if (_currentTrigger != null) {
                _currentTrigger.CameraPositionX = (float)newData;
                GD.Print($"[Data] В объект записано CameraPositionX: {_currentTrigger.CameraPositionX}");
            }
            OnPropertyChanged(nameof(camPosXLineEdit));
        };

        camPosXCheckButton.Toggled += (toggled) => {
            GD.Print($"[UI] Чекбокс X: {toggled}");
            if (_currentTrigger != null) {
                _currentTrigger.IsCameraPositionXActive = toggled;
            }
            OnPropertyChanged(nameof(camPosXCheckButton));
        };

        // --- Поле Y ---
        camPosYLineEdit.DataChanged += (newData) => {
            GD.Print($"[UI] Изменено значение Y: {newData}");
            if (_currentTrigger != null) {
                _currentTrigger.CameraPositionY = (float)newData;
                GD.Print($"[Data] В объект записано CameraPositionY: {_currentTrigger.CameraPositionY}");
            }
            OnPropertyChanged(nameof(camPosYLineEdit));
        };

        camPosYCheckButton.Toggled += (toggled) => {
            GD.Print($"[UI] Чекбокс Y: {toggled}");
            if (_currentTrigger != null) {
                _currentTrigger.IsCameraPositionYActive = toggled;
            }
            OnPropertyChanged(nameof(camPosYCheckButton));
        };

        // --- Панель типа анимации (Easing) ---
        easeTypePanel.OnEaseTypeSelectedAction += (index) => {
            GD.Print($"[UI] Выбран Easing индекс: {index}");
            if (_currentTrigger != null) {
                _currentTrigger.EasingType = (EasingType)index;
            }
            OnPropertyChanged(nameof(easeTypePanel));
        };

        // --- Базовые данные (BaseDataTriggerPanel) ---
        baseDataTriggerPanel.startTimeLineEdit.DataChanged += (newData) => {
            GD.Print($"[UI] Start Time: {newData}");
            if (_currentTrigger != null) {
                _currentTrigger.startTime = (float)newData;
            }
        };

        baseDataTriggerPanel.transitionTimeLineEdit.DataChanged += (newData) => {
            GD.Print($"[UI] Transition Time: {newData}");
            if (_currentTrigger != null) {
                _currentTrigger.endTime = (float)newData;
            }
        };

        baseDataTriggerPanel.triggerType.ItemSelected += (long index) => {
            GD.Print($"[UI] Тип триггера изменен: {index}");
            if (_currentTrigger != null) {
                _currentTrigger.triggerType = (TriggerType)index;
            }
        };

        baseDataTriggerPanel.isAdditiveCheckButton.Toggled += (bool isAdditive) => {
            GD.Print($"[UI] Тип триггера изменен: {isAdditive}");
            if (_currentTrigger != null) {
                _currentTrigger.IsAdditive = isAdditive;
            }
        };
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public void LoadData(TriggerCameraPosition trigger)
    {
        _currentTrigger = trigger;
        // Твой метод загрузки данных
        camPosXCheckButton.ButtonPressed = trigger.IsCameraPositionXActive;
        camPosXLineEdit.SetValueWithoutNotify(trigger.CameraPositionX);
        camPosYCheckButton.ButtonPressed = trigger.IsCameraPositionYActive;
        camPosYLineEdit.SetValueWithoutNotify(trigger.CameraPositionY);
        easeTypePanel.SetType(trigger.EasingType);
        baseDataTriggerPanel.startTimeLineEdit.SetValueWithoutNotify(trigger.startTime);
        baseDataTriggerPanel.transitionTimeLineEdit.SetValueWithoutNotify(trigger.endTime);
        baseDataTriggerPanel.triggerType.Select((int)trigger.triggerType);
        
        GD.Print($"ДАННЫЕ ЗАДАНЫ {trigger.CameraPositionX} {trigger.CameraPositionY}");
    }
}