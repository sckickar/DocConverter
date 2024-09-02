using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Esprima.Ast;

public class Literal : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly double NumericValue;

	public readonly RegexValue Regex;

	public readonly object Value;

	public readonly string Raw;

	public readonly TokenType TokenType;

	public string StringValue
	{
		get
		{
			if (TokenType != TokenType.StringLiteral)
			{
				return null;
			}
			return Value as string;
		}
	}

	public bool BooleanValue
	{
		get
		{
			if (TokenType == TokenType.BooleanLiteral)
			{
				return NumericValue != 0.0;
			}
			return false;
		}
	}

	public Regex RegexValue
	{
		get
		{
			if (TokenType != TokenType.RegularExpression)
			{
				return null;
			}
			return (Regex)Value;
		}
	}

	public override IEnumerable<INode> ChildNodes => Enumerable.Empty<INode>();

	internal Literal(TokenType tokenType, object value, string raw)
		: base(Nodes.Literal)
	{
		TokenType = tokenType;
		Value = value;
		Raw = raw;
	}

	public Literal(string value, string raw)
		: this(TokenType.StringLiteral, value, raw)
	{
	}

	public Literal(bool value, string raw)
		: this(TokenType.BooleanLiteral, value, raw)
	{
		NumericValue = (value ? 1 : 0);
	}

	public Literal(double value, string raw)
		: this(TokenType.NumericLiteral, value, raw)
	{
		NumericValue = value;
	}

	public Literal(string raw)
		: this(TokenType.NullLiteral, null, raw)
	{
	}

	public Literal(string pattern, string flags, object value, string raw)
		: this(TokenType.RegularExpression, value, raw)
	{
		Regex = new RegexValue(pattern, flags);
	}
}
