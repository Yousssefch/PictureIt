using Godot;
using System;
using System.Threading.Tasks;

public partial class HomeScreen : Control
{
    private NetworkHandler network;
    private LineEdit oidInput;
    private ColorRect transition;
    private ColorRect blur;
    private ColorRect transparent;
    private Label loading;
    AnimationPlayer animationPlayer;
    GameController gameController;
    LevelManager levelManager;
    public override void _Ready()
    {
        network = GetNode<NetworkHandler>("/root/NetworkHandler");
        oidInput = GetNode<LineEdit>("CanvasLayer/VBoxContainer/HBoxContainer/OIDInput");
        transition = GetNode<ColorRect>("CanvasLayer/Transition");
        blur = GetNode<ColorRect>("CanvasLayer/Blur");
        loading = GetNode<Label>("CanvasLayer/Loading");
        transparent = GetNode<ColorRect>("CanvasLayer/Transparent");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        gameController = GetNode<GameController>("/root/GameController");
        levelManager = gameController.GetNode<LevelManager>("Level");

        network.FailedToJoinSession+= OnSessionNotAbleToJoin;

        //set z index of loading component to -1
        loadingComponentInvisible();

        network.SessionCreated += OnSessionCreated;
        network.SessionJoined += OnSessionJoined;

    }

    private async void OnPressHostButton()
    {
        loadingComponentVisible();
        animationPlayer.Play("Loading");

        await ToSignal(animationPlayer, "animation_finished");
        network.CreateServer();
    }
    private async void OnPressJoinButton()
    {
        string oid = oidInput.Text;
        loadingComponentVisible();

        animationPlayer.Play("Loading");
        await ToSignal(animationPlayer, "animation_finished");

        network.CreateClient(oid);
    }

    private void loadingComponentVisible()
    {
        transition.Visible = true;
        blur.Visible = true;
        transparent.Visible = true;
        loading.Visible = true;
    }

    private void loadingComponentInvisible()
    {
        transition.Visible = false;
        blur.Visible = false;
        transparent.Visible = false;
        loading.Visible = false;
    }

    private async void OnSessionCreated(string sessionId)
    {
        await ExitAnimation();
        if(Multiplayer.IsServer())
        {
            await gameController.LoadLevel(GameController.Levels.Lobby);
            levelManager.NotifyReadyToDisposeLevel();
        }
    }

    private async void OnSessionJoined()
    {
        string oid = oidInput.Text;
        network.SetSessionId(oid);
        await ExitAnimation();
        levelManager.NotifyReadyToDisposeLevel();
        gameController.ClearLevel(GameController.Levels.HomeScreen);

    }

    private async Task ExitAnimation()
    {
        animationPlayer.Stop();
        animationPlayer.PlayBackwards("Loading");
        await ToSignal(
        animationPlayer,
        AnimationPlayer.SignalName.AnimationFinished
    );

        float animationSpeed = 1.5f;
        animationPlayer.Play("Transition", customSpeed: animationSpeed);
        await ToSignal(
        animationPlayer,
        AnimationPlayer.SignalName.AnimationFinished
        );
        
    }

    private void OnSessionNotAbleToJoin()
    {
        loadingComponentInvisible();
        animationPlayer.PlayBackwards("Loading");
        gameController.ShowNotice("Failed to join session.");
    }


}
