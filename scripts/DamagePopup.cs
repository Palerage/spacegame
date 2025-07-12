using Godot;

public partial class DamagePopup : Label
{
    public async void ShowPopup(Vector2 position, string text, Color color, int fontSize = 6)
    {
        GlobalPosition = position;
        Text = text;
        AddThemeColorOverride("font_color", color);
        AddThemeFontSizeOverride("font_size", fontSize);

        // Valfritt: tweena uppåt
        var tween = CreateTween();
        tween.TweenProperty(this, "global_position", GlobalPosition + new Vector2(0, -20), 1.0f);
        tween.TweenProperty(this, "modulate:a", 0, 1.0f);

        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        QueueFree();
    }
}