using Godot;
using System;

namespace LostEditor;

public partial class ColorItemButton : AspectRatioContainer
{
    public int ColorIndex { get; private set; }
    public ObjectColor ObjColor { get; private set; }

    // Темы задаются через инспектор
    [Export] public Theme ColorSelectedTheme;
    [Export] public Theme ColorNotSelectedTheme;

    // Внутренняя кнопка — задаётся через инспектор, чтобы можно было назначить тему
    [Export] public Button Button;

    public event Action<ColorItemButton> OnColorSelected;

    public void Setup(int index, ObjectColor color)
    {
        ColorIndex = index;
        ObjColor = color;

        // Применяем цвет на кнопку, а не на контейнер
        if (Button != null)
            Button.SelfModulate = color.color;

        SetSelected(false);
    }

    // Вызывается нажатием на Button (подключи сигнал pressed в сцене)
    public void OnButtonPressed()
    {
        OnColorSelected?.Invoke(this);
    }

    /// <summary>
    /// Переключает визуальное состояние кнопки (выбрана / не выбрана).
    /// Тема задаётся на самой Button, чтобы стиль (border_radius и т.д.) менялся.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (Button == null) return;

        if (selected)
        {
            Button.Theme = ColorSelectedTheme; // может быть null — тоже ок, сбросит тему
        }
        else
        {
            // Если тема не назначена — сбрасываем в null (дефолтный вид)
            Button.Theme = ColorNotSelectedTheme;
        }
    }
}