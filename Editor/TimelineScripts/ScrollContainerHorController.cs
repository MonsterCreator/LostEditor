using Godot;
using System;

public partial class ScrollContainerHorController : ScrollContainer
{
	[Export] public float ScrollSpeed = 30f;

	public bool isMouseHover = false;
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

	private void MouseEntered()
	{
		isMouseHover = true;
		GD.Print("MouseEntered");
	}

	private void MouseExited()
	{
		isMouseHover = false;
		GD.Print("MouseExited");
	}
}
