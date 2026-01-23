 using Godot;

using System;

using System.Collections.Generic;


namespace LostEditor;

/*

    TimelineTriggerController - система управления TriggerBlock на таймлайне

    TimelineTriggerController управляет триггер блоками (создание, удаление, (копирование(не готово)))

*/

public partial class TimelineTriggerController : Node

{

    [Export] public TimelineController timelineController;

    [Export] public TimelineTriggerSelectionManager triggerSelectionManager;

    [Export] public TriggersPanelManager triggersPanelManager;

    [Export] public Control[] Rows;

    [Export] public WorkPanel workPanel;

    [Export] public DebugEditorManager debugEditorManager;

    [Export] public PackedScene triggerBlockScene;

    [Export] public TriggerManager triggerManager;

   

    // Ссылка на SelectionManager (нужно назначить в инспекторе)

    [Export] public TimelineTriggerSelectionManager selectionManager;


    public List<TriggerBlock> triggerBlocks = new List<TriggerBlock>();


    public override void _Ready()

    {

        triggerSelectionManager.OnNewBlockSelected += LoadTriggerData;

    }


    private void LoadTriggerData(TriggerBlock block)

    {

        workPanel.OpenPanel(WorkPanelType.TriggerPanel);

        triggersPanelManager.LoadTriggerData(block.GetTriggerData());

    }


    public void OnCreateTriggerButtonPressed()

    {

        CreateTrigger();

    }


    public void CreateTrigger()

    {

        // Создаем данные

        var trigger = new TriggerCameraPosition();

        var block = triggerBlockScene.Instantiate<TriggerBlock>();

        block.Setup(trigger, ref timelineController.PixelsPerSecond);


        trigger.CameraPositionX = 56;

        trigger.CameraPositionY = -56;

       

        // Устанавливаем дефолтное время там, где сейчас курсор таймлайна

        trigger.startTime = timelineController.timelineTime;

        trigger.endTime = trigger.startTime + 2.0f; // Дефолтная длительность 2 сек

       

        triggerManager.registerTrigger(trigger);


        // Создаем визуал

       


        // Добавляем на сцену

        if (Rows.Length > 0)

            Rows[0].AddChild(block);

        else

            GD.PrintErr("Нет строк для добавления триггера!");


        triggerBlocks.Add(block);


        // ВАЖНО: Подключаем менеджер выделения

        if (selectionManager != null)

        {

            selectionManager.SubscribeToBlock(block);

            // Сразу выделяем новый блок для удобства

            selectionManager.Select(block, false);

        }


        GD.Print("Триггер блок создан и подключен к системе выделения");

        GD.Print($"trigger.startTime {trigger.startTime} trigger.endTime {trigger.endTime + 2}");

    }


    // Метод для удаления, вызываемый из SelectionManager

    public void DeleteTriggerBlock(TriggerBlock block)

    {

        if (block == null) return;


        // 1. Отписываемся от событий ввода

        if (selectionManager != null) selectionManager.UnsubscribeFromBlock(block);


        // 2. Удаляем из списка контроллера

        if (triggerBlocks.Contains(block)) triggerBlocks.Remove(block);


        // 3. Удаляем данные (Trigger)

        // Нам нужно получить Trigger из Block.

        // Добавьте метод GetTriggerData() в TriggerBlock, как описано ниже.

        Trigger data = block.GetTriggerData();

        if (triggerManager.triggers.Contains(data))

        {

            triggerManager.triggers.Remove(data);

        }


        // 4. Удаляем визуальный узел

        block.QueueFree();

    }

} 