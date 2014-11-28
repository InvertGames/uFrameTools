using System;

public class TemplateClass : Attribute
{
}

public class TemplateMember : Attribute
{
    public string Group { get; set; }
    public MemberGeneratorLocation Location { get; set; }
    public TemplateMember()
    {
    }

    public TemplateMember(string @group)
    {
        Group = @group;
    }

    public TemplateMember(MemberGeneratorLocation location, string @group)
    {
        Group = @group;
        Location = location;
    }
}

public class TemplateMethod : TemplateMember
{
    public TemplateMethod(MemberGeneratorLocation location, string @group) : base(location, @group)
    {
    }

    public TemplateMethod(string @group) : base(@group)
    {
    }
}

public class TemplateProperty : TemplateMember
{
    public TemplateProperty(MemberGeneratorLocation location, string @group) : base(location, @group)
    {
    }

    public TemplateProperty(string @group)
        : base(@group)
    {
    }
    
}
[Flags]
 public enum MemberGeneratorLocation 
    {
        DesignerFile = 0,
        EditableFile = 1,
        Both = DesignerFile | EditableFile
    }