using Godot;
using System;

public partial class BaseBullet : Area2D
{
    protected ElementType ElementType;
    private float Damage;
    private Vector2 _direction;
    public bool IsCritical { get; private set; }
    public float Speed { get; set; }

    public override void _Ready()
    {
        Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Player)
            return;

        if (body is EnemyBase enemy)
        {
            OnHit(enemy);
        }
        else
        {
            QueueFree();
        }
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction.Normalized();
    }

    public virtual void Setup(float damage, ElementType elementType, bool isCrit = false)
    {
        Damage = damage;
        ElementType = elementType;
        IsCritical = isCrit;
    }

    public override void _Process(double delta)
    {
        Position += _direction * Speed * (float)delta;
        var viewport = GetViewport();
        Rect2 visibleRect = viewport.GetVisibleRect();

        if (!GetViewport().GetVisibleRect().HasPoint(GlobalPosition))
            QueueFree();
    }

    public void OnHit(Node2D target)
    {
        if (target is EnemyBase enemy)
        {
            enemy.TakeDamage(Damage, ElementType, IsCritical);
        }
        QueueFree();
    }
}
