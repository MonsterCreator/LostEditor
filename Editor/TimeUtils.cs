using Godot;
using System;

namespace LostEditor;

public partial class TimeUtils : Node
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


