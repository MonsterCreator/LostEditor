using Godot;
using System.Collections.Generic;
using System;


using System.Linq;
using LostEditor;

using Godot;
using System.Collections.Generic;
using LostEditor;

public partial class Editor : Node2D
{
    #region Exports
    [ExportGroup("Components")]
    [Export] public ObjectManager ObjManager;
    [Export] public ScrollContainer HorScroll;
    [Export] public Control[] Rows;
    [Export] public Node ViewportObj;
    
    [ExportGroup("UI Links")]
    [Export] public TextEdit GlobalTimeTextEdit;
    [Export] public VBoxContainer TimelineContainer;
    [Export] public HSlider TimelineSlider;

    [ExportGroup("Prefabs")]
    [Export] public PackedScene GameObjectScene;
    [Export] public PackedScene TimelineBlockScene;

    [ExportGroup("Settings")]
    [Export] public float PixelsPerSecond = 100f;
    [Export] public float timelineMaxTime = 60f;
    #endregion

    #region Private Fields
    private List<TimelineBlock> _activeBlocks = new();
    private SelectionManager _selection = new SelectionManager();
    
    private bool _isDragging = false;
    private bool _hasMoved = false;
    private float _mouseYAccumulator = 0f;
    private TimelineBlock _lastClickedBlock = null;
    
    public float timelineTime;
    private bool _isPlay = false;
    private const float RowThreshold = 30f;
    #endregion

    public override void _Ready() => UpdateTimelineSize();

    public override void _PhysicsProcess(double delta)
    {
        if (_isPlay) timelineTime += (float)delta;
        if (ObjManager != null) ObjManager.time = timelineTime;

        GlobalTimeTextEdit.Text = TimeUtils.SecondsToMinutesString(timelineTime);
        TimelineSlider.Value = timelineTime;
        
        if (Input.IsActionJustPressed("Space")) _isPlay = !_isPlay;
    }

    #region Input Handling
    public override void _Input(InputEvent @event)
    {
        if (HandleDeletion(@event)) return;
        HandleDrag(@event);
    }

    private bool HandleDeletion(InputEvent @event)
    {
        if ((@event.IsActionPressed("ui_text_backspace") || Input.IsKeyPressed(Key.Delete)) && _selection.SelectedBlocks.Count > 0)
        {
            var toDelete = new List<TimelineBlock>(_selection.SelectedBlocks);
            toDelete.ForEach(DeleteBlock);
            GetViewport().SetInputAsHandled();
            return true;
        }
        return false;
    }

