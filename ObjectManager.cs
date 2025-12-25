using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
    public partial class ObjectManager : Node2D
    {

        public float time = 0f;


        public List<GameObject> objects = new List<GameObject>();

        [Export] public TimelineController timeline;
        [Export] public PackedScene gameObject;
        [Export] public TestScene testScene;

        [Export] public Polygon2D polygonObject;
        [Export] public CollisionPolygon2D collisionPlygonObject;
        private float _currentTime { get; set; }



        public override void _Ready()
        {
            var objectExample = gameObject.Instantiate<GameObject>();

            objectExample.shapeObj.Polygon = (Vector2[])polygonObject.Polygon.Clone();
            objectExample.collisionShapeObj = collisionPlygonObject;
            objectExample.startTime = 2f;
            objectExample.endTime = 6f;

            var xK = new List<KeyframePosX>();
            var yK = new List<KeyframePosY>();
            KeyframePosGenerator(5, ref xK, ref yK);

            objectExample.keyframePosX = xK;
            objectExample.keyframePosY = yK;
            objectExample.Visible = false;

            timeline.AddChild(objectExample);
            objects.Add(objectExample);
        }

        public void KeyframePosGenerator(int nums, ref List<KeyframePosX> xK, ref List<KeyframePosY> yK)
        {
            // Linear keyframe for initial position
            xK.Add(new KeyframePosX(0f, EasingType.Linear, 0f));
            yK.Add(new KeyframePosY(0f, EasingType.Linear, 0f));

            float ttt = 0.1f;
            Random rnd = new();
            var easeT = Enum.GetValues(typeof(EasingType));

            for (int i = 0; i < 16; i++)
            {
                EasingType easeT2 = (EasingType)easeT.GetValue(i);
                for (int j = 0; j < nums; j++)
                {
                    float x = rnd.Next(-200, 200);
                    float y = rnd.Next(-200, 200);
                    xK.Add(new KeyframePosX(ttt, easeT2, x));
                    yK.Add(new KeyframePosY(ttt, easeT2, y));
                    ttt += 0.5f;
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            time = timeline.currentTime;
            testScene.debugLabel2.Text = time.ToString();
            foreach (GameObject obj in objects)
            {
                ObjectStateUpdate(obj);
                PositionObjectUpdate(obj);
            }
        }

        public void ObjectStateUpdate(GameObject gameObject)
        {
            // Проверяем, должен ли объект быть видимым в текущий момент времени
            bool shouldBeVisible = time >= gameObject.startTime && time < gameObject.endTime;

            // Если текущее состояние не совпадает с требуемым — меняем его
            if (gameObject.Visible != shouldBeVisible)
            {
                gameObject.Visible = shouldBeVisible;
                GD.Print(shouldBeVisible ? "объект появился!!" : "объект скрыт!!");
            }
        }





        public void PositionObjectUpdate(GameObject gameObject)
        {
            if (gameObject.keyframePosX.Count == 0 || gameObject.keyframePosY.Count == 0) return;

            int index = BinarySearchForTime(gameObject.keyframePosX, time);

            if (index < 0)
            {
                gameObject.Position = new Vector2(gameObject.keyframePosX[0].X, gameObject.keyframePosY[0].Y); // ✅ ИСПРАВЛЕНО!
                return;
            }

            if (index >= gameObject.keyframePosX.Count - 1)
            {
                gameObject.Position = new Vector2(gameObject.keyframePosX[gameObject.keyframePosX.Count - 1].X,
                gameObject.keyframePosY[gameObject.keyframePosY.Count - 1].Y); // ✅ ИСПРАВЛЕНО!
                return;
            }

            var leftX = gameObject.keyframePosX[index];
            var rightX = gameObject.keyframePosX[index + 1];
            var leftY = gameObject.keyframePosY[index];
            var rightY = gameObject.keyframePosY[index + 1];

            float t = (time - leftX.Time) / (rightX.Time - leftX.Time);
            float easedT = EasingFunctions.Ease(t, leftX.EasingType);

            float interpolatedX = EasingFunctions.Lerp(leftX.X, rightX.X, easedT);
            float interpolatedY = EasingFunctions.Lerp(leftY.Y, rightY.Y, easedT);
            gameObject.Position = new Vector2(interpolatedX, interpolatedY);
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

        
    }
}




