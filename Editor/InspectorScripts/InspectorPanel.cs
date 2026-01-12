using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;
public partial class InspectorPanel : HBoxContainer
{
	[Export] public LineEdit NameInput;
	[Export] public LineEdit StartTimeInput;
	[Export] public LineEdit EndTimeInput;
	[Export] public Button SetStartToCurrentBtn;
	[Export] public Button SetEndToCurrentBtn;
	[Export] public WorkPanel workPanel;

	private TimelineBlock obj;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Inspect(TimelineBlock timelineBlock)
	{
		workPanel.OpenInspectorPanel();
		obj = timelineBlock;
		LoadObjectData(obj.Data);
		
	}

	private void LoadObjectData(GameObject gameObject)
	{
		NameInput.Text = gameObject.Name.ToString();
		StartTimeInput.Text = gameObject.startTime.ToString();
		EndTimeInput.Text = gameObject.endTime.ToString();
	}

	public void ObjectNameTextEditSubmitted(string text)
	{
		if(text != null) obj.Data.name = text;
		LoadObjectData(obj.Data);
	}

	public void ObjectStartTimeTextEditSubmitted(string text)
	{
		float startTime;
		bool canParse = float.TryParse(text, out startTime);
		if(canParse) 
		{
			obj.Data.startTime = startTime;
			LoadObjectData(obj.Data);
		}
		else return;
		
	}
}


