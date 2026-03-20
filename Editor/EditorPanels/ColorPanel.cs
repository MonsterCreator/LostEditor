using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;

public partial class ColorPanel : Control
{
    [Export] public HBoxContainer ColorsContainer;
    [Export] public PackedScene ColorItemButtonScene;

    // --- Превью итогового цвета ---
    [Export] public Panel PreviewRect;

    // --- Поля модификаторов (InputLineEdit) ---
    [Export] public InputLineEdit HInput;
    [Export] public InputLineEdit SInput;
    [Export] public InputLineEdit VInput;
    [Export] public InputLineEdit AInput;

    // --- Тип интерполяции ---
    [Export] public EaseTypePanel EaseTypePanel;

    private List<ObjectColor> _levelColors;
    private Keyframe<ObjectColor> _currentKeyframe;
    private ColorItemButton _selectedButton;
    private bool _isLoading = false;

    public override void _Ready()
    {
        if (HInput != null) HInput.DataChanged += _ => OnModifierChanged();
        if (SInput != null) SInput.DataChanged += _ => OnModifierChanged();
        if (VInput != null) VInput.DataChanged += _ => OnModifierChanged();
        if (AInput != null) AInput.DataChanged += _ => OnModifierChanged();

        if (EaseTypePanel != null)
            EaseTypePanel.OnEaseTypeSelectedAction += OnEaseTypeChanged;
    }

    public void LoadForKeyframe(Keyframe<ObjectColor> keyframe, List<ObjectColor> levelColors)
    {
        _currentKeyframe = keyframe;
        _levelColors = levelColors;
        _selectedButton = null;

        RefreshColorButtons();
        LoadModifiersFromKeyframe();
        UpdatePreview();
    }

    private void RefreshColorButtons()
    {
        ColorsContainer.QueueFreeChildren();
        _selectedButton = null;

        for (int i = 0; i < _levelColors.Count; i++)
        {
            var btn = ColorItemButtonScene.Instantiate<ColorItemButton>();
            btn.Setup(i, _levelColors[i]);
            btn.OnColorSelected += OnColorSelected;
            ColorsContainer.AddChild(btn);

            if (_currentKeyframe?.Value != null &&
                _currentKeyframe.Value.ColorId == _levelColors[i].ColorId)
            {
                btn.SetSelected(true);
                _selectedButton = btn;
            }
        }
    }

    private void OnColorSelected(ColorItemButton btn)
    {
        if (_currentKeyframe == null) return;

        if (_selectedButton != null && _selectedButton != btn)
            _selectedButton.SetSelected(false);

        btn.SetSelected(true);
        _selectedButton = btn;

        // Сохраняем текущие модификаторы при смене цвета
        float h = ReadInput(HInput, 0f);
        float s = ReadInput(SInput, 0f);
        float v = ReadInput(VInput, 0f);
        float a = ReadInput(AInput, 1f);

        btn.ObjColor.LoadData(h, s, v, a, null);
        _currentKeyframe.Value = btn.ObjColor;

        UpdatePreview();

        GD.Print($"[ColorPanel] Выбран цвет #{btn.ColorIndex}, ColorId={btn.ObjColor.ColorId}");
    }

    private void OnModifierChanged()
    {
        if (_isLoading) return;
        if (_currentKeyframe?.Value == null) return;

        float h = ReadInput(HInput, 0f);
        float s = ReadInput(SInput, 0f);
        float v = ReadInput(VInput, 0f);
        float a = ReadInput(AInput, 1f);

        _currentKeyframe.Value.LoadData(h, s, v, a, null);

        UpdatePreview();
    }

    private void OnEaseTypeChanged(int index)
    {
        if (_currentKeyframe == null) return;

        // EasingType хранится на самом Keyframe, не на ObjectColor
        _currentKeyframe.EasingType = (EasingType)index;

        GD.Print($"[ColorPanel] EasingType = {(EasingType)index}");
    }

    private void LoadModifiersFromKeyframe()
    {
        var oc = _currentKeyframe?.Value;

        _isLoading = true;

        HInput?.SetValueWithoutNotify(oc?.HModifier ?? 0f);
        SInput?.SetValueWithoutNotify(oc?.SModifier ?? 0f);
        VInput?.SetValueWithoutNotify(oc?.VModifier ?? 0f);
        AInput?.SetValueWithoutNotify(oc?.AModifier ?? 1f);

        // Загружаем тип интерполяции из кейфрейма
        if (EaseTypePanel != null && _currentKeyframe != null)
            EaseTypePanel.SetType(_currentKeyframe.EasingType);

        _isLoading = false;
    }

    private void UpdatePreview()
    {
        if (PreviewRect == null) return;
        var oc = _currentKeyframe?.Value;
        if (oc == null) return;

        PreviewRect.Modulate = oc.color;
    }

    private float ReadInput(InputLineEdit input, float fallback)
    {
        if (input == null) return fallback;
        return input.GetValueAsFloat();
    }
}

public static class NodeExtensions
{
    public static void QueueFreeChildren(this Node node)
    {
        foreach (Node child in node.GetChildren())
            child.QueueFree();
    }
}