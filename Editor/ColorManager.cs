using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LostEditor;

/// <summary>
/// Менеджер применения цветовых триггеров к LevelColorData
/// Аналог TCameraManager но для цветов объектов
/// </summary>
public partial class ColorManager : Node
{
    [Export] public TriggerManager TriggerManager { get; set; }
    [Export] public LevelColorData LevelColors { get; set; }
    [Export] public TimelineController TimelineController { get; set; }
    
    // Кеш активных триггеров по цветам
    private Dictionary<int, List<TriggerColorChange>> _activeTriggersByColor = new();
    
    public override void _Ready()
    {
        if (LevelColors == null)
            LevelColors = GetTree().GetFirstNodeInGroup("level_colors") as LevelColorData;
        
        if (TriggerManager == null)
            TriggerManager = GetTree().GetFirstNodeInGroup("trigger_manager") as TriggerManager;
    }
    
    public override void _Process(double delta)
    {
        if (TriggerManager == null || LevelColors == null || TimelineController == null)
            return;
        
        double currentTime = TimelineController.timelineTime;
        UpdateColors(currentTime);
    }
    
    private void UpdateColors(double currentTime)
    {
        // Получаем все ColorChange триггеры
        var colorTriggers = TriggerManager.GetTriggersOfType(TriggerType.ColorChange)
            .Cast<TriggerColorChange>()
            .Where(t => t.startTime <= currentTime && currentTime <= t.startTime + t.endTime)
            .ToList();
        
        // Группируем по ID цвета
        var triggersByColor = colorTriggers
            .SelectMany(t => t.ColorChanges.Select(c => new { t, c.ColorId, c.TargetColor, t.startTime, t.endTime, t.EasingType }))
            .GroupBy(x => x.ColorId);
        
        // Применяем к каждому цвету
        foreach (var colorGroup in triggersByColor)
        {
            int colorId = colorGroup.Key;
            var baseColor = LevelColors.GetBaseColor(colorId);
            
            // Находим последний активный триггер для этого цвета
            var activeTrigger = colorGroup
                .Where(t => t.startTime <= currentTime)
                .OrderByDescending(t => t.startTime)
                .FirstOrDefault();
            
            if (activeTrigger.t != null)
            {
                float progress = GetTriggerProgress(activeTrigger.t, currentTime);
                Color interpolated = baseColor.Lerp(activeTrigger.TargetColor, progress);
                LevelColors.GetColor(colorId).CurrentColor = interpolated;
            }
            else
            {
                LevelColors.GetColor(colorId).CurrentColor = baseColor;
            }
        }
    }
    
    private float GetTriggerProgress(Trigger trigger, double currentTime)
    {
        if (trigger.endTime <= 0) return currentTime >= trigger.startTime ? 1f : 0f;
        float t = (float)(currentTime - trigger.startTime) / trigger.endTime;
        t = Mathf.Clamp(t, 0f, 1f);
        return EasingFunctions.Ease(t, trigger.EasingType);
    }
    
    /// <summary>
    /// Принудительно обновить кеш триггеров
    /// </summary>
    public void RefreshCache()
    {
        _activeTriggersByColor.Clear();
    }
}