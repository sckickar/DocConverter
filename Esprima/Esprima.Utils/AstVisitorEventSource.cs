using System;
using Esprima.Ast;

namespace Esprima.Utils;

public class AstVisitorEventSource : AstVisitor
{
	public event EventHandler<INode> VisitingNode;

	public event EventHandler<INode> VisitedNode;

	public event EventHandler<Program> VisitingProgram;

	public event EventHandler<Program> VisitedProgram;

	public event EventHandler<Statement> VisitingStatement;

	public event EventHandler<Statement> VisitedStatement;

	public event EventHandler<INode> VisitingUnknownNode;

	public event EventHandler<INode> VisitedUnknownNode;

	public event EventHandler<CatchClause> VisitingCatchClause;

	public event EventHandler<CatchClause> VisitedCatchClause;

	public event EventHandler<FunctionDeclaration> VisitingFunctionDeclaration;

	public event EventHandler<FunctionDeclaration> VisitedFunctionDeclaration;

	public event EventHandler<WithStatement> VisitingWithStatement;

	public event EventHandler<WithStatement> VisitedWithStatement;

	public event EventHandler<WhileStatement> VisitingWhileStatement;

	public event EventHandler<WhileStatement> VisitedWhileStatement;

	public event EventHandler<VariableDeclaration> VisitingVariableDeclaration;

	public event EventHandler<VariableDeclaration> VisitedVariableDeclaration;

	public event EventHandler<TryStatement> VisitingTryStatement;

	public event EventHandler<TryStatement> VisitedTryStatement;

	public event EventHandler<ThrowStatement> VisitingThrowStatement;

	public event EventHandler<ThrowStatement> VisitedThrowStatement;

	public event EventHandler<SwitchStatement> VisitingSwitchStatement;

	public event EventHandler<SwitchStatement> VisitedSwitchStatement;

	public event EventHandler<SwitchCase> VisitingSwitchCase;

	public event EventHandler<SwitchCase> VisitedSwitchCase;

	public event EventHandler<ReturnStatement> VisitingReturnStatement;

	public event EventHandler<ReturnStatement> VisitedReturnStatement;

	public event EventHandler<LabeledStatement> VisitingLabeledStatement;

	public event EventHandler<LabeledStatement> VisitedLabeledStatement;

	public event EventHandler<IfStatement> VisitingIfStatement;

	public event EventHandler<IfStatement> VisitedIfStatement;

	public event EventHandler<EmptyStatement> VisitingEmptyStatement;

	public event EventHandler<EmptyStatement> VisitedEmptyStatement;

	public event EventHandler<DebuggerStatement> VisitingDebuggerStatement;

	public event EventHandler<DebuggerStatement> VisitedDebuggerStatement;

	public event EventHandler<ExpressionStatement> VisitingExpressionStatement;

	public event EventHandler<ExpressionStatement> VisitedExpressionStatement;

	public event EventHandler<ForStatement> VisitingForStatement;

	public event EventHandler<ForStatement> VisitedForStatement;

	public event EventHandler<ForInStatement> VisitingForInStatement;

	public event EventHandler<ForInStatement> VisitedForInStatement;

	public event EventHandler<DoWhileStatement> VisitingDoWhileStatement;

	public event EventHandler<DoWhileStatement> VisitedDoWhileStatement;

	public event EventHandler<Expression> VisitingExpression;

	public event EventHandler<Expression> VisitedExpression;

	public event EventHandler<ArrowFunctionExpression> VisitingArrowFunctionExpression;

	public event EventHandler<ArrowFunctionExpression> VisitedArrowFunctionExpression;

	public event EventHandler<UnaryExpression> VisitingUnaryExpression;

	public event EventHandler<UnaryExpression> VisitedUnaryExpression;

	public event EventHandler<UpdateExpression> VisitingUpdateExpression;

	public event EventHandler<UpdateExpression> VisitedUpdateExpression;

	public event EventHandler<ThisExpression> VisitingThisExpression;

	public event EventHandler<ThisExpression> VisitedThisExpression;

	public event EventHandler<SequenceExpression> VisitingSequenceExpression;

	public event EventHandler<SequenceExpression> VisitedSequenceExpression;

	public event EventHandler<ObjectExpression> VisitingObjectExpression;

