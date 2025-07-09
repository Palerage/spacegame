using Godot;
using System;

public partial class Bullet : Area2D
{
    [Export] public float Speed = 400f;
    public Vector2 Direction = Vector2.Up;

    public override void _PhysicsProcess(double delta)
    {
        Position += Direction.Normalized() * Speed * (float)delta;
    }

    private void OnVisibilityNotifier2DScreenExited()
    {
        QueueFree();
    }
}