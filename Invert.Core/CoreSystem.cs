using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core;
using Invert.IOC;

namespace Invert.Core
{
    public class LambdaCommand :ICommand
    {
        public LambdaCommand(Action action)
        {
            Action = action;
        }

        public Action Action { get; set; }
    }
    public class CoreSystem : CorePlugin, IExecuteCommand<LambdaCommand>
    {
        public override bool Enabled { get { return true; } set{}}
        public override void Loaded(UFrameContainer container)
        {
            
        }

        public void Execute(LambdaCommand command)
        {
            command.Action();
        }
    }
}
