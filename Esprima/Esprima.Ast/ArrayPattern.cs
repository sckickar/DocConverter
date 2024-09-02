using System.Collections.Generic;

namespace Esprima.Ast;

public class ArrayPattern : Node, BindingPattern, IArrayPatternElement, INode, IFunctionParameter, PropertyValue
{
	private readonly NodeList<IArrayPatternElement> _elements;

	public ref readonly NodeList<IArrayPatternElement> Elements => ref _elements;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(_elements);

	public ArrayPattern(in NodeList<IArrayPatternElement> elements)
		: base(Nodes.ArrayPattern)
	{
		_elements = elements;
	}
}
