using Godot;
using System;
using LostEditor;

public partial class FpsCounter : Control
{
	// Переменная для хранения значения FPS
	[Export] public Label fpsLabel;

	// Вызывается каждый кадр
	public override void _Process(double delta)
	{
		// Получаем текущую частоту кадров
		fpsLabel.Text = Engine.GetFramesPerSecond().ToString();

	}
}
