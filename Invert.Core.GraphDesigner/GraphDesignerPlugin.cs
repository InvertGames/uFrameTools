using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions; 
using UnityEngine;
#if !UNITY_DLL
using KeyCode = System.Windows.Forms.Keys;
#endif
namespace Invert.Core.GraphDesigner
{
    public class GraphDesignerPlugin : DiagramPlugin, IPrefabNodeProvider, ICommandEvents, IConnectionEvents
    {
        public override decimal LoadPriority
        {
            get { return -100; }
        }
        public override bool Required
        {
            get { return true; }
        }

        public override void Initialize(uFrameContainer container)
        {
            ListenFor<ICommandEvents>();
            ListenFor<IConnectionEvents>();
//#if UNITY_DLL
        
//            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
//            {
//                InvertApplication.CachedAssemblies.Add(assembly);
//            }
//#endif
            var typeContainer = InvertGraphEditor.TypesContainer;
            // Drawers
            container.Register<DiagramViewModel,DiagramViewModel>();
            container.RegisterDrawer<PropertyFieldViewModel, PropertyFieldDrawer>();
            container.Register<SectionHeaderDrawer, SectionHeaderDrawer>();
            container.RegisterItemDrawer<GenericItemHeaderViewModel, GenericChildItemHeaderDrawer>();
             
            container.RegisterDrawer<SectionHeaderViewModel, SectionHeaderDrawer>();
            container.RegisterDrawer<ConnectorViewModel, ConnectorDrawer>();
            container.RegisterDrawer<ConnectionViewModel, ConnectionDrawer>();
            container.RegisterDrawer<InputOutputViewModel, SlotDrawer>();
            container.RegisterDrawer<DiagramViewModel, DiagramDrawer>();
            //typeContainer.AddItem<GenericSlot,InputOutputViewModel,SlotDrawer>();
            //typeContainer.AddItem<BaseClassReference, InputOutputViewModel, SlotDrawer>();

            container.RegisterInstance<IConnectionStrategy>(new InputOutputStrategy(),"InputOutputStrategy");
            //container.RegisterConnectable<GenericTypedChildItem, IClassTypeNode>();
            container.RegisterConnectable<GenericInheritableNode, GenericInheritableNode>();
            container.RegisterInstance<IConnectionStrategy>(new TypedItemConnectionStrategy(), "TypedConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new RegisteredConnectionStrategy(),"RegisteredConnectablesStrategy");

            container.AddNode<EnumNode>("Enum")
                .AddCodeTemplate<EnumNodeGenerator>();
            container.AddItem<EnumChildItem>();

            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(int), Group = "", Label = "int", IsPrimitive = true }, "int");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(string), Group = "", Label = "string", IsPrimitive = true }, "string");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(decimal), Group = "", Label = "decimal", IsPrimitive = true }, "decimal");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(float), Group = "", Label = "float", IsPrimitive = true }, "float");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(bool), Group = "", Label = "bool", IsPrimitive = true }, "bool");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(char), Group = "", Label = "char", IsPrimitive = true }, "char");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(DateTime), Group = "", Label = "date", IsPrimitive = true }, "date");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(Vector2), Group = "", Label = "Vector2", IsPrimitive = true }, "Vector2");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(Vector3), Group = "", Label = "Vector3", IsPrimitive = true }, "Vector3");
   
            container.Register<DesignerGeneratorFactory, RegisteredTemplateGeneratorsFactory>("TemplateGenerators");
            
#if UNITY_DLL        
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(Quaternion), Group = "", Label = "Quaternion", IsPrimitive = true }, "Quaternion");
            container.Register<DesignerGeneratorFactory, Invert.uFrame.CodeGen.ClassNodeGenerators.SimpleClassNodeCodeFactory>("ClassNodeData");
            
            // Enums
            container.RegisterGraphItem<EnumData, EnumNodeViewModel>();
            container.RegisterChildGraphItem<EnumItem, EnumItemViewModel>();
            //container.RegisterInstance(new AddEnumItemCommand());

            // Class Nodes
            container.RegisterGraphItem<ClassPropertyData, ClassPropertyItemViewModel>();
            container.RegisterGraphItem<ClassCollectionData, ClassCollectionItemViewModel>();
            container.RegisterGraphItem<ClassNodeData, ClassNodeViewModel>();
            

