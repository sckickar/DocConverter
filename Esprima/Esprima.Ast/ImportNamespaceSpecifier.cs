using System.Collections.Generic;

namespace Esprima.Ast;

public class ImportNamespaceSpecifier : Node, ImportDeclarationSpecifier, INode
{
	public readonly Identifier Local;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Local);

	public ImportNamespaceSpecifier(Identifier local)
		: base(Nodes.ImportNamespaceSpecifier)
	{
		Local = local;
	}
}
