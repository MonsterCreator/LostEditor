using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LostEditor
{
    public partial class ObjectManager : Node2D
    {
        public float time = 0f;
        public List<GameObject> objects = new List<GameObject>();

        [Export] public LevelColorData LevelColorData { get; set; }
        [Export] public TimelineController timelineController;
        [Export] public Polygon2D polygonObject;
        [Export] public CollisionPolygon2D collisionPlygonObject;
        [Export] public DebugEditorManager debugEditorManager;
        [Export] public ObjectRenderer objectRenderer;

        public List<ObjectColor> levelColors = new();

        private float _prevTime = 0f;
        private List<GameObject> _sortedByStart = new();
        private bool _indexDirty = true;

        public override void _Ready() { }

        public void RegisterObject(GameObject obj)
        {
            if (!objects.Contains(obj))
            {
                objects.Add(obj);
                obj.OnDataChanged += () => _indexDirty = true;
                _indexDirty = true;
            }
        }

        public void UnregisterObject(GameObject obj)
        {
            objects.Remove(obj);
            _sortedByStart.Remove(obj);
            _indexDirty = true;
        }

        private void RebuildSortedIndex()
        {
            _sortedByStart = new List<GameObject>(objects);
            _sortedByStart.Sort((a, b) => a.startTime.CompareTo(b.startTime));
            _indexDirty = false;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_indexDirty) RebuildSortedIndex();

            DebugProfiler.Begin("Objects.Animation");

            var results = new AnimationResult[_sortedByStart.Count];

            Parallel.For(0, _sortedByStart.Count, i =>
            {
                var obj = _sortedByStart[i];

                if (obj.startTime > time)
                {
                    results[i] = new AnimationResult { ShouldBeVisible = false };
                    return;
                }

                float localTime     = time - obj.startTime;
                float prevLocalTime = _prevTime - obj.startTime;

                float globalEnd      = obj.startTime + obj.cachedEndTime;
                bool shouldBeVisible = time >= obj.startTime && time < globalEnd;

                if (!shouldBeVisible)
                {
                    results[i] = new AnimationResult { ShouldBeVisible = false };
                    return;
                }

                var cache = obj.animCache;

                if (cache.IsDirty)
                {
                    cache.IndexPosX = cache.IndexPosY = cache.IndexScaleX =
                    cache.IndexScaleY = cache.IndexRotation = cache.IndexColor = 0;
                    cache.IsDirty = false;
                }

                results[i] = new AnimationResult
                {
                    ShouldBeVisible = true,
                    Position        = CalculatePosition(obj, localTime, prevLocalTime, cache),
                    Scale           = CalculateScale(obj, localTime, prevLocalTime, cache),
                    Rotation        = CalculateRotation(obj, localTime, prevLocalTime, cache),
                    Color           = CalculateColor(obj, localTime, prevLocalTime, cache),
                };
            });

            bool anyChanged = false;
            for (int i = 0; i < _sortedByStart.Count; i++)
            {
                var obj    = _sortedByStart[i];
                var result = results[i];

                if (obj.Visible != result.ShouldBeVisible)
                {
                    obj.Visible = result.ShouldBeVisible;
                    if (result.ShouldBeVisible) obj.animCache.IsDirty = true;
                    anyChanged = true;
                }

                if (!result.ShouldBeVisible) continue;

                if (obj.Position != result.Position ||
                    obj.Scale    != result.Scale    ||
                    obj.Rotation != result.Rotation ||
                    obj.Color    != result.Color)
                {
                    obj.Position = result.Position;
                    obj.Scale    = result.Scale;
                    obj.Rotation = result.Rotation;
                    obj.Color    = result.Color;
                    anyChanged   = true;
                }
            }

            if (anyChanged) objectRenderer?.MarkDirty();

            DebugProfiler.FlushAccumulated();
            _prevTime = time;
            DebugProfiler.End("Objects.Animation");
        }

        // ── Инкрементальный поиск ─────────────────────────────────────────
        private int GetCurrentIndex<T>(List<T> list, ref int cachedIndex, float time, float prevTime)
            where T : IKeyframe
        {
            if (list.Count == 0) return -1;

            bool isRewind = time < prevTime;

            if (isRewind)
            {
                while (cachedIndex > 0 && list[cachedIndex].Time > time)
                    cachedIndex--;
            }
            else
            {
                while (cachedIndex < list.Count - 1 && list[cachedIndex + 1].Time <= time)
                    cachedIndex++;
            }

            cachedIndex = Mathf.Clamp(cachedIndex, 0, list.Count - 1);

            // Если даже первый кейфрейм ещё не наступил — возвращаем -1
            if (list[cachedIndex].Time > time) return -1;

            return cachedIndex;
        }

        // ── ObjectStateUpdate ─────────────────────────────────────────────
        public void ObjectStateUpdate(GameObject gameObject)
        {
            float globalStart = gameObject.startTime;
            float globalEnd   = gameObject.startTime + gameObject.cachedEndTime;

            bool shouldBeVisible = time >= globalStart && time < globalEnd;

            if (gameObject.Visible != shouldBeVisible)
            {
                gameObject.Visible = shouldBeVisible;

                // Объект стал видимым — сбрасываем кеш,
                // иначе индексы останутся от предыдущего проигрывания
                if (shouldBeVisible)
                    gameObject.animCache.IsDirty = true;
            }
        }

        // ── Position ──────────────────────────────────────────────────────
        private Vector2 CalculatePosition(GameObject obj, float localTime, float prevLocalTime, GameObject.AnimationCache cache)
        {
            // Не читаем obj.Position — это свойство Node, запрещено из потока
            float finalX = 0f;
            float finalY = 0f;

            if (obj.keyframePositionX.Count > 0)
            {
                int indexX = GetCurrentIndex(obj.keyframePositionX, ref cache.IndexPosX, localTime, prevLocalTime);
                if (indexX < 0)
                    finalX = obj.keyframePositionX[0].Value;
                else if (indexX >= obj.keyframePositionX.Count - 1)
                    finalX = obj.keyframePositionX[^1].Value;
                else
                {
                    var left  = obj.keyframePositionX[indexX];
                    var right = obj.keyframePositionX[indexX + 1];
                    float t   = (localTime - left.Time) / (right.Time - left.Time);
                    finalX    = EasingFunctions.Lerp(left.Value, right.Value, EasingFunctions.Ease(t, right.EasingType));
                }
            }

            if (obj.keyframePositionY.Count > 0)
            {
                int indexY = GetCurrentIndex(obj.keyframePositionY, ref cache.IndexPosY, localTime, prevLocalTime);
                if (indexY < 0)
                    finalY = obj.keyframePositionY[0].Value;
                else if (indexY >= obj.keyframePositionY.Count - 1)
                    finalY = obj.keyframePositionY[^1].Value;
                else
                {
                    var left  = obj.keyframePositionY[indexY];
                    var right = obj.keyframePositionY[indexY + 1];
                    float t   = (localTime - left.Time) / (right.Time - left.Time);
                    finalY    = EasingFunctions.Lerp(left.Value, right.Value, EasingFunctions.Ease(t, right.EasingType));
                }
            }

            return new Vector2(finalX, -finalY);
        }

        private Vector2 CalculateScale(GameObject obj, float localTime, float prevLocalTime, GameObject.AnimationCache cache)
        {
            // Не читаем obj.Scale — запрещено из потока
            float finalScaleX = 1f;
            float finalScaleY = 1f;

            if (obj.keyframeScaleX.Count > 0)
            {
                int indexX = GetCurrentIndex(obj.keyframeScaleX, ref cache.IndexScaleX, localTime, prevLocalTime);
                if (indexX < 0)
                    finalScaleX = obj.keyframeScaleX[0].Value;
                else if (indexX >= obj.keyframeScaleX.Count - 1)
                    finalScaleX = obj.keyframeScaleX[^1].Value;
                else
                {
                    var left    = obj.keyframeScaleX[indexX];
                    var right   = obj.keyframeScaleX[indexX + 1];
                    float t     = (localTime - left.Time) / (right.Time - left.Time);
                    finalScaleX = EasingFunctions.Lerp(left.Value, right.Value, EasingFunctions.Ease(t, right.EasingType));
                }
            }

            if (obj.keyframeScaleY.Count > 0)
            {
                int indexY = GetCurrentIndex(obj.keyframeScaleY, ref cache.IndexScaleY, localTime, prevLocalTime);
                if (indexY < 0)
                    finalScaleY = obj.keyframeScaleY[0].Value;
                else if (indexY >= obj.keyframeScaleY.Count - 1)
                    finalScaleY = obj.keyframeScaleY[^1].Value;
                else
                {
                    var left    = obj.keyframeScaleY[indexY];
                    var right   = obj.keyframeScaleY[indexY + 1];
                    float t     = (localTime - left.Time) / (right.Time - left.Time);
                    finalScaleY = EasingFunctions.Lerp(left.Value, right.Value, EasingFunctions.Ease(t, right.EasingType));
                }
            }

            return new Vector2(finalScaleX, finalScaleY);
        }

        private float CalculateRotation(GameObject obj, float localTime, float prevLocalTime, GameObject.AnimationCache cache)
        {
            // Не читаем obj.Rotation — запрещено из потока
            if (obj.keyframeRotation.Count == 0) return 0f;

            int index = GetCurrentIndex(obj.keyframeRotation, ref cache.IndexRotation, localTime, prevLocalTime);

            if (index < 0)
                return Mathf.DegToRad(obj.keyframeRotation[0].Value);

            if (index >= obj.keyframeRotation.Count - 1)
                return Mathf.DegToRad(obj.keyframeRotation[^1].Value);

            var left     = obj.keyframeRotation[index];
            var right    = obj.keyframeRotation[index + 1];
            float t      = (localTime - left.Time) / (right.Time - left.Time);
            float easedT = EasingFunctions.Ease(t, right.EasingType);

            return Mathf.LerpAngle(Mathf.DegToRad(left.Value), Mathf.DegToRad(right.Value), easedT);
        }

        private Color CalculateColor(GameObject obj, float localTime, float prevLocalTime, GameObject.AnimationCache cache)
        {
            // Не читаем obj.Color — запрещено из потока
            if (obj.keyframeColor.Count == 0) return Colors.White;

            int index = GetCurrentIndex(obj.keyframeColor, ref cache.IndexColor, localTime, prevLocalTime);

            if (index < 0)
            {
                var first = obj.keyframeColor[0].Value;
                return first != null ? first.color : Colors.White;
            }

            if (index >= obj.keyframeColor.Count - 1)
            {
                var last = obj.keyframeColor[^1].Value;
                return last != null ? last.color : Colors.White;
            }

            var leftKf   = obj.keyframeColor[index];
            var rightKf  = obj.keyframeColor[index + 1];
            var leftVal  = leftKf.Value;
            var rightVal = rightKf.Value;

            if (leftVal == null || rightVal == null)
                return leftVal?.color ?? rightVal?.color ?? Colors.White;

            float tt     = (localTime - leftKf.Time) / (rightKf.Time - leftKf.Time);
            float easedT = EasingFunctions.Ease(tt, rightKf.EasingType);
            return leftVal.color.Lerp(rightVal.color, easedT);
        }

        // Вызывать при изменении LevelColorData
        public void RestoreAllLevelColors()
        {
            if (LevelColorData == null) return;
            foreach (var obj in objects)
                foreach (var kf in obj.keyframeColor)
                    kf.Value?.RestoreBaseLevelColor(LevelColorData);
        }

        // BinarySearchForTime оставляем — используется в ColorManager
        private int BinarySearchForTime<T>(List<T> list, float t) where T : IKeyframe
        {
            int left = 0, right = list.Count - 1, result = -1;
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (list[mid].Time <= t) { result = mid; left = mid + 1; }
                else right = mid - 1;
            }
            return result;
        }

        public void KeyframePosGenerator(int nums, ref List<Keyframe<float>> xK, ref List<Keyframe<float>> yK)
        {
            xK.Add(new Keyframe<float>() { Time = 0f, EasingType = EasingType.Linear, Value = 0f, kType = KeyframeType.PositionX });
            yK.Add(new Keyframe<float>() { Time = 0f, EasingType = EasingType.Linear, Value = 0f, kType = KeyframeType.PositionY });

            float ttt = 0.1f;
            Random rnd = new();
            var easeT = Enum.GetValues(typeof(EasingType));

            for (int i = 0; i < 4; i++)
            {
                EasingType easeT2 = (EasingType)easeT.GetValue(i);
                for (int j = 0; j < nums; j++)
                {
                    float x = rnd.Next(-200, 200);
                    float y = rnd.Next(-200, 200);
                    xK.Add(new Keyframe<float>() { Time = ttt, EasingType = easeT2, Value = x, kType = KeyframeType.PositionX });
                    yK.Add(new Keyframe<float>() { Time = ttt, EasingType = easeT2, Value = y, kType = KeyframeType.PositionY });
                    ttt += 0.5f;
                }
            }
        }

        private struct AnimationResult
        {
            public bool    ShouldBeVisible;
            public Vector2 Position;
            public Vector2 Scale;
            public float   Rotation;
            public Color   Color;
        }
    }
}