using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Commands
{
    public class RunQueuedOperations : IBackgroundCommand
    {
        public BackgroundWorker Worker { get; set; }
    }
}
