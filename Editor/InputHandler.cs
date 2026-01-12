using Godot;
using System;

namespace LostEditor;
public partial class InputHandler : Node
{
	//[Export] public Editor editor;


	[Export] public Viewport viewportPanel;
	[Export] public Control timeLinePanel;
	[Export] public Control inspectorPanel;

	private bool _isDraggingTimelineObject = false;
	private bool _isLeftMouseKeyDown = false;
	private bool _isRightMouseKeyDown = false;


    private enum PanelType { None, Viewport, Timeline, Inspector }
	private PanelType _activePanel = PanelType.None;

	public override void _Ready()
	{
		// Подключаем сигналы программно или в редакторе
		timeLinePanel.MouseEntered += () => _activePanel = PanelType.Timeline;
		timeLinePanel.MouseExited += () => _activePanel = PanelType.None;
		// ... и так для остальных
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			
		}
	}






}
