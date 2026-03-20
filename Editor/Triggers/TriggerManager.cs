using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LostEditor;

public partial class TriggerManager : Node
{
	
    public List<Trigger> triggers = new List<Trigger>();

    // Кеш по типам для быстрого поиска
    private Dictionary<TriggerType, List<Trigger>> _typedTriggers = new();

    // Новый кеш для float-значений (Zoom, Rotation)
    private Dictionary<TriggerType, List<float>> _additiveFloatSectors = new();
    
    // Сектора для аддитивных расчетов
	public const float SectorSize = 10.0f;
    private Dictionary<TriggerType, List<Vector2>> _additiveSectors = new();

    public void RegisterTrigger(Trigger trigger)
    {
        if (!triggers.Contains(trigger))
        {
            triggers.Add(trigger);
            InvalidateCache(); // ОБЯЗАТЕЛЬНО! Без этого TCameraManager не "увидит" новые триггеры
        }
    }

    public void UnregisterTrigger(Trigger trigger)
    {
        if (trigger == null) return;
        
        if (triggers.Contains(trigger))
        {
            triggers.Remove(trigger);
            InvalidateCache(); // Пересчитываем всё: сортировку, типы, аддитивные сектора
            GD.Print("[TriggerManager] Триггер удален, кеш обновлен");
        }
    }

    // Вызывать каждый раз, когда триггер изменился, удалился или добавился
    public void InvalidateCache()
    {
        // 1. Сортируем основной список
        triggers = triggers.OrderBy(t => t.startTime).ToList();

        // 2. Группируем по типам
        _typedTriggers.Clear();
        foreach (var type in Enum.GetValues<TriggerType>())
            _typedTriggers[type] = triggers.Where(t => t.triggerType == type).ToList();

        // 3. Пересчитываем сектора для аддитивных параметров (например, Position)
        RebuildAdditiveSectors(TriggerType.CameraPosition);
        
        // 4. Пересчитываем сектора для ColorChange триггеров
        RebuildColorChangeCache();
    }

    private void RebuildAdditiveSectors(TriggerType type)
    {
        // ... существующий код для Vector2 ...

        // Обработка float-триггеров
        if (type == TriggerType.CameraZoom || type == TriggerType.CameraRotation)
        {
            if (!_additiveFloatSectors.ContainsKey(type))
                _additiveFloatSectors[type] = new List<float>();

            var sectors = _additiveFloatSectors[type];
            sectors.Clear();
            var list = _typedTriggers[type].Where(t => t.IsAdditive).ToList();
            float maxTime = list.Count > 0 ? list.Max(t => t.startTime + t.endTime) : 0;
            int sectorCount = (int)Math.Ceiling(maxTime / SectorSize) + 1;
            float runningSum = 0f;

            for (int i = 0; i < sectorCount; i++)
            {
                float sectorEndTime = i * SectorSize;
                var finished = list.Where(t =>
                    (t.startTime + t.endTime) <= sectorEndTime &&
                    (t.startTime + t.endTime) > (sectorEndTime - SectorSize));

                foreach (var t in finished)
                {
                    float val = t switch
                    {
                        TriggerCameraZoom tz => tz.CameraZoom,
                        TriggerCameraRotation tr => tr.CameraRotation,
                        _ => 0f
                    };
                    runningSum += val;
                }
                sectors.Add(runningSum);
            }
        }
    }

    // Новый метод:
    private void RebuildColorChangeCache()
    {
        // Очистка и пересчёт кеша для цветовых триггеров
        // Можно добавить дополнительную оптимизацию если нужно
        GD.Print("[TriggerManager] ColorChange cache rebuilt");
    }


    

    public float GetAdditiveFloatBase(TriggerType type, double time)
    {
        if (!_additiveFloatSectors.TryGetValue(type, out var sectors) || sectors.Count == 0)
            return 0f;

        int sectorIdx = (int)Math.Floor(time / SectorSize);
        if (sectorIdx >= sectors.Count) return sectors.Last();
        if (sectorIdx <= 0) return 0f;
        return sectors[sectorIdx - 1];
    }

    // Метод получения данных из кеша
    public Vector2 GetAdditiveBase(TriggerType type, double time)
    {
        if (!_additiveSectors.ContainsKey(type) || _additiveSectors[type].Count == 0) return Vector2.Zero;
        
        int sectorIdx = (int)Math.Floor(time / SectorSize);
        if (sectorIdx >= _additiveSectors[type].Count) return _additiveSectors[type].Last();
        if (sectorIdx <= 0) return Vector2.Zero;

        return _additiveSectors[type][sectorIdx - 1]; // Берем сумму до текущего сектора
    }

    public Trigger CreateTriggerInstanceByType(TriggerType type)
    {
        switch (type)
        {
            case TriggerType.CameraPosition: return new TriggerCameraPosition();
            case TriggerType.CameraZoom: return new TriggerCameraZoom();
            case TriggerType.CameraRotation: return new TriggerCameraRotation();
            case TriggerType.CameraShake: return new TriggerCameraShake();
            case TriggerType.ColorChange: return new TriggerColorChange();
            default: return new Trigger(); // Fallback
        }
    }

    public List<Trigger> GetTriggersOfType(TriggerType type) => _typedTriggers.GetValueOrDefault(type, new List<Trigger>());
}