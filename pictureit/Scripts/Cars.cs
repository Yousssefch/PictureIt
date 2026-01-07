using Godot;
using System;
using System.Runtime;

public partial class Cars : CharacterBody3D
{
    [Export] public Godot.Collections.Array<Vector3> TargetPositions { get; set; }
    [Export] public float Speed = 50.0f;
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
        currentTargetPosition.Y = this.Position.Y; // Keep target position at the same height as the car
        if(targetPositionsCount == 0) return; // No target positions defined

        // Move towards the current target position
        Vector3 directionToTarget = (currentTargetPosition - this.Position).Normalized();
        directionToTarget.Y = 0; // Keep movement in the horizontal plane

        //Calculate Distance to target
        float distanceToTarget = this.Position.DistanceTo(currentTargetPosition);
        float stepDistance = 0.05f; // Distance covered in this frame
        if(distanceToTarget < stepDistance)
        {
            this.Position = currentTargetPosition;
            currentTargetIndex = (currentTargetIndex + 1) % targetPositionsCount;
            currentTargetPosition = TargetPositions[currentTargetIndex];
            directionToTarget = (currentTargetPosition - this.Position).Normalized();
        }
        this.Velocity = directionToTarget * Speed;
        
        AdjustRotation(directionToTarget);
        MoveAndSlide();
    }

    void AdjustRotation(Vector3 direction)
    {
        float directionThreshold = 0.5f; // Threshold to determine significant direction
        switch (direction)
        {
            case Vector3 dir when dir.X > directionThreshold:
                this.Rotation = new Vector3(0, Mathf.DegToRad(90), 0);
                break;
            case Vector3 dir when dir.X < -directionThreshold:
                this.Rotation = new Vector3(0, Mathf.DegToRad(-90), 0);
                break;
            case Vector3 dir when dir.Z > directionThreshold :
                this.Rotation = new Vector3(0, 0, 0);
                break;
            case Vector3 dir when dir.Z < -directionThreshold:
                this.Rotation = new Vector3(0, Mathf.DegToRad(180), 0);
                break;
        }
    }
}
