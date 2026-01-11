using Godot;
using System;
using LostEditor;

//TimelineController является основой таймлайна. Здесь идёт обработка времени
public partial class TimelineController : Node
{
    [Export] public float PixelsPerSecond = 100f;

    [Export] public float timelineMaxTime = 60f;
    public float timelineTime = 0;
    private bool _isPlay = false;
    public float timelineSpeed = 1;

    [Export] public Editor editor;
    [Export] public float maxTime;

    public override void _Ready()
    {
        
    }

    
    public override void _PhysicsProcess(double delta)
    {

        if (_isPlay) timelineTime += (float)delta * timelineSpeed;
        if (editor.objectManager != null) editor.objectManager.time = timelineTime;

        editor.GlobalTimeTextEdit.Text = Editor.TimeUtils.SecondsToMinutesString(timelineTime);
        editor.timelineSlider.Value = timelineTime;
        var scriptSlider = editor.timelineSlider;
        scriptSlider.line.Position = new Vector2(timelineTime * editor.timelineController.PixelsPerSecond,0);
        
        if (Input.IsActionJustPressed("Space")) _isPlay = !_isPlay;
    }

    public void UpdateAllBlocks() => editor.timeLineObjectControl.activeBlocks.ForEach(b => b.UpdateVisual(PixelsPerSecond));

    public void UpdateTimelineSize()
    {
        editor.TimelineContainer.CustomMinimumSize = new Vector2(editor.timelineController.timelineMaxTime * PixelsPerSecond, 0);
        editor.timelineSlider.MaxValue = editor.timelineController.timelineMaxTime;
    }

    public void OnTimeLineSliderChangedValue(float newTime)
    {
        // Обновляем глобальное время редактора значением из слайдера
        timelineTime = newTime;
    
        // Опционально: сразу обновляем текст, чтобы не ждать следующего кадра физики
        if (editor.GlobalTimeTextEdit != null)
            editor.GlobalTimeTextEdit.Text = Editor.TimeUtils.SecondsToMinutesString(timelineTime);
    }

    public void ApplyZoom(float factor)
    {
        // 1. Сохраняем "центр" зума (текущее время)
        float zoomCenterTime = timelineTime; 
    
        // 2. Вычисляем смещение относительно экрана, чтобы таймлайн не "прыгал"
        float screenOffset = (zoomCenterTime * PixelsPerSecond) - editor.HorScroll.ScrollHorizontal;

        // 3. Рассчитываем и ограничиваем новый масштаб
        float newPPS = PixelsPerSecond * factor;
        PixelsPerSecond = Mathf.Clamp(newPPS, 10f, 2000f); 

        // 4. Обновляем размеры контейнера таймлайна
        UpdateTimelineSize();

        // 5. Корректируем скролл для сохранения фокуса на времени
        float newScrollPos = (zoomCenterTime * PixelsPerSecond) - screenOffset;
        editor.HorScroll.ScrollHorizontal = (int)newScrollPos;

        // 6. Обновляем визуальное положение всех блоков
        UpdateAllBlocks();
    }
}
