﻿//using System.CodeDom;
//using System.Collections.Generic;
//using Invert.uFrame;
//using Invert.uFrame.Editor;
//using Invert.uFrame.Editor.ViewModels;


using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Invert.Common;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class ComputedPropertyData : DiagramNode, ITypeDiagramItem
{
    public string NameAsChangedMethod
    {
        get { return string.Format("{0}Changed", Name); }
    }

    public string ViewFieldName
    {
        get { return string.Format("_{0}", Name); }
    }

    private List<string> _dependantPropertyIdentifiers = new List<string>();
    private string _type;

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get { yield break; }
    }

    public IEnumerable<ElementData> DependantNodes
    {
        get
        {
            foreach (var property in DependantProperties)
            {
                var relateType = property.RelatedNode() as ElementData;
                if (relateType == null) continue;
                yield return relateType;
            }
        }
    }

    public IEnumerable<ITypeDiagramItem> DependantSubProperties
    {
        get
        {
            foreach (var node in DependantNodes)
            {
                foreach (var item in node.AllProperties)
                {
                    if (this[item.Identifier])
                    {
                        yield return item;
                    }
                }
            }
        }
    }

    public List<string> DependantPropertyIdentifiers
    {
        get { return _dependantPropertyIdentifiers; }
        set { _dependantPropertyIdentifiers = value; }
    }

    public IEnumerable<ITypeDiagramItem> DependantProperties
    {
        get
        {
            var properties =
                Node.Project.GetElements()
                    .SelectMany(
                        p => p.Properties.Cast<ITypeDiagramItem>().Concat(p.ComputedProperties.Cast<ITypeDiagramItem>()))
                    .ToArray();
            foreach (var property in DependantPropertyIdentifiers)
            {
                var result = properties.FirstOrDefault(p => p.Identifier == property);
                if (result != null)
                    yield return result;
            }

        }
    }

    public override void NodeRemoved(IDiagramNode enumData)
    {
        base.NodeRemoved(enumData);

    }

    public override string Label
    {
        get { return this.Name; }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { yield break; }
        set { }
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Project.RemoveNode(this);
    }

    public string NameAsComputeMethod
    {
        get { return string.Format("Compute{0}", Name); }
    }

    public string PropertyIdentifier { get; set; }

    public string RelatedType
    {
        get { return _type ?? (_type = typeof(bool).Name); }
        set { _type = value; }
    }

    public string RelatedTypeName
    {
        get
        {
            var relatedNode = this.RelatedNode();
            if (relatedNode != null)
            {
                var element = relatedNode as ElementData;
                if (element != null)
                    return element.NameAsViewModel;

                return relatedNode.Name;
            }


            return RelatedType;
        }
    }

    public string RelatedTypeNameOrViewModel
    {
        get
        {
            var relatedNode = this.RelatedNode();
            if (relatedNode != null)
            {
                var element = relatedNode as ElementData;
                if (element != null)
                    return element.NameAsViewModel;

                return relatedNode.Name;
            }


            return RelatedType;
        }
    }

    public bool AllowEmptyRelatedType
    {
        get { return false; }
    }

    public string FieldName
    {
        get { return string.Format("_{0}Property", Name); }
    }


    public void SetType(IDesignerType input)
    {
        RelatedType = input.Identifier;
    }

    public void RemoveType()
    {
        RelatedType = null;
    }

    public CodeTypeReference GetFieldType()
    {
        var t = new CodeTypeReference(uFrameEditor.UFrameTypes.P);
        t.TypeArguments.Add(new CodeTypeReference(RelatedTypeName));
        return t;
    }

    public CodeTypeReference GetPropertyType()
    {
        var relatedNode = this.RelatedNode();
        if (relatedNode != null)
        {
            return relatedNode.GetPropertyType(this);
        }
        return new CodeTypeReference(RelatedTypeName);
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.AddPrimitiveArray("DependantOn", _dependantPropertyIdentifiers, i => new JSONData(i));
        cls.Add("RelatedType", new JSONData(RelatedType));
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["RelatedType"] != null)
            RelatedType = cls["RelatedType"].Value;
        if (cls["DependantOn"] != null)
        {
            _dependantPropertyIdentifiers = cls["DependantOn"].DeserializePrimitiveArray(n => n.Value).ToList();
        }
    }

    public override void NodeItemRemoved(IDiagramNodeItem item)
    {
        DependantPropertyIdentifiers.RemoveAll(p => p == item.Identifier);
        this[item.Identifier] = false;
    }

    public ElementData Element
    {
        get
        {
            var dependantProperty = DependantProperties.FirstOrDefault();
            if (dependantProperty == null)
            {
                return null;
            }
            return dependantProperty.Node as ElementData;
        }
    }
}
public class ComputedPropertyDrawer : DiagramNodeDrawer<ComputedPropertyNodeViewModel>
{
    private string _cachedRelatedType;
    private float _typeWidth;