	public event EventHandler<ObjectExpression> VisitedObjectExpression;

	public event EventHandler<NewExpression> VisitingNewExpression;

	public event EventHandler<NewExpression> VisitedNewExpression;

	public event EventHandler<MemberExpression> VisitingMemberExpression;

	public event EventHandler<MemberExpression> VisitedMemberExpression;

	public event EventHandler<BinaryExpression> VisitingLogicalExpression;

	public event EventHandler<BinaryExpression> VisitedLogicalExpression;

	public event EventHandler<Literal> VisitingLiteral;

	public event EventHandler<Literal> VisitedLiteral;

	public event EventHandler<Identifier> VisitingIdentifier;

	public event EventHandler<Identifier> VisitedIdentifier;

	public event EventHandler<IFunction> VisitingFunctionExpression;

	public event EventHandler<IFunction> VisitedFunctionExpression;

	public event EventHandler<ClassExpression> VisitingClassExpression;

	public event EventHandler<ClassExpression> VisitedClassExpression;

	public event EventHandler<ExportDefaultDeclaration> VisitingExportDefaultDeclaration;

	public event EventHandler<ExportDefaultDeclaration> VisitedExportDefaultDeclaration;

	public event EventHandler<ExportAllDeclaration> VisitingExportAllDeclaration;

	public event EventHandler<ExportAllDeclaration> VisitedExportAllDeclaration;

	public event EventHandler<ExportNamedDeclaration> VisitingExportNamedDeclaration;

	public event EventHandler<ExportNamedDeclaration> VisitedExportNamedDeclaration;

	public event EventHandler<ExportSpecifier> VisitingExportSpecifier;

	public event EventHandler<ExportSpecifier> VisitedExportSpecifier;

	public event EventHandler<Import> VisitingImport;

	public event EventHandler<Import> VisitedImport;

	public event EventHandler<ImportDeclaration> VisitingImportDeclaration;

	public event EventHandler<ImportDeclaration> VisitedImportDeclaration;

	public event EventHandler<ImportNamespaceSpecifier> VisitingImportNamespaceSpecifier;

	public event EventHandler<ImportNamespaceSpecifier> VisitedImportNamespaceSpecifier;

	public event EventHandler<ImportDefaultSpecifier> VisitingImportDefaultSpecifier;

	public event EventHandler<ImportDefaultSpecifier> VisitedImportDefaultSpecifier;

	public event EventHandler<ImportSpecifier> VisitingImportSpecifier;

	public event EventHandler<ImportSpecifier> VisitedImportSpecifier;

	public event EventHandler<MethodDefinition> VisitingMethodDefinition;

	public event EventHandler<MethodDefinition> VisitedMethodDefinition;

	public event EventHandler<ForOfStatement> VisitingForOfStatement;

	public event EventHandler<ForOfStatement> VisitedForOfStatement;

	public event EventHandler<ClassDeclaration> VisitingClassDeclaration;

	public event EventHandler<ClassDeclaration> VisitedClassDeclaration;

	public event EventHandler<ClassBody> VisitingClassBody;

	public event EventHandler<ClassBody> VisitedClassBody;

	public event EventHandler<YieldExpression> VisitingYieldExpression;

	public event EventHandler<YieldExpression> VisitedYieldExpression;

	public event EventHandler<TaggedTemplateExpression> VisitingTaggedTemplateExpression;

	public event EventHandler<TaggedTemplateExpression> VisitedTaggedTemplateExpression;

	public event EventHandler<Super> VisitingSuper;

	public event EventHandler<Super> VisitedSuper;

	public event EventHandler<MetaProperty> VisitingMetaProperty;

	public event EventHandler<MetaProperty> VisitedMetaProperty;

	public event EventHandler<ObjectPattern> VisitingObjectPattern;

	public event EventHandler<ObjectPattern> VisitedObjectPattern;

	public event EventHandler<SpreadElement> VisitingSpreadElement;

	public event EventHandler<SpreadElement> VisitedSpreadElement;

	public event EventHandler<AssignmentPattern> VisitingAssignmentPattern;

	public event EventHandler<AssignmentPattern> VisitedAssignmentPattern;

	public event EventHandler<ArrayPattern> VisitingArrayPattern;

