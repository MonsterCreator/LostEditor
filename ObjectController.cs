using Godot;
using System;
using System.Collections.Generic;

public partial class ObjectController : Node2D
{
    public float time = 0f;
    public List<KeyframeX> xK;
    public List<KeyframeY> yK;

    public override void _Ready()
    {
        xK = new List<KeyframeX>();
        yK = new List<KeyframeY>();

        // Linear keyframe for initial position
        xK.Add(new KeyframeX(0f, EasingType.Linear, 0f));
        yK.Add(new KeyframeY(0f, EasingType.Linear, 0f));

        float ttt = 0.1f;
        Random rnd = new();
        var easeT = Enum.GetValues(typeof(EasingType));

        for (int i = 0; i < 16; i++)
        {
            EasingType easeT2 = (EasingType)easeT.GetValue(i);
            for (int j = 0; j < 10; j++)
            {
                float x = rnd.Next(-200, 200);
                float y = rnd.Next(-200, 200);
                xK.Add(new KeyframeX(ttt, easeT2, x));
                yK.Add(new KeyframeY(ttt, easeT2, y));
                ttt += 0.5f;
            }
            
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float d = (float)delta;
        PositionObjectUpdate();
        time += d;
    }

    public void PositionObjectUpdate()
    {
        if (xK.Count == 0 || yK.Count == 0) return;

        int index = BinarySearchForTime(xK, time);

        if (index < 0)
        {
            Position = new Vector2(xK[0].X, yK[0].Y);
            return;
        }

        if (index >= xK.Count - 1)
        {
            Position = new Vector2(xK[xK.Count - 1].X, yK[yK.Count - 1].Y);
            return;
        }

        var leftX = xK[index];
        var rightX = xK[index + 1];
        var leftY = yK[index];
        var rightY = yK[index + 1];

        float t = (time - leftX.Time) / (rightX.Time - leftX.Time);
        float easedT = Ease(t, leftX.EasingType);

        float interpolatedX = EasingFunctions.Lerp(leftX.X, rightX.X, easedT);
        float interpolatedY = EasingFunctions.Lerp(leftY.Y, rightY.Y, easedT);
        Position = new Vector2(interpolatedX, interpolatedY);
    }

    private int BinarySearchForTime<T>(List<T> list, float t) where T : Keyframe
    {
        int left = 0;
        int right = list.Count - 1;
        int result = -1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            if (list[mid].Time <= t)
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return result;
    }

    private float Ease(float t, EasingType easingType)
    {
        switch (easingType)
        {
            case EasingType.Linear:
                return EasingFunctions.Linear(t);
            case EasingType.Instant:
                return EasingFunctions.Instant(t);
            case EasingType.InSine:
                return EasingFunctions.InSine(t);
            case EasingType.OutSine:
                return EasingFunctions.OutSine(t);
            case EasingType.InOutSine:
                return EasingFunctions.InOutSine(t);
            case EasingType.InCirc:
                return EasingFunctions.InCirc(t);
            case EasingType.OutCirc:
                return EasingFunctions.OutCirc(t);
            case EasingType.InOutCirc:
                return EasingFunctions.InOutCirc(t);
            case EasingType.InQuad:
                return EasingFunctions.InQuad(t);
            case EasingType.OutQuad:
                return EasingFunctions.OutQuad(t);
            case EasingType.InOutQuad:
                return EasingFunctions.InOutQuad(t);
            case EasingType.InExpo:
                return EasingFunctions.InExpo(t);
            case EasingType.OutExpo:
                return EasingFunctions.OutExpo(t);
            case EasingType.InOutExpo:
                return EasingFunctions.InOutExpo(t);
            case EasingType.InElastic:
                return EasingFunctions.InElastic(t);
            case EasingType.OutElastic:
                return EasingFunctions.OutElastic(t);
            case EasingType.InOutElastic:
                return EasingFunctions.InOutElastic(t);
            default:
                return t;
        }
    }

    public class Keyframe
    {
        protected float _time { get; set; }
        protected EasingType _easingType { get; set; }

        public float Time => _time;
        public EasingType EasingType => _easingType;
    }

    public class KeyframeX : Keyframe
    {
        public float X { get; set; }

        public KeyframeX(float time, EasingType ease, float x)
        {
            _time = time;
            _easingType = ease;
            X = x;
        }
    }

    public class KeyframeY : Keyframe
    {
        public float Y { get; set; }

        public KeyframeY(float time, EasingType ease, float y)
        {
            _time = time;
            _easingType = ease;
            Y = y;
        }
    }

    public enum EasingType
    {
        Linear,
        Instant,
        InSine,
        OutSine,
        InOutSine,
        InCirc,
        OutCirc,
        InOutCirc,
        InQuad,
        OutQuad,
        InOutQuad,
        InExpo,
        OutExpo,
        InOutExpo,
        InElastic,
        OutElastic,
        InOutElastic
    }

    public static class EasingFunctions
    {
        public static float Linear(float t) => t;
        public static float Instant(float t) => t >= 1f ? 1f : 0f;
        
        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1 - InQuad(1 - t);
        public static float InOutQuad(float t)
        {
            if (t < 0.5f) return InQuad(t * 2) / 2;
            return 1 - InQuad((1 - t) * 2) / 2;
        }

        public static float InCubic(float t) => t * t * t;
        public static float OutCubic(float t) => 1 - InCubic(1 - t);
        public static float InOutCubic(float t)
        {
            if (t < 0.5f) return InCubic(t * 2) / 2;
            return 1 - InCubic((1 - t) * 2) / 2;
        }

        public static float InQuart(float t) => t * t * t * t;
        public static float OutQuart(float t) => 1 - InQuart(1 - t);
        public static float InOutQuart(float t)
        {
            if (t < 0.5f) return InQuart(t * 2) / 2;
            return 1 - InQuart((1 - t) * 2) / 2;
        }

        public static float InQuint(float t) => t * t * t * t * t;
        public static float OutQuint(float t) => 1 - InQuint(1 - t);
        public static float InOutQuint(float t)
        {
            if (t < 0.5f) return InQuint(t * 2) / 2;
            return 1 - InQuint((1 - t) * 2) / 2;
        }

        public static float InSine(float t) => 1 - (float)Math.Cos(t * Math.PI / 2);
        public static float OutSine(float t) => (float)Math.Sin(t * Math.PI / 2);
        public static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;

        public static float InExpo(float t) => (float)Math.Pow(2, 10 * (t - 1));
        public static float OutExpo(float t) => 1 - InExpo(1 - t);
        public static float InOutExpo(float t)
        {
            if (t < 0.5f) return InExpo(t * 2) / 2;
            return 1 - InExpo((1 - t) * 2) / 2;
        }

        public static float InCirc(float t) => -((float)Math.Sqrt(1 - t * t) - 1);
        public static float OutCirc(float t) => 1 - InCirc(1 - t);
        public static float InOutCirc(float t)
        {
            if (t < 0.5f) return InCirc(t * 2) / 2;
            return 1 - InCirc((1 - t) * 2) / 2;
        }

        public static float InElastic(float t) => 1 - OutElastic(1 - t);
        public static float OutElastic(float t)
        {
            float p = 0.3f;
            return (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - p / 4) * (2 * Math.PI) / p) + 1;
        }
        public static float InOutElastic(float t)
        {
            if (t < 0.5f) return InElastic(t * 2) / 2;
            return 1 - InElastic((1 - t) * 2) / 2;
        }

        public static float InBack(float t)
        {
            float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }
        public static float OutBack(float t) => 1 - InBack(1 - t);
        public static float InOutBack(float t)
        {
            if (t < 0.5f) return InBack(t * 2) / 2;
            return 1 - InBack((1 - t) * 2) / 2;
        }

        public static float InBounce(float t) => 1 - OutBounce(1 - t);
        public static float OutBounce(float t)
        {
            float div = 2.75f;
            float mult = 7.5625f;

            if (t < 1 / div)
            {
                return mult * t * t;
            }
            else if (t < 2 / div)
            {
                t -= 1.5f / div;
                return mult * t * t + 0.75f;
            }
            else if (t < 2.5 / div)
            {
                t -= 2.25f / div;
                return mult * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / div;
                return mult * t * t + 0.984375f;
            }
        }
        public static float InOutBounce(float t)
        {
            if (t < 0.5f) return InBounce(t * 2) / 2;
            return 1 - InBounce((1 - t) * 2) / 2;
        }

        // Добавлен метод Lerp для скалярной интерполяции
        public static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}