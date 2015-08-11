using System;
using System.Collections;
using System.Collections.Generic;
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
            Action task = () =>
            {
                try
                {
                    command.Action();
                }
                catch (ThreadAbortException ex)
                {
                }
            };

            var thread = new Thread(new ThreadStart(task));
            var bgTask = new BackgroundTask(thread);
            thread.Start();

            command.Task = bgTask;
        }
    }

    public class BackgroundTaskCommand : ICommand
    {
        public string Title { get; private set; }
        public Action Action { get; set; }
        public BackgroundTask Task { get; set; }
    }


    public interface ICommandProgressEvent
    {
        void Progress(ICommand command, string message, float progress);
    }

    public class BackgroundTask
    {
        public Thread Thread { get; set; }

        public BackgroundTask(Thread thread)
        {
            Thread = thread;
        }

        public void Cancel()
        {
            Thread.Abort();
        }

        public bool IsRunning
        {
            get { return Thread.IsAlive; }
        }

    }

}