	public event EventHandler<ArrayPattern> VisitedArrayPattern;

	public event EventHandler<VariableDeclarator> VisitingVariableDeclarator;

	public event EventHandler<VariableDeclarator> VisitedVariableDeclarator;

	public event EventHandler<TemplateLiteral> VisitingTemplateLiteral;

	public event EventHandler<TemplateLiteral> VisitedTemplateLiteral;

	public event EventHandler<TemplateElement> VisitingTemplateElement;

	public event EventHandler<TemplateElement> VisitedTemplateElement;

	public event EventHandler<RestElement> VisitingRestElement;

	public event EventHandler<RestElement> VisitedRestElement;

	public event EventHandler<Property> VisitingProperty;

	public event EventHandler<Property> VisitedProperty;

	public event EventHandler<ConditionalExpression> VisitingConditionalExpression;

	public event EventHandler<ConditionalExpression> VisitedConditionalExpression;

	public event EventHandler<CallExpression> VisitingCallExpression;

	public event EventHandler<CallExpression> VisitedCallExpression;

	public event EventHandler<BinaryExpression> VisitingBinaryExpression;

	public event EventHandler<BinaryExpression> VisitedBinaryExpression;

	public event EventHandler<ArrayExpression> VisitingArrayExpression;

	public event EventHandler<ArrayExpression> VisitedArrayExpression;

	public event EventHandler<AssignmentExpression> VisitingAssignmentExpression;

	public event EventHandler<AssignmentExpression> VisitedAssignmentExpression;

	public event EventHandler<ContinueStatement> VisitingContinueStatement;

	public event EventHandler<ContinueStatement> VisitedContinueStatement;

	public event EventHandler<BreakStatement> VisitingBreakStatement;

	public event EventHandler<BreakStatement> VisitedBreakStatement;

	public event EventHandler<BlockStatement> VisitingBlockStatement;

	public event EventHandler<BlockStatement> VisitedBlockStatement;

	public override void Visit(INode node)
	{
		this.VisitingNode?.Invoke(this, node);
		base.Visit(node);
		this.VisitedNode?.Invoke(this, node);
	}

	protected override void VisitProgram(Program program)
	{
		this.VisitingProgram?.Invoke(this, program);
		base.VisitProgram(program);
		this.VisitedProgram?.Invoke(this, program);
	}

	protected override void VisitStatement(Statement statement)
	{
		this.VisitingStatement?.Invoke(this, statement);
		base.VisitStatement(statement);
		this.VisitedStatement?.Invoke(this, statement);
	}

	protected override void VisitUnknownNode(INode node)
	{
		this.VisitingUnknownNode?.Invoke(this, node);
		base.VisitUnknownNode(node);
		this.VisitedUnknownNode?.Invoke(this, node);
	}

	protected override void VisitCatchClause(CatchClause catchClause)
	{
		this.VisitingCatchClause?.Invoke(this, catchClause);
		base.VisitCatchClause(catchClause);
		this.VisitedCatchClause?.Invoke(this, catchClause);
	}

	protected override void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
	{
		this.VisitingFunctionDeclaration?.Invoke(this, functionDeclaration);
		base.VisitFunctionDeclaration(functionDeclaration);
		this.VisitedFunctionDeclaration?.Invoke(this, functionDeclaration);
	}

	protected override void VisitWithStatement(WithStatement withStatement)
	{
		this.VisitingWithStatement?.Invoke(this, withStatement);
		base.VisitWithStatement(withStatement);
		this.VisitedWithStatement?.Invoke(this, withStatement);
	}

	protected override void VisitWhileStatement(WhileStatement whileStatement)
	{
		this.VisitingWhileStatement?.Invoke(this, whileStatement);
		base.VisitWhileStatement(whileStatement);
		this.VisitedWhileStatement?.Invoke(this, whileStatement);
	}

	protected override void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
	{
		this.VisitingVariableDeclaration?.Invoke(this, variableDeclaration);
		base.VisitVariableDeclaration(variableDeclaration);
		this.VisitedVariableDeclaration?.Invoke(this, variableDeclaration);
	}

	protected override void VisitTryStatement(TryStatement tryStatement)
	{
		this.VisitingTryStatement?.Invoke(this, tryStatement);
		base.VisitTryStatement(tryStatement);
		this.VisitedTryStatement?.Invoke(this, tryStatement);
	}

