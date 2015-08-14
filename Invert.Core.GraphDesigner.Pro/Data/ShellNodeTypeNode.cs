using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using Invert.Common;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;
using UnityEngine;


public interface IShellNodeTypeClass : IShellNodeConfigItem
{
    //bool AllowMultipleInputs { get; set; }
    //bool AllowMultipleOutputs { get; set; }
    bool Inheritable { get; set; }
    IEnumerable<ShellNodeConfigSection> Sections { get; set; }
    IEnumerable<ShellNodeConfigInput> InputSlots { get; set; }
    IEnumerable<ShellNodeConfigOutput> OutputSlots { get; set; }

}

public class ShellNodeTypeNode : ShellInheritableNode, IShellNode, IShellConnectable
{
    private string _classFormat = "{0}";
    private bool _allowMultipleOutputs;

    [JsonProperty, InspectorProperty]
    public bool MultipleInputs { get; set; }

    [JsonProperty, InspectorProperty]
    public bool MultipleOutputs
    {
        get
        {
            if (this["Inheritable"])
            {
                return true;
            }
            return _allowMultipleOutputs;
        }
        set { _allowMultipleOutputs = value; }
    }

    public NodeColor Color
    {
        get
        {
            var color = DataBag["Color"];
            if (color != null)
            {
                try
                {
                    var value = (NodeColor)Enum.Parse(typeof(NodeColor), color, true);
                    return value;
                }
                catch (Exception ex)
                {
                    return NodeColor.Gray;
                }
            }
            return NodeColor.Gray;
        }
    }

    [Browsable(false)]
    public IShellNode ReferenceType
    {
        get { return GetConnectionReference<ReferenceItemType>().InputFrom<IShellNode>(); }
    }
    [InspectorProperty]
    public bool Inheritable
    {
        get
        {
            return this["Inheritable"];
        }
        set { this["Inheritable"] = value; }

    }


    [Browsable(false)]
    [OutputSlot("Sub Nodes")]
    public MultiOutputSlot<ShellNodeTypeNode> SubNodesSlot { get; set; }
    [Browsable(false)]
    public IEnumerable<ShellNodeTypeNode> SubNodes
    {
        get { return SubNodesSlot.Items; }
    }


    [Browsable(false)]
    [ReferenceSection("Sections", SectionVisibility.WhenNodeIsFilter, false)]
    public IEnumerable<ShellNodeSectionsSlot> Sections
    {
        get { return PersistedItems.OfType<ShellNodeSectionsSlot>(); }
    }
    [Browsable(false)]
    public IEnumerable<ShellNodeTypeSection> PossibleSections
    {
        get { return this.Repository.AllOf<ShellNodeTypeSection>(); }
    }
    [Browsable(false)]
    public IEnumerable<IReferenceNode> IncludedInSections
    {
        get
        {
            return this.Repository.AllOf<IReferenceNode>().Where(p => p.AcceptableTypes.Any(x => x.SourceItem == this));
        }
    }
    [Browsable(false)]
    public IEnumerable<ShellNodeTypeReferenceSection> ReferenceSections
    {
        get { return Sections.Select(p => p.SourceItem).OfType<ShellNodeTypeReferenceSection>(); }
    }
    [Browsable(false)]
    [ReferenceSection("Inputs", SectionVisibility.WhenNodeIsFilter, true)]
    public IEnumerable<ShellNodeInputsSlot> InputSlots
    {
        get
        {
            return PersistedItems.OfType<ShellNodeInputsSlot>();
        }
    }
    [Browsable(false)]
    [ReferenceSection("Outputs", SectionVisibility.WhenNodeIsFilter, true)]
    public IEnumerable<ShellNodeOutputsSlot> OutputSlots
    {
        get
        {
            return PersistedItems.OfType<ShellNodeOutputsSlot>();
        }
    }
    [Browsable(false)]
    public IEnumerable<ShellSlotTypeNode> PossibleInputSlots
    {
        get { return this.Repository.AllOf<ShellSlotTypeNode>().Where(p => !p.IsOutput); }
    }
    [Browsable(false)]
    public IEnumerable<ShellSlotTypeNode> PossibleOutputSlots
    {
        get { return this.Repository.AllOf<ShellSlotTypeNode>().Where(p => p.IsOutput); }
    }

