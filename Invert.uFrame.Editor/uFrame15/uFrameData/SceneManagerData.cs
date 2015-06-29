using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;
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
    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);
        _subSystemIdentifier = cls["SubSystemIdentifier"].Value;
    }


    [SerializeField]
    private string _subSystemIdentifier;

    [SerializeField]
    private List<SceneManagerTransition> _transitions = new List<SceneManagerTransition>();

    public IEnumerable<ElementData> AllImportedElements
    {
        get
        {
            foreach (var subsystem in Subsystems)
            {
                foreach (var element in subsystem.GetContainingNodes(Graph).OfType<ElementData>())
                {
                    yield return element;
                }
            }
        }
    }

    public IEnumerable<SubSystemData> Subsystems
    {
        get
        {
            yield return SubSystem;
            foreach (var subsystem in SubSystem.GetAllImportedSubSystems(Graph))
            {
                yield return subsystem;
            }
        }
    }
    public override IEnumerable<IDiagramNodeItem> PersistedItems
    {
        get { return Transitions.Cast<IDiagramNodeItem>(); }
        set
        {
            Transitions = value.OfType<SceneManagerTransition>().ToList();
            
        }
    }

    public Type CurrentSettingsType
    {
        get
        {
            return Type.GetType(uFrameEditor.UFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", NameAsSettings));
        }
    }


    public override bool ImportedOnly
    {
        get { return true; }
    }

    public override IEnumerable<IDiagramNodeItem> DisplayedItems
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


    public SubSystemData SubSystem
    {
        get
        {
            return Graph.GetSubSystems().FirstOrDefault(p => p.Identifier == SubSystemIdentifier);
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

    }


    public override bool EndEditing()
    {
        return base.EndEditing();
    }

    public override void NodeItemRemoved(IDiagramNodeItem item)
    {
        Transitions.Remove(item as SceneManagerTransition);
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
        Project.RemoveNode(this);
    }

}