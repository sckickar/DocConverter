using System.Collections.Generic;

namespace Esprima.Ast;

public class SequenceExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	private NodeList<Expression> _expressions;

	public ref readonly NodeList<Expression> Expressions => ref _expressions;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Expressions);

	public SequenceExpression(in NodeList<Expression> expressions)
		: base(Nodes.SequenceExpression)
	{
		_expressions = expressions;
	}

	internal void UpdateExpressions(in NodeList<Expression> value)
	{
		_expressions = value;
	}
}
