using Godot;
using System;

public partial class MultiplayerSpawner : Godot.MultiplayerSpawner
{
    [Export] private PackedScene playerScene;
    public override void _Ready()
    {
        if(!Multiplayer.IsServer()) return;
            
        SpawnPlayer(Multiplayer.GetUniqueId());

        foreach (var peerId in Multiplayer.GetPeers())
        {
            SpawnPlayer(peerId);
        }
       
    }
    private void OnPeerConnected(long peerId)
    {
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
