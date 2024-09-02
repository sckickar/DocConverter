using System.Collections.Generic;
using System.Linq;

namespace Esprima.Ast;

public class Import : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public override IEnumerable<INode> ChildNodes => Enumerable.Empty<INode>();

	public Import()
		: base(Nodes.Import)
	{
	}
}
