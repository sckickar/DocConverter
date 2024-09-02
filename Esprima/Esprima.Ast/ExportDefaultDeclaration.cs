using System.Collections.Generic;

namespace Esprima.Ast;

public class ExportDefaultDeclaration : Node, ExportDeclaration, IDeclaration, IStatementListItem, INode
{
	public readonly IDeclaration Declaration;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Declaration);

	public ExportDefaultDeclaration(IDeclaration declaration)
		: base(Nodes.ExportDefaultDeclaration)
	{
		Declaration = declaration;
	}
}
