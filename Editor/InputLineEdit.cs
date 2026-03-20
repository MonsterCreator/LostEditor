using Godot;
using System;
using System.Globalization;

namespace LostEditor;


public partial class InputLineEdit : LineEdit
{
    public enum InputDataType
    {
        Integer,
        Float,
        String
    }

    [Signal]
    public delegate void DataChangedEventHandler(Variant value);

    #region Export Parameters

    [ExportGroup("Settings")]
    [Export] public InputDataType DataType { get; set; } = InputDataType.Float;

    [ExportSubgroup("Scrolling")]
    [Export] public bool ScrollEnabled { get; set; } = true;
    [Export] public float ScrollStep { get; set; } = 1.0f;
    [Export] public float ShiftMultiplier { get; set; } = 0.1f;
    [Export] public float CtrlMultiplier { get; set; } = 10f;

    [ExportSubgroup("Value Limits")]
    [Export] public bool IsUpLimit { get; set; } = false;
    [Export] public bool IsDownLimit { get; set; } = false;
    [Export] public float UpLimit { get; set; } = 0;
    [Export] public float DownLimit { get; set; } = 0;

    #endregion

    private Variant _lastValidValue;

    public override void _Ready()
    {
        TextSubmitted += OnTextSubmitted;
        FocusExited += OnFocusExited;
        ValidateAndSaveCurrent(Text, false);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (ScrollEnabled && DataType != InputDataType.String && Editable)
        {
            if (@event is InputEventMouseButton mb && mb.Pressed)
            {
                if (mb.ButtonIndex == MouseButton.WheelUp)
                {
                    ApplyScroll(1);
                    AcceptEvent(); 
                }
                else if (mb.ButtonIndex == MouseButton.WheelDown)
                {
                    ApplyScroll(-1);
                    AcceptEvent();
                }
            }
        }
        
        base._GuiInput(@event);
    }

    // --- Логика Скроллинга ---
    private void ApplyScroll(int direction)
    {
        float multiplier = 1.0f;
        if (Input.IsKeyPressed(Key.Shift)) multiplier = ShiftMultiplier;
        if (Input.IsKeyPressed(Key.Ctrl)) multiplier = CtrlMultiplier;

        float step = ScrollStep * multiplier * direction;

        if (DataType == InputDataType.Integer)
        {
            int current = _lastValidValue.AsInt32();
            // ИЗМЕНЕНО: Применяем лимиты после сложения
            int result = (int)ClampToLimits(current + step);
            SetNewValue(result);
        }
        else if (DataType == InputDataType.Float)
        {
            float current = (float)_lastValidValue.AsDouble();
            // ИЗМЕНЕНО: Применяем лимиты после сложения
            float result = ClampToLimits(current + step);
            result = (float)Math.Round(result, 4);
            SetNewValue(result);
        }
    }

    // --- Вспомогательный метод для ограничений (ДОБАВЛЕНО) ---
    private float ClampToLimits(float value)
    {
        if (IsDownLimit && value < DownLimit) value = DownLimit;
        if (IsUpLimit && value > UpLimit) value = UpLimit;
        return value;
    }

    private void OnTextSubmitted(string newText)
    {
        ValidateAndSaveCurrent(newText, true);
        ReleaseFocus(); 
    }

    private void OnFocusExited()
    {
        ValidateAndSaveCurrent(Text, true);
    }

    // --- Основная логика валидации ---
    private void ValidateAndSaveCurrent(string text, bool emitSignal)
    {
        bool isValid = false;
        Variant newValue = default;

        switch (DataType)
        {
            case InputDataType.Integer:
                if (int.TryParse(text, out int intVal))
                {
                    // ИЗМЕНЕНО: Ограничиваем введенное число
                    newValue = (int)ClampToLimits(intVal);
                    isValid = true;
                }
                break;

            case InputDataType.Float:
                text = text.Replace(",", "."); 
                if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatVal))
                {
                    // ИЗМЕНЕНО: Ограничиваем введенное число
                    newValue = ClampToLimits(floatVal);
                    isValid = true;
                }
                break;

            case InputDataType.String:
                newValue = text;
                isValid = true;
                break;
        }

        if (isValid)
        {
            UpdateValue(newValue, emitSignal);
        }
        else
        {
            if (DataType == InputDataType.Float)
                Text = ((float)_lastValidValue.AsDouble()).ToString("0.###", CultureInfo.InvariantCulture);
            else
                Text = _lastValidValue.ToString();
        }
    }

    private void SetNewValue(Variant value)
    {
        UpdateValue(value, true);
    }

    private void UpdateValue(Variant value, bool emitSignal)
    {
        _lastValidValue = value;

        if (DataType == InputDataType.Float)
            Text = ((float)value.AsDouble()).ToString("0.###", CultureInfo.InvariantCulture);
        else
            Text = value.ToString();

        CaretColumn = Text.Length;

        if (emitSignal)
        {
            EmitSignal(SignalName.DataChanged, value);
        }
    }
    
    public void SetValueWithoutNotify(Variant value)
    {
        // ДОБАВЛЕНО: Даже при прямой установке значения из кода, 
        // стоит учитывать лимиты, если это числа
        if (DataType == InputDataType.Integer)
            value = (int)ClampToLimits(value.AsInt32());
        else if (DataType == InputDataType.Float)
            value = ClampToLimits((float)value.AsDouble());

        _lastValidValue = value;
        
        if (DataType == InputDataType.Float)
            Text = ((float)value.AsDouble()).ToString("0.###", CultureInfo.InvariantCulture);
        else
            Text = value.ToString();
    }

    public float GetValueAsFloat()
    {
        if (_lastValidValue.VariantType == Variant.Type.Int)
            return (float)_lastValidValue.AsInt32();
    
        return (float)_lastValidValue.AsDouble();
    }
}