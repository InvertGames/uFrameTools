using System.Collections.Generic;
using System.IO;
using Invert.uFrame.Editor;

public class StateMachineCodeFactory : DesignerGeneratorFactory<StateMachineNodeData>
{
    public override IEnumerable<CodeGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
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