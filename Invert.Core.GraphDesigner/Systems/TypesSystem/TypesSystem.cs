using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Data;
using Invert.IOC;

namespace Invert.Core.GraphDesigner
{
    public class SelectTypeCommand : Command
    {
        public Func<IDiagramNodeItem, GraphTypeInfo[]> TypesSelector { get; set; }
        private List<GraphTypeInfo> _additionalTypes;
        public bool AllowNone { get; set; }
        public bool PrimitiveOnly { get; set; }
        public bool IncludePrimitives { get; set; }
        public List<GraphTypeInfo> AdditionalTypes
        {
            get { return _additionalTypes ?? (_additionalTypes = new List<GraphTypeInfo>()); }
            set { _additionalTypes = value; }
        }

        public TypedItemViewModel ItemViewModel { get; set; }
    }

    public class SetTypeCommand : Command
    {
        
    }
    public class TypesSystem : DiagramPlugin
        , IContextMenuQuery
        , IExecuteCommand<SelectTypeCommand>
    {
        public override void Loaded(UFrameContainer container)
        {
            base.Loaded(container);
            TypesInfo = InvertGraphEditor.TypesContainer.ResolveAll<GraphTypeInfo>().ToArray();
            Repository = container.Resolve<IRepository>();
        }

        public IRepository Repository { get; set; }

        public GraphTypeInfo[] TypesInfo { get; set; }

        public void QueryContextMenu(ContextMenuUI ui, MouseEvent evt, object obj)
        {
            var typedItem = obj as TypedItemViewModel;
            
            if (typedItem != null)
            {
                foreach (var item in TypesInfo)
                {
                    var item1 = item;
                    ui.AddCommand(new ContextMenuItem()
                    {
                        Title = item1.Name,
                        Group = item.Group,
                        Command = new LambdaCommand(() =>
                        {
                            typedItem.RelatedType = item1.Name;
                        })
                    });
                }
            }
        }


        public void Execute(SelectTypeCommand command)
        {
            InvertGraphEditor.WindowManager.InitItemWindow(GetRelatedTypes(command).ToArray(),_=>
            {
                command.ItemViewModel.RelatedType = _.Name;
            },command.AllowNone);
        }
        public virtual IEnumerable<GraphTypeInfo> GetRelatedTypes(SelectTypeCommand command)
        {
            if (command.AllowNone)
            {
                yield return new GraphTypeInfo() { Name = null, Group = "", Label = "[ None ]" };
            }
            if (command.IncludePrimitives)
            {
                var itemTypes = InvertGraphEditor.TypesContainer.ResolveAll<GraphTypeInfo>();
                foreach (var elementItemType in itemTypes)
                {
                    yield return elementItemType;
                }
            }
            foreach (var item in command.AdditionalTypes)
            {
                yield return item;
            }
  
            if (command.PrimitiveOnly) yield break;

            foreach (var item in Repository.AllOf<IClassTypeNode>())
            {
                if (item.Graph != null)
                yield return new GraphTypeInfo() { Name = item.Identifier, Group = item.Graph.Name, Label = item.Name };
            }
        }
    }
}
