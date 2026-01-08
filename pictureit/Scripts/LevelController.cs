using Godot;
using System;

public partial class LevelController : Node3D
{
    [Export] private float _levelDuration = 60f; // Duration of the level in seconds
    private float _elapsedTime = 0f;
    private Timer _levelTimer;
    private NetworkHandler networkHandler;
    CanvasLayer hudLayer;
    TimerUI timerUI;

    public override void _Ready()
    {
        _levelTimer = GetNode<Timer>("LevelTimer");
        _levelTimer.Connect("timeout", new Callable(this, nameof(OnTimerTimeout)));
        _levelTimer.Start(1f); // Timer ticks every second
        _levelTimer.Autostart = true;

        networkHandler = GetNode<NetworkHandler>("/root/NetworkHandler");
        hudLayer = GetNode<CanvasLayer>("HUD");
        timerUI = hudLayer.GetNode<TimerUI>("TimerUI");
        timerUI.SetLevelDuration(_levelDuration);

    }

    private void OnTimerTimeout()
    {
        if(!networkHandler.IsServer()) return;
        _elapsedTime += 1f;
        Rpc(MethodName.SetElapsedTime, _elapsedTime);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void SetElapsedTime(float time)
    {
        _elapsedTime = time;
        float timeLeft = _levelDuration - _elapsedTime;
        timerUI.UpdateTimer(timeLeft);
    }
}
