using Godot;
using System;

namespace LostEditor;

/// <summary>
/// Одна строка в списке цветов триггера ColorChangeTriggerPanel.
/// 
/// В сцене:
///   Корень: HBoxContainer
///     ColorRect [Export] BaseColorPreview    (24x24) — базовый цвет
///     Label     [Export] ColorNameLabel      (SizeFlagsH = ExpandFill)
///     ColorRect [Export] TargetColorPreview  (24x24) — текущий выбранный целевой цвет
///     ColorPickerButton [Export] TargetColorPicker
///     Button    [Export] RemoveButton        ("X")
/// </summary>
public partial class ColorTriggerEntryControl : MarginContainer
{
    [Export] public ColorRect BaseColorPreview;
    [Export] public Label ColorNameLabel;
    [Export] public ColorRect TargetColorPreview;
    [Export] public ColorPickerButton TargetColorPicker;
    [Export] public Button RemoveButton;

    public int ColorId { get; private set; }

    public event Action<int, Color> OnTargetColorChanged;  // colorId, newColor
    public event Action<int> OnRemovePressed;               // colorId

    public void Setup(LevelColor levelColor, Color targetColor)
    {
        ColorId = levelColor.Id;

        if (BaseColorPreview != null)
            BaseColorPreview.Color = levelColor.BaseColor;

        if (ColorNameLabel != null)
            ColorNameLabel.Text = $"{levelColor.Name} (ID: {levelColor.Id})";

        if (TargetColorPicker != null)
        {
            TargetColorPicker.Color = targetColor;
            TargetColorPicker.ColorChanged += OnPickerColorChanged;
        }

        if (TargetColorPreview != null)
            TargetColorPreview.Color = targetColor;

        if (RemoveButton != null)
            RemoveButton.Pressed += () => OnRemovePressed?.Invoke(ColorId);
    }

    private void OnPickerColorChanged(Color newColor)
    {
        if (TargetColorPreview != null)
            TargetColorPreview.Color = newColor;

        OnTargetColorChanged?.Invoke(ColorId, newColor);
    }
}