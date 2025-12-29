using Godot;
using System;

public class ObjectController
{
    private InspectorPanel _view;
    private TimelineBlock _currentTarget;
    private Editor _editor;

    public ObjectController(InspectorPanel view, Editor editor)
    {
        _view = view;
        _editor = editor;
        ConnectSignals();
    }

    private void ConnectSignals()
    {
        // Подключаем изменение имени
        _view.NameInput.TextSubmitted += (text) => ApplyName(text);
        _view.NameInput.FocusExited += () => ApplyName(_view.NameInput.Text);

        // Подключаем изменение времени старта
        _view.StartTimeInput.TextSubmitted += (text) => ApplyStartTime(text);
        _view.StartTimeInput.FocusExited += () => ApplyStartTime(_view.StartTimeInput.Text);

        // Кнопка "Set to current" для старта
        _view.SetStartToCurrentBtn.Pressed += () => {
            ApplyStartTime(_editor.timelineTime.ToString());
            _view.StartTimeInput.Text = _editor.timelineTime.ToString("F3");
        };
    }

    public void Inspect(TimelineBlock block)
    {
        _currentTarget = block;
        if (_currentTarget == null)
        {
            _view.Visible = false;
            return;
        }

        _view.Visible = true;
        RefreshUI();
    }

    private void RefreshUI()
    {
        _view.NameInput.Text = _currentTarget.Data.Name;
        _view.StartTimeInput.Text = _currentTarget.Data.startTime.ToString("F3");
        _view.EndTimeInput.Text = _currentTarget.Data.endTime.ToString("F3");
        // Поле офсета пока скрываем по умолчанию
        // _view.OffsetInput.Visible = false; 
    }

    private void ApplyName(string newName)
    {
        if (_currentTarget == null) return;
        _currentTarget.Data.Name = newName;
        GD.Print($"Имя объекта изменено на: {newName}");
    }

    private void ApplyStartTime(string text)
    {
        if (_currentTarget == null || !float.TryParse(text, out float newStart)) return;

        // Сохраняем длительность, чтобы объект не сжимался при переносе старта
        float duration = _currentTarget.Data.endTime - _currentTarget.Data.startTime;
        
        _currentTarget.Data.startTime = newStart;
        _currentTarget.Data.endTime = newStart + duration;

        // Обновляем визуальную часть на таймлайне
        _currentTarget.UpdateVisual(_editor.PixelsPerSecond);
        _view.EndTimeInput.Text = _currentTarget.Data.endTime.ToString("F3");
    }
}