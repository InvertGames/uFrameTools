using System.Linq;
using Invert.Data;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class InputOutputViewModel : GraphItemViewModel
    {
        private bool _allowSelection;
        public override Vector2 Position { get; set; }
        public override string Name { get; set; }
        public bool IsInput { get; set; }
        public bool IsOutput { get; set; }
        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
            if (ReferenceItem != null)
            {
                AllowSelection = ReferenceItem.AllowSelection;
            }
        }

        public bool AllowSelection
        {
            get
            {
                
                return _allowSelection;
            }
            set { _allowSelection = value; }
        }

        public override ConnectorViewModel InputConnector
        {
            get
            {
                if (!IsInput) return null;
                return base.InputConnector;
            }
        }
        public override ConnectorViewModel OutputConnector
        {
            get
            {
                if (!IsOutput || AllowSelection) return null;
                return base.OutputConnector;
            }
        }

        public GenericSlot ReferenceItem
        {
            get { return DataObject as GenericSlot; }
        }
        public string SelectedItemName
        {
            get
            {
                if (ReferenceItem != null)
                {
                    var source = ReferenceItem.InputFrom<IDiagramNodeItem>();
                    if (source != null)
                    {
                        return source.Name;
                    }
                }
                return "-- Select Item --";
            }
        }

        public void SelectItem()
        {
            InvertGraphEditor.WindowManager.InitItemWindow(ReferenceItem.GetAllowed().ToArray(), _ =>
            {
                InvertApplication.Execute(new LambdaCommand(() =>
                {
                    if (IsInput)
                    {
                        ReferenceItem.SetInput(_);
                    }
                    else
                    {
                        ReferenceItem.SetOutput(_);
                    }
                   
                }));
            });
        }
    }

   
;}