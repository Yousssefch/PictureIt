using Godot;
using System;

public partial class BackButton : TextureButton
{
    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        this.MouseEntered += OnButtonHovered;
        this.MouseExited += OnButtonFocusExited;
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    private void OnButtonHovered()
    {
        animationPlayer.Play("OnHover");
    }

    private void OnButtonFocusExited()
    {
        animationPlayer.PlayBackwards("OnHover");
    }
}
