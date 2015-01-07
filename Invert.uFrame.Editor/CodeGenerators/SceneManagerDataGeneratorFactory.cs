using System.Collections.Generic;
using System.IO;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;

public class SceneManagerDataGeneratorFactory : DesignerGeneratorFactory<SceneManagerData>
{
    public override IEnumerable<OutputGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData, SceneManagerData item)
    {
        if (item.SubSystem == null) yield break;
        yield return new SceneManagerGenerator()
        {
            Filename = pathStrategy.GetDesignerFilePath("SceneManagers"),
            Data = item,
            DiagramData = diagramData,
            IsDesignerFile = true,
        };
        yield return new SceneManagerGenerator()
        {
            Filename = pathStrategy.GetEditableFilePath(item),
            Data = item,
            DiagramData = diagramData,
            IsDesignerFile = false,
        };
        yield return new SceneManagerSettingsGenerator()
        {
            IsDesignerFile = false,
            Data = item,
            DiagramData = diagramData,
            RelatedType = item.CurrentSettingsType,
            Filename = pathStrategy.GetEditableFilePath(item,"Settings")
        };
    }
}