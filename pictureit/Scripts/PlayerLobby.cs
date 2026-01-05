using Godot;
using System;

public partial class PlayerLobby : PanelContainer
{
    private AnimationPlayer animationPlayer;
    [Export] private bool isHost = false;

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(int.Parse(this.Name));
    }

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        if (isHost)
        {
            animationPlayer.Play("Server");
        }
        else
        {
            animationPlayer.Play("Client");
        }
    }
}
