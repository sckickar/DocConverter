using System.Collections.Generic;

namespace Esprima.Ast;

public interface INode
{
	Nodes Type { get; }

	Range Range { get; set; }

	Location Location { get; set; }

	IEnumerable<INode> ChildNodes { get; }
}