	protected override void VisitThrowStatement(ThrowStatement throwStatement)
	{
		this.VisitingThrowStatement?.Invoke(this, throwStatement);
		base.VisitThrowStatement(throwStatement);
		this.VisitedThrowStatement?.Invoke(this, throwStatement);
	}

	protected override void VisitSwitchStatement(SwitchStatement switchStatement)
	{
		this.VisitingSwitchStatement?.Invoke(this, switchStatement);
		base.VisitSwitchStatement(switchStatement);
		this.VisitedSwitchStatement?.Invoke(this, switchStatement);
	}

	protected override void VisitSwitchCase(SwitchCase switchCase)
	{
		this.VisitingSwitchCase?.Invoke(this, switchCase);
		base.VisitSwitchCase(switchCase);
		this.VisitedSwitchCase?.Invoke(this, switchCase);
	}

	protected override void VisitReturnStatement(ReturnStatement returnStatement)
	{
		this.VisitingReturnStatement?.Invoke(this, returnStatement);
		base.VisitReturnStatement(returnStatement);
		this.VisitedReturnStatement?.Invoke(this, returnStatement);
	}

	protected override void VisitLabeledStatement(LabeledStatement labeledStatement)
	{
		this.VisitingLabeledStatement?.Invoke(this, labeledStatement);
		base.VisitLabeledStatement(labeledStatement);
		this.VisitedLabeledStatement?.Invoke(this, labeledStatement);
	}

	protected override void VisitIfStatement(IfStatement ifStatement)
	{
		this.VisitingIfStatement?.Invoke(this, ifStatement);
		base.VisitIfStatement(ifStatement);
		this.VisitedIfStatement?.Invoke(this, ifStatement);
	}

	protected override void VisitEmptyStatement(EmptyStatement emptyStatement)
	{
		this.VisitingEmptyStatement?.Invoke(this, emptyStatement);
		base.VisitEmptyStatement(emptyStatement);
		this.VisitedEmptyStatement?.Invoke(this, emptyStatement);
	}

	protected override void VisitDebuggerStatement(DebuggerStatement debuggerStatement)
	{
		this.VisitingDebuggerStatement?.Invoke(this, debuggerStatement);
		base.VisitDebuggerStatement(debuggerStatement);
		this.VisitedDebuggerStatement?.Invoke(this, debuggerStatement);
	}

	protected override void VisitExpressionStatement(ExpressionStatement expressionStatement)
	{
		this.VisitingExpressionStatement?.Invoke(this, expressionStatement);
		base.VisitExpressionStatement(expressionStatement);
		this.VisitedExpressionStatement?.Invoke(this, expressionStatement);
	}

	protected override void VisitForStatement(ForStatement forStatement)
	{
		this.VisitingForStatement?.Invoke(this, forStatement);
		base.VisitForStatement(forStatement);
		this.VisitedForStatement?.Invoke(this, forStatement);
	}

	protected override void VisitForInStatement(ForInStatement forInStatement)
	{
		this.VisitingForInStatement?.Invoke(this, forInStatement);
		base.VisitForInStatement(forInStatement);
		this.VisitedForInStatement?.Invoke(this, forInStatement);
	}

	protected override void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
	{
		this.VisitingDoWhileStatement?.Invoke(this, doWhileStatement);
		base.VisitDoWhileStatement(doWhileStatement);
		this.VisitedDoWhileStatement?.Invoke(this, doWhileStatement);
	}

	protected override void VisitExpression(Expression expression)
	{
		this.VisitingExpression?.Invoke(this, expression);
		base.VisitExpression(expression);
		this.VisitedExpression?.Invoke(this, expression);
	}

	protected override void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
	{
		this.VisitingArrowFunctionExpression?.Invoke(this, arrowFunctionExpression);
		base.VisitArrowFunctionExpression(arrowFunctionExpression);
		this.VisitedArrowFunctionExpression?.Invoke(this, arrowFunctionExpression);
	}

	protected override void VisitUnaryExpression(UnaryExpression unaryExpression)
	{
		this.VisitingUnaryExpression?.Invoke(this, unaryExpression);
		base.VisitUnaryExpression(unaryExpression);
		this.VisitedUnaryExpression?.Invoke(this, unaryExpression);
	}

