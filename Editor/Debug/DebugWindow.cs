using Godot;
using System.Collections.Generic;
using System.Linq;
using LostEditor;
using System;

public partial class DebugWindow : Window
{
    // ── Ссылки на данные уровня ────────────────────────────────────────────
    [Export] public ObjectManager  objectManager;

    // ── Вкладка Performance ────────────────────────────────────────────────
    private GraphWidget _cpuGraph;
    private GraphWidget _gpuGraph;
    private Label       _cpuLabel;
    private Label       _gpuLabel;
    private Label       _fpsLabel;
    private Label       _memLabel;

    // ── Вкладка Profiler ───────────────────────────────────────────────────
    private VBoxContainer         _profilerRows;
    private Dictionary<string, (Label last, Label avg, Label min, Label max, GraphWidget graph)>
        _profilerUI = new();

    // ── Вкладка Scene Stats ────────────────────────────────────────────────
    private Label _objCountLabel;
    private Label _kfCountLabel;

    private float _updateTimer = 0f;
    private const float UpdateInterval = 0.1f; // обновляем UI каждые 100мс

    public override void _Ready()
    {
        CloseRequested += () => Hide();
        BuildUI();
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;

        _updateTimer += (float)delta;
        if (_updateTimer < UpdateInterval) return;
        _updateTimer = 0f;

        RefreshPerformance();
        RefreshProfiler();
        RefreshSceneStats();
    }

