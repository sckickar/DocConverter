using System;

namespace Esprima;

public class ParseError
{
	public string Description { get; }

	public string Source { get; }

	public bool IsIndexDefined => Index >= 0;

	public int Index { get; }

	public bool IsPositionDefined => Position.Line > 0;

	public Position Position { get; }

	public int LineNumber => Position.Line;

	public int Column => Position.Column;

	public ParseError(string description)
		: this(description, null, -1, default(Position))
	{
	}

	public ParseError(string description, string source, int index, Position position)
	{
		Description = description ?? throw new ArgumentNullException("description");
		Source = source;
		Index = index;
		Position = position;
	}

	public override string ToString()
	{
		if (LineNumber <= 0)
		{
			return Description;
		}
		return $"Line {LineNumber}: {Description}";
	}
}
