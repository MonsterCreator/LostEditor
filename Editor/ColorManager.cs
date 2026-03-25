using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LostEditor;

/// <summary>
/// Менеджер цветовых триггеров.
/// Кеш строится один раз при изменении триггеров (InvalidateCache),
/// в _Process только бинарный поиск + одна Lerp на каждый активный цвет.
/// </summary>
public partial class ColorManager : Node
{
    [Export] public TriggerManager TriggerManager { get; set; }
    [Export] public LevelColorData LevelColors { get; set; }
    [Export] public TimelineController TimelineController { get; set; }

    // Кеш: ColorId → отсортированный список триггеров затрагивающих этот цвет
    private Dictionary<int, List<TriggerColorChange>> _triggersByColor = new();

    // Множество ColorId у которых есть хотя бы один триггер — 
    // остальные не трогаем в _Process вообще
    private HashSet<int> _affectedColorIds = new();

    public override void _Ready()
    {
        if (LevelColors == null)
            LevelColors = GetTree().GetFirstNodeInGroup("level_colors") as LevelColorData;
    
        if (TriggerManager == null)
            TriggerManager = GetTree().GetFirstNodeInGroup("trigger_manager") as TriggerManager;
    
        // НОВОЕ: подписываемся на инвалидацию кеша TriggerManager
        if (TriggerManager != null)
            TriggerManager.OnCacheInvalidated += InvalidateCache;
    
        ResetAllColors();
        BuildCache();
    }

    public override void _Process(double delta)
    {
        DebugProfiler.Begin("Triggers.Color");
        
        if (TriggerManager != null && LevelColors != null && TimelineController != null)
            UpdateColors((float)TimelineController.timelineTime);
        
        DebugProfiler.End("Triggers.Color"); // вызовется всегда
    }

    // -------------------------------------------------------------------------
    // Публичные методы
    // -------------------------------------------------------------------------

    /// <summary>
    /// Вызывать при любом изменении триггеров в редакторе.
    /// Перестраивает кеш за O(T log T) где T — общее число триггеров.
    /// </summary>
    public void InvalidateCache()
    {
        BuildCache();
    }

    /// <summary>
    /// Сбрасывает CurrentColor всех цветов к BaseColor.
    /// Вызывать при сбросе таймлайна на 0.
    /// </summary>
    public void ResetAllColors()
    {
        if (LevelColors == null) return;
        foreach (var lc in LevelColors.Colors.Values)
            lc.CurrentColor = lc.BaseColor;
    }

    // -------------------------------------------------------------------------
    // Построение кеша
    // -------------------------------------------------------------------------

    private void BuildCache()
    {
        // НОВОЕ: запоминаем какие цвета были затронуты ДО перестройки
        var previouslyAffected = new HashSet<int>(_affectedColorIds);
    
        _triggersByColor.Clear();
        _affectedColorIds.Clear();
    
        if (TriggerManager == null) return;
    
        var allColorTriggers = TriggerManager
            .GetTriggersOfType(TriggerType.ColorChange)
            .Cast<TriggerColorChange>();
    
        foreach (var trigger in allColorTriggers)
        {
            foreach (var entry in trigger.ColorChanges)
            {
                if (!_triggersByColor.ContainsKey(entry.ColorId))
                    _triggersByColor[entry.ColorId] = new List<TriggerColorChange>();
    
                if (!_triggersByColor[entry.ColorId].Contains(trigger))
                    _triggersByColor[entry.ColorId].Add(trigger);
    
                _affectedColorIds.Add(entry.ColorId);
            }
        }
    
        // Сортируем каждый список по startTime
        foreach (var list in _triggersByColor.Values)
            list.Sort((a, b) => a.startTime.CompareTo(b.startTime));
    
        // НОВОЕ: сбрасываем CurrentColor для цветов которые больше не затронуты
        // Это решает баг с "зависшим" промежуточным цветом после удаления
        if (LevelColors != null)
        {
            foreach (int colorId in previouslyAffected)
            {
                if (!_affectedColorIds.Contains(colorId))
                {
                    var lc = LevelColors.GetColor(colorId);
                    if (lc != null)
                    {
                        lc.CurrentColor = lc.BaseColor;
                        GD.Print($"[ColorManager] Цвет ID={colorId} сброшен в BaseColor");
                    }
                }
            }
        }
    
        GD.Print($"[ColorManager] Кеш перестроен: {_affectedColorIds.Count} затронутых цветов");
    }

    // -------------------------------------------------------------------------
    // Обновление цветов в рантайме (вызывается каждый кадр)
    // -------------------------------------------------------------------------

