using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Invert.Common.UI;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


public class SelectedCodePreview : EditorWindow
{
    private List<IDrawer> _generatorDrawers;
    private CodeFileGenerator[] fileGenerators;
    private Vector2 _scrollPosition;

    [MenuItem("uFrame/Code Preview Window #&p")]
    internal static void ShowWindow()
    {
        var window = GetWindow<SelectedCodePreview>();
        window.title = "Code Preview";
        // window.minSize = new Vector2(400, 500);

        window.Show();
    }

    public void OnGUI()
    {
        if (Issues)
        {
            EditorGUILayout.HelpBox("Fix Errors First", MessageType.Info);
        }
        if (GeneratorDrawers != null)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            var rect = new Vector2(0f, 28f);
            foreach (var fileGenerator in GeneratorDrawers)
            {
                if (GUIHelpers.DoToolbarEx(fileGenerator.ViewModelObject.Name))
                {
                    var lastRect = new Rect(0f, 0f, Screen.width, Screen.height);

                    fileGenerator.Refresh(InvertGraphEditor.PlatformDrawer, rect);
                    rect.y += fileGenerator.Bounds.height;
                    GUILayoutUtility.GetRect(Screen.width, fileGenerator.Bounds.height);
                    fileGenerator.Draw(InvertGraphEditor.PlatformDrawer, 1f);
                    //EditorGUILayout.TextArea(fileGenerator.ToString());
                }
                rect.y += 28f;
            }
            GUILayout.EndScrollView();
        }

    }

    public void Update()
    {
        var vm = InvertGraphEditor.CurrentDiagramViewModel;
        if (vm == null) return;

        if (SelectedNode != InvertGraphEditor.CurrentDiagramViewModel.SelectedNode || SelectedNode == null)
        {

            SelectedItemChanged();
            Repaint();
        }

    }
    public List<IDrawer> GeneratorDrawers
    {
        get { return _generatorDrawers ?? (_generatorDrawers = new List<IDrawer>()); }
        set { _generatorDrawers = value; }
    }

    private void SelectedItemChanged()
    {

        GeneratorDrawers.Clear();
        fileGenerators = null;

        SelectedNode = InvertGraphEditor.CurrentDiagramViewModel.SelectedGraphItem;

        if (SelectedNode == null)
        {
            return;
        }
        //Issues = SelectedNode.Issues.Any(p => p.Siverity == ValidatorType.Error);
        //if (Issues) return;
        var item = SelectedNode == null ? null : SelectedNode.DataObject;

        fileGenerators = InvertGraphEditor.GetAllFileGenerators(null,
            InvertGraphEditor.DesignerWindow.DiagramViewModel.CurrentRepository, true).ToArray();

        foreach (var fileGenerator in fileGenerators)
        {
            var list = fileGenerator.Generators.ToList();
            if (item != null)
                list.RemoveAll(p => p.ObjectData != item);
            fileGenerator.Generators = list.ToArray();
            if (fileGenerator.Generators.Length < 1) continue;

            var syntaxViewModel = new SyntaxViewModel(fileGenerator.ToString(), fileGenerator.Generators[0].Filename, 0);
            var syntaxDrawer = new SyntaxDrawer(syntaxViewModel);

            GeneratorDrawers.Add(syntaxDrawer);
        }

    }

    public bool Issues { get; set; }

    public GraphItemViewModel SelectedNode { get; set; }
}

public class DocumentationWindow : EditorWindow
{
    private List<IDrawer> _generatorDrawers;
    private CodeFileGenerator[] fileGenerators;
    private Vector2 _scrollPosition;
    private DiagramDrawer drawer;

    private List<DiagramNode> _screenshots;
    private int _currentScreenshotIndex = 0;
    private bool _capturing = false;
    private bool _exitOnComplete = false;

