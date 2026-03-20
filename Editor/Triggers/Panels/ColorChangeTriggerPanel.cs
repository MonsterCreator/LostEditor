using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;

public partial class ColorChangeTriggerPanel : Control, ITriggerPanel
{
    [Export] public BaseDataTriggerPanel BaseDataPanel { get; set; }
    [Export] public VBoxContainer ColorChangesContainer { get; set; }
    [Export] public PackedScene ColorChangeEntryScene { get; set; }
    [Export] public Button AddColorButton { get; set; }
    [Export] public OptionButton ColorIdSelector { get; set; }
    [Export] public ColorPickerButton ColorPicker { get; set; }
    [Export] public EaseTypePanel EaseTypePanel { get; set; }
    
    [Export] public LevelColorData LevelColors { get; set; }
    
    private TriggerColorChange _currentTrigger;
    private List<ColorChangeEntryControl> _entryControls = new();
    
    public override void _Ready()
    {
        AddColorButton.Pressed += OnAddColorPressed;
        
        // Заполняем селектор цветов из LevelColorData
        if (LevelColors != null)
        {
            LevelColors.OnPaletteChanged += UpdateColorSelector;
            UpdateColorSelector();
        }
        
        if (EaseTypePanel != null)
        {
            EaseTypePanel.OnEaseTypeSelectedAction += OnEaseTypeChanged;
        }
    }
    
    public void LoadDataToPanel(Trigger abstractTrigger)
    {
        var trigger = abstractTrigger as TriggerColorChange;
        if (trigger == null)
        {
            GD.PrintErr("[ColorChangePanel] Неверный тип триггера!");
            return;
        }
        
        _currentTrigger = trigger;
        
        // Очищаем старые контролы
        foreach (var control in _entryControls)
        {
            control.QueueFree();
        }
        _entryControls.Clear();
        
        // Загружаем базовые данные
        BaseDataPanel.startTimeLineEdit.SetValueWithoutNotify(trigger.startTime);
        BaseDataPanel.transitionTimeLineEdit.SetValueWithoutNotify(trigger.endTime);
        BaseDataPanel.isAdditiveCheckButton.ButtonPressed = trigger.IsAdditive;
        BaseDataPanel.triggerType.Select((int)TriggerType.ColorChange);
        EaseTypePanel.SetType(trigger.EasingType);
        
        // Загружаем изменения цветов
        foreach (var change in trigger.ColorChanges)
        {
            AddColorEntryControl(change.ColorId, change.TargetColor);
        }
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
    
    private void OnAddColorPressed()
    {
        if (_currentTrigger == null || ColorIdSelector == null || ColorPicker == null) return;
        
        int selectedColorId = ColorIdSelector.Selected;
        Color selectedColor = ColorPicker.Color;
        
        _currentTrigger.AddColorChange(selectedColorId, selectedColor);
        AddColorEntryControl(selectedColorId, selectedColor);
        
        GD.Print($"[ColorChangePanel] Добавлен цвет {selectedColorId}");
    }
    
    private void AddColorEntryControl(int colorId, Color color)
    {
        if (ColorChangesContainer == null || ColorChangeEntryScene == null) return;
        
        var control = ColorChangeEntryScene.Instantiate<ColorChangeEntryControl>();
        control.Setup(colorId, color, LevelColors?.GetColor(colorId)?.Name ?? $"Color {colorId}");
        control.OnRemovePressed += () => OnRemoveColorEntry(colorId, control);
        control.OnColorChanged += (newColor) => OnColorEntryChanged(colorId, newColor);
        
        ColorChangesContainer.AddChild(control);
        _entryControls.Add(control);
    }
    
    private void OnRemoveColorEntry(int colorId, ColorChangeEntryControl control)
    {
        _currentTrigger?.RemoveColorChange(colorId);
        _entryControls.Remove(control);
        control.QueueFree();
        GD.Print($"[ColorChangePanel] Удалён цвет {colorId}");
    }
    
    private void OnColorEntryChanged(int colorId, Color newColor)
    {
        _currentTrigger?.AddColorChange(colorId, newColor);
    }
    
    private void OnStartTimeChanged(Variant value)
    {
        if (_currentTrigger != null) _currentTrigger.startTime = (float)value;
    }
    
    private void OnEndTimeChanged(Variant value)
    {
        if (_currentTrigger != null) _currentTrigger.endTime = (float)value;
    }
    
    private void OnIsAdditiveToggled(bool value)
    {
        if (_currentTrigger != null) _currentTrigger.IsAdditive = value;
    }
    
    private void OnEaseTypeChanged(int index)
    {
        if (_currentTrigger != null) _currentTrigger.EasingType = (EasingType)index;
    }
    
    private void UpdateColorSelector()
    {
        if (ColorIdSelector == null || LevelColors == null) return;
        
        ColorIdSelector.Clear();
        foreach (var color in LevelColors.Colors.Values)
        {
            ColorIdSelector.AddItem($"{color.Name} (ID: {color.Id})");
        }
    }
    
    public override void _ExitTree()
    {
        if (LevelColors != null)
            LevelColors.OnPaletteChanged -= UpdateColorSelector;
    }
}

/// <summary>
/// Контрол для одной записи изменения цвета в панели
/// </summary>
public partial class ColorChangeEntryControl : HBoxContainer
{
    [Export] public Label ColorNameLabel { get; set; }
    [Export] public ColorPickerButton ColorPicker { get; set; }
    [Export] public Button RemoveButton { get; set; }
    
    public event Action OnRemovePressed;
    public event Action<Color> OnColorChanged;
    
    public override void _Ready()
    {
        if (RemoveButton != null)
            RemoveButton.Pressed += () => OnRemovePressed?.Invoke();
        
        if (ColorPicker != null)
            ColorPicker.ColorChanged += (color) => OnColorChanged?.Invoke(color);
    }
    
    public void Setup(int colorId, Color color, string name)
    {
        if (ColorNameLabel != null)
            ColorNameLabel.Text = $"{name} (ID: {colorId})";
        
        if (ColorPicker != null)
            ColorPicker.Color = color;
    }
}