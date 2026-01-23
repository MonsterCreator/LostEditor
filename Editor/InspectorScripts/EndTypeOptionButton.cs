using Godot;
using System;

namespace LostEditor;
public partial class EndTypeOptionButton : OptionButton
{
	// Called when the node enters the scene tree for the first time.
	[Export] LineEdit offsetTimeTextEdit;

	    
    [Signal] public delegate void DataChangedEventHandler(int value);
	public void OnItemSelected(int index)
	{
		if(index == 3) ShowTextEdit();
		else HideTextEdit();
		EmitSignal(SignalName.DataChanged, index);
		GD.Print("Сигнал!!!");
	}

	private void HideTextEdit() {offsetTimeTextEdit.Visible = false;}
	
	private void ShowTextEdit() {offsetTimeTextEdit.Visible = true;}

}
