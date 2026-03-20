using Godot;
using LostEditor;
using System;
using System.Collections.Generic;

namespace LostEditor;

public partial class TriggersPanelManager : Control
{
    [Export] public BaseDataTriggerPanel baseDataTriggerPanel;
    [Export] public TabContainer triggerDataPanels;
	private ITriggerPanel _currentPanel = null;
    private List<ITriggerPanel> triggerPanels = new();

	public Action<Trigger, TriggerType> OnTriggerTypeChangeRequested;

    private Trigger _currentTrigger;

    public override void _Ready()
    {
        // Инициализация списка панелей из дочерних узлов TabContainer
        foreach (var child in triggerDataPanels.GetChildren())
        {
            if (child is ITriggerPanel panel)
                triggerPanels.Add(panel);
        }

        // Подписка на UI базовых данных
        baseDataTriggerPanel.startTimeLineEdit.DataChanged += OnStartTimeChanged;
        baseDataTriggerPanel.transitionTimeLineEdit.DataChanged += OnTransitionTimeChanged;
        baseDataTriggerPanel.isAdditiveCheckButton.Toggled += OnIsAdditiveToggled;
    }

    public void LoadTriggerData(Trigger trigger)
	{
		// Отписываемся от предыдущей панели
		if (_currentPanel != null)
		{
			_currentPanel.UnsubscribePanelChanges();
		}

		_currentTrigger = trigger;
		// ... остальной код загрузки ...

		int tabIndex = (int)trigger.triggerType;
		if (tabIndex < triggerPanels.Count)
		{
			triggerDataPanels.CurrentTab = tabIndex;
			var newPanel = triggerPanels[tabIndex];
			newPanel.LoadDataToPanel(trigger);
			newPanel.SubscribePanelChanges();

            baseDataTriggerPanel.startTimeLineEdit.SetValueWithoutNotify(trigger.startTime);
            baseDataTriggerPanel.transitionTimeLineEdit.SetValueWithoutNotify(trigger.endTime);
            baseDataTriggerPanel.triggerType.Select((int)trigger.triggerType);
            baseDataTriggerPanel.isAdditiveCheckButton.ButtonPressed = trigger.IsAdditive;

			_currentPanel = newPanel; // Сохраняем ссылку на текущую панель
		}
	}

    private void OnStartTimeChanged(Variant newData)
    {
        if (_currentTrigger != null) _currentTrigger.startTime = (float)newData;
    }

    private void OnTransitionTimeChanged(Variant newData)
    {
        if (_currentTrigger != null) _currentTrigger.endTime = (float)newData;
    }

    private void OnTriggerTypeSelected(long index)
	{
		if (_currentTrigger == null) return;

		TriggerType newType = (TriggerType)index;
		
		// Если тип реально меняется
		if (_currentTrigger.triggerType != newType)
		{
			// НЕ меняем тип напрямую: _currentTrigger.triggerType = ...
			// А запрашиваем изменение у контроллера:
			OnTriggerTypeChangeRequested?.Invoke(_currentTrigger, newType);
            triggerDataPanels.CurrentTab = (int)newType;
            //LoadTriggerData(_currentTrigger);
		}
	}

    private void OnIsAdditiveToggled(bool isAdditive)
    {
        if (_currentTrigger != null) _currentTrigger.IsAdditive = isAdditive;
    }
}
