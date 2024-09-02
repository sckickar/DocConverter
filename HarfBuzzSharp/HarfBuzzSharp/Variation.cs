using System;

namespace HarfBuzzSharp;

public struct Variation : IEquatable<Variation>
{
	private uint tag;

	private float value;

	public uint Tag
	{
		readonly get
		{
			return tag;
		}
		set
		{
			tag = value;
		}
	}

	public float Value
	{
		readonly get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public readonly bool Equals(Variation obj)
	{
		if (tag == obj.tag)
		{
			return value == obj.value;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is Variation obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(Variation left, Variation right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Variation left, Variation right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(tag);
		hashCode.Add(value);
		return hashCode.ToHashCode();
	}
}
