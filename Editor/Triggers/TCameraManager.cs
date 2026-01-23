using Godot;
using System;
using System.Linq;

namespace LostEditor;

public partial class TCameraManager : Node
{
    [Export] public TriggerManager triggerManager;
    [Export] public TimelineController timelineController;
    [Export] public Camera2D ViewportCamera;

    [Export] public Vector2 DefaultPosition = Vector2.Zero;

    // ВОТ ОН - ГЛАВНЫЙ ЦИКЛ ОБНОВЛЕНИЯ
    public override void _Process(double delta)
    {
        if (triggerManager == null || timelineController == null || ViewportCamera == null)
            return;

        UpdateCameraPosition();
    }

    public void UpdateCameraPosition()
    {
        double currentTime = timelineController.timelineTime;
        
        // 1. Считаем фиксированную позицию (Override)
        Vector2 basePos = CalculateFixedValue(TriggerType.CameraPosition, currentTime);

        // 2. Считаем аддитивную позицию (накопленную)
        Vector2 offsetPos = CalculateAdditiveValue(TriggerType.CameraPosition, currentTime);

        // Прямое назначение позиции камере
        ViewportCamera.Position = new Vector2(basePos.X,-basePos.Y) + new Vector2(offsetPos.X,-offsetPos.Y);
    }

    private Vector2 CalculateFixedValue(TriggerType type, double time)
    {
        var list = triggerManager.GetTriggersOfType(type).Where(t => !t.IsAdditive).ToList();
        var activeTrigger = list.LastOrDefault(t => t.startTime <= time);

        if (activeTrigger == null) return DefaultPosition;

        if (activeTrigger is TriggerCameraPosition tcp)
        {
            Vector2 target = new Vector2(tcp.CameraPositionX, tcp.CameraPositionY);
            // Рекурсивно ищем точку, от которой интерполировать
            Vector2 startPos = CalculateFixedValue(type, activeTrigger.startTime - 0.001); 
            float weight = activeTrigger.GetProgress(time);
            return startPos.Lerp(target, weight);
        }
        return DefaultPosition;
    }

    private Vector2 CalculateAdditiveValue(TriggerType type, double time)
    {
        Vector2 sum = triggerManager.GetAdditiveBase(type, time);
        float sectorStartTime = (float)Math.Floor(time / TriggerManager.SectorSize) * TriggerManager.SectorSize;
        
        var list = triggerManager.GetTriggersOfType(type).Where(t => t.IsAdditive).ToList();

        // Берем только те, что влияют на текущий момент и еще не в кеше секторов
        var relevantTriggers = list.Where(t => 
            t.startTime <= time && 
            (t.startTime + t.endTime) > sectorStartTime);

        foreach (var t in relevantTriggers)
        {
            if (t is TriggerCameraPosition tcp)
            {
                float weight = t.GetProgress(time);
                sum += new Vector2(tcp.CameraPositionX, tcp.CameraPositionY) * weight;
            }
        }
        return sum;
    }
}