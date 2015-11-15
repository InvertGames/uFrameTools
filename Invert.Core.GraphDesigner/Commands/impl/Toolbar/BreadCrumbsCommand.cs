using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.IOC;

namespace Invert.Core.GraphDesigner
{
    public class BreadCrumbsCommand : ElementsDiagramToolbarCommand, IDynamicOptionsCommand
    {

        public override void Perform(DiagramViewModel node)
        {
            node.NothingSelected();
            node.GraphData.PopToFilter((string)SelectedOption.Value);
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object arg)
        {
            var item = arg as DiagramViewModel;
            if (item == null)
            {
                yield break;
            }
            
            yield return new UFContextMenuItem()
            {
                Name = item.GraphData.RootFilter.Name, 
                Value = item.GraphData.RootFilter.Identifier, 
                Checked = item.GraphData.CurrentFilter == item.GraphData.RootFilter
            };
            foreach (var filter in item.GraphData.GetFilterPath())
            {
                yield return new UFContextMenuItem()
                {
                    Name = filter.Name,
                    Value = filter.Identifier,
                    Checked = item.GraphData.CurrentFilter == filter
                };
            }
        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.Left; }
        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get{ return MultiOptionType.Buttons; } }
    }

    public class SelectProjectCommand : ToolbarCommand<DesignerWindow>, IDropDownCommand
    {
      

        public ProjectService ProjectService
        {
            get
            {
                return  InvertGraphEditor.Container.Resolve<ProjectService>();
            }
        }
        public override string Name
        {
            get
            {
                var ps = ProjectService;
                if (ps == null) return "Project: [None]";
                if (ps.CurrentProject != null && !object.ReferenceEquals(ps.CurrentProject, null))
                {
                    return "Project: " + ps.CurrentProject.Name;
                }
                return "Project: [None]";
            }
        }

        public override void Perform(DesignerWindow node)
        {
            var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
            var projects = projectService.Projects;
            var contextMenu = InvertApplication.Container.Resolve<ContextMenuUI>();
            contextMenu.Handler = node;
            //var menu = new GenericMenu();
            foreach (var project in projects)
            {
                
                IProjectRepository project1 = project;
                var command = new SimpleEditorCommand<DesignerWindow>(_ =>
                {
                    projectService.CurrentProject = project1;

                    node.Designer = null;
                    node.SwitchDiagram(project1.CurrentGraph);

                }, project.Name);

                contextMenu.AddCommand(command);
            }

            contextMenu.AddSeparator("");
            contextMenu.AddCommand(new SimpleEditorCommand<DesignerWindow>(_ =>
            {
                projectService.RefreshProjects();
            }, "Force Refresh"));
            contextMenu.Go();
        }
        public override string CanPerform(DesignerWindow node)
        {
            return null;
        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.Left; }
        }
    }

    public class SelectDiagramCommand : ToolbarCommand<DesignerWindow>, IDropDownCommand
    {
     
        public ProjectService ProjectService
        {
            get
            {
                return InvertGraphEditor.Container.Resolve<ProjectService>();
            }
        }

        public override string Name
        {
            get
            {
                var ps = ProjectService;
                if (ps == null || ps.CurrentProject == null || ps.CurrentProject.Equals(null) || ps.CurrentProject.CurrentGraph == null || ps.CurrentProject.CurrentGraph.Equals(null))
                {
                    return "Graph: [None]";
                }

                return string.Format("Graph: {0}", ps.CurrentProject.CurrentGraph.Name);
            }
        }

        public override void Perform(DesignerWindow node)
        {
            var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
            var contextMenu = InvertApplication.Container.Resolve<ContextMenuUI>();
            contextMenu.Handler = node;
            foreach (var item in projectService.CurrentProject.Graphs.OrderBy(p=>p.Name))
            {
                IGraphData item1 = item;

                var simpleEditorCommand = new SimpleEditorCommand<DesignerWindow>(_ =>
                {
                    projectService.CurrentProject.CurrentGraph = item1;
                    
                    node.SwitchDiagram(item1);
                }, item.Name, "Switch");
             
                contextMenu.AddCommand(simpleEditorCommand);

            }
            contextMenu.AddSeparator("");
            foreach (var graphType in InvertGraphEditor.Container.Mappings.Where(p => p.Key.Item1 == typeof(IGraphData)))
            {
                
                contextMenu.AddCommand(new SimpleEditorCommand<DesignerWindow>(_ =>
                {
                    var diagram = projectService.CurrentProject.CreateNewDiagram(graphType.Value);
                    node.SwitchDiagram(diagram);
                }, "Create " + graphType.Value.Name, "Create"));
            }
            contextMenu.AddSeparator("");
            contextMenu.AddCommand(new SimpleEditorCommand<DesignerWindow>(_ =>
            {
                projectService.CurrentProject.Refresh();
            }, "Force Refresh", "Refresh"));
            contextMenu.Go();
        }

        public override string CanPerform(DesignerWindow node)
        {
            if (node.CurrentProject == null)
                return "No project selected.";

            return null;
        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.Left; }
        }
    }

}