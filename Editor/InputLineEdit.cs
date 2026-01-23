using Godot;
using System;
using System.Globalization; // ЭТА СТРОКА ОБЯЗАТЕЛЬНА для NumberStyles и CultureInfo

namespace LostEditor;
[GlobalClass]
public partial class InputLineEdit : LineEdit
{
    // Типы данных
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
    [Export] public float ShiftMultiplier { get; set; } = 10.0f;
    [Export] public float CtrlMultiplier { get; set; } = 0.1f;

    #endregion

    // Храним последнее валидное значение для отката при ошибке ввода
    private Variant _lastValidValue;

    public override void _Ready()
    {
        // Подключаем стандартные сигналы
        TextSubmitted += OnTextSubmitted;
        FocusExited += OnFocusExited;

        // Инициализируем начальное значение
        ValidateAndSaveCurrent(Text, false);
    }

    public override void _GuiInput(InputEvent @event)
    {
        // ИСПРАВЛЕНИЕ: Используем Editable вместо !IsReadOnly()
        // Скроллим только если поле активно (Editable = true)
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
            int result = current + (int)step;
            SetNewValue(result);
        }
        else if (DataType == InputDataType.Float)
        {
            float current = (float)_lastValidValue.AsDouble();
            float result = current + step;
            // Округляем до 4 знаков, чтобы избежать проблем с плавающей точкой
            result = (float)Math.Round(result, 4);
            SetNewValue(result);
        }
    }

    // --- Логика подтверждения ввода ---
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
                    newValue = intVal;
                    isValid = true;
                }
                break;

            case InputDataType.Float:
                // Заменяем запятую на точку для универсальности
                text = text.Replace(",", "."); 
                // CultureInfo.InvariantCulture требует using System.Globalization;
                if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatVal))
                {
                    newValue = floatVal;
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
            // Откат к старому значению, если ввод неверен
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

        // Ставим каретку в конец
        CaretColumn = Text.Length;

        if (emitSignal)
        {
            EmitSignal(SignalName.DataChanged, value);
        }
    }
    
    public void SetValueWithoutNotify(Variant value)
    {
        _lastValidValue = value;
        if (DataType == InputDataType.Float)
            Text = ((float)value.AsDouble()).ToString("0.###", CultureInfo.InvariantCulture);
        else
            Text = value.ToString();
    }
}