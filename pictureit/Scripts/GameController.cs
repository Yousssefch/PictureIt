using Godot;
using System;
using System.Threading.Tasks;

public partial class GameController : Node
{
    [Export] private PackedScene homeScreenScene;
    [Export] private PackedScene lobbyScene;
    [Export] private PackedScene gameScene;
    [Export] private PackedScene noticeScene;
    [Export] private PackedScene pictureScene;
    private Node pictures;
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
        pictures = GetNode("Pictures");
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

    public void AddPicture(int player_id, Picture picture)
    {
        GD.Print("Adding picture for player ID: " + player_id);
        if(pictures.GetNodeOrNull(player_id.ToString()) == null)
        {
            Node playerPicturesNode = new Node();
            playerPicturesNode.Name = player_id.ToString();
            pictures.AddChild(playerPicturesNode);
        }
        pictures.GetNode(player_id.ToString()).AddChild(picture);
        _ = picture.AnimationTakePicture();
    }

    public void ShowNotice(string message)
    {
        var noticeInstance = noticeScene.Instantiate();
        noticeInstance.Set("currentMessage", message);
        AddChild(noticeInstance);
    }

    public void CreatePicture(Image texture, Vector3 position, Vector3 rotation, float fov, float warmth, int player_id)
    {
        Picture picture = pictureScene.Instantiate<Picture>();
        ImageTexture imgTexture = ImageTexture.CreateFromImage(texture);
        Texture2D tex = imgTexture;
        picture.image = tex;
        AddPicture(player_id, picture);

        picture.setTexture(tex);
        picture.SetMetadata(position, rotation, fov, warmth);
        
    }
}
