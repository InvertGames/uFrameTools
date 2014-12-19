using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    
    public interface IDiagramNode : IDiagramNodeItem
    {
    
        string SubTitle { get; }
        /// <summary>
        /// The label that sits above the node providing additional insight.
        /// </summary>
        string InfoLabel { get; }
        /// <summary>
        /// Is this node collapsed or expanded
        /// </summary>
        bool IsCollapsed { get; set; }

        /// <summary>
        /// Any child list items of the node
        /// </summary>
        IEnumerable<IDiagramNodeItem> DisplayedItems { get; }

        /// <summary>
        /// Is this node dirty/modified and should its bounds be recalculated.
        /// </summary>
        bool Dirty { get; set; }

        /// <summary>
        /// Begins renaming the node.
        /// </summary>
        /// <param name="newName"></param>
        void Rename(string newName);

        /// <summary>
        /// Remove the node from the diagram.  Usually justs calls RemoveNode on the OwnerData
        /// </summary>
        void RemoveFromDiagram();

        /// <summary>
        /// The current element data displaying this node
        /// </summary>
        IProjectRepository Project { get; 
            //set; 
        }

        /// <summary>
        /// The current filter
        /// </summary>
        IDiagramFilter Filter { get; }
        /// <summary>
        /// The name that was used when the last save occured.
        /// </summary>
        string OldName { get; set; }

        /// <summary>
        /// The items that should be persisted with this diagram node.
        /// </summary>
        IEnumerable<IDiagramNodeItem> PersistedItems { get; }

        IEnumerable<IGraphItem> GraphItems { get; } 

        bool IsNewNode { get; set; }
        bool IsExternal { get; }

        /// <summary>
        /// The location that is used when entering a new filter or sub-diagram.
        /// </summary>
        Vector2 DefaultLocation { get; }

        IGraphData Graph { get; set; }


        /// <summary>
        /// Begin the rename process
        /// </summary>
        void BeginEditing();
        /// <summary>
        /// Apply changes to the renaming of the node.
        /// </summary>
        /// <returns>Could it successfully rename the node.</returns>
        bool EndEditing();


        //void NodeRemoved(IDiagramNode enumData);
        CodeTypeReference GetPropertyType(ITypedItem itemData);
        CodeTypeReference GetFieldType(ITypedItem itemData);
        void NodeAddedInFilter(IDiagramNode newNodeData);
    
    }

    public interface IRefactorable
    {
        IEnumerable<Refactorer> Refactorings { get;  }
        void RefactorApplied();
    }
}