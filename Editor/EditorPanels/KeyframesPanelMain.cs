using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LostEditor;
public partial class KeyframesPanelMain : Control
{
	[Export] public AnimationPanel animationPanel;
	[Export] TimelineController timelineController;
	[Export] InspectorPanel inspector;
	[Export] Control RawsPanel;
	[Export] Control RawsDarkPanel;
	[Export] public TimelineSlider slider;
	[Export] public Control[] keyframeLines;
	[Export] public PackedScene keyframePoint;
	[Export] public LevelColorData levelColorData;

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

	public GameObject GetCurrentObject()
	{
		return _currentObj;
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
	public void LoadKeyframesToPanel(GameObject obj) {
		// 1. Очищаем старые ноды!
		foreach (var line in keyframeLines) {
			foreach (Node child in line.GetChildren()) {
				child.QueueFree();
			}
		}
		keyframePointsX.Clear();
		keyframePointsY.Clear();
		keyframeScaleX.Clear();
		keyframeScaleY.Clear();
		keyframeRotation.Clear();
		keyframeColor.Clear();

		// 2. Грузим новые
		LoadPosXKeyframes(obj.keyframePositionX);
		LoadPosYKeyframes(obj.keyframePositionY);
		LoadScaXKeyframes(obj.keyframeScaleX);
		LoadScaYKeyframes(obj.keyframeScaleY);
		LoadRotKeyframes(obj.keyframeRotation);
		LoadColKeyframes(obj.keyframeColor);

	
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

	private void LoadPosYKeyframes(List<Keyframe<float>> keyframePosYes) 
	{
		foreach (Keyframe<float> keyframe in keyframePosYes) {
			var keyframeP = keyframePoint.Instantiate<KeyframePoint>();
			keyframeP.KeyframeData = keyframe;
			keyframeP.OnInputEvent += timelineKeyframeControl.HandleKeyframeInput;
			keyframeP.OnKeyframeDataChanged += UpdateKeyframeData;
			keyframeP.OnKeyframeTimeChanged += UpdateKeyframeTime;
			keyframeP.Position = new Vector2(keyframe.Time * PixelsPerSecond, 0f);
			
			keyframeLines[1].AddChild(keyframeP); // Индекс 0 для X
			keyframePointsY.Add(keyframeP);       // Список X
		}
	}

	private void LoadScaXKeyframes(List<Keyframe<float>> keyframeScaXes) 
	{
		foreach (Keyframe<float> keyframe in keyframeScaXes) {
			var keyframeP = keyframePoint.Instantiate<KeyframePoint>();
			keyframeP.KeyframeData = keyframe;
			keyframeP.OnInputEvent += timelineKeyframeControl.HandleKeyframeInput;
			keyframeP.OnKeyframeDataChanged += UpdateKeyframeData;
			keyframeP.OnKeyframeTimeChanged += UpdateKeyframeTime;
			keyframeP.Position = new Vector2(keyframe.Time * PixelsPerSecond, 0f);
			
			keyframeLines[2].AddChild(keyframeP); // Индекс 0 для X
			keyframePointsY.Add(keyframeP);       // Список X
		}
	}

	private void LoadScaYKeyframes(List<Keyframe<float>> keyframeScaYes) 
	{
		foreach (Keyframe<float> keyframe in keyframeScaYes) 
		{
			var keyframeP = keyframePoint.Instantiate<KeyframePoint>();
			keyframeP.KeyframeData = keyframe;
			keyframeP.OnInputEvent += timelineKeyframeControl.HandleKeyframeInput;
			keyframeP.OnKeyframeDataChanged += UpdateKeyframeData;
			keyframeP.OnKeyframeTimeChanged += UpdateKeyframeTime;
			keyframeP.Position = new Vector2(keyframe.Time * PixelsPerSecond, 0f);
			
			keyframeLines[3].AddChild(keyframeP); // Индекс 0 для X
			keyframePointsY.Add(keyframeP);       // Список X
		}
	}

	private void LoadRotKeyframes(List<Keyframe<float>> keyframeRot) 
	{
		foreach (Keyframe<float> keyframe in keyframeRot) 
		{
			var keyframeP = keyframePoint.Instantiate<KeyframePoint>();
			keyframeP.KeyframeData = keyframe;
			keyframeP.OnInputEvent += timelineKeyframeControl.HandleKeyframeInput;
			keyframeP.OnKeyframeDataChanged += UpdateKeyframeData;
			keyframeP.OnKeyframeTimeChanged += UpdateKeyframeTime;
			keyframeP.Position = new Vector2(keyframe.Time * PixelsPerSecond, 0f);
			
			keyframeLines[4].AddChild(keyframeP); // Индекс 0 для X
			keyframePointsY.Add(keyframeP);       // Список X
		}
	}

	private void LoadColKeyframes(List<Keyframe<ObjectColor>> keyframeCol) 
	{
		if (keyframeCol == null) return;

		foreach (Keyframe<ObjectColor> keyframe in keyframeCol) 
		{
			var keyframeP = keyframePoint.Instantiate<KeyframePoint>();
			keyframeP.KeyframeData = keyframe;
			keyframeP.OnInputEvent += timelineKeyframeControl.HandleKeyframeInput;
			keyframeP.OnKeyframeDataChanged += UpdateKeyframeData;
			keyframeP.OnKeyframeTimeChanged += UpdateKeyframeTime;
			keyframeP.Position = new Vector2(keyframe.Time * PixelsPerSecond, 0f);

			keyframeLines[5].AddChild(keyframeP); // строка для Color
			keyframeColor.Add(keyframeP);         // правильный список для цветовых точек
		}
	}





	private void UpdateSliderPosition(float newTime)
	{
		if (_currentObj == null) return;

		float localTime = newTime - _currentObj.startTime;
		slider.Value = localTime;
		
		// Линия двигается по той же логике что и на главном таймлайне
		if (slider.line != null)
			slider.line.Position = new Vector2(localTime * PixelsPerSecond, 0);
	}

	public void ApplyZoom(float factor, ScrollContainer horScroll)
	{
		float mouseScreenX = horScroll.GetLocalMousePosition().X;
		float mouseContentX = mouseScreenX + horScroll.ScrollHorizontal;
		float timeAtMouse = mouseContentX / PixelsPerSecond;

		PixelsPerSecond = Mathf.Clamp(PixelsPerSecond * factor, 10f, 2000f);
		timelineKeyframeControl.SetPixelsPerSecond(PixelsPerSecond);

		if (_currentObj != null) UpdatePanelWidth(_currentObj);
		RepositionAllKeyframePoints();

		// Обновляем линию слайдера под новый PPS
		if (slider?.line != null)
		{
			float localTime = (float)slider.Value;
			slider.line.Position = new Vector2(localTime * PixelsPerSecond, 0);
		}

		float newScrollX = timeAtMouse * PixelsPerSecond - mouseScreenX;
		horScroll.ScrollHorizontal = (int)Mathf.Max(0f, newScrollX);
	}

	private void RepositionAllKeyframePoints()
	{
		foreach (var line in keyframeLines)
		{
			foreach (Node child in line.GetChildren())
			{
				if (child is KeyframePoint point)
					point.Position = new Vector2(point.KeyframeData.Time * PixelsPerSecond, point.Position.Y);
			}
		}
	}

	public List<KeyframePoint> GetAllKeyframePoints()
	{
		var allPoints = new List<KeyframePoint>();
		allPoints.AddRange(keyframePointsX);
		allPoints.AddRange(keyframePointsY);
		allPoints.AddRange(keyframeScaleX); 
		allPoints.AddRange(keyframeScaleY); 
		allPoints.AddRange(keyframeRotation); 
		allPoints.AddRange(keyframeColor); 
		// ... добавьте сюда остальные списки (ScaleY, Rotation, Color и т.д.), если они используются
		return allPoints;
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
		GD.Print($"Пытаюсь выбрать панель {keyframeData.kType.ToString()}");
		animationPanel.KeyframeDataTabContainer.CurrentTab = tabIndex + 1;
	
		if (keyframeData.kType != KeyframeType.Color)
		{
			GetPanel(tabIndex).LoadData(keyframeData);
		}
		else
		{
			var colorsList = levelColorData.Colors.Values
				.Select(lc => {
					var oc = new ObjectColor();
					oc.ColorId = lc.Id;          // ИСПРАВЛЕНО: реальный ID, не индекс
					oc.SetBaseLevelColor(lc);
					return oc;
				})
				.ToList();
	
			var colorKeyframe = keyframeData as Keyframe<ObjectColor>;
	
			// Восстанавливаем baseLevelColor у ObjectColor внутри кейфрейма,
			// если он был потерян (например после LoadKeyframesToPanel)
			if (colorKeyframe?.Value != null)
			{
				var levelColor = levelColorData.GetColor(colorKeyframe.Value.ColorId);
				if (levelColor != null)
					colorKeyframe.Value.SetBaseLevelColor(levelColor);
			}
	
			animationPanel.colorPanel.LoadForKeyframe(colorKeyframe, colorsList);
		}
	
		UpdateKeyframeTime(keyframeData);
	}

	public void UpdateKeyframeTime(IKeyframe keyframeData)
	{
		animationPanel.keyframeBasePanel.KeyframeTimeInput.Text = keyframeData.Time.ToString();
	}
	public int GetKeyframeTypeIndex(KeyframeType keyframeType)
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

	public KeyframeDataPanel GetPanel(int panelId)
	{
		switch (panelId)
		{
			case 0: return animationPanel.panels[0];  // PositionX
			case 1: return animationPanel.panels[1];  // PositionY
			case 2: return animationPanel.panels[2];  // ScaleX
			case 3: return animationPanel.panels[3];  // ScaleY
			case 4: return animationPanel.panels[4];  // Rotation
			default: return animationPanel.panels[animationPanel.panels.Length - 1];
		}
	}


	

}
