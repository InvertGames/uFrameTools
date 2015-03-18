using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.Core.GraphDesigner
{
    public class PopToFilterCommand : ElementsDiagramToolbarCommand, IDynamicOptionsCommand
    {

        public override void Perform(DiagramViewModel node)
        {
            node.NothingSelected();
            node.DiagramData.PopToFilter(SelectedOption.Name);
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
                Name = item.DiagramData.RootFilter.Name, 
                Checked = item.DiagramData.CurrentFilter == item.DiagramData.RootFilter
            };
            foreach (var filter in item.DiagramData.GetFilterPath())
            {
                yield return new UFContextMenuItem()
                {
                    Name = filter.Name,
                    Checked = item.DiagramData.CurrentFilter == filter
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
              
                }, project.Name);

                contextMenu.AddCommand(command);
                //menu.AddItem(new GUIContent(project.Name), project1 == CurrentProject, () =>
                //{
                //    CurrentProject = project1;
                //    LoadDiagram(CurrentProject.CurrentGraph);
                //});
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
            foreach (var item in projectService.CurrentProject.Graphs)
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
            foreach (var graphType in InvertGraphEditor.Container.Mappings.Where(p => p.From == typeof(IGraphData)))
            {
                TypeMapping type = graphType;
                contextMenu.AddCommand(new SimpleEditorCommand<DesignerWindow>(_ =>
                {
                    var diagram = projectService.CurrentProject.CreateNewDiagram(type.To);
                    node.SwitchDiagram(diagram);
                }, "Create " + type.To.Name,"Create"));
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