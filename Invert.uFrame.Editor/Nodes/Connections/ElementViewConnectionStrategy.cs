using System.Collections.Generic;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class ElementViewConnectionStrategy : DefaultConnectionStrategy<ElementData,ViewData>
    {
        public override Color ConnectionColor
        {
            get { return uFrameEditor.Settings.ViewLinkColor; }
        }

        public override ConnectionViewModel Connect(ConnectorViewModel a, ConnectorViewModel b)
        {

            if (a.ConnectorFor is ElementNodeViewModel && b.ConnectorFor is ViewNodeViewModel)
            {
                if (a.Direction == ConnectorDirection.Output && b.Direction == ConnectorDirection.Input)
                {
                    return new ConnectionViewModel()
                    {
                        ConnectorA = a,
                        ConnectorB = b,
                        Apply = Apply
                    };
                }

            }
            return null;
        }

        public override void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        {
            base.GetConnections(connections, info);
            connections.AddRange(info.ConnectionsByData<ElementData, ViewData>(Color.white, (o, i) => i.ForAssemblyQualifiedName == o.AssemblyQualifiedName, Remove));
        }

        protected override bool IsConnected(ElementData outputData, ViewData inputData)
        {
            return inputData.ForAssemblyQualifiedName == outputData.AssemblyQualifiedName;
        }

        protected override void ApplyConnection(ElementData output, ViewData input)
        {
            input.SetElement(output);
            
        }

        protected override void RemoveConnection(ElementData output, ViewData input)
        {
            input.RemoveFromElement(output);
        }
    }
}