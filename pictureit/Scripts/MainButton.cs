using Godot;
using System;

public partial class MainButton : Button
{
    AnimationPlayer animationPlayer;
    [Export] float YSize = 200f;
    public override void _Ready()
    {
        this.MouseEntered += OnButtonHovered;
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        this.MouseExited += OnButtonFocusExited;
    }
    private void OnButtonHovered()
    {
    }
    private void OnButtonFocusExited()
    {
    }
}
