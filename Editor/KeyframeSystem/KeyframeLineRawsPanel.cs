using Godot;
using System;

namespace LostEditor;
public partial class KeyframeLineRawsPanel : Control
{
	[Export] TimelineKeyframeControlSystem timelineKeyframeControl;
	[Export] KeyframesPanelMain keyframesPanelMain;

	public override void _GuiInput(InputEvent @event)
	{
		// Если клик дошел до панели, значит мы не попали по кейфрейму
		// (так как кейфреймы вызывают GetViewport().SetInputAsHandled())
		HandlePanelInput(@event);
	}
	public void HandlePanelInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
		{
			// Клик по пустому месту сбрасывает всё
			timelineKeyframeControl.DeselectAll();
		}
	}
}
