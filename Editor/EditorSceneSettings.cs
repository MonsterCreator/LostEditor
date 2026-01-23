using Godot;
using LostEditor;
using System;

public partial class EditorSceneSettings : MenuButton
{
	[Export] public WorkPanel workPanel;

	public override void _Ready()
    {
        // Получаем объект всплывающего меню
        PopupMenu popup = GetPopup();
		popup.AddItem("Scene settings");

        // Подключаем сигнал нажатия на пункт меню
        // В Godot 4 C# сигналы подключаются через оператор +=
        popup.IdPressed += OnMenuIdPressed;
    }

    private void OnMenuIdPressed(long id)
    {
        // Проверяем, назначена ли ссылка на панель в инспекторе
        if (workPanel == null)
        {
            GD.PrintErr("Ошибка: workPanel не назначен в инспекторе!");
            return;
        }

        // Логика переключения:
        // Так как мы заранее задали ID пунктов меню равными индексам вкладок,
        // мы можем просто передать ID в CurrentTab.
        
        GD.Print($"Переключение на панель с ID: {id}");
        if((int)id == 0) workPanel.CurrentTab = 3;
        // Устанавливаем текущую вкладку
        
    }
}
