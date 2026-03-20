using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;

/// <summary>
/// Триггер изменения базовых цветов уровня
/// </summary>
public partial class TriggerColorChange : Trigger
{
    // Список изменений: ID цвета → целевой Color
    public List<ColorChangeEntry> ColorChanges { get; private set; } = new();
    
    public TriggerColorChange()
    {
        triggerType = TriggerType.ColorChange;
    }
    
    /// <summary>
    /// Добавить изменение цвета
    /// </summary>
    public void AddColorChange(int colorId, Color targetColor)
    {
        var existing = ColorChanges.Find(c => c.ColorId == colorId);
        if (existing != null)
        {
            existing.TargetColor = targetColor;
        }
        else
        {
            ColorChanges.Add(new ColorChangeEntry(colorId, targetColor));
        }
        GD.Print($"[TriggerColorChange] Добавлено изменение цвета {colorId}: {targetColor}");
    }
    
    /// <summary>
    /// Удалить изменение цвета
    /// </summary>
    public void RemoveColorChange(int colorId)
    {
        ColorChanges.RemoveAll(c => c.ColorId == colorId);
    }
    
    /// <summary>
    /// Получить целевой цвет для конкретного ID
    /// </summary>
    public Color GetTargetColor(int colorId)
    {
        var entry = ColorChanges.Find(c => c.ColorId == colorId);
        return entry?.TargetColor ?? Colors.White;
    }
    
    /// <summary>
    /// Получить все изменения цветов с прогрессом интерполяции
    /// </summary>
    public Dictionary<int, Color> GetInterpolatedColors(double currentTime, Color startColor)
    {
        var result = new Dictionary<int, Color>();
        float progress = GetProgress(currentTime);
        
        foreach (var change in ColorChanges)
        {
            // Интерполяция от белого (или startColor) к целевому цвету
            Color interpolated = startColor.Lerp(change.TargetColor, progress);
            result[change.ColorId] = interpolated;
        }
        
        return result;
    }
    
    public override string ToString()
    {
        return $"ColorChange Trigger: {ColorChanges.Count} colors, {startTime}s - {startTime + endTime}s";
    }
}

/// <summary>
/// Запись изменения одного цвета
/// </summary>
[Serializable]
public class ColorChangeEntry
{
    public int ColorId { get; set; }
    public Color TargetColor { get; set; }
    
    public ColorChangeEntry(int colorId, Color targetColor)
    {
        ColorId = colorId;
        TargetColor = targetColor;
    }
}