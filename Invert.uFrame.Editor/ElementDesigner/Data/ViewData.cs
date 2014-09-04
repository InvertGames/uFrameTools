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
    private string _forAssemblyQualifiedName;

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
    public string BaseViewName
    {
        get
        {
            var baseNode = BaseNode;
            var viewNode = baseNode as ViewData;
            if (viewNode != null)
            {
                return viewNode.NameAsView;
            }
            var baseView = BaseView;
            if (baseView != null)
            {
                return baseView.NameAsView;
            }
            var element = baseNode as ElementData;
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

    public override string AssemblyQualifiedName
    {
        get { return uFrameEditor.uFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", NameAsView); }
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
    public string ForAssemblyQualifiedName
    {
        get { return _forAssemblyQualifiedName; }
        set { _forAssemblyQualifiedName = value; }
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
            return uFrameEditor.uFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", NameAsView);
        }
    }

    //public DiagramNode BaseView
    //{
    //    get { return Data.Elements.FirstOrDefault(p => p.Identifier == BaseViewIdentifier); }
    //}

    public IDiagramNode BaseNode
    {
        get { return Data.NodeItems.FirstOrDefault(p => p.AssemblyQualifiedName == ForAssemblyQualifiedName); }
    }

    public ElementData ViewForElement
    {
        get
        {
            var bn = BaseNode;
            if (bn is ElementData)
                return bn as ElementData;
            
            var data = bn as ViewData;
            return data != null ? data.ViewForElement : null;
        }
    }

    public override bool CanCreateLink(IGraphItem target)
    {
        return target is ViewData && target != this;
    }

    public override void CreateLink(IDiagramNode container, IGraphItem target)
    {
        var i = target as ViewData;
        i.ForAssemblyQualifiedName = AssemblyQualifiedName;
        //i.BaseViewIdentifier = Identifier;
    }

    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameViewRefactorer(this);
    }

    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);
        _forAssemblyQualifiedName = cls["ForAssemblyQualifiedName"].Value;

        _baseViewIdentifier = cls["BaseViewIdentifier"].Value;
        _componentIdentifiers = cls["ComponentIdentifiers"].AsArray.DeserializePrimitiveArray(n => n.Value).ToList();

    }

    public override bool EndEditing()
    {
        var oldAssemblyName = AssemblyQualifiedName;
        if (!base.EndEditing()) return false;
        foreach (var v in Data.GetViews().Where(p => p.ForAssemblyQualifiedName == oldAssemblyName))
        {
            v.ForAssemblyQualifiedName = AssemblyQualifiedName;
        }
        return true;
    }

    public override IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] nodes)
    {
        var vm = ViewForElement;
        //var items = new UFrameBehaviours[] { };
        //if (vm != null)
        //{
        //    items = UBAssetManager.Behaviours.OfType<UFrameBehaviours>()
        //        .Where(p => p != null && p.ViewModelTypeString == vm.ViewModelAssemblyQualifiedName).ToArray();
        //}

        //Behaviours = items.Select(p => new BehaviourSubItem() { Behaviour = p }).ToArray();

        var baseView = nodes.FirstOrDefault(p => p.Identifier == BaseViewIdentifier);

        if (baseView != null)
        {
            yield return new ViewLink()
            {
                Element = baseView,
                Data = this
            };
        }

        var item = nodes.FirstOrDefault(p => p.AssemblyQualifiedName == ForAssemblyQualifiedName);
        if (item != null)
        {
            yield return new ViewLink()
            {
                Element = item,
                Data = this
            };
        }

        //SetBindingMethods();
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
        foreach (var source in Data.GetViews().Where(p => p.ForAssemblyQualifiedName == this.AssemblyQualifiedName))
        {
            source.ForAssemblyQualifiedName = null;
        }
    }

    //public BehaviourSubItem[] Behaviours { get; set; }
    public override void RemoveLink(IDiagramNode target)
    {
        var viewData = target as ViewData;
        if (viewData != null)
        {
            viewData.ForAssemblyQualifiedName = null;
            viewData.BaseViewIdentifier = null;
        }
        //viewData.BaseViewIdentifier = null;
        BaseViewIdentifier = null;
        //var elementData = target as ElementData;
        //if (target is )
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ForAssemblyQualifiedName", new JSONData(_forAssemblyQualifiedName));
        cls.Add("BaseViewIdentifier", new JSONData(_baseViewIdentifier));
        cls.AddPrimitiveArray("ComponentIdentifiers", _componentIdentifiers, i => new JSONData(i));
    }
}