using Invert.uFrame.Editor;

public class SceneManagerGenerator : SceneManagerClassGenerator
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddSceneManager(Data);
        if (IsDesignerFile)
            AddTypeEnum(Data.Name + "Registry", Data.Instances);
    }
}