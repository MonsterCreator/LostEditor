using Godot;
using LostEditor;
using System;

namespace LostEditor;
public partial class TriggersPanelManager : Control
{
	[Export] public BaseDataTriggerPanel baseDataTriggerPanel;
	[Export] public TabContainer triggerDataPanels;

	[Export] public CameraPositionTriggerPanel cameraPositionTriggerPanel;

	private Trigger _trigger;

	public void LoadTriggerData(Trigger trigger)
	{
		_trigger = trigger;
		GetTriggerType();
	}

	private void GetTriggerType()
	{
		switch (_trigger.triggerType)
		{
			case TriggerType.CameraPosition: LoadCameraPositionData(_trigger as TriggerCameraPosition); break;
			default: LoadCameraZoomData(); break;
		}

	}

	private void LoadCameraPositionData(TriggerCameraPosition triggerCameraPosition)
	{
		triggerDataPanels.CurrentTab = 0;

		cameraPositionTriggerPanel.LoadData(triggerCameraPosition);
	}

	private void LoadCameraZoomData()
	{
		triggerDataPanels.CurrentTab = 1;
	}
}
