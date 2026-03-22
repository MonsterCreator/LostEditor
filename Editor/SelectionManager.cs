using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LostEditor;

public partial class SelectionManager : Node
{
    [Export] public DebugEditorManager debugEditorManager;
    [Export] public InspectorPanel inspector;
    public List<TimelineBlock> SelectedBlocks { get; private set; } = new();

    private int operationIndex = 0;

    public void HandleSelection(TimelineBlock block, bool isMultiSelect)
    {
        if (!isMultiSelect && !block.IsSelected)
        {
            DeselectAll();
        }

        if (block.IsSelected && isMultiSelect)
        {
            block.IsSelected = false;
            SelectedBlocks.Remove(block);
            block.UpdateVisual(); // визуал после снятия
        }
        else if (!SelectedBlocks.Contains(block))
        {
            block.IsSelected = true;
            SelectedBlocks.Add(block);
            block.UpdateVisual();
        }
    }

    // clearFirst = true  → стандартное одиночное выделение (сначала сброс)
    // clearFirst = false → добавить к выделению без сброса (для Ctrl-логики и схлопывания)
    public void SelectBlock(TimelineBlock block, bool clearFirst = true)
    {
        if (clearFirst) DeselectAll();

        if (!SelectedBlocks.Contains(block))
        {
            block.IsSelected = true; // было пропущено — отсюда старый костыль IsSelected = true в ButtonPressed
            SelectedBlocks.Add(block);
        }

        inspector.Inspect(block.Data);
    }

    public void DeselectAll()
    {
        operationIndex++;
        debugEditorManager.OverrideText(3, "пытаюсь сбросить выделение");
        foreach (var block in SelectedBlocks)
            block.DeselectBlock();
        SelectedBlocks.Clear();
    }
}