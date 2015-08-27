using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Systems.GraphUI
{
    public interface IQueryDesignerWindowModalContent
    {
        void QueryDesignerWindowModalContent(List<DesignerWindowModalContent> content);
    }

    public class DesignerWindowModalContent
    {
        public Action<Rect> Drawer { get; set; }
        public int ZIndex { get; set; }
    }

}
