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
    [Export] private PackedScene resultsScene;
    private NetworkHandler networkHandler;
    private Node pictures;
    private Node Level;
    public enum Levels
    {
        HomeScreen,
        Lobby,
        Game,
        Results
    }
    private Levels currentLevel = Levels.HomeScreen;
    private int maxPlayers = 4;
    private Godot.Collections.Dictionary<int, string> playerNames = new Godot.Collections.Dictionary<int, string>();

    public override async void _Ready()
    {
        Level = GetNode("Level");
        pictures = GetNode("Pictures");
        networkHandler = GetNode<NetworkHandler>("/root/NetworkHandler");
        networkHandler.PeerConnected += OnPeerConnected;
        networkHandler.SessionCreated += OnSessionCreated;
        networkHandler.SessionLeft += OnSessionLeft;
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
            case Levels.Results:
                instance = resultsScene.Instantiate();
                instance.AddToGroup("Results");
                break;
        }
        currentLevel = level;
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
                case Levels.Results:
                    if (child.IsInGroup("Results"))
                    {
                        child.QueueFree();
                    }
                    break;
            }
        }
    }

    public void SetPlayerName(int player_id, string name)
    {
        playerNames[player_id] = name;
        Rpc(MethodName.BroadcastPlayerName, player_id, name);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void BroadcastPlayerName(int player_id, string name)
    {
        GD.Print("Broadcasting name for player ID: " + player_id + " Name: " + name);
        playerNames[player_id] = name;
    }
    public string GetPlayerName(int player_id)
    {
        if(playerNames.ContainsKey(player_id))
        {
            return playerNames[player_id];
        }
        return "Player " + player_id;
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

        if(Multiplayer.GetUniqueId() == player_id) _ = picture.AnimationTakePicture();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public void SendPictureToOtherPeers(byte[] imageData, Vector3 position, Vector3 rotation, float fov, float warmth, int player_id, Godot.Collections.Dictionary<string, Vector3> referenceMetadata)
    {
        GD.Print("Received picture from player ID: " + player_id);
        Image img = new Image();
        img.LoadPngFromBuffer(imageData);
        CreatePicture(img, position, rotation, fov, warmth, player_id, referenceMetadata);
    }

    public void ShowNotice(string message)
    {
        var noticeInstance = noticeScene.Instantiate();
        noticeInstance.Set("currentMessage", message);
        AddChild(noticeInstance);
    }

    public void CreatePicture(Image texture, Vector3 position, Vector3 rotation, float fov, float warmth, int player_id, Godot.Collections.Dictionary<string, Vector3> referenceMetadata, bool saveToReferences = false)
    {
        Picture picture = pictureScene.Instantiate<Picture>();
        ImageTexture imgTexture = ImageTexture.CreateFromImage(texture);
        Texture2D tex = imgTexture;
        picture.image = tex;
        byte[] imgData = texture.SavePngToBuffer();

        //Debug: Show all reference metadata

        
        AddPicture(player_id, picture);
        picture.setTexture(tex);


        Godot.Collections.Dictionary<string, Vector3> metadata = new Godot.Collections.Dictionary<string, Vector3>();
        metadata["position"] = position;
        metadata["rotation"] = rotation;
        metadata["fov"] = new Vector3(fov, 0, 0);
        metadata["warmth"] = new Vector3(warmth, 0, 0);
        picture.SetMetadata(metadata);

        if(saveToReferences)
        {
            GD.Print("Saving picture to references for player ID: " + player_id);
            AddPictureToReferences(picture);
            return;
        }

        picture.SetReferenceMetadata(referenceMetadata);
        picture.CalculateScoreBasedOnMetadata();
        
    }

    public void AddPictureToReferences(Picture picture)
    {
        DirAccess dir = DirAccess.Open("res://References");
        if(dir != null)
        {
            string filePath = "res://References/picture_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".tscn";
            PackedScene packedScene = new PackedScene();
            Picture pictureNode = picture.Duplicate() as Picture;
            pictureNode.SetMetadata(picture.GetMetadata());
            packedScene.Pack(pictureNode);
            ResourceSaver.Save(packedScene, filePath);
            
            GD.Print("Saved picture to: " + filePath);
        }
    }

    public Godot.Collections.Array<Picture> GetPlayerPictures(int player_id)
    {
        Godot.Collections.Array<Picture> playerPictures = new Godot.Collections.Array<Picture>();
        Node playerNode = pictures.GetNodeOrNull(player_id.ToString());
        if (playerNode != null)
        {
            foreach (Node child in playerNode.GetChildren())
            {
                if (child is Picture picture)
                {
                    playerPictures.Add(picture);
                }
            }
        }
        return playerPictures;
    }

    public Godot.Collections.Dictionary<int, string> GetPlayers()
    {
        return playerNames;
    }

    private void OnSessionCreated(string sessionId)
    {
        playerNames.Clear();
        playerNames[Multiplayer.GetUniqueId()] = "Player " + Multiplayer.GetUniqueId();
    }

    private void OnPeerConnected(int id)
    {
        if (!Multiplayer.IsServer()) return;

        foreach (var kv in playerNames)
        {
            Rpc(MethodName.BroadcastPlayerName, kv.Key, kv.Value);
        }
        
        GD.Print("Current Players: " + playerNames.Count);
    }

    private void OnSessionLeft()
    {
        playerNames.Clear();
        _ = LoadLevel(Levels.HomeScreen);
    }
}
