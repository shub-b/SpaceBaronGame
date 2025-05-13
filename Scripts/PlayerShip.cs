// PlayerShip.cs
using System;
using Godot;

public partial class PlayerShip : CharacterBody3D
{

    [Export] public PackedScene Projectile {get; set;}
    [Export] public float ProjectileFireDelay {get; set;} = 0.5f;

    [Export] public float ProjectileInstantiationOffsetZ {get; set;} = 1.5f;
    [Export] public Vector3 ProjectileScale {get; set;} = new Vector3(0.5f, 0.5f, 0.5f);

    [Export] public float MaxPlayerSpeed {get; set;} = 25f;
    [Export] public float StrafeAcceleration {get; set;} = 30f;
    [Export] public float MaxStrafeSpeed {get; set;} = 25f;
    [Export] public float XAxisMaxBound {get; set;} = 30f;
    [Export] public float MaxHealth {get; set;} = 100f;

    bool active;

    private float currentStrafeSpeed = 0f;
    private float initialXPos;
    private float currentHealth;

    private bool canShoot = true;
    private Timer timer;


    

    public override void _Ready()
    {
        initialXPos = GlobalPosition.X;
        currentHealth = MaxHealth;
        timer = GetNode<Timer>("ShootTimer");
        timer.WaitTime = ProjectileFireDelay;
        timer.Timeout += OnShootTimeout;
    }

    private void OnShootTimeout()
    {
        GD.Print($"Shoot Delay Activated");
        canShoot = true;
    }


    public override void _PhysicsProcess(double delta)
    {
        float input = Input.GetActionStrength("strafe_right") - Input.GetActionStrength("strafe_left");
        Velocity = new Vector3(currentStrafeSpeed, 0f, -MaxPlayerSpeed);

        if (Input.IsActionPressed("fire_projectile") && canShoot){
            FireProjectile();
            canShoot = false;
            timer.Start();
        }

        StrafeMovement(input, (float)delta);
        MoveAndSlide();
        CheckEnemyCollisions();
        BoundPlayerXAxis();

        if (currentHealth <= 0)
            OnDeath();
    }

    private void StrafeMovement(float input, float delta)
    {
        if (!Mathf.IsZeroApprox(input))
        {
            currentStrafeSpeed += input * StrafeAcceleration * delta;
            currentStrafeSpeed = Mathf.Clamp(currentStrafeSpeed, -MaxStrafeSpeed, MaxStrafeSpeed);
        }
        else
        {
            currentStrafeSpeed = Mathf.MoveToward(currentStrafeSpeed, 0f, StrafeAcceleration * delta);
        }
    }
    public bool IsActive() => active;


    private void FireProjectile()
    {
        if (Projectile == null)
            return;
      
        var missile = Projectile.Instantiate<Area3D>();
        GetParent().AddChild(missile);
        missile.Scale = ProjectileScale;
        missile.GlobalPosition = GlobalPosition + Vector3.Forward * ProjectileInstantiationOffsetZ;
    }

    private void BoundPlayerXAxis()
    {
        Vector3 pos = GlobalPosition;
        pos.X = Mathf.Clamp(pos.X, initialXPos - XAxisMaxBound, initialXPos + XAxisMaxBound);
        GlobalPosition = pos;
    }

    private void CheckEnemyCollisions()
    {
        int collisionCount = GetSlideCollisionCount();
        for (int i = 0; i < collisionCount; i++)
        {
            var collision = GetSlideCollision(i);
            var hitNode = collision.GetCollider() as Node;
            if (hitNode != null && hitNode.IsInGroup("Hostile"))
            {
                float damage = 0;
                if (hitNode is IEnemy enemy)
                {
                    damage = enemy.ApplyDamage();
                    enemy.Deactivate();
                }

                TakeDamage(damage);
                break;
            }
        }
    }

    private void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        GD.Print($"Player took {amount} damage, health now {currentHealth}/{MaxHealth}");
    }

    private void OnDeath()
    {
        GD.Print("Player has died. Deactivating ship.");
        DeactivatePlayer();
    }

    private void DeactivatePlayer()
    {
        Hide();
        SetProcess(false);
        SetPhysicsProcess(false);
        CollisionLayer = 0;
        CollisionMask = 0;
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetCurrentStrafeSpeed() => currentStrafeSpeed;
}
