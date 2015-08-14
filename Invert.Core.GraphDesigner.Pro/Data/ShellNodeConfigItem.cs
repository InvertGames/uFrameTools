using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;

public class ShellNodeConfigItem : GenericNodeChildItem, IShellNodeConfigItem, IClassTypeNode
{
    [JsonProperty, InspectorProperty]
    public int Row { get; set; }
    [JsonProperty, InspectorProperty]
    public int Column { get; set; }
    [JsonProperty, InspectorProperty]
    public int ColumnSpan { get; set; }
    [JsonProperty, InspectorProperty]
    public bool IsNewRow { get; set; }

    [JsonProperty,InspectorProperty(InspectorType.TextArea)]
    public string Comments { get; set; }

    [InspectorProperty,JsonProperty]
    public override string Name
    {
        get { return base.Name; }
        set { base.Name = value; }
    }

    private string _typeName;
    private SectionVisibility _visibility;


    //[InspectorProperty, JsonProperty]
    public virtual string TypeName
    {
        get
        {
            return Regex.Replace(Name, @"[^a-zA-Z0-9_\.]+", "");
            if (string.IsNullOrEmpty(_typeName))
            {

            }
            return _typeName;
        }
        set { _typeName = value; }
    }

    public override bool AutoFixName
    {
        get { return false; }
    }

    [InspectorProperty, JsonProperty]
    public SectionVisibility Visibility
    {
        get { return _visibility; }
        set
        {
            _visibility = value;
            this.Changed("Visibility", _visibility, value);
        }
    }

    public virtual string ClassName
    {
        get { return this.Node.Name + TypeName; }
    }

    public string ReferenceClassName
    {
        get { return "I" + this.TypeName + "Connectable"; }
    }
    public virtual IEnumerable<IShellNodeConfigItem> IncludedInSections
    {
        get
        {
            return this.OutputsTo<IShellNodeConfigItem>();
        }
    }

    public override void Document(IDocumentationBuilder docs)
    {
        base.Document(docs);
        var className = ClassName;
        var type = InvertApplication.FindTypeByName(className);
        if (type == null)
        {
            InvertApplication.Log("Couldn't find type in documentation " + className);
            // base.Document(docs);
        }
        else
        {
            var instance = Activator.CreateInstance(type) as IDiagramNodeItem;
            if (instance == null)
            {
                //base.Document(docs);
            }
            else
            {
                instance.Document(docs);
            }

        }
    }
}

public class ShellNodeConfigSelector : ShellNodeConfigItem
{
    
}

