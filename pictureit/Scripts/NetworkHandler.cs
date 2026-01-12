using Godot;
using Godot.Collections;
using System;

public partial class NetworkHandler : Node
{
    [Signal] public delegate void SessionCreatedEventHandler(string sessionId);
    [Signal] public delegate void PeerConnectedEventHandler(int peerId);
    [Signal] public delegate void PeerDisconnectedEventHandler(int peerId);
    [Signal] public delegate void SessionJoinedEventHandler();
    [Signal] public delegate void FailedToCreateSessionEventHandler();
    [Signal] public delegate void FailedToJoinSessionEventHandler();
    [Signal] public delegate void SessionLeftEventHandler();
    [Export] public string sessionId;
    Timer connectionTimer;
    bool isConnected = false;
    bool isConnecting = false;
    Node tubeManager;
    override public void _Ready()
    {
        tubeManager = GetNode("TubeManager");
        connectionTimer = GetNode<Timer>("ConnectionTimer");
    }

    override public void _Process(double delta)
    {
    }

    public void CreateServer()
    {
        tubeManager.Call("create_session");
        isConnecting = true;
    }

    public void CreateClient(string oid)
    {
        tubeManager.Call("join_session", oid);

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
        this.sessionId = tubeManager.Get("session_id").ToString();
        GD.Print("Session Created with ID: " + this.sessionId);
        EmitSignal(SignalName.SessionCreated, this.sessionId);
        isConnected = true;
        isConnecting = false;
    }
    private void OnSessionJoined()
    {
        this.sessionId = tubeManager.Get("session_id").ToString();
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

    private void OnPeerDisconnected(int peerId)
    {
        GD.Print("Peer Disconnected with ID: " + peerId);
        EmitSignal(SignalName.PeerDisconnected, peerId);
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
    }

    private void OnFailedToJoinSession()
    {
        EmitSignal(SignalName.FailedToJoinSession);
        GD.Print("Failed to join session");
    }

    public async void LeaveSession()
    {
        if (Multiplayer.IsServer())
        {
            Rpc(MethodName.AllPeersLeft);
        }
        tubeManager.Call("leave_session");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void AllPeersLeft()
    {
        LeaveSession();
    }

    public async void OnSessionLeft()
    {
        EmitSignal(SignalName.SessionLeft);
        GD.Print("Session Left");
        isConnected = false;
        isConnecting = false;
    }
    public async void CloseServer()
    {
        isConnected = false;
        isConnecting = false;
        Multiplayer.MultiplayerPeer.Close();
    }

    private void TimeOut()
    {
        if(isConnecting && !isConnected) {
            OnFailedToJoinSession();
            LeaveSession();
            isConnecting = false;
        }
    }

    public bool IsServer()
    {
        return Multiplayer.IsServer();
    }
 
}
