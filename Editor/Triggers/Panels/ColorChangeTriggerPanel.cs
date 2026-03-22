using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;

/// <summary>
/// Панель редактирования триггера изменения цвета.
/// 
/// В сцене нужно иметь:
///   [Export] BaseDataTriggerPanel BaseDataPanel
///   [Export] EaseTypePanel EaseTypePanel
///   [Export] Button AddColorButton             — "Добавить цвет"
///   [Export] VBoxContainer ColorEntriesContainer — список добавленных цветов
///   [Export] PackedScene ColorTriggerEntryScene  — сцена ColorTriggerEntryControl
///   [Export] ColorSelectPopup ColorPopup         — модальное окно (узел на сцене)
///   [Export] LevelColorData LevelColors
///   [Export] ColorManager ColorManager           — для инвалидации кеша
/// </summary>
public partial class ColorChangeTriggerPanel : Control, ITriggerPanel
{
    [Export] public BaseDataTriggerPanel BaseDataPanel { get; set; }
    [Export] public EaseTypePanel EaseTypePanel { get; set; }
    [Export] public Button AddColorButton { get; set; }
    [Export] public VBoxContainer ColorEntriesContainer { get; set; }
    [Export] public PackedScene ColorTriggerEntryScene { get; set; }
    [Export] public ColorSelectPopup ColorPopup { get; set; }
    [Export] public LevelColorData LevelColors { get; set; }
    [Export] public ColorManager ColorManager { get; set; }

    private TriggerColorChange _currentTrigger;

    // Словарь активных строк: ColorId → контрол
    private Dictionary<int, ColorTriggerEntryControl> _entries = new();

    public override void _Ready()
    {
        if (AddColorButton != null)
            AddColorButton.Pressed += OnAddColorPressed;

        if (EaseTypePanel != null)
            EaseTypePanel.OnEaseTypeSelectedAction += OnEaseTypeChanged;

        if (ColorPopup != null)
            ColorPopup.OnColorSelected += OnColorSelectedFromPopup;
    }

    // -------------------------------------------------------------------------
    // ITriggerPanel
    // -------------------------------------------------------------------------

    public void LoadDataToPanel(Trigger abstractTrigger)
    {
        var trigger = abstractTrigger as TriggerColorChange;
        if (trigger == null)
        {
            GD.PrintErr("[ColorChangeTriggerPanel] Неверный тип триггера!");
            return;
        }

        _currentTrigger = trigger;

        BaseDataPanel.startTimeLineEdit.SetValueWithoutNotify(trigger.startTime);
        BaseDataPanel.transitionTimeLineEdit.SetValueWithoutNotify(trigger.endTime);
        BaseDataPanel.isAdditiveCheckButton.ButtonPressed = trigger.IsAdditive;
        BaseDataPanel.triggerType.Select((int)TriggerType.ColorChange);
        EaseTypePanel?.SetType(trigger.EasingType);

        RebuildEntries();
    }

    public void SubscribePanelChanges()
    {
        if (_currentTrigger == null) return;

        BaseDataPanel.startTimeLineEdit.DataChanged += OnStartTimeChanged;
        BaseDataPanel.transitionTimeLineEdit.DataChanged += OnEndTimeChanged;
        BaseDataPanel.isAdditiveCheckButton.Toggled += OnIsAdditiveToggled;
    }

    public void UnsubscribePanelChanges()
    {
        BaseDataPanel.startTimeLineEdit.DataChanged -= OnStartTimeChanged;
        BaseDataPanel.transitionTimeLineEdit.DataChanged -= OnEndTimeChanged;
        BaseDataPanel.isAdditiveCheckButton.Toggled -= OnIsAdditiveToggled;
    }

    // -------------------------------------------------------------------------
    // Построение списка добавленных цветов
    // -------------------------------------------------------------------------

    private void RebuildEntries()
    {
        if (ColorEntriesContainer == null) return;

        ColorEntriesContainer.QueueFreeChildren();
        _entries.Clear();

        if (_currentTrigger == null) return;

        foreach (var change in _currentTrigger.ColorChanges)
            AddEntryControl(change.ColorId, change.TargetColor);
    }

    private void AddEntryControl(int colorId, Color targetColor)
    {
        if (ColorTriggerEntryScene == null || LevelColors == null) return;

        var levelColor = LevelColors.GetColor(colorId);
        if (levelColor == null) return;

        var entry = ColorTriggerEntryScene.Instantiate<ColorTriggerEntryControl>();
        entry.Setup(levelColor, targetColor);

        entry.OnTargetColorChanged += OnEntryColorChanged;
        entry.OnRemovePressed += OnEntryRemoved;

        ColorEntriesContainer.AddChild(entry);
        _entries[colorId] = entry;
    }

    // -------------------------------------------------------------------------
    // Обработчики событий
    // -------------------------------------------------------------------------

    private void OnAddColorPressed()
    {
        if (_currentTrigger == null || ColorPopup == null || LevelColors == null) return;

        // Передаём уже добавленные ID чтобы их задизейблить в окне
        var alreadyAdded = new HashSet<int>();
        foreach (var change in _currentTrigger.ColorChanges)
            alreadyAdded.Add(change.ColorId);

        ColorPopup.OpenFor(LevelColors, alreadyAdded);
    }

    private void OnColorSelectedFromPopup(LevelColor levelColor)
    {
        if (_currentTrigger == null) return;

        // Добавляем в триггер с базовым цветом как TargetColor по умолчанию
        _currentTrigger.AddColorChange(levelColor.Id, levelColor.BaseColor);
        AddEntryControl(levelColor.Id, levelColor.BaseColor);

        // Инвалидируем кеш ColorManager
        ColorManager?.InvalidateCache();

        GD.Print($"[ColorChangeTriggerPanel] Добавлен цвет {levelColor.Name} (ID: {levelColor.Id})");
    }

    private void OnEntryColorChanged(int colorId, Color newColor)
    {
        if (_currentTrigger == null) return;
        _currentTrigger.AddColorChange(colorId, newColor);
        // Кеш не трогаем — изменился только TargetColor, структура триггеров не менялась
    }

    private void OnEntryRemoved(int colorId)
    {
        if (_currentTrigger == null) return;

        _currentTrigger.RemoveColorChange(colorId);

        if (_entries.TryGetValue(colorId, out var entry))
        {
            entry.QueueFree();
            _entries.Remove(colorId);
        }

        // Инвалидируем кеш — структура триггера изменилась
        ColorManager?.InvalidateCache();

        GD.Print($"[ColorChangeTriggerPanel] Удалён цвет ID: {colorId}");
    }

    private void OnEaseTypeChanged(int index)
    {
        if (_currentTrigger == null) return;
        _currentTrigger.EasingType = (EasingType)index;
        // Easing влияет только на интерполяцию в рантайме, кеш не нужно перестраивать
    }

    private void OnStartTimeChanged(Variant value)
    {
        if (_currentTrigger == null) return;
        _currentTrigger.startTime = (float)value;
        ColorManager?.InvalidateCache();
    }

    private void OnEndTimeChanged(Variant value)
    {
        if (_currentTrigger == null) return;
        _currentTrigger.endTime = (float)value;
        // endTime влияет только на прогресс, структура кеша не меняется
    }

    private void OnIsAdditiveToggled(bool value)
    {
        if (_currentTrigger != null) _currentTrigger.IsAdditive = value;
    }

    public override void _ExitTree()
    {
        if (ColorPopup != null)
            ColorPopup.OnColorSelected -= OnColorSelectedFromPopup;
    }
}