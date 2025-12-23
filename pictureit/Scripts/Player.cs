// Credits: @AymenChakirIGA
using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float Speed { get; set; } = 6f;
    [Export] public float JumpVelocity { get; set; } = 5f;
    [Export] public float Gravity { get; set; } = 9.8f;
    [Export] public bool Debug { get; set; } = false;
    [Export] public bool AutoMoveDebug { get; set; } = false;
    [Export] public float Senesitivity { get; set; } = 0.002f;
    [Export] public float RunningSpeedMultiplier { get; set; } = 1.5f;
    [Export] public float defaultSpeedMultiplier { get; set; } = 1f;

    private Vector3 _velocity = Vector3.Zero;
    private Node3D cameraPivot;
    private Camera3D camera;
    private float currentSpeedMultiplier = 1f;

    //camera variables

    //Bob Effect
    [Export] float defaultBobFrequency = 2.0f;
    [Export] float bobAmplitude = 0.05f;
    [Export] float t_bob = 0.0f;
    
    //FOV Effect
    [Export] float defaultFov = 65.0f;
    [Export] float runFov = 75.0f;
    [Export] float currentFov = 65.0f;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        cameraPivot = GetNode<Node3D>("CameraPivot");
        
        currentSpeedMultiplier = defaultSpeedMultiplier;
        currentFov = defaultFov;

        camera = cameraPivot.GetNode<Camera3D>("Camera3D");
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

    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event is InputEventMouseMotion)
        {
            InputEventMouseMotion mouseMotion = @event as InputEventMouseMotion;
                cameraPivot.RotateY(-mouseMotion.Relative.X * Senesitivity);
                camera.RotateX(-mouseMotion.Relative.Y * Senesitivity);
                camera.Rotation = new Vector3(
                    Mathf.Clamp(camera.Rotation.X, Mathf.DegToRad(-89), Mathf.DegToRad(89)),
                    camera.Rotation.Y,
                    camera.Rotation.Z
                );
        }
        base._UnhandledInput(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Handle input
        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        Vector3 direction = (cameraPivot.Transform.Basis * new Vector3(input.X, 0, input.Y)).Normalized();
        bool isRunning = Input.IsActionPressed("run");
        bool space = Input.IsKeyPressed(Key.Space);

        //Handle movements
        _velocity.X = direction.X * Speed * currentSpeedMultiplier;
        _velocity.Z = direction.Z * Speed * currentSpeedMultiplier;

        // Handle running
        if (isRunning) currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, RunningSpeedMultiplier, 0.1f);
        else currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, defaultSpeedMultiplier, 0.3f);

        if (IsOnFloor() && space)  _velocity.Y = JumpVelocity;
        else _velocity.Y -= Gravity * dt;

        Velocity = _velocity;
        MoveAndSlide();
        _velocity = Velocity;

        // Camera bobbing effect
        t_bob += dt * Velocity.Length() * (IsOnFloor() ? 1f : 0f);
        camera.Transform = new Transform3D(camera.Transform.Basis, _headBob(t_bob, defaultBobFrequency, bobAmplitude));

        // FOV effect
        currentFov = Mathf.Lerp(currentFov, isRunning ? runFov : defaultFov, 0.05f);
        camera.Fov = currentFov;
    }

    private Vector3 _headBob(float t, float frequency, float amplitude)
    {
        Vector3 pos = Vector3.Zero;
        pos.Y += Mathf.Sin(t * frequency) * amplitude;
        return pos;
    } 
}