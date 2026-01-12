using Godot;
using System;

public partial class Temp : Node3D
{
    [Export] private Camera3D screenshotCamera;
    [Export] private string savePath = "res://start_screenshot.png";

    public override async void _Ready()
    {
        // Make screenshot camera active
        screenshotCamera.MakeCurrent();

        // Wait for rendering to finish (VERY IMPORTANT)
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        TakeScreenshot();

        // Continue game
        StartGame();
    }

    private void TakeScreenshot()
    {
        Viewport viewport = GetViewport();
        Image image = viewport.GetTexture().GetImage();

        image.FlipY(); // Godot textures are upside-down

        Error err = image.SavePng(savePath);
        if (err == Error.Ok)
            GD.Print("Screenshot saved to ", savePath);
        else
            GD.PrintErr("Failed to save screenshot");
    }

    private void StartGame()
    {
        // Example: switch to gameplay camera
        // GetNode<Camera3D>("../GameCamera").MakeCurrent();

        // Or load next scene, enable controls, etc.
    }
}
