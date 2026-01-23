using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;
/*
    TriggerManager - "элемент ядра" системы управления эффектами, цветами и прочими вещами. 
	TriggerManager управляет параметрами сцены (цвета, управление камерой, визуальные эффекты и т.д.)
*/
public partial class TriggerManager : Node
{
	public List<Trigger> triggers = new List<Trigger>();
	public void registerTrigger(Trigger trigger)
    {
        if(!triggers.Contains(trigger))
        {
            triggers.Add(trigger);
        }
    }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
