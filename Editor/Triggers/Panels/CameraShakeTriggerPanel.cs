using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices; 

namespace LostEditor;
public partial class CameraShakeTriggerPanel : Control, ITriggerPanel, INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
	private TriggerCameraPosition _currentTrigger;

	protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

	public void LoadDataToPanel(Trigger abstarctTrigger)
	{
		
	}

	public void SubscribePanelChanges()
	{
		
	}

	public void UnsubscribePanelChanges()
	{
		
	}
}
