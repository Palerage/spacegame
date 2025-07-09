using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed { get; private set; } = 200f;
    [Export] public float Acceleration { get; private set; } = 800f;
    [Export] public float Friction { get; private set; } = 600f;
    [Export] public float TurboMultiplier = 1.5f;

    [Export] public PackedScene BulletScene;
    [Export] public int DamageAreaBulletCount = 3;
    [Export] public float DamageAreaRadius = 50f;
    [Export] public float DamageAreaSpeed = 90f;

    private List<Area2D> _damageAreaBullets = new();
    private List<float> _damageAreaAngles = new();

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
        var graphics = GetNodeOrNull<Node2D>("Graphics");
        if (graphics == null) return;

        var body = graphics.GetNodeOrNull<Sprite2D>("Body");
        var wing = graphics.GetNodeOrNull<Sprite2D>("Wing");

        if (body != null)
        {
            body.Modulate = BodyColor;
            body.QueueRedraw();
        }

        if (wing != null)
        {
            wing.Modulate = WingColor;
            wing.QueueRedraw();
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

    private void Fire()
    {
        // 1. Hämta spawnpunkten
        var marker = GetNodeOrNull<Marker2D>("Points/Single/Marker2D");
        if (marker == null || BulletScene == null)
        {
            GD.PrintErr("Marker or BulletScene missing!");
            return;
        }

        // 2. Skapa instansen
        var bulletInstance = BulletScene.Instantiate<Area2D>();

        // 3. Sätt position vid marker
        bulletInstance.GlobalPosition = marker.GlobalPosition;

        // 4. Lägg till i scenen
        GetTree().CurrentScene.AddChild(bulletInstance);

        // 5. Sätt riktning/hastighet om din bullet behöver det
        var bulletScript = bulletInstance as Bullet;
        if (bulletScript != null)
        {
            bulletScript.Direction = Vector2.Up; // exempel
        }
    }

    private void FireDouble()
    {
        var doubleNode = GetNodeOrNull<Node2D>("Points/Double");
        if (doubleNode == null || BulletScene == null)
        {
            GD.PrintErr("Double node or BulletScene missing!");
            return;
        }

        foreach (var child in doubleNode.GetChildren())
        {
            if (child is Marker2D marker)
            {
                // Skapa instans
                var bulletInstance = BulletScene.Instantiate<Area2D>();
                bulletInstance.GlobalPosition = marker.GlobalPosition;
                GetTree().CurrentScene.AddChild(bulletInstance);

                if (bulletInstance is Bullet bulletScript)
                {
                    bulletScript.Direction = Vector2.Up;
                }
            }
        }
    }

    private void ActivateDamageArea()
    {
        if (BulletScene == null)
        {
            GD.PrintErr("BulletScene saknas!");
            return;
        }

        var marker = GetNodeOrNull<Marker2D>("Points/DamageArea/Marker2D");
        if (marker == null)
        {
            GD.PrintErr("Marker2D i DamageArea saknas!");
            return;
        }

        _damageAreaBullets.Clear();
        _damageAreaAngles.Clear();

        for (int i = 0; i < DamageAreaBulletCount; i++)
        {
            var bullet = BulletScene.Instantiate<Area2D>();
            GetTree().CurrentScene.CallDeferred("add_child", bullet);

            // Fördela vinklar jämnt runt cirkeln
            float angle = i * (360f / DamageAreaBulletCount);
            _damageAreaAngles.Add(angle);

            _damageAreaBullets.Add(bullet);
        }
    }

    private void UpdateDamageAreaBullet(double delta)
    {
        if (_damageAreaBullets.Count == 0)
            return;

        var marker = GetNodeOrNull<Marker2D>("Points/DamageArea/Marker2D");
        if (marker == null)
            return;

        for (int i = 0; i < _damageAreaBullets.Count; i++)
        {
            _damageAreaAngles[i] += DamageAreaSpeed * (float)delta;
            if (_damageAreaAngles[i] >= 360f)
                _damageAreaAngles[i] -= 360f;

            float radians = Mathf.DegToRad(_damageAreaAngles[i]);
            Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * DamageAreaRadius;
            _damageAreaBullets[i].GlobalPosition = marker.GlobalPosition + offset;
        }
    }
}
