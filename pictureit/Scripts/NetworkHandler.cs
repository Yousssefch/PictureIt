using Godot;
using Godot.Collections;
using System;

public partial class NetworkHandler : Node
{
    PackedScene tubeServer;
    PackedScene tubeClient;
    [Signal] public delegate void SessionCreatedEventHandler(string sessionId);
    [Signal] public delegate void PeerConnectedEventHandler(int peerId);
    [Signal] public delegate void SessionJoinedEventHandler();
    [Export] public string sessionId;
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

    public string GetSessionId()
    {
        return this.sessionId;
    }

    public void SetSessionId(string id)
    {
        this.sessionId = id;
    }

    // Event Callbacks
    private void OnSessionCreated()
    {
        this.sessionId =  GetNode("tube_server").Get("session_id").ToString();
        GD.Print("Session Created with ID: " + this.sessionId);
        EmitSignal(SignalName.SessionCreated, this.sessionId);
    }
    private void OnSessionJoined()
    {
        GD.Print("Session Joined");
        EmitSignal(SignalName.SessionJoined);
    }

    private void OnPeerConnected(int peerId)
    {
        GD.Print("Peer Connected with ID: " + peerId);
        EmitSignal(SignalName.PeerConnected, peerId);
    }
 
}
