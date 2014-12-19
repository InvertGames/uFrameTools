using Invert.Core.GraphDesigner;

public class ShellContextCommandTemplate : EditorCommand<GenericNode>
{
    [TemplateMethod(MemberGeneratorLocation.Both,true)]
    public override void Perform(GenericNode node)
    {
        
    }

    [TemplateMethod(MemberGeneratorLocation.Both, true)]
    public override string CanPerform(GenericNode node)
    {
        return null;
    }
}