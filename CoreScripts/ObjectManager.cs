using Godot;
using System;
using System.Collections.Generic;

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

            for (int i = 0; i < _sortedByStart.Count; i++)
            {
                var obj = _sortedByStart[i];

                if (obj.startTime > time)
                {
                    // Скрываем этот и все последующие объекты в списке —
                    // они все гарантированно ещё не начались (список отсортирован)
                    for (int j = i; j < _sortedByStart.Count; j++)
                    {
                        var remaining = _sortedByStart[j];
                        if (remaining.Visible)
                        {
                            remaining.Visible = false;
                            remaining.animCache.IsDirty = true;
                            objectRenderer?.MarkDirty();
                        }
                    }
                    break;
                }

                float localTime     = time - obj.startTime;
                float prevLocalTime = _prevTime - obj.startTime;

                DebugProfiler.BeginAccum("Anim.StateUpdate");
                bool wasVisible = obj.Visible;
                ObjectStateUpdate(obj);
                if (wasVisible != obj.Visible) objectRenderer?.MarkDirty();
                DebugProfiler.EndAccum("Anim.StateUpdate");

                if (!obj.Visible) continue;

                var cache = obj.animCache;

                if (cache.IsDirty)
                {
                    cache.IndexPosX = cache.IndexPosY = cache.IndexScaleX =
                    cache.IndexScaleY = cache.IndexRotation = cache.IndexColor = 0;
                    cache.IsDirty = false;
                }

                Vector2 prevPos      = obj.Position;
                Vector2 prevScale    = obj.Scale;
                float   prevRotation = obj.Rotation;
                Color   prevColor    = obj.Color;

                DebugProfiler.BeginAccum("Anim.Position");
                PositionObjectUpdate(obj, localTime, prevLocalTime, cache);
                DebugProfiler.EndAccum("Anim.Position");

                DebugProfiler.BeginAccum("Anim.Scale");
                ScaleObjectUpdate(obj, localTime, prevLocalTime, cache);
                DebugProfiler.EndAccum("Anim.Scale");

                DebugProfiler.BeginAccum("Anim.Rotation");
                RotationObjectUpdate(obj, localTime, prevLocalTime, cache);
                DebugProfiler.EndAccum("Anim.Rotation");

                DebugProfiler.BeginAccum("Anim.Color");
                ColorObjectUpdate(obj, localTime, prevLocalTime, cache);
                DebugProfiler.EndAccum("Anim.Color");

                if (obj.Position != prevPos      ||
                    obj.Scale    != prevScale    ||
                    obj.Rotation != prevRotation ||
                    obj.Color    != prevColor)
                {
                    objectRenderer?.MarkDirty();
                }
            }

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
        public void PositionObjectUpdate(GameObject gameObject, float localTime, float prevLocalTime, GameObject.AnimationCache cache)
        {
            float finalX = gameObject.Position.X;
            float finalY = gameObject.Position.Y;

            if (gameObject.keyframePositionX.Count > 0)
            {
                int indexX = GetCurrentIndex(gameObject.keyframePositionX, ref cache.IndexPosX, localTime, prevLocalTime);

                if (indexX < 0)
                    finalX = gameObject.keyframePositionX[0].Value;
                else if (indexX >= gameObject.keyframePositionX.Count - 1)
                    finalX = gameObject.keyframePositionX[^1].Value;
                else
                {
                    var left  = gameObject.keyframePositionX[indexX];
                    var right = gameObject.keyframePositionX[indexX + 1];
                    float t   = (localTime - left.Time) / (right.Time - left.Time);
                    finalX    = EasingFunctions.Lerp(left.Value, right.Value, EasingFunctions.Ease(t, right.EasingType));
                }
            }

            if (gameObject.keyframePositionY.Count > 0)
            {
                int indexY = GetCurrentIndex(gameObject.keyframePositionY, ref cache.IndexPosY, localTime, prevLocalTime);

                if (indexY < 0)
                    finalY = gameObject.keyframePositionY[0].Value;
                else if (indexY >= gameObject.keyframePositionY.Count - 1)
                    finalY = gameObject.keyframePositionY[^1].Value;
                else
                {
                    var left  = gameObject.keyframePositionY[indexY];
                    var right = gameObject.keyframePositionY[indexY + 1];
                    float t   = (localTime - left.Time) / (right.Time - left.Time);
                    finalY    = EasingFunctions.Lerp(left.Value, right.Value, EasingFunctions.Ease(t, right.EasingType));
                }
            }

            gameObject.Position = new Vector2(finalX, -finalY);
        }

        // ── Scale ─────────────────────────────────────────────────────────
        public void ScaleObjectUpdate(GameObject gameObject, float localTime, float prevLocalTime, GameObject.AnimationCache cache)
        {
            float finalScaleX = gameObject.Scale.X;
            float finalScaleY = gameObject.Scale.Y;

            if (gameObject.keyframeScaleX.Count > 0)
            {
                int indexX = GetCurrentIndex(gameObject.keyframeScaleX, ref cache.IndexScaleX, localTime, prevLocalTime);

                if (indexX < 0)
                    finalScaleX = gameObject.keyframeScaleX[0].Value;
                else if (indexX >= gameObject.keyframeScaleX.Count - 1)
                    finalScaleX = gameObject.keyframeScaleX[^1].Value;
                else
                {
                    var left    = gameObject.keyframeScaleX[indexX];
                    var right   = gameObject.keyframeScaleX[indexX + 1];
                    float t     = (localTime - left.Time) / (right.Time - left.Time);
                    finalScaleX = EasingFunctions.Lerp(left.Value, right.Value, EasingFunctions.Ease(t, right.EasingType));
                }
            }

            if (gameObject.keyframeScaleY.Count > 0)
            {
                int indexY = GetCurrentIndex(gameObject.keyframeScaleY, ref cache.IndexScaleY, localTime, prevLocalTime);

                if (indexY < 0)
                    finalScaleY = gameObject.keyframeScaleY[0].Value;
                else if (indexY >= gameObject.keyframeScaleY.Count - 1)
                    finalScaleY = gameObject.keyframeScaleY[^1].Value;
                else
                {
                    var left    = gameObject.keyframeScaleY[indexY];
                    var right   = gameObject.keyframeScaleY[indexY + 1];
                    float t     = (localTime - left.Time) / (right.Time - left.Time);
                    finalScaleY = EasingFunctions.Lerp(left.Value, right.Value, EasingFunctions.Ease(t, right.EasingType));
                }
            }

            gameObject.Scale = new Vector2(finalScaleX, finalScaleY);
        }

        // ── Rotation ──────────────────────────────────────────────────────
        public void RotationObjectUpdate(GameObject gameObject, float localTime, float prevLocalTime, GameObject.AnimationCache cache)
        {
            if (gameObject.keyframeRotation.Count == 0) return;

            int index = GetCurrentIndex(gameObject.keyframeRotation, ref cache.IndexRotation, localTime, prevLocalTime);

            if (index < 0)
            {
                gameObject.RotationDegrees = gameObject.keyframeRotation[0].Value;
                return;
            }

            if (index >= gameObject.keyframeRotation.Count - 1)
            {
                gameObject.RotationDegrees = gameObject.keyframeRotation[^1].Value;
                return;
            }

            var left     = gameObject.keyframeRotation[index];
            var right    = gameObject.keyframeRotation[index + 1];
            float t      = (localTime - left.Time) / (right.Time - left.Time);
            float easedT = EasingFunctions.Ease(t, right.EasingType);

            float startRad = Mathf.DegToRad(left.Value);
            float endRad   = Mathf.DegToRad(right.Value);
            gameObject.Rotation = Mathf.LerpAngle(startRad, endRad, easedT);
        }

        // ── Color ─────────────────────────────────────────────────────────
        public void ColorObjectUpdate(GameObject gameObject, float localTime, float prevLocalTime, GameObject.AnimationCache cache)
        {
            if (gameObject.keyframeColor.Count == 0) return;

            int index = GetCurrentIndex(gameObject.keyframeColor, ref cache.IndexColor, localTime, prevLocalTime);

            Color finalColor;

            if (index < 0)
            {
                var first = gameObject.keyframeColor[0].Value;
                finalColor = first != null ? first.color : Colors.White;
            }
            else if (index >= gameObject.keyframeColor.Count - 1)
            {
                var last = gameObject.keyframeColor[^1].Value;
                finalColor = last != null ? last.color : Colors.White;
            }
            else
            {
                var leftKf  = gameObject.keyframeColor[index];
                var rightKf = gameObject.keyframeColor[index + 1];
                var left    = leftKf.Value;
                var right   = rightKf.Value;

                if (left == null || right == null)
                    finalColor = left?.color ?? right?.color ?? Colors.White;
                else
                {
                    float t      = (localTime - leftKf.Time) / (rightKf.Time - leftKf.Time);
                    float easedT = EasingFunctions.Ease(t, rightKf.EasingType);
                    finalColor   = left.color.Lerp(right.color, easedT);
                }
            }

            gameObject.Color = finalColor;
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
    }
}