using Godot;
using System;

public partial class GlobalInput : Node
{
    public override void _Input(InputEvent @event)
    {
        // Проверяем нажатие F11
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Pressed && eventKey.Keycode == Key.F11)
            {
                ToggleFullscreen();
            }
        }
    }

    private void ToggleFullscreen()
    {
		GD.Print("пытаемя преключить Fullscreen");
        // Получаем текущий режим окна
        var currentMode = DisplayServer.WindowGetMode();

        if (currentMode == DisplayServer.WindowMode.Fullscreen || 
            currentMode == DisplayServer.WindowMode.ExclusiveFullscreen)
        {
            // Если во весь экран — переключаем в оконный
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			GD.Print("Fullscreen OFF");
        }
        else
        {
            // Если в окне — переключаем в эксклюзивный полноэкранный
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
			GD.Print("Fullscreen ON");
        }
    }
}