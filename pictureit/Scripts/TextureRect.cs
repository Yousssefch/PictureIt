using Godot;
using System;

public partial class TextureRect : Godot.TextureRect
{
    [Export] private SpriteFrames animatedSprite;
    private string currentAnimation = "default";
    [Export] private bool autoPlay = false;
    [Export]  private bool isPlaying = false;
    int frame = 0;
    double refreshRate = 0.1;
    double fps = 30.0;
    float frame_delta = 0.0f;

    public override void _Ready()
    {
        getAnimationData();
    }

    override public void _Process(double delta)
    {
        if(!animatedSprite.HasAnimation(currentAnimation) || !isPlaying)
            return;
        getAnimationData();
        frame_delta += (float)delta;
        if(frame_delta >= refreshRate)
        {
            Texture2D newTexture = getFrameTexture(frame);
            Texture = newTexture;
            frame_delta = 0.0f;
        }
    }
    

    private void getAnimationData()
    {
        fps = animatedSprite.GetAnimationSpeed(currentAnimation);
        refreshRate =  animatedSprite.GetFrameDuration(currentAnimation,frame);
    }

    private Texture2D getFrameTexture(int frameIndex)
    {
        frame++;
        var frameCount = animatedSprite.GetFrameCount(currentAnimation);
        if(frame >= frameCount)
            frame = 0;
            if(!animatedSprite.GetAnimationLoop(currentAnimation) && frame == 0)
                isPlaying = false;
        getAnimationData();
        return animatedSprite.GetFrameTexture(currentAnimation, frameIndex);
    }

    private void Play(string animationName)
    {
        currentAnimation = animationName;
        frame = 0;
        getAnimationData();
        isPlaying = true;
    }

    private void Pause()
    {
        isPlaying = false;
    }

    private void Resume()
    {
        isPlaying = true;
    }

    private void Stop()
    {
        isPlaying = false;
        frame = 0;
    } 
    
}
