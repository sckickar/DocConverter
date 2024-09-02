using System.Collections.Generic;

namespace Esprima.Ast;

public class TaggedTemplateExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly Expression Tag;

	public readonly TemplateLiteral Quasi;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Tag, Quasi);

	public TaggedTemplateExpression(Expression tag, TemplateLiteral quasi)
		: base(Nodes.TaggedTemplateExpression)
	{
		Tag = tag;
		Quasi = quasi;
	}
}
