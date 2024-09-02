namespace Esprima.Ast;

public abstract class Statement : Node, INode, IStatementListItem
{
	public Identifier LabelSet { get; internal set; }

	protected Statement(Nodes type)
		: base(type)
	{
	}
}
