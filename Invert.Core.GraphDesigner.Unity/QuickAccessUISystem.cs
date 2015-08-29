using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Systems.GraphUI;
using UnityEditor;
using UnityEngine;

namespace Assets.UnderConstruction.Editor
{
    public class QuickAccessUISystem : DiagramPlugin,
        IQueryDesignerWindowOverlayContent, 
        IOverlayDrawer, 
        IEnableQuickAccess, 
        IDisableQuickAccess, 
        IUpdateQuickAccess
    {
        private TreeViewModel _treeModel;
        private IPlatformDrawer _platrformDrawer;
        private bool _focusNeeded;

        public Vector2? RequestPosition {get; set; }
        public QuickAccessContext Context { get; set; }
        public bool EnableContent { get; set; }
        public TreeViewModel TreeModel
        {
            get { return _treeModel ?? (_treeModel = ConstructViewModel(Context)); }
            set { _treeModel = value; }
        }


        public void QueryDesignerWindowOverlayContent(List<DesignerWindowOverlayContent> content)
        {
            if (EnableContent)
            content.Add(new DesignerWindowOverlayContent()
            {
                Drawer = this,
                DisableTransparency = true
            });
        }

        public void Draw(Rect bouds)
        {

            HandleInput(bouds);

            if (!EnableContent) return;

            if(TreeModel.IsDirty) TreeModel.Refresh();

            var searcbarRect = bouds.WithHeight(50).Pad(15, 15, 60, 30);
            var listRect = bouds.Below(searcbarRect).Clip(bouds).PadSides(15).Translate(0,-7);
            var searchIconRect = new Rect().WithSize(17, 17).AlignHorisonallyByCenter(searcbarRect).RightOf(searcbarRect).Translate(10, 0);

            PlatformDrawer.DrawImage(searchIconRect, "SearchIcon", true);

            GUI.SetNextControlName("QuickAccess_Search");
            EditorGUI.BeginChangeCheck();
            SearchCriteria = GUI.TextField(searcbarRect, SearchCriteria ?? "");
            if (EditorGUI.EndChangeCheck())
            {
                if (string.IsNullOrEmpty(SearchCriteria))
                {
                    TreeModel.Predicate = null;
                }
                else
                {
                    TreeModel.Predicate = i => i.Title.Contains(SearchCriteria);
                }
            }

            InvertApplication.SignalEvent<IDrawTreeView>(_ =>
            {
                _.DrawTreeView(listRect, TreeModel, (m, i) => { SelectItem(i); });
            });


            if(_focusNeeded)
                GUI.FocusControl("QuickAccess_Search");
 
        }

        private void HandleInput(Rect rect)
        {
            var evt = Event.current;
            if (evt == null) return;

            if (evt.isMouse && !rect.Contains(evt.mousePosition))
            {
                DisableQuickAcess();
                return;
            }

            if (evt.isKey && evt.rawType == EventType.KeyUp)
            {
                switch (evt.keyCode)
                {
                        case KeyCode.Escape:
                            DisableQuickAcess();
                        break;
                }
            }
            
        }

        public string SearchCriteria { get; set; }

        public IPlatformDrawer PlatformDrawer
        {
            get { return _platrformDrawer ?? (_platrformDrawer = InvertApplication.Container.Resolve<IPlatformDrawer>()); }
            set { _platrformDrawer = value; }
        }

        public const int QuickAccessWidth = 300;
        public const int QuickAccessHeigth = 300;

        public Rect CalculateBounds(Rect diagramRect)
        {
            if (RequestPosition.HasValue) return new Rect().WithSize(QuickAccessWidth, QuickAccessHeigth).WithOrigin(RequestPosition.Value.x, RequestPosition.Value.y);

            return new Rect().WithSize(QuickAccessWidth, QuickAccessHeigth).CenterInsideOf(diagramRect);
        }

        [MenuItem("uFrame/Quick Shot &z")]
        public static void OpenQuickAccess()
        {
            InvertApplication.SignalEvent<IEnableQuickAccess>(_=>_.EnableQuickAccess(new QuickAccessContext()
            {
                ContextType = typeof(IInsertQuickAccessContext)
            }));
        }

        public void EnableQuickAccess(QuickAccessContext context, Vector2? position = null)
        {
            RequestPosition = position;
            Context = context;
            TreeModel = null;
            EnableContent = true;
            _focusNeeded = true;
        }

        public void DisableQuickAcess()
        {
            RequestPosition = null;
            Context = null;
            TreeModel = null;
            SearchCriteria = null;
            EnableContent = false;
        }

        public void UpdateQuickAcess(QuickAccessContext context)
        {
            Context = context;
            TreeModel = null;
        }

        public void SelectItem(IItem i)
        {
            var item = i as QuickAccessItem;
            if (item == null) return;

            InvertApplication.Execute(new LambdaCommand("Select Item", () =>
            {
                item.Action(item.Item);
            }));

            DisableQuickAcess();
        }

        protected TreeViewModel ConstructViewModel(QuickAccessContext context)
        {
            var result = new TreeViewModel();
            var items = new List<IEnumerable<QuickAccessItem>>();
            Signal<IQuickAccessEvents>(_ => _.QuickAccessItemsEvents(context, items));
            result.Data = items.SelectMany(i => i).OfType<IItem>().ToList();
            result.Submit = SelectItem;
            


            return result;
        }

    }
}
