using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.Core.GraphDesigner
{
    public class PopToFilterCommand : ElementsDiagramToolbarCommand, IDynamicOptionsCommand
    {

        public override void Perform(DiagramViewModel node)
        {
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

    public class SelectProjectCommand : ToolbarCommand<DesignerWindow>
    {
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
                var command = new SimpleEditorCommand<DiagramViewModel>(_ =>
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
            contextMenu.AddCommand(new SimpleEditorCommand<DiagramViewModel>(_ =>
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

    public class SelectDiagramCommand : ToolbarCommand<DesignerWindow>
    {
        public override void Perform(DesignerWindow node)
        {
            var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
            var contextMenu = InvertApplication.Container.Resolve<ContextMenuUI>();
            contextMenu.Handler = node;
            foreach (var item in projectService.CurrentProject.Graphs)
            {
                IGraphData item1 = item;

                contextMenu.AddCommand(new SimpleEditorCommand<DiagramViewModel>(_ =>
                {
                    projectService.CurrentProject.CurrentGraph = item1;
                    node.SwitchDiagram(item1);
                }, item.Name));

            }
            contextMenu.AddSeparator("");
            foreach (var graphType in InvertGraphEditor.Container.Mappings.Where(p => p.From == typeof(IGraphData)))
            {
                TypeMapping type = graphType;
                contextMenu.AddCommand(new SimpleEditorCommand<DiagramViewModel>(_ =>
                {
                    var diagram = projectService.CurrentProject.CreateNewDiagram(type.To);
                    node.SwitchDiagram(diagram);
                }, "Create " + type.To.Name));
            }
            contextMenu.AddSeparator("");
            contextMenu.AddCommand(new SimpleEditorCommand<DiagramViewModel>(_ =>
            {
                projectService.CurrentProject.Refresh();
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

}