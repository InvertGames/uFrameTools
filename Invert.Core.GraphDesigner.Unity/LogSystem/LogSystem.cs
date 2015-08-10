using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Unity.WindowsPlugin;
using Invert.Data;
using Invert.IOC;

namespace Invert.Core.GraphDesigner.Unity.LogSystem
{
    public class LogSystem : DiagramPlugin, ILogEvents
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
            Repository.Add(msg);
            Repository.Commit();       
        }

        public void Log<T>(T message) where T : LogMessage, new()
        {
            var msg = Repository.Create<T>();
            Repository.Add(msg);
            Repository.Commit();    
        }

    }
}
