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
    private Godot.Collections.Dictionary<string, Vector3> metadata = new Godot.Collections.Dictionary<string, Vector3>();
    private Godot.Collections.Dictionary<string, Vector3> referenceMetadata = new Godot.Collections.Dictionary<string, Vector3>();
    
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
        metadata = new Godot.Collections.Dictionary<string, Vector3>
        {
           {"position", position   },
           {"rotation", rotation},
           {"fov", new Vector3(fov, 0, 0)},
           {"warmth", new Vector3(warmth, 0, 0)}
        };
    }

    public Godot.Collections.Dictionary<string, Vector3> GetMetadata()
    {
        return metadata;
    }

    public void SetReferenceMetadata(Vector3 position, Vector3 rotation, float fov, float warmth)
    {
        referenceMetadata = new Godot.Collections.Dictionary<string, Vector3>
        {
            {"position", position   },
            {"rotation", rotation},
            {"fov", new Vector3(fov, 0, 0)},
            {"warmth", new Vector3(warmth, 0, 0)}
        };
    }
    public void CalculateScoreBasedOnMetadata()
    {

        float positionScore = Mathf.Max(0, 100 - playerPosition.DistanceTo(referenceMetadata["position"]) * 10);
        float rotationScore = Mathf.Max(0, 100 - playerRotation.DistanceTo(referenceMetadata["rotation"]) * 5);
        float fovScore = Mathf.Max(0, 100 - Mathf.Abs(cameraFov - referenceMetadata["fov"].X) * 2);
        float warmthScore = Mathf.Max(0, 100 - Mathf.Abs(warmthLevel - referenceMetadata["warmth"].X) * 20);

        score = (positionScore + rotationScore + fovScore + warmthScore) / 4;
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

    public async Task AnimationEvaluate()
    {
        animationPlayer.Play("OnEvaluate");
        await ToSignal(animationPlayer, "animation_finished");
    }

    public async Task ShowReferencePicture()
    {
        animationPlayer.Play("ShowReferencePicture");
        await ToSignal(animationPlayer, "animation_finished");
    }

    public async Task HideReferencePicture()
    {
        animationPlayer.PlayBackwards("ShowReferencePicture");
        await ToSignal(animationPlayer, "animation_finished");
    }



    


}
