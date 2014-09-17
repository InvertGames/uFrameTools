using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.uFrame.Editor;

public class StateMachineStateCodeFactory : DesignerGeneratorFactory<StateMachineStateData>
{
    public override IEnumerable<CodeGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
        StateMachineStateData item)
    {
        yield return new StateMachineStateClassGenerator()
        {
            Data = item,
            StateMachineType = uFrameEditor.UFrameTypes.StateMachine,
            StateType = uFrameEditor.UFrameTypes.State,
            IsDesignerFile = true,
            ObjectData = item,
            Filename = diagramData.Name + "StateMachines.designer.cs"
        };
        yield return new StateMachineStateClassGenerator()
        {
            Data = item,
            StateMachineType = uFrameEditor.UFrameTypes.StateMachine,
            StateType = uFrameEditor.UFrameTypes.State,
            IsDesignerFile = false,
            ObjectData = item,
            Filename = Path.Combine("States", item.Name + "State.cs")
        };
    }

}
public class StateMachineViewModelProcessor : TypeGeneratorPostProcessor<ViewModelGenerator>
{
    public override void Apply()
    {
        var constructor = this.CodeGenerator.WireCommandsMethod;
        if (constructor != null)
        {
            var element = CodeGenerator.ElementData;
            var stateMachines = CodeGenerator.ElementData.GetContainingNodes(CodeGenerator.DiagramData).OfType<StateMachineNodeData>().ToArray();
            var properties = CodeGenerator.ElementData.SubscribableProperties.ToArray();
            // var transitions = stateMachines.SelectMany(p => p.Transitions).ToArray();

            foreach (var stateMachine in stateMachines)
            {
                var stateMachineProperty =
                    element.Properties.FirstOrDefault(p => p.RelatedType == stateMachine.Identifier);

                if (stateMachineProperty == null) continue;

                foreach (var transition in stateMachine.Transitions)
                {
                    var transitionProperties =
                        properties.Where(p => transition[p.Identifier]).ToArray();

                    foreach (var transitionProperty in transitionProperties)
                    {
                        if (transition.TransitionTo == null) continue;
                        constructor.Statements.Add(new CodeSnippetExpression(string.Format("{0}.{1}.AddTrigger({2},{0}.{1}.{3})",
                            stateMachineProperty.FieldName, transition.StateMachineState.Name, transitionProperty.FieldName, transition.Name)));

                        //constructor.Statements.Add(new CodeSnippetExpression(
                        //    string.Format("{0}.Subscribe((v)=>{{ if (v) {1}.Transition(\"{2}\"); }})", transitionProperty.FieldName,stateMachineProperty.FieldName, transition.Name)
                        //    ));
                    }
                }
            }


        }
    }
}