using System.Collections.Generic;

namespace Esprima.Ast;

public class Identifier : Node, BindingIdentifier, IArrayPatternElement, INode, IFunctionParameter, PropertyValue, Expression, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly string Name;

	public override IEnumerable<INode> ChildNodes => Node.ZeroChildNodes;

	public Identifier(string name)
		: base(Nodes.Identifier)
	{
		Name = name;
	}
}
