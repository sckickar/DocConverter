using System.Collections.Generic;
using System.Linq;

namespace Esprima.Ast;

public abstract class Node : INode
{
	protected static IEnumerable<INode> ZeroChildNodes = Enumerable.Empty<INode>();

	public Nodes Type { get; }

	public Range Range { get; set; }

	public Location Location { get; set; }

	public abstract IEnumerable<INode> ChildNodes { get; }

	protected Node(Nodes type)
	{
		Type = type;
	}
}
