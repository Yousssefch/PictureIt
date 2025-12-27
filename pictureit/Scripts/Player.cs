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
    private Node3D catGUI;

    //camera variables

    //Bob Effect
    [Export] float defaultBobFrequency = 2.0f;
    [Export] float bobAmplitude = 0.05f;
    [Export] float t_bob = 0.0f;
    
    //FOV Effect
    [Export] float defaultFov = 65.0f;
    [Export] float runFov = 75.0f;
    float currentFov = 65.0f;

    //Animations Variables
    private AnimationTree animationTree;
    private enum PlayerState{ Idle, Walking, Running, Jumping }
    private PlayerState currentState = PlayerState.Idle;
    private float walkingValue = 0.0f;
    private float runningValue = 0.0f;
    private float jumpingValue = 0.0f;

    private MeshInstance3D catMesh;

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(int.Parse(this.Name));
    }

    public override void _Ready()
    {
        cameraPivot = GetNode<Node3D>("CameraPivot");
        catGUI = cameraPivot.GetNode<Node3D>("Cat");
        animationTree = catGUI.GetNode<AnimationTree>("AnimationTree");
        if(!IsMultiplayerAuthority()) return;
        GD.Print("Player _Ready called for peer ID: " + this.Name);

        Input.MouseMode = Input.MouseModeEnum.Hidden;
        cameraPivot = GetNode<Node3D>("CameraPivot");
        
        currentSpeedMultiplier = defaultSpeedMultiplier;
        currentFov = defaultFov;
        catGUI = cameraPivot.GetNode<Node3D>("Cat");
        animationTree = catGUI.GetNode<AnimationTree>("AnimationTree");
        catMesh = catGUI.GetNode<MeshInstance3D>("Armature/Skeleton3D/Cat_007");
        catMesh.Layers = 2; // Set cat mesh to layer 2 to be visible in the GUI camera only

        camera = cameraPivot.GetNode<Camera3D>("Camera3D");
        camera.Current = true;
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
        if(!IsMultiplayerAuthority()) return;
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
        //Animations and movement only for the local player
        HandleAnimations((float)delta);

        if(!IsMultiplayerAuthority()) return;

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

        // Handle animations
        PlayerState newState = GetPlayerState(_velocity);
        if(newState != currentState)
        {
            Rpc(nameof(UpdatePlayerState), (int)newState);
            currentState = newState; //Change state locally as well
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void UpdatePlayerState(int state)
    {
        GD.Print($"Updating player state to: {(PlayerState)state}");
        this.currentState =  (PlayerState)state; //Change state on all peers
    }

    private Vector3 _headBob(float t, float frequency, float amplitude)
    {
        Vector3 pos = Vector3.Zero;
        pos.Y += Mathf.Sin(t * frequency) * amplitude;
        return pos;
    }

    private PlayerState GetPlayerState(Vector3 velocity)
    {
        if (!IsOnFloor()) return PlayerState.Jumping;

        if (velocity.Length() > 0 )
        {
            if(Input.IsActionPressed("run")) return PlayerState.Running;
            else return PlayerState.Walking;
        }

        return PlayerState.Idle;
    }

    private void HandleAnimations(float dt)
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                walkingValue = Mathf.Lerp(walkingValue, 0.0f, 0.1f);
                jumpingValue = Mathf.Lerp(jumpingValue, 0.0f, 0.1f);
                runningValue = Mathf.Lerp(runningValue, 0.0f, 0.1f);
                break;
            case PlayerState.Walking:
                walkingValue = Mathf.Lerp(walkingValue, 1.0f, 0.1f);
                jumpingValue = Mathf.Lerp(jumpingValue, 0.0f, 0.1f);
                runningValue = Mathf.Lerp(runningValue, 0.0f, 0.1f);
                break;
            case PlayerState.Running:
                // Handle running animation
                walkingValue = Mathf.Lerp(walkingValue, 0.0f, 0.1f);
                jumpingValue = Mathf.Lerp(jumpingValue, 0.0f, 0.1f);
                runningValue = Mathf.Lerp(runningValue, 1.0f, 0.1f);
                break;
            case PlayerState.Jumping:
                // Handle jumping animation
                walkingValue = Mathf.Lerp(walkingValue, 0.0f, 0.1f);
                jumpingValue = Mathf.Lerp(jumpingValue, 1.0f, 0.1f);
                runningValue = Mathf.Lerp(runningValue, 0.0f, 0.1f);
                break;
        }
        animationTree.Set("parameters/Walk/blend_amount", walkingValue);
        animationTree.Set("parameters/Fall/blend_amount", jumpingValue); //TODO: change T-pose to Jumping
        animationTree.Set("parameters/Run/blend_amount", runningValue);

    }
}