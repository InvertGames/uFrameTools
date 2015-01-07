using System.Collections.Generic;
using System.IO;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;

public class StateMachineCodeFactory : DesignerGeneratorFactory<StateMachineNodeData>
{
    public override IEnumerable<OutputGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
        StateMachineNodeData item)
    {
        yield return new StateMachineClassGenerator()
        {
            Data = item as StateMachineNodeData,
            Repository = diagramData,
            StateMachineType = uFrameEditor.UFrameTypes.StateMachine,
            IsDesignerFile = true,
            ObjectData = item,
            Filename = pathStrategy.GetDesignerFilePath("StateMachines")
        };

        yield return new StateMachineClassGenerator()
        {
            Data = item as StateMachineNodeData,
            Repository = diagramData,
            StateMachineType = uFrameEditor.UFrameTypes.StateMachine,
            IsDesignerFile = false,
            ObjectData = item,
            Filename = Path.Combine("Machines", item.Name + ".cs")
        };
    }

}


public class CustomizedGeneratorPlugin : DiagramPlugin
{
    public override decimal LoadPriority
    {
        get { return 2; } // Make sure it loads last
    }

    public override void Initialize(uFrameContainer container)
    {
        container.Register<DesignerGeneratorFactory, StateMachineCodeFactory>("StateMachineCodeFactory");
    }
}

public class CustomStateMachineClassGenerator : StateMachineClassGenerator
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        // EDIT THIS PART
        TryAddNamespace("INSERT_NAMESPACE_HERE");
        // --------------------------------------
    }
}
public class CustomStateMachineCodeFactory : StateMachineCodeFactory
{
    public override IEnumerable<OutputGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
        StateMachineNodeData item)
    {
        yield return new CustomStateMachineClassGenerator()
        {
            Data = item as StateMachineNodeData,
            Repository = diagramData,
            StateMachineType = uFrameEditor.UFrameTypes.StateMachine,
            IsDesignerFile = true,
            ObjectData = item,
            Filename = pathStrategy.GetDesignerFilePath("StateMachines")
        };

        yield return new CustomStateMachineClassGenerator()
        {
            Data = item as StateMachineNodeData,
            Repository = diagramData,
            StateMachineType = uFrameEditor.UFrameTypes.StateMachine,
            IsDesignerFile = false,
            ObjectData = item,
            Filename = Path.Combine("Machines", item.Name + ".cs")
        };
    }
}
