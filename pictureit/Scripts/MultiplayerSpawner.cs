using Godot;
using System;

public partial class MultiplayerSpawner : Godot.MultiplayerSpawner
{
    [Export] private PackedScene playerScene;
    public override void _Ready()
    {
        if (Multiplayer.IsServer())
        {
            SpawnPlayer(Multiplayer.GetUniqueId());
        }
        Multiplayer.PeerConnected += OnPeerConnected;
    }
    private void OnPeerConnected(long peerId)
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
