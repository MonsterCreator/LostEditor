using Godot;
using System;

namespace LostEditor;
public partial class TCameraManager : Node
{
	[Export] public TriggerManager triggerManager;
	[Export] public TimelineController timelineController;
	[Export] public Camera2D ViewportCamera;

	public void UpdateCameraPosition()
	{

	}
}