    private void HandleDrag(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && _isDragging)
        {
            _hasMoved = true;
            if (Input.IsKeyPressed(Key.Shift)) ProcessVerticalMove(motion.Relative.Y);
            else ProcessHorizontalMove(motion.Relative.X);
        }

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
        {
            FinalizeClick();
        }
    }
    #endregion

    #region Core Logic
    public void CreateObjectButtonPressed()
    {
        // 1. Проверки на наличие необходимых ссылок
        if (Rows == null || Rows.Length == 0 || GameObjectScene == null || TimelineBlockScene == null)
        {
            GD.PrintErr("Ошибка: Не назначены Rows или Prefabs в инспекторе Editor!");
            return;
        }

        // 2. Создаем РЕАЛЬНЫЙ игровой объект (данные)
        var newSceneObj = GameObjectScene.Instantiate<GameObject>();
        float start = timelineTime;
        float duration = 5.0f;
        newSceneObj.startTime = start;
        newSceneObj.endTime = start + duration;

        // Добавляем его в мир (SceneObjects) и регистрируем в менеджере
        // Предполагаем, что SceneObjects — это родитель для игровых объектов на сцене
        ViewportObj.AddChild(newSceneObj); // Или используйте специфичный узел
        ObjManager.RegisterObject(newSceneObj);

        // 3. Создаем ВИЗУАЛЬНЫЙ блок для таймлайна
        var block = TimelineBlockScene.Instantiate<TimelineBlock>();
        block.Data = newSceneObj;
        block.EditorRef = this;
        
        // По умолчанию добавляем на первую дорожку
        Rows[0].AddChild(block);
        _activeBlocks.Add(block);

        // Обновляем позицию блока в соответствии с текущим PPS
        block.UpdateVisual(PixelsPerSecond);
        
        GD.Print($"Объект создан на времени: {start}");
    }
    
    public void HandleBlockSelection(TimelineBlock block, bool isCtrl)
    {
        _isDragging = true;
        _hasMoved = false;
        _lastClickedBlock = block;
        _selection.HandleSelection(block, isCtrl);
        UpdateAllBlocks();
    }

    private void ProcessHorizontalMove(float deltaX)
    {
        float timeDelta = deltaX / PixelsPerSecond;
        // Проверка границ для всей группы
        foreach (var b in _selection.SelectedBlocks)
        {
            float newStart = b.Data.startTime + timeDelta;
            if (newStart < 0 || (b.Data.endTime + timeDelta) > timelineMaxTime) return;
        }
        
        _selection.SelectedBlocks.ForEach(b => {
            b.Data.startTime += timeDelta;
            b.Data.endTime += timeDelta;
            b.UpdateVisual(PixelsPerSecond);
        });
    }

    private void ProcessVerticalMove(float deltaY)
    {
        _mouseYAccumulator += deltaY;
    
        if (Mathf.Abs(_mouseYAccumulator) > RowThreshold)
        {
            int direction = _mouseYAccumulator > 0 ? 1 : -1;
            MoveGroup(direction);
        
            // ВАЖНО: После смены строки нужно обновить визуал, 
            // так как Position.Y может сброситься при смене родителя
            UpdateAllBlocks(); 
        
            _mouseYAccumulator = 0;
        }
    }

    private void FinalizeClick()
    {
        if (!_hasMoved && _lastClickedBlock != null && !Input.IsKeyPressed(Key.Ctrl))
        {
            _selection.DeselectAll();
            _selection.HandleSelection(_lastClickedBlock, false);
            UpdateAllBlocks();
        }
        _isDragging = false;
        _lastClickedBlock = null;
    }
    #endregion

    #region Infrastructure
    public void UpdateAllBlocks() => _activeBlocks.ForEach(b => b.UpdateVisual(PixelsPerSecond));
    
    private void UpdateTimelineSize()
    {
        TimelineContainer.CustomMinimumSize = new Vector2(timelineMaxTime * PixelsPerSecond, 0);
        TimelineSlider.MaxValue = timelineMaxTime;
    }
    
    public void OnTimeLineSliderChangedValue(float newTime)
    {
        // Обновляем глобальное время редактора значением из слайдера
        timelineTime = newTime;
    
        // Опционально: сразу обновляем текст, чтобы не ждать следующего кадра физики
        if (GlobalTimeTextEdit != null)
            GlobalTimeTextEdit.Text = TimeUtils.SecondsToMinutesString(timelineTime);
    }

    private void MoveGroup(int dir)
    {
        bool canMove = true;
        // 1. Проверяем, может ли вся группа сдвинуться
        foreach (var block in _selection.SelectedBlocks)
        {
            int currentIndex = Array.IndexOf(Rows, block.GetParent());
            int targetIndex = currentIndex + dir;
            if (targetIndex < 0 || targetIndex >= Rows.Length) { canMove = false; break; }
        }

        if (canMove)
        {
            foreach (var block in _selection.SelectedBlocks)
            {
                // Получаем текущую строку
                Node currentRow = block.GetParent();
                int currentIndex = Array.IndexOf(Rows, currentRow);
            
                // Убираем из старой и добавляем в новую
                currentRow.RemoveChild(block);
                Rows[currentIndex + dir].AddChild(block);
            
                // Важный хак для Godot: при смене родителя нужно дождаться кадра 
                // или принудительно обновить положение, иначе блок может "улететь" в 0,0
                block.UpdateVisual(PixelsPerSecond);
            }
        }
    }

    public void DeleteBlock(TimelineBlock block)
    {
        _activeBlocks.Remove(block);
        _selection.SelectedBlocks.Remove(block);
        ObjManager.objects.Remove(block.Data);
        block.Data.QueueFree();
        block.QueueFree();
    }
    
    public void ApplyZoom(float factor)
    {
        // 1. Сохраняем "центр" зума (текущее время)
        float zoomCenterTime = timelineTime; 
    
        // 2. Вычисляем смещение относительно экрана, чтобы таймлайн не "прыгал"
        float screenOffset = (zoomCenterTime * PixelsPerSecond) - HorScroll.ScrollHorizontal;

        // 3. Рассчитываем и ограничиваем новый масштаб
        float newPPS = PixelsPerSecond * factor;
        PixelsPerSecond = Mathf.Clamp(newPPS, 10f, 2000f); 

        // 4. Обновляем размеры контейнера таймлайна
        UpdateTimelineSize();

        // 5. Корректируем скролл для сохранения фокуса на времени
        float newScrollPos = (zoomCenterTime * PixelsPerSecond) - screenOffset;
        HorScroll.ScrollHorizontal = (int)newScrollPos;

        // 6. Обновляем визуальное положение всех блоков
        UpdateAllBlocks();
    }
    #endregion
}


public class SelectionManager
{
    public List<TimelineBlock> SelectedBlocks { get; private set; } = new();

    public void HandleSelection(TimelineBlock block, bool isMultiSelect)
    {
        if (!isMultiSelect && !block.IsSelected)
        {
            DeselectAll();
        }

        if (block.IsSelected && isMultiSelect)
        {
            block.IsSelected = false;
            SelectedBlocks.Remove(block);
        }
        else if (!SelectedBlocks.Contains(block))
        {
            block.IsSelected = true;
            SelectedBlocks.Add(block);
        }
    }

    public void DeselectAll()
    {
        SelectedBlocks.ForEach(b => b.IsSelected = false);
        SelectedBlocks.Clear();
    }
}


public static class TimeUtils
{
    public static string SecondsToMinutesString(float seconds)
    {
        int totalSeconds = (int)Mathf.Floor(seconds);
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        string ms = (seconds - totalSeconds).ToString("F3").Substring(2);
        return $"{minutes}:{secs:D2}.{ms}";
    }

    public static float ClampTime(float time, float duration, float maxTime)
    {
        return Mathf.Clamp(time, 0, maxTime - duration);
    }
}