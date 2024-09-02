using System.Collections.Generic;

namespace Esprima.Ast;

public class ObjectExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	private readonly NodeList<ObjectExpressionProperty> _properties;

	public ref readonly NodeList<ObjectExpressionProperty> Properties => ref _properties;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(_properties);

	public ObjectExpression(in NodeList<ObjectExpressionProperty> properties)
		: base(Nodes.ObjectExpression)
	{
		_properties = properties;
	}
}
