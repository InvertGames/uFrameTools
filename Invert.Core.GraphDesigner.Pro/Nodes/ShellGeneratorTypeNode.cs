using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;

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

public class OverrideMethodReference : GenericNodeChildItem
{
  
}