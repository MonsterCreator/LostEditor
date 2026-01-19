using Godot;
using LostEditor;
using System;

public partial class KeyframeBasePanel : HBoxContainer
{
	[Export] public TimelineKeyframeControlSystem timelineKeyframeControl;
	[Export] public LineEdit KeyframeTimeInput;
	[Export] public LineEdit KeyframeIndex;
	// Called when the node enters the scene tree for the first time.
	
	public Action<float> OnKeyframeTimeWasChanged;
	private void OnKeyframeTimeChanged(string text)
	{
		OnKeyframeTimeWasChanged?.Invoke(text.ToFloat());
	}
}
