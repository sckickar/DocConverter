using System.Collections.Generic;

namespace Esprima.Ast;

public class ThisExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public override IEnumerable<INode> ChildNodes => Node.ZeroChildNodes;

	public ThisExpression()
		: base(Nodes.ThisExpression)
	{
	}
}
