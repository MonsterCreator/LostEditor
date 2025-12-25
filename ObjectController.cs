using Godot;
using System;
using System.Collections.Generic;

public partial class ObjectController : Node2D
{
    public float time = 0f;
    public List<KeyframePosX> xK;
    public List<KeyframePosY> yK;

    public override void _Ready()
    {
        
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

    
}