    //public override float Padding
    //{
    //    get { return 12; }
    //}

    public override float HeaderPadding
    {
        get { return 6; }
    }

    public ComputedPropertyDrawer()
    {
    }

    public ComputedPropertyDrawer(ComputedPropertyNodeViewModel viewModel)
        : base(viewModel)
    {
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader10; }
    }

    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
        _cachedRelatedType = NodeViewModel.RelatedTypeName;

        _typeWidth = ElementDesignerStyles.Tag1.CalcSize(new GUIContent(_cachedRelatedType)).x;
    }

    public override void Draw(float scale)
    {
        base.Draw(scale);

        if (GUI.Button(new Rect(Bounds.x + 5, Bounds.y - 18f, _typeWidth, 15f).Scale(Scale),
            _cachedRelatedType,
            ElementDesignerStyles.Tag2))
        {
            var commandName = ViewModelObject.DataObject.GetType().Name.Replace("Data", "") + "TypeSelection";
            Debug.Log(commandName);
            var command = uFrameEditor.Container.Resolve<IEditorCommand>(commandName);
            ViewModel.Select();
            uFrameEditor.ExecuteCommand(command);
            //ElementItemTypesWindow.InitTypeListWindow("Choose Type",);
        }
    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
        foreach (var item in NodeViewModel.DependantNodes)
        {
            ElementData item1 = item;
            drawers.Add(new NodeItemHeader(NodeViewModel)
            {
                Label = item.Name,
                AddCommand = new SimpleEditorCommand<ComputedPropertyNodeViewModel>(node =>
                {
                    ItemSelectionWindow.Init("Add Child Dependants", item1.AllProperties.Cast<IItem>().ToArray(),
                        (i) =>
                        {
                            node.AddChildDependant(i as IDiagramNodeItem);
                        });
                })
            });

            foreach (var property in item.AllProperties)
            {
                if (NodeViewModel.GraphItem[property.Identifier])
                {
                    ViewModelPropertyData property1 = property;
                    drawers.Add(new ItemDrawer(new ComputedPropertySubItemViewModel(NodeViewModel)
                    {
                        Data = property,
                        RemoveItemCommand = new SimpleEditorCommand<DiagramViewModel>(
                            d =>
                            {
                                NodeViewModel.RemoveChildDependant(property1);
                            })
                    }));
                }
            }
        }
    }
}

public class ComputedPropertySubItemViewModel : ItemViewModel<IDiagramNodeItem>
{
    public ComputedPropertySubItemViewModel(DiagramNodeViewModel nodeViewModel)
        : base(nodeViewModel)
    {
    }

    public override string Name
    {
        get { return Data.Name; }
    }

}
public class ComputedPropertyNodeViewModel : DiagramNodeViewModel<ComputedPropertyData>
{
    public ComputedPropertyNodeViewModel(ComputedPropertyData graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {
    }

    public IEnumerable<ElementData> DependantNodes
    {
        get { return GraphItem.DependantNodes; }
    }

    public string RelatedTypeName
    {
        get
        {
            var relatedNode = GraphItem.RelatedNode();
            if (relatedNode != null)
            {
                return relatedNode.Name;
            }
            return GraphItem.RelatedType;
        }
    }

    public void AddChildDependant(IDiagramNodeItem diagramNodeItem)
    {
        GraphItem[diagramNodeItem.Identifier] = true;
    }

    public void RemoveChildDependant(IDiagramNodeItem diagramNodeItem)
    {
        GraphItem[diagramNodeItem.Identifier] = false;
    }
}


public class ComputedPropertyInputsConnectionStrategy :
       DefaultConnectionStrategy<ITypeDiagramItem, ComputedPropertyData>
{
    public override Color ConnectionColor
    {
        get { return Color.white; }
    }

    protected override bool CanConnect(ITypeDiagramItem output, ComputedPropertyData input)
    {
        if (output.Identifier == input.Identifier) return false;
        //if (!(output is ComputedPropertyData) && !(output is ViewPropertyData)) return false;
        return base.CanConnect(output, input);
    }

    protected override bool IsConnected(ITypeDiagramItem outputData, ComputedPropertyData inputData)
    {
        return inputData.DependantPropertyIdentifiers.Contains(outputData.Identifier);
    }

    protected override void ApplyConnection(ITypeDiagramItem output, ComputedPropertyData input)
    {
        input.DependantPropertyIdentifiers.Add(output.Identifier);
    }

    protected override void RemoveConnection(ITypeDiagramItem output, ComputedPropertyData input)
    {
        input.DependantPropertyIdentifiers.Remove(output.Identifier);
    }
}
