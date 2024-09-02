using System.Collections.Generic;

namespace Esprima.Ast;

public class YieldExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly Expression Argument;

	public readonly bool Delegate;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Argument);

	public YieldExpression(Expression argument, bool delgate)
		: base(Nodes.YieldExpression)
	{
		Argument = argument;
		Delegate = delgate;
	}
}
