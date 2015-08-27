using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Systems;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity.Notifications
{


    public class DesignerNotificationSystem : DiagramPlugin, INotify, IDesignerWindowEvents
    {
        private List<NotificationItem> _items;
        private IStyleProvider _styleProvider;

        private List<NotificationItem> Items
        {
            get { return _items ?? (_items = new List<NotificationItem>()); }
            set { _items = value; }
        }

        public IStyleProvider StyleProvider
        {
            get { return _styleProvider ?? (_styleProvider = InvertApplication.Container.Resolve<IStyleProvider>()); }
            set { _styleProvider = value; }
        }
        
        public IPlatformDrawer PlatformDrawer
        {
            get { return _platformDrawer ?? (_platformDrawer = InvertApplication.Container.Resolve<IPlatformDrawer>()); }
            set { _platformDrawer = value; }
        }

        public void Notify(string message, string icon)
        {
            var index = Enumerable.Range(0, 10).First(i => Items.All(n => n.Index != i));

            var notificationItem = new NotificationItem()
            {
                AnimatedX = 0,
                Icon = StyleProvider.GetImage(icon),
                Message = message,
                TimeLeft = 5000,
                Index = index
            };
            Items.Add(notificationItem);

            Signal<IDebugWindowEvents>(_ => _.RegisterInspectedItem(notificationItem, notificationItem.Message, true));
            //Add notification item to the queue
        }

        public void Notify(string message, NotificationIcon icon)
        {
            string iconCode = "InfoIcon";
            switch (icon)
            {
                case NotificationIcon.Info:
                    iconCode = "InfoIcon";
                    break;
                case NotificationIcon.Error:
                    iconCode = "InfoIcon";
                    break;
                case NotificationIcon.Warning:
                    iconCode = "InfoIcon";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("icon", icon, null);
            }
            Notify(message,iconCode);
        }

        public void AfterDrawGraph(Rect diagramRect)
        {

         
        }

        public void BeforeDrawGraph(Rect diagramRect)
        {
        }

        public void AfterDrawDesignerWindow(Rect windowRect)
        {
            _deltaTime = (float)(DateTime.Now - _lastUpdate).TotalMilliseconds;

            var firstUnpaddedItemRect = windowRect.WithSize(200, 100).InnerAlignWithBottomRight(windowRect).Translate(-15,-70);
            
            foreach (var item in Items.ToArray())
            {

                item.TimeLeft -= _deltaTime;
                if (item.TimeLeft < 0)
                {
                    Items.Remove(item);
                    continue;
                }

                var unpaddedItemRect = firstUnpaddedItemRect.AboveAll(firstUnpaddedItemRect, item.Index);

                if (item.TimeLeft > 4600) item.AnimatedX = Mathf.Lerp(400, 0, (5000 - item.TimeLeft) / 400);
                else if (item.TimeLeft < 400) item.AnimatedX = Mathf.Lerp(400, 0, item.TimeLeft/400);
                else item.AnimatedX = 0;

                var paddedItemRect = unpaddedItemRect.WithOrigin(unpaddedItemRect.x+item.AnimatedX, unpaddedItemRect.y).PadSides(15);

                PlatformDrawer.DrawStretchBox(paddedItemRect,CachedStyles.TooltipBoxStyle,14);
                paddedItemRect = paddedItemRect.PadSides(15);
                PlatformDrawer.DrawLabel(paddedItemRect.Pad(0,0,33,0),item.Message,CachedStyles.BreadcrumbTitleStyle,DrawingAlignment.MiddleLeft);
                PlatformDrawer.DrawImage(paddedItemRect.WithSize(33,33).InnerAlignWithBottomRight(paddedItemRect).AlignHorisonallyByCenter(paddedItemRect),item.Icon,true);


            }

            _lastUpdate = DateTime.Now;
        }

        public void DrawComplete()
        {
        }

        public void ProcessInput()
        {
        }

        private DateTime _lastUpdate;
        private float _deltaTime;
        private IPlatformDrawer _platformDrawer;

        internal class NotificationItem
        {
            public object Icon { get; set; }
            public string Message { get; set; }
            public float AnimatedX { get; set; }
            public float TimeLeft { get; set; }
            public int Index { get; set; }
        }
    
    }

    


}
