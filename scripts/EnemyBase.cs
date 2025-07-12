using Godot;
using System;

public partial class EnemyBase : Node2D
{
    [ExportCategory("UI")]
    [Export] public PackedScene DamagePopupScene { get; set; }

    [Export] public float MaxHealth { get; set; } = 10f;
    public float Health { get; private set; }

    [Export] public ElementType Weakness { get; set; }
    [Export] public ElementType Resistance { get; set; }

    public float LastBaseDamage { get; private set; }
    public float LastElementalDamage { get; private set; }

    public override void _Ready()
    {
        Health = MaxHealth;
    }

    public void TakeDamage(float damage, ElementType elementType, bool isCrit = false)
    {
        if (elementType == Resistance)
        {
            LastBaseDamage = 0;
            LastElementalDamage = 0;

            // Visa "Immune"
            ShowImmunePopup();
            return;
        }

        LastBaseDamage = damage;
        LastElementalDamage = 0f;

        if (elementType == Weakness)
        {
            float elementalBuff = BuffManager.Instance.GetElementalBuff(elementType);
            LastElementalDamage = damage * (elementalBuff - 1f);
        }

        float totalDamage = LastBaseDamage + LastElementalDamage;
        Health -= totalDamage;

        ShowDamagePopup(isCrit);

        if (Health <= 0)
            Die();
    }

    private void ShowDamagePopup(bool isCrit)
    {
        if (DamagePopupScene == null)
        {
            GD.PrintErr("DamagePopupScene är inte satt i EnemyBase!");
            return;
        }

        // Bas damage popup
        Vector2 baseOffset = new Vector2(
            (float)GD.RandRange(-10, 10),
            (float)GD.RandRange(-10, 10)
        );

        var basePopup = DamagePopupScene.Instantiate<DamagePopup>();
        GetParent().AddChild(basePopup);

        Color baseColor = isCrit ? Colors.Yellow : Colors.White;
        int baseFontSize = isCrit ? 12 : 8;

        // Formatera med max 2 decimaler, inga onödiga nollor
        string baseText = $"+{LastBaseDamage.ToString("0.##")}";

        basePopup.ShowPopup(GlobalPosition + baseOffset, baseText, baseColor, baseFontSize);

        // Om elemental damage finns, spawn en separat popup för den
        if (LastElementalDamage > 0)
        {
            Vector2 elementalOffset = new Vector2(
                (float)GD.RandRange(-10, 10),
                (float)GD.RandRange(-10, 10)
            );

            var elementalPopup = DamagePopupScene.Instantiate<DamagePopup>();
            GetParent().AddChild(elementalPopup);

            Color elementalColor = Colors.Cyan;  // Välj en färg du gillar för elemental damage
            int elementalFontSize = isCrit ? 10 : 8;

            // Samma formatering för elemental damage
            string elementalText = $"+{LastElementalDamage.ToString("0.##")}";

            elementalPopup.ShowPopup(GlobalPosition + elementalOffset, elementalText, elementalColor, elementalFontSize);
        }
    }


    private void ShowImmunePopup()
    {
        if (DamagePopupScene == null)
        {
            GD.PrintErr("DamagePopupScene är inte satt i EnemyBase!");
            return;
        }

        Vector2 randomOffset = new Vector2(
            (float)GD.RandRange(-10, 10),
            (float)GD.RandRange(-10, 10)
        );

        var popup = DamagePopupScene.Instantiate<DamagePopup>();
        GetParent().AddChild(popup);

        Color color = Colors.LightGray;  // Immune kan vara grått eller valfri färg
        int fontSize = 10;

        popup.ShowPopup(GlobalPosition + randomOffset, "Immune", color, fontSize);
    }

    protected virtual void Die()
    {
        QueueFree();
    }
}
