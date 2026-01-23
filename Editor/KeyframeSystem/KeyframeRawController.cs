using Godot;
using System;

namespace LostEditor;
public partial class KeyframeRawController : Control
{
	// Тип кейфреймов, которые хранит эта строка
    [Export] public KeyframeType rowType;
    
    // Ссылка на главную систему управления (должна быть назначена в инспекторе)
    [Export] public TimelineKeyframeControlSystem controlSystem;

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            // Проверяем нажатие ПКМ (Right Click)
            if (mb.ButtonIndex == MouseButton.Right && mb.Pressed)
            {
                // Вычисляем локальную позицию мыши на строке
                float localMouseX = GetLocalMousePosition().X;
                
                // Запрашиваем создание кейфрейма
                if (controlSystem != null)
                {
                    controlSystem.RequestCreateKeyframe(rowType, localMouseX);
                }
                
                // Помечаем событие как обработанное, чтобы оно не ушло дальше
                AcceptEvent();
            }
        }
    }
}
