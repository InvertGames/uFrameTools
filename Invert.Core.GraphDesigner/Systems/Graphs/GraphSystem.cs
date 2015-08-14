using System;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;

public class GraphSystem : DiagramPlugin
    , IContextMenuQuery
    , IToolbarQuery
    , IExecuteCommand<CreateGraphMenuCommand>
    , IExecuteCommand<CreateGraphCommand>
    , IExecuteCommand<AddGraphToWorkspace>
{
    public void QueryContextMenu(ContextMenuUI ui, MouseEvent evt, object obj)
    {
        if (obj is CreateGraphMenuCommand)
        {
            foreach (var item in Container.Mappings.Where(p=>p.From == typeof(IGraphData)))
            {
                ui.AddCommand(new ContextMenuItem()
                {
                    Title = item.Name,
                    Command = new CreateGraphCommand()
                    {
                        GraphType = item.To,
                        Name = "New" + item.To.Name
                    }
                });
            }
            
        }
        var diagram = obj as DiagramViewModel;
        if (diagram != null)
        {
            ui.AddCommand(new ContextMenuItem()
            {
                Title = "Delete This Graph",
                Group = "Remove",
                Command =  new LambdaCommand(() =>
                {
                    Container.Resolve<IRepository>().Remove(diagram.DataObject as IDataRecord);
                })
            });
        }
    }

    public void QueryToolbarCommands(ToolbarUI ui)
    {
        ui.AddCommand(new ToolbarItem()
        {
            Title = "Create Graph",
            Position = ToolbarPosition.BottomRight,
            Command = new CreateGraphMenuCommand(),
            Order = -1
        });

        ui.AddCommand(new ToolbarItem()
        {
            Title = "Import Graph",
            Position = ToolbarPosition.BottomRight,
            Command = new AddGraphToWorkspace()
        });
    }

    public void Execute(CreateGraphMenuCommand command)
    {
        Signal<IShowContextMenu>(_ => _.Show(null, command));
    }

    public void Execute(AddGraphToWorkspace command)
    {
        var workspaceService = Container.Resolve<WorkspaceService>();
        var repo = Container.Resolve<IRepository>();
        var workspaceGraphs = workspaceService.CurrentWorkspace.Graphs.Select(p => p.Identifier).ToArray();
        var importableGraphs = repo.AllOf<IGraphData>().Where(p => !workspaceGraphs.Contains(p.Identifier));
        InvertGraphEditor.WindowManager.InitItemWindow(importableGraphs, _ =>
        {
            workspaceService.CurrentWorkspace.AddGraph(_);
            repo.Commit();
        });
    }

    public void Execute(CreateGraphCommand command)
    {
        var workspaceService = Container.Resolve<WorkspaceService>();
        var repo = Container.Resolve<IRepository>();
        var graph = Activator.CreateInstance(command.GraphType) as IGraphData;
        repo.Add(graph);
        graph.Name = command.Name;
        workspaceService.CurrentWorkspace.AddGraph(graph);
        workspaceService.CurrentWorkspace.CurrentGraphId = graph.Identifier;
        repo.Commit();
    }
}
public class CreateGraphCommand : Command
{
    public string Name { get; set; }
    public Type GraphType { get; set; }
}
public class CreateGraphMenuCommand : Command
{

}

public class AddGraphToWorkspace : Command
{

}