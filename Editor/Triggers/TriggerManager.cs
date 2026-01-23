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
        // ... для Zoom, Rotation и т.д.
    }

    private void RebuildAdditiveSectors(TriggerType type)
    {
        if (!_additiveSectors.ContainsKey(type)) _additiveSectors[type] = new List<Vector2>();
        var sectors = _additiveSectors[type];
        sectors.Clear();

        var list = _typedTriggers[type].Where(t => t.IsAdditive).ToList();
        
        // Находим максимальное время, чтобы понять сколько секторов нужно
        float maxTime = list.Count > 0 ? list.Max(t => t.startTime + t.endTime) : 0;
        int sectorCount = (int)Math.Ceiling(maxTime / SectorSize) + 1;

        Vector2 runningSum = Vector2.Zero;
        for (int i = 0; i < sectorCount; i++)
        {
            float sectorEndTime = i * SectorSize;
            
            // Суммируем вклад всех триггеров, которые ПОЛНОСТЬЮ завершились к этому времени
            // (Важно: только те, что еще не были учтены в предыдущем секторе)
            var finishedInThisWindow = list.Where(t => 
                (t.startTime + t.endTime) <= sectorEndTime && 
                (t.startTime + t.endTime) > (sectorEndTime - SectorSize));

            foreach (var t in finishedInThisWindow)
            {
                if (t is TriggerCameraPosition tcp) 
                    runningSum += new Vector2(tcp.CameraPositionX, tcp.CameraPositionY);
            }
            
            sectors.Add(runningSum);
        }
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

    public List<Trigger> GetTriggersOfType(TriggerType type) => _typedTriggers.GetValueOrDefault(type, new List<Trigger>());
}