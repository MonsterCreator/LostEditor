using Godot;
using System;

namespace LostEditor;
public partial class TriggerTypeOptionButtonController : OptionButton
{
    [Signal] public delegate void TriggerTypeSelectedEventHandler(int index);
    public Action<int> TypeSelectedAction;

    public override void _Ready()
    {
        // Подключаем стандартный сигнал выбора
        this.ItemSelected += OnEaseTypeSelected;
        
        // Подключаем обработку колесика мыши
        this.GuiInput += HandleOptionButtonInput;
    }

    public void SetType(TriggerType triggerType)
    {
        this.Select((int)triggerType);
    }

    private void HandleOptionButtonInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            int currentIndex = this.Selected;
            int count = this.ItemCount;

            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
            {
                if (currentIndex > 0)
                {
                    ChangeSelection(currentIndex - 1);
                    EmitSignal(SignalName.TriggerTypeSelected, currentIndex - 1);
                }
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
            {
                if (currentIndex < count - 1)
                {
                    ChangeSelection(currentIndex + 1);
                    EmitSignal(SignalName.TriggerTypeSelected, currentIndex + 1);
                }
            }
        }
    }

    private void ChangeSelection(int newIndex)
    {
        this.Selected = newIndex;
        // Генерируем событие вручную, так как изменение .Selected из кода его не вызывает
        OnEaseTypeSelected(newIndex);
    }

    private void OnEaseTypeSelected(long index)
    {
        // Приводим long к int, так как Action ожидает int
        TypeSelectedAction?.Invoke((int)index);
    }
}


