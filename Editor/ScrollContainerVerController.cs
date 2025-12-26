using Godot;
using System;

public partial class ScrollContainerVerController : ScrollContainer
{
	
	[Export] public float ScrollSpeed = 30f;
	[Export] public ScrollContainerHorController _parentScroll;
	[Export] public Editor _editor;


	public override void _Ready()
	{
		// Находим родительский горизонтальный контейнер
		FollowFocus = false;
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.WheelUp || 
			    mouseEvent.ButtonIndex == MouseButton.WheelDown)
			{
				float direction = mouseEvent.ButtonIndex == MouseButton.WheelUp ? -1 : 1;

				// 1. ЗУМ (Только если зажат Shift)
				if (Input.IsKeyPressed(Key.Shift))
				{
					float factor = mouseEvent.ButtonIndex == MouseButton.WheelUp ? 1.1f : 0.9f;
					_editor?.ApplyZoom(factor);
					AcceptEvent();
					return;
				}
            
				// 2. ВЕРТИКАЛЬНЫЙ СКРОЛЛ (Только если зажат Ctrl)
				else if (Input.IsKeyPressed(Key.Ctrl))
				{
					// Прокручиваем текущий вертикальный контейнер
					ScrollVertical += (int)(direction * ScrollSpeed);
					AcceptEvent();
					return;
				}
            
				// 3. ГОРИЗОНТАЛЬНЫЙ СКРОЛЛ (Если модификаторы НЕ зажаты)
				else
				{
					// Передаем команду родительскому горизонтальному скроллу
					_parentScroll?.ScrollManually(direction);
					AcceptEvent();
				}
			}
		}
	}
}
