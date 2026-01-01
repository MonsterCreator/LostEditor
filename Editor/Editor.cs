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
    [ExportGroup("Modules")]
    [Export] public TimelineController TimeCtrl;
    [Export] public TrackManager Tracks;
    [Export] public SelectionSystem Selection;
    [Export] public InputHandler InputSys;
    [Export] public EditorUI UI;
    
    [Signal]
    public delegate void PpsChangedEventHandler(float newPps);

    // 1. Добавляем ссылку на кнопку (назначьте её в Инспекторе!)
    [Export] public Button CreateObjectBtn; 

    public override void _Ready()
    {
        // ... существующий код связывания времени ...
        TimeCtrl.TimeChanged += UI.OnTimeChanged;
        TimeCtrl.ZoomChanged += UI.OnZoomChanged;
        TimeCtrl.ZoomChanged += Tracks.RedrawBlocks;

        if (UI.TimelineSlider != null)
        {
            UI.TimelineSlider.ValueChanged += (val) => { TimeCtrl.SetTime((float)val); };
        }

        UI.OnZoomChanged(TimeCtrl.PixelsPerSecond);
        TimeCtrl.SetTime(0);

        // --- НОВЫЙ КОД ---
        // 2. Передаем ссылку на редактор в TrackManager
        if (Tracks != null) Tracks.EditorRef = this;


        
    }

    public void CreateObjectButtonPressed()
    {
        Tracks.CreateBlock(TimeCtrl.CurrentTime);
        Tracks.RedrawBlocks(TimeCtrl.PixelsPerSecond);
    }
    
    public void ApplyZoom(float factor)
    {
        // 1. Рассчитываем новый зум на основе текущего из TimeCtrl
        float newPPS = TimeCtrl.PixelsPerSecond * factor;

        // 2. Устанавливаем его в контроллер времени (это изменит масштаб)
        TimeCtrl.SetZoom(newPPS);

        // 3. Испускаем ваш новый сигнал (если он нужен другим компонентам)
        EmitSignal(SignalName.PpsChanged, TimeCtrl.PixelsPerSecond);

        // 4. Явно уведомляем UI и Треки, так как автоматическая подписка 
        // иногда срабатывает ПОСЛЕ того, как данные уже нужны.
        UI.OnZoomChanged(TimeCtrl.PixelsPerSecond);
        Tracks.RedrawBlocks(TimeCtrl.PixelsPerSecond);
    }
}

