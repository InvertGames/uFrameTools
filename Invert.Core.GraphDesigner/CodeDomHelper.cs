using System;
using System.CodeDom;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Invert.Core.GraphDesigner
{
    public static class CodeDomHelpers
    {
        public static CodeTypeDeclaration Base(this CodeTypeDeclaration decleration, string baseType)
        {
            decleration.BaseTypes.Add(baseType);
            return decleration;
        }

        public static CodeTypeDeclaration Base(this CodeTypeDeclaration decleration, Type baseType)
        {
            decleration.BaseTypes.Add(baseType);
            return decleration;
        }

        public static CodeMemberMethod Method(this CodeTypeDeclaration decleration, MemberAttributes attributes,
            Type returnType, string name)
        {
            var method = new CodeMemberMethod() {Name = name, Attributes = attributes};
            decleration.Members.Add(method);
            return method;
        }

        public static CodeMemberMethod Add(this CodeMemberMethod method, string snippet, params object[] args)
        {
            method.Statements.Add(new CodeSnippetExpression(string.Format(snippet, args)));
            return method;
        }

        public static CodeMemberMethod MethodFromTypeMethod(this Type type, string methodName, bool callBase = true)
        {
            var m = type.GetMethod(methodName);
            var method = new CodeMemberMethod()
            {
                Name = methodName,
                ReturnType = new CodeTypeReference(m.ReturnType)
            };
            method.Attributes = MemberAttributes.Override;
            if (m.IsPublic)
            {
                method.Attributes |= MemberAttributes.Public;
            }
            if (m.IsPrivate)
            {
                method.Attributes |= MemberAttributes.Private;
            }
            if (m.IsFamily)
            {
                method.Attributes |= MemberAttributes.Family;
            }
            if (m.IsFamilyAndAssembly)
            {
                method.Attributes |= MemberAttributes.FamilyAndAssembly;
            }
            if (m.IsFamilyOrAssembly)
            {
                method.Attributes |= MemberAttributes.FamilyOrAssembly;
            }
            var baseCall = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), methodName);
            var ps = m.GetParameters();
            foreach (var p in ps)
            {
                var parameter = new CodeParameterDeclarationExpression(p.ParameterType, p.Name);
                method.Parameters.Add(parameter);
                baseCall.Parameters.Add(new CodeVariableReferenceExpression(parameter.Name));
            }
            if (callBase && !m.IsAbstract)
            {
                method.Statements.Add(baseCall);
            }
            return method;
        }


        //public static CodeMemberMethod Parameter(this CodeMemberMethod method)
        //{

        //}
        public static CodeTypeDeclaration end(this CodeMemberMethod method)
        {
            return method.UserData["Decleration"] as CodeTypeDeclaration;
        }
        public static CodeTypeDeclaration end(this CodeMemberProperty method)
        {
            return method.UserData["Decleration"] as CodeTypeDeclaration;
        }
        public static CodeTypeDeclaration end(this CodeMemberField method)
        {
            return method.UserData["Decleration"] as CodeTypeDeclaration;
        }
        public static CodeTypeDeclaration end(this CodeMemberEvent method)
        {
            return method.UserData["Decleration"] as CodeTypeDeclaration;
        }

        #region methods
        public static CodeMemberMethod private_func(this CodeTypeDeclaration s, string returnType, string methodName, params object[] parameters)
        {
            var method = new CodeMemberMethod()
            {
                
                Attributes = MemberAttributes.Private | MemberAttributes.Final,
                Name = methodName
            };
            if (returnType == null || returnType == "void")
            {
                method.ReturnType = new CodeTypeReference(typeof (void));
            }
            else
            {
                method.ReturnType = new CodeTypeReference(returnType);
            }

            for (var i = 0; i < parameters.Length; i+=2)
            {
                if (parameters.Length <= (i + 1)) break;
                var type = parameters[i];
                var name = parameters[i + 1];
                var baseType = type as Type;
                if (baseType != null)
                {
                    method.Parameters.Add(new CodeParameterDeclarationExpression(baseType, (string)name));
                }
                else
                {
                    method.Parameters.Add(new CodeParameterDeclarationExpression(type.ToString(), (string)name));
                }
                
            }
            if (!method.UserData.Contains("Decleration"))
            {
                method.UserData.Add("Decleration", s);
            }
            s.Members.Add(method);
            return method;
        }
        public static CodeMemberMethod protected_func(this CodeTypeDeclaration s, string returnType, string methodName, params object[] parameters)
        {
            var result = private_func(s, returnType, methodName, parameters);
            result.Attributes = MemberAttributes.Family | MemberAttributes.Final;
            return result;
        }

        public static CodeMemberMethod public_func(this CodeTypeDeclaration s, string returnType, string methodName, params object[] parameters)
        {
            var result = private_func(s, returnType, methodName, parameters);
            result.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            return result;
        }

        public static CodeMemberMethod invoke_base(this CodeMemberMethod m)
        {
            var baseInvoke =
                new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeBaseReferenceExpression(),
                    m.Name));
            foreach (CodeParameterDeclarationExpression parameter in m.Parameters)
            {
                baseInvoke.Parameters.Add(new CodeVariableReferenceExpression(parameter.Name));
            }
            m.Statements.Add(baseInvoke);
            return m;
        }
        //public static CodeMemberMethod invoke_base_tovar(this CodeMemberMethod m)
        //{
        //    var baseInvoke =
        //        new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeBaseReferenceExpression(),
        //            m.Name));
        //    foreach (CodeParameterDeclarationExpression parameter in m.Parameters)
        //    {
        //        baseInvoke.Parameters.Add(new CodeVariableReferenceExpression(parameter.Name));
        //    }
        //    m.Statements.Add(baseInvoke);
        //    return m;
        //}
        public static CodeMemberMethod protected_virtual_func(this CodeTypeDeclaration s, string returnType, string methodName, params object[] parameters)
        {
            var result = private_func(s, returnType, methodName, parameters);
            result.Attributes = MemberAttributes.Family;
            return result;
        }

        public static CodeMemberMethod public_virtual_func(this CodeTypeDeclaration s, string returnType, string methodName, params object[] parameters)
        {
            var result = private_func(s, returnType, methodName, parameters);
            result.Attributes = MemberAttributes.Public;
            return result;
        }
        public static CodeMemberMethod protected_override_func(this CodeTypeDeclaration s, string returnType, string methodName, params object[] parameters)
        {
            var result = private_func(s, returnType, methodName, parameters);
            result.Attributes = MemberAttributes.Family;
            return result;
        }

        public static CodeMemberMethod public_override_func(this CodeTypeDeclaration s, string returnType, string methodName, params object[] parameters)
        {
            var result = private_func(s, returnType, methodName, parameters);
            result.Attributes = MemberAttributes.Public;
            return result;
        }
        #endregion

        #region properties


        public static CodeMemberProperty private_(this CodeTypeDeclaration s, string returnType, string propertyName, params object[] nameArgs)
        {
            var method = new CodeMemberProperty()
            {
                Type = new CodeTypeReference(returnType),
                Attributes = MemberAttributes.Private | MemberAttributes.Final,
                Name=string.Format(propertyName,nameArgs)
            };
            if (!method.UserData.Contains("Decleration"))
            {
                method.UserData.Add("Decleration",s);
            }
            s.Members.Add(method);
            return method;
        }
        public static CodeMemberProperty protected_(this CodeTypeDeclaration s, string returnType, string propertyName, params object[] nameArgs)
        {
            var result = private_(s, returnType, propertyName, nameArgs);
            result.Attributes = MemberAttributes.Family | MemberAttributes.Final;
            return result;
        }

        public static CodeMemberProperty public_(this CodeTypeDeclaration s, string returnType, string propertyName, params object[] nameArgs)
        {
            var result = private_(s, returnType, propertyName, nameArgs);
            result.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            return result;
        }

        public static CodeMemberProperty protected_virtual_(this CodeTypeDeclaration s, string returnType, string propertyName, params object[] nameArgs)
        {
            var result = private_(s, returnType, propertyName, nameArgs);
            result.Attributes = MemberAttributes.Family;
            return result;
        }

        public static CodeMemberProperty public_virtual_(this CodeTypeDeclaration s, string returnType, string propertyName, params object[] nameArgs)
        {
            var result = private_(s, returnType, propertyName, nameArgs);
            result.Attributes = MemberAttributes.Public;
            return result;
        }
        public static CodeMemberProperty protected_override_(this CodeTypeDeclaration s, string returnType, string propertyName, params object[] nameArgs)
        {
            var result = private_(s, returnType, propertyName, nameArgs);
            result.Attributes = MemberAttributes.Family;
            return result;
        }

        public static CodeMemberProperty public_override_(this CodeTypeDeclaration s, string returnType, string propertyName, params object[] nameArgs)
        {
            var result = private_(s, returnType, propertyName, nameArgs);
            result.Attributes = MemberAttributes.Public;
            return result;
        }
        #endregion

        #region fields
        public static CodeMemberField _private_(this CodeTypeDeclaration s, string returnType, string fieldName, params object[] nameArgs)
        {
            var method = new CodeMemberField()
            {
                Type = new CodeTypeReference(returnType),
                Attributes = MemberAttributes.Private | MemberAttributes.Final,
                Name = string.Format(fieldName,nameArgs)
            };
            if (!method.UserData.Contains("Decleration"))
            {
                method.UserData.Add("Decleration", s);
            }
            s.Members.Add(method);
            return method;
        }
        public static CodeMemberField _protected_(this CodeTypeDeclaration s, string returnType, string fieldName, params object[] nameArgs)
        {
            var result = _private_(s, returnType, fieldName, nameArgs);
            result.Attributes = MemberAttributes.Family | MemberAttributes.Final;
            return result;
        }

        public static CodeMemberField _public_(this CodeTypeDeclaration s, string returnType, string fieldName, params object[] nameArgs)
        {
            var result = _private_(s, returnType, fieldName, nameArgs);
            result.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            return result;
        }

        public static CodeMemberField _protected_virtual_(this CodeTypeDeclaration s, string returnType, string fieldName, params object[] nameArgs)
        {
            var result = _private_(s, returnType, fieldName, nameArgs);
            result.Attributes = MemberAttributes.Family;
            return result;
        }

        public static CodeMemberField _public_virtual_(this CodeTypeDeclaration s, string returnType, string fieldName, params object[] nameArgs)
        {
            var result = _private_(s, returnType, fieldName, nameArgs);
            result.Attributes = MemberAttributes.Public;
            return result;
        }
        public static CodeMemberField _protected_override_(this CodeTypeDeclaration s, string returnType, string fieldName, params object[] nameArgs)
        {
            var result = _private_(s, returnType, fieldName, nameArgs);
            result.Attributes = MemberAttributes.Family;
            return result;
        }

        public static CodeMemberField _public_override_(this CodeTypeDeclaration s, string returnType, string fieldName, params object[] nameArgs)
        {
            var result = _private_(s, returnType, fieldName, nameArgs);
            result.Attributes = MemberAttributes.Public;
            return result;
        }
        #endregion


        #region Statemenets 
        public static CodeConditionStatement _if(this CodeStatementCollection statements, string expression, params object[] args)
        {
            var condition = new CodeConditionStatement(new CodeSnippetExpression(string.Format(expression,args)));
            statements.Add(condition);
            return condition;
        }
        public static CodeMemberMethod _(this CodeMemberMethod method, string expression, params object[] args)
        {
            _(method.Statements, expression, args);
            return method;
        }

        public static CodeStatementCollection _(this CodeStatementCollection statements, string expression, params object[] args)
        {
            statements.Add(new CodeSnippetExpression(string.Format(expression, args)));
            return statements;
        }
        public static CodeMemberProperty _get(this CodeMemberProperty property, string expression, params object[] args)
        {
            property.GetStatements.Add(new CodeSnippetExpression(string.Format(expression, args)));
            property.HasGet = true;
            return property;
        }
        public static CodeMemberProperty _set(this CodeMemberProperty property, string expression, params object[] args)
        {
            property.SetStatements.Add(new CodeSnippetExpression(string.Format(expression, args)));
            property.HasSet = true;
            return property;
        }

        #endregion
        public static CodeTypeDeclaration Declare(this CodeNamespace ns, MemberAttributes attributes, string name)
        {
            var decl = new CodeTypeDeclaration
            {
                Name = name,
                Attributes = MemberAttributes.Public
            };
            ns.Types.Add(decl);
            return decl;
        }

        public static CodeTypeDeclaration Field(this CodeTypeDeclaration decleration, Type fieldType, string name,
            CodeExpression init = null, params CodeAttributeDeclaration[] customAttributes)
        {
            return Field(decleration, MemberAttributes.Private, fieldType, name, init, customAttributes);
        }

        public static CodeTypeDeclaration Field(this CodeTypeDeclaration decleration, MemberAttributes attributes,
            Type fieldType, string name, CodeExpression init = null, params CodeAttributeDeclaration[] customAttributes)
        {
            var field = new CodeMemberField(fieldType, name) {InitExpression = init, Attributes = attributes};
            if (customAttributes != null)
                field.CustomAttributes.AddRange(customAttributes);
            decleration.Members.Add(field);
            return decleration;
        }

        public static CodeTypeDeclaration EncapsulatedField(this CodeTypeDeclaration decleration, Type fieldType,
            string name, string propertyName, CodeExpression lazyValue, bool publicField = false)
        {
            var field = new CodeMemberField(fieldType, name);
            if (publicField)
            {
                field.Attributes = MemberAttributes.Public;
            }
            decleration.Members.Add(field);
            decleration.EncapsulateField(field, propertyName, lazyValue);
            return decleration;
        }

        public static CodeTypeDeclaration EncapsulateField(this CodeTypeDeclaration typeDeclaration,
            CodeMemberField field, string name, CodeExpression lazyValue, CodeExpression lazyCondition = null)
        {
            var p = new CodeMemberProperty
            {
                Name = name,
                Type = field.Type,
                HasGet = true,
                Attributes = MemberAttributes.Public
            };
            typeDeclaration.Members.Add(p);

            var r = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);
            var lazyConditionStatement = new CodeConditionStatement();
            CodeExpression finalLazyCondition = lazyCondition;
            if (finalLazyCondition == null)
            {
                var defaultConditionStatement =
                    new CodeBinaryOperatorExpression(
                        r,
                        CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression("null"));

                finalLazyCondition = defaultConditionStatement;
            }

            lazyConditionStatement.Condition = finalLazyCondition;
            lazyConditionStatement.TrueStatements.Add(new CodeAssignStatement(r, lazyValue));

            p.GetStatements.Add(lazyConditionStatement);
            p.GetStatements.Add(
                new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                    field.Name)));

            return typeDeclaration;
        }

        public static CodeMemberProperty EncapsulateField(this CodeMemberField field, string name)
        {
            return EncapsulateField(field, name, null, null, true);
        }

        public static CodeMemberProperty EncapsulateField(this CodeMemberField field, string name,
            CodeExpression lazyValue, CodeExpression lazyCondition = null, bool generateSetter = false)
        {
            var p = new CodeMemberProperty
            {
                Name = name,
                Type = field.Type,
                HasGet = true,
                Attributes = MemberAttributes.Public
            };
            var r = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);

            if (lazyValue != null)
            {
                var lazyConditionStatement = new CodeConditionStatement();
                CodeExpression finalLazyCondition = lazyCondition;

                if (finalLazyCondition == null)
                {
                    var defaultConditionStatement =
                        new CodeBinaryOperatorExpression(
                            r,
                            CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression("null"));

                    finalLazyCondition = defaultConditionStatement;
                }

                lazyConditionStatement.Condition = finalLazyCondition;
                lazyConditionStatement.TrueStatements.Add(new CodeAssignStatement(r, lazyValue));
                p.GetStatements.Add(lazyConditionStatement);
            }



            p.GetStatements.Add(
                new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                    field.Name)));
            if (generateSetter)
            {
                p.HasSet = true;
                p.SetStatements.Add(new CodeSnippetExpression(string.Format("{0} = value", field.Name)));
            }
            return p;
        }
    }
}