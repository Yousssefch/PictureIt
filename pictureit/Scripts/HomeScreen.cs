using Godot;
using System;

public partial class HomeScreen : Control
{
    private NetworkHandler network;
    private LineEdit oidInput;
    public override void _Ready()
    {
        network = GetNode<NetworkHandler>("/root/NetworkHandler");
        oidInput = GetNode<LineEdit>("VBoxContainer/OIDInput");
    }

    private void OnPressHostButton()
    {
        network.CreateServer();
        GD.Print("Hosting game...");
        GetTree().ChangeSceneToFile("res://Levels/test_car_scene.tscn");
    }
    private void OnPressJoinButton()
    {
        string oid = oidInput.Text;
        GD.Print("Joining game with OID: " + oid);
        network.CreateClient(oid);
        GetTree().ChangeSceneToFile("res://Levels/test_car_scene.tscn");
    }
}
