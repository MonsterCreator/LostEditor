using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices; 

namespace LostEditor;
public partial class CameraZoomTriggerPanel : Control, ITriggerPanel, INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
	private TriggerCameraZoom _currentTrigger;

	[Export] public InputLineEdit cameraZoomLineEdit;
	[Export] public EaseTypePanel easeTypePanel;


	protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

	public void LoadDataToPanel(Trigger abstarctTrigger)
    {
        var trigger = abstarctTrigger as TriggerCameraZoom;

        if (trigger == null) 
        {
            GD.PrintErr("[CameraPanelZoom] Передан неверный тип триггера!");
            return;
        }

        _currentTrigger = trigger; 
        GD.Print("ЗАГРУЗКА ДАННЫХ В ПАНЕЛЬ");

        cameraZoomLineEdit.SetValueWithoutNotify(trigger.CameraZoom);
        easeTypePanel.SetType(trigger.EasingType);
    }

	public void SubscribePanelChanges()
    {
        if (_currentTrigger == null) return;
        GD.Print("ПОДПИСКА НА СОБЫТИЯ ПАНЕЛИ ЗУМА");

        // Сначала отписываемся (на всякий случай), чтобы не было двойной подписки
        UnsubscribePanelChanges();

        // Подписываемся на события
        cameraZoomLineEdit.DataChanged += OnCamZoomChanged;
        easeTypePanel.OnEaseTypeSelectedAction += OnEaseTypeSelected;
    }

    public void UnsubscribePanelChanges()
    {
        cameraZoomLineEdit.DataChanged += OnCamZoomChanged;
        easeTypePanel.OnEaseTypeSelectedAction -= OnEaseTypeSelected;
    }

	private void OnCamZoomChanged(Variant newData) // Предполагаю double, судя по (float)newData
    {
        GD.Print("ЗУМ ИЗМЕНЕН");
        if (_currentTrigger != null)
        {
            _currentTrigger.CameraZoom = (float)newData;
            OnPropertyChanged(nameof(cameraZoomLineEdit));
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
