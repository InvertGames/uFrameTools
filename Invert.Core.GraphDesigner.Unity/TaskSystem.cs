using System.Collections;
using UnityEditor;

namespace Invert.Core.GraphDesigner.Unity
{
    public interface ITaskProgressEvent
    {
        void Progress(float progress, string message, bool modal);
    }
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

                    Signal<ITaskProgressEvent>(_ => _.Progress(0f, string.Empty, IsModal));
        
                    Task = null;
                }
                else
                {
                    var current = Task.Current as TaskProgress;
                    if (current != null)
                    {
                        Signal<ITaskProgressEvent>(_ => _.Progress(current.Percentage, current.Message, IsModal));
                    }
                }
                return;
            }
            else
            {
            }

            
        }
        public bool IsModal { get; set; }
        public void BeginTask(IEnumerator task)
        {
            Task = task;
            IsModal = true;
        }

        public void BeginBackgroundTask(IEnumerator task)
        {
            IsModal = false;
            Task = task;
        }
    }
}