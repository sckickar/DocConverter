using System.Collections.Generic;

namespace Esprima.Ast;

public class AssignmentPattern : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement, IArrayPatternElement, IFunctionParameter
{
	public readonly INode Left;

	public INode Right;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Left, Right);

	public AssignmentPattern(INode left, INode right)
		: base(Nodes.AssignmentPattern)
	{
		Left = left;
		Right = right;
	}
}
