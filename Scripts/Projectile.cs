using Godot;
using System;
using System.Collections.Generic;

public partial class Projectile : Area3D
{
    [Export] public float ProjectileSpeed { get; set; } = 100f;
    [Export] public float ProjectileLifeTime { get; set; } = 5f;
    [Export] public float Damage { get; set; } = 2.5f;
    public static float GlobalDamageMultiplier { get; set; } = 1f;

    private RayCast3D[] rays;

    public override void _Ready()
    {
        var list = new List<RayCast3D>();
        foreach (Node child in GetChildren())
            if (child is RayCast3D rc)
                list.Add(rc);
        rays = list.ToArray();
        foreach (var ray in rays){
            ray.Enabled = true;
            ray.CollideWithAreas = true;
            ray.CollideWithBodies = true;
        }

        var timer = GetNode<Timer>("Timer");
        timer.WaitTime = ProjectileLifeTime;
        timer.OneShot = true;
        timer.Timeout += OnTimeTimeout;
        timer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var ray in rays)
        {
            ray.ForceRaycastUpdate();
            if (ray.IsColliding())
            {
                var col = ray.GetCollider();
                if (col is Node node && node is IEnemy enemy)
                {
                    bool wasAlive = enemy.IsActive();
                    float damageDealt = Damage * GlobalDamageMultiplier;
                    enemy.TakeDamage(damageDealt);

                    if (wasAlive && !enemy.IsActive())
                    {
                        var hud = GetTree().Root
                            .GetNode<HeadsUpDisplay>("OuterSpace/HeadsUpDisplay");
                        hud?.AddScore(enemy.PointsValue);
                        hud?.ShowDamage(damageDealt);
                    }
                }
                QueueFree();
                return;
            }
        }

        Translate(-GlobalTransform.Basis.Z * ProjectileSpeed * (float)delta);
    }

    private void OnTimeTimeout()
    {
        QueueFree();
    }
}
