using System.Collections.Generic;

namespace Esprima.Ast;

public abstract class ClassProperty : Node
{
	public PropertyKind Kind;

	public Expression Key;

	public bool Computed;

	public PropertyValue Value;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Key, Value);

	protected ClassProperty(Nodes type)
		: base(type)
	{
	}
}
