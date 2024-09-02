using System.Collections.Generic;

namespace Esprima.Ast;

public class ArrowParameterPlaceHolder : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public static readonly ArrowParameterPlaceHolder Empty;

	private readonly NodeList<INode> _params;

	public ref readonly NodeList<INode> Params => ref _params;

	public bool Async { get; }

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(_params);

	public ArrowParameterPlaceHolder(in NodeList<INode> parameters, bool async)
		: base(Nodes.ArrowParameterPlaceHolder)
	{
		Async = async;
		_params = parameters;
	}

	static ArrowParameterPlaceHolder()
	{
		NodeList<INode> parameters = default(NodeList<INode>);
		Empty = new ArrowParameterPlaceHolder(in parameters, async: false);
	}
}
