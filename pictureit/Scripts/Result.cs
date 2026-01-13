using Godot;
using System;
using System.Threading.Tasks;

public partial class Result : Control
{
    private GameController gameController;
    AnimationPlayer animationPlayer;
    private Godot.Collections.Dictionary<int, Godot.Collections.Array<Picture>> playerPictures = new Godot.Collections.Dictionary<int, Godot.Collections.Array<Picture>>();
    [Export] private PackedScene pictureScene;
    private PanelContainer playerScore;
    private PlayerLobby playerLobby;
    private Label scoreLabel;
    Node Pictures;
    string winnerName = "";
    float score = 0.0f;
    public override void _Ready()
    {
        gameController = GetNode<GameController>("/root/GameController");
        Pictures = gameController.GetNode("Pictures");
        playerScore = GetNode<PanelContainer>("CanvasLayer/PlayerScore");
        playerLobby = playerScore.GetNode<PlayerLobby>("VBoxContainer/PlayerLobby");
        scoreLabel = playerScore.GetNode<Label>("VBoxContainer/ScoreLabel");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

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

                if(playerScore >= score)
                {
                    score = playerScore;
                    winnerName = gameController.GetPlayerName(playerId);
                    gameController.SetWinner(winnerName, score);
                }
            }
        }
       await SwitchScene();
    }

    private async Task SwitchScene()
    {
        animationPlayer.PlayBackwards("Init");
        await ToSignal(animationPlayer, "animation_finished");
        await gameController.LoadLevel(GameController.Levels.Winner);
    }
    private void SetPlayerScore(float score)
    {
        scoreLabel.Text = score.ToString();
    }

    private async Task IncrementScoreLabel(float targetScore)
    {
        float displayedScore = 0;
        float increment = targetScore / 100f; // Increment in 100 steps
        while(displayedScore < targetScore)
        {
            displayedScore += increment;
            if(displayedScore > targetScore)
                displayedScore = targetScore;

        }
    }
}
