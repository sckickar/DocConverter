using System.Collections.Generic;

namespace Esprima.Ast;

public class BreakStatement : Statement
{
	public readonly Identifier Label;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Label);

	public BreakStatement(Identifier label)
		: base(Nodes.BreakStatement)
	{
		Label = label;
	}
}
