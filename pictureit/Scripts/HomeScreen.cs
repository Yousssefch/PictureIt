using Godot;
using System;
using System.Threading.Tasks;

public partial class HomeScreen : Control
{
    private NetworkHandler network;
    private LineEdit oidInput;
    private Godot.TextureRect transition;
    private ColorRect blur;
    private ColorRect transparent;
    private Label loading;
    AnimationPlayer animationPlayer;
    GameController gameController;
    LevelManager levelManager;
    [Export] private PackedScene enterNameScene;
    public override void _Ready()
    {
        network = GetNode<NetworkHandler>("/root/NetworkHandler");
        oidInput = GetNode<LineEdit>("CanvasLayer/VBoxContainer/HBoxContainer/OIDInput");
        transition = GetNode<Godot.TextureRect>("CanvasLayer/Transition");
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
        await EndLoadingAnimation();

        //Add enter name screen
        AddEnterNameScreen();
    }

    private async void OnSessionJoined()
    {
        string oid = oidInput.Text;
        network.SetSessionId(oid);
        await EndLoadingAnimation();
        AddEnterNameScreen();
    }

    private void AddEnterNameScreen()
    {
        //Add enter name screen
        CanvasLayer enterNameControl = enterNameScene.Instantiate<CanvasLayer>();
        AddChild(enterNameControl);

        Button submitButton = enterNameControl.GetNode<Button>("VBoxContainer/InputPanel/VBoxContainer/Button");
        LineEdit nameinput = enterNameControl.GetNode<LineEdit>("VBoxContainer/InputPanel/VBoxContainer/VBoxContainer/LineEdit");
        submitButton.Pressed += async() => await OnEnterNameSubmitted(nameinput.Text);
    }

    private async Task OnEnterNameSubmitted(string playerName)
    {
        gameController.SetPlayerName(Multiplayer.GetUniqueId(), playerName);

        //Exit Enter Name
        AnimationPlayer enterNameAnimationPlayer = GetNode<CanvasLayer>("PlayerNameSelect").GetNode<AnimationPlayer>("AnimationPlayer");
        enterNameAnimationPlayer.PlayBackwards("Init");
        await ToSignal(enterNameAnimationPlayer, "animation_finished");
        GetNode<CanvasLayer>("PlayerNameSelect").QueueFree();

        //Notify level manager to dispose home screen
        await ExitAnimation();
        
        if(!Multiplayer.IsServer()) levelManager.NotifyReadyToDisposeLevel();
        else _ = gameController.LoadLevel(GameController.Levels.Lobby);
    }

    private async Task EndLoadingAnimation()
    {
        animationPlayer.PlayBackwards("Loading");
        await ToSignal(
        animationPlayer,
        AnimationPlayer.SignalName.AnimationFinished
    );
    }

    private async Task ExitAnimation()
    {

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
