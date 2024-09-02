using System.Collections.Generic;

namespace Esprima.Ast;

public abstract class MemberExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement, IArrayPatternElement
{
	public readonly Expression Object;

	public readonly Expression Property;

	public readonly bool Computed;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Object, Property);

	protected MemberExpression(Expression obj, Expression property, bool computed)
		: base(Nodes.MemberExpression)
	{
		Object = obj;
		Property = property;
		Computed = computed;
	}
}
