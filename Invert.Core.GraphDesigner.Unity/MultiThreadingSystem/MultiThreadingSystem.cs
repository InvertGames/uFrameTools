using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor;

namespace Invert.Core.GraphDesigner.Unity.MultiThreadingSystem
{
    public class MultiThreadingSystem : DiagramPlugin, ISchedule
    {
        public void RunInBackground(Action action)
        {
        }

        public void RunInBackground<T>(Func<T> action)
        {

        }
    }


    public interface ISchedule
    {
        void RunInBackground(Action action);
        void RunInBackground<T>(Func<T> action);
    }


namespace Swing.Editor
{
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


}



