using System.Collections.Generic;

namespace Esprima.Ast;

public sealed class AwaitExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly Expression Argument;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Argument);

	public AwaitExpression(Expression argument)
		: base(Nodes.AwaitExpression)
	{
		Argument = argument;
	}
}
