using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class DebugCommand : ElementsDiagramToolbarCommand
    {
        public string _name;

        public DebugCommand(string name, Action<DiagramViewModel> action)
        {
            _name = name;
            Action = action;
        }

        public DebugCommand(Action<DiagramViewModel> action)
        {
            Action = action;
        }

        public override string Name
        {
            get { return _name; }
        }

        Action<DiagramViewModel> Action { get; set; }
        public override void Perform(DiagramViewModel node)
        {
            if (Action != null)
                Action(node);

        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.BottomRight; }
        }
    }
    public class PrintPlugins : ElementsDiagramToolbarCommand
    {
        public override string Name
        {
            get { return "Print Json"; }
        }

        public override void Perform(DiagramViewModel node)
        {
  
            var subsystem = node.SelectedGraphItem as SubSystemViewModel;
            if (subsystem != null)
            {
                var data = subsystem.DataObject as SubSystemData;
                var instances = data.AllInstancesDistinct;
                foreach (var instane in instances)
                {
                    Debug.Log(instane.RelatedTypeName+" : "+instane.Name);
                }
            }
            //Debug.Log(uFrameEditor.uFrameTypes);
            Type T = typeof(GUIUtility);
            PropertyInfo systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            systemCopyBufferProperty.SetValue(null, ElementsGraph.Serialize(node.DiagramData as GraphData).ToString(), null);
            Debug.Log("Json copied to clipboard.");
        }
    }
    public class ForceUpgradeDiagram : ElementsDiagramToolbarCommand
    {
        public override string Name
        {
            get { return "Force Upgrade"; }
        }

        public override void Perform(DiagramViewModel node)
        {

            var data = node.CurrentRepository.NodeItems.OfType<SubSystemData>();
            foreach (var view in data)
            {
                
                view.Instances.RemoveAll(p => true);
            }
            var views = node.CurrentRepository.NodeItems.OfType<ViewData>();
            foreach (var view in views)
            {
                view.Bindings.RemoveAll(p => true);
            }
            node.UpgradeProject();
        }
    }
}