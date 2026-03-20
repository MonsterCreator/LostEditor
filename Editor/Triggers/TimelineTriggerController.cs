 using Godot;
using System.Linq;
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
        
        // ПОДПИСКА НА ЗАПРОС СМЕНЫ ТИПА
        triggersPanelManager.OnTriggerTypeChangeRequested += OnTypeChangeRequestedFromUI;
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

        block.Setup(trigger, timelineController);

        trigger.startTime = timelineController.timelineTime;
        trigger.endTime = 10f; // Длительность
        trigger.CameraPositionX = 500;
        trigger.CameraPositionY = 500;
        trigger.IsAdditive = false;
        trigger.triggerType = TriggerType.CameraPosition;
        trigger.EasingType = EasingType.Linear;

        // Устанавливаем дефолтное время там, где сейчас курсор таймлайна
        trigger.startTime = timelineController.timelineTime;
        trigger.endTime = 5f; // Дефолтная длительность 5 сек

        triggerManager.RegisterTrigger(trigger);

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

        // 1. Снимаем выделение и отписываемся от ввода
        if (selectionManager != null) selectionManager.UnsubscribeFromBlock(block);
        
        // 2. Блок сам должен отписаться от событий (PPS и т.д.)
        // Если ты сделал Unsubscribe(timelineController), вызывай его
        block.Unsubscribe(timelineController);

        // 3. Удаляем визуальный блок из списка
        triggerBlocks.Remove(block);

        // 4. ГЛАВНОЕ: Удаляем данные через менеджер с инвалидацией кеша
        Trigger data = block.GetTriggerData();
        if (data != null)
        {
            triggerManager.UnregisterTrigger(data); 
        }

        // 5. Удаляем узел из сцены
        block.QueueFree();
    }

    public void ChangeTriggerType(TriggerBlock block, TriggerType newType)
    {
        Trigger oldData = block.GetTriggerData();
        
        // Защита от дурака: если тип тот же, ничего не делаем
        if (oldData.triggerType == newType) return;

        GD.Print($"[Controller] Changing trigger type from {oldData.triggerType} to {newType}");

        // 1. Создаем новый экземпляр данных нужного типа
        Trigger newData = triggerManager.CreateTriggerInstanceByType(newType);

        // 2. Копируем общие параметры (Mapping)
        newData.startTime = oldData.startTime;
        newData.endTime = oldData.endTime;
        newData.IsAdditive = oldData.IsAdditive;
        newData.EasingType = oldData.EasingType;
        newData.triggerType = newType; // Важно установить тип явно

        // 3. Работаем с TriggerManager: удаляем старый, регистрируем новый
        triggerManager.UnregisterTrigger(oldData);
        triggerManager.RegisterTrigger(newData);

        // 4. Обновляем Блок (Visual)
        block.SwapTriggerData(newData);

        // 5. Если этот блок сейчас открыт в панели свойств -> нужно перезагрузить панель!
        // Проверяем через SelectionManager или просто принудительно обновляем, если это текущий выбранный
        if (selectionManager.SelectedBlocks.Contains(block))
        {
            triggersPanelManager.LoadTriggerData(newData);
        }
    }

    private void OnTypeChangeRequestedFromUI(Trigger triggerData, TriggerType newType)
    {
        // Нам нужно найти Блок, который держит этот triggerData
        // Это не очень эффективно перебором, но надежно
        var block = triggerBlocks.FirstOrDefault(b => b.GetTriggerData() == triggerData);
        
        if (block != null)
        {
            ChangeTriggerType(block, newType);
        }
        else
        {
            GD.PrintErr("Не найден блок для изменяемого триггера!");
        }
    }

} 