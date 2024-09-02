using System.Collections.Generic;

namespace Esprima.Ast;

public class ClassExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly Identifier Id;

	public readonly Expression SuperClass;

	public readonly ClassBody Body;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Id, SuperClass, Body);

	public ClassExpression(Identifier id, Expression superClass, ClassBody body)
		: base(Nodes.ClassExpression)
	{
		Id = id;
		SuperClass = superClass;
		Body = body;
	}
}
