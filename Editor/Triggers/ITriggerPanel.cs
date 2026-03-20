using Godot;
using LostEditor;
using System;

public interface ITriggerPanel
{
	public void LoadDataToPanel(Trigger abstarctTrigger);
	public void SubscribePanelChanges();
	public void UnsubscribePanelChanges();
}