    private void UpdateColors(float currentTime)
    {
        if (LevelColors == null) return;

        // Обновляем только цвета у которых есть триггеры — остальные не трогаем
        foreach (int colorId in _affectedColorIds)
        {
            var levelColor = LevelColors.GetColor(colorId);
            if (levelColor == null) continue;

            levelColor.CurrentColor = CalculateColor(levelColor, colorId, currentTime);
        }
    }

    private Color CalculateColor(LevelColor levelColor, int colorId, float currentTime)
    {
        if (!_triggersByColor.TryGetValue(colorId, out var triggers) || triggers.Count == 0)
            return levelColor.BaseColor;
    
        int activeIndex = BinarySearchLastStarted(triggers, currentTime);
    
        // До первого триггера — базовый цвет
        if (activeIndex < 0)
            return levelColor.BaseColor;
    
        var activeTrigger = triggers[activeIndex];
        float progress = GetProgress(activeTrigger, currentTime);
    
        // ИСПРАВЛЕНО: стартовый цвет — это реальный цвет в момент СТАРТА активного триггера.
        // Вычисляем рекурсивно: берём все триггеры которые начались ДО activeTrigger
        // и смотрим какой цвет они давали в момент activeTrigger.startTime.
        //
        // Пример: триггер 1 идёт с 0 до 1с (синий→жёлтый),
        //         триггер 2 стартует в 0.5с (→красный).
        // startColor для триггера 2 = CalculateColor на момент 0.5с
        //   но только по триггерам до триггера 2, т.е. только триггер 1.
        //   Триггер 1 в 0.5с имеет progress=0.5 → startColor = Lerp(синий, жёлтый, 0.5)
        //
        // Это корректнее чем брать TargetColor предыдущего триггера (жёлтый),
        // потому что тот мог ещё не завершиться.
    
        Color startColor = CalculateColorAtTime(levelColor, colorId, triggers, activeIndex, activeTrigger.startTime);
        Color targetColor = activeTrigger.GetTargetColor(colorId);
    
        return startColor.Lerp(targetColor, progress);
    }

    /// <summary>
    /// Вычисляет цвет в момент времени time, используя только триггеры
    /// с индексом СТРОГО меньше maxIndex (то есть начавшиеся раньше активного).
    /// Используется чтобы найти стартовый цвет для текущего активного триггера.
    /// </summary>
    private Color CalculateColorAtTime(
        LevelColor levelColor,
        int colorId,
        List<TriggerColorChange> triggers,
        int maxIndex,
        float time)
    {
        // Нет предыдущих триггеров — базовый цвет
        if (maxIndex <= 0)
            return levelColor.BaseColor;
    
        // Среди триггеров [0, maxIndex) ищем последний начавшийся к моменту time
        int prevIndex = -1;
        for (int i = maxIndex - 1; i >= 0; i--)
        {
            if (triggers[i].startTime <= time)
            {
                prevIndex = i;
                break;
            }
        }
    
        // Ни один предыдущий триггер ещё не начался — базовый цвет
        if (prevIndex < 0)
            return levelColor.BaseColor;
    
        var prevTrigger = triggers[prevIndex];
        float prevProgress = GetProgress(prevTrigger, time);
    
        // Рекурсивно находим стартовый цвет для prevTrigger
        Color prevStartColor = CalculateColorAtTime(levelColor, colorId, triggers, prevIndex, prevTrigger.startTime);
        Color prevTargetColor = prevTrigger.GetTargetColor(colorId);
    
        return prevStartColor.Lerp(prevTargetColor, prevProgress);
    }

    /// <summary>
    /// Бинарный поиск: возвращает индекс последнего триггера
    /// у которого startTime <= currentTime. Возвращает -1 если таких нет.
    /// O(log N).
    /// </summary>
    private int BinarySearchLastStarted(List<TriggerColorChange> triggers, float currentTime)
    {
        int left = 0;
        int right = triggers.Count - 1;
        int result = -1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            if (triggers[mid].startTime <= currentTime)
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return result;
    }

    private float GetProgress(Trigger trigger, float currentTime)
    {
        if (trigger.endTime <= 0f)
            return currentTime >= trigger.startTime ? 1f : 0f;

        float t = (currentTime - trigger.startTime) / trigger.endTime;
        t = Mathf.Clamp(t, 0f, 1f);
        return EasingFunctions.Ease(t, trigger.EasingType);
    }

    public override void _ExitTree()
    {
        // Отписываемся чтобы не было утечек
        if (TriggerManager != null)
            TriggerManager.OnCacheInvalidated -= InvalidateCache;
    }
}