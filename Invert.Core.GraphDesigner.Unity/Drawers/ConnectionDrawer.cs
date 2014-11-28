using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class ConnectionDrawer : Drawer<ConnectionViewModel>
    {
        public override int ZOrder
        {
            get
            {
                if (ViewModel.IsFullColor)
                {
                    return 5;
                }
                
                return -1;
            }
        }

        public ConnectionDrawer(ConnectionViewModel viewModelObject) : base(viewModelObject)
        {
        }

        public override void Draw(float scale)
        {
            base.Draw(scale);
            if (ViewModel.IsStateLink)
            {
                DrawStateLink(scale);
            }
            else
            {
                DrawBeizureLink(scale);
            }
        
        }

        private void DrawStateLink(float scale)
        {
            var _startPos = ViewModel.ConnectorA.Bounds.center;
            var _endPos = ViewModel.ConnectorB.Bounds.center;

            var _startRight = ViewModel.ConnectorA.Direction == ConnectorDirection.Output;
            var _endRight = ViewModel.ConnectorB.Direction == ConnectorDirection.Output;
            //Handles.color = ViewModel.CurrentColor;
            List<Vector2> points = new List<Vector2>();
            Vector2 curr;
            points.Add(curr = _startPos);
   
            if (_endPos.x < _startPos.x)
            {
                points.Add(curr = curr + new Vector2(15f, 0f));
                points.Add(curr = curr + new Vector2(0f, (_endPos.y - _startPos.y)/2f));
                points.Add(_endPos - new Vector2(15f, (_endPos.y - _startPos.y)/2f));
                points.Add(_endPos - new Vector2(15f, 0f));
            }
            else
            {
                points.Add(curr = _startPos + new Vector2((_endPos.x - _startPos.x)/ 2f,0f));
                points.Add(new Vector2(curr.x,_endPos.y));
            }
        
        
    
            points.Add(_endPos);
            var scaled = points.Select(p => new Vector3(p.x * scale,p.y * scale)).ToArray();

            InvertGraphEditor.PlatformDrawer.DrawPolyLine(scaled);
            InvertGraphEditor.PlatformDrawer.DrawPolyLine(scaled.Select(p => p + new Vector3(1f, 1f, 0f)).ToArray());
        
        
        }

        private void DrawBeizureLink(float scale)
        {
            var _startPos = ViewModel.ConnectorA.Bounds.center;
            var _endPos = ViewModel.ConnectorB.Bounds.center;

            var _startRight = ViewModel.ConnectorA.Direction == ConnectorDirection.Output;
            var _endRight = ViewModel.ConnectorB.Direction == ConnectorDirection.Output;


            var multiplier = Mathf.Min(30f, (_endPos.x - _startPos.x)*0.3f);


            var m2 = 3;
            if (multiplier < 0)
            {
                _startRight = !_startRight;
                _endRight = !_endRight;
            }


            var startTan = _startPos + (_endRight ? -Vector2.right*m2 : Vector2.right*m2)*multiplier;

            var endTan = _endPos + (_startRight ? -Vector2.right*m2 : Vector2.right*m2)*multiplier;

            var shadowCol = new Color(0, 0, 0, 0.1f);
            if (ViewModel.IsFullColor)
                for (int i = 0; i < 3; i++) // Draw a shadow
                    InvertGraphEditor.PlatformDrawer.DrawBezier(_startPos*scale, _endPos*scale, startTan*ElementDesignerStyles.Scale,
                        endTan*ElementDesignerStyles.Scale, shadowCol,  (i + 1)*5);

            InvertGraphEditor.PlatformDrawer.DrawBezier(_startPos * scale, _endPos * scale, startTan * ElementDesignerStyles.Scale,
                endTan*ElementDesignerStyles.Scale, ViewModel.CurrentColor,  3);
        }
    }
}