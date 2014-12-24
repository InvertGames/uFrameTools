using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public abstract class TypedItemViewModel : ItemViewModel<ITypedItem>
    {
        public static Dictionary<string, string> TypeNameAliases = new Dictionary<string, string>()
    {
        {"Int32","int"},
        {"Boolean","bool"},
        {"Char","char"},
        {"Decimal","decimal"},
        {"Double","double"},
        {"Single","float"},
        {"String","string"},
    };
        public static string TypeAlias(string typeName)
        {
            if (typeName == null)
            {
                return " ";
            }
            if (TypeNameAliases.ContainsKey(typeName))
            {
                return TypeNameAliases[typeName];
            }
            return typeName;
        }

        protected TypedItemViewModel(ITypedItem viewModelItem, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            DataObject = viewModelItem;
        }

 
        public string RelatedType
        {
            get
            {

                return TypeAlias(Data.RelatedTypeName);//ElementDataBase.TypeAlias(Data.RelatedType);
            }
            set
            {
                Data.RelatedType = value;
            }
        }

        public virtual string TypeLabel
        {
            get { return RelatedType; }
        }
    }
}