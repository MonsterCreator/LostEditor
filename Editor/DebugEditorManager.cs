using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using LostEditor;

public partial class DebugEditorManager : Node
{
	[Export] public Label[] debugLables;

	public void AddTextToTheNewRaw(int labelIndex, string text)
	{
		if(labelIndex >= debugLables.Length) return;
		debugLables[labelIndex].Text += $"\n {text}";
	}

	public void OverrideText(int labelIndex, string text)
	{
		if (labelIndex >= debugLables.Length) return;

		string current = debugLables[labelIndex].Text.Trim();

		if (current == text || current.StartsWith(text + " ("))
		{
			debugLables[labelIndex].Text = UpIterationLabel(current, text);
		}
		else
		{
			debugLables[labelIndex].Text = text;
		}
	}

	private string UpIterationLabel(string currentText, string baseText)
	{
		// Проверяем, заканчивается ли строка на " (число)"
		if (currentText.Length > baseText.Length && 
			currentText.StartsWith(baseText) && 
			currentText[baseText.Length] == ' ' && 
			currentText[baseText.Length + 1] == '(')
		{
			int openIndex = baseText.Length + 1; // позиция '('
			int closeIndex = currentText.IndexOf(')', openIndex);
			
			if (closeIndex != -1)
			{
				string numPart = currentText.Substring(openIndex + 1, closeIndex - openIndex - 1);
				
				if (int.TryParse(numPart, out int counter))
				{
					return $"{baseText} ({counter + 1})";
				}
			}
		}

		// Если формат не распознан — начинаем с (1)
		return $"{baseText} (1)";
	}
}
