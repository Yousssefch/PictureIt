using Godot;
using System;
using System.Threading.Tasks;

public partial class LobbySpawner : Godot.MultiplayerSpawner
{
    [Export] private PackedScene playerScene;
    private NetworkHandler network;
    public override async void _Ready()
    {
        network = GetNode<NetworkHandler>("/root/NetworkHandler");
        if (Multiplayer.IsServer())
        {
            SpawnPlayer(Multiplayer.GetUniqueId());
            network.PeerConnected += OnPeerConnected;
        }
    }
    private void OnPeerConnected(int peerId)
    {
        if (!Multiplayer.IsServer()) return;
        SpawnPlayer(peerId);
    }

    private void SpawnPlayer(long peerId)
    {
        var playerInstance = playerScene.Instantiate();
        playerInstance.Name = peerId.ToString();
        GetNode(SpawnPath).CallDeferred("add_child", playerInstance);
        GD.Print("Peer connected with ID: " + playerInstance.Name);
    }

}
