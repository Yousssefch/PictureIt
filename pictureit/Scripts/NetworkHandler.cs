using Godot;
using System;

public partial class NetworkHandler : Node
{
    private string IP_ADDRESS = "localhost";
    private int PORT = 3000;
    private ENetMultiplayerPeer peer;
    public void CreateServer()
    {

        peer = new ENetMultiplayerPeer();
        peer.CreateServer(PORT, 32);
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Server created on port " + PORT);
    }

    public void CreateClient()
    {
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(IP_ADDRESS, PORT);
        if (error != Error.Ok)
        {
            GD.PrintErr("Failed to create client: " + error);
            return;
        }
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Client connected to " + IP_ADDRESS + ":" + PORT); 
    }
 
}
