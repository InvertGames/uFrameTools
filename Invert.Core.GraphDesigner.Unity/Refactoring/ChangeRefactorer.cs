using System;
using System.IO;
using System.Linq;
using Invert.ICSharpCode.NRefactory.CSharp;
using Invert.ICSharpCode.NRefactory.CSharp.Expressions;
using Invert.ICSharpCode.NRefactory.CSharp.GeneralScope;
using Invert.ICSharpCode.NRefactory.CSharp.TypeMembers;
using Invert.ICSharpCode.NRefactory.PatternMatching;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{

    public interface IRefactorer : IAstVisitor
    {
        DocumentScript Script { get; set; }
        bool Changed { get; set; }
    }

    public class DefaultRefactorer : DepthFirstAstVisitor,IRefactorer
    {
        //public virtual void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
        //{
             
        //}

        //public virtual void VisitUndocumentedExpression(UndocumentedExpression undocumentedExpression)
        //{
             
        //}

        //public virtual void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
        //{
             
        //}

        //public virtual void VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression)
        //{
             
        //}

        //public virtual void VisitAsExpression(AsExpression asExpression)
        //{
             
        //}

        //public virtual void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        //{
             
        //}

        //public virtual void VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression)
        //{
             
        //}

        //public virtual void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        //{
             
        //}

        //public virtual void VisitCastExpression(CastExpression castExpression)
        //{
             
        //}

        //public virtual void VisitCheckedExpression(CheckedExpression checkedExpression)
        //{
             
        //}

        //public virtual void VisitConditionalExpression(ConditionalExpression conditionalExpression)
        //{
             
        //}

        //public virtual void VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression)
        //{
             
        //}

        //public virtual void VisitDirectionExpression(DirectionExpression directionExpression)
        //{
             
        //}

        //public virtual void VisitIdentifierExpression(IdentifierExpression identifierExpression)
        //{
             
        //}

        //public virtual void VisitIndexerExpression(IndexerExpression indexerExpression)
        //{
             
        //}

        //public virtual void VisitInvocationExpression(InvocationExpression invocationExpression)
        //{
             
        //}

        //public virtual void VisitIsExpression(IsExpression isExpression)
        //{
             
        //}

        //public virtual void VisitLambdaExpression(LambdaExpression lambdaExpression)
        //{
             
        //}

        //public virtual void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
        //{
             
        //}

        //public virtual void VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression)
        //{
             
        //}

        //public virtual void VisitNamedExpression(NamedExpression namedExpression)
        //{
             
        //}

        //public virtual void VisitNullReferenceExpression(NullReferenceExpression nullReferenceExpression)
        //{
             
        //}

        //public virtual void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
        //{
             
        //}

        //public virtual void VisitAnonymousTypeCreateExpression(AnonymousTypeCreateExpression anonymousTypeCreateExpression)
        //{
             
        //}

        //public virtual void VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression)
        //{
             
        //}

        //public virtual void VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression)
        //{
             
        //}

        //public virtual void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
        //{
             
        //}

        //public virtual void VisitSizeOfExpression(SizeOfExpression sizeOfExpression)
        //{
             
        //}

        //public virtual void VisitStackAllocExpression(StackAllocExpression stackAllocExpression)
        //{
             
        //}

        //public virtual void VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression)
        //{
             
        //}

        //public virtual void VisitTypeOfExpression(TypeOfExpression typeOfExpression)
        //{
             
        //}

        //public virtual void VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression)
        //{
             
        //}

        //public virtual void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        //{
             
        //}

        //public virtual void VisitUncheckedExpression(UncheckedExpression uncheckedExpression)
        //{
             
        //}

        //public virtual void VisitQueryExpression(QueryExpression queryExpression)
        //{
             
        //}

        //public virtual void VisitQueryContinuationClause(QueryContinuationClause queryContinuationClause)
        //{
             
        //}

        //public virtual void VisitQueryFromClause(QueryFromClause queryFromClause)
        //{
             
        //}

        //public virtual void VisitQueryLetClause(QueryLetClause queryLetClause)
        //{
             
        //}

        //public virtual void VisitQueryWhereClause(QueryWhereClause queryWhereClause)
        //{
             
        //}

        //public virtual void VisitQueryJoinClause(QueryJoinClause queryJoinClause)
        //{
             
        //}

        //public virtual void VisitQueryOrderClause(QueryOrderClause queryOrderClause)
        //{
             
        //}

        //public virtual void VisitQueryOrdering(QueryOrdering queryOrdering)
        //{
             
        //}

        //public virtual void VisitQuerySelectClause(QuerySelectClause querySelectClause)
        //{
             
        //}

        //public virtual void VisitQueryGroupClause(QueryGroupClause queryGroupClause)
        //{
             
        //}

        //public virtual void VisitAttribute(Attribute attribute)
        //{
             
        //}

        //public virtual void VisitAttributeSection(AttributeSection attributeSection)
        //{
             
        //}

        //public virtual void VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration)
        //{
             
        //}

        //public virtual void VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
        //{
             
        //}

        //public virtual void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        //{
             
        //}

        //public virtual void VisitUsingAliasDeclaration(UsingAliasDeclaration usingAliasDeclaration)
        //{
             
        //}

        //public virtual void VisitUsingDeclaration(UsingDeclaration usingDeclaration)
        //{
             
        //}

        //public virtual void VisitExternAliasDeclaration(ExternAliasDeclaration externAliasDeclaration)
        //{
             
        //}

        //public virtual void VisitBlockStatement(BlockStatement blockStatement)
        //{
             
        //}

        //public virtual void VisitBreakStatement(BreakStatement breakStatement)
        //{
             
        //}

        //public virtual void VisitCheckedStatement(CheckedStatement checkedStatement)
        //{
             
        //}

        //public virtual void VisitContinueStatement(ContinueStatement continueStatement)
        //{
             
        //}

        //public virtual void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        //{
             
        //}

        //public virtual void VisitEmptyStatement(EmptyStatement emptyStatement)
        //{
             
        //}

        //public virtual void VisitExpressionStatement(ExpressionStatement expressionStatement)
        //{
             
        //}

        //public virtual void VisitFixedStatement(FixedStatement fixedStatement)
        //{
             
        //}

        //public virtual void VisitForeachStatement(ForeachStatement foreachStatement)
        //{
             
        //}

        //public virtual void VisitForStatement(ForStatement forStatement)
        //{
             
        //}

        //public virtual void VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement)
        //{
             
        //}

        //public virtual void VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement)
        //{
             
        //}

        //public virtual void VisitGotoStatement(GotoStatement gotoStatement)
        //{
             
        //}

        //public virtual void VisitIfElseStatement(IfElseStatement ifElseStatement)
        //{
             
        //}

        //public virtual void VisitLabelStatement(LabelStatement labelStatement)
        //{
             
        //}

        //public virtual void VisitLockStatement(LockStatement lockStatement)
        //{
             
        //}

        //public virtual void VisitReturnStatement(ReturnStatement returnStatement)
        //{
             
        //}

        //public virtual void VisitSwitchStatement(SwitchStatement switchStatement)
        //{
             
        //}

        //public virtual void VisitSwitchSection(SwitchSection switchSection)
        //{
             
        //}

        //public virtual void VisitCaseLabel(CaseLabel caseLabel)
        //{
             
        //}

        //public virtual void VisitThrowStatement(ThrowStatement throwStatement)
        //{
             
        //}

        //public virtual void VisitTryCatchStatement(TryCatchStatement tryCatchStatement)
        //{
             
        //}

        //public virtual void VisitCatchClause(CatchClause catchClause)
        //{
             
        //}

        //public virtual void VisitUncheckedStatement(UncheckedStatement uncheckedStatement)
        //{
             
        //}

        //public virtual void VisitUnsafeStatement(UnsafeStatement unsafeStatement)
        //{
             
        //}

        //public virtual void VisitUsingStatement(UsingStatement usingStatement)
        //{
             
        //}

        //public virtual void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement)
        //{
             
        //}

        //public virtual void VisitWhileStatement(WhileStatement whileStatement)
        //{
             
        //}

        //public virtual void VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement)
        //{
             
        //}

        //public virtual void VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement)
        //{
             
        //}

        //public virtual void VisitAccessor(Accessor accessor)
        //{
             
        //}

        //public virtual void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
        //{
             
        //}

        //public virtual void VisitConstructorInitializer(ConstructorInitializer constructorInitializer)
        //{
             
        //}

        //public virtual void VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
        //{
             
        //}

        //public virtual void VisitEnumMemberDeclaration(EnumMemberDeclaration enumMemberDeclaration)
        //{
             
        //}

        //public virtual void VisitEventDeclaration(EventDeclaration eventDeclaration)
        //{
             
        //}

        //public virtual void VisitCustomEventDeclaration(CustomEventDeclaration customEventDeclaration)
        //{
             
        //}

        //public virtual void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
        //{
             
        //}

        //public virtual void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
        //{
             
        //}

        //public virtual void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        //{
             
        //}

        //public virtual void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
        //{
             
        //}

        //public virtual void VisitParameterDeclaration(ParameterDeclaration parameterDeclaration)
        //{
             
        //}

        //public virtual void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
        //{
             
        //}

        //public virtual void VisitVariableInitializer(VariableInitializer variableInitializer)
        //{
             
        //}

        //public virtual void VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration)
        //{
             
        //}

        //public virtual void VisitFixedVariableInitializer(FixedVariableInitializer fixedVariableInitializer)
        //{
             
        //}

        //public virtual void VisitSyntaxTree(SyntaxTree syntaxTree)
        //{
             
        //}

        //public virtual void VisitSimpleType(SimpleType simpleType)
        //{
             
        //}

        //public virtual void VisitMemberType(MemberType memberType)
        //{
             
        //}

        //public virtual void VisitComposedType(ComposedType composedType)
        //{
             
        //}

        //public virtual void VisitArraySpecifier(ArraySpecifier arraySpecifier)
        //{
             
        //}

        //public virtual void VisitPrimitiveType(PrimitiveType primitiveType)
        //{
             
        //}

        //public virtual void VisitComment(Comment comment)
        //{
             
        //}

        //public virtual void VisitNewLine(NewLineNode newLineNode)
        //{
             
        //}

        //public virtual void VisitWhitespace(WhitespaceNode whitespaceNode)
        //{
             
        //}

        //public virtual void VisitText(TextNode textNode)
        //{
             
        //}

        //public virtual void VisitPreProcessorDirective(PreProcessorDirective preProcessorDirective)
        //{
             
        //}

        //public virtual void VisitDocumentationReference(DocumentationReference documentationReference)
        //{
             
        //}

        //public virtual void VisitTypeParameterDeclaration(TypeParameterDeclaration typeParameterDeclaration)
        //{
             
        //}

        //public virtual void VisitConstraint(Constraint constraint)
        //{
             
        //}

        //public virtual void VisitCSharpTokenNode(CSharpTokenNode cSharpTokenNode)
        //{
             
        //}

        //public virtual void VisitIdentifier(Identifier identifier)
        //{
             
        //}

        //public virtual void VisitNullNode(AstNode nullNode)
        //{
             
        //}

        //public virtual void VisitErrorNode(AstNode errorNode)
        //{
             
        //}

        //public virtual void VisitPatternPlaceholder(AstNode placeholder, Pattern pattern)
        //{
             
        //}

        public DocumentScript Script { get; set; }
        public bool Changed { get; set; }

        public virtual void OutputNodeVisited(INode node, TextWriter outputFormatter)
        {
             
        }

        public virtual void OutputNodeVisiting(INode node, TextWriter outputFormatter)
        {
             
        }
    }
    public class RenameTypeRefactorer : DefaultRefactorer
    {
        public string Old { get; set; }
        public string New { get; set; }

        
        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
        
            if (typeDeclaration.Name == Old)
            {
                var nameToken = (Identifier) typeDeclaration.NameToken.Clone();
                nameToken.Name = New;

                Script.Replace(typeDeclaration.NameToken, nameToken);
                Changed = true;
            }
            foreach (var type in typeDeclaration.BaseTypes)
            {
                var simpleType = type as SimpleType;
                if (simpleType != null && simpleType.Identifier == Old)
                {
                    var nameToken = (Identifier)simpleType.IdentifierToken.Clone();
                    nameToken.Name = New;
                    Script.Replace(simpleType.IdentifierToken, nameToken);
                    Changed = true;
                }
            }
        }

        public override void VisitSimpleType(SimpleType simpleType)
        {
            base.VisitSimpleType(simpleType);
            if (simpleType.Identifier == Old)
            {
                var nameToken = (Identifier)simpleType.IdentifierToken.Clone();
                nameToken.Name = New;
                Script.Replace(simpleType.IdentifierToken, nameToken);
            }
        }
    }
    public class RenameMethodRefactorer : DefaultRefactorer
    {
        public string Old { get; set; }
        public string New { get; set; }

        public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        {
            base.VisitMethodDeclaration(methodDeclaration);
            if (methodDeclaration.Name == Old)
            {
                var nameToken = (Identifier)methodDeclaration.NameToken.Clone();
                nameToken.Name = New;

                Script.Replace(methodDeclaration.NameToken, nameToken);
                Changed = true;
            }
        }
    }
    public class InsertTextAtBottomRefactorer : DefaultRefactorer
    {
        public string Text { get; set; }
        public string ClassName { get; set; }

        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            InvertApplication.Log(string.Format("{0} : {1}",typeDeclaration.Name,ClassName));
            if (typeDeclaration.Name == ClassName)
            {
                Script.InsertBefore(typeDeclaration.LastChild, new TextNode(Text));
                Changed = true;
            }
            
        }



  
    }

}