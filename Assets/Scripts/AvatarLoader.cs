using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Loader;

public class AvatarLoader : MonoBehaviour
{
    [SerializeField] private Transform rootBone;
    [SerializeField] private Material avatarMaterial;
    [SerializeField] private Material skinMaterial;
    [SerializeField] private Material mouthMaterial;
    [SerializeField] private Material eyesMaterial;
    [SerializeField] private Animator animator;
    [SerializeField] private SkinnedMeshRenderer head;

    public TextAsset jsonFile;
    public UserAvatarAsset userAvatar;

    private const string GG_BASE_URL = "https://www.geoguessr.com/assets/";
    private const string PROXY_BASE_URL = "https://solin-ochre.vercel.app/api/";
    private NetworkManager _api = new();
    
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    private List<AvatarPart> _parts = new();

    private string GetAvatarUrl(string asset)
    {
        return $"{PROXY_BASE_URL}proxy?url={GG_BASE_URL}{asset}";
    }

    private string ImageProxy(string assetUrl)
    {
        var fetchUrl = $"{PROXY_BASE_URL}image?url={GG_BASE_URL}{assetUrl}";
        return $"{PROXY_BASE_URL}proxy?url={fetchUrl}";
    }

    private void Start()
    {
        userAvatar = JsonUtility.FromJson<UserAvatarAsset>(jsonFile.text);
        /*Load(userAvatar);*/
        AsyncLoad();
    }

    private async void AsyncLoad()
    {
        //Carl K - 62973f68ec260053a2ffde4e
        //Dogge - 588524808b9ff40d2423007d
        //Filip - 5f2c6f6e0ee6d80001055476
        //Self - 61af416a5e5a4300015aa9b5
        // Hamlet - 61af684778ee090001b810fd
        // kribba - 63175bf6c2133975022c5b3d
        // Oskar - 6047badd0be6cc0001fd185b
        // Jester - 54d74a21ab462aac88f37820
        // Daniel N - 636ea0ea3d17a8baabf20ccd
        // Anton - 547cf50aa737a7ea70b73939
        // Christina - 6362d08c9a3562881d71a37c
        
        userAvatar = await _api.GetUserAvatarAsset("588524808b9ff40d2423007d");
        Load(userAvatar);
    }

    private async void Load(UserAvatarAsset userAvatarAsset)
    {
        // Only body-related slots
        var filtered = userAvatarAsset.equipped.FindAll(aa => aa.slot <= 10);
        var skin = userAvatarAsset.equipped.Find(asset => asset.slot == (int)AssetSlot.Skin);
        var eyes = userAvatarAsset.equipped.Find(asset => asset.slot == (int)AssetSlot.Eyes);
        var mouth = userAvatarAsset.equipped.Find(asset => asset.slot == (int)AssetSlot.Mouth);
        
        var skinTex = await _api.GetTexture(ImageProxy(skin.texture));
        var eyesTex = await _api.GetTexture(ImageProxy(eyes.texture));
        var mouthTex = await _api.GetTexture(ImageProxy(mouth.texture));
        
        LoadHead(skinTex, eyesTex, mouthTex);
        foreach (var assetSlot in filtered)
        {
            await LoadAvatarAsset(assetSlot, skinTex);
        }
        
        HandleHide();
        
        animator.enabled = true;

        var hairPart = _parts.Find(p => p.asset.slot == (int)AssetSlot.Hair);
        var hatPart = _parts.Find(p => p.asset.slot == (int)AssetSlot.Hats);

        // Handle hair / hat morphTargets
        if (hairPart == null || hatPart == null) return;
        if (string.IsNullOrEmpty(hatPart.asset.morphTarget)) return;
        var blendShapeIndex = hairPart.skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(hatPart.asset.morphTarget);
        if (blendShapeIndex < 0) return;
        hairPart.skinnedMeshRenderer.SetBlendShapeWeight(0, 1);
    }

    private void HandleHide()
    {
        foreach (var part in _parts)
        {
            if (part.asset.hides.Length <= 0) continue;
            foreach (var hide in part.asset.hides)
            {
                var slot = int.Parse(hide);
                var partToHide = _parts.Find(p => p.asset.slot == slot);
                partToHide.skinnedMeshRenderer.gameObject.SetActive(false);
            }
        }
    }

