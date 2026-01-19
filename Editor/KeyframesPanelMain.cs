using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;
public partial class KeyframesPanelMain : Control
{
	[Export] AnimationPanel animationPanel;
	[Export] TimelineController timelineController;
	[Export] InspectorPanel inspector;
	[Export] Control RawsPanel;
	[Export] Control RawsDarkPanel;
	[Export] HSlider slider;
	[Export] public Control[] keyframeLines;
	[Export] public PackedScene keyframePoint;

	[Export] public TimelineKeyframeControlSystem timelineKeyframeControl;

	private List<KeyframePoint> keyframePointsX = new List<KeyframePoint>();
	private List<KeyframePoint> keyframePointsY = new List<KeyframePoint>();
	private List<KeyframePoint> keyframeScaleX = new List<KeyframePoint>();
	private List<KeyframePoint> keyframeScaleY = new List<KeyframePoint>();
	private List<KeyframePoint> keyframeRotation = new List<KeyframePoint>();
	private List<KeyframePoint> keyframeColor = new List<KeyframePoint>();

	private GameObject _currentObj;

    public override void _Ready()
    {
		inspector.OnDataUpdated += LoadData;
		timelineController.OnSliderTimeChanged += UpdateSliderPosition;
    }

	[Export] public float PixelsPerSecond = 100f;

	private void LoadData(GameObject obj)
	{
		timelineKeyframeControl.DeselectAll(); 

    	_currentObj = obj;

		_currentObj = obj;
		UpdatePanelWidth(obj);
		LoadKeyframesToPanel(obj);
	}
	public void UpdatePanelWidth(GameObject obj)
	{
		float width = obj.cachedEndTime * PixelsPerSecond;
		float widthDark = (obj.cachedEndTime + 5f) * PixelsPerSecond;
		RawsPanel.CustomMinimumSize = new Vector2(width,0);
		
		slider.MaxValue = obj.cachedEndTime + 5f;

		RawsDarkPanel.CustomMinimumSize = new Vector2(widthDark,0);
		timelineKeyframeControl.SetPixelsPerSecond(PixelsPerSecond);
		GD.Print($"Width {RawsPanel.CustomMinimumSize}, widthDark {RawsDarkPanel.CustomMinimumSize}");
		
	}
	private void LoadKeyframesToPanel(GameObject obj) {
		// 1. Очищаем старые ноды!
		foreach (var line in keyframeLines) {
			foreach (Node child in line.GetChildren()) {
				child.QueueFree();
			}
		}
		keyframePointsX.Clear();
		keyframePointsY.Clear();

		// 2. Грузим новые
		LoadPosXKeyframes(obj.keyframePosX);
	
	}

	private void LoadPosXKeyframes(List<Keyframe<float>> keyframePosXes) {
		foreach (Keyframe<float> keyframe in keyframePosXes) {
			var keyframeP = keyframePoint.Instantiate<KeyframePoint>();
			keyframeP.KeyframeData = keyframe;
			keyframeP.OnInputEvent += timelineKeyframeControl.HandleKeyframeInput;
			keyframeP.OnKeyframeDataChanged += UpdateKeyframeData;
			keyframeP.OnKeyframeTimeChanged += UpdateKeyframeTime;
			keyframeP.Position = new Vector2(keyframe.Time * PixelsPerSecond, 0f);
			
			keyframeLines[0].AddChild(keyframeP); // Индекс 0 для X
			keyframePointsX.Add(keyframeP);       // Список X
		}
	}



	private void UpdateSliderPosition(float newTime) //Updatign current slider value for sync with the timeline
	{
		if (_currentObj == null) return;

        float localTime = newTime - _currentObj.startTime;
		slider.Value = localTime;
	}

	private void OnSliderValueChanged(float val)
	{
		if(_currentObj == null) return;
		timelineController.timelineSlider.Value = val + _currentObj.startTime;
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.Pressed)
		{
			timelineKeyframeControl.DeselectAll();
		}
	}

	public void UpdateKeyframeData(IKeyframe keyframeData)
	{
		int tabIndex = GetKeyframeTypeIndex(keyframeData.kType);
		animationPanel.KeyframeDataTabContainer.CurrentTab = tabIndex;
		IDataPanel panelData = GetPanel(tabIndex);
		panelData.ValueLineEdit.Text = keyframeData.Value.ToString();
		UpdateKeyframeTime(keyframeData);
	}

	public void UpdateKeyframeTime(IKeyframe keyframeData)
	{
		animationPanel.keyframeBasePanel.KeyframeTimeInput.Text = keyframeData.Time.ToString();
	}
	private int GetKeyframeTypeIndex(KeyframeType keyframeType)
	{
		switch (keyframeType)
		{
			case KeyframeType.PositionX: return 0;
			case KeyframeType.PositionY: return 1;
			case KeyframeType.ScaleX: return 2;
			case KeyframeType.ScaleY: return 3;
			case KeyframeType.Rotation: return 4;
			case KeyframeType.Color: return 5;
			case KeyframeType.Custom: return 6;
			default: return 6;
		}
	}

	private IDataPanel GetPanel(int pnaelId)
	{
		switch (pnaelId)
		{
			case 1: return animationPanel.posXPanel;

			default: return animationPanel.posXPanel;
		}
	}



	

}
