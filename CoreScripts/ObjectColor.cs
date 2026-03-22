using Godot;
using System;

namespace LostEditor;

public partial class ObjectColor : Node
{
    public int ColorId { get; set; } = 0;
    public event Action OnDataChanged;

    private LevelColor baseLevelColor;
    public Color color;

    private float _HModifier;
    private float _SModifier;
    private float _VModifier;
    private float _AModifier = 1f;

    public float HModifier => _HModifier;
    public float SModifier => _SModifier;
    public float VModifier => _VModifier;
    public float AModifier => _AModifier;

    public EasingType easingType;

    public void LoadData(float? Hue, float? Sat, float? Val, float? Opacity, EasingType? easingType)
    {
        if (Hue.HasValue)
            _HModifier = Mathf.Clamp(Hue.Value, -1f, 1f);

        if (Sat.HasValue)
            _SModifier = Mathf.Clamp(Sat.Value, -1f, 1f);

        if (Val.HasValue)
            _VModifier = Mathf.Clamp(Val.Value, -1f, 1f);

        if (Opacity.HasValue)
            _AModifier = Mathf.Clamp(Opacity.Value, 0f, 1f);

        if (easingType.HasValue)
            this.easingType = easingType.Value;

        color = GetFinalColor();
        OnDataChanged?.Invoke();
    }


    private Color GetFinalColor()
    {
        // ИЗМЕНЕНО: берём CurrentColor — он отражает состояние после триггеров.
        // BaseColor остаётся нетронутым как исходное значение уровня.
        Color baseCol = baseLevelColor?.CurrentColor ?? new Color(1f, 1f, 1f, 1f);
        baseCol.ToHsv(out float h, out float s, out float v);
    
        h = Mathf.Wrap(h + _HModifier, 0f, 1f);
        s = Mathf.Clamp(s + _SModifier, 0f, 1f);
        v = Mathf.Clamp(v + _VModifier, 0f, 1f);
    
        float a = Mathf.Clamp(baseCol.A * _AModifier, 0f, 1f);
    
        return Color.FromHsv(h, s, v, a);
    }


    // Внешний метод для установки базового цвета
    public void SetBaseLevelColor(LevelColor levelColor)
    {
        baseLevelColor = levelColor;
        this.color = GetFinalColor();
        OnDataChanged?.Invoke();
    }

    public void RestoreBaseLevelColor(LevelColorData levelColorData)
    {
        if (levelColorData == null) return;
        var lc = levelColorData.GetColor(ColorId);
        if (lc != null)
            SetBaseLevelColor(lc);
    }

    public ObjectColor Clone()
    {
        var copy = new ObjectColor();
        copy.ColorId = this.ColorId;
        copy.easingType = this.easingType;
    
        // Копируем baseLevelColor — это ссылка на уровневый цвет,
        // её шарить между кейфреймами безопасно (она read-only по смыслу)
        copy.SetBaseLevelColor(this.baseLevelColor);
    
        // Копируем модификаторы через LoadData — он же пересчитает color
        copy.LoadData(_HModifier, _SModifier, _VModifier, _AModifier, easingType);
    
        return copy;
    }
 

}
