using Godot;
using System;

public partial class Winner : Control
{
    [Export] public string winnerName { get; set; } = "";
    [Export] public float score { get; set; } = 0.0f;
    private PanelContainer winnerContainer;
    private PlayerLobby playerLobby;
    private Label scoreLabel;
    private GameController gameController;
    private NetworkHandler networkHandler;

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
        gameController = GetNode<GameController>("/root/GameController");
        networkHandler = GetNode<NetworkHandler>("/root/NetworkHandler");
        winnerContainer = GetNode<PanelContainer>("CanvasLayer/Winner");
        playerLobby = GetNode<PlayerLobby>("CanvasLayer/Winner/VBoxContainer/PlayerLobby");
        scoreLabel = GetNode<Label>("CanvasLayer/Winner/VBoxContainer/ScoreLabel");
        playerLobby.SetPlayerName(winnerName);
        scoreLabel.Text = score.ToString();

    }

    private void OnReturnButtonPressed()
    {
        networkHandler.LeaveSession();
    }
}
