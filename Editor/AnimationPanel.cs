using Godot;
using LostEditor;
using System;

namespace LostEditor;
public partial class AnimationPanel : VBoxContainer
{
	
	[Export] public KeyframesPanelMain keyframesPanel;
	[Export] public KeyframeBasePanel keyframeBasePanel;
	[Export] public TabContainer KeyframeDataTabContainer;
	[Export] public TimelineKeyframeControlSystem timelineKeyframeControl;
	[Export] public PositionXPanel posXPanel;

    public override void _Ready()
    {
        if(keyframeBasePanel != null) keyframeBasePanel.OnKeyframeTimeWasChanged += UpdateObjectTimeData;
    }

	private void UpdateObjectTimeData(float time)
	{
		KeyframePoint keyframePoint = timelineKeyframeControl.GetSelectedKeyframe();
		keyframePoint.KeyframeData.Time = time;
	}


}

public interface IDataPanel
{
	TimelineKeyframeControlSystem timelineKeyframeControl {get; set;}
	LineEdit ValueLineEdit {get; set;}
	LineEdit ValueRandomLineEdit {get; set;}
	LineEdit ValueRandomStepLineEdit {get; set;}
	CheckBox RelativeModeCkeckBox {get; set;}
	OptionButton KeyframeRandomType {get; set;}

}


