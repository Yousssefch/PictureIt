using Godot;
using System;
using System.Threading.Tasks;

public partial class Objectives : PanelContainer
{
    Node referencePicture;
    VBoxContainer container;
    private Node referencePictureContainer;
    [Export(PropertyHint.ResourceType, "PackedScene")]
    public Godot.Collections.Array<PackedScene> referencePictures
    = new Godot.Collections.Array<PackedScene>();
    [Export] private int picturesToTake = 3;
    int currentPictureIndex = 0;

    private Godot.Collections.Array<Picture> selectedReferencePictures;
    public override async void _Ready()
    {
        container = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
        referencePictureContainer = container.GetNode("ReferencePictureContainer");
        selectedReferencePictures = GetReferencePictures();
        await UpdateReferencePicture();
    }

    private Godot.Collections.Array<Picture> GetReferencePictures()
    {
        var shuffled = new Godot.Collections.Array<PackedScene>(referencePictures);
        Random random = new Random();

        //shuffle the pictures
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        var selected = new Godot.Collections.Array<Picture>();
        for (int i = 0; i < picturesToTake && i < shuffled.Count; i++)
        {
            selected.Add(shuffled[i].Instantiate<Picture>());
        }

        return selected;
    }

    public async Task UpdateReferencePicture()
    {
        await ClearReferencePicture();
        if (currentPictureIndex < selectedReferencePictures.Count)
        {
            Picture pic = selectedReferencePictures[currentPictureIndex];
            Picture picInstance = selectedReferencePictures[currentPictureIndex].Duplicate() as Picture;
            referencePictureContainer.AddChild(picInstance);
            currentPictureIndex++;
            await picInstance.ShowReferencePicture();
        }
        else
        {
            GD.Print("All reference pictures have been shown.");
        }
    }

    public Godot.Collections.Dictionary<string, Vector3> GetCurrentReferencePictureMetaData()
    {
        GD.Print("Getting metadata for picture index: ", currentPictureIndex - 1 );
        Picture pic = selectedReferencePictures[currentPictureIndex - 1];
        return pic.GetMetadata();
    }
    private async Task ClearReferencePicture()
    {
        foreach(Node child in referencePictureContainer.GetChildren())
        {
            if(child is Picture pic)
                await pic.HideReferencePicture();
            child.QueueFree();
        }
    }


}