#endif
            // Register the container itself
            container.RegisterInstance<IUFrameContainer>(container);
            container.RegisterInstance<uFrameContainer>(container);

            container.AddNode<TypeReferenceNode, TypeReferenceNodeViewModel, TypeReferenceNodeDrawer>("Type Reference");

            // Toolbar commands
            container.RegisterInstance<IToolbarCommand>(new SelectProjectCommand(), "SelectProjectCommand");
            container.RegisterInstance<IToolbarCommand>(new SelectDiagramCommand(), "SelectDiagramCommand");
            container.RegisterInstance<IToolbarCommand>(new PopToFilterCommand(), "PopToFilterCommand");
            container.RegisterInstance<IToolbarCommand>(new SaveCommand(), "SaveCommand");
            container.RegisterToolbarCommand<HelpCommand>();
            container.RegisterNodeCommand<NodeHelpCommand>();
            //container.RegisterInstance<IToolbarCommand>(new AddNewCommand(), "AddNewCommand");
            

            // For no selection diagram context menu
            container.RegisterGraphCommand<AddNodeToGraph>();
            container.RegisterGraphCommand<AddReferenceNode>();
            container.RegisterGraphCommand<ShowItemCommand>();
            container.RegisterNodeCommand<PullFromCommand>();
            //container.RegisterNodeCommand<ToExternalGraph>();
            container.RegisterNodeCommand<OpenCommand>();
            container.RegisterNodeCommand<DeleteCommand>();
            container.RegisterNodeCommand<RenameCommand>();
            container.RegisterNodeCommand<HideCommand>();
            //container.RegisterNodeCommand<RemoveLinkCommand>();

            container.RegisterNodeItemCommand<DeleteItemCommand>();
            container.RegisterNodeItemCommand<MoveUpCommand>();
            container.RegisterNodeItemCommand<MoveDownCommand>();

            container.RegisterInstance<IEditorCommand>(new RemoveNodeItemCommand(), "RemoveNodeItem");

            container.RegisterKeyBinding(new RenameCommand(), "Rename", KeyCode.F2);
            container.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                p.DeselectAll();
            }), "End All Editing", KeyCode.Return);
      
            container.RegisterKeyBinding(new DeleteItemCommand(), "Delete Item", KeyCode.X, true);
            container.RegisterKeyBinding(new DeleteCommand(), "Delete", KeyCode.Delete);
#if UNITY_DLL
            container.RegisterKeyBinding(new MoveUpCommand(), "Move Up", KeyCode.UpArrow);
            container.RegisterKeyBinding(new MoveDownCommand(), "Move Down", KeyCode.DownArrow);
#endif


            container.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                InvertGraphEditor.Settings.ShowHelp = !InvertGraphEditor.Settings.ShowHelp;
            }), "Show/Hide This Help", KeyCode.F1);
#if DEBUG
            container.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                InvertGraphEditor.Settings.ShowGraphDebug = !InvertGraphEditor.Settings.ShowGraphDebug;
            }), "Show/Hide Debug", KeyCode.F3);
