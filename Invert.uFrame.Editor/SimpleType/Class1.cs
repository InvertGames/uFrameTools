using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Common;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;


public class ClassNodeData : DiagramNode, IDesignerType
{
    private List<IDiagramNodeItem> _nodeItems;
 

    public List<IDiagramNodeItem> NodeItems
    {
        get { return _nodeItems ?? (_nodeItems= new List<IDiagramNodeItem>()); }
        set { _nodeItems = value; }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get
        {
            return NodeItems;
        }
        set
        {
            NodeItems = value.ToList();
        }
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Project.RemoveNode(this);
    }

    public override string Label
    {
        get
        {
            return Name;
        }
    }

    public IEnumerable<ClassPropertyData> Properties
    {
        get
        {
            return NodeItems.OfType<ClassPropertyData>();
        }
    }
    public IEnumerable<ClassCollectionData> Collections
    {
        get
        {
            return NodeItems.OfType<ClassCollectionData>();
        }
    }
    public string BaseIdentifier { get; set; }

    public ClassNodeData BaseClass
    {
        get
        {
            return Project.NodeItems.OfType<ClassNodeData>().FirstOrDefault(p => p.Identifier == BaseIdentifier);
        }
    }

    public IEnumerable<ClassNodeData> DerivedElements
    {
        get
        {
            var derived = Project.NodeItems.OfType<ClassNodeData>().Where(p => p.BaseIdentifier == Identifier);
            foreach (var derivedItem in derived)
            {
                yield return derivedItem;
                foreach (var another in derivedItem.DerivedElements)
                {
                    yield return another;
                }
            }
        }
    }

    public void SetBaseClass(ClassNodeData output)
    {
        BaseIdentifier = output.Identifier;
    }

    public void RemoveBaseClass()
    {
        BaseIdentifier = null;
    }

    public override void NodeItemRemoved(IDiagramNodeItem diagramNodeItem)
    {
        base.NodeItemRemoved(diagramNodeItem);
        NodeItems.Remove(diagramNodeItem);
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        if (BaseIdentifier != null)
            cls.Add("BaseIdentifier", new JSONData(BaseIdentifier));
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["BaseIdentifier"] != null)
            BaseIdentifier = cls["BaseIdentifier"].Value;
    }
}

public class ClassNodeInheritanceConnectionStrategy : DefaultConnectionStrategy<ClassNodeData, ClassNodeData>
{
    public override Color ConnectionColor
    {
        get { return Color.green; }
    }
    
    protected override bool CanConnect(ClassNodeData output, ClassNodeData input)
    {
        if (output.Identifier == input.Identifier) return false;
        if (input.DerivedElements.Any(p => p.Identifier == output.Identifier)) return false;
        return base.CanConnect(output, input);
    }

    protected override bool IsConnected(ClassNodeData outputData, ClassNodeData inputData)
    {
        return inputData.BaseIdentifier == outputData.Identifier;
    }

    protected override void ApplyConnection(ClassNodeData output, ClassNodeData input)
    {
        input.SetBaseClass(output);
    }

    protected override void RemoveConnection(ClassNodeData output, ClassNodeData input)
    {
        input.RemoveBaseClass();
    }
}
public class ClassPropertyData : TypedDiagramNodeItem
{
    public override string FullLabel
    {
        get { return Name; }
    }

    public override string Label
    {
        get { return Name; }
    }

    public override void Remove(IDiagramNode diagramNode)
    {
        base.Remove(diagramNode);

    }
}
public class ClassCollectionData : TypedDiagramNodeItem
{
    public override string FullLabel
    {
        get { return Name; }
    }

    public override string Label
    {
        get { return Name; }
    }

    public override string RelatedTypeName
    {
        get
        {
            return base.RelatedTypeName;
        }
    }

    public override void Remove(IDiagramNode diagramNode)
    {
        base.Remove(diagramNode);

    }
}
public class ClassNodeViewModel : DiagramNodeViewModel<ClassNodeData>
{
    public ClassNodeViewModel(ClassNodeData graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
    {
    }

    public void AddProperty()
    {
        GraphItem.NodeItems.Add(new ClassPropertyData()
        {
            Name = DiagramViewModel.CurrentRepository.GetUniqueName("NewClassProperty"),
            Node = GraphItem,
            RelatedType = typeof(string).Name
            
        });
    }

    public void AddCollection()
    {
        GraphItem.NodeItems.Add(new ClassCollectionData()
        {
            Name = DiagramViewModel.CurrentRepository.GetUniqueName("NewClassCollection"),
            Node = GraphItem,
            RelatedType = typeof(string).Name

        });
    }
}

public class ClassPropertyItemViewModel : ElementItemViewModel<ClassPropertyData>
{

    public ClassPropertyItemViewModel(ClassPropertyData viewModelItem, DiagramNodeViewModel nodeViewModel) : base(viewModelItem, nodeViewModel)
    {
    }

    public override string TypeLabel
    {
        get { return ElementDataBase.TypeAlias(Data.RelatedTypeName); }
    }
}
public class ClassCollectionItemViewModel : ElementItemViewModel<ClassCollectionData>
{
    public ClassCollectionItemViewModel(ClassCollectionData viewModelItem, DiagramNodeViewModel nodeViewModel)
        : base(viewModelItem, nodeViewModel)
    {
    }

    public override string TypeLabel
    {
        get { return ElementDataBase.TypeAlias(Data.RelatedTypeName); }
    }
}

public class ClassNodeDrawer : DiagramNodeDrawer<ClassNodeViewModel>
{
    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader12; }
    }

    public ClassNodeDrawer(ClassNodeViewModel viewModelObject) : base(viewModelObject)
    {

    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        //base.GetContentDrawers(drawers);
     
        drawers.Add(new NodeItemHeader(this.ViewModel)
        {
            Label = "Properties",
            AddCommand = new SimpleEditorCommand<ClassNodeViewModel>(n =>
            {
                n.AddProperty();
            }),
        });
        foreach (var item in ViewModel.ContentItems.Where(p => p.DataObject is ClassPropertyData))
        {
            var drawer = uFrameEditor.CreateDrawer(item);
            if (drawer == null) Debug.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.", item.GetType().Name));
            drawers.Add(drawer);
        }
        drawers.Add(new NodeItemHeader(this.ViewModel)
        {
            Label = "Collections",
            AddCommand = new SimpleEditorCommand<ClassNodeViewModel>(n =>
            {
                n.AddCollection();
            }),
        });
        foreach (var item in ViewModel.ContentItems.Where(p=>p.DataObject is ClassCollectionData))
        {
            var drawer = uFrameEditor.CreateDrawer(item);
            if (drawer == null) Debug.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.", item.GetType().Name));
            drawers.Add(drawer);
        }
     
    }
}
