using System.Collections.Generic;

namespace Esprima.Ast;

public class ContinueStatement : Statement
{
	public readonly Identifier Label;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Label);

	public ContinueStatement(Identifier label)
		: base(Nodes.ContinueStatement)
	{
		Label = label;
	}
}