	protected override void VisitUpdateExpression(UpdateExpression updateExpression)
	{
		this.VisitingUpdateExpression?.Invoke(this, updateExpression);
		base.VisitUpdateExpression(updateExpression);
		this.VisitedUpdateExpression?.Invoke(this, updateExpression);
	}

	protected override void VisitThisExpression(ThisExpression thisExpression)
	{
		this.VisitingThisExpression?.Invoke(this, thisExpression);
		base.VisitThisExpression(thisExpression);
		this.VisitedThisExpression?.Invoke(this, thisExpression);
	}

	protected override void VisitSequenceExpression(SequenceExpression sequenceExpression)
	{
		this.VisitingSequenceExpression?.Invoke(this, sequenceExpression);
		base.VisitSequenceExpression(sequenceExpression);
		this.VisitedSequenceExpression?.Invoke(this, sequenceExpression);
	}

	protected override void VisitObjectExpression(ObjectExpression objectExpression)
	{
		this.VisitingObjectExpression?.Invoke(this, objectExpression);
		base.VisitObjectExpression(objectExpression);
		this.VisitedObjectExpression?.Invoke(this, objectExpression);
	}

	protected override void VisitNewExpression(NewExpression newExpression)
	{
		this.VisitingNewExpression?.Invoke(this, newExpression);
		base.VisitNewExpression(newExpression);
		this.VisitedNewExpression?.Invoke(this, newExpression);
	}

	protected override void VisitMemberExpression(MemberExpression memberExpression)
	{
		this.VisitingMemberExpression?.Invoke(this, memberExpression);
		base.VisitMemberExpression(memberExpression);
		this.VisitedMemberExpression?.Invoke(this, memberExpression);
	}

	protected override void VisitLogicalExpression(BinaryExpression binaryExpression)
	{
		this.VisitingLogicalExpression?.Invoke(this, binaryExpression);
		base.VisitLogicalExpression(binaryExpression);
		this.VisitedLogicalExpression?.Invoke(this, binaryExpression);
	}

	protected override void VisitLiteral(Literal literal)
	{
		this.VisitingLiteral?.Invoke(this, literal);
		base.VisitLiteral(literal);
		this.VisitedLiteral?.Invoke(this, literal);
	}

	protected override void VisitIdentifier(Identifier identifier)
	{
		this.VisitingIdentifier?.Invoke(this, identifier);
		base.VisitIdentifier(identifier);
		this.VisitedIdentifier?.Invoke(this, identifier);
	}

	protected override void VisitFunctionExpression(IFunction function)
	{
		this.VisitingFunctionExpression?.Invoke(this, function);
		base.VisitFunctionExpression(function);
		this.VisitedFunctionExpression?.Invoke(this, function);
	}

	protected override void VisitClassExpression(ClassExpression classExpression)
	{
		this.VisitingClassExpression?.Invoke(this, classExpression);
		base.VisitClassExpression(classExpression);
		this.VisitedClassExpression?.Invoke(this, classExpression);
	}

	protected override void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
	{
		this.VisitingExportDefaultDeclaration?.Invoke(this, exportDefaultDeclaration);
		base.VisitExportDefaultDeclaration(exportDefaultDeclaration);
		this.VisitedExportDefaultDeclaration?.Invoke(this, exportDefaultDeclaration);
	}

	protected override void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
	{
		this.VisitingExportAllDeclaration?.Invoke(this, exportAllDeclaration);
		base.VisitExportAllDeclaration(exportAllDeclaration);
		this.VisitedExportAllDeclaration?.Invoke(this, exportAllDeclaration);
	}

	protected override void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
	{
		this.VisitingExportNamedDeclaration?.Invoke(this, exportNamedDeclaration);
		base.VisitExportNamedDeclaration(exportNamedDeclaration);
		this.VisitedExportNamedDeclaration?.Invoke(this, exportNamedDeclaration);
	}

	protected override void VisitExportSpecifier(ExportSpecifier exportSpecifier)
	{
		this.VisitingExportSpecifier?.Invoke(this, exportSpecifier);
		base.VisitExportSpecifier(exportSpecifier);
		this.VisitedExportSpecifier?.Invoke(this, exportSpecifier);
	}

