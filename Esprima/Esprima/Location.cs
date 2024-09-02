using System;

namespace Esprima;

public readonly struct Location : IEquatable<Location>
{
	public Position Start { get; }

	public Position End { get; }

	public string Source { get; }

	public Location(Position start, Position end)
		: this(start, end, null)
	{
	}

	public Location(Position start, Position end, string source)
	{
		Start = start;
		if ((start == default(Position) && end != default(Position)) || (end == default(Position) && start != default(Position)) || end.Line < start.Line || (start.Line > 0 && start.Line == end.Line && end.Column < start.Column))
		{
			throw new ArgumentOutOfRangeException("end", end, Exception<ArgumentOutOfRangeException>.DefaultMessage);
		}
		End = end;
		Source = source;
	}

	public Location WithPosition(Position start, Position end)
	{
		return new Location(start, end, Source);
	}

	public override bool Equals(object obj)
	{
		if (obj is Location other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(Location other)
	{
		if (Start.Equals(other.Start) && End.Equals(other.End))
		{
			return string.Equals(Source, other.Source);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Start.GetHashCode() * 397) ^ (End.GetHashCode() * 397) ^ (Source?.GetHashCode() ?? 0);
	}

	public override string ToString()
	{
		object arg = Start;
		object arg2 = End;
		string source = Source;
		return string.Format("{0}...{1}{2}", arg, arg2, (source != null) ? (": " + source) : null);
	}

	public static bool operator ==(Location left, Location right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Location left, Location right)
	{
		return !left.Equals(right);
	}

	public void Deconstruct(out Position start, out Position end)
	{
		start = Start;
		end = End;
	}

	public void Deconstruct(out Position start, out Position end, out string source)
	{
		start = Start;
		end = End;
		source = Source;
	}
}
