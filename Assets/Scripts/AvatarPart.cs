using UnityEngine;

public class AvatarPart
{
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public AvatarAsset asset;

    public AvatarPart(AvatarAsset asset, SkinnedMeshRenderer smr)
    {
        this.asset = asset;
        skinnedMeshRenderer = smr;
    }
}
