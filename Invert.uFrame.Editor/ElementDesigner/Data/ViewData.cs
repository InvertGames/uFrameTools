using System.Reflection;
using System.Text;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ViewData : DiagramNode, ISubSystemType
{
    [SerializeField]
    private string _baseViewIdentifier;

    [SerializeField]
    private List<string> _componentIdentifiers = new List<string>();

    [SerializeField]
    private string _forElementIdentifier;

    private List<ViewPropertyData> _properties;
    private List<IBindingGenerator> _newBindings;
    private List<MethodInfo> _bindingMethods;

    public override string SubTitle
    {
        get { return BaseViewName; }
    }

    /// <summary>
    /// The baseview class if any
    /// </summary>
    public ViewData BaseView
    {
        get
        {

            if (string.IsNullOrEmpty(BaseViewIdentifier)) return null;
            return Data.GetViews().FirstOrDefault(p => p.Identifier == BaseViewIdentifier);
        }
    }

    /// <summary>
    /// The identifier to the base view this view will derived from
    /// </summary>
    public string BaseViewIdentifier
    {
        get { return _baseViewIdentifier; }
        set { _baseViewIdentifier = value;
            _bindingMethods = null;
            _newBindings = new List<IBindingGenerator>();
        }
    }

    public List<ViewPropertyData> Properties
    {
        get { return _properties ?? (_properties = new List<ViewPropertyData>()); }
        set { _properties = value; }
    }

    /// <summary>
    /// The name of the view that this view will derive from
    /// </summary>
    public string  BaseViewName
    {
        get
        {
            var baseNode = BaseNode;
            var viewNode = baseNode as ViewData;
            if (viewNode != null)
            {
                return viewNode.NameAsView;
            }
            var element = ViewForElement;
            if (element != null)
            {
                return element.NameAsViewBase;
            }
            return "[None]";
        }
    }

    public List<string> ComponentIdentifiers
    {
        get { return _componentIdentifiers; }
        set { _componentIdentifiers = value; }
    }

    public IEnumerable<ViewComponentData> Components
    {
        get { return Data.GetViewComponents().Where(p => ComponentIdentifiers.Contains(p.Identifier)); }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get
        {
            
            return this.Properties.Cast<IDiagramNodeItem>().ToArray();
        }
        set { this.Properties = value.OfType<ViewPropertyData>().ToList(); }
    }

    public Type CurrentViewType
    {
        get
        {
            
            return Type.GetType(ViewAssemblyQualifiedName);
        }
    }

    public override IEnumerable<Refactorer> Refactorings
    {
        get
        {
            if (RenameRefactorer != null)
            {
                yield return RenameRefactorer;
            }
            if (NewBindings.Count > 0)
            {
                yield return BindingInsertMethodRefactorer;
            }
        }
    }
    public string ForElementIdentifier
    {
        get { return _forElementIdentifier; }
        set { _forElementIdentifier = value; }
    }
    public List<MethodInfo> BindingMethods
    {
        get
        {
            if (_bindingMethods == null)
            {
                SetBindingMethods();
            }
            return _bindingMethods ?? (_bindingMethods = new List<MethodInfo>());
        }
    }

    public List<IBindingGenerator> NewBindings
    {
        get { return _newBindings ?? (_newBindings = new List<IBindingGenerator>()); }
        set { _newBindings = value; }
    }

    public InsertMethodRefactorer BindingInsertMethodRefactorer
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var addedGenerator in NewBindings)
            {
                addedGenerator.CallBase = false;
                sb.AppendLine(addedGenerator.ToString());
            }
            return new InsertMethodRefactorer()
            {
                InsertText = sb.ToString(),
                ClassName = Name
            };
        }
    }
    public override IEnumerable<IDiagramNodeItem> Items
    {
        get
        {
            yield break;
            //if (Behaviours == null)
            //    yield break;

            //foreach (var behaviourSubItem in Behaviours)
            //{
            //    yield return behaviourSubItem;
            //}
        }
    }

    public override string Label { get { return Name; } }

    public string NameAsView
    {
        get
        {
            return string.Format("{0}", Name);
        }
    }

    public string NameAsViewBase
    {
        get
        {
            return string.Format("{0}Base", Name);
        }
    }

    public string NameAsViewViewBase
    {
        get
        {
            return string.Format("{0}ViewBase", Name);
        }
    }

    public string ViewAssemblyQualifiedName
    {
        get
        {
            return uFrameEditor.UFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", NameAsView);
        }
    }

    //public DiagramNode BaseView
    //{
    //    get { return Data.Elements.FirstOrDefault(p => p.Identifier == BaseViewIdentifier); }
    //}

    public IDiagramNode BaseNode
    {
        get { return Data.NodeItems.FirstOrDefault(p => p.Identifier == BaseViewIdentifier); }
    }

    public ElementData ViewForElement
    {
        get
        {
            var bn = ForElementIdentifier;
            if (bn != null)
            {
                var item = Data.NodeItems.OfType<ElementData>().FirstOrDefault(p => p.Identifier == bn);
                if (item != null)
                    return item;
            }

            var baseNode = BaseNode as ViewData;
            if (baseNode != null)
            {
                return baseNode.ViewForElement;
            }
            return null;
        }
    }

    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameViewRefactorer(this);
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        // Upgrading project from old assembly names
        if (cls["ForAssemblyQualifiedName"] != null)
        {
            var element =
                repository.GetElements()
                    .FirstOrDefault(p => p.AssemblyQualifiedName == cls["ForAssemblyQualifiedName"].Value);
            if (element != null)
            {
                _forElementIdentifier = element.Identifier;
            }
        }
        else
        {
            _forElementIdentifier = cls["ForElementIdentifier"].Value;
        }
        

        _baseViewIdentifier = cls["BaseViewIdentifier"].Value;
        _componentIdentifiers = cls["ComponentIdentifiers"].AsArray.DeserializePrimitiveArray(n => n.Value).ToList();

    }

    private void SetBindingMethods()
    {
        var element = ViewForElement;
        if (element != null)
        {
            var list = new List<MethodInfo>();
            var vmType = this.CurrentViewType;
            if (vmType == null)
            {
                return;
            }
            var methods = vmType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            var bindingMethodNames = uFrameEditor.GetBindingGeneratorsFor(element).Select(p => p.MethodName).ToArray();
            foreach (var method in methods)
            {
                if (bindingMethodNames.Contains(method.Name))
                {
                    list.Add(method);
                }
            }
            _bindingMethods = list;
        }
    }
    public override void RefactorApplied()
    {
        base.RefactorApplied();
        NewBindings.Clear();

    }
    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Data.RemoveNode(this);
        foreach (var source in Data.GetViews().Where(p => p.ForElementIdentifier == this.Identifier))
        {
            source.ForElementIdentifier = null;
        }
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ForElementIdentifier", new JSONData(_forElementIdentifier));
        cls.Add("BaseViewIdentifier", new JSONData(_baseViewIdentifier));
        cls.AddPrimitiveArray("ComponentIdentifiers", _componentIdentifiers, i => new JSONData(i));
    }

    public void SetElement(ElementData output)
    {
        ForElementIdentifier = output.Identifier;
        BaseViewIdentifier = null;
    }

    public void RemoveFromElement(ElementData output)
    {
        ForElementIdentifier = null;
    }

    public void SetBaseView(ViewData output)
    {
        BaseViewIdentifier = output.Identifier;
    }

    public void ClearBaseView()
    {
        BaseViewIdentifier = null;
    }
}