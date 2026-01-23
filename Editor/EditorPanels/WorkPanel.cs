using Godot;
using System;

namespace LostEditor;


public partial class WorkPanel : TabContainer
{




    public void OpenPanel(WorkPanelType type)
    {
        string typeString = type.ToString();
        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild(i);
            if (child.HasMeta("panel_type") && child.GetMeta("panel_type").AsString() == typeString)
            {
                CurrentTab = i;
                return;
            }
        }
    }



}

public enum WorkPanelType
{
    NoPanel = 0,
    ObjectEdit = 1,
    MultiObjectEdit = 2,
    EditorSettings = 3,
    TriggerPanel = 4,
    
}