using Godot;
using System;

namespace LostEditor;
public partial class EaseTypePanel : HBoxContainer
{
    [Export] private OptionButton _optionButton;
    [Signal] public delegate void EaseTypeSelectedEventHandler(int index);
    public Action<int> OnEaseTypeSelectedAction;

    public override void _Ready()
    {
        // Подключаем стандартный сигнал выбора
        _optionButton.ItemSelected += OnEaseTypeSelected;
        
        // Подключаем обработку колесика мыши
        _optionButton.GuiInput += HandleOptionButtonInput;
    }

    public void SetType(EasingType easingType)
    {
        _optionButton.Select((int)easingType);
    }

    private void HandleOptionButtonInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            int currentIndex = _optionButton.Selected;
            int count = _optionButton.ItemCount;

            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
            {
                if (currentIndex > 0)
                {
                    ChangeSelection(currentIndex - 1);
                    EmitSignal(SignalName.EaseTypeSelected, currentIndex - 1);
                }
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
            {
                if (currentIndex < count - 1)
                {
                    ChangeSelection(currentIndex + 1);
                    EmitSignal(SignalName.EaseTypeSelected, currentIndex + 1);
                }
            }
        }
    }

    private void ChangeSelection(int newIndex)
    {
        _optionButton.Selected = newIndex;
        // Генерируем событие вручную, так как изменение .Selected из кода его не вызывает
        OnEaseTypeSelected(newIndex);
    }

    private void OnEaseTypeSelected(long index)
    {
        // Приводим long к int, так как Action ожидает int
        OnEaseTypeSelectedAction?.Invoke((int)index);
    }
}