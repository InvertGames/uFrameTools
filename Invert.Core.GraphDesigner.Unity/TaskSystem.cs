using System.Collections;
using UnityEditor;

namespace Invert.Core.GraphDesigner.Unity
{
    public class TaskSystem : DiagramPlugin, IUpdate, ITaskHandler
    {
        private int fpsCount = 0;
        public IEnumerator Task { get; set; }
        public void Update()
        {
            if (Task != null)
            {
                if (!Task.MoveNext())
                {
                    Task = null;
                    Signal<IRepaintWindow>(_=>_.Repaint());
                    
                }
                else
                {
                    var current = Task.Current as TaskProgress;
                    if (current != null)
                    {
                        InvertGraphEditor.Platform.Progress(current.Percentage, current.Message);
                    }

                    Signal<IRepaintWindow>(_ => _.Repaint());
                }
                return;
            }
            else
            {
            }

            
        }

        public void BeginTask(IEnumerator task)
        {
            Task = task;
        }
    }
}