    private void LoadHead(Texture skin, Texture eyes, Texture mouth)
    {
        AttachMeshToHumanoidAvatar(head, rootBone);

        var mats = head.materials;

        mats[0] = skinMaterial;
        mats[0].SetTexture(BaseMap, skin);
        mats[1] = mouthMaterial;
        mats[1].SetTexture(BaseMap, mouth);
        mats[2] = eyesMaterial;
        mats[2].SetTexture(BaseMap, eyes);

        head.materials = mats;
    }

    private async Task LoadAvatarAsset(AvatarAsset asset, Texture skinTexture)
    {
        if (string.IsNullOrEmpty(asset.meshGlb)) return;
        
        var importOpt = new ImportOptions
        {
            DataLoader = new UnityWebRequestLoader(GetAvatarUrl(asset.meshGlb)),
            AnimationMethod = AnimationMethod.None, // Humanoid for Mixamo animation
            ImportBlendShapeNames = true,
        };

        var import = new GLTFSceneImporter("", importOpt);
        import.CustomShaderName = "Shader Graphs/AvatarShader";
        await import.LoadSceneAsync();
        
        import.LastLoadedScene.gameObject.isStatic = false;

        var smrs = import.LastLoadedScene.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var smr in smrs)
        {
            AttachMeshToHumanoidAvatar(smr, rootBone);
            _parts.Add(new AvatarPart(asset, smr));

            var mats = smr.materials;
            for(var i = 0; i < smr.materials.Length; i++)
            {
                if (mats[i].name.ToLower().Contains("skin"))
                {
                    mats[i] = skinMaterial;
                    mats[i].SetTexture(BaseMap, skinTexture);
                    continue;
                }

                if (string.IsNullOrEmpty(asset.texture)) continue;
                var tex = await _api.GetTexture(ImageProxy(asset.texture));
                mats[i] = new Material(avatarMaterial);
                mats[i].SetTexture(BaseMap, tex);
            }
            
            smr.materials = mats;
        }
    }
    
    private void AttachMeshToHumanoidAvatar(SkinnedMeshRenderer smr, Transform root)
    {
        if (smr == null || root == null) return;

        var meshTransform = smr.transform;
        
        // --- 1. Map Bones & Setup ---
        
        meshTransform.SetParent(root, false);
        meshTransform.localScale = Vector3.one;

        // Map bones by name to existing avatar
        var boneMap = root.GetComponentsInChildren<Transform>();
        var newBones = new Transform[smr.bones.Length];

        for (var i = 0; i < smr.bones.Length; i++)
        {
            var boneName = smr.bones[i].name;
            var newBone = Array.Find(boneMap, b => b.name == boneName);
            
            if (newBone != null)
            {
                newBones[i] = newBone;
            }
            else
            {
                Debug.LogWarning($"Bone not found in avatar: {boneName}");
            }
        }

        // --- 2. Recalculate Bindposes ---

        // The mesh's current world matrix:
        var meshWorldMatrix = meshTransform.localToWorldMatrix;
        
        // Create an array for the new bindposes
        var newBindPoses = new Matrix4x4[newBones.Length];
        
        for (var i = 0; i < newBones.Length; i++)
        {
            var bone = newBones[i];
            
            // If the bone wasn't found, use the root transform's binding pose
            // or just the identity matrix as a last resort.
            if (bone == null)
            {
                // This is a common pattern: if a bone is missing, its bindpose 
                // is calculated against the rootBone/mesh root.
                // For simplicity, we can use the root transform.
                bone = root; 
            }

            // The new bindpose is: (Inverse World Matrix of the Bone) * (World Matrix of the Mesh)
            // This transforms vertices from mesh local space to bone local space when calculating skinning.
            newBindPoses[i] = bone.worldToLocalMatrix * meshWorldMatrix;
        }

        // --- 3. Apply Changes ---

        // Apply the new bones and bindposes
        smr.bones = newBones;
        smr.sharedMesh.bindposes = newBindPoses; // ✅ THIS IS THE KEY FIX

        // Set the root bone
        smr.rootBone = root;

        // Optionally, check if you need to call smr.sharedMesh.RecalculateBounds();
        smr.sharedMesh.RecalculateBounds();
    }
}
