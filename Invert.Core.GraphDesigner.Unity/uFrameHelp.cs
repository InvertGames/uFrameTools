using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.Core.GraphDesigner;
using UnityEditor;
using UnityEngine;

public class uFrameHelp : EditorWindow, IDocumentationBuilder, ICommandEvents
{
    public static Dictionary<string, Texture> ImageCache
    {
        get { return _imageCache ?? (_imageCache = new Dictionary<string, Texture>()); }
        set { _imageCache = value; }
    }

    public static Dictionary<string, string> ContentCache
    {
        get { return _contentCache ?? (_contentCache = new Dictionary<string, string>()); }
        set { _contentCache = value; }
    }
    public Texture GetImage(string url)
    {
        Texture texture;
        if (ImageCache.TryGetValue(url, out texture))
        {
            return texture;
        }
        ImageCache.Add(url, null);
        DownloadImage(url);
        if (ImageCache.TryGetValue(url, out texture))
        {
            return texture;
        }
        return ElementDesignerStyles.ArrowDownTexture;
    }
    public string GetContent(string url)
    {
        string texture;
        if (ContentCache.TryGetValue(url, out texture))
        {
            return texture;
        }
        ContentCache.Add(url, null);
        DownloadString(url);
        if (ContentCache.TryGetValue(url, out texture))
        {
            return texture;
        }
        return "Loading...";
    }
    public void DownloadImage(string url)
    {
        var ww = new WWW(url);
        while (!ww.isDone)
        {

        }
        ImageCache[url] = ww.texture;
        Repaint();
    }
    public void DownloadString(string url)
    {
        var ww = new WWW(url);
        while (!ww.isDone)
        {

        }
        ContentCache[url] = ww.text;
        Repaint();
    }
    [SerializeField]
    public string LastPage;
    private static IDocumentationProvider[] _documentationProvider;
    private static List<DocumentationPage> _pages;
    private Stack<DocumentationPage> _pageStack;
    [SerializeField]
    private Vector2 _tocScrollPosition;
    [SerializeField]
    private Vector2 _pageScrollPosition;
    private static Dictionary<string, Texture> _imageCache;
    private static GUIStyle _titleStyle;
    private static GUIStyle _paragraphStyle;
    private static Dictionary<string, string> _contentCache;
    private static GUIStyle _tutorialActionStyle;


    public static void ShowPage(Type pageType)
    {
        ShowWindow(FindPage(Pages, _ => _.GetType() == pageType));
        
    }
    public static void ShowPage(string name)
    {
        ShowWindow(FindPage(Pages, _ => _.Name == name));
    }

    [MenuItem("uFrame/Documentation")]
    public static void ShowWindow()
    {
        var window = GetWindow<uFrameHelp>();
        window.title = "uFrame Help";
        window.minSize = new Vector2(400, 500);
        window.ShowUtility();
    }

    public static void ShowWindow(Type graphItemType)
    {
        if (graphItemType != null)
        {
            var page = Pages.FirstOrDefault(p => p.RelatedNodeType == graphItemType);
            ShowWindow(page);
        }
    }
    public static void ShowWindow(DocumentationPage page)
    {

        var window = GetWindow<uFrameHelp>();
        window.title = "uFrame Help";
        window.minSize = new Vector2(400, 500);

        if (page != null)
            window.ShowPage(page);
        window.ShowUtility();
    }
    private void ShowPage(DocumentationPage page)
    {
        LastPage = page.Name;
        PageStack.Push(page);
    }

    public static IDocumentationProvider[] DocumentationProvider
    {
        get
        {
            if (_documentationProvider != null) return _documentationProvider;


            _documentationProvider =
                InvertApplication.Container.ResolveAll<IDocumentationProvider>().ToArray();

            return _documentationProvider;
        }
        set { _documentationProvider = value; }
    }

    public static List<DocumentationPage> Pages
    {
        get
        {
            if (_pages == null)
            {
                _pages = new List<DocumentationPage>();
                foreach (var provider in DocumentationProvider)
                {
                    provider.GetPages(_pages);
                }
                
            }
            return _pages;
        }
        set { _pages = value; }
    }

