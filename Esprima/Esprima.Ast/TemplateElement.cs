using System.Collections.Generic;

namespace Esprima.Ast;

public class TemplateElement : Node
{
	public class TemplateElementValue
	{
		public string Cooked;

		public string Raw;
	}

	public readonly TemplateElementValue Value;

	public readonly bool Tail;

	public override IEnumerable<INode> ChildNodes => Node.ZeroChildNodes;

	public TemplateElement(TemplateElementValue value, bool tail)
		: base(Nodes.TemplateElement)
	{
		Value = value;
		Tail = tail;
	}
}
