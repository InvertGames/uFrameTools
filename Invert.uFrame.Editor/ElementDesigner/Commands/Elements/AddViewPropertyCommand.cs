using System.Linq;
using System.Reflection;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddViewPropertyCommand : EditorCommand<ViewNodeViewModel>
    {
        public override void Perform(ViewNodeViewModel node)
        {
            var quickFind = new MemberInfo[]
            {
                typeof (Transform).GetMember("position").First(),
                typeof (Transform).GetMember("localPosition").First(),
                typeof (Transform).GetMember("rotation").First(),
                typeof (Transform).GetMember("localRotation").First(),
                typeof (Transform).GetMember("localScale").First(),
                typeof (Rigidbody).GetMember("velocity").First(),
                typeof (Rigidbody).GetMember("angularVelocity").First(),
                typeof (Rigidbody).GetMember("drag").First(),
                typeof (Rigidbody).GetMember("angularDrag").First(),
                typeof (Rigidbody).GetMember("mass").First(),
                typeof (Rigidbody).GetMember("useGravity").First(),


            };
            uFrameComponentSearchWindow.ShowWindow((w,m) =>
            {
                node.AddProperty(new ViewPropertyData()
                {
                    Name = m.Name,
                    ComponentProperty = m.Name,
                    ComponentTypeName = m.DeclaringType.FullName
                });


            }, (w,m) =>
            {
                node.RemoveProperty(m);
            },node.Properties.Select(p=>p.MemberInfo).ToArray(),quickFind);
           
        }

        public override string CanPerform(ViewNodeViewModel node)
        {
            if (node == null) return "Arg can't be null";
            return null;
        }
    }
}