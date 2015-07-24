using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner
{
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
            var message = string.Format("Create {0} with the name '{1}'", singularItemTypeName,
                requiredName);
            if (requiredNode != null)
            {
                message += string.Format(" on the '{0}' node", requiredNode.Name);
            }
            builder.ShowTutorialStep(new TutorialStep(message, () =>
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

            builder.ShowTutorialStep(new TutorialStep(string.Format("Create a new {0} Graph with the name '{1}'", typeof(TGraphType).Name.Replace("Graph",""),name ?? "ANYTHING"), () =>
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
                StepContent = b =>
                {
                    b.Paragraph("Graphs are used to create various sections of your project.  Graphs can also share nodes between each other as long as they belong to the same project.");
                    b.ImageByUrl("http://i.imgur.com/MqXxE6h.png");
                    if (stepContent != null)
                    {
                        stepContent(b);
                    }
                }
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
                if (currentProject.Name == "DefaultProject")
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
}