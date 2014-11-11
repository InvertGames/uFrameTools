using System;
using System.CodeDom;

namespace Invert.uFrame.Editor
{
    public class LambdaMemberGenerator<TData> : MemberGenerator
    {
        public LambdaMemberGenerator(Func<LambdaMemberGenerator<TData>, CodeTypeMember> lambda)
        {
            Lambda = lambda;
        }

        public Func<LambdaMemberGenerator<TData>, CodeTypeMember> Lambda { get; set; }
        public bool IsDesignerFile { get; set; }

        public TData Data
        {
            get { return (TData)DataObject; }
        }

        public override CodeTypeMember Create(bool isDesignerFile)
        {
            IsDesignerFile = isDesignerFile;
            return Lambda(this);
        }
    }
}