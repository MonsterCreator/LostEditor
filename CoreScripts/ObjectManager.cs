using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
    public partial class ObjectManager : Node2D
    {

        public float time = 0f;
        public List<GameObject> objects = new List<GameObject>();
        

        [Export] public TimelineController timelineController;
        [Export] public Polygon2D polygonObject;
        [Export] public CollisionPolygon2D collisionPlygonObject;
        [Export] public DebugEditorManager debugEditorManager;



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
        
        
        public void KeyframePosGenerator(int nums, ref List<Keyframe<float>> xK, ref List<Keyframe<float>> yK)
        {
            // Начальные кадры
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
                    
                    xK.Add(new Keyframe<float>() 
                    { 
                        Time = ttt, 
                        EasingType = easeT2, 
                        Value = x, 
                        kType = KeyframeType.PositionX 
                    });
                    
                    yK.Add(new Keyframe<float>() 
                    { 
                        Time = ttt, 
                        EasingType = easeT2, 
                        Value = y, 
                        kType = KeyframeType.PositionY 
                    });
                    
                    ttt += 0.5f;
                }
            }
        }
        

        // Генератор размерных кейфреймов
        /*
        public void KeyframeSizeGenerator(int nums, ref List<Keyframe<float>> xK, ref List<Keyframe<float>> yK)
        
        {
            xK.Add(new Keyframe<float>()
            {
                Time = 0f,
                EasingType = EasingType.Linear,
                Value = 1f
            });
            yK.Add(new Keyframe<float>()
            {
                Time = 0f,
                EasingType = EasingType.Linear,
                Value = 1f
            });

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
                    xK.Add(new Keyframe<float>()
                    {
                        Time = ttt,
                        EasingType = easeT2,
                        Value = x
                    });
                    yK.Add(new Keyframe<float>()
                    {
                        Time = ttt,
                        EasingType = easeT2,
                        Value = y
                    });
                    ttt += 0.5f;
                }
            }
        }
        */

        // Генератор кейфреймов вращения
        /*
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
        */

        /*
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
        */

        public override void _PhysicsProcess(double delta)
        {
            foreach (GameObject obj in objects)
            {
                // Вычисляем локальное время ОДИН РАЗ для этого объекта
                float localTime = time - obj.startTime;
                debugEditorManager.OverrideText(2,$"Object local time: {localTime}");
                // Если объект еще не начался или уже закончился, 
                // ObjectStateUpdate его скроет, но для расчетов используем localTime
                ObjectStateUpdate(obj); 

                // Передаем localTime во все методы обновления
                PositionObjectUpdate(obj, localTime);
                //SizeObjectUpdate(obj, localTime);
                //RotationObjectUpdate(obj, localTime);
                //ColorObjectUpdate(obj, localTime);
            }
        }

        public void ObjectStateUpdate(GameObject gameObject)
        {
            // startTime — когда объект начался (глобально)
            // cachedEndTime — сколько он длится (локально)
            
            float globalStart = gameObject.startTime;
            float globalEnd = gameObject.startTime + gameObject.cachedEndTime;

            // Объект виден, если текущее время больше старта, но меньше конца
            bool shouldBeVisible = time >= globalStart && time < globalEnd;

            if (gameObject.Visible != shouldBeVisible)
            {
                gameObject.Visible = shouldBeVisible;
            }
        }





        public void PositionObjectUpdate(GameObject gameObject, float localTime)
        {
            if (gameObject.keyframePosX.Count == 0 || gameObject.keyframePosY.Count == 0) return;

            int index = BinarySearchForTime(gameObject.keyframePosX, localTime);
            debugEditorManager.OverrideText(3,$"Pos keyfarme inex: {index}");

            if (index < 0)
            {
                gameObject.Position = new Vector2(gameObject.keyframePosX[0].Value, gameObject.keyframePosY[0].Value); // ✅ ИСПРАВЛЕНО!
                return;
            }

            if (index >= gameObject.keyframePosX.Count - 1)
            {
                gameObject.Position = new Vector2(gameObject.keyframePosX[gameObject.keyframePosX.Count - 1].Value,
                gameObject.keyframePosY[gameObject.keyframePosY.Count - 1].Value); // ✅ ИСПРАВЛЕНО!
                return;
            }

            var leftX = gameObject.keyframePosX[index];
            var rightX = gameObject.keyframePosX[index + 1];
            var leftY = gameObject.keyframePosY[index];
            var rightY = gameObject.keyframePosY[index + 1];

            // ИСПРАВЛЕНО: используем localTime вместо time
            float t = (localTime - leftX.Time) / (rightX.Time - leftX.Time); 
            
            float easedT = EasingFunctions.Ease(t, leftX.EasingType);
            float interpolatedX = EasingFunctions.Lerp(leftX.Value, rightX.Value, easedT);
            float interpolatedY = EasingFunctions.Lerp(leftY.Value, rightY.Value, easedT);
            gameObject.Position = new Vector2(interpolatedX, interpolatedY);
        }

        // Обновление размера объекта
        /*
        public void SizeObjectUpdate(GameObject gameObject, float localTime)
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
        */

        // Обновление вращения объекта
        /*
        public void RotationObjectUpdate(GameObject gameObject, float localTime)
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
        */

        // Обновление цвета объекта
        /*
        public void ColorObjectUpdate(GameObject gameObject, float localTime)
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
        */

        private int BinarySearchForTime<T>(List<T> list, float t) where T : IKeyframe
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