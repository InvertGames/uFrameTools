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
    private List<ViewBindingData> _bindings;

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
           // return this.SceneProperties.Cast<IDiagramNodeItem>();
            return Bindings.Cast<IDiagramNodeItem>();
        }
        set
        {
            this.Bindings = value.OfType<ViewBindingData>().ToList();
        }
    }

    public Type CurrentViewType
    {
        get
        {
            
            return Type.GetType(ViewAssemblyQualifiedName);
        }
    }

    public IEnumerable<ViewModelPropertyData> SceneProperties 
    {
        get
        {
            var elementData = ViewForElement;
            if (elementData == null) yield break;
            foreach (var item in elementData.Properties.Where(p => p.DataBag["ViewIdentifier"] == this.Identifier))
            {
                yield return item;
            }
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
            if (NewBindings.Any())
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

    public List<MethodInfo> ReflectionBindingMethods
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

    public IEnumerable<ViewBindingData> NewBindings
    {
        get { return Bindings.Where(p => p.Generator != null); }
    }

    //public List<IBindingGenerator> NewBindings
    //{
    //    get { return _newBindings ?? (_newBindings = new List<IBindingGenerator>()); }
    //    set { _newBindings = value; }
    //}

    public InsertMethodRefactorer BindingInsertMethodRefactorer
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var addedGenerator in NewBindings.Select(p=>p.Generator))
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

    public List<ViewBindingData> Bindings
    {
        get { return _bindings ?? (_bindings = new List<ViewBindingData>()); }
        set { _bindings = value; }
    }

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get
        {
            foreach (var item in SceneProperties)
            {
                yield return new ViewPropertyData()
                {
                    Name = item.Name,
                    RelatedType = item.RelatedType,
                    Node = this,
                    
                };
            }
            foreach (var binding in Bindings)
                yield return binding;
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

    [Obsolete] // Still used for upgrading old versions
    public string ForAssemblyQualifiedName { get; set; }

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
            ForAssemblyQualifiedName = cls["ForAssemblyQualifiedName"].Value;
        }
        
        if (cls["ForElementIdentifier"] != null)
        {
            _forElementIdentifier = cls["ForElementIdentifier"].Value;
        }

        if (cls["BaseViewIdentifier"] != null)
        _baseViewIdentifier = cls["BaseViewIdentifier"].Value;
        if (cls["ComponentIdentifiers"] != null)
        _componentIdentifiers = cls["ComponentIdentifiers"].AsArray.DeserializePrimitiveArray(n => n.Value).ToList();

    }

    private void SetBindingMethods()
    {
        var element = ViewForElement;
        if (element != null)
        {
            var list = new List<MethodInfo>();
            var vmType = CurrentType;
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
        foreach (var binding in Bindings)
        {
            binding.Generator = null;
        }

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

public class ViewBindingData : DiagramNodeItem
{ 
    public override string FullLabel
    {
        get { return Name; }
    }

    public override void Remove(IDiagramNode diagramNode)
    {
        var viewData = Node as ViewData;
        if (viewData != null)
        {
            viewData.Bindings.Remove(this);
        }
    }

    public override string Label
    {
        get { return Name; }
    }

    public IBindingGenerator Generator { get; set; }
}