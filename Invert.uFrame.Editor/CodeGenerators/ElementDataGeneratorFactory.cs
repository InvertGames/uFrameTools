using System.Collections.Generic;
using System.IO;
using Invert.Core.GraphDesigner;
using Invert.uFrame;
using Invert.uFrame.Editor;

public class ElementDataGeneratorFactory : DesignerGeneratorFactory<ElementData>
{
    public override IEnumerable<CodeGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy codePathStrategy, INodeRepository diagramData, ElementData item)
    {

        yield return CreateDesignerControllerGenerator(codePathStrategy, diagramData, item);
        yield return CreateEditableControllerGenerator(codePathStrategy, diagramData, item);


        yield return CreateDesignerViewModelGenerator(codePathStrategy, diagramData, item);
        yield return CreateEditableViewModelGenerator(codePathStrategy, diagramData, item);

        yield return CreateViewBaseGenerator(codePathStrategy, diagramData, item);
    }

    public virtual CodeGenerator CreateDesignerControllerGenerator(ICodePathStrategy codePathStrategy, INodeRepository diagramData, ElementData item)
    {

        return new ControllerGenerator()
        {
            ElementData = item,
            DiagramData = diagramData,
            Filename = codePathStrategy.GetDesignerFilePath("Controllers"),
            IsDesignerFile = true
        };
    }

    public virtual CodeGenerator CreateEditableControllerGenerator(ICodePathStrategy codePathStrategy, INodeRepository diagramData, ElementData item)
    {
        return new ControllerGenerator()
        {
            ElementData = item,
            RelatedType = item.ControllerType,
            DiagramData = diagramData,
            Filename = codePathStrategy.GetEditableFilePath(item, "Controller"),
            IsDesignerFile = false
        };
    }

    public virtual CodeGenerator CreateDesignerViewModelGenerator(ICodePathStrategy codePathStrategy, INodeRepository diagramData, ElementData item)
    {
        return new ViewModelGenerator(true, item)
        {
            ElementData = item,
            IsDesignerFile = true,
            DiagramData = diagramData,
            Filename = codePathStrategy.GetDesignerFilePath(string.Empty)
        };
    }

    public virtual CodeGenerator CreateEditableViewModelGenerator(ICodePathStrategy codePathStrategy, INodeRepository diagramData, ElementData item)
    {
        return new ViewModelGenerator(false, item)
        {
            IsDesignerFile = false,
            ElementData = item,
            RelatedType = item.CurrentViewModelType,
            DiagramData = diagramData,
            Filename = codePathStrategy.GetEditableFilePath(item)
        };
    }

    public virtual CodeGenerator CreateViewBaseGenerator(ICodePathStrategy codePathStrategy, INodeRepository diagramData, ElementData item)
    {
        return new ViewBaseGenerator()
        {
            ElementData = item,
            DiagramData = diagramData,
            IsDesignerFile = true,

            Filename = codePathStrategy.GetDesignerFilePath("Views")
        };
    }
}

