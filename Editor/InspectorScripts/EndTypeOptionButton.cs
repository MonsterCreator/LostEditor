using Godot;
using System;

public partial class EndTypeOptionButton : OptionButton
{
	// Called when the node enters the scene tree for the first time.
	[Export] LineEdit offsetTimeTextEdit;
	public void ItemSelected(int index)
	{
		if(index == 3) ShowTextEdit();
		else HideTextEdit();
	}

	private void HideTextEdit() {offsetTimeTextEdit.Visible = false;}
	
	private void ShowTextEdit() {offsetTimeTextEdit.Visible = true;}

}
