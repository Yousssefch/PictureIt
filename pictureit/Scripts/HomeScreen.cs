using Godot;
using System;

public partial class HomeScreen : Control
{
    private NetworkHandler network;
    public override void _Ready()
    {
        network = GetNode<NetworkHandler>("/root/NetworkHandler");
    }

    private void OnPressHostButton()
    {
        network.CreateServer();
        GD.Print("Hosting game...");
        GetTree().ChangeSceneToFile("res://Levels/test_car_scene.tscn");
    }
    private void OnPressJoinButton()
    {
        network.CreateClient();
        GetTree().ChangeSceneToFile("res://Levels/test_car_scene.tscn");
    }
}
