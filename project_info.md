# LostEditor — Project Info

## Общее описание

**LostEditor** — редактор уровней для ритм-игры, написанный на **Godot 4 + C#**.
Редактор позволяет создавать анимированные объекты на таймлайне синхронизированные с музыкой.

**Целевая платформа:** Windows Desktop  
**Движок:** Godot 4, C# (.NET 8)  
**Namespace:** `LostEditor`

---

## Архитектура проекта

### Ключевые классы

| Класс | Файл | Назначение |
|---|---|---|
| `GameObject` | `CoreScripts/GameObject.cs` | Данные объекта на сцене (кейфреймы, время, форма) |
| `Keyframe<T>` | `CoreScripts/KeyframeAnimation.cs` | Универсальный кейфрейм (float или ObjectColor) |
| `ObjectManager` | `CoreScripts/ObjectManager.cs` | Управление объектами, цикл анимации |
| `ObjectRenderer` | — | Единый рендерер всех объектов через ArrayMesh |
| `ShapeLibrary` | — | Генератор базовых полигонов (Square, Triangle, Circle, Hexagon) |
| `TimelineObjectController` | `Editor/TimelineScripts/TimelineObjectController.cs` | Создание/удаление/перемещение блоков на таймлайне |
| `TimelineKeyframeControlSystem` | `Editor/TimelineKeyframeControlSystem.cs` | Управление кейфреймами (выделение, драг, copy/paste) |
| `SelectionManager` | — | Мультивыделение TimelineBlock |
| `TriggerManager` | — | Хранение и регистрация триггеров |
| `TimelineTriggerController` | — | Создание/удаление TriggerBlock |
| `ColorManager` | `Editor/ColorManager.cs` | Обработка цветовых триггеров |
| `TCameraManager` | — | Обработка триггеров камеры |
| `TimelineController` | — | Управление временем, zoom, слайдер |
| `DebugProfiler` | — | Статический профайлер участков кода |
| `DebugWindow` | — | Window-сцена с графиками производительности |

---

## Система объектов (GameObject)

### Структура данных

```
GameObject : Node2D
├── ShapeType (enum: Square, Triangle, Circle, Hexagon, Custom)
├── CustomPolygon (Vector2[]) — используется если ShapeType == Custom
├── CircleSegments (int) — детализация круга
├── Color — текущий цвет (пишется ObjectManager-ом каждый кадр)
├── startTime, endTime, endTimeOffset, endTimeMode, cachedEndTime
├── objectColor (ObjectColor) — базовый цвет объекта
├── keyframePositionX/Y (List<Keyframe<float>>)
├── keyframeScaleX/Y    (List<Keyframe<float>>)
├── keyframeRotation    (List<Keyframe<float>>) — в ГРАДУСАХ
├── keyframeColor       (List<Keyframe<ObjectColor>>)
└── animCache (AnimationCache) — кеш индексов для инкрементального поиска
```

### AnimationCache (вложен в GameObject)

```csharp
public class AnimationCache
{
    public int IndexPosX, IndexPosY;
    public int IndexScaleX, IndexScaleY;
    public int IndexRotation, IndexColor;
    public bool IsDirty = true; // сброс индексов при следующем кадре
}
```

`IsDirty = true` нужно выставлять при:
- Добавлении/удалении/перемещении кейфреймов
- Когда объект становится видимым после невидимости
- В `ObjectStateUpdate` при смене видимости

---

## Система анимации (ObjectManager)

### Цикл (_PhysicsProcess)

1. `RebuildSortedIndex()` — пересортировка по `startTime`, только если `_indexDirty`
2. `Parallel.For` по `_sortedByStart` — параллельное **вычисление** значений
3. Если `obj.startTime > time` → скрыть остаток списка, break
4. Параллельно вычисляются: Position, Scale, Rotation, Color → записываются в `AnimationResult[]`
5. Главный поток применяет результаты в Node-свойства
6. `objectRenderer.MarkDirty()` если что-то изменилось

### Важное правило многопоточности

```
В Parallel.For можно:   читать List<Keyframe>, AnimationCache, float-поля C#
В Parallel.For НЕЛЬЗЯ: читать/писать Position, Scale, Rotation, Color, Visible (Node-свойства Godot)
Запись в Node — ТОЛЬКО в однопоточном цикле после Parallel.For
```

### Инкрементальный поиск (GetCurrentIndex)

Вместо бинарного поиска `O(log N)` каждый кадр — инкрементальный `O(1)`:

```
Время вперёд  → while: двигаем индекс вправо пока следующий кейфрейм не наступил
Время назад   → while: двигаем индекс влево пока текущий кейфрейм > time
Низкий FPS    → while покрывает любое количество пропущенных кейфреймов
```

