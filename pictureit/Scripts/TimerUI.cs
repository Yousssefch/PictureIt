using Godot;
using System;

public partial class TimerUI : PanelContainer
{
    private Label _timerLabel;
    private TextureProgressBar timerCircle;
    [Export] private float _levelDuration = 60f; // Duration of the level in seconds

    public override void _Ready()
    {
        _timerLabel = GetNode<Label>("HBoxContainer/Label");
        timerCircle = GetNode<TextureProgressBar>("HBoxContainer/TextureProgressBar");
        timerCircle.MaxValue = _levelDuration;
    }

    public void UpdateTimer(float timeLeft)
    {
        _timerLabel.Text = timeLeft.ToString();
        timerCircle.Value = _levelDuration - timeLeft;
    }

    public void SetLevelDuration(float duration)
    {
        _levelDuration = duration;
        timerCircle.MaxValue = _levelDuration;
        _timerLabel.Text = _levelDuration.ToString();
    }
}
