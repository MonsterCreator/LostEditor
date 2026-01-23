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
		// Здесь можно менять иконку в зависимости от EasingType
		UpdateVisuals();
		OnKeyframeDataChanged?.Invoke(KeyframeData);
	}

	private void UpdateVisuals()
	{
		// Пример: если тип плавности не Linear, делаем кейфрейм синим
		if (KeyframeData.EasingType != EasingType.Linear) {
			// Modulate = Colors.SkyBlue; 
		}
	}

	private void KeyframeTimeChanged()
	{
		OnKeyframeTimeChanged?.Invoke(KeyframeData);
	}




	
}
