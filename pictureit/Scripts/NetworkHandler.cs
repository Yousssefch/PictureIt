using Godot;
using Godot.Collections;
using System;

public partial class NetworkHandler : Node
{
    PackedScene tubeServer;
    PackedScene tubeClient;
    override public void _Ready()
    {
        tubeServer = GD.Load<PackedScene>("res://Objects/Other/tube_server.tscn");
        tubeClient = GD.Load<PackedScene>("res://Objects/Other/tube_join.tscn");
    }

    override public void _Process(double delta)
    {
    }

    public void CreateServer()
    {
        var serverInstance = tubeServer.Instantiate();
        AddChild(serverInstance);
        
    }

    public void CreateClient(string oid)
    {
        var clientInstance = tubeClient.Instantiate();
        clientInstance.Set("session_id", oid);
        AddChild(clientInstance);
    }

 
}
