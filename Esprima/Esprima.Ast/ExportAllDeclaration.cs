using System.Collections.Generic;

namespace Esprima.Ast;

public class ExportAllDeclaration : Node, ExportDeclaration, IDeclaration, IStatementListItem, INode
{
	public readonly Literal Source;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Source);

	public ExportAllDeclaration(Literal source)
		: base(Nodes.ExportAllDeclaration)
	{
		Source = source;
	}
}
