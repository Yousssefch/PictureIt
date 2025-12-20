using Godot;
using System;

public partial class Cars : CharacterBody3D
{
    [Export] public Godot.Collections.Array<Vector3> TargetPositions { get; set; }
    [Export] public float Speed = 1.0f;
    int currentTargetIndex = 0;
    int targetPositionsCount = 0;
    Vector3 currentTargetPosition;

    override public void _Ready()
    {
        targetPositionsCount = TargetPositions.Count;
        if(targetPositionsCount > 0)
        {
            currentTargetPosition = TargetPositions[0]; //first target position

        }
        
    }

    override public void _Process(double delta)
    {
        if(targetPositionsCount == 0) return; // No target positions defined

        // Move towards the current target position
        Vector3 direction = Vector3.Zero;

        if(this.GlobalPosition.DistanceTo(currentTargetPosition) <= 0.01f)
        {
            this.GlobalPosition = currentTargetPosition;
            currentTargetIndex = (currentTargetIndex + 1) % targetPositionsCount;
            currentTargetPosition = TargetPositions[currentTargetIndex];
        }

        this.LookAt(currentTargetPosition, Vector3.Up);
        Vector3 directionToTarget = (currentTargetPosition - this.GlobalPosition).Normalized();
        this.Velocity = directionToTarget * Speed;
        
        MoveAndSlide();
         
        
    }
}
