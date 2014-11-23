using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Pro;

public class ShellGeneratorTypeNode : GenericInheritableNode
{
    private Type _baseType;
    private string _baseTypeString;
    private string _classNameFormat = "{0}";

    public ShellGeneratorTypeNode()
    {
        _baseTypeString = typeof (System.Object).Name;
    }

    
    public Type BaseType
    {
        get
        {
            
            return _baseType ?? (_baseType = InvertApplication.FindTypeByName(BaseTypeString));
        }
        set { _baseType = value; }
    }

    [JsonProperty, InspectorProperty(InspectorType.TypeSelection)]
    public string BaseTypeString
    {
        get { return _baseTypeString; }
        set { _baseTypeString = value;
            _baseType = null;
        }
    }
    [JsonProperty,InspectorProperty]
    public bool IsEditorExtension { get; set; }

    [JsonProperty,InspectorProperty]
    public bool IsDesignerOnly { get; set; }

    [JsonProperty, InspectorProperty]
    public string ClassNameFormat
    {
        get { return _classNameFormat; }
        set { _classNameFormat = value; }
    }

    [JsonProperty, InspectorProperty]
    public string FolderName { get; set; }

    public ShellNodeGeneratorsSlot ShellNodeShellNodeGeneratorsSlot
    {
        get
        {
            return this.InputFrom<ShellNodeGeneratorsSlot>();
        }
    }
    public ShellNodeTypeNode GeneratorFor
    {
        get
        {
            var channel = ShellNodeShellNodeGeneratorsSlot;
            if (channel == null) return null;
            return channel.Node as ShellNodeTypeNode;
        }
    }
    public IEnumerable<OverrideMethodReference> Overrides
    {
        get { return ChildItems.OfType<OverrideMethodReference>(); }
    }
}

public class ShellGeneratorTypeNodeGenerator : GenericNodeGenerator<ShellGeneratorTypeNode>
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        TryAddNamespace("System.CodeDom");

        if (IsDesignerFile)
        {
            InitializeMethod = Decleration.protected_override_func(null, "Initialize", "CodeFileGenerator", "generator")
                .invoke_base();
            InitializeDesignerMethod = Decleration.protected_override_func(null, "InitializeDesignerFile")
                .invoke_base();
            InitializeEditorMethod = Decleration.protected_override_func(null, "InitializeEditableFile")
                .invoke_base();
        }

        var type = Data.BaseType;

        var templateMethods =
            type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);

        // Template Methods
        foreach (var templateMethod in templateMethods)
        {
            var attribute = templateMethod.GetCustomAttributes(typeof (TemplateMethod), true).OfType<TemplateMethod>().FirstOrDefault();
            if (attribute == null) continue;

            var fillMethod = Decleration
                     .public_virtual_func("CodeMemberMethod",
                         string.Format("Decorate{0}Method", templateMethod.Name),
                         "CodeMemberMethod",
                         "method");

            if (IsDesignerFile)
            {
                var locationMethod = GetMethodByLocation(attribute.Location);
                var varName = string.Format("{0}_{1}", type.Name, templateMethod.Name);
                locationMethod._("var {0} = CreateMethodFromType<{1}>(\"{2}\")", varName, type.Name, templateMethod.Name);
                locationMethod._("AddMember({0}({1}))", fillMethod.Name, varName);
            }
            else
            {
                fillMethod.Attributes |= MemberAttributes.Override;
                fillMethod.invoke_base();
                fillMethod.Statements.Add(new CodeCommentStatement("// return null if conditions aren't met."));

                fillMethod._("return method");
            }
          
        }

        // Template Properties
        var templateProperties =
            type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (var templateProperty in templateProperties)
        {
            var attribute = templateProperty.GetCustomAttributes(typeof(TemplateProperty), true).OfType<TemplateProperty>().FirstOrDefault();
            if (attribute == null) continue;
        }
    }

    public CodeMemberMethod GetMethodByLocation(MemberGeneratorLocation location)
    {
        if (location == MemberGeneratorLocation.DesignerFile && location != MemberGeneratorLocation.EditableFile)
        {
            return InitializeDesignerMethod;
        }
        if (location == MemberGeneratorLocation.EditableFile && location != MemberGeneratorLocation.DesignerFile)
        {
            return InitializeEditorMethod;
        }
        return InitializeMethod;
    }
    public CodeMemberMethod InitializeMethod { get; set; }

    public CodeMemberMethod InitializeEditorMethod { get; set; }

    public CodeMemberMethod InitializeDesignerMethod { get; set; }
}

public class OverrideMethodReference : GenericNodeChildItem
{
  
}