using Godot;
using System;

public partial class Result : Control
{
    private GameController gameController;
    private Godot.Collections.Dictionary<int, Godot.Collections.Array<Picture>> playerPictures = new Godot.Collections.Dictionary<int, Godot.Collections.Array<Picture>>();
    [Export] private PackedScene pictureScene;
    private PanelContainer playerScore;
    private PlayerLobby playerLobby;
    private Label scoreLabel;
    Node Pictures;
    public override void _Ready()
    {
        gameController = GetNode<GameController>("/root/GameController");
        Pictures = gameController.GetNode("Pictures");
        playerScore = GetNode<PanelContainer>("CanvasLayer/PlayerScore");
        playerLobby = playerScore.GetNode<PlayerLobby>("PlayerLobby");
        scoreLabel = playerScore.GetNode<Label>("ScoreLabel");

        ResultAnimation();
    }

    private async void ResultAnimation()
    {
        foreach(Node node in Pictures.GetChildren())
        {
            int playerId = int.Parse(node.Name);
            playerLobby.SetPlayerName( gameController.GetPlayerName(playerId));
            float playerScore = 0;

            foreach(Picture pic in node.GetChildren())
            {
                playerScore += pic.GetScore();
                SetPlayerScore(playerScore);
                await pic.AnimationEvaluate();
                pic.QueueFree();
            }
        }
    }

    private void SetPlayerScore(float score)
    {
        scoreLabel.Text = score.ToString();
    }
}
