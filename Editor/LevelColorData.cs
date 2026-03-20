using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;

/// <summary>
/// Хранит палитру цветов доступных на уровне
/// </summary>
public partial class LevelColorData : Node
{
    // Словарь: ID цвета → данные цвета
    public Dictionary<int, LevelColor> Colors { get; private set; } = new();
    
    // Событие при изменении палитры
    public event Action OnPaletteChanged;
    
    public override void _Ready()
    {
        // Инициализация дефолтных цветов (можно настроить в инспекторе)
        InitializeDefaultColors();
    }
    
    private void InitializeDefaultColors()
    {
        AddColor(0, "Primary", new Color(1f, 0f, 0f, 1f));
        AddColor(1, "Secondary", new Color(0f, 1f, 0f, 1f));
        AddColor(2, "Accent", new Color(0f, 0f, 1f, 1f));
        AddColor(3, "Third", new Color(1f, 1f, 1f, 1f));
        AddColor(4, "Fourth", new Color(0f, 0f, 0f, 1f));
    }
    
    public void AddColor(int id, string name, Color baseColor)
    {
        if (Colors.ContainsKey(id))
        {
            Colors[id].BaseColor = baseColor;
            Colors[id].Name = name;
        }
        else
        {
            Colors.Add(id, new LevelColor(id, name, baseColor));
        }
        OnPaletteChanged?.Invoke();
    }
    
    public void RemoveColor(int id)
    {
        if (Colors.Remove(id))
        {
            OnPaletteChanged?.Invoke();
        }
    }
    
    public LevelColor GetColor(int id)
    {
        return Colors.TryGetValue(id, out var color) ? color : null;
    }
    
    public Color GetBaseColor(int id)
    {
        return GetColor(id)?.BaseColor ?? Godot.Colors.White;
    }
    
    /// <summary>
    /// Применяет HSV модификаторы к базовому цвету
    /// </summary>
    public Color GetModifiedColor(int id, float hMod, float sMod, float vMod, float aMod = 0f)
    {
        var levelColor = GetColor(id);
        if (levelColor == null) return Godot.Colors.White;
        
        // Конвертируем в HSV
        levelColor.BaseColor.ToHsv(out float h, out float s, out float v);
        
        // Применяем модификаторы (диапазон -1 до 1)
        h = Mathf.Clamp(h + hMod, 0f, 1f);
        s = Mathf.Clamp(s + sMod, 0f, 1f);
        v = Mathf.Clamp(v + vMod, 0f, 1f);
        float a = Mathf.Clamp(levelColor.BaseColor.A + aMod, 0f, 1f);
        
        // Конвертируем обратно в RGB
        return Color.FromHsv(h, s, v, a);
    }
}

/// <summary>
/// Данные отдельного цвета уровня
/// </summary>
[Serializable]
public class LevelColor
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Color BaseColor { get; set; }
    public Color CurrentColor { get; set; } // Цвет после применения триггеров
    
    public LevelColor(int id, string name, Color baseColor)
    {
        Id = id;
        Name = name;
        BaseColor = baseColor;
        CurrentColor = baseColor;
    }
}