using Godot;
using System;

public partial class MultiplayerSpawner : Godot.MultiplayerSpawner
{
    [Export] private PackedScene playerScene;
    public override void _Ready()
    {
        if (!Multiplayer.IsServer())
            return;

        foreach (var peerId in Multiplayer.GetPeers())
        {
            GD.Print("Spawning player for peer ID: " + peerId);
            SpawnPlayer(peerId);
        }
        SpawnPlayer(Multiplayer.GetUniqueId());
       
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
    }
}
