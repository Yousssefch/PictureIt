using Godot;
using System;

public partial class Pivot : Node3D
{
    [Export] public float MouseSensitivity { get; set; } = 0.005f;

    public override void _Ready()
    {
    }

    public override void _Input(InputEvent @event)
    {
       if(@event is InputEventMouseMotion)
       {
           var MM = (InputEventMouseMotion)@event;
           Rotation = new Vector3(Rotation.X - MM.Relative.Y * MouseSensitivity, Rotation.Y - MM.Relative.X * MouseSensitivity, Rotation.Z);}
    }

    public override void _Process(double delta)
    {
    }
}
