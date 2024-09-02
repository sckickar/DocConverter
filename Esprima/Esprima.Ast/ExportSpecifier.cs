using System.Collections.Generic;

namespace Esprima.Ast;

public class ExportSpecifier : Statement
{
	public readonly Identifier Exported;

	public readonly Identifier Local;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Exported, Local);

	public ExportSpecifier(Identifier local, Identifier exported)
		: base(Nodes.ExportSpecifier)
	{
		Exported = exported;
		Local = local;
	}
}
