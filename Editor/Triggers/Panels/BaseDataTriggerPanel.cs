using Godot;
using LostEditor;
using System;

public partial class BaseDataTriggerPanel : Control
{
	// Called when the node enters the scene tree for the first time.
	[Export] public InputLineEdit startTimeLineEdit;
	[Export] public InputLineEdit transitionTimeLineEdit;
	[Export] public OptionButton triggerType;
	[Export] public CheckButton isAdditiveCheckButton;

	
}


