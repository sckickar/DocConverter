using System.Collections.Generic;

namespace Esprima.Ast;

public class ClassDeclaration : Statement, IDeclaration, IStatementListItem, INode
{
	public readonly Identifier Id;

	public readonly Expression SuperClass;

	public readonly ClassBody Body;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Id, SuperClass, Body);

	public ClassDeclaration(Identifier id, Expression superClass, ClassBody body)
		: base(Nodes.ClassDeclaration)
	{
		Id = id;
		SuperClass = superClass;
		Body = body;
	}
}
