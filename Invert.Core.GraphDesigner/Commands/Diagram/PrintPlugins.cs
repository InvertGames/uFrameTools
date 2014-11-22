using System;
using System.Reflection;
using Invert.Core.GraphDesigner;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class PrintPlugins : ElementsDiagramToolbarCommand
    {
        public override string Name
        {
            get { return "Print Json"; }
        }

        public override void Perform(DiagramViewModel node)
        {
  
            //var subsystem = node.SelectedGraphItem as SubSystemViewModel;
            //if (subsystem != null)
            //{
            //    var data = subsystem.DataObject as SubSystemData;
            //    var instances = data.AllInstancesDistinct;
            //    foreach (var instane in instances)
            //    {
            //        Debug.Log(instane.RelatedTypeName+" : "+instane.Name);
            //    }
            //}
            //Debug.Log(uFrameEditor.uFrameTypes);
            Type T = typeof(GUIUtility);
            PropertyInfo systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            systemCopyBufferProperty.SetValue(null, GraphData.Serialize(node.DiagramData as GraphData).ToString(), null);
            Debug.Log("Json copied to clipboard.");
        }
    }
}