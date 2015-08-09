using System;
using System.Collections.Generic;
using System.Data;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.IOC;
using UnityEditor;

namespace Invert.Core.GraphDesigner
{ 
    public interface ICommandUI
    {
        void AddCommand(IEditorCommand command);
        void Go();
        ICommandHandler Handler { get; set; }
        void GoBottom();
    }

    public class ContextMenus : DiagramPlugin, 
        IShowContextMenu, 
        IContextMenuQuery, 
        IExecuteCommand<CreateNodeCommand>
    {
        public void Show(MouseEvent evt, params object[] objects)
        {
            var ui = InvertApplication.Container.Resolve<ContextMenuUI>() as ICommandUI;
            ui.Handler = InvertGraphEditor.DesignerWindow;
            foreach (var item in objects)
            {
                var item1 = item;
                Signal<IContextMenuQuery>(_ => _.QueryContextMenu(ui, item1));
            }
            ui.Go();

         
        }
        public void Execute<TCommand>(TCommand command) where TCommand : IExecuteCommand<TCommand>, ICommand
        {
            InvertApplication.SignalEvent<IExecuteCommand<TCommand>>(_=>_.Execute(command));
        }
        public void QueryContextMenu(ICommandUI ui, object obj)
        {
            var diagramNode = obj as DiagramNodeViewModel;
            if (diagramNode != null)
            {
                ui.AddCommand(new RenameCommand());
                ui.AddCommand(new DeleteCommand());
                ui.AddCommand(new HideCommand());
                ui.AddCommand(new ShowItemCommand());
            }
            var diagram = obj as DiagramViewModel;
            if (diagram != null)
            {
                var filter = diagram.GraphData.CurrentFilter;
                foreach (var nodeType in FilterExtensions.AllowedFilterNodes[filter.GetType()])
                {
                    if (nodeType.IsAbstract) continue;
                       Execute(new CreateNodeCommand()
                        {
                            NodeType = nodeType,
                            DiagramViewModel = diagram,
                        });
                   // ui.AddCommand();
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
    }

   
    public interface IExecuteCommand<TCommandType> where TCommandType : ICommand
    {
        void Execute(TCommandType command);
    }

    public interface ICommand
    {
        
    }
    public class CreateNodeCommand : ICommand
    {
        public Type NodeType { get; set; }
        public DiagramViewModel DiagramViewModel { get; set; }
    }
    public interface IShowContextMenu
    {
        void Show(MouseEvent evt, params object[] objects);
    }
    public interface IContextMenuQuery
    {
        void QueryContextMenu(ICommandUI ui, object obj);
    }
    public interface IToolbarQuery
    {
        void QueryToolbarCommands(ICommandUI ui);
    }
}