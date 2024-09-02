using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Esprima;

public readonly struct Position : IEquatable<Position>
{
	public int Line { get; }

	public int Column { get; }

	public Position(int line, int column)
	{
		Line = ((line >= 0) ? line : ThrowArgumentOutOfRangeException("line", line));
		Column = (((line > 0 && column >= 0) || (line == 0 && column == 0)) ? column : ThrowArgumentOutOfRangeException("column", column));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static int ThrowArgumentOutOfRangeException(string name, int column)
	{
		throw new ArgumentOutOfRangeException(name, column, Exception<ArgumentOutOfRangeException>.DefaultMessage);
	}

	public override bool Equals(object obj)
	{
		if (obj is Position other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(Position other)
	{
		if (Line == other.Line)
		{
			return Column == other.Column;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Line * 397) ^ Column;
	}

	public override string ToString()
	{
		return Line.ToString(CultureInfo.InvariantCulture) + "," + Column.ToString(CultureInfo.InvariantCulture);
	}

	public static bool operator ==(Position left, Position right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Position left, Position right)
	{
		return !left.Equals(right);
	}

	public void Deconstruct(out int line, out int column)
	{
		line = Line;
		column = Column;
	}
}
