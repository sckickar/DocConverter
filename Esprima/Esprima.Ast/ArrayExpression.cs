using System.Collections.Generic;

namespace Esprima.Ast;

public class ArrayExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	private readonly NodeList<ArrayExpressionElement> _elements;

	public ref readonly NodeList<ArrayExpressionElement> Elements => ref _elements;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(_elements);

	public ArrayExpression(in NodeList<ArrayExpressionElement> elements)
		: base(Nodes.ArrayExpression)
	{
		_elements = elements;
	}
}