	protected override void VisitImport(Import import)
	{
		this.VisitingImport?.Invoke(this, import);
		base.VisitImport(import);
		this.VisitedImport?.Invoke(this, import);
	}

	protected override void VisitImportDeclaration(ImportDeclaration importDeclaration)
	{
		this.VisitingImportDeclaration?.Invoke(this, importDeclaration);
		base.VisitImportDeclaration(importDeclaration);
		this.VisitedImportDeclaration?.Invoke(this, importDeclaration);
	}

	protected override void VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
	{
		this.VisitingImportNamespaceSpecifier?.Invoke(this, importNamespaceSpecifier);
		base.VisitImportNamespaceSpecifier(importNamespaceSpecifier);
		this.VisitedImportNamespaceSpecifier?.Invoke(this, importNamespaceSpecifier);
	}

	protected override void VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
	{
		this.VisitingImportDefaultSpecifier?.Invoke(this, importDefaultSpecifier);
		base.VisitImportDefaultSpecifier(importDefaultSpecifier);
		this.VisitedImportDefaultSpecifier?.Invoke(this, importDefaultSpecifier);
	}

	protected override void VisitImportSpecifier(ImportSpecifier importSpecifier)
	{
		this.VisitingImportSpecifier?.Invoke(this, importSpecifier);
		base.VisitImportSpecifier(importSpecifier);
		this.VisitedImportSpecifier?.Invoke(this, importSpecifier);
	}

	protected override void VisitMethodDefinition(MethodDefinition methodDefinitions)
	{
		this.VisitingMethodDefinition?.Invoke(this, methodDefinitions);
		base.VisitMethodDefinition(methodDefinitions);
		this.VisitedMethodDefinition?.Invoke(this, methodDefinitions);
	}

	protected override void VisitForOfStatement(ForOfStatement forOfStatement)
	{
		this.VisitingForOfStatement?.Invoke(this, forOfStatement);
		base.VisitForOfStatement(forOfStatement);
		this.VisitedForOfStatement?.Invoke(this, forOfStatement);
	}

	protected override void VisitClassDeclaration(ClassDeclaration classDeclaration)
	{
		this.VisitingClassDeclaration?.Invoke(this, classDeclaration);
		base.VisitClassDeclaration(classDeclaration);
		this.VisitedClassDeclaration?.Invoke(this, classDeclaration);
	}

	protected override void VisitClassBody(ClassBody classBody)
	{
		this.VisitingClassBody?.Invoke(this, classBody);
		base.VisitClassBody(classBody);
		this.VisitedClassBody?.Invoke(this, classBody);
	}

	protected override void VisitYieldExpression(YieldExpression yieldExpression)
	{
		this.VisitingYieldExpression?.Invoke(this, yieldExpression);
		base.VisitYieldExpression(yieldExpression);
		this.VisitedYieldExpression?.Invoke(this, yieldExpression);
	}

	protected override void VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
	{
		this.VisitingTaggedTemplateExpression?.Invoke(this, taggedTemplateExpression);
		base.VisitTaggedTemplateExpression(taggedTemplateExpression);
		this.VisitedTaggedTemplateExpression?.Invoke(this, taggedTemplateExpression);
	}

	protected override void VisitSuper(Super super)
	{
		this.VisitingSuper?.Invoke(this, super);
		base.VisitSuper(super);
		this.VisitedSuper?.Invoke(this, super);
	}

	protected override void VisitMetaProperty(MetaProperty metaProperty)
	{
		this.VisitingMetaProperty?.Invoke(this, metaProperty);
		base.VisitMetaProperty(metaProperty);
		this.VisitedMetaProperty?.Invoke(this, metaProperty);
	}

	protected override void VisitObjectPattern(ObjectPattern objectPattern)
	{
		this.VisitingObjectPattern?.Invoke(this, objectPattern);
		base.VisitObjectPattern(objectPattern);
		this.VisitedObjectPattern?.Invoke(this, objectPattern);
	}

	protected override void VisitSpreadElement(SpreadElement spreadElement)
	{
		this.VisitingSpreadElement?.Invoke(this, spreadElement);
		base.VisitSpreadElement(spreadElement);
		this.VisitedSpreadElement?.Invoke(this, spreadElement);
	}

