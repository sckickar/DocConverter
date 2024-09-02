using System.Collections.Generic;

namespace Esprima.Ast;

public class SpreadElement : Node, ArgumentListElement, INode, ArrayExpressionElement, ObjectExpressionProperty, Expression, PropertyValue, IDeclaration, IStatementListItem
{
	public readonly Expression Argument;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Argument);

	public SpreadElement(Expression argument)
		: base(Nodes.SpreadElement)
	{
		Argument = argument;
	}
}
