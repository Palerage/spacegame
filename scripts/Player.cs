using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    [ExportCategory("Movement")]
    [Export] public float Speed { get; private set; } = 200f;
    [Export] public float Acceleration { get; private set; } = 800f;
    [Export] public float Friction { get; private set; } = 600f;
    [Export] public float TurboMultiplier = 1.5f;

    [ExportCategory("Weapons")]
    [Export] public PackedScene BulletScene;

    [ExportCategory("SpawnPoints")]
    [Export] public NodePath SingleShotPoint;
    [Export] public NodePath[] DoubleShotPoints;
    [Export] public NodePath DamageAreaPoint;

    [ExportCategory("Damage Radius")]
    [Export] public int DamageAreaBulletCount = 3;
    [Export] public float DamageAreaRadius = 50f;
    [Export] public float DamageAreaSpeed = 90f;

    private List<Area2D> _damageAreaBullets = new();
    private List<float> _damageAreaAngles = new();

    [ExportCategory("Cosmetics")]
    [Export] public Color BodyColor = Colors.White;
    [Export] public Color WingColor = Colors.White;

    private Vector2 _inputDirection = Vector2.Zero;
    private bool _turbo;

    public override void _Ready()
    {
        CenterPlayer();
        ApplyPlayerColor();
        CallDeferred(nameof(ActivateDamageArea));
    }

    public override void _PhysicsProcess(double delta)
    {
        GetInput();
        UpdateDamageAreaBullet(delta);
        MovePlayer(delta);

        if (Input.IsActionJustPressed("fire"))
        {
            Fire();
            FireDouble();
        }
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
            Velocity = Velocity.MoveToward(targetVelocity, Acceleration * (float)delta);
        else
            Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);

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
        var graphics = GetNodeOrNull<Node2D>("Graphics");
        if (graphics == null) return;

        var body = graphics.GetNodeOrNull<Sprite2D>("Body");
        var wing = graphics.GetNodeOrNull<Sprite2D>("Wing");

        if (body != null)
            body.Modulate = BodyColor;
        if (wing != null)
            wing.Modulate = WingColor;
    }

    private void ClampPositionToScreen()
    {
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;

        var collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collisionShape == null) return;

        Vector2 halfExtents = Vector2.One * 16;
        if (collisionShape.Shape is RectangleShape2D rectShape)
            halfExtents = rectShape.Size / 2;
        else if (collisionShape.Shape is CircleShape2D circleShape)
            halfExtents = new Vector2(circleShape.Radius, circleShape.Radius);

        Position = new Vector2(
            Mathf.Clamp(Position.X, halfExtents.X, screenSize.X - halfExtents.X),
            Mathf.Clamp(Position.Y, halfExtents.Y, screenSize.Y - halfExtents.Y)
        );
    }

    private void Fire()
    {
        if (BulletScene == null || SingleShotPoint == null) return;

        var marker = GetNodeOrNull<Marker2D>(SingleShotPoint);
        if (marker == null) return;

        var bulletInstance = BulletScene.Instantiate<Area2D>();
        bulletInstance.GlobalPosition = marker.GlobalPosition;
        GetTree().CurrentScene.AddChild(bulletInstance);

        if (bulletInstance is Bullet bulletScript)
            bulletScript.Direction = Vector2.Up;
    }

    private void FireDouble()
    {
        if (BulletScene == null || DoubleShotPoints == null) return;

        foreach (var path in DoubleShotPoints)
        {
            var marker = GetNodeOrNull<Marker2D>(path);
            if (marker == null) continue;

            var bulletInstance = BulletScene.Instantiate<Area2D>();
            bulletInstance.GlobalPosition = marker.GlobalPosition;
            GetTree().CurrentScene.AddChild(bulletInstance);

            if (bulletInstance is Bullet bulletScript)
                bulletScript.Direction = Vector2.Up;
        }
    }

    private void ActivateDamageArea()
    {
        if (BulletScene == null || DamageAreaPoint == null) return;

        var marker = GetNodeOrNull<Marker2D>(DamageAreaPoint);
        if (marker == null) return;

        _damageAreaBullets.Clear();
        _damageAreaAngles.Clear();

        for (int i = 0; i < DamageAreaBulletCount; i++)
        {
            var bullet = BulletScene.Instantiate<Area2D>();
            GetTree().CurrentScene.CallDeferred("add_child", bullet);

            _damageAreaBullets.Add(bullet);
            _damageAreaAngles.Add(i * (360f / DamageAreaBulletCount));
        }
    }

    private void UpdateDamageAreaBullet(double delta)
    {
        if (_damageAreaBullets.Count == 0 || DamageAreaPoint == null) return;

        var marker = GetNodeOrNull<Marker2D>(DamageAreaPoint);
        if (marker == null) return;

        for (int i = 0; i < _damageAreaBullets.Count; i++)
        {
            _damageAreaAngles[i] = (_damageAreaAngles[i] + DamageAreaSpeed * (float)delta) % 360f;
            float radians = Mathf.DegToRad(_damageAreaAngles[i]);
            Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * DamageAreaRadius;
            _damageAreaBullets[i].GlobalPosition = marker.GlobalPosition + offset;
        }
    }
}