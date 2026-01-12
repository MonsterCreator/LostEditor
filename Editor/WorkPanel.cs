using Godot;
using System;

namespace LostEditor;

[Tool]
public partial class WorkPanel : Control
{
    // Используем [Export] для ссылок на узлы — это самый надежный способ в Godot 4
    [Export] public Control EmptyPanel;
    [Export] public Control SingleObjSettingsPanel;
    [Export] public Control MultiObjEditPanel;

    private int _panelNum;

    [Export]
    public int PanelNum
    {
        get => _panelNum;
        set
        {
            _panelNum = value;
            // Проверяем, что узлы не null, прежде чем менять их состояние
            ChangePanelDisplay();
        }
    }

    public override void _Ready()
    {
        // Если вы не назначили узлы в инспекторе, попробуем найти их по путям
        EmptyPanel ??= GetNodeOrNull<Control>("NoObjectsOrKeyframesPanel");
        SingleObjSettingsPanel ??= GetNodeOrNull<Control>("SingleObjectSettingsPanel");
        MultiObjEditPanel ??= GetNodeOrNull<Control>("MultiObjectEditPanel");

        ChangePanelDisplay();
    }

    public void OpenInspectorPanel()
    {
        PanelNum = 1; // Используем сеттер для переключения
    }

    private void ChangePanelDisplay()
    {
        // Защита от NullReferenceException: 
        // Если хотя бы один узел еще не инициализирован, выходим из метода
        if (EmptyPanel == null || SingleObjSettingsPanel == null || MultiObjEditPanel == null)
            return;

        // Устанавливаем видимость на основе индекса
        EmptyPanel.Visible = (_panelNum == 0);
        SingleObjSettingsPanel.Visible = (_panelNum == 1);
        MultiObjEditPanel.Visible = (_panelNum == 2);
        
        // Обработка default (если число вне диапазона 0-2)
        if (_panelNum < 0 || _panelNum > 2)
        {
            EmptyPanel.Visible = true;
            SingleObjSettingsPanel.Visible = false;
            MultiObjEditPanel.Visible = false;
        }
    }
}