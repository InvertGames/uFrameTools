//using System.CodeDom;
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
        get { return string.Format("_{0}",Name); }
    }

    private List<string> _dependantPropertyIdentifiers = new List<string>();
    private string _type;

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get { yield break; }
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
            var properties = Node.Data.GetElements().SelectMany(p => p.Properties.Cast<ITypeDiagramItem>().Concat(p.ComputedProperties.Cast<ITypeDiagramItem>())).ToArray();
            foreach (var property in DependantPropertyIdentifiers)
            {
                var result = properties.FirstOrDefault(p => p.Identifier == property);
                if (result != null)
                    yield return result;
            }

        }
    }
    public override string Label
    {
        get { return this.Name; }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get
        {
            yield break;
        }
        set { }
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Data.RemoveNode(this);
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
                return relatedNode.Name;

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
        get
        {
            return string.Format("_{0}Property", Name);
        }
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
        var t = new CodeTypeReference(uFrameEditor.UFrameTypes.Computed);
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

    public string Title
    {
        get { return Name; }
    }

    public string SearchTag
    {
        get { return Name; }
    }
}

public class ComputedPropertyNodeViewModel : DiagramNodeViewModel<ComputedPropertyData>
{
    public ComputedPropertyNodeViewModel(ComputedPropertyData graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {
    }

    public string RelatedTypeName
    {
        get { return GraphItem.RelatedTypeName; }
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
}


public class ComputedPropertyPlugin : DiagramPlugin
{
    public override void Initialize(uFrameContainer container)
    {
        uFrameEditor.RegisterGraphItem<ComputedPropertyData, ComputedPropertyNodeViewModel, ComputedPropertyDrawer>();
        uFrameEditor.RegisterFilterNode<ElementData, ComputedPropertyData>();

        container.RegisterInstance<IConnectionStrategy>(new ComputedPropertyInputsConnectionStrategy(), "ComputedPropertyInputsConnectionStrategy");
        //container.Register<DesignerGeneratorFactory, ComputedPopertyCodeFactory>("ComputedPropertyFactory");
    }
}

public class ComputedPropertyInputsConnectionStrategy :
    DefaultConnectionStrategy<ITypeDiagramItem, ComputedPropertyData>
{
    public override Color ConnectionColor
    {
        get { return Color.white; }
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


//public class ComputedPopertyCodeFactory : DesignerGeneratorFactory<ComputedPropertyData>
//{
//    public override IEnumerable<CodeGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
//        ComputedPropertyData item)
//    {
//        yield return new ComputedPropertyClassGenerator()
//        {
//            Data = item,
//            IsDesignerFile = true,
//            Filename = diagramData.Name + "ComputedProperties.designer.cs"
//        };

//    }
//}

//public class ComputedPropertyClassGenerator : CodeGenerator
//{
//    public ComputedPropertyData Data { get; set; }

//    public override void Initialize(CodeFileGenerator fileGenerator)
//    {
//        base.Initialize(fileGenerator);
//        var decleration = new CodeTypeDeclaration(Data.Name + "ComputedPropertyClass");

//        Namespace.Types.Add(decleration);
//    }
//}