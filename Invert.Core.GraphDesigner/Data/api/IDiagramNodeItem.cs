using System.ComponentModel;

namespace Invert.Core.GraphDesigner
{
    public interface IDiagramNodeItem : ISelectable, IJsonObject,IItem
    {
        string Name { get; set; }
        string Highlighter { get; }
        string FullLabel { get; }
        bool IsSelectable { get;}
        DiagramNode Node { get; set; }
        [Browsable(false)]
        DataBag DataBag { get; set; }
        
        /// <summary>
        /// Is this node currently in edit mode/ rename mode.
        /// </summary>
        bool IsEditing { get; set; }

        FlagsDictionary Flags { get; set; }

        //void Remove(IDiagramNode diagramNode);
        void Rename(IDiagramNode data, string name);
        void NodeRemoved(IDiagramNode nodeData);
        void NodeItemRemoved(IDiagramNodeItem nodeItem);
        void NodeAdded(IDiagramNode data);
        void NodeItemAdded(IDiagramNodeItem data);
        bool ValidateInput(IDiagramNodeItem arg1, IDiagramNodeItem arg2);
        bool ValidateOutput(IDiagramNodeItem arg1, IDiagramNodeItem arg2);
    }
}