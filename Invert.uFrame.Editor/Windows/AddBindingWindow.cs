using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class AddBindingWindow : SearchableScrollWindow
{
    private ViewNodeViewModel _ViewData;

    private MethodInfo[] _MemberMethods;
    private List<UFStyle> _addedGenerators = new List<UFStyle>();
    private Vector2 _previewScrollPosition = new Vector2();
    private Vector2 _bindingsScrollPosition;
    public UFStyle[] Items { get; set; }

    public ElementsDesigner ElementsDesigner
    {
        get { return EditorWindow.GetWindow<ElementsDesigner>(); }
    }

    //public List<UFStyle> AddedGenerators
    //{
    //    get { return _addedGenerators; }
    //    set { _addedGenerators = value; }
    //}

    public static void Init(string title, ViewNodeViewModel data)
    {
        // Get existing open window or if none, make a new one:
        var window = (AddBindingWindow)GetWindow(typeof(AddBindingWindow));
        window.title = title;
        window._ViewData = data;
        window.ApplySearch();
        window.minSize = new Vector2(500, 300);
        window.Show();


    }

    public ViewBindingData LastSelected { get; set; }

    public override void OnGUI()
    {

        if (_ViewData == null)
        {
            if (ElementsDesigner != null && ElementsDesigner.DiagramDrawer != null)
            {

                _ViewData = InvertGraphEditor.CurrentDiagramViewModel.SelectedNode as ViewNodeViewModel;
            }
            if (_ViewData == null)
            {
                EditorGUILayout.HelpBox("Selected a view first.", MessageType.Info);
                return;
            }
            else
            {
                ApplySearch();
            }
        }
        else
        {


        }

        if (!_ViewData.HasElement)
        {
            EditorGUILayout.HelpBox("This view must be associated with an element in order to add bindings.",
                MessageType.Error);
        }
        else if (_ViewData != null)
        {
            CallOnGui();
        }



    }

    public override bool AllowSearch
    {
        get { return false; }
    }

    public void CallOnGui()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(325));
        base.OnGUI();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        _previewScrollPosition = EditorGUILayout.BeginScrollView(_previewScrollPosition);
        GUIHelpers.DoToolbar(_ViewData.Name + " Preview");
        if (LastSelected != null)
            EditorGUILayout.LabelField(_ViewData.Preview,EditorStyles.textArea, GUILayout.Height(Screen.height - 55f));

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(GUILayout.Width(400));
        
        _bindingsScrollPosition = EditorGUILayout.BeginScrollView(_bindingsScrollPosition);
        DoCurrentBindings();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

    }

    private void DoCurrentBindings()
    {
        foreach (var group in _ViewData.AllBindings.GroupBy(p=>p.Generator != null ? "Bindings To Add" : "Current Bindings"))
        {
            if (GUIHelpers.DoToolbarEx(group.Key))
            {
                foreach (var item in group)
                {
                    if (GUIHelpers.DoTriggerButton(new UFStyle()
                    {
                        Label = item.Name,
                        SubLabel = item.Generator != null ? item.Generator.Description : item.GeneratorType,
                        Tag = item,
                        BackgroundStyle = UBStyles.EventButtonStyle,
                        FullWidth = false,
                        IsWindow = false,
                        Enabled = item.Node == _ViewData.GraphItem
                    }))
                    {
                        LastSelected = item;
                        _ViewData.RemoveBinding(item);
                        ApplySearch();
                    }
                }
            }
           
            
        }
    }

    public void Apply()
    {

    }

    public override void OnGUIScrollView()
    {

        foreach (var group in Items.Where(p => !_ViewData.NewBindings.Contains(p.Tag as IBindingGenerator) && p.Enabled == true).GroupBy(p => p.Group))
        {
            
            if (GUIHelpers.DoToolbarEx(group.Key))
            {
                foreach (var item in group)
                {
                    if (GUIHelpers.DoTriggerButton(item))
                    {
                        UFStyle item1 = item;
                        InvertGraphEditor.ExecuteCommand(n =>
                        {
                            LastSelected = _ViewData.AddNewBinding(item1.Tag as IBindingGenerator);
                            ApplySearch();
                        });
                    }
                }
            }
        }
    }


    protected override void ApplySearch()
    {
        if (_ViewData == null) return;

        Generators = _ViewData.BindingGenerators;
        var bindings = _ViewData.Bindings.Select(p => p.Property).ToArray();

        //Where(p => _MemberMethods.FirstOrDefault(x => x.Name == p.MethodName) != null)
        Items = Generators.Where(p=>!bindings.Contains(p.Item)).Select(item => new UFStyle()
        {
            Label = item.MethodName,
            Tag = item,
            BackgroundStyle = UBStyles.EventButtonStyleSmall,
            //SubLabel = item.Description,
            Group = item.Title,
            FullWidth = false,
            IsWindow = true,
            Enabled = _ViewData.Bindings.FirstOrDefault(p=>p.Name == item.MethodName) == null
        }).ToArray();


    }


    public IEnumerable<IBindingGenerator> Generators { get; set; }
}