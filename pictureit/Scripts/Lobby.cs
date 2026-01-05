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
            animationPlayer.Play("Init");
        }
        else
        {
            this.Visible = false;
            canvasLayer.Visible = false;
            levelManager.OnReadyToDisposeLevel += LevelReadyToDispose;
        }

        string sessionId = network.GetSessionId();
        roomIDLabel.Text = "Room ID: " + sessionId;
    }

    private void LevelReadyToDispose()
    {
        GD.Print("Lobby notified that previous level is ready to be disposed.");
        this.Visible = true;
        canvasLayer.Visible = true;
        animationPlayer.Play("Init");
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
}
