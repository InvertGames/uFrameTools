using System;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity.WindowsPlugin
{
    public class ConsoleDrawer : Drawer<ConsoleViewModel>
    {
        public ConsoleDrawer(ConsoleViewModel viewModelObject) : base(viewModelObject)
        {

        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);

            var messageRect = new Rect(5, 5, 100, 30);

            foreach (var messages in ViewModel.Messages)
            {

                var message = string.Format("{0} : {1}", Enum.GetName(typeof (MessageType), messages.MessageType), 
                    messages.Message);

                platform.DrawLabel(messageRect,message,CachedStyles.HeaderTitleStyle);
        
                messageRect = new Rect(messageRect)
                {
                    y = messageRect.y + messageRect.height
                };
            }
        
        }
    }
}