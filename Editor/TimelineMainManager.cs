using Godot;
using System;

public partial class TimelineMainManager : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	[Export] public TabContainer timelinePanels;
	[Export] public TabContainer topPanels;
	
	public void OnTabSelected(int index)
	{
		if(index == 0)
		{
			timelinePanels.CurrentTab = 0;
			topPanels.CurrentTab = 0;
		}
		else if(index == 1)
		{
			timelinePanels.CurrentTab = 1;
			topPanels.CurrentTab = 1;
		}
	}
}