Возвращает `-1` если время ДО первого кейфрейма (важно! без этого объект получает значение первого кейфрейма раньше времени).

### EndTimeMode

| Режим | Поведение |
|---|---|
| `NoEndTime` | cachedEndTime = float.MaxValue (бесконечный) |
| `FixedTime` | cachedEndTime = endTime |
| `LastKeyframe` | cachedEndTime = время последнего кейфрейма |
| `LastKeyframeOffset` | cachedEndTime = последний кейфрейм + offset |
| `GlobalTime` | cachedEndTime = endTime (глобальное время на таймлайне) |

**Важно:** `ObjectStateUpdate` использует `globalEnd = startTime + cachedEndTime`, кроме `GlobalTime` где `globalEnd = endTime` напрямую.

---

## Система рендеринга (ObjectRenderer)

### Принцип работы

Один `MeshInstance2D` рисует все объекты за **1 draw call**:
- Собирает геометрию всех видимых объектов в `List<Vector3>` + `List<Color>`
- Триангуляция через `Geometry2D.TriangulatePolygon` (поддерживает вогнутые)
- Результат загружается в `ArrayMesh` через `AddSurfaceFromArrays`

### Dirty flag

`_isDirty = true` → `RebuildMesh()` выполняется.  
`_isDirty = false` → `_Process` пропускает перестройку.  
`MarkDirty()` вызывается из `ObjectManager` когда объект изменился.

### Кеш триангуляции

```csharp
private class ObjectCache
{
    public int[]     Indices;
    public Vector2[] Polygon;
    public ShapeType ShapeType;
    public int       CircleSegments;
}
```

Пересчитывается только если изменился `ShapeType` или `CircleSegments`.  
При удалении объекта: `objectRenderer.RemoveFromCache(obj)`.

### Polygon2D

**Не используется для рендеринга.** Все `Polygon2D` ноды должны быть убраны из сцены `GameObject` или иметь пустой полигон — иначе каждый даёт отдельный draw call даже если `Visible = false`.

---

## Система кейфреймов (TimelineKeyframeControlSystem)

### Операции

- **ПКМ по строке** → создать кейфрейм (`KeyframeRawController`)
- **ЛКМ** → выделить, **Ctrl+ЛКМ** → мультивыделение
- **Drag** → перемещение с сортировкой и fix overlap
- **Delete/Backspace** → удалить выделенные
- **Ctrl+C / Ctrl+X / Ctrl+V** → copy/paste с offset по позиции слайдера

### После любого изменения кейфреймов обязательно:

```csharp
obj.RecalculateEndTime();
obj.animCache.IsDirty = true; // ← иначе анимация будет использовать устаревшие индексы
keyframesPanel.LoadKeyframesToPanel(obj);
```

### ClipboardEntry (struct внутри класса)

```csharp
private struct ClipboardEntry
{
    public KeyframeType Type;
    public float OriginalTime;
    public float FloatValue;
    public ObjectColor ColorValue; // глубокая копия
    public EasingType EasingType;
}
```

---

## Система объектов на таймлайне (TimelineObjectController)

### Операции

- **Create Object** → `GameObjectScene.Instantiate` + `TimelineBlockScene.Instantiate`
- **Delete** → `DeleteBlock()` — удаляет из `activeBlocks`, `objectManager.objects`, `objectRenderer._cache`, QueueFree
- **Drag горизонтальный** → изменяет `startTime`
- **Drag вертикальный (Shift)** → смена строки (Row)
- **Ctrl+C / Ctrl+X / Ctrl+V** → copy/paste объектов целиком включая все кейфреймы
- **Клик по пустому месту** → `DeselectAll`

### ObjectClipboardEntry

При копировании объекта делается **глубокая копия** всех 6 списков кейфреймов и `ObjectColor`. При вставке объект получает имя `"(copy)"` и `startTime` смещается относительно позиции курсора таймлайна.

---

## Система триггеров

### Типы триггеров (TriggerType)

```
CameraPosition = 0
CameraZoom     = 1
CameraRotation = 2
CameraShake    = 3
ColorChange    = 4
```

### Обработчики

- `TCameraManager` — Position, Zoom, Rotation, Shake
- `ColorManager` — ColorChange с кешем по ColorId

### ColorManager

Строит кеш `Dictionary<int, List<TriggerColorChange>>` — список триггеров на каждый ColorId.  
Пересчитывается при `InvalidateCache()` (подписан на `TriggerManager.OnCacheInvalidated`).  
Использует `BinarySearchForTime` (не инкрементальный — триггеры меняются редко).

---

## Система цветов

### Классы

- `LevelColor` — базовый цвет уровня (`BaseColor`, `CurrentColor`, `Id`)
- `ObjectColor` — цвет конкретного объекта, хранит ссылку на `LevelColor`
- `LevelColorData` — словарь всех цветов уровня `Dictionary<int, LevelColor>`

