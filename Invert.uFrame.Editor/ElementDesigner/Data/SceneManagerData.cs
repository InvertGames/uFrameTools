using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SceneManagerData : DiagramNode
{
    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("SubSystemIdentifier", _subSystemIdentifier);
        
    }
    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        _subSystemIdentifier = cls["SubSystemIdentifier"].Value;
    }

    [SerializeField]
    private FilterLocations _locations = new FilterLocations();

    [SerializeField]
    private string _subSystemIdentifier;

    [SerializeField]
    private List<SceneManagerTransition> _transitions = new List<SceneManagerTransition>();

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { return Transitions.Cast<IDiagramNodeItem>(); }
        set { Transitions = value.OfType<SceneManagerTransition>().ToList(); }
    }

    public Type CurrentSettingsType
    {
        get
        {
            return Type.GetType(uFrameEditor.UFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", NameAsSettings));
        }
    }

    public Type CurrentType
    {
        get
        {
            return Type.GetType(uFrameEditor.UFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", NameAsSceneManager));
        }
    }

    public bool ImportedOnly
    {
        get { return true; }
    }

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get
        {
            //if (SubSystem == null) yield break;
            //foreach (var command in SubSystem.IncludedCommands)
            //{
            //    yield return command;
            //}
            return Transitions.Cast<IDiagramNodeItem>();
        }
    }

    public override string Label
    {
        get { return Name; }
    }

    public FilterLocations Locations
    {
        get { return _locations; }
        set { _locations = value; }
    }

    public string NameAsSceneManager
    {
        get { return string.Format("{0}", Name); }
    }

    public string NameAsSceneManagerBase
    {
        get { return string.Format("{0}Base", Name); }
    }

    public string NameAsSettings
    {
        get { return string.Format("{0}Settings", Name); }
    }

    public string NameAsSettingsField { get { return string.Format("_{0}Settings", Name); } }

    public RenameSceneManagerRefactorer RenameRefactor { get; set; }

    public SubSystemData SubSystem
    {
        get
        {
            return Data.GetSubSystems().FirstOrDefault(p => p.Identifier == SubSystemIdentifier);
        }
    }

    public string SubSystemIdentifier
    {
        get { return _subSystemIdentifier; }
        set { _subSystemIdentifier = value; }
    }

    public List<SceneManagerTransition> Transitions
    {
        get { return _transitions; }
        set { _transitions = value; }
    }

    public override void BeginEditing()
    {
        base.BeginEditing();
        if (RenameRefactor == null)
        {
            RenameRefactor = new RenameSceneManagerRefactorer(this);
        }
    }



    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameSceneManagerRefactorer(this);
    }

    public override bool EndEditing()
    {
        return base.EndEditing();
    }

 

    public bool IsAllowed(object item, Type t)
    {
        if (t == typeof(SceneManagerData))
        {
            return false;
        }
        return true;
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Data.RemoveNode(this);
    }

}