    //[Section("Custom Selectors", SectionVisibility.WhenNodeIsFilter)]
    [Browsable(false)]
    public IEnumerable<ShellPropertySelectorItem> CustomSelectors
    {
        get
        {
            return PersistedItems.OfType<ShellPropertySelectorItem>();
        }
    }

    public override string ClassName
    {
        get { return Name + "Node"; }
    }
    [Browsable(false)]
    [ReferenceSection("Connectable To", SectionVisibility.WhenNodeIsFilter, false)]
    public IEnumerable<ShellConnectableReferenceType> ConnectableTo
    {
        get { return PersistedItems.OfType<ShellConnectableReferenceType>(); }
    }

    [Browsable(false)]
    public IEnumerable<IShellNode> PossibleConnectableTo
    {
        get { return this.Repository.AllOf<IShellNode>(); }
    }

}

public class ShellTemplateConfigNode : GenericNode
{
    
    private bool _autoInherit = true;

    public IShellNodeConfigItem NodeConfig
    {
        get { return this.InputFrom<IShellNodeConfigItem>(); }
    }

    [JsonProperty, NodeProperty]
    public string OutputPath { get; set; }
    [JsonProperty, NodeProperty]
    public string ClassNameFormat { get; set; }

    [JsonProperty,InspectorProperty(InspectorType.TypeSelection)]
    public string TemplateBaseClass { get; set; }

    [JsonProperty, NodeProperty]
    public TemplateLocation Files { get; set; }

    [JsonProperty,NodeProperty]
    public bool AutoInherit
    {
        get { return _autoInherit; }
        set { _autoInherit = value; }
    }
}

public class ShellNodeConfig : ShellInheritableNode, IShellNodeTypeClass, IDocumentable
{
    public override bool AllowMultipleInputs
    {
        get { return true; }
    }

    private string _nodeLabel;
    private NodeColor _color;
    private bool _inheritable;
    private bool _isClass;

    public override void Document(IDocumentationBuilder docs)
    {
        docs.BeginSection(this.Name);
        docs.Section(this.Node.Name);
        docs.NodeImage(this);
        var className = FullName + "Node";
        var type = InvertApplication.FindType(className);
        if (type == null)
        {
            //Debug.Log("Couldn't find type in documentation " + className);
           // base.Document(docs);
        }
        else
        {
            var instance =  Activator.CreateInstance(type) as IDiagramNodeItem;
            if (instance == null)
            {
               //base.Document(docs);
            }
            else
            {
                instance.Document(docs);
            }

        }

        foreach (var item in PersistedItems.OfType<IShellNodeConfigItem>())
        {
            docs.BeginArea("NODEITEM");
            item.Document(docs);
            docs.EndArea();
        }
            
        docs.EndSection();
    }

    public string NodeLabel
    {
        get
        {
            if (string.IsNullOrEmpty(_nodeLabel))
                return Name;
            return _nodeLabel;
        }
        set { _nodeLabel = value; }
    }

