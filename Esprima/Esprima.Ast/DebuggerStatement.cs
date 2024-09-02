using System.Collections.Generic;

namespace Esprima.Ast;

public class DebuggerStatement : Statement
{
	public override IEnumerable<INode> ChildNodes => Node.ZeroChildNodes;

	public DebuggerStatement()
		: base(Nodes.DebuggerStatement)
	{
	}
}
