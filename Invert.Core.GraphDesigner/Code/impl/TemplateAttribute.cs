using System;
using System.CodeDom;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Invert.Core.GraphDesigner
{
    public abstract class TemplateAttribute : Attribute
    {
        public virtual int Priority
        {
            get { return 0; }
        }
        public virtual bool CanGenerate(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            return true;
        }

        public virtual void Modify(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            
        }
    }

    public class ForEach : TemplateAttribute
    {
        public virtual int Priority
        {
            get { return -1; }
        }
        public ForEach(string iteratorMemberName)
        {
            IteratorMemberName = iteratorMemberName;
        }

        public string IteratorMemberName { get; set; }
        
        public override bool CanGenerate(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            ctx.AddMemberIterator(info.Name, _ => ctx.GetTemplateProperty(templateInstance, IteratorMemberName) as IEnumerable);
            //CreateIterator(templateInstance, IteratorMemberName, _));
            return true;
        }
        private static IEnumerable CreateIterator(object instance, string iteratorName, object arg1)
        {
            

            var property = instance.GetType().GetProperty(iteratorName);
            if (property == null && arg1 != null)
            {
                property = arg1.GetType().GetProperty(iteratorName);
                return property.GetValue(arg1, null) as IEnumerable;
            }
            if (property != null)
            {
                return property.GetValue(instance, null) as IEnumerable;
            }

            throw new Exception(string.Format("ForEach on property '{0}' could not be found on the template, or the node.",
                iteratorName));
        }
    }

    public class If : TemplateAttribute
    {
        public override int Priority
        {
            get { return -2; }
        }

        public string ConditionMemberName { get; set; }

        public If(string conditionMemberName)
        {
            ConditionMemberName = conditionMemberName;
        }

        public override bool CanGenerate(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            try
            {
                var condition = (bool) ctx.GetTemplateProperty(templateInstance, ConditionMemberName);
                return condition;
            }
            catch (InvalidCastException ex)
            {
                throw new TemplateException(string.Format("Condition {0} is not a valid condition, make sure it is of type boolean.", ConditionMemberName),ex);
            }
        }
    }

    [Flags]
    public enum TemplateLocation
    {
        DesignerFile = 0,
        EditableFile = 1,
        Both = 2
    }

    public class Inside : TemplateAttribute
    {
        public TemplateLocation TemplateLocation { get; set; }

        public Inside(TemplateLocation location)
        {
            TemplateLocation = location;
        }

        public override bool CanGenerate(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            ctx.CurrentAttribute.Location = (TemplateLocation)((int)TemplateLocation);
            if (TemplateLocation == TemplateLocation.Both)
            {
                return true;
            }
            if (ctx.IsDesignerFile && TemplateLocation == TemplateLocation.DesignerFile)
            {
                return true;
            } 
            if(!ctx.IsDesignerFile && TemplateLocation == TemplateLocation.EditableFile)
            {
                return true;
            }
            return false;
        }
        
    }

    public class InsideAll : Inside
    {
        public InsideAll() : base(TemplateLocation.Both)
        {
        }
    }
    public class GenerateConstructor : GenerateMember
    {
        public string[] BaseCallArgs { get; set; }

        public GenerateConstructor(TemplateLocation location, params string[] baseCallArgs)
            : base(location)
        {
            BaseCallArgs = baseCallArgs;
        }

        public GenerateConstructor(params string[] baseCallArgs)
        {
            BaseCallArgs = baseCallArgs;
        }
    }
    public class TemplateSetup : Attribute
    {

    }

    public class TemplateClass : Attribute
    {
        private string _classNameFormat = "{0}";
        private TemplateLocation _location = TemplateLocation.Both;
        private bool _autoInherit = true;

        public TemplateLocation Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public string ClassNameFormat
        {
            get { return _classNameFormat; }
            set { _classNameFormat = value; }
        }



        public bool AutoInherit
        {
            get { return _autoInherit; }
            set { _autoInherit = value; }
        }

        public TemplateClass()
        {
        }

        public TemplateClass(TemplateLocation location)
        {
            Location = location;
        }



        public TemplateClass(TemplateLocation location, string classNameFormat)
        {
            ClassNameFormat = classNameFormat;
            Location = location;
        }
    }

    public class GenerateMember : TemplateAttribute
    {
        private string _nameFormat;

        public string NameFormat
        {
            get { return _nameFormat; }
            set { _nameFormat = value; }
        }

        public TemplateLocation Location { get; set; }
        public GenerateMember()
        {
        }


        public GenerateMember(TemplateLocation location)
        {
            Location = location;
        }

        public override void Modify(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            base.Modify(templateInstance, info, ctx);
            string strRegex = @"_(?<name>[a-zA-Z0-9]+)_";
            bool replaced = false;
            var newName = Regex.Replace(info.Name, strRegex, _ =>
            {
                var name = _.Groups["name"].Value;
                try
                {
                    replaced = true;
                    return (string) ctx.GetTemplateProperty(templateInstance, name);
                }
                catch (TemplateException ex)
                {
                    return ctx.Item.Name;
                }
                
            });

            if (!replaced && NameFormat != null)
            {
                ctx.CurrentMember.Name = string.Format(NameFormat, ctx.Item.Name.Clean());
            }
            else
            {
                ctx.CurrentMember.Name = newName.Clean();
            }
            
        }
    }


    public class GenerateMethod : GenerateMember
    {
        private bool _callBase = true;

        public GenerateMethod(TemplateLocation location)
            : base(location)
        {
        }

        public GenerateMethod()
            : base()
        {
        }

        public GenerateMethod(TemplateLocation location, bool callBase)
            : base(location)
        {
            CallBase = callBase;
        }

        public GenerateMethod(string nameFormat, TemplateLocation location, bool callBase)
            : base(location)
        {
            _callBase = callBase;
            NameFormat = nameFormat;
        }

        public bool CallBase
        {
            get { return _callBase; }
            set { _callBase = value; }
        }


        public override void Modify(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            base.Modify(templateInstance, info, ctx);
            var methodInfo = info as MethodInfo;
            var t = ctx.ProcessType(methodInfo.ReturnType);
            if (t != null)
            {
                ctx.CurrentMethod.ReturnType = new CodeTypeReference(t);
            }
            var prms = methodInfo.GetParameters();
            for (int index = 0; index < prms.Length; index++)
            {
                var parameter = prms[index];
                var templateParameter = ctx.CurrentMethod.Parameters[index];
                var x = ctx.ProcessType(parameter.ParameterType);
                if (x != null)
                {
                    templateParameter.Type = new CodeTypeReference(x);
                }

            }
            var isOverried = false;
            if (!ctx.IsDesignerFile && ctx.CurrentMember.Attributes != MemberAttributes.Final && ctx.CurrentAttribute.Location == TemplateLocation.Both)
            {
                ctx.CurrentMethod.Attributes |= MemberAttributes.Override;
                isOverried = true;
            }
            if ((methodInfo.IsVirtual && !ctx.IsDesignerFile) || (methodInfo.IsOverride() && !methodInfo.GetBaseDefinition().IsAbstract && ctx.IsDesignerFile))
            {
                if ((ctx.CurrentAttribute as GenerateMethod).CallBase)
                {
                    //if (!info.IsOverride() || !info.GetBaseDefinition().IsAbstract && IsDesignerFile)
                    //{ 
                    ctx.CurrentMethod.invoke_base(true);
                    //}

                }
            }

        }
    }


    public class GenerateProperty : GenerateMember
    {


        public GenerateProperty(TemplateLocation location)
            : base(location)
        {
          
        }

        public GenerateProperty(string nameFormat)
        {
            NameFormat = nameFormat;
        }

        public GenerateProperty(TemplateLocation location, string nameFormat)
            : base(location)
        {
            NameFormat = nameFormat;
        }



        public GenerateProperty()
            : base()
        {
        }


        public override void Modify(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            base.Modify(templateInstance, info, ctx);
            var propertyInfo = info as PropertyInfo;
            var t = ctx.ProcessType(propertyInfo.PropertyType);
            if (t != null)
            {
                ctx.CurrentProperty.Type = new CodeTypeReference(t);
            }
        }
    }

    public class _TEMPLATETYPE_
    {

        public virtual string TheType(TemplateContext context)
        {
            return "void";
        }
    }

    public class _ITEMTYPE_ : _TEMPLATETYPE_
    {
        public override string TheType(TemplateContext context)
        {
            if (context.TypedItem == null)
            {
                return context.Item.Name;
            }
            return context.TypedItem.RelatedTypeName;
        }
    }

    public class WithNameFormat : TemplateAttribute
    {
        public override int Priority
        {
            get { return 1; }
        }

        public override void Modify(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            base.Modify(templateInstance, info, ctx);
            ctx.CurrentMember.Name = string.Format(Format, ctx.Item.Name.Clean());
        }

        public string Format { get; set; }

        public WithNameFormat(string format)
        {
            Format = format;
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class WithField : TemplateAttribute
    {
        public override int Priority
        {
            get { return 1; }
        }

        public override void Modify(object templateInstance, MemberInfo info, TemplateContext ctx)
        {
            base.Modify(templateInstance, info, ctx);
            var field = ctx.CurrentDecleration._private_(ctx.CurrentProperty.Type, "_{0}", ctx.CurrentProperty.Name.Clean());
            ctx.CurrentProperty.GetStatements._("return {0}", field.Name);
            ctx.CurrentProperty.SetStatements._("{0} = value", field.Name);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class WithLazyField : WithField
    {
        
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class WithObservable : TemplateAttribute
    {
        public WithObservable(Action selector)
        {
            Selector = selector;
        }

        public Action Selector { get; set; }
        public override int Priority
        {
            get { return 1; }
        }
        public string Format { get; set; }
        public WithObservable(string format = "{0}Observable")
        {
            Format = format;
        }

        
    }

}