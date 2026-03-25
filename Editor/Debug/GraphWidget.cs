using Godot;
using System.Collections.Generic;

namespace LostEditor;

public partial class GraphWidget : Control
{
    [Export] public Color LineColor    = new Color(0.2f, 0.8f, 0.4f);
    [Export] public Color BgColor      = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    [Export] public Color GridColor    = new Color(1, 1, 1, 0.08f);
    [Export] public float MaxValue     = 16.6f; // ms @ 60fps по умолчанию
    [Export] public bool  AutoScale    = true;
    [Export] public string UnitLabel   = "ms";

    private readonly Queue<float> _values = new();
    private int _capacity = 120;

    public void Push(float value)
    {
        _values.Enqueue(value);
        if (_values.Count > _capacity) _values.Dequeue();
        if (AutoScale)
        {
            float max = 0;
            foreach (var v in _values) if (v > max) max = v;
            MaxValue = max > 0 ? max * 1.2f : 1f;
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        var size = Size;

        // Фон
        DrawRect(new Rect2(Vector2.Zero, size), BgColor);

        // Горизонтальные линии сетки (4 штуки)
        for (int i = 1; i <= 4; i++)
        {
            float y = size.Y * (1f - i / 4f);
            DrawLine(new Vector2(0, y), new Vector2(size.X, y), GridColor);
            float labelVal = MaxValue * i / 4f;
            DrawString(ThemeDB.FallbackFont, new Vector2(2, y - 2),
                $"{labelVal:F1}{UnitLabel}", HorizontalAlignment.Left,
                -1, 9, new Color(1, 1, 1, 0.4f));
        }

        // Линия 16.6ms (60fps) — визуальный ориентир
        if (UnitLabel == "ms" && MaxValue > 16.6f)
        {
            float targetY = size.Y * (1f - 16.6f / MaxValue);
            DrawDashedLine(new Vector2(0, targetY), new Vector2(size.X, targetY),
                new Color(1f, 0.8f, 0.2f, 0.5f), 1f, 4f);
        }

        // График
        if (_values.Count < 2) return;

        var arr     = _values.ToArray();
        float stepX = size.X / (_capacity - 1f);

        for (int i = 1; i < arr.Length; i++)
        {
            float x0 = (i - 1) * stepX + (size.X - arr.Length * stepX);
            float x1 = i       * stepX + (size.X - arr.Length * stepX);
            float y0 = size.Y * (1f - Mathf.Clamp(arr[i - 1] / MaxValue, 0, 1));
            float y1 = size.Y * (1f - Mathf.Clamp(arr[i]     / MaxValue, 0, 1));
            DrawLine(new Vector2(x0, y0), new Vector2(x1, y1), LineColor, 1.5f);
        }
    }
}