	protected override void VisitAssignmentPattern(AssignmentPattern assignmentPattern)
	{
		this.VisitingAssignmentPattern?.Invoke(this, assignmentPattern);
		base.VisitAssignmentPattern(assignmentPattern);
		this.VisitedAssignmentPattern?.Invoke(this, assignmentPattern);
	}

	protected override void VisitArrayPattern(ArrayPattern arrayPattern)
	{
		this.VisitingArrayPattern?.Invoke(this, arrayPattern);
		base.VisitArrayPattern(arrayPattern);
		this.VisitedArrayPattern?.Invoke(this, arrayPattern);
	}

	protected override void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
	{
		this.VisitingVariableDeclarator?.Invoke(this, variableDeclarator);
		base.VisitVariableDeclarator(variableDeclarator);
		this.VisitedVariableDeclarator?.Invoke(this, variableDeclarator);
	}

	protected override void VisitTemplateLiteral(TemplateLiteral templateLiteral)
	{
		this.VisitingTemplateLiteral?.Invoke(this, templateLiteral);
		base.VisitTemplateLiteral(templateLiteral);
		this.VisitedTemplateLiteral?.Invoke(this, templateLiteral);
	}

	protected override void VisitTemplateElement(TemplateElement templateElement)
	{
		this.VisitingTemplateElement?.Invoke(this, templateElement);
		base.VisitTemplateElement(templateElement);
		this.VisitedTemplateElement?.Invoke(this, templateElement);
	}

	protected override void VisitRestElement(RestElement restElement)
	{
		this.VisitingRestElement?.Invoke(this, restElement);
		base.VisitRestElement(restElement);
		this.VisitedRestElement?.Invoke(this, restElement);
	}

	protected override void VisitProperty(Property property)
	{
		this.VisitingProperty?.Invoke(this, property);
		base.VisitProperty(property);
		this.VisitedProperty?.Invoke(this, property);
	}

	protected override void VisitConditionalExpression(ConditionalExpression conditionalExpression)
	{
		this.VisitingConditionalExpression?.Invoke(this, conditionalExpression);
		base.VisitConditionalExpression(conditionalExpression);
		this.VisitedConditionalExpression?.Invoke(this, conditionalExpression);
	}

	protected override void VisitCallExpression(CallExpression callExpression)
	{
		this.VisitingCallExpression?.Invoke(this, callExpression);
		base.VisitCallExpression(callExpression);
		this.VisitedCallExpression?.Invoke(this, callExpression);
	}

	protected override void VisitBinaryExpression(BinaryExpression binaryExpression)
	{
		this.VisitingBinaryExpression?.Invoke(this, binaryExpression);
		base.VisitBinaryExpression(binaryExpression);
		this.VisitedBinaryExpression?.Invoke(this, binaryExpression);
	}

	protected override void VisitArrayExpression(ArrayExpression arrayExpression)
	{
		this.VisitingArrayExpression?.Invoke(this, arrayExpression);
		base.VisitArrayExpression(arrayExpression);
		this.VisitedArrayExpression?.Invoke(this, arrayExpression);
	}

	protected override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
	{
		this.VisitingAssignmentExpression?.Invoke(this, assignmentExpression);
		base.VisitAssignmentExpression(assignmentExpression);
		this.VisitedAssignmentExpression?.Invoke(this, assignmentExpression);
	}

	protected override void VisitContinueStatement(ContinueStatement continueStatement)
	{
		this.VisitingContinueStatement?.Invoke(this, continueStatement);
		base.VisitContinueStatement(continueStatement);
		this.VisitedContinueStatement?.Invoke(this, continueStatement);
	}

	protected override void VisitBreakStatement(BreakStatement breakStatement)
	{
		this.VisitingBreakStatement?.Invoke(this, breakStatement);
		base.VisitBreakStatement(breakStatement);
		this.VisitedBreakStatement?.Invoke(this, breakStatement);
	}

	protected override void VisitBlockStatement(BlockStatement blockStatement)
	{
		this.VisitingBlockStatement?.Invoke(this, blockStatement);
		base.VisitBlockStatement(blockStatement);
		this.VisitedBlockStatement?.Invoke(this, blockStatement);
	}
}
