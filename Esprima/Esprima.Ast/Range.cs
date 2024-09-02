using System;
using System.Globalization;

namespace Esprima.Ast;

public readonly struct Range : IEquatable<Range>
{
	public readonly int Start;

	public readonly int End;

	public Range(int start, int end)
	{
		Start = start;
		End = end;
	}

	public bool Equals(Range other)
	{
		if (Start == other.Start)
		{
			return End == other.End;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Range other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Start * 397) ^ End;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "[{0}..{1})", Start, End);
	}

	public static bool operator ==(Range left, Range right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Range left, Range right)
	{
		return !left.Equals(right);
	}
}
