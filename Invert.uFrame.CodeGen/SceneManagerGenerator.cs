using System.CodeDom;
using Invert.uFrame.Editor;

public class SceneManagerGenerator : SceneManagerClassGenerator
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        Namespace.Imports.Add(new CodeNamespaceImport("UniRx"));
        AddSceneManager(Data);
    }
}