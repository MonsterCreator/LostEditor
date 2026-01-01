using Godot;
using System;
using LostEditor;

public partial class ScrollContainerHorController : ScrollContainer
{
	[Export] public float ScrollSpeed = 30f;

	public override void _Ready()
	{
		// Отключаем стандартную обработку фокуса, чтобы он не прыгал
		FollowFocus = false;
	}

	// Метод для ручного управления прокруткой
	public void ScrollManually(float delta)
	{
		ScrollHorizontal += (int)(delta * ScrollSpeed);
	}
}
