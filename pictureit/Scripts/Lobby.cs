using Godot;
using System;
using System.Threading.Tasks;

public partial class Lobby : Control
{
    [Export] private string sessionIdLabel;
    private Label roomIDLabel;
    private NetworkHandler network;
    private CanvasLayer canvasLayer;
    private AnimationPlayer animationPlayer;
    private LevelManager levelManager;
    private GameController gameController;
    public override void _Ready()
    {
        network = GetNode<NetworkHandler>("/root/NetworkHandler");
        roomIDLabel = GetNode<Label>("CanvasLayer/RoomID");
        canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        levelManager = GetNode<LevelManager>("/root/GameController/Level");
        gameController = GetNode<GameController>("/root/GameController");
        
        if(Multiplayer.IsServer())
        {
            animationPlayer.Play("Init", customSpeed: 1.5f);
            this.UpdateRoomIDLabel();
        }
        else
        {
            this.ToggleVisibility(false);
            levelManager.OnReadyToDisposeLevel += LevelReadyToDispose;
        }
    }

    private async void LevelReadyToDispose()
    {
        GD.Print("Lobby notified that previous level is ready to be disposed.");
        
        //Client in the scene
        this.ToggleVisibility(true);
        this.UpdateRoomIDLabel();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        gameController.ClearLevel(GameController.Levels.HomeScreen); //Clear previous level
        float animationSpeed = 1.5f;
        animationPlayer.Play("Init", customSpeed: animationSpeed);
    }

    private async void OnStartButtonPressed()
    {
       if(Multiplayer.IsServer())
       {
            await gameController.LoadLevel(GameController.Levels.Game);
       }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public async Task RpcStartGame()
    {
        await gameController.LoadLevel(GameController.Levels.Game);
    }

    private void ToggleVisibility(bool isVisible)
    {
        this.Visible = isVisible;
        canvasLayer.Visible = isVisible;
    }

    private void UpdateRoomIDLabel()
    {
        string sessionId = network.GetSessionId();
        roomIDLabel.Text = "Room ID: " + sessionId;
    }

    private void OnBackButtonPressed()
    {
        network.LeaveSession();
    }

    public override void _ExitTree()
    {
        levelManager.OnReadyToDisposeLevel -= LevelReadyToDispose;
    }
}
