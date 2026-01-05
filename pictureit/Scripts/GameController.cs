using Godot;
using System;
using System.Threading.Tasks;

public partial class GameController : Node
{
    [Export] private PackedScene homeScreenScene;
    [Export] private PackedScene lobbyScene;
    [Export] private PackedScene gameScene;
    private Node Level;
    public enum Levels
    {
        HomeScreen,
        Lobby,
        Game
    }

    public override async void _Ready()
    {
        Level = GetNode("Level");
    }

    public async Task LoadLevel(Levels level)
    {
        ClearAllLevels();
        Node defaultInstance = homeScreenScene.Instantiate();
        Node instance = defaultInstance;
        switch (level)
        {
            case Levels.HomeScreen:
                instance = homeScreenScene.Instantiate();
                instance.AddToGroup("HomeScreen");
                break;
            case Levels.Lobby:
                instance = lobbyScene.Instantiate();
                instance.AddToGroup("Lobby");
                break;
            case Levels.Game:
                instance = gameScene.Instantiate();
                instance.AddToGroup("Game");
                break;
        }
        Level.CallDeferred("add_child", instance);
    }

    private void ClearAllLevels()
    {
        foreach (Node child in Level.GetChildren())
        {
            child.QueueFree();
        }
    }
    public void ClearLevel(Levels level)
    {
        foreach (Node child in Level.GetChildren())
        {
            switch (level)
            {
                case Levels.HomeScreen:
                    if (child.IsInGroup("HomeScreen"))
                    {
                        child.QueueFree();
                    }
                    break;
                case Levels.Lobby:
                    if (child.IsInGroup("Lobby"))
                    {
                        GD.Print("Clearing HomeScreen");
                        child.QueueFree();
                    }
                    break;
                case Levels.Game:
                    if (child.IsInGroup("Game"))
                    {
                        child.QueueFree();
                    }
                    break;
            }
        }
    }
}
