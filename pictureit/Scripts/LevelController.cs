using Godot;
using System;
using System.Threading.Tasks;

public partial class LevelController : Node3D
{
    [Export] private float _levelDuration = 60f; // Duration of the level in seconds
    private float _elapsedTime = 0f;
    private Timer _levelTimer;
    private NetworkHandler networkHandler;
    CanvasLayer hudLayer;
    TimerUI timerUI;
    GameController gameController;
    AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        _levelTimer = GetNode<Timer>("LevelTimer");
        _levelTimer.Connect("timeout", new Callable(this, nameof(OnTimerTimeout)));
        _levelTimer.Start(1f); // Timer ticks every second
        _levelTimer.Autostart = true;

        networkHandler = GetNode<NetworkHandler>("/root/NetworkHandler");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        hudLayer = GetNode<CanvasLayer>("HUD");
        timerUI = hudLayer.GetNode<TimerUI>("TimerUI");
        timerUI.SetLevelDuration(_levelDuration);
        gameController = GetNode<GameController>("/root/GameController");

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

        if (_elapsedTime >= _levelDuration)
        {
            _levelTimer.Stop();
            _ = EndLevel();
        }
    }

    private async Task EndLevel()
    {
        animationPlayer.Play("Transition_Out");
        await ToSignal(animationPlayer, "animation_finished");
        await gameController.LoadLevel(GameController.Levels.Results);
    }
    public async void UpdateObjectives()
    {
        Objectives objectives = hudLayer.GetNode<Objectives>("Objectives");
        await objectives.UpdateReferencePicture();
    }

    public Godot.Collections.Dictionary<string, Vector3> GetCurrentReferencePictureMetaData()
    {
        Objectives objectives = hudLayer.GetNode<Objectives>("Objectives");
        Godot.Collections.Dictionary<string, Vector3> referenceMetadata = objectives.GetCurrentReferencePictureMetaData();
        GD.Print("Reference Metadata: ", referenceMetadata);
        return referenceMetadata;
    }

    public void HideHUD()
    {
        GD.Print("Hiding HUD");
        hudLayer.Visible = false;
    }
    public void ShowHUD()
    {
        GD.Print("Showing HUD");
        hudLayer.Visible = true;
    }
}
