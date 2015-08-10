using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Invert.Core.GraphDesigner.Unity.WindowsPlugin;
using UnityEditor;
using MessageType = Invert.Core.GraphDesigner.Unity.WindowsPlugin.MessageType;

namespace Invert.Core.GraphDesigner.Unity.MultiThreadingSystem
{
    public class MultiThreadingSystem : DiagramPlugin, ISchedule
    {

        public void RunInBackground(Action action)
        {

            Action task = () =>
            {
                try
                {
                    action();
                }
                catch (ThreadAbortException ex)
                {
                    Signal<ILogEvents>(i => i.Log("Thread aborted",MessageType.Warning));
                }
            };

           // th.Start();
        
        }

        public BackgroundTask<TProgress> RunInBackground<TProgress>(Action action, Action<TProgress> onProgress)
        {

            Action task = () =>
            {
                try
                {
                  //  action(onProgress);
                }
                catch (ThreadAbortException ex)
                {
                    Signal<ILogEvents>(i => i.Log("Thread aborted", MessageType.Warning));
                }
            };

            var th = new Thread(new ThreadStart(action));
            
            var bTask = new BackgroundTask<TProgress>(th);

            return bTask;
        }

        public void RunInBackground<T>(Func<T> action)
        {

        }

        [MenuItem("uFrame Dev/Multithreading/Test")]
        public static void Test()
        {
        }

    }



    public interface IProgressEvent
    {
        void Progress(string message, float progress);
    }

    public interface IBackgroundCommand
    {

    }

    public class BackgroundTask<TProgress>
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

        public Action<TProgress> OnProgress;

    }

    public interface ISchedule
    {
        void RunInBackground(Action action);
        BackgroundTask<TProgress> RunInBackground<TProgress>(Action action, Action<TProgress> onProgress);
        void RunInBackground<T>(Func<T> action);
    }

	public class EditorCoroutine
	{
		public static EditorCoroutine Start( IEnumerator _routine )
		{
			EditorCoroutine coroutine = new EditorCoroutine(_routine);
            coroutine.Start();
			return coroutine;
		}

		readonly IEnumerator routine;
		EditorCoroutine( IEnumerator _routine )
		{
			routine = _routine;
		}

        void Start()
		{
			//Debug.Log("start");
			EditorApplication.update += Update;
		}
		public void Stop()
		{
			//Debug.Log("stop");
            EditorApplication.update -= Update;
		}

		void Update()
		{
			/* NOTE: no need to try/catch MoveNext,
			 * if an IEnumerator throws its next iteration returns false.
			 * Also, Unity probably catches when calling EditorApplication.update.
			 */

			//Debug.Log("update");
			if (!routine.MoveNext())
			{
				Stop();
			}
		}
	}
}




