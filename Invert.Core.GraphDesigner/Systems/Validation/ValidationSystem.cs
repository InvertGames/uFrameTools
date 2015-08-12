using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Invert.Data;
using JetBrains.Annotations;

namespace Invert.Core.GraphDesigner
{
    public class ValidationSystem : DiagramPlugin, 
        //IDataRecordPropertyChanged, 
        //ICommandProgressEvent,
        IExecuteCommand<ValidateDatabaseCommand>
    {
        public bool shouldRestart = false;
        public void PropertyChanged(IDataRecord record, string name, object previousValue, object nextValue)
        {
            DoValidation();
        }

        private void DoValidation()
        {
            if (ValidationTask != null)
            {
               
                ValidationTask.Cancel();
                
                ValidationTask = null;
            }
            ValidationTask = InvertApplication.ExecuteInBackground(new ValidateDatabaseCommand()
            {
                FullPath = Container.Resolve<IGraphConfiguration>().FullPath,
               
                Task = ValidationTask,
               
            });
            ValidationTask.Worker.RunWorkerCompleted += (sender, args) =>
            {
                ValidationTask = null;
                if (shouldRestart)
                {
                    shouldRestart = false;
                    DoValidation();
                }
                InvertApplication.Log("Task complete");
            };
            
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
