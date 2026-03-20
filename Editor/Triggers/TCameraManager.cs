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
    [Export] public float DefaultZoom = 1.0f;
    [Export] public float DefaultRotation = 0.0f;

    private Random _random = new();

    public override void _Process(double delta)
    {
        if (triggerManager == null || timelineController == null || ViewportCamera == null)
            return;

        double currentTime = timelineController.timelineTime;

        // Позиция
        Vector2 fixedPos = CalculateFixedValue(TriggerType.CameraPosition, currentTime);
        Vector2 additivePos = CalculateAdditiveValue(TriggerType.CameraPosition, currentTime);
        ViewportCamera.Position = new Vector2(fixedPos.X, -fixedPos.Y) + new Vector2(additivePos.X, -additivePos.Y);

        // Зум
        float fixedZoom = CalculateFixedFloat(TriggerType.CameraZoom, currentTime, DefaultZoom);
        float additiveZoom = CalculateAdditiveFloat(TriggerType.CameraZoom, currentTime);
        ViewportCamera.Zoom = new Vector2(fixedZoom + additiveZoom, fixedZoom + additiveZoom);

        // Вращение (в градусах!)
        float fixedRot = CalculateFixedFloat(TriggerType.CameraRotation, currentTime, DefaultRotation);
        float additiveRot = CalculateAdditiveFloat(TriggerType.CameraRotation, currentTime);
        ViewportCamera.RotationDegrees = fixedRot + additiveRot;

        // Тряска — применяется как временный оффсет к позиции
        Vector2 shakeOffset = CalculateShakeOffset(currentTime);
        ViewportCamera.Position += shakeOffset;
    }

    // === ПОЗИЦИЯ (уже есть, но для полноты картины оставим комментарий) ===
    private Vector2 CalculateFixedValue(TriggerType type, double time)
    {
        var list = triggerManager.GetTriggersOfType(type).Where(t => !t.IsAdditive).ToList();
        var activeTrigger = list.LastOrDefault(t => t.startTime <= time);
        if (activeTrigger == null) return DefaultPosition;

        if (activeTrigger is TriggerCameraPosition tcp)
        {
            Vector2 target = new Vector2(tcp.CameraPositionX, tcp.CameraPositionY);
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

    // === FLOAT-значения: Zoom и Rotation ===
    private float CalculateFixedFloat(TriggerType type, double time, float defaultValue)
    {
        var list = triggerManager.GetTriggersOfType(type).Where(t => !t.IsAdditive).ToList();
        var activeTrigger = list.LastOrDefault(t => t.startTime <= time);
        if (activeTrigger == null) return defaultValue;

        float target = GetFloatValueFromTrigger(activeTrigger);
        float startValue = CalculateFixedFloat(type, activeTrigger.startTime - 0.001, defaultValue);
        float weight = activeTrigger.GetProgress(time);
        return Mathf.Lerp(startValue, target, weight);
    }

    private float CalculateAdditiveFloat(TriggerType type, double time)
    {
        float sum = GetAdditiveFloatBase(type, time); // ← нужно добавить этот метод в TriggerManager
        float sectorStartTime = (float)Math.Floor(time / TriggerManager.SectorSize) * TriggerManager.SectorSize;
        var list = triggerManager.GetTriggersOfType(type).Where(t => t.IsAdditive).ToList();
        var relevantTriggers = list.Where(t =>
            t.startTime <= time &&
            (t.startTime + t.endTime) > sectorStartTime);

        foreach (var t in relevantTriggers)
        {
            float value = GetFloatValueFromTrigger(t);
            float weight = t.GetProgress(time);
            sum += value * weight;
        }
        return sum;
    }

    private float GetFloatValueFromTrigger(Trigger t)
    {
        return t switch
        {
            TriggerCameraZoom tz => tz.CameraZoom,
            TriggerCameraRotation tr => tr.CameraRotation,
            _ => 0f
        };
    }

    // === ТРЯСКА ===
    private Vector2 CalculateShakeOffset(double time)
    {
        var shakeTriggers = triggerManager.GetTriggersOfType(TriggerType.CameraShake)
            .Where(t => t.startTime <= time && time <= t.startTime + t.endTime)
            .Cast<TriggerCameraShake>();

        Vector2 totalShake = Vector2.Zero;

        foreach (var t in shakeTriggers)
        {
            float progress = t.GetProgress(time);
            float strength = t.CameraShakeStrenght * progress; // затухание по прогрессу
            float speed = t.CameraShakeSpeed;

            // Используем псевдослучайный шум на основе времени и seed (ID триггера)
            // Чтобы не использовать Random каждый кадр, используем sin/cos с разными фазами
            float phaseX = (float)(time * speed + t.GetHashCode() * 13.579f);
            float phaseY = (float)(time * speed + t.GetHashCode() * 24.680f);

            float offsetX = 0f, offsetY = 0f;
            if (t.IsCameraShakeXActive) offsetX = Mathf.Sin(phaseX) * strength;
            if (t.IsCameraShakeYActive) offsetY = Mathf.Cos(phaseY) * strength;

            totalShake += new Vector2(offsetX, offsetY);
        }

        return new Vector2(totalShake.X, -totalShake.Y); // Y инвертируем, как и позицию
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ FLOAT-АДДИТИВНЫХ СЕКТОРОВ ===
    // Эти методы потребуют доработки TriggerManager (см. ниже)
    private float GetAdditiveFloatBase(TriggerType type, double time)
    {
        // Просто делегируем в менеджер
        return triggerManager.GetAdditiveFloatBase(type, time);
    }
}