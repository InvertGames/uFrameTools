﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Invert.Core.GraphDesigner.Unity.WindowsPlugin;
using Invert.Data;
using Invert.IOC;
using UnityEditor;
using MessageType = Invert.Core.GraphDesigner.Unity.WindowsPlugin.MessageType;

namespace Invert.Core.GraphDesigner.Unity.LogSystem
{
    public class LogSystem : DiagramPlugin, ILogEvents, IExecuteCommand<InfiniteLoopCommand>//, ICommandProgressEvent
    {
        public static IRepository Repository { get; set; }

        public override void Loaded(UFrameContainer container)
        {
            base.Loaded(container);
            Repository = container.Resolve<IRepository>();
        }

        public void Log(string message, MessageType type)
        {
            var msg = Repository.Create<LogMessage>();
            msg.Message = message;
            msg.MessageType = type;
            //Repository.Add(msg);
            //Repository.Commit();       
        }

        public void Log<T>(T message) where T : LogMessage, new()
        {
            var msg = Repository.Create<T>();
           // Repository.Add(msg);
           // Repository.Commit();    
        }


        [MenuItem("uFrame Dev/Multithreading/Start Infinite Loop")]
        public static void RunInfiniteLoop()
        {
            Task = InvertApplication.ExecuteInBackground(new InfiniteLoopCommand());
        }

        [MenuItem("uFrame Dev/Multithreading/Stop Infinite Loop")]
        public static void StopInfiniteLoop()
        {
            Task.Cancel();
        }

        public static BackgroundTask Task { get; set; }

        public void Execute(InfiniteLoopCommand command)
        {
            while (true)
            {
                InvertApplication.SignalEvent<ILogEvents>(
                    i => i.Log(string.Format("Generated from infinite loop, {0}", DateTime.Now), MessageType.Info));
                Thread.Sleep(1000);
            }
        }

        public void Progress(ICommand command, string message, float progress)
        {
            Log(message,MessageType.Info);
        }
    }

    public class InfiniteLoopCommand : ICommand, IBackgroundCommand
    {
        public string Title { get; private set; }

        public BackgroundWorker Worker { get; set; }
    }

}