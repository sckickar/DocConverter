using System.Collections.Generic;

namespace Esprima.Ast;

public class ExportNamedDeclaration : Node, ExportDeclaration, IDeclaration, IStatementListItem, INode
{
	private readonly NodeList<ExportSpecifier> _specifiers;

	public readonly IStatementListItem Declaration;

	public readonly Literal Source;

	public ref readonly NodeList<ExportSpecifier> Specifiers => ref _specifiers;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Declaration, _specifiers, Source);

	public ExportNamedDeclaration(IStatementListItem declaration, in NodeList<ExportSpecifier> specifiers, Literal source)
		: base(Nodes.ExportNamedDeclaration)
	{
		Declaration = declaration;
		_specifiers = specifiers;
		Source = source;
	}
}
