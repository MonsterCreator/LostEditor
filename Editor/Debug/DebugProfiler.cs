using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LostEditor;

public static class DebugProfiler
{
    private const int MaxSamples = 120; // скользящее окно в кадрах

    public class SectionStats
    {
        public double LastMs;
        public double AvgMs;
        public double MinMs = double.MaxValue;
        public double MaxMs;
        public readonly Queue<double> Samples = new(MaxSamples);
    }

    private static readonly Dictionary<string, Stopwatch>    _active  = new();
    private static readonly Dictionary<string, SectionStats> _stats   = new();

    // ── Публичный API ──────────────────────────────────────────────────────

    public static void Begin(string section)
    {
        if (!_active.TryGetValue(section, out var sw))
        {
            sw = new Stopwatch();
            _active[section] = sw;
        }
        sw.Restart();
    }

    public static void End(string section)
    {
        if (!_active.TryGetValue(section, out var sw) || !sw.IsRunning) return;
        sw.Stop();

        double ms = sw.Elapsed.TotalMilliseconds;

        if (!_stats.TryGetValue(section, out var stat))
        {
            stat = new SectionStats();
            _stats[section] = stat;
        }

        stat.LastMs = ms;
        stat.MinMs  = Math.Min(stat.MinMs, ms);
        stat.MaxMs  = Math.Max(stat.MaxMs, ms);

        stat.Samples.Enqueue(ms);
        if (stat.Samples.Count > MaxSamples) stat.Samples.Dequeue();

        // Пересчёт среднего
        double sum = 0;
        foreach (var s in stat.Samples) sum += s;
        stat.AvgMs = sum / stat.Samples.Count;
    }

    public static IReadOnlyDictionary<string, SectionStats> GetAll() => _stats;

    public static void Reset(string section)
    {
        if (_stats.TryGetValue(section, out var stat))
        {
            stat.Samples.Clear();
            stat.MinMs = double.MaxValue;
            stat.MaxMs = 0;
        }
    }

    public static void ResetAll()
    {
        foreach (var key in _stats.Keys) Reset(key);
    }

	// Добавляет время к уже существующему замеру вместо перезаписи
	public static void Add(string section, double ms)
	{
		if (!_stats.TryGetValue(section, out var stat))
		{
			stat = new SectionStats();
			_stats[section] = stat;
		}

		stat.LastMs = ms;
		stat.MinMs  = Math.Min(stat.MinMs, ms);
		stat.MaxMs  = Math.Max(stat.MaxMs, ms);

		stat.Samples.Enqueue(ms);
		if (stat.Samples.Count > MaxSamples) stat.Samples.Dequeue();

		double sum = 0;
		foreach (var s in stat.Samples) sum += s;
		stat.AvgMs = sum / stat.Samples.Count;
	}

	// Накопительный Begin/End — суммирует время за кадр
	private static readonly Dictionary<string, double> _accumulated = new();

	public static void BeginAccum(string section)
	{
		if (!_active.TryGetValue(section, out var sw))
		{
			sw = new Stopwatch();
			_active[section] = sw;
		}
		sw.Restart();
	}

	public static void EndAccum(string section)
	{
		if (!_active.TryGetValue(section, out var sw) || !sw.IsRunning) return;
		sw.Stop();

		double ms = sw.Elapsed.TotalMilliseconds;
		if (_accumulated.ContainsKey(section))
			_accumulated[section] += ms;
		else
			_accumulated[section] = ms;
	}

	// Вызывать ОДИН РАЗ в конце кадра чтобы зафиксировать накопленное
	public static void FlushAccumulated()
	{
		foreach (var kvp in _accumulated)
			Add(kvp.Key, kvp.Value);
		_accumulated.Clear();
	}
}