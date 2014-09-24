using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Invert.Common;
using Invert.Common.UI;
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

                _ViewData = uFrameEditor.CurrentDiagramViewModel.SelectedNode as ViewNodeViewModel;
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
            EditorGUILayout.TextArea(_ViewData.Preview, GUILayout.Height(Screen.height - 55f));

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        GUILayout.Space(13);
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        GUIHelpers.DoToolbar("Bindings");
        _bindingsScrollPosition = EditorGUILayout.BeginScrollView(_bindingsScrollPosition);
        DoCurrentBindings();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

    }

    private void DoCurrentBindings()
    {
         foreach (var item in _ViewData.Bindings.ToArray())
        {
            if (GUIHelpers.DoTriggerButton(new UFStyle()
            {
                Label = item.Name,
                SubLabel = item.GeneratorType,
                Tag = item,
                BackgroundStyle = UBStyles.EventButtonStyleSmall,
                //SubLabel = item.Description,
                FullWidth = false,
                IsWindow = false,
                Enabled = true
            }))
            {
                LastSelected = item;
                _ViewData.RemoveBinding(item);
                ApplySearch();
            }
        }
    }

    public void Apply()
    {
        //_ViewData.NewBindings.AddRange(AddedGenerators.Select(p=>p.Tag as IBindingGenerator));
    }

    public override void OnGUIScrollView()
    {

        foreach (var group in Items.Where(p => !_ViewData.NewBindings.Contains(p.Tag as IBindingGenerator) && p.Enabled == true).GroupBy(p => p.Group))
        {
            GUIHelpers.DoToolbar(group.Key);
            foreach (var item in group)
            {
                if (GUIHelpers.DoTriggerButton(item))
                {
                    uFrameEditor.ExecuteCommand(n =>
                    {
                        LastSelected = _ViewData.AddNewBinding(item.Tag as IBindingGenerator);
                    });
                    
              
                }
            }
        }
    }


    protected override void ApplySearch()
    {
        if (_ViewData == null) return;
        
        Generators = _ViewData.BindingGenerators;

        //Where(p => _MemberMethods.FirstOrDefault(x => x.Name == p.MethodName) != null)
        Items = Generators.Select(item => new UFStyle()
        {
            Label = item.MethodName,
            Tag = item,
            BackgroundStyle = UBStyles.EventButtonStyle,
            SubLabel = item.Description,
            Group = item.Title,
            FullWidth = true,
            IsWindow = true,
            Enabled = _ViewData.Bindings.FirstOrDefault(p=>p.GeneratorType == item.GetType().Name && item.MethodName == p.Name) == null
        }).ToArray();


    }

    
    public IEnumerable<IBindingGenerator> Generators { get; set; }
}