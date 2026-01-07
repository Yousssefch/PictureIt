using Godot;
using System;

public partial class PlayerLobby : PanelContainer
{
    [Export] private bool isHost = false;

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(int.Parse(this.Name));
    }

    public override void _Ready()
    {
    }
}
