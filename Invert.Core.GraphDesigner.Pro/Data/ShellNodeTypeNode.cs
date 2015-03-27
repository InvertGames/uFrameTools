using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using Invert.Common;
using Invert.Core;
using Invert.Core.GraphDesigner;
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
        get { return ChildItems.OfType<ShellNodeSectionsSlot>(); }
    }
    [Browsable(false)]
    public IEnumerable<ShellNodeTypeSection> PossibleSections
    {
        get { return Project.NodeItems.OfType<ShellNodeTypeSection>(); }
    }
    [Browsable(false)]
    public IEnumerable<IReferenceNode> IncludedInSections
    {
        get
        {
            return Project.NodeItems.OfType<IReferenceNode>().Where(p => p.AcceptableTypes.Any(x => x.SourceItem == this));
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
            return ChildItems.OfType<ShellNodeInputsSlot>();
        }
    }
    [Browsable(false)]
    [ReferenceSection("Outputs", SectionVisibility.WhenNodeIsFilter, true)]
    public IEnumerable<ShellNodeOutputsSlot> OutputSlots
    {
        get
        {
            return ChildItems.OfType<ShellNodeOutputsSlot>();
        }
    }
    [Browsable(false)]
    public IEnumerable<ShellSlotTypeNode> PossibleInputSlots
    {
        get { return Project.NodeItems.OfType<ShellSlotTypeNode>().Where(p => !p.IsOutput); }
    }
    [Browsable(false)]
    public IEnumerable<ShellSlotTypeNode> PossibleOutputSlots
    {
        get { return Project.NodeItems.OfType<ShellSlotTypeNode>().Where(p => p.IsOutput); }
    }

    //[Section("Custom Selectors", SectionVisibility.WhenNodeIsFilter)]
    [Browsable(false)]
    public IEnumerable<ShellPropertySelectorItem> CustomSelectors
    {
        get
        {
            return ChildItems.OfType<ShellPropertySelectorItem>();
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
        get { return ChildItems.OfType<ShellConnectableReferenceType>(); }
    }

    [Browsable(false)]
    public IEnumerable<IShellNode> PossibleConnectableTo
    {
        get { return Project.NodeItems.OfType<IShellNode>(); }
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
    public MemberGeneratorLocation Files { get; set; }

    [JsonProperty,NodeProperty]
    public bool AutoInherit
    {
        get { return _autoInherit; }
        set { _autoInherit = value; }
    }
    
}

public class ShellNodeConfig : ShellInheritableNode, IShellNodeTypeClass, IDocumentable
{
    private string _nodeLabel;
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
    public NodeColor Color { get; set; }

    [InspectorProperty, JsonProperty]
    public bool Inheritable { get; set; }

    [InspectorProperty, JsonProperty]
    public bool IsClass { get; set; }

    public int Row { get; set; }

    public int Column { get; set; }

    public SectionVisibility Visibility { get; set; }

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
        get { return ChildItems.OfType<ShellNodeConfigSection>().Concat(ChildItems.OfType<ShellNodeConfigSectionPointer>().Select(p => p.SourceItem)); }
        set { }
    }

    public IEnumerable<ShellNodeConfigInput> InputSlots
    {
        get { return ChildItems.OfType<ShellNodeConfigInput>().Concat(ChildItems.OfType<ShellNodeConfigInputPointer>().Select(p => p.SourceItem)); }
        set { }
    }

    public IEnumerable<ShellNodeConfigOutput> OutputSlots
    {
        get { return ChildItems.OfType<ShellNodeConfigOutput>().Concat(ChildItems.OfType<ShellNodeConfigOutputPointer>().Select(p => p.SourceItem)); }
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
        get { return this.GetContainingNodes(Project).OfType<ShellNodeConfig>(); }
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
        get { return GraphItem.ChildItems.Count == 0; }
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
            AddCommand = item.AllowAdding ? new SimpleEditorCommand<DiagramNodeViewModel>(_ => { }) : null,
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
        DiagramViewModel.CurrentRepository.AddItem(new ShellNodeConfigSection()
        {
            Node = GraphItem,
            Name = "New Section",
         IsNewRow = true,
        });
    }

    public void AddInputItem()
    {
        DiagramViewModel.CurrentRepository.AddItem(new ShellNodeConfigInput()
        {
            Node = GraphItem,
            Name = "New Input",
            IsNewRow = true,
        });

    }

    public void AddOutputItem()
    {
        DiagramViewModel.CurrentRepository.AddItem(new ShellNodeConfigOutput()
        {
            Node = GraphItem,
            Name = "New Output",
            IsNewRow = true,

        });
    }

    public void RemoveSelected()
    {
        DiagramViewModel.CurrentRepository.RemoveItem(ContentItems.First(p => p.IsSelected).DataObject as IDiagramNodeItem);
    }

    public void AddSectionPointer(ShellNodeConfigSection item)
    {
        DiagramViewModel.CurrentRepository.AddItem(new ShellNodeConfigSectionPointer()
        {
            Node = GraphItem,
            SourceIdentifier = item.Identifier

        });
    }
    public void AddInputPointer(ShellNodeConfigInput item)
    {
        DiagramViewModel.CurrentRepository.AddItem(new ShellNodeConfigInputPointer()
        {
            Node = GraphItem,
            SourceIdentifier = item.Identifier
        });
    }
    public void AddOutputPointer(ShellNodeConfigOutput item)
    {
        DiagramViewModel.CurrentRepository.AddItem(new ShellNodeConfigOutputPointer()
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

    [InspectorProperty]
    public override string Name
    {
        get { return base.Name; }
        set { base.Name = value; }
    }

    private string _typeName;


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
    public SectionVisibility Visibility { get; set; }

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

    [JsonProperty, InspectorProperty]
    public ShellNodeConfigSectionType SectionType { get; set; }
    [InspectorProperty, JsonProperty]
    public bool IsTyped { get; set; }

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
        set { _allowAdding = value; }
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
    public bool IsEditable { get; set; }

    [InspectorProperty, JsonProperty]
    public bool AllowDuplicates { get; set; }

    [InspectorProperty, JsonProperty]
    public bool IsAutomatic { get; set; }
    [InspectorProperty, JsonProperty]
    public bool HasPredefinedOptions { get; set; }
}

public enum ShellNodeConfigSectionType
{
    ChildItems,
    ReferenceItems,
    ProxyItems
}
public class ShellNodeConfigSectionPointer : GenericReferenceItem<ShellNodeConfigSection>, IShellNodeConfigItem
{
    [InspectorProperty, JsonProperty]
    public int Row { get; set; }
    [InspectorProperty, JsonProperty]
    public int Column { get; set; }
    [InspectorProperty, JsonProperty]
    public SectionVisibility Visibility { get; set; }

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
    [InspectorProperty, JsonProperty]
    public int Row { get; set; }
    [InspectorProperty, JsonProperty]
    public int Column { get; set; }
    [InspectorProperty, JsonProperty]
    public SectionVisibility Visibility { get; set; }
    public string ClassName
    {
        get { return this.SourceItem.TypeName; }
    }
}
public class ShellNodeConfigOutputPointer : GenericReferenceItem<ShellNodeConfigOutput>
{
    [InspectorProperty, JsonProperty]
    public int Row { get; set; }
    [InspectorProperty, JsonProperty]
    public int Column { get; set; }
    [InspectorProperty, JsonProperty]
    public SectionVisibility Visibility { get; set; }
    public string ClassName
    {
        get { return this.SourceItem.TypeName; }
    }
}
public class ShellNodeConfigInput : ShellNodeConfigItem, IShellSlotType
{
    public bool IsOutput
    {
        get { return false; }
        set
        {

        }
    }

    [JsonProperty, InspectorProperty]
    public bool AllowMultiple { get; set; }
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
    public bool AllowMultiple { get; set; }
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
            () => { InvertGraphEditor.ExecuteCommand(_ => { addItem(); }); });
        ctxMenu.AddSeparator("");
        var nodeConfigSection =
            NodeViewModel.DiagramViewModel.CurrentRepository.AllGraphItems.OfType<TItem>();
        foreach (var item in nodeConfigSection)
        {
            ctxMenu.AddItem(new GUIContent(item.Name), false,
                () => { InvertGraphEditor.ExecuteCommand(_ => { addPointer(item); }); });
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
            });
        }
    }

    public void DrawInspector(IPlatformDrawer platformDrawer)
    {
#if UNITY_DLL
       
#endif
    }
}