#endif
            container.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                var saveCommand = InvertApplication.Container.Resolve<IToolbarCommand>("Save");
                InvertGraphEditor.ExecuteCommand(saveCommand);
            }), "Save & Compile", KeyCode.S, true, true);

          
        }

        public override void Loaded(uFrameContainer container)
        {
            InvertGraphEditor.DesignerPluginLoaded();
        }

        public IEnumerable<QuickAddItem> PrefabNodes(INodeRepository nodeRepository)
        {
            return nodeRepository.GetImportableItems(nodeRepository.CurrentFilter).OfType<DiagramNode>().Select(p=>new QuickAddItem("Show Item",p.Name,
                _ =>
                {
                    nodeRepository.SetItemLocation(p, _.MousePosition);
                }));
        }

        public void CommandExecuting(ICommandHandler handler, IEditorCommand command, object o)
        {
            
        }

        public void CommandExecuted(ICommandHandler handler, IEditorCommand command, object o)
        {
            var item = o as IDiagramNodeItem;
            if (item != null)
            {
                var projectService = InvertApplication.Container.Resolve<ProjectService>();
                foreach (var graph in projectService.CurrentProject.Graphs)
                {
                    if (graph.Identifier == item.Node.Graph.Identifier)
                    {
                        UnityEditor.EditorUtility.SetDirty(graph as UnityEngine.Object);
                    }
                }
            }
        }

        public void ConnectionApplying(IGraphData graph, IConnectable output, IConnectable input)
        {
            
        }

        public void ConnectionApplied(IGraphData g, IConnectable output, IConnectable input)
        {
            var projectService = InvertApplication.Container.Resolve<ProjectService>();
            foreach (var graph in projectService.CurrentProject.Graphs)
            {
                if (graph.Identifier == g.Identifier)
                {
                    UnityEditor.EditorUtility.SetDirty(graph as UnityEngine.Object);
                }
            }
        }
    }
    public interface IDocumentationProvider
    {
        void GetDocumentation(IDocumentationBuilder node);
        void GetPages(List<DocumentationPage> rootPages);

    }

    public abstract class DocumentationPage
    {
        public virtual decimal Order
        {
            get { return 0; }
        }

        public virtual bool ShowInNavigation
        {
            get { return true; }
        }
        public virtual IEnumerable<ScaffoldGraph> Scaffolds()
        {
            yield break;
        }
        private static ProjectService _projectService;
        public static ProjectService ProjectService
        {
            get { return _projectService ?? (_projectService = InvertApplication.Container.Resolve<ProjectService>()); }
            set { _projectService = value; }
        }

        private List<DocumentationPage> _childPages;
        private string _name;

        public virtual string Name
        {
            get
            {
                return AddSpacesToSentence(this.GetType().Name, true);
            }
     
        }
        string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
        public List<DocumentationPage> ChildPages
        {
            get { return _childPages ?? (_childPages = new List<DocumentationPage>()); }
            set { _childPages = value; }
        }

        public Action<IDocumentationBuilder> PageContent { get; set; }
        public virtual Type RelatedNodeType { get; set; }

        public virtual Type ParentPage
        {
            get { return null; }
        }


        public virtual void GetContent(IDocumentationBuilder _)
        {
            _.Title(Name);
        }

        public T DoNamedNodeStep<T>(IDocumentationBuilder builder, string requiredName, IDiagramFilter requiredFilter = null,
        Action<IDocumentationBuilder> stepContent = null) where T : GenericNode
        {
            
            T existing = null;
            if (ProjectService.CurrentProject == null || ProjectService.CurrentProject.CurrentGraph == null)
            {

            }
            else
            {
                existing = ProjectService.CurrentProject.NodeItems.OfType<T>().FirstOrDefault(p => p.Name == requiredName);
            }
            
            builder.ShowTutorialStep(new TutorialStep(string.Format("Create a '{0}' node with the name '{1}'", InvertApplication.Container.GetNodeConfig<T>().Name, requiredName), () =>
            {
                if (existing == null)
                {
                    if (requiredFilter != null)
                    {
                        if (ProjectService.CurrentProject.CurrentFilter != requiredFilter)
                        {
                            return string.Format("Double-click on the '{0}' Node.", requiredFilter.Name);
                        }
                    }

                    return "Node not created yet";
                }
                return null;
            })
            {
                StepContent = _ =>
                {
                    _.Paragraph("To create a node you need to right click on an empty space on the graph.");
                    _.Paragraph("Then choose Add {0}", typeof(T).Name.Replace("Node", ""));
                    if (stepContent != null)
                    {
                        stepContent(_);
                    }
                    _.Break();
                    _.ToggleContentByNode<T>(null);
                }
            });
            return existing;
        }

        public T DoNamedItemStep<T>(IDocumentationBuilder builder,
            string requiredName,
            IDiagramNode requiredNode,
            string singularItemTypeName,
            Action<IDocumentationBuilder> stepContent

            ) where T : class, IDiagramNodeItem
        {
            T existing = requiredNode == null ?  (T) null : requiredNode.PersistedItems.OfType<T>().FirstOrDefault(p => p.Name == requiredName);
            builder.ShowTutorialStep(new TutorialStep(string.Format("Create {0} with the name '{1}' on the '{2}' Node.", singularItemTypeName, requiredName, requiredNode == null ? "the node" : requiredNode.Name), () =>
            {
                if (existing == null)
                {
                    return "Item not created yet";
                }
                return null;
            })
            {
                StepContent = _ =>
                {
                    _.Paragraph("To create a node you need to right click on an empty space on the graph.");
                    _.Paragraph("Then choose Add {0}", typeof(T).Name.Replace("Node", ""));
                    _.Break();
                    _.ToggleContentByPage<T>(singularItemTypeName);

                    if (stepContent != null)
                    {
                        stepContent(_);
                    }
                }
            });
            return existing;
        }



        public TGraphType DoGraphStep<TGraphType>(IDocumentationBuilder builder, string name = null, Action<IDocumentationBuilder> stepContent = null) where TGraphType : class,IGraphData
        {
            var currentGraph =
                (ProjectService.CurrentProject == null || object.ReferenceEquals(ProjectService.CurrentProject, null))
                || (ProjectService.CurrentProject.CurrentGraph == null || object.ReferenceEquals(ProjectService.CurrentProject.CurrentGraph, null))
                ? null : (ProjectService.CurrentProject.Graphs.OfType<UnityGraphData>().Select(p=>p.Graph).OfType<TGraphType>().FirstOrDefault()) as TGraphType;

            builder.ShowTutorialStep(new TutorialStep(string.Format("Create a new {0} Graph", typeof(TGraphType).Name.Replace("Graph","")), () =>
            {
                if (currentGraph == null)
                {
                    return "Graph hasn't been created yet.";
                }
                else if (!string.IsNullOrEmpty(name) && currentGraph.RootFilter.Name != name)
                {
                    return string.Format("Rename it to '{0}'", name);
                }
                return null;
            })
            {
               StepContent = stepContent
            });
            return currentGraph;
        }

        public IProjectRepository DoCreateNewProjectStep(IDocumentationBuilder builder, Action<IDocumentationBuilder> stepContent = null)
        {
            var currentProject = ProjectService.CurrentProject;
            builder.ShowTutorialStep(new TutorialStep("Create A Project", () =>
            {
                if (currentProject == null || object.ReferenceEquals(currentProject, null))
                {
                    return "Project hasn't been created yet.";
                }
                return null;
            })
            {
                StepContent = _ =>
                {
                    if (stepContent != null)
                    {
                        stepContent(_);
                    }
                    _.Paragraph("You need to create a new project to begin this journey.");
                    _.Paragraph("Navigate to the project window and create an empty folder.");
                    _.Paragraph("From here right click on the folder and choose uFrame->New Project");
                    _.Paragraph("This will create a new project repository for you.  " +
                                "The project repository will hold references to all of your graphs.");
                    
                },
            });
            return currentProject;
        }
        public TutorialStep SaveAndCompile(DiagramNode node)
        {
            return new TutorialStep("Save & Compile the project.", () =>
            {
                if (InvertApplication.FindType(node.FullName) == null)
                {
                    return string.Format("Type {0} doesn't exist so you haven't save and compiled your project yet.", node.FullName);
                }
                return null;
            });
        }
        public string EnsureCodeInEditableFile(DiagramNode elementNode, string filenameSearchText, string codeSearchText)
        {
            var firstOrDefault = elementNode.GetCodeGeneratorsForNode()
                .FirstOrDefault(p => !p.AlwaysRegenerate && (p.Filename.Contains(filenameSearchText) || p.Filename == filenameSearchText));
            if (firstOrDefault == null || !File.Exists(firstOrDefault.FullPathName) || !File.ReadAllText(firstOrDefault.FullPathName).Contains(codeSearchText))
            {
                return "File not found, or you haven't implemented it properly.";
            }
            return null;
        }
        public ConnectionData DoCreateConnectionStep(IDocumentationBuilder builder, IDiagramNodeItem output, IDiagramNodeItem input, Action<IDocumentationBuilder> stepContent = null)
        {
     
            var inputName = input == null ? "A" :input.Name;
            var outputName = output == null ? "B" : output.Name;

            if (input != null && input != input.Node)
            {
                inputName = input.Node.Name + "'s " + input.Name + " input slot";
            }
            if (output != null && output != output.Node)
            {
                outputName = output.Node.Name + "'s " + output.Name + " output slot";
            }
            builder.ShowTutorialStep( new TutorialStep(string.Format("Create a connection from {0} to {1}.", outputName, inputName), () =>
            {
                var existing = output == null || input == null ? null :
                    ProjectService.CurrentProject.Connections.FirstOrDefault(p => p.OutputIdentifier == output.Identifier && p.InputIdentifier == input.Identifier);
                if (existing == null)
                {
                    var typedItem = output as ITypedItem;
                    if (typedItem != null && input != null)
                    {
                        if (typedItem.RelatedType == input.Identifier)
                        {
                            return null;
                        }
                    }
                    return "The connection hasen't been created yet.";
                }
                return null;
            })
            {
                StepContent = stepContent
            });
            return null;
        }

    }
    public class DocumentationDefaultProvider : IDocumentationProvider
    {
        private ProjectService _projectService;

        public ProjectService ProjectService
        {
            get { return _projectService ?? (_projectService = InvertApplication.Container.Resolve<ProjectService>()); }
            set { _projectService = value; }
        }

        public virtual Type RootPageType
        {
            get { return null; }
        }

        public virtual void GetPages(List<DocumentationPage> rootPages)
        {
            var pages = InvertApplication.GetDerivedTypes<DocumentationPage>(false, false)
                .Where(p=>RootPageType == null || RootPageType.IsAssignableFrom(p))
                .Where(p => p.Name != "DocumentationPageTemplate")
                .Select(Activator.CreateInstance).OfType<DocumentationPage>().ToArray();

            GetValue(pages, rootPages);
        }

        private static void GetValue(DocumentationPage[] allPages, List<DocumentationPage> pages, DocumentationPage parentPage = null)
        {
            foreach (var page in allPages)
            {
                //  foreach (var page in allPages
//                .Where(p => p.ParentPage == (parentPage == null ? null : parentPage.GetType())))
          
                if (parentPage != null && page.ParentPage != null && page.ParentPage.IsAssignableFrom(parentPage.GetType()))
                {
                    pages.Add(page);
                    GetValue(allPages, page.ChildPages, page);
                }
                else if (parentPage == null && page.ParentPage == null)
                {
                    pages.Add(page);
                    GetValue(allPages, page.ChildPages, page);
                }
            }
        }


        public virtual void GetDocumentation(IDocumentationBuilder node)
        {

        }
    }

    public class HelpCommand : ToolbarCommand<DiagramViewModel>
    {
        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.BottomRight; }
        }

        public override void Perform(DiagramViewModel node)
        {
            InvertGraphEditor.WindowManager.ShowHelpWindow(node.GraphData.RootFilter.GetType().Name,null);
        }

        public override string CanPerform(DiagramViewModel node)
        {
            if (node == null)
            {
                return "Show a diagram first.";
            }
            return null;
        }
    }
    public class NodeHelpCommand : EditorCommand<DiagramNode>, IDiagramNodeCommand
    {
        public override string Name
        {
            get { return "Help and Documentation"; }
        }

        public override string Path
        {
            get { return "Help and Documentation"; }
        }

        public override void Perform(DiagramNode node)
        {
            InvertGraphEditor.WindowManager.ShowHelpWindow(node.Project.CurrentGraph.RootFilter.GetType().Name, node.GetType());
        }

        public override string CanPerform(DiagramNode node)
        {
            if (node == null)
            {
                return "Show a diagram first.";
            }
            return null;
        }
    }
}
