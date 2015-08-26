using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Invert.Data;
using Invert.IOC;

namespace Invert.Core.GraphDesigner {
    public class NavigateToNodeCommand : Command
    {
        public IDiagramNode Node;
    }
    public class NavigationSystem : DiagramPlugin
        , IExecuteCommand<NavigateToNodeCommand>

    {
        public void Execute(NavigateToNodeCommand nodeCommand)
        {
            var graph = nodeCommand.Node.Graph;

            var workspace = WorkspaceService.Workspaces.FirstOrDefault(p => p.Graphs.Any(x => x.Identifier == graph.Identifier));
            WorkspaceService.Execute(new OpenWorkspaceCommand()
            {
                Workspace = workspace
            });
            WorkspaceService.CurrentWorkspace.CurrentGraphId = graph.Identifier;
            var filterPath = nodeCommand.Node.FilterPath().ToArray();
            foreach (var item in filterPath)
            {
                InvertApplication.Log(item.Name);
            }
            InvertApplication.Log("Popping");
            graph.PopToFilter(graph.RootFilter);
        }

        public override void Loaded(UFrameContainer container)
        {
            base.Loaded(container);
            WorkspaceService = container.Resolve<WorkspaceService>();
            GraphSystem = container.Resolve<GraphSystem>();
            Repository = container.Resolve<IRepository>();
        }

        public IRepository Repository { get; set; }

        public GraphSystem GraphSystem { get; set; }

        public WorkspaceService WorkspaceService { get; set; }
    }
}
