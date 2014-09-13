using System;
using System.Reflection;
using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class PrintPlugins : ElementsDiagramToolbarCommand
    {
        public override string Name
        {
            get { return "Print Json"; }
        }

        public override void Perform(DiagramViewModel node)
        {
            var diagram = uFrameEditor.CurrentDiagramViewModel.Data;
            if (diagram.CurrentFilter == null)
            {
                Debug.Log("CURRENT FILTER NULL");
            }
            else if (diagram.FilterState == null)
            {
                Debug.Log("FILTER STATE IS NULL");
            }
            //Debug.Log(uFrameEditor.uFrameTypes);
            Type T = typeof(GUIUtility);
            PropertyInfo systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            systemCopyBufferProperty.SetValue(null, ElementsGraph.Serialize(node.Data).ToString(), null);
            Debug.Log("Json copied to clipboard.");
        }
    }
}