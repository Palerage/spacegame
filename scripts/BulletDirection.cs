using Godot;

[GlobalClass]
public partial class BulletDirection : Marker2D
{
    [Export] public Vector2 Direction { get; set; }

    public Vector2 GetNormalizedDirection()
    {
        return Direction.Normalized();
    }
}