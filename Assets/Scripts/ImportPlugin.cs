using GLTF.Schema;
using UnityEngine;
using UnityGLTF.Plugins;
using Vector3 = GLTF.Math.Vector3;

public class ImportPlugin : GLTFImportPlugin
{
    public override string DisplayName => "My Import Plugin";
    public override string Description => "";
    
    public override GLTFImportPluginContext CreateInstance(GLTFImportContext context)
    {
        return new MyImportPluginContext();
    }
}

public class MyImportPluginContext: GLTFImportPluginContext
{
    public override void OnBeforeImportScene(GLTFScene scene)
    {
        base.OnBeforeImportScene(scene);
        /*foreach (var node in scene.Nodes)
        {
            node.Value.Scale = new Vector3(1f, 1f, 1f);
        }*/
    }

    public override void OnAfterImportScene(GLTFScene scene, int sceneIndex, GameObject sceneObject)
    {
        // Set all to static
        var objs = sceneObject.GetComponentsInChildren<Transform>();
        foreach (var obj in objs)
            obj.gameObject.isStatic = true;
    }
}