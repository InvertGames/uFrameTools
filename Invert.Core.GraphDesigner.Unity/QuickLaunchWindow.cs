using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Invert.Common;
using Invert.IOC;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity
{
  
    public class QuickLaunchWindow : EditorWindow
    {
        public static GUIStyle TextFieldStyle
        {
            get
            {
                if (_textFieldStyle == null)
                    _textFieldStyle = new GUIStyle(EditorStyles.textField)
                    {
                        fontSize = 20,
                    };
                return _textFieldStyle;
            }
        }
     
        public static float width = 350;
        public static float height = 400;
        [SerializeField]
        private string searchText = string.Empty;

        private List<QuickLaunchItem> _quickLaunchItems;

        private static GUIStyle _textFieldStyle;
        private Vector2 scrollPosition;

        public static void ShowWindow(Vector2 position, Type launchType)
        {
            var quickLaunchWindow = CreateInstance<QuickLaunchWindow>();
            quickLaunchWindow.maxSize = new Vector2(width, height);
            quickLaunchWindow.position = new Rect(position.x,position.y, width, height);
            quickLaunchWindow.LaunchType = launchType;
            quickLaunchWindow.ShowPopup();
            quickLaunchWindow.Focus();
            //quickLaunchWindow.ShowAuxWindow();
        }

        public Type LaunchType { get; set; }
        
        public void OnLostFocus()
        {
            this.Close();
        }

        public float ItemHeight = 40;

        public void OnGUI()
        {
            HandleInput();
            EditorGUI.DrawRect(new Rect(0f,0f,Screen.width,Screen.height), InvertGraphEditor.Settings.GridLinesColorSecondary );
            EditorGUI.DrawRect(new Rect(1f,1f,Screen.width -2,Screen.height- 2), InvertGraphEditor.Settings.GridLinesColor );

            GUI.SetNextControlName("SearchField");
            EditorGUI.BeginChangeCheck();
            searchText = GUILayout.TextField(searchText, TextFieldStyle);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateSearch();
            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (var item in QuickLaunchItems)
            {
                var rect = EditorGUILayout.GetControlRect(false, ItemHeight);
                EditorGUI.DrawRect(rect, Color.black);
                GUI.Label(rect, item.Item.Title,ElementDesignerStyles.ClearItemStyle);
                //GUILayout.BeginArea(rect);
                
                //GUILayout.BeginHorizontal();
                //GUILayout.Label(item.Item.Group,ElementDesignerStyles.ClearItemStyle);
                //GUILayout.Label(item.Item.Title,ElementDesignerStyles.ItemTextEditingStyle);
                //GUILayout.EndHorizontal();
                //GUILayout.Label(item.Item.SearchTag,ElementDesignerStyles.ItemTextEditingStyle);
                
                //GUILayout.EndArea();
                
            }
            GUILayout.EndScrollView();

            GUI.FocusControl("SearchField");

        }

        private void UpdateSearch()
        {
            QuickLaunchItems.Clear();
            var launchItems = new List<IEnumerable<QuickLaunchItem>>();
            
            InvertApplication.SignalEvent<IQueryQuickLaunch>(_ => _.QueryQuickLaunch(LaunchType,launchItems));

            foreach (var item in launchItems.SelectMany(p=>p))
            {
                if (item.Item.SearchTag.Contains(searchText))
                {
                    QuickLaunchItems.Add(item);
                }
                if (QuickLaunchItems.Count >= 10)
                {
                    break;
                }
            }

        }

        public List<QuickLaunchItem> QuickLaunchItems
        {
            get { return _quickLaunchItems ?? (_quickLaunchItems = new List<QuickLaunchItem>()); }
            set { _quickLaunchItems = value; }
        }

        

        private void HandleInput()
        {
            var evt = Event.current;
            if (evt.isKey && evt.type == EventType.KeyUp)
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    this.Execute();
                    this.Close();
                }
                    
                if (evt.keyCode == KeyCode.Escape)
                    this.Close();
            }
        }

        private void Execute()
        {
            
        }
    }

    public interface IShowQuickLaunch
    {
        void Show(Type launchType);
    }

    public interface IQueryQuickLaunch
    {
        void QueryQuickLaunch(Type contextType, List<IEnumerable<QuickLaunchItem>> addItem);
   
    }

    public class QuickLaunchItem
    {
        public QuickLaunchItem(IItem item, Action<QuickLaunchItem> command)
        {
            Item = item;
            Command = command;
        }

        public IItem Item { get; set; }
        public Action<QuickLaunchItem> Command { get; set; }
    }
    public class QuickLaunchPlugin : DiagramPlugin, IShowQuickLaunch,IQueryQuickLaunch
    {
        [MenuItem("uFrame/Quick Find %t")]
        public static void ShowQuickLaunch()
        {
            InvertApplication.SignalEvent<IShowQuickLaunch>(_ => _.Show(typeof(IDiagramContextCommand)));
        }
        public override void Initialize(UFrameContainer container)
        {
            ListenFor<IShowQuickLaunch>();
            ListenFor<IQueryQuickLaunch>();
        }

        public void QueryQuickLaunch(Type contextType, List<IEnumerable<QuickLaunchItem>> addItem)
        {
            addItem.Add(NavigateToNodes());
        }

        private static IEnumerable<QuickLaunchItem> NavigateToNodes()
        {
            var ps =InvertApplication.Container.Resolve<ProjectService>();
            var repo = ps.CurrentProject;
            foreach (var item in repo.NodeItems)
            {
                yield return new QuickLaunchItem(item, _ =>
                {
                    
                });
            }
        }


        public void Show(Type launchType)
        {
            QuickLaunchWindow.ShowWindow(new Vector2((Screen.width /2f), Screen.height / 2f), launchType);
        }
    }
}
