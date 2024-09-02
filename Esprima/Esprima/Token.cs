using Esprima.Ast;

namespace Esprima;

public class Token
{
	public TokenType Type;

	public string Literal;

	public int Start;

	public int End;

	public int LineNumber;

	public int LineStart;

	public Location Location;

	public int Precedence;

	public bool Octal;

	public bool Head;

	public bool Tail;

	public string RawTemplate;

	public bool BooleanValue;

	public double NumericValue;

	public object Value;

	public RegexValue RegexValue;

	public void Clear()
	{
		Type = TokenType.BooleanLiteral;
		Literal = null;
		Start = 0;
		End = 0;
		LineNumber = 0;
		LineStart = 0;
		Location = default(Location);
		Precedence = 0;
		Octal = false;
		Head = false;
		Tail = false;
		RawTemplate = null;
		BooleanValue = false;
		NumericValue = 0.0;
		Value = null;
		RegexValue = null;
	}
}
