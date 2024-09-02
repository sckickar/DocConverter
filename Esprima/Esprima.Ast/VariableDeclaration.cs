using System.Collections.Generic;

namespace Esprima.Ast;

public class VariableDeclaration : Statement, IDeclaration, IStatementListItem, INode
{
	private readonly NodeList<VariableDeclarator> _declarations;

	public readonly VariableDeclarationKind Kind;

	public ref readonly NodeList<VariableDeclarator> Declarations => ref _declarations;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(_declarations);

	public VariableDeclaration(in NodeList<VariableDeclarator> declarations, VariableDeclarationKind kind)
		: base(Nodes.VariableDeclaration)
	{
		_declarations = declarations;
		Kind = kind;
	}
}
