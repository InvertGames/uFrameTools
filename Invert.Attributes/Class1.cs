using System;

public class TemplateClass : Attribute
{
    private string _classNameFormat = "{0}";
    private MemberGeneratorLocation _location = MemberGeneratorLocation.Both;
    private bool _autoInherit = true;
    public bool IsEditorExtension { get; set; }

    public MemberGeneratorLocation Location
    {
        get { return _location; }
        set { _location = value; }
    }

    public string ClassNameFormat
    {
        get { return _classNameFormat; }
        set { _classNameFormat = value; }
    }

    public string OutputFolderName { get; set; }

    public bool AutoInherit
    {
        get { return _autoInherit; }
        set { _autoInherit = value; }
    }

    public TemplateClass()
    {
    }

    public TemplateClass(MemberGeneratorLocation location)
    {
        Location = location;
    }

    public TemplateClass(string outputFolderName, MemberGeneratorLocation location)
    {
        OutputFolderName = outputFolderName;
        Location = location;
    }

    public TemplateClass(string outputFolderName, string classNameFormat, MemberGeneratorLocation location)
    {
        OutputFolderName = outputFolderName;
        ClassNameFormat = classNameFormat;
        Location = location;
    }
}

public class TemplateMember : Attribute
{
    private string _nameFormat = "{0}";

    public string NameFormat
    {
        get { return _nameFormat; }
        set { _nameFormat = value; }
    }

    public MemberGeneratorLocation Location { get; set; }
    public TemplateMember()
    {
    }


    public TemplateMember(MemberGeneratorLocation location)
    {
        Location = location;
    }
}

public class TemplateMethod : TemplateMember
{
    private bool _callBase = true;

    public TemplateMethod(MemberGeneratorLocation location) : base(location)
    {
    }

    public TemplateMethod() : base()
    {
    }

    public TemplateMethod(MemberGeneratorLocation location, bool callBase) : base(location)
    {
        CallBase = callBase;
    }

    public TemplateMethod(string nameFormat,MemberGeneratorLocation location, bool callBase) : base(location)
    {
        _callBase = callBase;
        NameFormat = nameFormat;
        AutoFill = AutoFillType.NameOnly;
    }

    public bool CallBase
    {
        get { return _callBase; }
        set { _callBase = value; }
    }

    public AutoFillType AutoFill { get; set; }
}

public class TemplateConstructor : TemplateMember
{
    public string[] BaseCallArgs { get; set; }

    public TemplateConstructor(MemberGeneratorLocation location, params string[] baseCallArgs) : base(location)
    {
        BaseCallArgs = baseCallArgs;
    }

    public TemplateConstructor(params string[] baseCallArgs)
    {
        BaseCallArgs = baseCallArgs;
    }
}
public enum AutoFillType
{
    None,
    NameOnly,
    NameOnlyWithBackingField,
    NameAndType,
    NameAndTypeWithBackingField
}
public class TemplateProperty : TemplateMember
{
    private AutoFillType _autoFill;// = AutoFillType.NameAndType;
  

    public TemplateProperty(MemberGeneratorLocation location, AutoFillType autoFill)
        : base(location)
    {
        _autoFill = autoFill;
    }

    public TemplateProperty(string nameFormat)
    {
        NameFormat = nameFormat;
    }
    public TemplateProperty(string nameFormat, AutoFillType autoFill)
    {
        NameFormat = nameFormat;
        AutoFill = autoFill;
    }

    public TemplateProperty(MemberGeneratorLocation location, string nameFormat) : base(location)
    {
        NameFormat = nameFormat;
    }

    public TemplateProperty(MemberGeneratorLocation location, string nameFormat, AutoFillType autoFill)
        : base(location)
    {
        NameFormat = nameFormat;
        AutoFill = autoFill;
    }

    public TemplateProperty(MemberGeneratorLocation location) : base(location)
    {
    }

    public TemplateProperty()
        : base()
    {
    }

    public AutoFillType AutoFill
    {
        get { return _autoFill; }
        set { _autoFill = value; }
    }
}
[Flags]
 public enum MemberGeneratorLocation 
    {
        DesignerFile = 0,
        EditableFile = 1,
        Both = DesignerFile | EditableFile
    }