using Godot;
using System;

namespace LostEditor;
public partial class KeyframeDataPanel : Control
{
	[Export] public TimelineKeyframeControlSystem timelineKeyframeControl {get; set;}
	[Export] public LineEdit ValueLineEdit {get; set;}
	[Export] public LineEdit ValueRandomLineEdit {get; set;}
	[Export] public LineEdit ValueRandomStepLineEdit {get; set;}
	[Export] public CheckBox RelativeModeCkeckBox {get; set;}
	[Export] public OptionButton KeyframeRandomType {get; set;}
	[Export] public EaseTypePanel easeTypePanel {get; set;}

	public IKeyframe _keyframe {get; private set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		easeTypePanel.OnEaseTypeSelectedAction += OnEaseTypeChanged;
	}

	public void LoadData(IKeyframe keyframeData)
	{
		_keyframe = keyframeData;
		ValueLineEdit.Text = _keyframe.Value.ToString();
		
	}

	public void OnValueLineEditorTextChanged(string newVal)
	{
		KeyframePoint keyframe = timelineKeyframeControl.GetSelectedKeyframe();
		if(keyframe != null) keyframe.KeyframeData.Value = newVal.ToFloat();
		
	}

	private void OnEaseTypeChanged(int index)
	{
		KeyframePoint keyframe = timelineKeyframeControl.GetSelectedKeyframe();

		if(keyframe != null) keyframe.KeyframeData.EasingType = (EasingType)index;
		GD.Print($"ДАННЫЕ ОБНОВЛЕНЫ НА {(EasingType)index} {keyframe}");
	}
}
	
