using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
    public partial class ObjectManager : Node2D
    {

        public float time = 0f;
        public List<GameObject> objects = new List<GameObject>();
        

        [Export] public Polygon2D polygonObject;
        [Export] public CollisionPolygon2D collisionPlygonObject;



        private float _currentTime { get; set; }



        public override void _Ready()
        {
            
        }
        
        // Этот метод вызывается Editor-ом при создании нового объекта
        public void RegisterObject(GameObject obj) 
        {
            if (!objects.Contains(obj)) 
            {
                objects.Add(obj);
            }
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

        // Генератор размерных кейфреймов
        public void KeyframeSizeGenerator(int nums, ref List<KeyframeSizeX> xK, ref List<KeyframeSizeY> yK)
        {
            xK.Add(new KeyframeSizeX(0f, EasingType.Linear, 1f));
            yK.Add(new KeyframeSizeY(0f, EasingType.Linear, 1f));

            float ttt = 0.1f;
            Random rnd = new Random();
            var easeT = Enum.GetValues(typeof(EasingType));

            for (int i = 0; i < 16; i++)
            {
                EasingType easeT2 = (EasingType)easeT.GetValue(i);
                for (int j = 0; j < nums; j++)
                {
                    float x = rnd.Next(1, 5); // Размер от 1 до 5
                    float y = rnd.Next(1, 5);
                    xK.Add(new KeyframeSizeX(ttt, easeT2, x));
                    yK.Add(new KeyframeSizeY(ttt, easeT2, y));
                    ttt += 0.5f;
                }
            }
        }

        // Генератор кейфреймов вращения
        public void KeyframeRotationGenerator(int nums, ref List<KeyframeRotation> rotationK)
        {
            rotationK.Add(new KeyframeRotation(0f, EasingType.Linear, 0f));

            float ttt = 0.1f;
            Random rnd = new Random();
            var easeT = Enum.GetValues(typeof(EasingType));

            for (int i = 0; i < 16; i++)
            {
                EasingType easeT2 = (EasingType)easeT.GetValue(i);
                for (int j = 0; j < nums; j++)
                {
                    float rotation = rnd.Next(0, 360); // Угол в градусах
                    rotationK.Add(new KeyframeRotation(ttt, easeT2, rotation));
                    ttt += 0.5f;
                }
            }
        }

        // Генератор кейфреймов цвета
        public void KeyframeColorGenerator(int nums, ref List<KeyframeColor> colorK)
        {
            colorK.Add(new KeyframeColor(0f, EasingType.Linear, new Color(1f, 1f, 1f, 1f)));

            float ttt = 0.1f;
            Random rnd = new Random();
            var easeT = Enum.GetValues(typeof(EasingType));

            for (int i = 0; i < 16; i++)
            {
                EasingType easeT2 = (EasingType)easeT.GetValue(i);
                for (int j = 0; j < nums; j++)
                {
                    Color color = new Color(
                        (float)rnd.NextDouble(),
                        (float)rnd.NextDouble(),
                        (float)rnd.NextDouble(),
                        (float)rnd.NextDouble()
                    );
                    colorK.Add(new KeyframeColor(ttt, easeT2, color));
                    ttt += 0.5f;
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            foreach (GameObject obj in objects)
            {
                ObjectStateUpdate(obj);
                PositionObjectUpdate(obj);
                SizeObjectUpdate(obj);      // Новая обработка размера
                RotationObjectUpdate(obj);  // Новая обработка вращения
                ColorObjectUpdate(obj);     // Новая обработка цвета
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

        // Обновление размера объекта
        public void SizeObjectUpdate(GameObject gameObject)
        {
            if (gameObject.keyframeSizeX.Count == 0 || gameObject.keyframeSizeY.Count == 0) return;

            int indexX = BinarySearchForTime(gameObject.keyframeSizeX, time);
            int indexY = BinarySearchForTime(gameObject.keyframeSizeY, time);

            if (indexX < 0)
            {
                gameObject.shapeObj.Scale = new Vector2(
                    gameObject.keyframeSizeX[0].X,
                    gameObject.keyframeSizeY[0].Y
                );
                return;
            }

            if (indexX >= gameObject.keyframeSizeX.Count - 1)
            {
                gameObject.shapeObj.Scale = new Vector2(
                    gameObject.keyframeSizeX[gameObject.keyframeSizeX.Count - 1].X,
                    gameObject.keyframeSizeY[gameObject.keyframeSizeY.Count - 1].Y
                );
                return;
            }

            var leftX = gameObject.keyframeSizeX[indexX];
            var rightX = gameObject.keyframeSizeX[indexX + 1];
            var leftY = gameObject.keyframeSizeY[indexY];
            var rightY = gameObject.keyframeSizeY[indexY + 1];

            float t = (time - leftX.Time) / (rightX.Time - leftX.Time);
            float easedT = EasingFunctions.Ease(t, leftX.EasingType);

            float interpolatedX = EasingFunctions.Lerp(leftX.X, rightX.X, easedT);
            float interpolatedY = EasingFunctions.Lerp(leftY.Y, rightY.Y, easedT);
            gameObject.shapeObj.Scale = new Vector2(interpolatedX, interpolatedY);
        }

        // Обновление вращения объекта
        public void RotationObjectUpdate(GameObject gameObject)
        {
            if (gameObject.keyframeRotation.Count == 0) return;

            int index = BinarySearchForTime(gameObject.keyframeRotation, time);

            if (index < 0)
            {
                gameObject.shapeObj.Rotation = gameObject.keyframeRotation[0].Rotation;
                return;
            }

            if (index >= gameObject.keyframeRotation.Count - 1)
            {
                gameObject.shapeObj.Rotation = gameObject.keyframeRotation[gameObject.keyframeRotation.Count - 1].Rotation;
                return;
            }

            var left = gameObject.keyframeRotation[index];
            var right = gameObject.keyframeRotation[index + 1];

            float t = (time - left.Time) / (right.Time - left.Time);
            float easedT = EasingFunctions.Ease(t, left.EasingType);

            float interpolatedRotation = EasingFunctions.Lerp(left.Rotation, right.Rotation, easedT);
            gameObject.shapeObj.Rotation = interpolatedRotation;
        }

        // Обновление цвета объекта
        public void ColorObjectUpdate(GameObject gameObject)
        {
            if (gameObject.keyframeColor.Count == 0) return;

            int index = BinarySearchForTime(gameObject.keyframeColor, time);

            if (index < 0)
            {
                gameObject.shapeObj.Color = gameObject.keyframeColor[0].Color;
                return;
            }

            if (index >= gameObject.keyframeColor.Count - 1)
            {
                gameObject.shapeObj.Color = gameObject.keyframeColor[gameObject.keyframeColor.Count - 1].Color;
                return;
            }

            var left = gameObject.keyframeColor[index];
            var right = gameObject.keyframeColor[index + 1];

            float t = (time - left.Time) / (right.Time - left.Time);
            float easedT = EasingFunctions.Ease(t, left.EasingType);

            Color interpolatedColor = new Color(
                EasingFunctions.Lerp(left.Color.R, right.Color.R, easedT),
                EasingFunctions.Lerp(left.Color.G, right.Color.G, easedT),
                EasingFunctions.Lerp(left.Color.B, right.Color.B, easedT),
                EasingFunctions.Lerp(left.Color.A, right.Color.A, easedT)
            );
            gameObject.shapeObj.Color = interpolatedColor;
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