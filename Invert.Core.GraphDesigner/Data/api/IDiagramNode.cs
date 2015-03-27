using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using Invert.Json;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IChangeData : IJsonObject
    {
        bool IsValid { get; }
        IGraphItem Item { get; set; }
        string ItemIdentifier { get; set; }
        void Update(IChangeData data);
    }

    public interface IChangeData<T> : IChangeData
    {
        T Old { get; set; }
        T New { get; set; }
    }


    public class NameChange : StringChange
    {
        public NameChange(IGraphItem item, string old, string @new) : base(item, old, @new)
        {
        }

        public NameChange()
        {
        }

        public override void Update(IChangeData data)
        {
            var tc = data as NameChange;
            if (tc != null)
            {
                if (New == tc.Old)
                {
                    New = tc.New;
                }
            }
        }
        public override string ToString()
        {
            return string.Format("{0}: Name {1} Changed to {2}", Item.Label, Old, New);
        }
    }

    public class TypeChange : StringChange
    {
        public TypeChange(IGraphItem item, string old, string @new) : base(item, old, @new)
        {
        }

        public TypeChange()
        {
        }

        public override void Update(IChangeData data)
        {
            var tc = data as TypeChange;
            if (tc != null)
            {
                if (New == tc.Old)
                {
                    New = tc.New;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: Type {1} Changed to {2}",Item.Label, Old, New);
        }
    }
    public abstract class StringChange : IChangeData<string>
    {
        public string Old { get; set; }
        public string New { get; set; }
        public IGraphItem Item { get; set; }
        public string ItemIdentifier { get; set; }
        public bool IsValid
        {
            get { return Old != null&& New != null && Old != New; }
        }

        protected StringChange()
        {
        }

        protected StringChange(IGraphItem item, string old, string @new)
        {
            Old = old;
            New = @new;
            Item = item;
            ItemIdentifier = item.Identifier;
        }

        public abstract void Update(IChangeData data);
        public void Serialize(JSONClass cls)
        {
            if (Item != null)
            {
                cls.Add("ItemIdentifier", new JSONData(Item.Identifier));
            }
            if (!string.IsNullOrEmpty(Old))
            {
                cls.Add("Old", new JSONData(Old));
            }
            if (!string.IsNullOrEmpty(New))
            {
                cls.Add("New", new JSONData(New));
            }
        }

        public void Deserialize(JSONClass cls)
        {
            if (cls["ItemIdentifier"] != null)
            {
                ItemIdentifier = cls["ItemIdentifier"].Value;
            }
            if (cls["Old"] != null)
            {
                Old = cls["Old"].Value;
            }
            if (cls["New"] != null)
            {
                Old = cls["New"].Value;
            }
        }

        
    }

    public interface IDiagramNode : IDiagramNodeItem
    {

        void TrackChange(IChangeData data);
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

    public interface IDocumentationBuilder
    {
        void BeginArea(string id);
        void EndArea();
        void BeginSection(string id);
        void EndSection();
        void PushIndent();
        void PopIndent();
        void LinkToNode(IDiagramNodeItem node, string text = null);
        void NodeImage(DiagramNode node);
        void Paragraph(string text, params object[] args);
        void Lines( params string[] lines);
        void Title(string text, params object[] args);
        void Title2(string text, params object[] args);
        void Title3(string text, params object[] args);
        void Note(string text, params object[] args);
        void TemplateLink();
        void Literal(string text, params object[] args);
        void Section(string text, params object[] args);
        void Rows(params Action[] actions);
        void Columns(params Action[] actions);
        void YouTubeLink(string id);
        void TemplateExample<TTemplate, TData>(TData data, string templateMember = null) where TTemplate : IClassTemplate<TData>;
        void ShowGist(string id, string filename, string userId = "micahosborne");
        bool ShowTutorialStep(ITutorialStep step, Action<IDocumentationBuilder> stepContent = null);
        void BeginTutorial(string walkthrough);
        void EndTutorial();
        void ImageByUrl(string empty);
    }


    public class InteractiveTutorial 
    {
        public string Name { get; set; }
        private List<ITutorialStep> _steps;
        private bool _lastStepCompleted = true;

        public InteractiveTutorial(string name)
        {
            Name = name;
        }

        public List<ITutorialStep> Steps
        {
            get { return _steps ?? (_steps = new List<ITutorialStep>()); }
            set { _steps = value; }
        }

        public bool LastStepCompleted
        {
            get { return _lastStepCompleted; }
            set { _lastStepCompleted = value; }
        }
    }

    public interface ITutorialStep
    {
        string Name { get; set; }
        Action DoIt { get; set; }
        Func<string> IsDone { get; set; }
        Action<IDocumentationBuilder> StepContent { get; set; }
    }

    public class TutorialStep : ITutorialStep
    {
        public TutorialStep( string name, Func<string> isDone)
            : this( name, isDone, null)
        {
        }

        public TutorialStep( string name, Func<string> isDone, Action doIt)
        {
         
            Name = name;
            IsDone = isDone;
            DoIt = doIt;
        }

        public string Name { get; set; }
        public Action DoIt { get; set; }
        public Func<string> IsDone { get; set; }
        public Action<IDocumentationBuilder> StepContent { get; set; }
    }
}