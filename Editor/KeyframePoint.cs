using Godot;
using LostEditor;
using System;
using System.Collections.Generic;

public partial class KeyframePoint : Control
{
	public IKeyframe KeyframeData;

	public event Action<IKeyframe> OnKeyframeTimeChanged;
	public event Action<IKeyframe> OnKeyframeDataChanged;
	public event Action<KeyframePoint, InputEvent> OnInputEvent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		KeyframeData.OnDataChanged += KeyframeDataChanged;
		KeyframeData.OnTimeChanged += KeyframeTimeChanged;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	private void UpdateVisual(float pps)
	{
		
	}

	public void _OnGuiInput(InputEvent @event)
	{
		OnInputEvent?.Invoke(this, @event);
	}

	public void SetSelected(bool selected)
    {
        Modulate = selected ? Colors.Yellow : Colors.White; // Пример подсветки
    }

	private void KeyframeDataChanged()
	{
		OnKeyframeDataChanged?.Invoke(KeyframeData);
	}

	private void KeyframeTimeChanged()
	{
		OnKeyframeTimeChanged?.Invoke(KeyframeData);
	}




	
}
