using System.Collections.Generic;

namespace Esprima.Ast;

public class Super : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public override IEnumerable<INode> ChildNodes => Node.ZeroChildNodes;

	public Super()
		: base(Nodes.Super)
	{
	}
}
