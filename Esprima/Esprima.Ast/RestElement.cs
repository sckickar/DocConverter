using System.Collections.Generic;

namespace Esprima.Ast;

public class RestElement : Node, IArrayPatternElement, INode, Expression, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement, ObjectPatternProperty
{
	public readonly INode Argument;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Argument);

	public RestElement(INode argument)
		: base(Nodes.RestElement)
	{
		Argument = argument;
	}
}
