using Godot;
using System;
using LostEditor;

//TimelineController является основой таймлайна. Здесь идёт обработка времени
public partial class TimelineController : Node
{
    [Export] public ObjectManager objectManager;

    [Export] public LineEdit GlobalTimeTextEdit;
    [Export] public LineEdit LocalTimeTextEdit;
    [Export] public TimelineSlider timelineSlider;
    [Export] public TimelineObjectController timeLineObjectControl;
    [Export] public VBoxContainer TimelineContainer;
    [Export] public ScrollContainer HorScroll;

    [Export] public float PixelsPerSecond = 100f;

    [Export] public float timelineMaxTime = 60f;
    public float timelineTime = 0;
    private bool _isPlay = false;

    public Action<float> OnSliderTimeChanged;
    public float timelineSpeed = 1;

    //[Export] public Editor editor;
    [Export] public float maxTime;

    public override void _Ready() => UpdateTimelineSize();

    
    public override void _PhysicsProcess(double delta)
    {

        if (_isPlay) timelineTime += (float)delta * timelineSpeed;
        if (objectManager != null) objectManager.time = timelineTime;

        GlobalTimeTextEdit.Text = TimeUtils.SecondsToMinutesString(timelineTime);
        timelineSlider.Value = timelineTime;
        var scriptSlider = timelineSlider;
        scriptSlider.line.Position = new Vector2(timelineTime * PixelsPerSecond,0);
        
        if (Input.IsActionJustPressed("Space")) _isPlay = !_isPlay;
    }

    public void CreateObjectButtonPressed()
    {
        timeLineObjectControl.CreateObject();
    }

    public void UpdateAllBlocks() => timeLineObjectControl.activeBlocks.ForEach(b => b.UpdateVisual(PixelsPerSecond));

    public void UpdateTimelineSize()
    {
        TimelineContainer.CustomMinimumSize = new Vector2(timelineMaxTime * PixelsPerSecond, 0);
        timelineSlider.MaxValue = timelineMaxTime;
    }

    public void OnTimeLineSliderChangedValue(float newTime)
    {
        // Обновляем глобальное время редактора значением из слайдера
        timelineTime = newTime;
        OnSliderTimeChanged(newTime);

        
        // Опционально: сразу обновляем текст, чтобы не ждать следующего кадра физики
        if (GlobalTimeTextEdit != null) GlobalTimeTextEdit.Text = TimeUtils.SecondsToMinutesString(timelineTime);
        if(timeLineObjectControl.GetSelectedBlock() != null)
        {
            float localTime = timelineTime - timeLineObjectControl.GetSelectedBlock().Data.startTime;
            if (LocalTimeTextEdit != null) LocalTimeTextEdit.Text = TimeUtils.SecondsToMinutesString(localTime);
        }
        
            
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
}
