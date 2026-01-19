using Godot;
using LostEditor;
using System;

namespace LostEditor;
public partial class PositionXPanel : Control, IDataPanel
{
	[Export] public TimelineKeyframeControlSystem timelineKeyframeControl {get; set;}
	[Export] public LineEdit ValueLineEdit {get; set;}
	[Export] public LineEdit ValueRandomLineEdit {get; set;}
	[Export] public LineEdit ValueRandomStepLineEdit {get; set;}
	[Export] public CheckBox RelativeModeCkeckBox {get; set;}
	[Export] public OptionButton KeyframeRandomType {get; set;}

	public IKeyframe _keyframe {get; private set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void LoadData(IKeyframe keyframeData)
	{
		_keyframe = keyframeData;
		ValueLineEdit.Text = _keyframe.Value.ToString();
		
	}

	public void OnPosXLineEditorTextChanged()
	{
		
	}
}
