using System.Collections.Generic;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;

public class ViewComponentDataGeneratorFactory : DesignerGeneratorFactory<ViewComponentData>
{
    public override IEnumerable<OutputGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData, ViewComponentData item)
    {
        if (item.View == null) yield break;
        if (item.View.ViewForElement == null) yield break;
        yield return CreateEditableGenerator(pathStrategy, diagramData, item);
        yield return CreateDesignerGenerator(pathStrategy, diagramData, item);
    }

    protected virtual ViewComponentGenerator CreateEditableGenerator(ICodePathStrategy pathStrategy, INodeRepository diagramData, ViewComponentData item)
    {
        return new ViewComponentGenerator()
        {
            IsDesignerFile = false,
            DiagramData = diagramData,
            ViewComponentData = item,
            Filename = pathStrategy.GetEditableFilePath(item),
            RelatedType = item.CurrentType
        };
    }

    protected virtual ViewComponentGenerator CreateDesignerGenerator(ICodePathStrategy pathStrategy, INodeRepository diagramData, ViewComponentData item)
    {
        return new ViewComponentGenerator()
        {
            IsDesignerFile = true,
            DiagramData = diagramData,
            ViewComponentData = item,
            Filename = pathStrategy.GetDesignerFilePath("Views")
          
        };
    }
}