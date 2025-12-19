using Godot;
using System;

public partial class Mc : CharacterBody3D
{
    [Export] public float Speed { get; set; } = 6f;
    [Export] public float JumpVelocity { get; set; } = 5f;
    [Export] public float Gravity { get; set; } = 9.8f;
    [Export] public bool Debug { get; set; } = false;
    [Export] public bool AutoMoveDebug { get; set; } = false;

    private Vector3 _velocity = Vector3.Zero;

    public override void _Ready()
    {
        SetPhysicsProcess(true);
        GD.Print("Mc._Ready: script attached — ready and physics process enabled");
       
        var cs = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        GD.Print($"CollisionShape3D present: {cs != null}");
        if (cs != null)
        {
            GD.Print($"CollisionShape3D has shape: {cs.Shape != null}");
        }
        GD.Print($"CharacterBody CollisionLayer: {CollisionLayer}, CollisionMask: {CollisionMask}");
        GD.Print("Tip: Click the game window so it has focus before pressing keys.");
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        Vector3 input = Vector3.Zero;
        bool w = Input.IsKeyPressed(Key.W);
        bool s = Input.IsKeyPressed(Key.S);
        bool a = Input.IsKeyPressed(Key.A);
        bool d = Input.IsKeyPressed(Key.D);
        bool space = Input.IsKeyPressed(Key.Space);

        if (w)
            input += GlobalTransform.Basis.Z;
        if (s)
            input -= GlobalTransform.Basis.Z;
        if (d)
            input -= GlobalTransform.Basis.X;
        if (a)
            input += GlobalTransform.Basis.X;

        input.Y = 0f;
        if (input != Vector3.Zero)
            input = input.Normalized() * Speed;

        _velocity.X = input.X;
        _velocity.Z = input.Z;
        if (IsOnFloor())
        {
            if (space)
                _velocity.Y = JumpVelocity;
            else
                _velocity.Y = 0f;
        }
        else
        {
            _velocity.Y -= Gravity * dt;
        }

        Velocity = _velocity;
        MoveAndSlide();
        _velocity = Velocity;

        if (Debug)
        {
            GD.Print($"Keys W A S D Space: {w} {a} {s} {d} {space}");
            GD.Print($"Input vector: {input}, Velocity: {Velocity}, OnFloor: {IsOnFloor()}");
        }
        if (AutoMoveDebug)
        {
            var testVel = _velocity;
            testVel.X = Speed * 0.5f;
            Velocity = testVel;
            MoveAndSlide();
            _velocity = Velocity;
            if (Debug)
                GD.Print($"AutoMove applied: Velocity {Velocity}");
            return;
        }
    }
}
