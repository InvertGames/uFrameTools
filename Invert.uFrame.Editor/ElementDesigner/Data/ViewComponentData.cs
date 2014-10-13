using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ViewComponentData : DiagramNode
{
    [SerializeField]
    private string _baseIdentifier;

    [SerializeField]
    private string _viewIdentifier;

    public IEnumerable<ViewComponentData> AllBaseTypes
    {
        get
        {
            var t = this.Base;
            while (t != null)
            {
                yield return t;
                t = t.Base;
            }
        }
    }

    public ViewComponentData Base
    {
        get { return Project.GetViewComponents().FirstOrDefault(p => p.Identifier == this.BaseIdentifier); }
    }

    public string BaseIdentifier
    {
        get { return _baseIdentifier; }
        set
        {
            _baseIdentifier = value;
            Dirty = true;
        }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { yield break; }
        set { }
    }

    public Type CurrentType
    {
        get { return Type.GetType(AssemblyQualifiedName); }
    }

    public ViewData View
    {
        get
        {
            if (Base != null)
            {
                return Base.View;
            }
            return Project.GetViews().FirstOrDefault(p => p.Identifier == ViewIdentifier);
        }
    }

    public string ViewIdentifier
    {
        get { return _viewIdentifier; }
        set { _viewIdentifier = value; }
    }

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get { yield break; }
    }

    public override string Label
    {
        get { return Name; }
    }

    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameViewComponentRefactorer(this);
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        _viewIdentifier = cls["ViewIdentifier"].Value;
        _baseIdentifier = cls["BaseIdentifier"].Value;
    }

    public void SetView(ViewData view)
    {
        ViewIdentifier = view.Identifier;
    }

    public void RemoveView()
    {
        ViewIdentifier = null;
    }

    public override bool EndEditing()
    {
        return base.EndEditing();
    }

    public override void NodeItemRemoved(IDiagramNodeItem item)
    {
        
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Project.RemoveNode(this);
        foreach (var viewComponentData in Project.GetViewComponents())
        {
            if (viewComponentData.BaseIdentifier == this.Identifier)
            {
                viewComponentData.BaseIdentifier = null;
            }
        }
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ViewIdentifier", new JSONData(_viewIdentifier));
        cls.Add("BaseIdentifier", new JSONData(_baseIdentifier));
    }

    public void RemoveBaseViewComponent()
    {
        BaseIdentifier = null;
    }

    public void SetBaseViewComponent(ViewComponentData output)
    {
        BaseIdentifier = output.Identifier;
    }
}