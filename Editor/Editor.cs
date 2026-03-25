using Godot;
using System.Collections.Generic;
using System;


using System.Linq;


namespace LostEditor;
public partial class Editor : Control
{
    #region Private Fields

    private bool _isDragging = false;
    private bool _hasMoved = false;
    private float _mouseYAccumulator = 0f;
    private TimelineBlock _lastClickedBlock = null;

    #endregion
    
    [Export] public SubViewportContainer subviewportContainer;
    [Export] public SubViewport subviewport;
    [Export] public Control ViewportPlace;
    [Export] public CanvasLayer editorCanvas;
    [Export] public DebugWindow debugWindow;

    public ObjectController Controller;
    private bool _isPreviewMode = false;


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey ek && ek.Pressed && ek.Keycode == Key.F10)
        {
            if(_isPreviewMode)
            {
                GD.Print("Переключаемся на редактор камеру");
                subviewportContainer.Reparent(ViewportPlace);
                subviewport.Size = new Vector2I(Convert.ToInt32(1920/1.8),Convert.ToInt32(1080/1.8));
                editorCanvas.Visible = true;
                _isPreviewMode = !_isPreviewMode;
            }
            else
            {
                GD.Print("Переключаемся на вьюпорт камеру");
                subviewportContainer.Reparent(this);
                subviewportContainer.Position = new Vector2(0,0);
                subviewport.Size = new Vector2I(1920,1080);
                editorCanvas.Visible = false;
                _isPreviewMode = !_isPreviewMode;
            }
        }
        else if (@event is InputEventKey ek2 && ek2.Pressed && ek2.Keycode == Key.F9)
        debugWindow.Toggle();
    }

}