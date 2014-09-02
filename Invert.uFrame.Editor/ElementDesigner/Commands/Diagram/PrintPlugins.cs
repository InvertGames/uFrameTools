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
            Debug.Log(uFrameEditor.uFrameTypes);
            //Type T = typeof(GUIUtility);
            //PropertyInfo systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            //systemCopyBufferProperty.SetValue(null, JsonElementDesignerData.Serialize(node.Data).ToString(), null);
            //Debug.Log("Json copied to clipboard.");
        }
    }
}