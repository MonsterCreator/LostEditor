using Godot;
using System;

public partial class ScrollContainerHorController : ScrollContainer
{
	[Export] public float ScrollSpeed = 30f;

	public bool isMouseHover = false;
	public override void _Ready()
	{
		FollowFocus = false;
		// Подключаем программно — не зависим от сигналов в сцене
		MouseEntered += OnMouseEnterd;
		MouseExited += OnMouseExited;
	}

	// Метод для ручного управления прокруткой
	public void ScrollManually(float delta)
	{
		ScrollHorizontal += (int)(delta * ScrollSpeed);
	}

	private void OnMouseEnterd()
	{
		isMouseHover = true;
	}

	private void OnMouseExited()
	{
		isMouseHover = false;
	}
}
