using Godot;
using System;
using System.Threading.Tasks;

public partial class Picture : PanelContainer
{
    
    //Score calculation variables
    private Vector3 playerPosition;
    private Vector3 playerRotation;
    private float cameraFov;
    private float warmthLevel;
    private float score;
    private Godot.TextureRect pictureTextureRect;
    private AnimationPlayer animationPlayer;
    [Export] public Texture2D image;
    
    public override void _Ready()
    {
        pictureTextureRect = GetNode<Godot.TextureRect>("TextureImage");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        pictureTextureRect.Texture = image;
    }

    public void setTexture(Texture2D texture)
    {
        pictureTextureRect.Texture = texture;
    }

    public void SetMetadata(Vector3 position, Vector3 rotation, float fov, float warmth)
    {
        playerPosition = position;
        playerRotation = rotation;
        cameraFov = fov;
        warmthLevel = warmth;
    }
    public float GetScore()
    {
        return score;
    }

    //Animations
    public async Task AnimationTakePicture()
    {
        animationPlayer.Play("OnTakePhoto");
    }



    


}
