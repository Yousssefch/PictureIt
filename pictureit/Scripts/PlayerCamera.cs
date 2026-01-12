 using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerCamera : Camera3D
{
    Player player;
    int ssCounter = 1;
    CanvasLayer CameraFrame;
    AnimationPlayer frameAnimationPlayer;
    Timer screenshotDelayTimer;
    enum CameraSettingMode
    {
        Zoom,
        Warmth,
        Blur
    }
    [Export] float defaultBobFrequency = 2.0f;
    [Export] float bobAmplitude = 0.05f;
    [Export] float t_bob = 0.0f;
    [Export] float defaultFov = 65.0f;
    [Export] Texture2D zoomIcon;
    [Export] Texture2D warmthIcon;
    [Export] Texture2D blurIcon;
    [Export] private PackedScene pictureScene;
    [Export] private bool isDebugSaveReferences = false;
    [Export] private int maxPictures = 3;
    private int pictureCounter = 0;

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
    private CanvasLayer blur;
    private ColorRect BlurRect;
    private float blurLOD;
    private float targetCameraZPositionOffset = 1f;
    private float targetCameraYPositionOffset = 0.2f;
    private float playerSpeedDivisorInCameraMode = 2f;
    private bool isFrameAnimationPlaying = false;
    private bool isTakingScreenshot = false;
    private Godot.Collections.Array<Picture> pictures = new Godot.Collections.Array<Picture>();
    private GameController gameController;
    private LevelController levelController;

    //Blur Effect
    [Export] float blurFarMax= 100f;
    [Export] float blurFarMin = 3f;
    [Export] float blurNearMax = 10f;
    [Export] float blurNearMin = 0.5f;
    [Export] float blurRange = -2f;
    float blurLerpSpeed = 5f;
    RayCast3D blurRangeRay;


    public override void _EnterTree()
    {
        Player player = GetParent().GetParent<Player>();
        SetMultiplayerAuthority(int.Parse(player.Name));
    }

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
        player = GetParent().GetParent<Player>();

        cameraFrame = GetNode<CanvasLayer>("../../CameraFrame");
        blur = cameraFrame.GetNode<CanvasLayer>("Blur");
        BlurRect = blur.GetNode<ColorRect>("BlurRect");
        blueLayer = cameraFrame.GetNode<CanvasLayer>("BlueLayer");
        blueRect = blueLayer.GetNode<ColorRect>("BlueRect");
        redLayer = cameraFrame.GetNode<CanvasLayer>("RedLayer");
        redRect = redLayer.GetNode<ColorRect>("RedRect");
        BlurAnimation = cameraFrame.GetNode<AnimationPlayer>("BlurAnimation");
        settingsIcon = cameraFrame.GetNode<Godot.TextureRect>("Elements/Settings/Icon");
        gameController = GetNode<GameController>("/root/GameController");

        blurRangeRay = GetNode<RayCast3D>("BlurRange");

        levelController = player.GetParent().GetParent<LevelController>();


    }

    public override void _Process(double delta)
    {
        if(!IsMultiplayerAuthority()) return;
        
        float standingBobFrequency = defaultBobFrequency/2f;
        if(!isCameraModeActive) Transform = new Transform3D(Transform.Basis, cameraBasePosition + _headBob(t_bob, player.Velocity.IsZeroApprox() ? standingBobFrequency : defaultBobFrequency, bobAmplitude)); //Prevents Bobbing in camera mode
        else Transform = new Transform3D(Transform.Basis, cameraBasePosition);
        t_bob += (float)delta * defaultBobFrequency;

        isCameraModeActive = Input.IsActionPressed("camera_mode") || isTakingScreenshot;
        CameraModeLoop();

        //Blur Effect Logic
        blurRangeRay.TargetPosition = new Vector3(0, 0, blurRange);
    }

    public async void TakeScreenshot()
    {
        GD.Print("Taking Screenshot...");
        pictureCounter++;
        isTakingScreenshot = true;
        screenshotDelayTimer.Start(1);
        frameAnimationPlayer.Play("TakePicture", customSpeed: 1.5f);
        await ToSignal(screenshotDelayTimer, Timer.SignalName.Timeout);
        levelController.HideHUD();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        Viewport viewport = GetViewport();
        Image screenshot = viewport.GetTexture().GetImage();
        levelController.ShowHUD();

        screenshot.Resize(512, 512, Image.Interpolation.Nearest); //for small bytes
        byte[] imageData = screenshot.SavePngToBuffer();
        Vector3 position = this.GlobalPosition;
        Vector3 rotation = this.GlobalRotation;
        float fov = this.Fov;
        float warmth = redRect.Color.A - blueRect.Color.A;
        int player_id = GetMultiplayerAuthority();

        levelController.UpdateObjectives();

        Rpc(MethodName.ReceivePicture, imageData, position, rotation, fov, warmth, player_id, isDebugSaveReferences ? true : false);


        frameAnimationPlayer.Play("TakePictureEnd");

        isTakingScreenshot = false;

        ssCounter++;
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ReceivePicture(byte[] imageData, Vector3 position, Vector3 rotation, float fov, float warmth, int player_id, bool saveToReferences = false)
    {
        Image img = new Image();
        img.LoadPngFromBuffer(imageData);
        gameController.CreatePicture(img, position, rotation, fov, warmth, player_id, saveToReferences);
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
            if(Input.IsActionJustPressed("take_screenshot") && !isTakingScreenshot && pictureCounter < maxPictures)
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
                float minimumFovChange = 30f;
                float maximumFovChange = 150f;
                if(Input.IsActionJustPressed("WMD"))
                {
                   Fov =  Mathf.Clamp(Fov - scrollSensitivity, minimumFovChange, maximumFovChange);
                   BlurAnimation.Play("Zoom_Blur");
                }
                else if(Input.IsActionJustPressed("WMU"))
                {
                    Fov =  Mathf.Clamp(Fov + scrollSensitivity, minimumFovChange, maximumFovChange);
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
            
            case CameraSettingMode.Blur:
                settingsIcon.Texture = blurIcon;
                float currentLOD = GetBlurLOD();
                float maxBlurLOD = 5f;
                float minBlurLOD = 0f;
                if(Input.IsActionJustPressed("WMU"))
                {
                    float targetLOD = Mathf.Clamp(currentLOD + 0.1f, minBlurLOD, maxBlurLOD);
                    ChangeBlurLOD(targetLOD);
                }
                else if(Input.IsActionJustPressed("WMD"))
                {
                    float targetLOD = Mathf.Clamp(currentLOD - 0.1f, minBlurLOD, maxBlurLOD);
                    ChangeBlurLOD(targetLOD);
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
        ChangeBlurLOD(0f);
        if(BlurAnimation.IsPlaying()) BlurAnimation.PlayBackwards("Zoom_Blur_Out");
    }

     private Vector3 _headBob(float t, float frequency, float amplitude)
    {
        Vector3 pos = Vector3.Zero;
        pos.Y += Mathf.Sin(t * frequency) * amplitude;
        return pos;
    }

    private void ChangeBlurLOD(float LOD)
    {
        Material blurMaterial = BlurRect.Material;
        if(blurMaterial is ShaderMaterial shaderMaterial)
        {
            GD.Print("Changing Blur LOD to: " + LOD);
            shaderMaterial.SetShaderParameter("lod", LOD);
        }
    }

    private float GetBlurLOD()
    {
        float blurLOD = 0f;
        Material blurMaterial = BlurRect.Material;
        if(blurMaterial is ShaderMaterial shaderMaterial)
        {
            blurLOD = (float)shaderMaterial.GetShaderParameter("lod");
            GD.Print("Current Blur LOD is: " + blurLOD);
        }
        return blurLOD;
    }

}
