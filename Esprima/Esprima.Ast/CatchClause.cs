using System.Collections.Generic;

namespace Esprima.Ast;

public class CatchClause : Statement
{
	public readonly IArrayPatternElement Param;

	public readonly BlockStatement Body;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Param, Body);

	public CatchClause(IArrayPatternElement param, BlockStatement body)
		: base(Nodes.CatchClause)
	{
		Param = param;
		Body = body;
	}
}
