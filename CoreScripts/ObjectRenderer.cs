using Godot;
using System.Collections.Generic;
using LostEditor;

public partial class ObjectRenderer : MeshInstance2D
{
    [Export] public ObjectManager objectManager;



    private ArrayMesh _mesh;
    private bool _isDirty = true;
    public void MarkDirty() => _isDirty = true;

    // Кеш триангуляции — пересчитывается только при смене формы
    private class ObjectCache
    {
        public int[]     Indices;
        public Vector2[] Polygon;
        public ShapeType ShapeType;
        public int       CircleSegments;
    }
    private readonly Dictionary<GameObject, ObjectCache> _cache = new();

    public override void _Ready()
    {
        _mesh = new ArrayMesh();
        Mesh  = _mesh;
        Material = new CanvasItemMaterial();
    }

    public override void _Process(double delta)
    {
        if (!_isDirty) return;
        _isDirty = false;
        RebuildMesh();
    }


    private void RebuildMesh()
    {
        if (objectManager == null || objectManager.objects.Count == 0)
        {
            if (_mesh.GetSurfaceCount() > 0) _mesh.ClearSurfaces();
            return;
        }

        DebugProfiler.Begin("Renderer.ClearSurfaces");
        _mesh.ClearSurfaces();
        DebugProfiler.End("Renderer.ClearSurfaces");

        var vertices = new List<Vector3>();
        var colors   = new List<Color>();

        DebugProfiler.Begin("Renderer.BuildArrays");
        foreach (var obj in objectManager.objects)
        {
            if (!obj.Visible) continue;

            if (!_cache.TryGetValue(obj, out var cache) || ShapeChanged(obj, cache))
            {
                Vector2[] polygon = obj.ShapeType == ShapeType.Custom
                    ? obj.CustomPolygon
                    : ShapeLibrary.GetPolygon(obj.ShapeType, obj.CircleSegments);

                cache = new ObjectCache
                {
                    Indices        = Geometry2D.TriangulatePolygon(polygon),
                    Polygon        = polygon,
                    ShapeType      = obj.ShapeType,
                    CircleSegments = obj.CircleSegments
                };
                _cache[obj] = cache;
            }

            if (cache.Indices == null || cache.Indices.Length == 0) continue;

            var transform = obj.GlobalTransform;
            for (int i = 0; i < cache.Indices.Length; i += 3)
            {
                vertices.Add(ToV3(transform * cache.Polygon[cache.Indices[i]]));
                vertices.Add(ToV3(transform * cache.Polygon[cache.Indices[i + 1]]));
                vertices.Add(ToV3(transform * cache.Polygon[cache.Indices[i + 2]]));
                colors.Add(obj.Color);
                colors.Add(obj.Color);
                colors.Add(obj.Color);
            }
        }
        DebugProfiler.End("Renderer.BuildArrays");

        if (vertices.Count == 0) return;

        DebugProfiler.Begin("Renderer.UploadToGPU");
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.Color]  = colors.ToArray();
        _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        DebugProfiler.End("Renderer.UploadToGPU");
    }

    // Удаляем кеш при удалении объекта
    public void RemoveFromCache(GameObject obj) => _cache.Remove(obj);

    private static bool ShapeChanged(GameObject obj, ObjectCache cache) =>
        obj.ShapeType      != cache.ShapeType ||
        obj.CircleSegments != cache.CircleSegments ||
        (obj.ShapeType == ShapeType.Custom && obj.CustomPolygon != cache.Polygon);

    private static Vector3 ToV3(Vector2 v) => new Vector3(v.X, v.Y, 0);
}