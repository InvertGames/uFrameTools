//using System.CodeDom;
//using System.Collections.Generic;
//using Invert.uFrame;
//using Invert.uFrame.Editor;
//using Invert.uFrame.Editor.ViewModels;


//public class ComputedPropertyData : DiagramNode
//{
//    private List<string> _dependantProperties = new List<string>();

//    public override IEnumerable<IDiagramNodeItem> Items
//    {
//        get { yield break; }
//    }

//    public List<string> DependantProperties
//    {
//        get { return _dependantProperties; }
//        set { _dependantProperties = value; }
//    }

//    public override string Label
//    {
//        get { return this.Name; }
//    }

//    public override IEnumerable<IDiagramNodeItem> ContainedItems
//    {
//        get
//        {
//            yield break;
//        }
//        set { }
//    }

//    public string PropertyIdentifier { get; set; }

//    public override void Serialize(JSONClass cls)
//    {
//        base.Serialize(cls);
//    }

//    public override void Deserialize(JSONClass cls, INodeRepository repository)
//    {
//        base.Deserialize(cls, repository);

//    }
//}

//public class ComputedPropertyNodeViewModel : DiagramNodeViewModel<ComputedPropertyData>
//{
//    public ComputedPropertyNodeViewModel(ComputedPropertyData graphItemObject, DiagramViewModel diagramViewModel)
//        : base(graphItemObject, diagramViewModel)
//    {
//    }
//}

//public class ComputedPropertyDrawer : DiagramNodeDrawer<ComputedPropertyNodeViewModel>
//{
//    public ComputedPropertyDrawer()
//    {
//    }

//    public ComputedPropertyDrawer(ComputedPropertyNodeViewModel viewModel)
//        : base(viewModel)
//    {
//    }
//}


//public class ComputedPropertyPlugin : DiagramPlugin
//{
//    public override void Initialize(uFrameContainer container)
//    {
//        uFrameEditor.RegisterGraphItem<ComputedPropertyData, ComputedPropertyNodeViewModel, ComputedPropertyDrawer>();
//        uFrameEditor.RegisterFilterNode<ElementData, ComputedPropertyData>();

//        container.Register<DesignerGeneratorFactory, ComputedPopertyCodeFactory>("ComputedPropertyFactory");
//    }
//}

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