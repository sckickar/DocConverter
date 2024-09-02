using System.Collections.Generic;

namespace Esprima.Ast;

public class ObjectPattern : Node, BindingPattern, IArrayPatternElement, INode, IFunctionParameter, PropertyValue
{
	private readonly NodeList<INode> _properties;

	public ref readonly NodeList<INode> Properties => ref _properties;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(_properties);

	public ObjectPattern(in NodeList<INode> properties)
		: base(Nodes.ObjectPattern)
	{
		_properties = properties;
	}
}
