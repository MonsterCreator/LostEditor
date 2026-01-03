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



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Inspect(TimelineBlock blocks)
	{
		
	}

	public void ObjectNameTextEditSubmitted()
	{
		
	}
}


