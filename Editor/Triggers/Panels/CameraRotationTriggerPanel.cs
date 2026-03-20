using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices; 

namespace LostEditor;
public partial class CameraRotationTriggerPanel : Control, ITriggerPanel, INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
	private TriggerCameraRotation _currentTrigger;

	[Export] public InputLineEdit cameraRotationLineEdit;
	[Export] public EaseTypePanel easeTypePanel;


	protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

	public void LoadDataToPanel(Trigger abstarctTrigger)
    {
        var trigger = abstarctTrigger as TriggerCameraRotation;

        if (trigger == null) 
        {
            GD.PrintErr("[CameraPanelRotation] Передан неверный тип триггера!");
            return;
        }

        _currentTrigger = trigger; 
        GD.Print("ЗАГРУЗКА ДАННЫХ В ПАНЕЛЬ");

        cameraRotationLineEdit.SetValueWithoutNotify(trigger.CameraRotation);
        easeTypePanel.SetType(trigger.EasingType);
    }

	public void SubscribePanelChanges()
    {
        if (_currentTrigger == null) return;
        GD.Print("ПОДПИСКА НА СОБЫТИЯ ПАНЕЛИ ВРАЩЕНИЯ");

        // Сначала отписываемся (на всякий случай), чтобы не было двойной подписки
        UnsubscribePanelChanges();

        // Подписываемся на события
        cameraRotationLineEdit.DataChanged += OnCamRotationChanged;
        easeTypePanel.OnEaseTypeSelectedAction += OnEaseTypeSelected;
    }

    public void UnsubscribePanelChanges()
    {
        cameraRotationLineEdit.DataChanged -= OnCamRotationChanged;
        easeTypePanel.OnEaseTypeSelectedAction -= OnEaseTypeSelected;
    }

	private void OnCamRotationChanged(Variant newData) // Предполагаю double, судя по (float)newData
    {
        GD.Print("ВРАЩЕНИЕ ИЗМЕНЕНО");
        if (_currentTrigger != null)
        {
            _currentTrigger.CameraRotation = (float)newData;
            OnPropertyChanged(nameof(cameraRotationLineEdit));
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
