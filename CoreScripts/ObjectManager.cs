using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor
{
    /*
        ObjectManager - "элемент ядра" системы анимации. 
        ObjectManager управляет состоянием объектов на сцене и их анимацией и не должен ничего знать о редакторе.
    */
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
                ScaleObjectUpdate(obj, localTime);
                RotationObjectUpdate(obj, localTime);
                ColorObjectUpdate(obj, localTime);
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
            // Инициализируем текущими значениями (на случай, если списки ключей пусты)
            float finalX = gameObject.Position.X;
            float finalY = gameObject.Position.Y;

            // --- РАСЧЕТ ОСИ X ---
            if (gameObject.keyframePositionX.Count > 0)
            {
                int indexX = BinarySearchForTime(gameObject.keyframePositionX, localTime);
                
                if (indexX < 0) 
                {
                    finalX = gameObject.keyframePositionX[0].Value;
                }
                else if (indexX >= gameObject.keyframePositionX.Count - 1) 
                {
                    finalX = gameObject.keyframePositionX[gameObject.keyframePositionX.Count - 1].Value;
                }
                else 
                {
                    var left = gameObject.keyframePositionX[indexX];
                    var right = gameObject.keyframePositionX[indexX + 1];
                    float t = (localTime - left.Time) / (right.Time - left.Time);
                    float easedT = EasingFunctions.Ease(t, right.EasingType);
                    finalX = EasingFunctions.Lerp(left.Value, right.Value, easedT);
                }
            }

            // --- РАСЧЕТ ОСИ Y (полностью обособлен от X) ---
            if (gameObject.keyframePositionY.Count > 0)
            {
                // Используем свой поиск индекса для Y
                int indexY = BinarySearchForTime(gameObject.keyframePositionY, localTime);
                
                if (indexY < 0) 
                {
                    finalY = gameObject.keyframePositionY[0].Value;
                }
                else if (indexY >= gameObject.keyframePositionY.Count - 1) 
                {
                    finalY = gameObject.keyframePositionY[gameObject.keyframePositionY.Count - 1].Value;
                }
                else 
                {
                    var left = gameObject.keyframePositionY[indexY];
                    var right = gameObject.keyframePositionY[indexY + 1];
                    float t = (localTime - left.Time) / (right.Time - left.Time);
                    float easedT = EasingFunctions.Ease(t, right.EasingType);
                    finalY = EasingFunctions.Lerp(left.Value, right.Value, easedT);
                }
            }

            // Применяем результат (каждая компонента рассчитана по своим ключам)
            gameObject.Position = new Vector2(finalX, -finalY);
        }

        // Обновление размера объекта
        
        public void ScaleObjectUpdate(GameObject gameObject, float localTime)
        {
            // Инициализируем текущими значениями (на случай, если списки ключей пусты)
            float finalScaleX = gameObject.shapeObj.Scale.X;
            float finalScaleY = gameObject.shapeObj.Scale.Y;

            // --- РАСЧЕТ МАСШТАБА ПО ОСИ X ---
            if (gameObject.keyframeScaleX.Count > 0)
            {
                int indexX = BinarySearchForTime(gameObject.keyframeScaleX, localTime);

                if (indexX < 0)
                {
                    finalScaleX = gameObject.keyframeScaleX[0].Value;
                }
                else if (indexX >= gameObject.keyframeScaleX.Count - 1)
                {
                    finalScaleX = gameObject.keyframeScaleX[gameObject.keyframeScaleX.Count - 1].Value;
                }
                else
                {
                    var left = gameObject.keyframeScaleX[indexX];
                    var right = gameObject.keyframeScaleX[indexX + 1];
                    float t = (localTime - left.Time) / (right.Time - left.Time);
                    float easedT = EasingFunctions.Ease(t, right.EasingType);
                    finalScaleX = EasingFunctions.Lerp(left.Value, right.Value, easedT);
                }
            }

            // --- РАСЧЕТ МАСШТАБА ПО ОСИ Y ---
            if (gameObject.keyframeScaleY.Count > 0)
            {
                int indexY = BinarySearchForTime(gameObject.keyframeScaleY, localTime);

                if (indexY < 0)
                {
                    finalScaleY = gameObject.keyframeScaleY[0].Value;
                }
                else if (indexY >= gameObject.keyframeScaleY.Count - 1)
                {
                    finalScaleY = gameObject.keyframeScaleY[gameObject.keyframeScaleY.Count - 1].Value;
                }
                else
                {
                    var left = gameObject.keyframeScaleY[indexY];
                    var right = gameObject.keyframeScaleY[indexY + 1];
                    float t = (localTime - left.Time) / (right.Time - left.Time);
                    float easedT = EasingFunctions.Ease(t, right.EasingType);
                    finalScaleY = EasingFunctions.Lerp(left.Value, right.Value, easedT);
                }
            }

            // Применяем итоговый вектор масштаба
            gameObject.shapeObj.Scale = new Vector2(finalScaleX, finalScaleY);
        }
        


        public void RotationObjectUpdate(GameObject gameObject, float localTime)
        {
            if (gameObject.keyframeRotation.Count == 0) return;

            int index = BinarySearchForTime(gameObject.keyframeRotation, localTime); 

            if (index < 0)
            {
                // Применяем как градусы
                gameObject.shapeObj.RotationDegrees = gameObject.keyframeRotation[0].Value;
                return;
            }

            if (index >= gameObject.keyframeRotation.Count - 1)
            {
                // Применяем как градусы
                gameObject.shapeObj.RotationDegrees = gameObject.keyframeRotation[gameObject.keyframeRotation.Count - 1].Value;
                return;
            }

            var left = gameObject.keyframeRotation[index];
            var right = gameObject.keyframeRotation[index + 1];

            float t = (localTime - left.Time) / (right.Time - left.Time);
            float easedT = EasingFunctions.Ease(t, right.EasingType);

            // 1. Если вы хотите использовать ваши кастомные Easings, но для ГРАДУСОВ:
            // Мы используем Mathf.LerpAngle, чтобы вращение всегда шло по кратчайшему пути.
            // Т.к. LerpAngle работает с радианами, мы конвертируем значения туда и обратно.
            
            float startRad = Mathf.DegToRad(left.Value);
            float endRad = Mathf.DegToRad(right.Value);
            float interpolatedRad = Mathf.LerpAngle(startRad, endRad, easedT);

            // Применяем итоговый результат в радианах (это надежнее для движка)
            gameObject.shapeObj.Rotation = interpolatedRad;
        }
  

        public void ColorObjectUpdate(GameObject gameObject, float localTime)
        {
            if (gameObject.keyframeColor.Count == 0) return;

            int index = BinarySearchForTime(gameObject.keyframeColor, time);

            if (index < 0)
            {
                gameObject.shapeObj.Color = gameObject.keyframeColor[0].Value;
                return;
            }

            if (index >= gameObject.keyframeColor.Count - 1)
            {
                gameObject.shapeObj.Color = gameObject.keyframeColor[gameObject.keyframeColor.Count - 1].Value;
                return;
            }

            var left = gameObject.keyframeColor[index];
            var right = gameObject.keyframeColor[index + 1];

            float t = (time - left.Time) / (right.Time - left.Time);
            float easedT = EasingFunctions.Ease(t, right.EasingType);

            Color interpolatedColor = new Color(
                EasingFunctions.Lerp(left.Value.R, right.Value.R, easedT),
                EasingFunctions.Lerp(left.Value.G, right.Value.G, easedT),
                EasingFunctions.Lerp(left.Value.B, right.Value.B, easedT),
                EasingFunctions.Lerp(left.Value.A, right.Value.A, easedT)
            );
            gameObject.shapeObj.Color = interpolatedColor;
        }


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