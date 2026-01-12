using Godot;
using System;

public partial class PlayerLobby : PanelContainer
{
    [Export] private bool isHost = false;
    [Export] private bool isInMultiplayerLobby = true;
    [Export] public string PlayerName { get; set; } = "Player";
    private Label playerNameLabel;
    private GameController gameController;

    public override void _EnterTree()
    {
        if(!isInMultiplayerLobby) return;
        SetMultiplayerAuthority(int.Parse(this.Name));
        gameController = GetNode<GameController>("/root/GameController");
    }

    public override void _Ready()
    {
        playerNameLabel = GetNode<Label>("VBoxContainer/player_name");
        playerNameLabel.Text = PlayerName;
    }

    public override void _Process(double delta)
    {
        if(!isInMultiplayerLobby) return;
        string displayName = gameController.GetPlayerName(int.Parse(this.Name));
        playerNameLabel.Text = displayName;
    }

    public void SetPlayerName(string name)
    {
        PlayerName = name;
        playerNameLabel.Text = PlayerName;
    }


}
