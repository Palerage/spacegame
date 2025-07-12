using Godot;
using System;
using System.Collections.Generic;

public enum ElementType
{
    Neutral,
    Fire,
    Ice,
    Poison,
    Electric
}

public partial class Player : CharacterBody2D
{
    [ExportCategory("Movement")]
    [Export] public float Speed { get; private set; } = 200f;
    [Export] public float Acceleration { get; private set; } = 200f;
    [Export] public float Friction { get; private set; } = 200f;
    [Export] public float TurboMultiplier = 1.2f;

    [ExportCategory("Weapons")]
    [Export] public WeaponBase LightWeapon { get; set; }
    [Export] public WeaponBase HeavyWeapon { get; set; }


    [ExportCategory("Cosmetics")]
    [Export] public Color BodyColor = Colors.White;
    [Export] public Color WingColor = Colors.White;

    [ExportCategory("Elemental Buffs")]
    public ElementType ActiveElementType { get; set; } = ElementType.Neutral;

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
        Fire(delta);
        HandleElementToggleInput();
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

    private void Fire(double delta)
    {

        if (Input.IsActionPressed("fire_light"))
        {
            LightWeapon.Fire(this);
        }
        else if (Input.IsActionPressed("fire_heavy"))
        {
            HeavyWeapon.Fire(this);
        }
    }
    public void ApplyBuff(ElementType element, float buffAmount)
    {
        if (BuffManager.Instance.ElementalDamageBuffs.ContainsKey(element))
        {
            BuffManager.Instance.ElementalDamageBuffs[element] = buffAmount;
        }
    }

    private void HandleElementToggleInput()
    {
        ElementType[] elementTypes = (ElementType[])Enum.GetValues(typeof(ElementType));
        int currentIndex = Array.IndexOf(elementTypes, ActiveElementType);

        if (Input.IsActionJustPressed("left_toggle_element"))
        {
            int prevIndex = (currentIndex - 1 + elementTypes.Length) % elementTypes.Length;
            ActiveElementType = elementTypes[prevIndex]; GD.Print($"Active Element changed to: {ActiveElementType}");
        }
        else if (Input.IsActionJustPressed("right_toggle_element"))
        {
            int nextIndex = (currentIndex + 1) % elementTypes.Length;
            ActiveElementType = elementTypes[nextIndex];
        }
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
}