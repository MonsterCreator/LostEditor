using Godot;
using System;

namespace LostEditor
{
    public partial class KeyframeAnimation : Node
    {
    }

    public partial class Keyframe
    {
        protected float _time { get; set; }
        protected EasingType _easingType { get; set; }

        public float Time => _time;
        public EasingType EasingType => _easingType;
    }

    public partial class KeyframePosX : Keyframe
    {
        public float X { get; set; }

        public KeyframePosX(float time, EasingType ease, float x)
        {
            _time = time;
            _easingType = ease;
            X = x;
        }
    }

    public partial class KeyframePosY : Keyframe
    {
        public float Y { get; set; }

        public KeyframePosY(float time, EasingType ease, float y)
        {
            _time = time;
            _easingType = ease;
            Y = y;
        }
    }

    // Новые кейфреймы
    public partial class KeyframeSizeX : Keyframe
    {
        public float X { get; set; }

        public KeyframeSizeX(float time, EasingType ease, float x)
        {
            _time = time;
            _easingType = ease;
            X = x;
        }
    }

    public partial class KeyframeSizeY : Keyframe
    {
        public float Y { get; set; }

        public KeyframeSizeY(float time, EasingType ease, float y)
        {
            _time = time;
            _easingType = ease;
            Y = y;
        }
    }

    public partial class KeyframeRotation : Keyframe
    {
        public float Rotation { get; set; }

        public KeyframeRotation(float time, EasingType ease, float rotation)
        {
            _time = time;
            _easingType = ease;
            Rotation = rotation;
        }
    }

    public partial class KeyframeColor : Keyframe
    {
        public Color Color { get; set; }

        public KeyframeColor(float time, EasingType ease, Color color)
        {
            _time = time;
            _easingType = ease;
            Color = color;
        }
    }
}