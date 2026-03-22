using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;

/// <summary>
/// Модальное окно выбора цвета уровня.
/// Показывает список всех цветов LevelColorData,
/// при клике на цвет вызывает OnColorSelected и закрывается.
/// 
/// В сцене нужно:
///   Корень: Window (или PopupPanel)
///     VBoxContainer
///       ScrollContainer (SizeFlagsVertical = ExpandFill)
///         VBoxContainer [Export] ColorListContainer
///       Button "Закрыть" — подключить сигнал pressed → OnClosePressed
/// </summary>
public partial class ColorSelectPopup : Window
{
    [Export] public VBoxContainer ColorListContainer;

    // Вызывается когда пользователь выбрал цвет
    public event Action<LevelColor> OnColorSelected;

    private LevelColorData _levelColors;

    public override void _Ready()
    {
        // Закрываем по крестику окна
        CloseRequested += Hide;
    }

    /// <summary>
    /// Открывает окно и наполняет список цветами уровня.
    /// Уже добавленные в триггер colorIds можно передать чтобы их выделить.
    /// </summary>
    public void OpenFor(LevelColorData levelColors, HashSet<int> alreadyAddedIds = null)
    {
        _levelColors = levelColors;
        BuildList(alreadyAddedIds ?? new HashSet<int>());
        Show();
    }

    private void BuildList(HashSet<int> alreadyAddedIds)
    {
        if (ColorListContainer == null || _levelColors == null) return;

        ColorListContainer.QueueFreeChildren();

        foreach (var levelColor in _levelColors.Colors.Values)
        {
            var row = new HBoxContainer();

            // Квадратик цвета
            var preview = new ColorRect();
            preview.Color = levelColor.BaseColor;
            preview.CustomMinimumSize = new Vector2(24, 24);

            // Название
            var label = new Label();
            label.Text = $"{levelColor.Name}  (ID: {levelColor.Id})";
            label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            // Кнопка выбора
            var btn = new Button();
            btn.Text = alreadyAddedIds.Contains(levelColor.Id) ? "Added" : "Add";
            btn.Disabled = alreadyAddedIds.Contains(levelColor.Id);

            // Захватываем для лямбды
            var captured = levelColor;
            btn.Pressed += () =>
            {
                OnColorSelected?.Invoke(captured);
                Hide();
            };

            row.AddChild(preview);
            row.AddChild(label);
            row.AddChild(btn);
            ColorListContainer.AddChild(row);
        }
    }

    private void OnClosePressed() // подключить в сцене
    {
        Hide();
    }
}