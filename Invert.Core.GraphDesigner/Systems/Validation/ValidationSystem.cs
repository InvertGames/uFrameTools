﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Invert.Data;
using Invert.IOC;
using JetBrains.Annotations;

namespace Invert.Core.GraphDesigner
{
    public interface INodeValidated
    {
        void NodeValidated(IDiagramNode node);
    }
    
    public class ValidationSystem : DiagramPlugin, 
        IDataRecordPropertyChanged, 
        //ICommandProgressEvent,
        IExecuteCommand<ValidateDatabaseCommand>,
        IDataRecordInserted,
        ICommandExecuted,
        IQueryErrors
    {
        public void QueryErrors(List<ErrorInfo> errorInfo)
        {
            foreach (var item in ErrorNodes)
            {
                errorInfo.AddRange(item.Errors);
            }
        }
        public bool shouldRestart = false;
        private List<IDiagramNode> _itemsToValidate;
        private List<ErrorInfo> _errorInfo;
        private List<IDiagramNode> _errorNodes;

        public override void Loaded(UFrameContainer container)
        {
            base.Loaded(container);

            Signal<ITaskHandler>(_ => _.BeginBackgroundTask(ValidateDatabase()));
          
        }

        public List<IDiagramNode> ItemsToValidate
        {
            get { return _itemsToValidate ?? (_itemsToValidate = new List<IDiagramNode>()); }
            set { _itemsToValidate = value; }
        }

        public IEnumerator ValidateDatabase()
        {
            var ws = Container.Resolve<DatabaseService>();
            if (ws != null && ws.CurrentConfiguration != null && ws.CurrentConfiguration.Database != null)
            foreach (var item in ws.CurrentConfiguration.Database.AllOf<IDiagramNode>())
            {
                yield return new TaskProgress("Validating " + item.Name, 95f);
                ValidateNode(item); 
                
            }
        } 
        public IEnumerator ValidateGraph()
        {
            var ws = Container.Resolve<WorkspaceService>();
            if (ws == null || ws.CurrentWorkspace == null || ws.CurrentWorkspace.CurrentGraph == null) yield break;
            var items =  ws.CurrentWorkspace.CurrentGraph.NodeItems.ToArray();
            var total = 100f / items.Length;
            for (int index = 0; index < items.Length; index++)
            {
                var item = items[index];
                yield return new TaskProgress("Validating " + item.Name, index * total);
                ValidateNode(item);
            }

        }
        public void PropertyChanged(IDataRecord record, string name, object previousValue, object nextValue)
        {
            if (name == "Selected") return;
            QueueValidate(record);
        }

 
        public List<IDiagramNode> ErrorNodes
        {
            get { return _errorNodes ?? (_errorNodes = new List<IDiagramNode>()); }
            set { _errorNodes = value; }
        }

        private void ValidateNode(IDiagramNode node)
        {
            ErrorNodes.Remove(node);
            var list = new List<ErrorInfo>();
            node.Validate(list);
            node.Errors = list.ToArray();
            if (node.Errors.Length > 0)
            {
                ErrorNodes.Add(node);
            }
            Signal<INodeValidated>(_=>_.NodeValidated(node));
        }


        public BackgroundTask ValidationTask { get; set; }

        public void Progress(ICommand command, string message, float progress)
        {
            InvertApplication.Log(message);
        }

        public void Execute(ValidateDatabaseCommand command)
        {
           //InvertApplication.Log("YUP");
            var list = new List<ErrorInfo>();
            var repo = new TypeDatabase(new JsonRepositoryFactory(command.FullPath));
            var items = repo.AllOf<IDiagramNode>();
            foreach (IDiagramNode t in items)
            {
                if (command.Worker.CancellationPending) return;
                var item = t;
                var item1 = item;
                command.Worker.ReportProgress(1, item1.Name);
                item.Validate(list);
            }
        }


        public void CommandExecuted(ICommand command)
        {
            if (command is SaveAndCompileCommand) return;
            if (ShouldRevalidate)
            Signal<ITaskHandler>(_ => _.BeginBackgroundTask(ValidateGraph()));

            ItemsToValidate.Clear();
            ShouldRevalidate = false;
        }

        public void RecordInserted(IDataRecord record)
        {
            QueueValidate(record);
        }

        private void QueueValidate(IDataRecord record)
        {
          
            var node = record as IDiagramNode;
            if (node != null)
            {
                ItemsToValidate.Add(node); ShouldRevalidate = true;
            }
            else
            {
                var nodeItem = record as IDiagramNodeItem;
                if (nodeItem != null)
                {
                    ShouldRevalidate = true;
                    node = nodeItem.Node;
                    if (node != null)
                    ItemsToValidate.Add(node);
                }
            }
                
        }

        public bool ShouldRevalidate { get; set; }
    }

    public class ValidateDatabaseCommand : Command, IBackgroundCommand
    {
        public IRepository Repository { get; set; }
        public BackgroundTask Task { get; set; }
        public BackgroundWorker Worker { get; set; }
        public IGraphConfiguration GraphConfiguration { get; set; }
        public string FullPath { get; set; }
    }
}
