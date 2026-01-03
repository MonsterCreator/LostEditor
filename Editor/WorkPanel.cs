using Godot;
using System;

namespace LostEditor;

[Tool]
public partial class WorkPanel : Control
{

	[Export] public Control emptyPanel;
	[Export] public Control singleObjSettingsPanel;
	[Export] public Control multiObjEditPanel;

	[Export] public int panelNum
	{
		get
		{
			return _panelNum;
		}
		set
		{
			_panelNum = value;
			ChangePanelDisplay();
		}
	}

	private int _panelNum;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		emptyPanel.Visible = true;
		singleObjSettingsPanel.Visible = false;
		multiObjEditPanel.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ChangePanelDisplay()
	{
		if (!IsInsideTree()) return;

		switch (_panelNum)
		{
			case 0:
				emptyPanel.Visible = true;
				singleObjSettingsPanel.Visible = false;
				multiObjEditPanel.Visible = false;
			break;

			case 1:
				emptyPanel.Visible = false;
				singleObjSettingsPanel.Visible = true;
				multiObjEditPanel.Visible = false;
			break;

			case 2:
				emptyPanel.Visible = false;
				singleObjSettingsPanel.Visible = false;
				multiObjEditPanel.Visible = true;
			break;

			default: 
				emptyPanel.Visible = true;
				singleObjSettingsPanel.Visible = false;
				multiObjEditPanel.Visible = false;
			break;
		}
	}
}
