using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Invert.IOC;

namespace Invert.Core
{
    public class MultiThreadingSystem : CorePlugin, IExecuteCommand<BackgroundTaskCommand>
    {

        public override bool Enabled
        {
            get { return true; }
            set { }
        }

        public override void Loaded(UFrameContainer container)
        {
        }

        public void Execute(BackgroundTaskCommand command)
        {
            BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            InvertApplication.Log("Creating background task");
            worker.DoWork += (sender, args) =>
            {
               
                InvertApplication.Log("Executing background task");
                var bgCommand = args.Argument as BackgroundTaskCommand;
               
                if (bgCommand != null)
                {
                    bgCommand.Command.Worker = sender as BackgroundWorker;
                    bgCommand.Action(bgCommand.Command);
                }
                   

              

            };
            worker.ProgressChanged += (sender, args) =>
            {
                InvertApplication.Log("PROGRESS");
                InvertApplication.SignalEvent<ICommandProgressEvent>(_=>_.Progress(null,args.UserState.ToString(),args.ProgressPercentage));
            };
            command.Task = new BackgroundTask(worker);
            worker.RunWorkerAsync(command);
       
        }



    }

    public class BackgroundTaskCommand : ICommand
    {
        public string Title { get;  set; }
        public Action<IBackgroundCommand> Action { get; set; }
        public BackgroundTask Task { get; set; }
        public IBackgroundCommand Command { get; set; }
    }


    public interface ICommandProgressEvent
    {
        void Progress(ICommand command, string message, float progress);
    }

    public class BackgroundTask
    {
        public BackgroundWorker Worker { get; set; }

        public BackgroundTask(BackgroundWorker worker)
        {
            Worker = worker;
        }

        public void Cancel()
        {
            Worker.CancelAsync();
            Worker.Dispose();
        }

        public bool IsRunning
        {
            get { return Worker.IsBusy; }
        }

    }
  
}




