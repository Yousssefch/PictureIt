 using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerCamera : Camera3D
{
    int ssCounter = 1;
    CanvasLayer CameraFrame;
    AnimationPlayer frameAnimationPlayer;
    Timer screenshotDelayTimer;

    public override void _Ready()
    {
        DirAccess  dir = DirAccess.Open("res://Screenshots");
        foreach (string file in dir.GetFiles())
        {
            dir.Remove(file);
        }
        CameraFrame = GetNode<CanvasLayer>("../../CameraFrame");
        frameAnimationPlayer = CameraFrame.GetNode<AnimationPlayer>("AnimationPlayer");
        screenshotDelayTimer = GetNode<Timer>("../../ScreenshotDelayTimer");
    }

    public async Task TakeScreenshot()
    {
        GD.Print("Taking Screenshot...");
        screenshotDelayTimer.Start(1);
        frameAnimationPlayer.Play("TakePicture");
        await ToSignal(screenshotDelayTimer, Timer.SignalName.Timeout);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Viewport viewport = GetViewport();
        Image screenshot = viewport.GetTexture().GetImage();
        screenshot.SavePng("res://Screenshots/ss_" + ssCounter + ".png");
        frameAnimationPlayer.Play("TakePictureEnd");

        ssCounter++;
    }
}
