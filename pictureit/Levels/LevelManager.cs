using Godot;
using System;

public partial class LevelManager : Node
{
    [Signal] public delegate void OnReadyToDisposeLevelEventHandler();

    public void NotifyReadyToDisposeLevel()
    {
        GD.Print("LevelManager: Notifying that level is ready to be disposed.");
        EmitSignal(SignalName.OnReadyToDisposeLevel);
    }

}
