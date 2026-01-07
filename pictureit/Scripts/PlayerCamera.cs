 using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerCamera : Camera3D
{
    int ssCounter = 1;
    CanvasLayer CameraFrame;
    AnimationPlayer frameAnimationPlayer;
    Timer screenshotDelayTimer;
    enum CameraSettingMode
    {
        Zoom,
        Warmth
    }
    [Export] float defaultBobFrequency = 2.0f;
    [Export] float bobAmplitude = 0.05f;
    [Export] float t_bob = 0.0f;
    [Export] float defaultFov = 65.0f;
    [Export] Texture2D zoomIcon;
    [Export] Texture2D warmthIcon;
    [Export] private PackedScene pictureScene;

    CameraSettingMode currentSettingMode = CameraSettingMode.Zoom;
    private int cameraSettingIndex = 0;
    bool isCameraModeActive = false;
    Vector3 cameraBasePosition = new Vector3(0, 1.6f, 0);
    Vector3 initialCameraPosition = new Vector3(0, 1.6f, 0);
    private CanvasLayer blueLayer;
    private ColorRect blueRect;
    private CanvasLayer redLayer;
    private ColorRect redRect;
    private float scrollSensitivity = 2f;
    private CanvasLayer cameraFrame;
    private Godot.TextureRect settingsIcon;
    private AnimationPlayer BlurAnimation;
    private float targetCameraZPositionOffset = 1f;
    private float targetCameraYPositionOffset = 0.2f;
    private float playerSpeedDivisorInCameraMode = 2f;
    private bool isFrameAnimationPlaying = false;
    private bool isTakingScreenshot = false;
    private Godot.Collections.Array<Picture> pictures = new Godot.Collections.Array<Picture>();
    private GameController gameController;

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
        cameraBasePosition = this.Position;
        initialCameraPosition = this.Position;

        cameraFrame = GetNode<CanvasLayer>("../../CameraFrame");
        blueLayer = cameraFrame.GetNode<CanvasLayer>("BlueLayer");
        blueRect = blueLayer.GetNode<ColorRect>("BlueRect");
        redLayer = cameraFrame.GetNode<CanvasLayer>("RedLayer");
        redRect = redLayer.GetNode<ColorRect>("RedRect");
        BlurAnimation = cameraFrame.GetNode<AnimationPlayer>("BlurAnimation");
        settingsIcon = cameraFrame.GetNode<Godot.TextureRect>("Elements/Settings/Icon");
        gameController = GetNode<GameController>("/root/GameController");


    }

    public override void _Process(double delta)
    {
        if(!isCameraModeActive) Transform = new Transform3D(Transform.Basis, cameraBasePosition + _headBob(t_bob, defaultBobFrequency, bobAmplitude)); //Prevents Bobbing in camera mode
        else Transform = new Transform3D(Transform.Basis, cameraBasePosition);

        isCameraModeActive = Input.IsActionPressed("camera_mode") || isTakingScreenshot;
        CameraModeLoop();
    }

    public async void TakeScreenshot()
    {
        GD.Print("Taking Screenshot...");
        isTakingScreenshot = true;
        screenshotDelayTimer.Start(1);
        frameAnimationPlayer.Play("TakePicture");
        await ToSignal(screenshotDelayTimer, Timer.SignalName.Timeout);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        Viewport viewport = GetViewport();
        Image screenshot = viewport.GetTexture().GetImage();
        gameController.CreatePicture(screenshot, this.GlobalPosition, this.RotationDegrees, this.Fov, redRect.Color.A - blueRect.Color.A, 1);

        frameAnimationPlayer.Play("TakePictureEnd");

        isTakingScreenshot = false;

        ssCounter++;
    }

    private void CameraModeLoop()
    {

        Vector3 target = initialCameraPosition;

        if (isCameraModeActive)
        {
            target.Y -= targetCameraYPositionOffset;
            target.Z -= targetCameraZPositionOffset;
            
            if(!isFrameAnimationPlaying)
            {
                frameAnimationPlayer.PlayBackwards("Off");
                isFrameAnimationPlaying = true;
            }

            //Take Screenshot
            if(Input.IsActionJustPressed("take_screenshot"))
            {
                TakeScreenshot();
            }

            if(Input.IsActionJustPressed("switch_camera_setting"))
            {
                currentSettingMode = (CameraSettingMode)(((int)currentSettingMode + 1) % Enum.GetNames(typeof(CameraSettingMode)).Length);
            }
            HandleCameraSetting();
        }
        else
        {
            if(isFrameAnimationPlaying)
            {
                SwitchBackToDefaultCameraSetting();
                frameAnimationPlayer.Play("Off");
                isFrameAnimationPlaying = false;
            }
        }

        cameraBasePosition = cameraBasePosition.Lerp(target, 0.1f);
        
    }

    private void HandleCameraSetting()
    {
        switch (currentSettingMode)
        {
            case CameraSettingMode.Zoom:
                settingsIcon.Texture = zoomIcon;
                if(Input.IsActionJustPressed("WMU"))
                {
                   Fov =  Mathf.Clamp(Fov - scrollSensitivity, 30f, 100f);
                   BlurAnimation.Play("Zoom_Blur");
                }
                else if(Input.IsActionJustPressed("WMD"))
                {
                    Fov =  Mathf.Clamp(Fov + scrollSensitivity, 30f, 100f);
                    BlurAnimation.Play("Zoom_Blur");
                }
                else
                {
                    if(!BlurAnimation.IsPlaying()) BlurAnimation.PlayBackwards("Zoom_Blur_Out");
                }
                break;
            
            case CameraSettingMode.Warmth:
                settingsIcon.Texture = warmthIcon;
                if(Input.IsActionJustPressed("WMU"))
                {
                    if(blueRect.Color.A == 0f)
                    {
                        Color targetColor = redRect.Color;
                        targetColor.A = Mathf.Clamp(targetColor.A + 0.01f, 0f, 0.5f);
                        redRect.Color = targetColor;
                        return;
                    }
                    else
                    {
                        Color targetColor = blueRect.Color;
                        targetColor.A = Mathf.Clamp(targetColor.A - 0.01f, 0f, 0.5f);
                        blueRect.Color = targetColor;
                        return;
                    }
                }
                else if(Input.IsActionJustPressed("WMD"))
                {
                    if(redRect.Color.A == 0f)
                    {
                        Color targetColor = blueRect.Color;
                        targetColor.A = Mathf.Clamp(targetColor.A + 0.01f, 0f, 0.5f);
                        blueRect.Color = targetColor;
                        return;
                    }
                    else
                    {
                        Color targetColor = redRect.Color;
                        targetColor.A = Mathf.Clamp(targetColor.A - 0.01f, 0f, 0.5f);
                        redRect.Color = targetColor;
                        return;
                    }
                }
                break;
        }
    }
    private void SwitchBackToDefaultCameraSetting()
    {
        Fov = defaultFov;
        redRect.Color = new Color(redRect.Color.R, redRect.Color.G, redRect.Color.B, 0f);
        blueRect.Color = new Color(blueRect.Color.R, blueRect.Color.G, blueRect.Color.B, 0f);
        this.Position = initialCameraPosition;
        if(BlurAnimation.IsPlaying()) BlurAnimation.PlayBackwards("Zoom_Blur_Out");
    }

     private Vector3 _headBob(float t, float frequency, float amplitude)
    {
        Vector3 pos = Vector3.Zero;
        pos.Y += Mathf.Sin(t * frequency) * amplitude;
        return pos;
    }

}