### Важно

`ObjectColor.RestoreBaseLevelColor(LevelColorData)` — восстанавливает ссылку на `LevelColor` после загрузки. Раньше вызывался каждый кадр в `ColorObjectUpdate` для каждого кейфрейма — **это было узкое место**. Теперь вызывается только через `ObjectManager.RestoreAllLevelColors()` при реальном изменении данных.

---

## Debug Monitor (DebugWindow + DebugProfiler)

### DebugProfiler (статический класс)

```csharp
DebugProfiler.Begin("Section");
// ... код ...
DebugProfiler.End("Section");

// Для параллельных циклов:
DebugProfiler.BeginAccum("Section"); // накапливает
DebugProfiler.EndAccum("Section");
DebugProfiler.FlushAccumulated();    // фиксирует в конце кадра
```

### DebugWindow

- Строится программно в `_Ready()` — никаких нод в сцене не нужно
- 3 вкладки: Performance, Profiler, Scene
- Открывается по `F9` (или другой клавише через `[Export] public DebugWindow debugWindow`)
- `[Export] public ObjectManager objectManager` — для Scene Stats

### Актуальные замеры в коде

```
Objects.Animation     — весь цикл анимации
Anim.StateUpdate      — проверка видимости
Anim.Position/Scale/Rotation/Color — по каналам
Renderer.ClearSurfaces / BuildArrays / UploadToGPU
Triggers.Color / Triggers.Camera
```

---

## Производительность (итоги оптимизации)

| Метрика | До | После |
|---|---|---|
| Draw Calls | 1 на объект | 1 на всё |
| Поиск кейфрейма | O(log N) каждый кадр | O(1) инкрементальный |
| Обработка объектов | 1 поток | Parallel.For (все ядра) |
| Пересборка меша | каждый кадр | только при изменениях |
| FPS при 500 объектах | 6-14 | стабильные 60+ |
| FPS при 2200 объектах | <5 | ~40 на Intel N95 |

**Типичный уровень:** до 600 одновременных объектов → система имеет большой запас.

---

## Тонкости и подводные камни

### Godot + многопоточность
В `Parallel.For` **нельзя** читать или писать любые свойства Godot Node:
`Position`, `Scale`, `Rotation`, `Color`, `Visible`, `GlobalTransform` и т.д.
Только C# поля и коллекции.

### Rotation в градусах
Кейфреймы вращения хранятся в **градусах**, но `GameObject.Rotation` (Node2D) в **радианах**.  
Применять через `Mathf.DegToRad()` и `Mathf.LerpAngle()` для корректной интерполяции.

### Y-инверсия позиции
```csharp
gameObject.Position = new Vector2(finalX, -finalY); // Y инвертируется
```
Это соглашение проекта — ось Y в редакторе направлена вверх.

### cachedEndTime vs endTime
`cachedEndTime` — всегда **локальная длительность** объекта.  
Для `GlobalTime` режима: `globalEnd = endTime` (не `startTime + cachedEndTime`).

### AnimationCache.IsDirty
Забыть выставить `IsDirty = true` после изменения кейфреймов → анимация будет некорректной (индексы застревают на старых позициях).

### Polygon2D
Даже скрытый `Polygon2D` с вершинами = draw call. Убирать полностью или очищать полигон.

### CollisionPolygon2D
В debug-сборке рисует формы отдельным draw call. Отключать через `Disabled = true` или убирать из сцены если физика не нужна.

### GetCurrentIndex возвращает -1
Если время ДО первого кейфрейма — возвращает `-1`. Обработка `-1` обязательна во всех `Calculate*` методах иначе `IndexOutOfRange`.

### _sortedByStart и _indexDirty
При любом изменении `startTime` объекта нужно выставить `_indexDirty = true` в `ObjectManager`. Сделано через подписку `obj.OnDataChanged += () => _indexDirty = true` в `RegisterObject`.

### Удаление объекта
При удалении вызывать:
```csharp
activeBlocks.Remove(block);
selectionManager.SelectedBlocks.Remove(block);
objectManager.objects.Remove(block.Data);
objectRenderer.RemoveFromCache(block.Data); // очистить кеш триангуляции
block.Data.QueueFree();
block.QueueFree();
```

---

## Планируемый функционал (упоминался в диалоге)

- Список базовых фигур: Square, Triangle, Circle, Hexagon + кастомные
- Импорт кастомных фигур (заполнить `CustomPolygon`, выставить `ShapeType = Custom`)
- Обратное воспроизведение таймлайна (система уже готова — инкрементальный поиск поддерживает)
- Редактор вершин для кастомных фигур
