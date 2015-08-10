using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Data;

namespace Invert.Core.GraphDesigner
{
    public class NodeSystem : DiagramPlugin,
        IContextMenuQuery,
        IExecuteCommand<CreateNodeCommand>,
        IExecuteCommand<RenameCommand>,
        IExecuteCommand<DeleteCommand>,
        IExecuteCommand<ShowCommand>,
        IExecuteCommand<HideCommand>
    {

        public void QueryContextMenu(ContextMenuUI ui, MouseEvent evt, object obj)
        {
            var diagramNode = obj as DiagramNodeViewModel;
            if (diagramNode != null)
            {
                ui.AddCommand(new ContextMenuItem()
                {
                    Title = "Rename",
                    Command = new RenameCommand() { ViewModel = diagramNode }
                });
                ui.AddCommand(new ContextMenuItem()
                {
                    Title = "Hide",
                    Command = new HideCommand() { Node = diagramNode.GraphItemObject, Filter = diagramNode.DiagramViewModel.GraphData.CurrentFilter }
                });
                ui.AddCommand(new ContextMenuItem()
                {
                    Title = "Delete",
                    Command = new DeleteCommand() { Item = diagramNode.GraphItemObject as Invert.Data.IDataRecord }
                });
            }
            var diagram = obj as DiagramViewModel;
            if (diagram != null)
            {
                InvertApplication.Log("YUP YUP YUP");
                var filter = diagram.GraphData.CurrentFilter;
                foreach (var nodeType in FilterExtensions.AllowedFilterNodes[filter.GetType()])
                {
                    if (nodeType.IsAbstract) continue;
                    ui.AddCommand(new ContextMenuItem()
                    {
                        Command = new CreateNodeCommand()
                        {
                            NodeType = nodeType,
                            DiagramViewModel = diagram,
                        }
                    });
                }
                foreach (var item in filter.GetAllowedDiagramItems())
                {
                    ui.AddCommand(new ContextMenuItem()
                    {
                        Title = "Show/" + item.Name,
                        Command = new ShowCommand() { Node = item, Filter = filter, Position = evt.MousePosition }
                    });
                }
            }

        }


        public void Execute(CreateNodeCommand command)
        {

            var node = Activator.CreateInstance(command.NodeType) as IDiagramNode;
            var repository = Container.Resolve<IRepository>();
            repository.Add(node);
            command.DiagramViewModel.AddNode(node, command.DiagramViewModel.LastMouseEvent.MouseDownPosition);

        }

        public void Execute(RenameCommand command)
        {
            command.ViewModel.BeginEditing();
        }

        public void Execute(DeleteCommand command)
        {
            command.Item.Repository.Remove(command.Item);
        }

        public void Execute(ShowCommand command)
        {
            command.Filter.ShowInFilter(command.Node, command.Position);
        }

        public void Execute(HideCommand command)
        {
            command.Filter.HideInFilter(command.Node);
        }
    }
}
