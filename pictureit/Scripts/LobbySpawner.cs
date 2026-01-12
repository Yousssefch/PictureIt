using Godot;
using System;
using System.Threading.Tasks;

public partial class LobbySpawner : Godot.MultiplayerSpawner
{
    [Export] private PackedScene playerScene;
    private NetworkHandler network;
    private GameController gameController;
    public override async void _Ready()
    {
        network = GetNode<NetworkHandler>("/root/NetworkHandler");
        gameController = GetNode<GameController>("/root/GameController");
        if (Multiplayer.IsServer())
        {
            SpawnPlayer(Multiplayer.GetUniqueId());
            network.PeerConnected += OnPeerConnected;
            network.PeerDisconnected += OnPeerDisconnected;
        }
    }
    private void OnPeerConnected(int peerId)
    {
        if (!Multiplayer.IsServer()) return;
        SpawnPlayer(peerId);
    }

    private void OnPeerDisconnected(int peerId)
    {
        if (!Multiplayer.IsServer()) return;
        var playerNode = GetNodeOrNull<Node>($"{SpawnPath}/{peerId}");
        if (playerNode != null)
        {
            playerNode.QueueFree();
        }
    }

    private void SpawnPlayer(long peerId)
    {
        var playerInstance = playerScene.Instantiate<PlayerLobby>();
        playerInstance.PlayerName = gameController.GetPlayerName((int)peerId);
        playerInstance.Name = peerId.ToString();
        GetNode(SpawnPath).CallDeferred("add_child", playerInstance);
    }

    public override void _ExitTree()
    {
        if (Multiplayer.IsServer())
        {
            network.PeerConnected -= OnPeerConnected;
            network.PeerDisconnected -= OnPeerDisconnected;
        }
    }

}
