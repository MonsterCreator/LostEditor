 using Godot;

using System;


namespace LostEditor;

public partial class CameraPositionTriggerPanel : Control

{

    [Export] public InputLineEdit camPosXLineEdit;

    [Export] public CheckButton camPosXCheckButton;

    [Export] public InputLineEdit camPosYLineEdit;

    [Export] public CheckButton camPosYCheckButton;

    [Export] public EaseTypePanel easeTypePanel;


    public void _Ready()

    {

       

    }


    public void OnCameraPositionXValueChanged()

    {

       

    }


    public void OnCameraPositionYValueChanged()

    {

       

    }


    public void OnEaseTypeSelected(int index)

    {

       

    }



    public void LoadData(TriggerCameraPosition trigger)

    {

        camPosXCheckButton.ButtonPressed = trigger.IsCameraPositionXActive;

        camPosXLineEdit.SetValueWithoutNotify(trigger.CameraPositionX);

        camPosYCheckButton.ButtonPressed = trigger.IsCameraPositionYActive;

        camPosYLineEdit.SetValueWithoutNotify(trigger.CameraPositionY);

        easeTypePanel.SetType(trigger.EasingType);

        GD.Print($"ДАННЫЕ ЗАДАНЫ {trigger.CameraPositionX} {trigger.CameraPositionY}");

    }

} 