    [InspectorProperty, JsonProperty]
    public NodeColor Color
    {
        get { return _color; }
        set
        {
            _color = value;
            this.Changed("Color", _color, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool Inheritable
    {
        get { return _inheritable; }
        set
        {
            _inheritable = value;
            this.Changed("Inheritable", _inheritable, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool IsClass
    {
        get { return _isClass; }
        set
        {
            _isClass = value;
            this.Changed("IsClass", _isClass, value);
        }
    }

    private SectionVisibility _visibility;
    private int _column;
    private int _row;

    [InspectorProperty, JsonProperty]
    public int Row
    {
        get { return _row; }
        set
        {
            _row = value;
            this.Changed("Row", _row, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public int Column
    {
        get { return _column; }
        set
        {
            _column = value;
            this.Changed("Column", _column, value);
        }
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

    public string ReferenceClassName
    {
        get { return "I" + this.Name + "Connectable"; }
    }

    public override string ClassName
    {
        get { return this.Name + "Node"; }
    }

    public IEnumerable<ShellNodeConfigSection> Sections
    {
        get { return PersistedItems.OfType<ShellNodeConfigSection>().Concat(PersistedItems.OfType<ShellNodeConfigSectionPointer>().Select(p => p.SourceItem)); }
        set { }
    }

    public IEnumerable<ShellNodeConfigInput> InputSlots
    {
        get { return PersistedItems.OfType<ShellNodeConfigInput>().Concat(PersistedItems.OfType<ShellNodeConfigInputPointer>().Select(p => p.SourceItem)); }
        set { }
    }

    public IEnumerable<ShellNodeConfigOutput> OutputSlots
    {
        get { return PersistedItems.OfType<ShellNodeConfigOutput>().Concat(PersistedItems.OfType<ShellNodeConfigOutputPointer>().Select(p => p.SourceItem)); }
        set { }
    }

    public IEnumerable<IShellNodeConfigItem> IncludedInSections
    {
        get { return this.OutputsTo<IShellNodeConfigItem>(); }
    }

    public string TypeName
    {
        get { return Name.Clean(); }
        set
        {
            
        }
    }

    [InspectorProperty]
    public bool IsGraphType
    {
        get { return this["Graph Type"]; }
        set { this["Graph Type"] = value; }
    }

    public IEnumerable<ShellNodeConfig> SubNodes
    {
        get { return this.FilterNodes().OfType<ShellNodeConfig>().Where(p=>p != this); }
    }
}

public class ShellNodeConfigViewModel : GenericNodeViewModel<ShellNodeConfig>
{
    public ShellNodeConfigViewModel(ShellNodeConfig graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {
    }

    public override NodeColor Color
    {
        get { return GraphItem.Color; }
    }

    public override bool IsCollapsed
    {
        get { return !GraphItem.PersistedItems.Any(); }
        set { base.IsCollapsed = value; }
    }

    public override bool AllowCollapsing
    {
        get { return false; }
    }
    
    protected override void CreateContent()
    {
        //base.CreateContent();
        
        foreach (var column in GraphItem.ChildItemsWithInherited.OfType<IShellNodeConfigItem>().GroupBy(p => p.Column))
        {
            foreach (var item in column.OrderBy(p => p.Row))
            {
                if (!IsVisible(item.Visibility)) continue;
                var section = item as ShellNodeConfigSection;
                if (section != null)
                {
                    CreateHeader(section, section);
                    continue;
                }
                var sectionPointer = item as ShellNodeConfigSectionPointer;
                if (sectionPointer != null)
                {
                    CreateHeader(sectionPointer.SourceItem, sectionPointer);
                    continue;
                }
                var input = item as ShellNodeConfigInput;
                if (input != null)
                {
                    CreateInput(input, input);
                    continue;
                }
                var inputPointer = item as ShellNodeConfigInputPointer;
                if (inputPointer != null)
                {
                    CreateInput(inputPointer.SourceItem, inputPointer);
                    continue;
                }
                var output = item as ShellNodeConfigOutput;
                if (output != null)
                {
                    CreateOutput(output, output);
                    continue;
                }
                var outputPointer = item as ShellNodeConfigOutputPointer;
                if (outputPointer != null)
                {
                    CreateOutput(outputPointer.SourceItem, outputPointer);
                    continue;
                }
            }
        }
    }

    private void CreateOutput(ShellNodeConfigOutput output, object dataObject)
    {
        var vm = new InputOutputViewModel()
        {
            IsInput = false,
            IsOutput = true,
            DiagramViewModel = this.DiagramViewModel,
            Name = output.Name,
            DataObject = dataObject,
            Column = output.Column,
            ColumnSpan = output.ColumnSpan,
            IsNewLine =  output.IsNewRow
        };
        ContentItems.Add(vm);
    }

    private void CreateInput(ShellNodeConfigInput input, object dataObject)
    {
        var vm = new InputOutputViewModel()
        {
            IsInput = true,
            IsOutput = false,
            DiagramViewModel = this.DiagramViewModel,
            Name = input.Name,
            DataObject = dataObject,
            Column = input.Column,
            ColumnSpan = input.ColumnSpan,
            IsNewLine = input.IsNewRow
        };
        ContentItems.Add(vm);
    }

    private void CreateHeader(ShellNodeConfigSection item, object dataObject)
    {
        var sectionViewModel = new GenericItemHeaderViewModel()
        {
            Name = item.Name,
            AddCommand = item.AllowAdding ? new LambdaCommand(()=>{}) : null,
            DataObject = dataObject,
            NodeViewModel = this,
            AllowConnections = true,
            Column = item.Column,
            ColumnSpan = item.ColumnSpan,
            IsNewLine = item.IsNewRow
        };
        ContentItems.Add(sectionViewModel);
    }

    public void AddSectionItem()
    {
        DiagramViewModel.CurrentRepository.Add(new ShellNodeConfigSection()
        {
            Node = GraphItem,
            Name = "New Section",
            IsNewRow = true,
        });
    }

    public void AddInputItem()
    {
        DiagramViewModel.CurrentRepository.Add(new ShellNodeConfigInput()
        {
            Node = GraphItem,
            Name = "New Input",
            IsNewRow = true,
        });

    }

    public void AddOutputItem()
    {
        DiagramViewModel.CurrentRepository.Add(new ShellNodeConfigOutput()
        {
            Node = GraphItem,
            Name = "New Output",
            IsNewRow = true,

        });
    }

    public void RemoveSelected()
    {
        DiagramViewModel.CurrentRepository.Remove(ContentItems.First(p => p.IsSelected).DataObject as IDiagramNodeItem);
    }

    public void AddSectionPointer(ShellNodeConfigSection item)
    {
        DiagramViewModel.CurrentRepository.Add(new ShellNodeConfigSectionPointer()
        {
            Node = GraphItem,
            SourceIdentifier = item.Identifier

        });
    }
    public void AddInputPointer(ShellNodeConfigInput item)
    {
        DiagramViewModel.CurrentRepository.Add(new ShellNodeConfigInputPointer()
        {
            Node = GraphItem,
            SourceIdentifier = item.Identifier
        });
    }
    public void AddOutputPointer(ShellNodeConfigOutput item)
    {
        DiagramViewModel.CurrentRepository.Add(new ShellNodeConfigOutputPointer()
        {
            Node = GraphItem,
            SourceIdentifier = item.Identifier
        });
    }
}

public interface IShellNodeConfigItem : IDocumentable, IClassTypeNode
{
    [JsonProperty, InspectorProperty]
    int Row { get; set; }
    [JsonProperty, InspectorProperty]
    int Column { get; set; }
    [InspectorProperty, JsonProperty]
    SectionVisibility Visibility { get; set; }
    string ReferenceClassName { get; }
    //string ClassName { get; }
    IEnumerable<IShellNodeConfigItem> IncludedInSections { get; }
    string TypeName { get; set; }
}
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
public class ShellNodeConfigSection : ShellNodeConfigItem
{
    private bool _allowAdding;
    private ShellNodeConfigSectionType _sectionType;
    private bool _isTyped;
    private bool _isEditable;
    private bool _allowDuplicates;
    private bool _isAutomatic;
    private bool _hasPredefinedOptions;

    [JsonProperty, InspectorProperty]
    public ShellNodeConfigSectionType SectionType
    {
        get { return _sectionType; }
        set
        {
            _sectionType = value;
            this.Changed("SectionType", _sectionType, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool IsTyped
    {
        get { return _isTyped; }
        set
        {
            _isTyped = value;
            this.Changed("IsTyped", _isTyped, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public virtual bool AllowAdding
    {
        get
        {
            if (SectionType == ShellNodeConfigSectionType.ChildItems)
            {
                return true;
            }
            return _allowAdding;
        }
        set
        {
            _allowAdding = value;
            this.Changed("AllowAdding", _allowAdding, value);
        }
    }

    public override string ClassName
    {
        get
        {
            if (SectionType == ShellNodeConfigSectionType.ChildItems)
            {
                return TypeName + "ChildItem";
            }
            return TypeName + "Reference";
        }
    }


    [InspectorProperty, JsonProperty]
    public bool IsEditable
    {
        get { return _isEditable; }
        set
        {
            _isEditable = value;
            this.Changed("IsEditable", _isEditable, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool AllowDuplicates
    {
        get { return _allowDuplicates; }
        set
        {
            _allowDuplicates = value;
            this.Changed("AllowDuplicates", _allowDuplicates, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool IsAutomatic
    {
        get { return _isAutomatic; }
        set
        {
            _isAutomatic = value;
            this.Changed("IsAutomatic", _isAutomatic, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool HasPredefinedOptions
    {
        get { return _hasPredefinedOptions; }
        set
        {
            _hasPredefinedOptions = value;
            this.Changed("HasPredefinedOptions", _hasPredefinedOptions, value);
        }
    }
}

public enum ShellNodeConfigSectionType
{
    ChildItems,
    ReferenceItems,
    ProxyItems
}
public class ShellNodeConfigSectionPointer : GenericReferenceItem<ShellNodeConfigSection>, IShellNodeConfigItem
{
    private SectionVisibility _visibility;
    private int _column;
    private int _row;

    [InspectorProperty, JsonProperty]
    public int Row
    {
        get { return _row; }
        set
        {
            _row = value;
            this.Changed("Row", _row, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public int Column
    {
        get { return _column; }
        set
        {
            _column = value;
            this.Changed("Column", _column, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public SectionVisibility Visibility
    {
        get { return _visibility; }
        set
        {
            _visibility = value;
            this.Changed("Visibility",_visibility,value);
        }
    }

    public string ReferenceClassName
    {
        get { return this.SourceItem.ReferenceClassName; }
    }

    public string ClassName
    {
        get { return this.SourceItem.ClassName; }
    }

    public IEnumerable<IShellNodeConfigItem> IncludedInSections
    {
        get { return this.OutputsTo<IShellNodeConfigItem>(); }
    }

    public string TypeName
    {
        get { return SourceItem.TypeName; }
        set
        {
            
        }
    }

    public bool AllowMultiple { get; set; }
}
public class ShellNodeConfigInputPointer : GenericReferenceItem<ShellNodeConfigInput>
{
    private SectionVisibility _visibility;
    private int _column;
    private int _row;

    [InspectorProperty, JsonProperty]
    public int Row
    {
        get { return _row; }
        set
        {
            _row = value;
            this.Changed("Row", _row, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public int Column
    {
        get { return _column; }
        set
        {
            _column = value;
            this.Changed("Column", _column, value);
        }
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
    public string ClassName
    {
        get { return this.SourceItem.TypeName; }
    }
}
public class ShellNodeConfigOutputPointer : GenericReferenceItem<ShellNodeConfigOutput>
{
    private SectionVisibility _visibility;
    private int _column;
    private int _row;

    [InspectorProperty, JsonProperty]
    public int Row
    {
        get { return _row; }
        set
        {
            _row = value;
            this.Changed("Row", _row, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public int Column
    {
        get { return _column; }
        set
        {
            _column = value;
            this.Changed("Column", _column, value);
        }
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
    public string ClassName
    {
        get { return this.SourceItem.TypeName; }
    }
}
public class ShellNodeConfigInput : ShellNodeConfigItem, IShellSlotType
{
    private bool _allowMultiple;

    public bool IsOutput
    {
        get { return false; }
        set
        {

        }
    }

    [JsonProperty, InspectorProperty]
    public bool AllowMultiple
    {
        get { return _allowMultiple; }
        set
        {
            _allowMultiple = value;
            this.Changed("AllowMultiple", _allowMultiple, value);
        }
    }

    public override string TypeName
    {
        get
        {
            return Regex.Replace(Name, @"[^a-zA-Z0-9_\.]+", "");

        }
        set { }
    }

    public override string ClassName
    {
        get { return TypeName; }
    }
}

public class ShellNodeConfigOutput : ShellNodeConfigItem, IShellSlotType
{
    private bool _allowMultiple;

    public bool IsOutput
    {
        get { return true; }
        set
        {

        }
    }
    public override string TypeName
    {
        get
        {
            return Regex.Replace(Name, @"[^a-zA-Z0-9_\.]+", "");

        }
        set { }
    }
    [JsonProperty, InspectorProperty]
    public bool AllowMultiple
    {
        get { return _allowMultiple; }
        set
        {
            _allowMultiple = value;
            this.Changed("AllowMultiple", _allowMultiple, value);
        }
    }
    public override string ClassName
    {
        get { return TypeName; }
    }
}
public class ShellNodeConfigDrawer : DiagramNodeDrawer<ShellNodeConfigViewModel>, IInspectorDrawer
{
    public ShellNodeConfigDrawer(ShellNodeConfigViewModel viewModel)
        : base(viewModel)
    {
    }

    public override void Draw(IPlatformDrawer platform, float scale)
    {
        base.Draw(platform, scale);

        if (IsSelected)
        {
            var selectedChild = Children.Skip(1).FirstOrDefault(p => p.IsSelected);
            var width = 75f;
            var buttonHeight = 25;
            var toolbarRect = new Rect(this.Bounds.x - width - 4, this.Bounds.y + 8, width, selectedChild == null ? (buttonHeight *3) + 20 : (buttonHeight *4) + 20);

            platform.DrawStretchBox(toolbarRect, CachedStyles.Item3, 12f);
            toolbarRect.y += 10;
            var x = toolbarRect.x;
            var y = toolbarRect.y;

            if (selectedChild != null)
            {
                platform.DoButton(new Rect(x,y,toolbarRect.width,buttonHeight),"Remove",CachedStyles.Item2,
                    () =>
                    {
                        NodeViewModel.RemoveSelected();
                    });
                y += buttonHeight;
            }
            platform.DoButton(new Rect(x, y, toolbarRect.width, buttonHeight), "+ Add Section", CachedStyles.Item2,
                   () =>
                   {
                       ShowAddPointerMenu<ShellNodeConfigSection>("Section", () =>
                       {
                           NodeViewModel.AddSectionItem();
                       }, _ => { NodeViewModel.AddSectionPointer(_); });
                   });
            y += buttonHeight;
            platform.DoButton(new Rect(x, y, toolbarRect.width, buttonHeight), "+ Input", CachedStyles.Item2,
                   () =>
                   {
                       ShowAddPointerMenu<ShellNodeConfigInput>("Input", () =>
                       {
                           NodeViewModel.AddInputItem();
                       }, _ => { NodeViewModel.AddInputPointer(_); });
                   });
            y += buttonHeight;
            platform.DoButton(new Rect(x, y, toolbarRect.width, buttonHeight), "+ Output", CachedStyles.Item2,
                   () =>
                   {
                       ShowAddPointerMenu<ShellNodeConfigOutput>("Output", () =>
                       {
                           NodeViewModel.AddOutputItem();
                       }, _ => { NodeViewModel.AddOutputPointer(_); });

                   });
            y += buttonHeight;



        }


    }

    private void ShowAddPointerMenu<TItem>(string name, Action addItem, Action<TItem> addPointer) where TItem : IDiagramNodeItem
    {


#if UNITY_DLL
        var ctxMenu = new UnityEditor.GenericMenu();
        ctxMenu.AddItem(new GUIContent("New " + name), false,
            () => { InvertApplication.Execute(() => { addItem(); }); });
        ctxMenu.AddSeparator("");
        var nodeConfigSection =
            NodeViewModel.DiagramViewModel.CurrentRepository.AllOf<TItem>();
        foreach (var item in nodeConfigSection)
        {
            ctxMenu.AddItem(new GUIContent(item.Name), false,
                () => { InvertApplication.Execute(() => { addPointer(item); }); });
        }
        ctxMenu.ShowAsContext();
#else
        var menu = InvertGraphEditor.Container.Resolve<ContextMenuUI>();
        menu.Handler = InvertGraphEditor.DesignerWindow;
        menu.AddCommand(new SimpleEditorCommand<DiagramNodeViewModel>(_ =>
        {
            addItem();
        }, "New " + name));
        menu.Go();
        //menu.AddSeparator("");
        //menu.AddCommand(new SimpleEditorCommand<DiagramNodeViewModel>(_ =>
        //{
        //    addItem();
        //}, "New " + name));
#endif
    }

    protected override void DrawChildren(IPlatformDrawer platform, float scale)
    {
        for (int index = 0; index < Children.Count; index++)
        {
            var item = Children[index];

            if (index == 0)
            {
                item.Draw(platform, scale);
                continue;
            }
            var optionsBounds = new Rect(item.Bounds.x, item.Bounds.y + 4, item.Bounds.width,
                item.Bounds.height);
            if (item.IsSelected)
            {
                platform.DrawStretchBox(optionsBounds, CachedStyles.Item1, 0f);
            }
            optionsBounds.width -= 35;
            //optionsBounds.x += 15;
            item.Draw(platform, scale);
            platform.DoButton(optionsBounds, "", CachedStyles.ClearItemStyle, () =>
            {
                ViewModel.DiagramViewModel.DeselectAll();
                ViewModel.Select();
                item.ViewModelObject.Select();
                InvertApplication.SignalEvent < IGraphSelectionEvents>(_=>_.SelectionChanged(item.ViewModelObject));
            });
        }
    }

    public void DrawInspector(IPlatformDrawer platformDrawer)
    {
#if UNITY_DLL
       
#endif
    }
}
