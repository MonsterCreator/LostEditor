using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

public partial class SelectionManager : Node
{
	[Export] public Editor editor;
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
        }
        else if (!SelectedBlocks.Contains(block))
        {
            block.IsSelected = true;
            SelectedBlocks.Add(block);
        }
    }

	public void SelectBlock(TimelineBlock block)
	{
		if (SelectedBlocks != null) DeselectAll();
		SelectedBlocks.Add(block);
	}

    public void DeselectAll()
    {
		operationIndex++;
		editor.debugEditorManager.OverrideText(3,"пытаюсь сбросить выделение");
        foreach (var block in SelectedBlocks)
		{
			block.DeselectBlock();
		}
        SelectedBlocks.Clear();
    }
}
