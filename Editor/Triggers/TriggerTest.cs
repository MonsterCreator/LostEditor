using Godot;
using System;

namespace LostEditor;

public partial class TriggerTest : Node
{
    [Export] public TriggerManager TriggerManager;
    [Export] public TCameraManager CameraManager;

    public override void _Ready()
    {
        if (TriggerManager == null || CameraManager == null) return;

        GD.Print("--- ЗАПУСК ТЕСТА ТРИГГЕРОВ ---");

        // 1. Создаем первый фиксированный триггер (0.0с - 10.0с)
        // Камера должна ехать в точку (500, 500)
        var fix1 = new TriggerCameraPosition {
            startTime = 0f,
            endTime = 10f, // Длительность
            CameraPositionX = 500,
            CameraPositionY = 500,
            IsAdditive = false,
            triggerType = TriggerType.CameraPosition,
            EasingType = EasingType.InOutQuad
        };

        // 2. Создаем второй фиксированный триггер (5.0с - 15.0с)
        // Он начинается на середине пути первого и уводит камеру в (0, 1000)
        var fix2 = new TriggerCameraPosition {
            startTime = 5f,
            endTime = 10f,
            CameraPositionX = 0,
            CameraPositionY = 1000,
            IsAdditive = false,
            triggerType = TriggerType.CameraPosition,
            EasingType = EasingType.InOutQuad
        };

        // 3. Создаем аддитивный триггер (2.0с - 8.0с)
        // Он добавит +200 по X к любому текущему значению
        var add1 = new TriggerCameraPosition {
            startTime = 2f,
            endTime = 6f,
            CameraPositionX = 200,
            CameraPositionY = 0,
            IsAdditive = true,
            triggerType = TriggerType.CameraPosition,
            EasingType = EasingType.Linear
        };

        // Регистрируем их
        TriggerManager.RegisterTrigger(fix1);
        TriggerManager.RegisterTrigger(fix2);
        TriggerManager.RegisterTrigger(add1);

        GD.Print("Триггеры созданы. Попробуйте двигать ползунок таймлайна.");
    }
}