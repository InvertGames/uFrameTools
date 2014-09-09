using System.Collections.Generic;
using System.IO;
using Invert.uFrame.Editor;

public class ViewDataGeneratorFactory : DesignerGeneratorFactory<ViewData>
{
    public override IEnumerable<CodeGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData, ViewData item)
    {
        if (item.ViewForElement == null) yield break;
        yield return new ViewViewBaseGenerator()
        {
            IsDesignerFile = true,
            View = item,
            DiagramData = diagramData,
            Filename = pathStrategy.GetViewsFileName(diagramData.Name),
        };
        yield return CreateEditableGenerator(pathStrategy, diagramData, item);
        yield return CreateDesignerGenerator(pathStrategy, diagramData, item);
    }

    protected virtual ViewGenerator CreateEditableGenerator(ICodePathStrategy pathStrategy, INodeRepository diagramData, ViewData item)
    {
        return new ViewGenerator()
        {
            IsDesignerFile = false,
            DiagramData = diagramData,
            View = item,
            Filename = pathStrategy.GetEditableViewFilename(item),
            RelatedType = item.CurrentViewType
        };
    }

    protected virtual ViewGenerator CreateDesignerGenerator(ICodePathStrategy pathStrategy, INodeRepository diagramData, ViewData item)
    {
        return new ViewGenerator()
        {
            IsDesignerFile = true,
            DiagramData = diagramData,
            View = item,
            Filename = pathStrategy.GetViewsFileName(diagramData.Name),
        };
    }
}