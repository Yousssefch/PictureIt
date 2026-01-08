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
    [Signal] public delegate void FailedToCreateSessionEventHandler();
    [Signal] public delegate void FailedToJoinSessionEventHandler();
    [Export] public string sessionId;
    Timer connectionTimer;
    bool isConnected = false;
    bool isConnecting = false;
    override public void _Ready()
    {
        tubeServer = GD.Load<PackedScene>("res://Objects/Other/tube_server.tscn");
        tubeClient = GD.Load<PackedScene>("res://Objects/Other/tube_join.tscn");
        connectionTimer = GetNode<Timer>("ConnectionTimer");
    }

    override public void _Process(double delta)
    {
    }

    public void CreateServer()
    {
        var serverInstance = tubeServer.Instantiate();
        AddChild(serverInstance);
        isConnecting = true;
    }

    public void CreateClient(string oid)
    {
        var clientInstance = tubeClient.Instantiate();
        clientInstance.Set("session_id", oid);
        AddChild(clientInstance);

        connectionTimer.Start();
        isConnecting = true;
    }

    private Timer InitTimer(float time)
    {
        Timer timer = new Timer();
        timer.WaitTime = time;
        timer.OneShot = true;

        return timer;
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
        isConnected = true;
        isConnecting = false;
    }
    private void OnSessionJoined()
    {
        GD.Print("Session Joined");
        EmitSignal(SignalName.SessionJoined);
        isConnected = true;
        isConnecting = false;
    }

    private void OnPeerConnected(int peerId)
    {
        GD.Print("Peer Connected with ID: " + peerId);
        EmitSignal(SignalName.PeerConnected, peerId);
    }

    private void OnTubeClientError(Variant code,string error)
    {
        if(code.ToString() == "1" && connectionTimer.IsStopped() == false) // Failed to join session
        {
            connectionTimer.Stop();
            OnFailedToJoinSession();
            isConnecting = false;
        }
    }

    private void OnFailedToCreateSession()
    {
        EmitSignal(SignalName.FailedToCreateSession);
        GD.Print("Failed to create session");
        removeTubeNodes();
    }

    private void OnFailedToJoinSession()
    {
        EmitSignal(SignalName.FailedToJoinSession);
        GD.Print("Failed to join session");
        removeTubeNodes();

    }

    private void removeTubeNodes()
    {
        if (HasNode("tube_server"))
        {
            GetNode("tube_server").QueueFree();
        }
        if (HasNode("tube_join"))
        {
            GetNode("tube_join").QueueFree();
        }
    }

    private void TimeOut()
    {
        OnFailedToJoinSession();
    }

    public bool IsServer()
    {
        if (HasNode("tube_server"))
        {
            return true;
        }
        return false;
    }
 
}
