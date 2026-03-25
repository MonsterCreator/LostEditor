using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;

public partial class TimelineController : Node
{
    [Export] public ObjectManager objectManager;
    [Export] public AudioStreamPlayer MusicPlayer; // Ссылка на плеер

    [Export] public LineEdit GlobalTimeTextEdit;
    [Export] public LineEdit LocalTimeTextEdit;
    [Export] public TimelineSlider timelineSlider;
    [Export] public TimelineObjectController timeLineObjectControl;
    [Export] public VBoxContainer TimelineContainer;
    [Export] public ScrollContainer HorScroll;

    public Action<float> OnPPSChanged;
    private float _pixelsPerSecond;
    public float PixelsPerSecond
    {
        get => _pixelsPerSecond;
        set
        {
            _pixelsPerSecond = value;
            OnPPSChanged?.Invoke(value);
        }
    }
    [Export] public float timelineMaxTime = 60f;
    
    public float timelineTime = 0;
    private bool _isPlay = false;
    public float timelineSpeed = 1; // Примечание: AudioStreamPlayer не меняет скорость легко, обычно это 1.0

    public Action<float> OnSliderTimeChanged;

    public override void _Ready()
    {
        PixelsPerSecond = 100f;
        UpdateTimelineSize();
        // Если музыка загружена, можно автоматически выставить длину таймлайна
        if (MusicPlayer?.Stream != null)
        {
            timelineMaxTime = (float)MusicPlayer.Stream.GetLength();
            UpdateTimelineSize();
        }
    }

    public override void _Process(double delta)
    {
        // 1. Обработка ввода
        if (Input.IsActionJustPressed("Space")) TogglePlay();

        // 2. Обновление времени
        if (_isPlay)
        {
            if (MusicPlayer != null && MusicPlayer.Playing)
            {
                // Синхронизация с аудио (самый точный метод)
                float audioPos = (float)MusicPlayer.GetPlaybackPosition();
                audioPos += (float)AudioServer.GetTimeSinceLastMix();
                audioPos -= (float)AudioServer.GetOutputLatency();
                timelineTime = audioPos;
            }
            else if (MusicPlayer == null)
            {
                // Фолбэк на обычное время, если музыки нет
                timelineTime += (float)delta * timelineSpeed;
            }

            // Остановка по достижению конца
            if (timelineTime >= timelineMaxTime)
            {
                timelineTime = timelineMaxTime;
                GD.Print("останавливаем проигрывание трека");
                StopPlayback();
            }
        }

        // 3. Обновление всего UI и зависимых объектов
        RefreshVisuals();
    }

    // Вынес обновление графики в отдельный метод, чтобы вызывать его и из Process, и при перемотке
    private void RefreshVisuals()
    {
        if (objectManager != null) objectManager.time = timelineTime;

        if (GlobalTimeTextEdit != null)
            GlobalTimeTextEdit.Text = TimeUtils.SecondsToMinutesString(timelineTime);

        if (timelineSlider != null)
        {
            timelineSlider.Value = timelineTime;
            timelineSlider.line.Position = new Vector2(timelineTime * PixelsPerSecond, 0);
        }

        // Обновление локального времени выделенного блока
        var selectedBlock = timeLineObjectControl.GetSelectedBlock();
        if (selectedBlock != null && LocalTimeTextEdit != null)
        {
            float localTime = timelineTime - selectedBlock.Data.startTime;
            LocalTimeTextEdit.Text = TimeUtils.SecondsToMinutesString(localTime);
        }
    }

    public void TogglePlay()
    {
        GD.Print("переключение проигрывания");
        if (_isPlay) StopPlayback();
        else StartPlayback();
    }

    private void StartPlayback()
    {
        _isPlay = true;
        if (MusicPlayer != null)
        {
            // Запускаем музыку ровно с того места, где стоит ползунок
            MusicPlayer.Play(timelineTime);
        }
    }

    private void StopPlayback()
    {
        _isPlay = false;
        MusicPlayer?.Stop();
    }

    public void OnTimeLineSliderChangedValue(float newTime)
    {
        // Рассчитываем разницу между текущим звуком и новым временем
        float diff = Math.Abs(timelineTime - newTime);

        timelineTime = newTime;

        // Если музыка играет и разница больше 0.1 сек (пользователь дернул слайдер)
        // Это предотвратит вызов Play() каждый кадр, который и вызывал треск
        if (_isPlay && MusicPlayer != null && diff > 0.1f)
        {
            MusicPlayer.Play(newTime);
        }

        OnSliderTimeChanged?.Invoke(newTime);
        RefreshVisuals();
    }

    // Остальные ваши методы (ApplyZoom, UpdateTimelineSize и т.д.) остаются без изменений
    public void UpdateTimelineSize()
    {
        TimelineContainer.CustomMinimumSize = new Vector2(timelineMaxTime * PixelsPerSecond, 0);
        timelineSlider.MaxValue = timelineMaxTime;
    }

    public void UpdateAllBlocks() => timeLineObjectControl.activeBlocks.ForEach(b => b.UpdateVisual());

    public void ApplyZoom(float factor)
    {
        float zoomCenterTime = timelineTime; 
        float screenOffset = (zoomCenterTime * PixelsPerSecond) - HorScroll.ScrollHorizontal;
        float newPPS = PixelsPerSecond * factor;
        PixelsPerSecond = Mathf.Clamp(newPPS, 10f, 2000f); 

        UpdateTimelineSize();

        float newScrollPos = (zoomCenterTime * PixelsPerSecond) - screenOffset;
        HorScroll.ScrollHorizontal = (int)newScrollPos;

        UpdateAllBlocks();
    }


}