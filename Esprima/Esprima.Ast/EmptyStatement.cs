using System.Collections.Generic;

namespace Esprima.Ast;

public class EmptyStatement : Statement
{
	public override IEnumerable<INode> ChildNodes => Node.ZeroChildNodes;

	public EmptyStatement()
		: base(Nodes.EmptyStatement)
	{
	}
}
