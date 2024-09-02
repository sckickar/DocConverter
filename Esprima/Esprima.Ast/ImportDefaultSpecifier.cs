using System.Collections.Generic;

namespace Esprima.Ast;

public class ImportDefaultSpecifier : Node, ImportDeclarationSpecifier, INode
{
	public readonly Identifier Local;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Local);

	public ImportDefaultSpecifier(Identifier local)
		: base(Nodes.ImportDefaultSpecifier)
	{
		Local = local;
	}
}
