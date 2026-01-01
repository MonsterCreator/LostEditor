using Godot;
using System;
using LostEditor;

public partial class ScrollContainerVerController : ScrollContainer
{
    [Export] public float ScrollSpeed = 30f;
    [Export] public ScrollContainerHorController _parentScroll;
    [Export] public Editor _editor;

    public override void _Input(InputEvent @event)
    {
        // Проверяем, находится ли мышь над областью таймлайна, 
        // чтобы не скроллить его, когда мы работаем в другом окне.
        if (!GetGlobalRect().HasPoint(GetGlobalMousePosition()))
            return;

        // 1. ЗУМ (ZoomTimelineUp / ZoomTimelineDown)
        if (@event.IsActionPressed("ZoomTimelineUp"))
        {
            _editor?.ApplyZoom(1.1f);
            GetViewport().SetInputAsHandled(); // Останавливаем событие
        }
        else if (@event.IsActionPressed("ZoomTimelineDown"))
        {
            _editor?.ApplyZoom(0.9f);
            GetViewport().SetInputAsHandled();
        }

        // 2. ВЕРТИКАЛЬНЫЙ СКРОЛЛ (ScrollTimelineVerticalUp / Down)
        else if (@event.IsActionPressed("ScrollTimelineVerticalUp"))
        {
            ScrollVertical -= (int)ScrollSpeed;
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed("ScrollTimelineVerticalDown"))
        {
            ScrollVertical += (int)ScrollSpeed;
            GetViewport().SetInputAsHandled();
        }

        // 3. ГОРИЗОНТАЛЬНЫЙ СКРОЛЛ (ScrollTimelineHorizontalLeft / Right)
        else if (@event.IsActionPressed("ScrollTimelineHorizontalLeft"))
        {
            _parentScroll?.ScrollManually(-1f);
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed("ScrollTimelineHorizontalRight"))
        {
            _parentScroll?.ScrollManually(1f);
            GetViewport().SetInputAsHandled();
        }
    }
}