    internal static void ShowWindow()
    {
        var window = GetWindow<DocumentationWindow>();
        window.title = "Documentation Window";
        window._currentScreenshotIndex = 0;

        // window.minSize = new Vector2(400, 500);
        //window.drawer = window.DiagramDrawer();
        window.ShowPopup();
    }
    internal static void ShowWindowAndGenerate()
    {
        var window = GetWindow<DocumentationWindow>();
        window.title = "Documentation Window";
        window._currentScreenshotIndex = 0;

        // window.minSize = new Vector2(400, 500);
        //window.drawer = window.DiagramDrawer();
        window.ShowPopup();
        var repository = InvertGraphEditor.DesignerWindow.DiagramViewModel.CurrentRepository;
        window._screenshots = repository.AllGraphItems.OfType<DiagramNode>().ToList();
        window._capturing = true;
        window._exitOnComplete = true;
        window.NextScreenshot();
    }
    public void OnGUI()
    {
        if (!_capturing)
        {
            if (GUILayout.Button("Generate Documentation"))
            {
                var repository = InvertGraphEditor.DesignerWindow.DiagramViewModel.CurrentRepository;
                _screenshots = repository.AllGraphItems.OfType<DiagramNode>().ToList();
                _capturing = true;
                NextScreenshot();
            }
            return;
        }

        if (drawer == null) return;
        foreach (var item in drawer.Children)
        {
            //if (!(item is DiagramNodeDrawer)) continue;
            if (item is ScreenshotNodeDrawer) continue;
            item.Draw(InvertGraphEditor.PlatformDrawer, 1f);
        }


        if (Event.current.type == EventType.Repaint)
        {
            Texture2D texture2D = new Texture2D(Mathf.RoundToInt(this.position.width), Mathf.RoundToInt(this.position.height), (TextureFormat)3, false);

            texture2D.ReadPixels(new Rect(0f, 0f, this.position.width, this.position.height), 0, 0);
            texture2D.Apply();
            byte[] bytes = texture2D.EncodeToPNG();
            Object.DestroyImmediate((Object)texture2D, true);
            var directory = Path.Combine("Documentation", "Screenshots");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes(Path2.Combine("Documentation", "Screenshots", _screenshots[_currentScreenshotIndex - 1].Name + ".png"), bytes);
            Debug.Log("Saved image " + _screenshots[_currentScreenshotIndex - 1].Name + ".png");
            NextScreenshot();
        }

        //this.Close();
    }

    private void NextScreenshot()
    {
        if (_currentScreenshotIndex >= _screenshots.Count)
        {
            _capturing = false;
            _currentScreenshotIndex = 0;
            if (_exitOnComplete)
            {
                _exitOnComplete = false;
                this.Close();
                return;
            }
            Repaint();
            return;
        }
        drawer = DiagramDrawer(_screenshots[_currentScreenshotIndex]);

        _currentScreenshotIndex++;
        if (drawer == null)
        {
            NextScreenshot();
            return;
        }
        Repaint();
    }

    private DiagramDrawer DiagramDrawer(DiagramNode node)
    {
        var window = InvertGraphEditor.DesignerWindow as ElementsDesigner;

        var diagramViewModel = new DiagramViewModel(node.Graph, window.DiagramViewModel.CurrentRepository);
        diagramViewModel.NavigateTo(node.Identifier);


    
        var drawer = new DiagramDrawer(diagramViewModel);
        drawer.Refresh(InvertGraphEditor.PlatformDrawer);

        var screenshotVM = diagramViewModel.AllViewModels.OfType<DiagramNodeViewModel>().FirstOrDefault(p => p.GraphItemObject.Identifier == node.Identifier);


        if (screenshotVM == null)
            return null;
        this.position = new Rect(this.position.x, this.position.y, screenshotVM.Bounds.width + 20f, screenshotVM.Bounds.height + 20f);
        var position = screenshotVM.Position - new Vector2(10f,10f);
        Debug.Log(diagramViewModel.CurrentRepository.CurrentFilter.Name + " " + position.x + ": " + position.y);
        foreach (var item in drawer.Children.OrderBy(p => p.ZOrder))
        {
           
            
            //item.Refresh(InvertGraphEditor.PlatformDrawer, new Vector2(item.Bounds.x - screenshotVM.Bounds.x, item.Bounds.y - screenshotVM.Bounds.y));
            if (item == null) continue;
            if (item.ViewModelObject != null)
            {
                item.IsSelected = false;
                item.ViewModelObject.ShowHelp = true;
            }
            

            item.Bounds = new Rect(item.Bounds.x - position.x, item.Bounds.y - position.y,
                item.Bounds.width, item.Bounds.height);

            foreach (var child in item.Children)
            {
                if (child == null) continue;
                child.Bounds = new Rect(child.Bounds.x - position.x, child.Bounds.y - position.y,
                    child.Bounds.width, child.Bounds.height);
                if (child.ViewModelObject != null)
                {
                    var cb = child.ViewModelObject.ConnectorBounds;

                    child.ViewModelObject.ConnectorBounds = new Rect(cb.x - position.x, cb.y - position.y,
                        cb.width, cb.height);
                }

                //child.Refresh(InvertGraphEditor.PlatformDrawer, new Vector2(item.Bounds.x - screenshotVM.Bounds.x, item.Bounds.y - screenshotVM.Bounds.y));
            }
        }
        return drawer;
    }
}