    public static DocumentationPage FindPage(IEnumerable<DocumentationPage> inside, Predicate<DocumentationPage> predicate )
    {
        foreach (var page in inside)
        {
            if (predicate(page))
            {
                return page;
            }
            var result = FindPage(page.ChildPages, predicate);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
    public static void DrawTitleBar(string subTitle)
    {
        //GUI.Label();
        ElementDesignerStyles.DoTilebar(subTitle);
    }

    public void OnGUI()
    {
        if (disposer == null)
        {
            disposer = InvertApplication.ListenFor<ICommandEvents>(this);
        }
        GUIHelpers.IsInsepctor = false;
        // DrawTitleBar("uFrame Help");

        if (DocumentationProvider == null)
        {
            EditorGUILayout.HelpBox(string.Format("No Help Found"), MessageType.Info);
            return;
        }
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Sidebar", EditorStyles.toolbarButton))
        {
            EditorPrefs.SetBool("uFrameHelpSidebar", !EditorPrefs.GetBool("uFrameHelpSidebar", true));
        }
        if (PageStack.Count > 0)
        {
            if (GUILayout.Button("Back", EditorStyles.toolbarButton))
            {
                PageStack.Pop();
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        if (EditorPrefs.GetBool("uFrameHelpSidebar", true))
        {
            _tocScrollPosition = EditorGUILayout.BeginScrollView(_tocScrollPosition, GUILayout.Width(260));
            EditorGUI.DrawRect(new Rect(_tocScrollPosition.x, _tocScrollPosition.y, Screen.width, Screen.height), new Color(0.3f, 0.3f, 0.4f));
            ShowPages(Pages);
            EditorGUILayout.EndScrollView();
        }

     
            _pageScrollPosition = EditorGUILayout.BeginScrollView(_pageScrollPosition);
            EditorGUI.DrawRect(new Rect(_pageScrollPosition.x, _pageScrollPosition.y, Screen.width, Screen.height), new Color(0.8f, 0.8f, 0.8f));


            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            EditorGUILayout.BeginVertical();
            if (CurrentPage != null)
            {

                CurrentPage.GetContent(this);
                if (false)
                foreach (var childPage in CurrentPage.ChildPages)
                {
                    childPage.PageContent(this);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
   


        EditorGUILayout.EndHorizontal();
    }

    private void ShowPages(List<DocumentationPage> pages, int indent = 1)
    {

        EditorGUILayout.BeginVertical();
        foreach (var item in pages.OrderBy(p=>p.Order))
        {
            if (item == null)
            {
                GUILayout.Label("Item is null");
                continue;
            }
            if (item.Name == null)
            {
                GUILayout.Label(string.Format("{0} name is null", item.GetType().Name));
                continue;
            }
            if (item.ChildPages.Count == 0)
            {
                if (GUIHelpers.DoTriggerButton(new UFStyle(item.Name, ElementDesignerStyles.Item4)
                {
                    
                }))
                {
                    this.ShowPage(item);
                }
            }
            else
            {

                var item1 = item;
                if (GUIHelpers.DoToolbarEx(item.Name, null, null, null, () => { ShowPage(item1); }))
                {

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(indent * 15f);
                    //if (GUIHelpers.DoTriggerButton(new UFStyle(item.Name,ElementDesignerStyles.EventButtonStyleSmall,ElementDesignerStyles.TriggerInActiveButtonStyle)))
                    //{
                    //    PageStack.Push(item);
                    //}

                    ShowPages(item.ChildPages, indent + 1);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndVertical();

    }

    public DocumentationPage CurrentPage
    {
        get
        {
            if (PageStack.Count < 1)
            {
                if (LastPage != null)
                {
                    var page = FindPage(Pages, p=>p.Name == LastPage);
                    if (page != null)
                    {
                        return page;
                    }
                }
                return Pages.FirstOrDefault();
            }


            return PageStack.Peek();
        }
    }
    public Stack<DocumentationPage> PageStack
    {
        get { return _pageStack ?? (_pageStack = new Stack<DocumentationPage>()); }
        set { _pageStack = value; }
    }

    public void BeginArea(string id)
    {

    }

    public void EndArea()
    {

    }

    public void BeginSection(string id)
    {

    }

    public void EndSection()
    {

    }

    public void PushIndent()
    {

    }

    public void PopIndent()
    {

    }

    public void LinkToNode(IDiagramNodeItem node, string text = null)
    {

    }

    public void NodeImage(DiagramNode node)
    {

    }

    public void Paragraph(string text, params object[] args)
    {
        GUILayout.Space(5f);
        if (args == null || args.Length == 0)
        {
            GUILayout.Label(text, ParagraphStyle);
        }
        else
        {
            GUILayout.Label(string.Format(text, args), ParagraphStyle);
        }
      
        
    }

    public void Break()
    {
        this.Paragraph(string.Empty);
    }

    public static GUIStyle TitleStyle
    {
        get
        {
            return _titleStyle ?? (_titleStyle = new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = new Color(0.3f, 0.3f, 0.4f) },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                });
        }

    }
    public static GUIStyle TutorialActionStyle
    {
        get
        {
            return _tutorialActionStyle ?? (_tutorialActionStyle = new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.red },
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            });
        }

    }
    public static GUIStyle ParagraphStyle
    {
        get
        {
            return _paragraphStyle ?? (_paragraphStyle = new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = new Color(0.2f, 0.2f, 0.2f) }
                ,
                fontSize = 12,
                wordWrap = true
            });
        }

    }
    public void Lines(params string[] lines)
    {
        foreach (var item in lines)
        {
            GUILayout.Label(item, ParagraphStyle);
        }
    }

    public void Title(string text, params object[] args)
    {
        GUILayout.Space(10f);
        TitleStyle.fontSize = 20;
        GUILayout.Label(text, TitleStyle);
        GUILayout.Space(10f);
    }

    public void Title2(string text, params object[] args)
    {
        GUILayout.Space(8f);
        TitleStyle.fontSize = 16;
        GUILayout.Label(text, TitleStyle);
    }

    public void Title3(string text, params object[] args)
    {
        GUILayout.Space(5f);
        TitleStyle.fontSize = 14;
        GUILayout.Label(text, TitleStyle);

    }

    public void Note(string text, params object[] args)
    {
        EditorGUILayout.HelpBox(text,MessageType.Info);
    }

    public void TemplateLink()
    {

    }

    public void Literal(string text, params object[] args)
    {

    }

    public void Section(string text, params object[] args)
    {

    }

    public void Rows(params Action[] actions)
    {

    }

    public void Columns(params Action[] actions)
    {

    }

    public bool DrawImage(string url, params object[] args)
    {
        var finalUrl = string.Format(url, args);
        var texture = GetImage(finalUrl);
        if (texture != null)
        {
            var rect = GUILayoutUtility.GetRect(texture.width, texture.width, texture.height, texture.height);
            rect = new Rect(rect.x, rect.y, texture.width, texture.height);
            return GUI.Button(rect, texture);
        }
        return false;

    }
    public void YouTubeLink(string id)
    {

        if (this.DrawImage("http://img.youtube.com/vi/{0}/mqdefault.jpg", id))
        {
            Application.OpenURL(string.Format("https://www.youtube.com/watch?v={0}", id));
        }
    }

    public void TemplateExample<TTemplate, TData>(TData data, bool isDesignerFile = true, params string[] members)
        where TTemplate : class, IClassTemplate<TData>, new() where TData : class, IDiagramNodeItem
    {
        
        var tempProject = new TemporaryProjectRepository(data.Node.Graph);

        var template = new TemplateClassGenerator<TData, TTemplate>()
        {
            Data = data,
            IsDesignerFile = isDesignerFile,
            // If we don't have any make sure its null
            FilterToMembers =members != null && members.Length > 0 ? members : null
        };
        var name = "Example " + data.Name;
        if (members != null)
        {
            name += string.Join(", ", members);
        }
        name += ".cs";
        if (GUIHelpers.DoToolbarEx(name,null,null,null,null,false,Color.black))
        {
            var codeFileGenerator = new CodeFileGenerator
            {
                Generators = new OutputGenerator[] { template },
                RemoveComments = true
            };
            EditorGUILayout.TextArea(codeFileGenerator.ToString());
        }
      

    }

    public void ShowGist(string id, string filename, string userId = "micahosborne")
    {
        GUILayout.Space(10f);

        //https://gist.githubusercontent.com/micahosborne/5e04f3cbbd28094edaf5/raw/
        if (GUIHelpers.DoToolbarEx("Gist: " + filename, null, null, null, null, false, Color.black))
        {
            EditorGUILayout.TextArea(
                GetContent(string.Format("https://gist.githubusercontent.com/{1}/{0}/raw", id, userId)));
        }

    }
    /// <summary>
    /// Show a tutorial step, and if it returns true, it is the current step.
    /// </summary>
    /// <param name="step"></param>
    /// <param name="stepContent"></param>
    /// <returns></returns>
    public bool ShowTutorialStep(ITutorialStep step, Action<IDocumentationBuilder> stepContent = null)
    {

        CurrentTutorial.Steps.Add(step);

        if (CurrentTutorial.LastStepCompleted == false) return false;


        var result = step.IsDone();
        if (result == null)
        {
            CurrentTutorial.LastStepCompleted = true;
            //TutorialActionStyle.fontSize = 13;
            //TutorialActionStyle.normal.textColor = new Color(0.3f, 0.6f, 0.3f);
            //GUILayout.Label(string.Format("Step {0}: Complete", CurrentTutorial.Steps.IndexOf(step) + 1),
            //    TutorialActionStyle);
            return false;
        }
        else
        {
            CurrentTutorial.LastStepCompleted = false;
        }
        Title2("Study Material");

        if (stepContent != null)
            stepContent(this);
        else if (step.StepContent != null)
        {
            step.StepContent(this);
        }
        Break();
        Break();
        Break();
        Title(string.Format("Step {0}: {1}", CurrentTutorial.Steps.IndexOf(step) + 1, step.Name)); 
        Break();
        Title2("Step Trouble Shooting");

        TutorialActionStyle.fontSize = 12;
        TutorialActionStyle.normal.textColor = Color.red;

        GUILayout.Label(result, TutorialActionStyle);
        Break();







        return true;

    }
    public InteractiveTutorial CurrentTutorial { get; set; }

    public void BeginTutorial(string name)
    {
        CurrentTutorial = new InteractiveTutorial(name);
    }

    public void EndTutorial()
    {
        if (CurrentTutorial.LastStepCompleted)
        {
            
            TutorialActionStyle.fontSize = 20;
            TutorialActionStyle.normal.textColor = new Color(0.3f, 0.6f, 0.3f);
            GUILayout.Label("Contratulations, you've completed this tutorial.", TutorialActionStyle);
        }
        CurrentTutorial = null;
    }

    public void ImageByUrl(string empty)
    {
        DrawImage(empty);
    }

    public void CodeSnippet(string code)
    {
        Paragraph(code);
    }

    public void ToggleContentByNode<TNode>(string name)
    {
        var page = FindPage(Pages, p => p.RelatedNodeType == typeof(TNode));
        if (GUIHelpers.DoToolbarEx(name ?? page.Name,null,null,null,null,false,Color.black))
        {
            page.GetContent(this);
        }
        
    }
    public void ToggleContentByPage<TPage>(string name)
    {
        var page = FindPage(Pages, p => p is TPage);
        if (GUIHelpers.DoToolbarEx(name ??page.Name, null, null, null, null, false, Color.black))
        {
            page.GetContent(this);
        }

    }

    public void ContentByNode<TNode>()
    {
        var page = FindPage(Pages, p => p.RelatedNodeType == typeof(TNode));
        page.GetContent(this);
    }
    public void ContentByPage<TPage>()
    {
        var page = FindPage(Pages, p => p is TPage);
        page.GetContent(this);
    }

    public void LinkToPage<TPage>()
    {
        var page = FindPage(Pages, p => p is TPage);
        if (GUILayout.Button(page.Name))
        {
            ShowPage(page);
            
        }
    }

    public void TemplateExample<TTemplate, TData>(TData data, bool designerFile = true, string templateMember = null) where TTemplate : IClassTemplate<TData>
    {

        //var a = Activator.CreateInstance(typeof (TTemplate)) as IClassTemplate<TData>;
        //a.Ctx = new TemplateContext<TData>(typeof(TTemplate))
        //{
        //    DataObject = data as IDiagramNodeItem,
        //    IsDesignerFile = designerFile, 
        //    CurrentDecleration = new CodeTypeDeclaration()

        //};
        //  var context = new TemplateContext<TData>(TemplateType);

        //    context.DataObject = Data;
        //    context.Namespace = Namespace;
        //    context.CurrentDecleration = Decleration;
        //    context.IsDesignerFile = IsDesignerFile;

        //a.Ctx.RenderTemplateMethod(a,templateMember);
        //a.Ctx.Render
    }

    public void CommandExecuting(ICommandHandler handler, IEditorCommand command, object o)
    {

    }

    public void CommandExecuted(ICommandHandler handler, IEditorCommand command, object o)
    {
        Repaint();
    }

    private Action disposer;
    public void OnDestory()
    {
        if (disposer != null)
        {
            disposer();
        }
    }

    
}