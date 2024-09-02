using System.Collections.Generic;

namespace Esprima.Ast;

public class ConditionalExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly Expression Test;

	public readonly Expression Consequent;

	public readonly Expression Alternate;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Test, Consequent, Alternate);

	public ConditionalExpression(Expression test, Expression consequent, Expression alternate)
		: base(Nodes.ConditionalExpression)
	{
		Test = test;
		Consequent = consequent;
		Alternate = alternate;
	}
}
