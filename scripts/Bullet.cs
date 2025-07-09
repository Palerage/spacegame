using Godot;
using System;
using System.Collections.Generic;

public partial class Bullet : Area2D
{
    [Export] public float Speed = 400f;
    public Vector2 Direction = Vector2.Up;

    [Export] public int MaxTrailPoints = 15;  // Max antal punkter i svansen

    private Line2D _trail;
    private Queue<Vector2> _positions = new Queue<Vector2>();

    public override void _Ready()
    {
        _trail = GetNode<Line2D>("Trail");
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += Direction.Normalized() * Speed * (float)delta;

        // Lägg till global position i kön
        _positions.Enqueue(GlobalPosition);

        if (_positions.Count > MaxTrailPoints)
            _positions.Dequeue();

        // Konvertera global position till lokala koordinater i förhållande till Bullet
        Vector2[] localPoints = new Vector2[_positions.Count];
        int i = 0;
        foreach (var globalPos in _positions)
        {
            localPoints[i++] = ToLocal(globalPos);
        }

        _trail.Points = localPoints;

        // Kolla om bullet är utanför skärmen + 20 px marginal
        Rect2 screenRect = GetViewport().GetVisibleRect();
        float margin = 64f;

        if (Position.X < screenRect.Position.X - margin ||
            Position.Y < screenRect.Position.Y - margin ||
            Position.X > screenRect.End.X + margin ||
            Position.Y > screenRect.End.Y + margin)
        {
            QueueFree();
        }
    }



}
