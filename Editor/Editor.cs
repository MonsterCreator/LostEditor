using Godot;
using System.Collections.Generic;
using System;


using System.Linq;


namespace LostEditor;
public partial class Editor : Node2D
{
    #region Exports
    [ExportGroup("Components")]
    [Export] public ObjectManager objectManager;
    [Export] public DebugEditorManager debugEditorManager;
    [Export] public TimelineController timelineController;
    [Export] public TimelineObjectController timeLineObjectControl;
    [Export] public ScrollContainer HorScroll;
    [Export] public Control[] Rows;
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

    #region Private Fields
    
    
    
    private bool _isDragging = false;
    private bool _hasMoved = false;
    private float _mouseYAccumulator = 0f;
    private TimelineBlock _lastClickedBlock = null;
    
    
    
    private const float RowThreshold = 30f;
    #endregion
    
    public ObjectController Controller;
    
    public override void _Ready() => timelineController.UpdateTimelineSize();
    

    public override void _PhysicsProcess(double delta)
    {
        
    }


    #region Core Logic
    public void CreateObjectButtonPressed()
    {
        timeLineObjectControl.CreateObject();
    }



    /*
    public void HandleBlockSelection(TimelineBlock block, bool isCtrl)
    {
        _isDragging = true;
        _hasMoved = false;
        _lastClickedBlock = block;
        selection.HandleSelection(block, isCtrl);
    
        // Передаем первый выделенный объект в инспектор
        if (selection.SelectedBlocks.Count > 0)
        {
            Inspector.Inspect(selection.SelectedBlocks[0]);
        }
        else
        {
            Inspector.Inspect(null);
        }

        UpdateAllBlocks();
    }
    





    private void FinalizeClick()
    {
        if (!_hasMoved && _lastClickedBlock != null && !Input.IsKeyPressed(Key.Ctrl))
        {
            selection.DeselectAll();
            selection.HandleSelection(_lastClickedBlock, false);
            UpdateAllBlocks();
        }
        _isDragging = false;
        _lastClickedBlock = null;
    }
    */
    #endregion

    #region Infrastructure
    
    
    
    
    

    private void MoveGroup(int dir)
    {
        bool canMove = true;
        // 1. Проверяем, может ли вся группа сдвинуться
        foreach (var block in selection.SelectedBlocks)
        {
            int currentIndex = Array.IndexOf(Rows, block.GetParent());
            int targetIndex = currentIndex + dir;
            if (targetIndex < 0 || targetIndex >= Rows.Length) { canMove = false; break; }
        }

        if (canMove)
        {
            foreach (var block in selection.SelectedBlocks)
            {
                // Получаем текущую строку
                Node currentRow = block.GetParent();
                int currentIndex = Array.IndexOf(Rows, currentRow);
            
                // Убираем из старой и добавляем в новую
                currentRow.RemoveChild(block);
                Rows[currentIndex + dir].AddChild(block);
            
                // Важный хак для Godot: при смене родителя нужно дождаться кадра 
                // или принудительно обновить положение, иначе блок может "улететь" в 0,0
                block.UpdateVisual(timelineController.PixelsPerSecond);
            }
        }
    }

    public void DeleteBlock(TimelineBlock block)
    {
        timeLineObjectControl.activeBlocks.Remove(block);
        selection.SelectedBlocks.Remove(block);
        objectManager.objects.Remove(block.Data);
        block.Data.QueueFree();
        block.QueueFree();
    }
    
    #endregion





    public static class TimeUtils
    {
        public static string SecondsToMinutesString(float seconds)
        {
            int totalSeconds = (int)Mathf.Floor(seconds);
            int minutes = totalSeconds / 60;
            int secs = totalSeconds % 60;
            string ms = (seconds - totalSeconds).ToString("F3").Substring(2);
            return $"{minutes}:{secs:D2}.{ms}";
        }

        public static float ClampTime(float time, float duration, float maxTime)
        {
            return Mathf.Clamp(time, 0, maxTime - duration);
        }
    }

}