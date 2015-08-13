using System;
using System.Collections.Generic;
using System.Data;
using Invert.Core.GraphDesigner;
using Invert.Data;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{ 
    public interface ICommandUI
    {
        void AddCommand(ICommand command);
        void Go();
        ICommandHandler Handler { get; set; }
        void GoBottom();
    }

    public class ContextMenus : DiagramPlugin, 
        IShowContextMenu
    {
        public void Show(MouseEvent evt, params object[] objects)
        {
            var ui = InvertApplication.Container.Resolve<ContextMenuUI>();
            
            foreach (var item in objects)
            {
                var item1 = item;
                Signal<IContextMenuQuery>(_ => _.QueryContextMenu(ui,evt, item1));
            }
            ui.Go();
        }
    }

    public class Command : ICommand
    {
        public string Title { get; set; }
    }
    public class CreateNodeCommand : Command
    {
        public Type NodeType { get; set; }
        public DiagramViewModel DiagramViewModel { get; set; }
    }
    
    public class RenameCommand : Command
    {
        public DiagramNodeViewModel ViewModel { get; set; }
    }
     public class DeleteCommand : Command
    {
        public Invert.Data.IDataRecord Item { get; set; }
    }

    public class HideCommand : Command
    {
        public IDiagramNode Node { get; set; }
        public IDiagramFilter Filter { get; set; }
    }

    public class ShowCommand : Command
    {
        public IDiagramNode Node { get; set; }
        public IDiagramFilter Filter { get; set; }
        public Vector2 Position { get; set; }
    }
    public interface IShowContextMenu
    {
        void Show(MouseEvent evt, params object[] objects);
    }
    public interface IContextMenuQuery
    {
        void QueryContextMenu(ContextMenuUI ui, MouseEvent evt, object obj);
    }
    public interface IToolbarQuery
    {
        void QueryToolbarCommands(ToolbarUI ui);
    }
}