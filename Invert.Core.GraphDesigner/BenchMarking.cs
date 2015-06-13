using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Invert.IOC;
using UnityEditor;

namespace Invert.Core.GraphDesigner
{
    public interface IPluginInspector
    {
        void DoInspector();
    }
    public class BenchMark
    {
        public DateTime Timestamp { get; set; }
        public TimeSpan TimeSinceLast { get; set; }
        public string Message { get; set; }
    }

    public interface IBenchmarkEvents
    {
        void BenchMark(string message);
    }
    public class BenchMarking : DiagramPlugin, IPluginInspector, IBenchmarkEvents
    {
        private List<BenchMark> _benchmarks = new List<BenchMark>();

        public List<BenchMark> Benchmarks
        {
            get { return _benchmarks; }
            set { _benchmarks = value; }
        }

        public int MaxMessages
        {
            get { return 10; }
        }

        public override void Initialize(UFrameContainer container)
        {
            ListenFor<IBenchmarkEvents>();
        }

        public void DoInspector()
        {
            //foreach (var item in Benchmarks)
            //{
            //    EditorGUI
            //}
        }

        public void BenchMark(string message)
        {

            var last = Benchmarks.LastOrDefault();
            if (last != null)
            {
                var mrk = new BenchMark()
                {
                    Message = message,
                    Timestamp = DateTime.Now,
                    TimeSinceLast = DateTime.Now.Subtract(last.Timestamp)
                };
                Benchmarks.Add(mrk);
                UnityEngine.Debug.Log(string.Format("{0}:{1} | {2}",mrk.TimeSinceLast.TotalSeconds, mrk.TimeSinceLast.TotalMilliseconds, mrk.Message));
            }
            else
            {
                Benchmarks.Add(new BenchMark()
                {
                    Message = message,
                    Timestamp = DateTime.Now,
                    TimeSinceLast = TimeSpan.FromSeconds(0)
                });
                
            }
            if (Benchmarks.Count > MaxMessages)
            {
                Benchmarks.Remove(Benchmarks.First());
            }
           
        }
    }
}
