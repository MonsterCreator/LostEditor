using Godot;
using System.Collections.Generic;
using System;


using System.Linq;


namespace LostEditor;
public partial class Editor : Node2D
{
    /*
    #region Exports
    [ExportGroup("Components")]
    [Export] public ObjectManager objectManager;
    [Export] public DebugEditorManager debugEditorManager;
    [Export] public TimelineController timelineController;
    [Export] public TimelineObjectController timeLineObjectControl;
    [Export] public ScrollContainer HorScroll;

    [Export] public Node ViewportObj;
    [Export] public InspectorPanel Inspector;
    [Export] public Editor EditorRef;
    [Export] public TimelineSlider timelineSlider;
    [Export] public SelectionManager selection;
    [Export] public ScrollContainerHorController scrollContainerHor;
    
    [ExportGroup("UI Links")]
    [Export] public TextEdit GlobalTimeTextEdit;
    [Export] public VBoxContainer TimelineContainer;


    [ExportGroup("Prefabs")]
    [Export] public PackedScene GameObjectScene;
    [Export] public PackedScene TimelineBlockScene;

    [ExportGroup("Settings")]
    
    #endregion
    */
    #region Private Fields
    
    
    
    private bool _isDragging = false;
    private bool _hasMoved = false;
    private float _mouseYAccumulator = 0f;
    private TimelineBlock _lastClickedBlock = null;
    
    
    
    private const float RowThreshold = 30f;
    #endregion
    
    public ObjectController Controller;
    
    
    

    public override void _PhysicsProcess(double delta)
    {

        
    }


    

}