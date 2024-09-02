using System.Collections.Generic;

namespace Esprima.Ast;

public class NewExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	private readonly NodeList<ArgumentListElement> _arguments;

	public readonly Expression Callee;

	public ref readonly NodeList<ArgumentListElement> Arguments => ref _arguments;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Callee, Arguments);

	public NewExpression(Expression callee, in NodeList<ArgumentListElement> args)
		: base(Nodes.NewExpression)
	{
		Callee = callee;
		_arguments = args;
	}
}
