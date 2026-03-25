using Godot;
using LostEditor;

public static class ShapeLibrary
{
    public static Vector2[] GetPolygon(ShapeType type, int circleSegments = 32)
    {
        return type switch
        {
            ShapeType.Square   => Square(),
            ShapeType.Triangle => Triangle(),
            ShapeType.Circle   => Circle(circleSegments),
            ShapeType.Hexagon  => RegularPolygon(6),
            _                  => Square()
        };
    }

    private static Vector2[] Square() => new[]
    {
        new Vector2(-10, -10),
        new Vector2( 10, -10),
        new Vector2( 10,  10),
        new Vector2(-10,  10),
    };

    private static Vector2[] Triangle() => new[]
    {
        new Vector2(  0, -10),
        new Vector2( 10,  10),
        new Vector2(-10,  10),
    };

    private static Vector2[] Circle(int segments)
    {
        var pts = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Tau * i / segments;
            pts[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 10f;
        }
        return pts;
    }

    private static Vector2[] RegularPolygon(int sides)
    {
        var pts = new Vector2[sides];
        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.Tau * i / sides - Mathf.Pi / 2f;
            pts[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 10f;
        }
        return pts;
    }
}