using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed { get; private set; } = 200f;
    [Export] public float Acceleration { get; private set; } = 800f;
    [Export] public float Friction { get; private set; } = 600f;
    [Export] public float TurboMultiplier = 2;

    [Export] public Color PlayerColor = Colors.White; // <--- Färg direkt i editorn

    private Vector2 _inputDirection = Vector2.Zero;
    private bool _turbo;

    public override void _Ready()
    {
        CenterPlayer();
        ApplyPlayerColor();

    }

    public override void _PhysicsProcess(double delta)
    {
        GetInput();
        MovePlayer(delta);
    }

    private void GetInput()
    {
        _inputDirection = Vector2.Zero;

        if (Input.IsActionPressed("move_right"))
            _inputDirection.X += 1;
        if (Input.IsActionPressed("move_left"))
            _inputDirection.X -= 1;
        if (Input.IsActionPressed("move_down"))
            _inputDirection.Y += 1;
        if (Input.IsActionPressed("move_up"))
            _inputDirection.Y -= 1;

        _inputDirection = _inputDirection.Normalized();
        _turbo = Input.IsActionPressed("turbo");
    }

    private void MovePlayer(double delta)
    {
        Vector2 targetVelocity = _inputDirection * Speed;

        if (_turbo)
            targetVelocity *= TurboMultiplier;

        if (_inputDirection != Vector2.Zero)
        {
            Velocity = Velocity.MoveToward(targetVelocity, Acceleration * (float)delta);
        }
        else
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
        }

        MoveAndSlide();
        ClampPositionToScreen();
    }

    private void CenterPlayer()
    {
        Vector2 windowSize = GetViewport().GetVisibleRect().Size;
        Position = windowSize / 2;
    }

    private void ApplyPlayerColor()
    {
        var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        if (sprite != null)
        {
            sprite.Modulate = PlayerColor;
            sprite.QueueRedraw();
        }
    }

    private void ClampPositionToScreen()
    {
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;

        var collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collisionShape == null)
            return;

        var shape = collisionShape.Shape;
        Vector2 halfExtents = Vector2.Zero;

        if (shape is RectangleShape2D rectShape)
        {
            halfExtents = rectShape.Size / 2;
        }
        else if (shape is CircleShape2D circleShape)
        {
            float radius = circleShape.Radius;
            halfExtents = new Vector2(radius, radius);
        }
        else
        {
            halfExtents = Vector2.One * 16; // fallback
        }

        float minX = halfExtents.X;
        float maxX = screenSize.X - halfExtents.X;
        float minY = halfExtents.Y;
        float maxY = screenSize.Y - halfExtents.Y;

        Position = new Vector2(
            Mathf.Clamp(Position.X, minX, maxX),
            Mathf.Clamp(Position.Y, minY, maxY)
        );
    }
}
