using System;
using System.Linq;
using Gwen;
using Gwen.Control;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Base = Gwen.Control.Base;

namespace Invert.Platform.Gwen
{
    public class InspectorControl : Base, IGraphSelectionEvents
    {
        private Action _selectionDisposer;
        private DiagramInspectorDrawer diagramInspectorDrawer;
        private Properties _PropertyGrid;
        public InspectorControl(Base parent = null) : base(parent)
        {
            _PropertyGrid = new Properties(this);
            _PropertyGrid.Dock = Pos.Fill;
            _selectionDisposer = InvertApplication.ListenFor<IGraphSelectionEvents>(this);
        }
        
        protected override void Render(global::Gwen.Skin.Base skin)
        {
            base.Render(skin);
            if (diagramInspectorDrawer != null)
            diagramInspectorDrawer.Draw(InvertGraphEditor.PlatformDrawer,1f);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_selectionDisposer != null)
                _selectionDisposer();
        }

        public void SelectionChanged(GraphItemViewModel selected)
        {
            var diagram = selected.DiagramViewModel;
            diagram.LoadInspector();
            _PropertyGrid.DeleteAllChildren();
            foreach (var item in diagram.InspectorItems.OfType<PropertyFieldViewModel>())
            {
                var
                value = item.Getter();
                if (item.Type == typeof (bool))
                {
                    _PropertyGrid.Add(item.Label, new global::Gwen.Control.Property.Check(_PropertyGrid),
                        item.Getter().ToString());
                }
                else if (item.Type == typeof(string))
                {
                    _PropertyGrid.Add(item.Label, new global::Gwen.Control.Property.Text(_PropertyGrid),
                        value == null ? string.Empty : (string)value);
                }
            }
        }
    }
}
