using Invert.uFrame.Editor;
using UnityEngine;

public class SceneManagerSettingsGenerator : SceneManagerClassGenerator
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddSceneManagerSettings(Data,null);
    }
}