    // ── Открыть / закрыть (вызывать снаружи) ─────────────────────────────
    public void Toggle()
    {
        if (Visible) Hide();
        else         Show();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Построение UI
    // ═════════════════════════════════════════════════════════════════════════

    private void BuildUI()
    {
        Title          = "Debug Monitor";
        Size           = new Vector2I(720, 480);
        Unresizable    = false;

        var tabs = new TabContainer();
        tabs.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(tabs);

        tabs.AddChild(BuildPerformanceTab());
        tabs.AddChild(BuildProfilerTab());
        tabs.AddChild(BuildSceneStatsTab());

        tabs.SetTabTitle(0, "Performance");
        tabs.SetTabTitle(1, "Profiler");
        tabs.SetTabTitle(2, "Scene");
    }

    // ── Performance ────────────────────────────────────────────────────────
    private Control BuildPerformanceTab()
    {
        var root  = new VBoxContainer { Name = "Performance" };
        root.AddThemeConstantOverride("separation", 6);

        // Верхняя строка с цифрами
        var topRow = new HBoxContainer();
        _fpsLabel  = MakeLabel("FPS: --");
        _cpuLabel  = MakeLabel("CPU: --");
        _gpuLabel  = MakeLabel("GPU: --");
        _memLabel  = MakeLabel("MEM: --");
        topRow.AddChild(_fpsLabel);
        topRow.AddChild(_cpuLabel);
        topRow.AddChild(_gpuLabel);
        topRow.AddChild(_memLabel);
        root.AddChild(topRow);

        // Графики
        var graphRow = new HBoxContainer();
        graphRow.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        _cpuGraph = MakeGraph(new Color(0.3f, 0.7f, 1f), "ms", 33f);
        _gpuGraph = MakeGraph(new Color(0.9f, 0.5f, 0.2f), "ms", 33f);

        var cpuBox = WrapInLabeledBox("CPU Process", _cpuGraph);
        var gpuBox = WrapInLabeledBox("GPU Frame",   _gpuGraph);
        graphRow.AddChild(cpuBox);
        graphRow.AddChild(gpuBox);
        root.AddChild(graphRow);

        return root;
    }

    // ── Profiler ───────────────────────────────────────────────────────────
    private Control BuildProfilerTab()
    {
        var root = new VBoxContainer { Name = "Profiler" };

        // Заголовок таблицы
        var header = new HBoxContainer();
        header.AddChild(MakeHeaderLabel("Section",  160));
        header.AddChild(MakeHeaderLabel("Last ms",  70));
        header.AddChild(MakeHeaderLabel("Avg ms",   70));
        header.AddChild(MakeHeaderLabel("Min ms",   70));
        header.AddChild(MakeHeaderLabel("Max ms",   70));
        header.AddChild(MakeHeaderLabel("Graph",    200));
        root.AddChild(header);

        var separator = new HSeparator();
        root.AddChild(separator);

        // Скролл для строк
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _profilerRows = new VBoxContainer();
        scroll.AddChild(_profilerRows);
        root.AddChild(scroll);

        // Кнопка Reset
        var resetBtn = new Button { Text = "Reset All" };
        resetBtn.Pressed += () => DebugProfiler.ResetAll();
        root.AddChild(resetBtn);

        return root;
    }

    // ── Scene Stats ────────────────────────────────────────────────────────
    private Control BuildSceneStatsTab()
    {
        var root = new VBoxContainer { Name = "Scene" };
        root.AddThemeConstantOverride("separation", 8);

        _objCountLabel = MakeLabel("Objects: --");
        _kfCountLabel  = MakeLabel("Total Keyframes: --");

        root.AddChild(_objCountLabel);
        root.AddChild(_kfCountLabel);

        return root;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Обновление данных
    // ═════════════════════════════════════════════════════════════════════════

    private void RefreshPerformance()
    {
        float fps        = (float)Engine.GetFramesPerSecond();
        float cpuMs      = (float)(Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000.0);
        float drawCalls  = (float)Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
        float memMb      = (float)(Performance.GetMonitor(Performance.Monitor.MemoryStatic) / 1048576.0);

        _fpsLabel.Text = $"FPS: {fps:F0}";
        _cpuLabel.Text = $"CPU: {cpuMs:F2} ms";
        _gpuLabel.Text = $"Draw Calls: {drawCalls:F0}"; // ← вместо GPU ms
        _memLabel.Text = $"MEM: {memMb:F1} MB";

        _cpuGraph.Push(cpuMs);
        _gpuGraph.Push(drawCalls);
    }

    private void RefreshProfiler()
    {
        var all = DebugProfiler.GetAll();

        foreach (var kvp in all)
        {
            string name = kvp.Key;
            var    stat = kvp.Value;

            // Если строки ещё нет — создаём
            if (!_profilerUI.ContainsKey(name))
                CreateProfilerRow(name);

            var ui = _profilerUI[name];
            ui.last.Text = $"{stat.LastMs:F3}";
            ui.avg.Text  = $"{stat.AvgMs:F3}";
            ui.min.Text  = stat.MinMs == double.MaxValue ? "--" : $"{stat.MinMs:F3}";
            ui.max.Text  = $"{stat.MaxMs:F3}";
            ui.graph.Push((float)stat.LastMs);

            // Подсвечиваем красным если avg > 5ms
            Color c = stat.AvgMs > 5.0 ? new Color(1f, 0.4f, 0.4f) : new Color(0.85f, 0.85f, 0.85f);
            ui.avg.AddThemeColorOverride("font_color", c);
        }
    }

    private void RefreshSceneStats()
    {
        if (objectManager == null) return;

        int objCount = objectManager.objects.Count;
        int kfCount  = 0;
        foreach (var obj in objectManager.objects)
        {
            kfCount += obj.keyframePositionX.Count;
            kfCount += obj.keyframePositionY.Count;
            kfCount += obj.keyframeScaleX.Count;
            kfCount += obj.keyframeScaleY.Count;
            kfCount += obj.keyframeRotation.Count;
            kfCount += obj.keyframeColor.Count;
        }

        _objCountLabel.Text = $"Objects: {objCount}";
        _kfCountLabel.Text  = $"Total Keyframes: {kfCount}";
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Вспомогательные методы
    // ═════════════════════════════════════════════════════════════════════════

    private void CreateProfilerRow(string sectionName)
    {
        var row = new HBoxContainer();

        var nameLabel = MakeFixedLabel(sectionName, 160);
        var lastLabel = MakeFixedLabel("--",  70);
        var avgLabel  = MakeFixedLabel("--",  70);
        var minLabel  = MakeFixedLabel("--",  70);
        var maxLabel  = MakeFixedLabel("--",  70);

        var graph     = MakeGraph(new Color(0.4f, 0.9f, 0.6f), "ms", 16.6f);
        graph.CustomMinimumSize = new Vector2(200, 30);
        graph.AutoScale         = true;

        row.AddChild(nameLabel);
        row.AddChild(lastLabel);
        row.AddChild(avgLabel);
        row.AddChild(minLabel);
        row.AddChild(maxLabel);
        row.AddChild(graph);

        _profilerRows.AddChild(row);
        _profilerUI[sectionName] = (lastLabel, avgLabel, minLabel, maxLabel, graph);
    }

    private static GraphWidget MakeGraph(Color color, string unit, float maxVal)
    {
        return new GraphWidget
        {
            LineColor  = color,
            UnitLabel  = unit,
            MaxValue   = maxVal,
            AutoScale  = false,
            CustomMinimumSize = new Vector2(0, 80),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical   = Control.SizeFlags.ExpandFill,
        };
    }

    private static Control WrapInLabeledBox(string title, Control inner)
    {
        var box = new VBoxContainer();
        box.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        box.SizeFlagsVertical   = Control.SizeFlags.ExpandFill;
        box.AddChild(MakeLabel(title));
        inner.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        box.AddChild(inner);
        return box;
    }

    private static Label MakeLabel(string text) =>
        new Label { Text = text, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };

    private static Label MakeFixedLabel(string text, int width)
    {
        var l = new Label { Text = text };
        l.CustomMinimumSize = new Vector2(width, 0);
        return l;
    }

    private static Label MakeHeaderLabel(string text, int width)
    {
        var l = MakeFixedLabel(text, width);
        l.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        return l;
    }
}