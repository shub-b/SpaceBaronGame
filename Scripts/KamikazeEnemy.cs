using Godot;
using System;

public partial class KamikazeEnemy : CharacterBody3D, IEnemy
{
    [Export] public float MoveSpeed { get; set; } = 100f;
    [Export] public float Damage { get; set; } = 26f;
    [Export] public float MaxHealth { get; set; } = 15f;
    [Export] public float LookAtPlayerOffset { get; set; } = 1.0f;
    [Export] public float StopLookAtPlayerOffset { get; set; } = 5.0f;
    [Export] public float DeactivationRange { get; set; } = 5.0f;
    [Export] public float AvoidStrength { get; set; } = 150f;

    private bool active;
    private float currentHealth;
    private MeshInstance3D mesh;
    private CollisionShape3D collisionShape;
    private CharacterBody3D playerShip;
    private RayCast3D rayF, rayL, rayR;

    public override void _Ready()
    {
        mesh = GetNode<MeshInstance3D>("KamikazeHullMesh3D");
        collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        rayF = GetNode<RayCast3D>("RayFront");
        rayL = GetNode<RayCast3D>("RayLeft");
        rayR = GetNode<RayCast3D>("RayRight");

        playerShip = GetTree().Root.GetNode<CharacterBody3D>("OuterSpace/PlayerShip");
        rayF.AddException(playerShip);
        rayL.AddException(playerShip);
        rayR.AddException(playerShip);
        AddToGroup("Hostile");

        currentHealth = MaxHealth;
        Deactivate();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!active)
            return;

        if (playerShip == null || !IsInstanceValid(playerShip))
            return;

        Vector3 moveDir;
        if (GlobalPosition.Z < playerShip.GlobalPosition.Z - StopLookAtPlayerOffset)
        {
            LookAt(playerShip.GlobalPosition, Vector3.Up);
            moveDir = (playerShip.GlobalPosition + Vector3.Forward * LookAtPlayerOffset - GlobalPosition).Normalized();
        }
        else
        {
            moveDir = -GlobalTransform.Basis.Z.Normalized();
        }

        rayF.ForceRaycastUpdate();
        rayL.ForceRaycastUpdate();
        rayR.ForceRaycastUpdate();

        if (rayF.IsColliding())
        {
            bool hitLeft = rayL.IsColliding();
            bool hitRight = rayR.IsColliding();

            if (!hitLeft && hitRight)
                moveDir -= GlobalTransform.Basis.X * AvoidStrength * (float)delta;
            else if (!hitRight && hitLeft)
                moveDir += GlobalTransform.Basis.X * AvoidStrength * (float)delta;
            else
                moveDir += GlobalTransform.Basis.X * AvoidStrength * (float)delta * (GD.Randf() < 0.5f ? 1f : -1f);

            moveDir = moveDir.Normalized();
        }
        Velocity = moveDir * MoveSpeed;
        MoveAndSlide();

        if (GlobalPosition.Z > playerShip.GlobalPosition.Z + DeactivationRange)
            Deactivate();
    }

    public bool IsActive() => active;

    public float ApplyDamage() => Damage;

    public void TakeDamage(float amount)
    {
        if (!active)
            return;

        currentHealth -= amount;
        GD.Print($"KamikazeEnemy took {amount} damage, HP now {currentHealth}/{Damage}");
        if (currentHealth <= 0)
            Deactivate();
    }

    public void Activate()
    {
        active = true;
        currentHealth = Damage;
        mesh.Visible = true;
        collisionShape.CallDeferred("set_disabled", false);
        SetPhysicsProcess(true);
    }

    public void Deactivate()
    {
        active = false;
        mesh.Visible = false;
        collisionShape.CallDeferred("set_disabled", true);
        SetPhysicsProcess(false);
    }
}
