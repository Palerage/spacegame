using Godot;
using System;
using System.Collections.Generic;

public partial class WeaponBase : Node
{
    [ExportCategory("Weapon Type")]
    [Export] public WeaponType Type { get; set; }

    [ExportCategory("Barrels")]
    [Export] public BarrelType Barrel { get; set; }

    [ExportCategory("Bullet SpawnPoints")]
    [Export] NodePath BulletSpawnPoints { get; set; }
    private Node2D _bulletSpawnPoints;
    private Dictionary<BarrelType, Node2D> _barrelSpawnNodes;

    [ExportCategory("Bullet Types")]
    [Export] public PackedScene NeutralBulletScene { get; set; }
    [Export] public PackedScene FireBulletScene { get; set; }
    [Export] public PackedScene IceBulletScene { get; set; }
    [Export] public PackedScene PoisonBulletScene { get; set; }
    [Export] public PackedScene ElectricBulletScene { get; set; }

    [ExportCategory("Weapon Stats")]
    [Export] public float Damage { get; set; } = 1f;
    [Export] public float Range { get; set; } = 30f;
    [Export] public float BulletSpeed { get; set; } = 600f;
    [Export] public float FireRate { get; set; } = 0.5f;
    [Export] public float CriticalChance { get; set; } = 0.05f;
    [Export] public float CriticalMultiplier { get; set; } = 1.5f;

    private float _currentCooldown = 0f;
    private Random _random = new Random();

    public override void _Ready()
    {
        _bulletSpawnPoints = GetNode<Node2D>(BulletSpawnPoints);

        _barrelSpawnNodes = new Dictionary<BarrelType, Node2D>
    {
        { BarrelType.SingleBarrel, _bulletSpawnPoints.GetNode<Node2D>("SingleBarrel") },
        { BarrelType.DoubleBarrel, _bulletSpawnPoints.GetNode<Node2D>("DoubleBarrel") },
        { BarrelType.TripleBarrel, _bulletSpawnPoints.GetNode<Node2D>("TripleBarrel") },
        { BarrelType.Spread, _bulletSpawnPoints.GetNode<Node2D>("Spread") }
    };
    }

    public override void _Process(double delta)
    {
        if (_currentCooldown > 0f)
            _currentCooldown -= (float)delta;
    }

    public bool CanFire()
    {
        return _currentCooldown <= 0f;
    }

    public void Fire(Player player)
    {
        if (!CanFire())
            return;

        _currentCooldown = FireRate;

        float totalDamage = Damage;

        bool isCrit = _random.NextDouble() < CriticalChance;
        if (isCrit)
            totalDamage *= CriticalMultiplier;

        if (!_barrelSpawnNodes.TryGetValue(Barrel, out var spawnNode))
        {
            return;
        }

        int bulletCount = 0;
        foreach (Node child in spawnNode.GetChildren())
        {
            if (child is BulletDirection)
                bulletCount++;
        }

        if (bulletCount == 0)
            return;

        float damagePerBullet = totalDamage / bulletCount;

        foreach (Node child in spawnNode.GetChildren())
        {
            if (child is BulletDirection marker)
            {
                PackedScene bulletScene = GetBulletSceneForElement(player.ActiveElementType);
                if (bulletScene == null)
                {
                    return;
                }

                var bulletInstance = bulletScene.Instantiate<Node2D>();
                bulletInstance.GlobalPosition = marker.GlobalPosition;

                if (bulletInstance is BaseBullet bullet)
                {
                    Vector2 direction = marker.GetNormalizedDirection();
                    bullet.Setup(damagePerBullet, player.ActiveElementType, isCrit);
                    bullet.SetDirection(direction);
                    bullet.Speed = BulletSpeed;
                }

                GetTree().CurrentScene.AddChild(bulletInstance);
            }
        }
    }

    private PackedScene GetBulletSceneForElement(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => FireBulletScene,
            ElementType.Ice => IceBulletScene,
            ElementType.Poison => PoisonBulletScene,
            ElementType.Electric => ElectricBulletScene,
            _ => NeutralBulletScene
        };
    }
}

public enum WeaponType
{
    Light,
    Heavy,
    AreaDamage
}

public enum BarrelType
{
    SingleBarrel,
    DoubleBarrel,
    TripleBarrel,
    Spread,
}
