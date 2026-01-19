using Godot;
using System;
using System.Collections.Generic;

namespace LostEditor;

public partial class InspectorPanel : HBoxContainer
{
    [Export] public LineEdit NameInput;
    [Export] public LineEdit StartTimeInput;
    [Export] public LineEdit EndTimeInput;
    [Export] public Button SetStartToCurrentBtn;
    [Export] public Button SetEndToCurrentBtn;
    [Export] public WorkPanel workPanel;

    private GameObject obj;

    public event Action<GameObject> OnDataUpdated;

    // Вызывается при удалении узла со сцены. Важно отписаться, чтобы избежать утечек памяти.
    public override void _ExitTree()
    {
        if (obj != null)
        {
            obj.OnDataChanged -= UpdateInspectorData;
        }
    }

    public void Inspect(GameObject Obj)
    {
        if (obj != null)
        {
            obj.OnDataChanged -= UpdateInspectorData;
        }

        workPanel.CurrentTab = 1;
        obj = Obj;

        // 2. Подписываемся на НОВЫЙ объект
        if (obj != null)
        {
            obj.OnDataChanged += UpdateInspectorData;
            
            // Загружаем данные первый раз
            UpdateInspectorData(); 
        }
        OnDataUpdated.Invoke(obj);
    }

    // Этот метод будет вызываться автоматически при срабатывании события OnDataChanged внутри GameObject
    private void UpdateInspectorData()
    {
        // Проверка на валидность (на случай если объект был уничтожен, но событие долетело)
        if (obj == null || !GodotObject.IsInstanceValid(obj)) return;

        // Важный момент: чтобы курсор не прыгал при вводе текста,
        // иногда добавляют проверки "если текст совпадает, не обновляем".
        // Но так как у тебя обновление по Enter (Submitted), можно обновлять всё.
        
        if (NameInput.Text != obj.name) // Небольшая оптимизация визуальная
            NameInput.Text = obj.name ?? ""; // Защита от null
            
        // ToString с форматированием, чтобы не было 5.0000001
        StartTimeInput.Text = obj.startTime.ToString("0.###"); 
        EndTimeInput.Text = obj.endTime.ToString("0.###");
        OnDataUpdated.Invoke(obj);
    }

    // --- Методы UI ---

    public void ObjectNameTextEditSubmitted(string text)
    {
        if (obj == null) return;
        
        // Изменение свойства вызовет OnDataChanged -> UpdateInspectorData.
        // Поэтому ручной вызов LoadObjectData здесь можно убрать,
        // но можно и оставить для мгновенной реакции UI.
        if(text != null) obj.name = text; 
    }

    public void ObjectStartTimeTextEditSubmitted(string text)
    {
        if (obj == null) return;

        float startTime;
        bool canParse = float.TryParse(text, out startTime);
        if(canParse) 
        {
            obj.startTime = startTime; // Это триггернет событие и обновит инспектор
        }
    }

	public void ObjectEndTimeTextEditSubmitted(string text)
    {
        if (obj == null) return;

        float endTime;
        bool canParse = float.TryParse(text, out endTime);
        if(canParse) 
        {
            obj.endTime = endTime; // Это триггернет событие и обновит инспектор
        }
    }

	public void ObjectEndTimeOffsetTextEditSubmitted(string text)
    {
        if (obj == null) return;

        float endTimeOffset;
        bool canParse = float.TryParse(text, out endTimeOffset);
        if(canParse) 
        {
            obj.endTimeOffset = endTimeOffset; // Это триггернет событие и обновит инспектор
        }
    }
    
	public void ItemSelected(int index)
	{
		if (obj == null) return;

		EndTimeMode endTimeMode = index switch
		{
			0 => EndTimeMode.NoEndTime,
			1 => EndTimeMode.FixedTime,
			2 => EndTimeMode.LastKeyframe,
			3 => EndTimeMode.LastKeyframeOffset,
			4 => EndTimeMode.GlobalTime
		};
		GD.Print(endTimeMode);
		obj.endTimeMode = endTimeMode;
	}
    // Аналогично для EndTime...
}