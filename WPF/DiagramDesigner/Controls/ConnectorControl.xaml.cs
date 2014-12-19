using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Invert.Core.GraphDesigner;
using UnityEngine;
using Rect = System.Windows.Rect;

namespace DiagramDesigner.Controls
{
    /// <summary>
    /// Interaction logic for ConnectorControl.xaml
    /// </summary>
    public partial class ConnectorControl : UserControl
    {
        public ConnectorViewModel ViewModel
        {
            get { return DataContext as ConnectorViewModel; }
        }
        public ConnectorControl()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var connectorFor = ViewModel.ConnectorFor;
            var connectorBounds = ViewModel.ConnectorFor.ConnectorBounds;
            var forItem = connectorFor as ItemViewModel;
            if (forItem != null)
            {
                if (forItem.NodeViewModel.IsCollapsed)
                {
                    connectorBounds = forItem.NodeViewModel.ConnectorBounds;
                }
            }
            var nodePosition = connectorBounds;
            var texture = new UnityEngine.Rect(0f, 0f, 16f, 16f);
            var pos = new Vector2(0f, 0f);

            if (ViewModel.Side == ConnectorSide.Left)
            {
                pos.x = nodePosition.x;
                pos.y = nodePosition.y + (nodePosition.height * ViewModel.SidePercentage);
                pos.y -= (texture.height / 2f);
                pos.x -= (texture.width) + 2;
            }
            else if (ViewModel.Side == ConnectorSide.Right)
            {
                pos.x = nodePosition.x + nodePosition.width;
                pos.y = nodePosition.y + (nodePosition.height * ViewModel.SidePercentage);
                pos.y -= (texture.height / 2f);
                pos.x += 2;
            }
            else if (ViewModel.Side == ConnectorSide.Bottom)
            {
                pos.x = nodePosition.x + (nodePosition.width * ViewModel.SidePercentage);
                pos.y = nodePosition.y + nodePosition.height;
                pos.x -= (texture.width / 2f);
                //pos.y += texture.height;
            }
            else if (ViewModel.Side == ConnectorSide.Top)
            {
                pos.x = nodePosition.x + (nodePosition.width * ViewModel.SidePercentage);
                pos.y = nodePosition.y;
                pos.x -= (texture.width / 2f);
                pos.y -= texture.height;
            }

            ViewModel.Bounds = new UnityEngine.Rect(pos.x, pos.y, texture.width, texture.height);
            //if (!ViewModel.IsMouseOver)
            //{
            //    var mouseOverBounds = new Rect(Bounds);
            //    //mouseOverBounds.x -= mouseOverBounds.width*0.2f;
            //    mouseOverBounds.y += mouseOverBounds.height * 0.125f;
            //    mouseOverBounds.x += mouseOverBounds.width * 0.125f;
            //    mouseOverBounds.width *= 0.75f;
            //    mouseOverBounds.height *= 0.75f;
            //    Bounds = mouseOverBounds;
            //}
            base.OnRender(drawingContext);
            Canvas.SetLeft(this,ViewModel.Bounds.x);
            Canvas.SetTop(this,ViewModel.Bounds.y);
        }
    }
}
