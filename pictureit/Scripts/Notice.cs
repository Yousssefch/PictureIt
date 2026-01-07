using Godot;
using System;

public partial class Notice : CanvasLayer
{
    private Label messageLabel;
    [Export] private string currentMessage = "";
    AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        messageLabel = GetNode<Label>("PanelContainer/MessageLabel");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        messageLabel.Text = currentMessage;
    }

    private async void OnCloseButtonPressed()
    {
        animationPlayer.Play("OnClose");
        await ToSignal(animationPlayer, "animation_finished");
        this.QueueFree();
    }
}
