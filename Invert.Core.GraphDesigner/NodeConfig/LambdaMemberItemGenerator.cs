using System;
using System.CodeDom;

namespace Invert.uFrame.Editor
{
    public class LambdaMemberItemGenerator<TData, TItem> : MemberItemGenerator<TItem>
    {
        public LambdaMemberItemGenerator(Func<LambdaMemberItemGenerator<TData, TItem>, CodeTypeMember> lambda)
        {
            Lambda = lambda;
        }
        public TData Data
        {
            get { return (TData)DataObject; }
        }
        public Func<LambdaMemberItemGenerator<TData, TItem>, CodeTypeMember> Lambda { get; set; }
        public bool IsDesignerFile { get; set; }
        public override CodeTypeMember Create(bool isDesignerFile)
        {
            IsDesignerFile = isDesignerFile;
            return Lambda(this);
        }
    }
}