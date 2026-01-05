using Godot;
using System;

public partial class LevelSpawner : Godot.MultiplayerSpawner
{
    [Export] private PackedScene levelScene;
    private NetworkHandler network;
    private LevelManager levelManager;
    public override async void _Ready()
    {
        network = GetNode<NetworkHandler>("/root/NetworkHandler");
        levelManager = GetNode<LevelManager>("/root/GameController/Level");

        if (Multiplayer.IsServer())
        {
            SpawnLevel(Multiplayer.GetUniqueId());
            network.PeerConnected += OnPeerConnected;
        }
    }
    private async void OnPeerConnected(int peerId)
    {
        if (!Multiplayer.IsServer()) return;
        SpawnLevel(peerId);
    }

    private void SpawnLevel(long peerId)
    {
        var levelInstance = levelScene.Instantiate();
        GetNode(SpawnPath).CallDeferred("add_child", levelInstance);
    }
}
