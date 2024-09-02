using System.Collections.Generic;

namespace Esprima.Ast;

public class CallExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	private readonly NodeList<ArgumentListElement> _arguments;

	public readonly Expression Callee;

	public ref readonly NodeList<ArgumentListElement> Arguments => ref _arguments;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Callee, _arguments);

	public CallExpression(Expression callee, in NodeList<ArgumentListElement> args)
		: base(Nodes.CallExpression)
	{
		Callee = callee;
		_arguments = args;
	}
}
