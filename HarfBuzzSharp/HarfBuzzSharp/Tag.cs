using System;

namespace HarfBuzzSharp;

public struct Tag : IEquatable<Tag>
{
	public static readonly Tag None = new Tag(0, 0, 0, 0);

	public static readonly Tag Max = new Tag(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static readonly Tag MaxSigned = new Tag(127, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private readonly uint value;

	private Tag(uint value)
	{
		this.value = value;
	}

	private Tag(byte c1, byte c2, byte c3, byte c4)
	{
		value = (uint)((c1 << 24) | (c2 << 16) | (c3 << 8) | c4);
	}

	public Tag(char c1, char c2, char c3, char c4)
	{
		value = (uint)(((byte)c1 << 24) | ((byte)c2 << 16) | ((byte)c3 << 8) | (byte)c4);
	}

	public static Tag Parse(string tag)
	{
		if (string.IsNullOrEmpty(tag))
		{
			return None;
		}
		char[] array = new char[4];
		int num = Math.Min(4, tag.Length);
		int i;
		for (i = 0; i < num; i++)
		{
			array[i] = tag[i];
		}
		for (; i < 4; i++)
		{
			array[i] = ' ';
		}
		return new Tag(array[0], array[1], array[2], array[3]);
	}

	public override string ToString()
	{
		if (value == (uint)None)
		{
			return "None";
		}
		if (value == (uint)Max)
		{
			return "Max";
		}
		if (value == (uint)MaxSigned)
		{
			return "MaxSigned";
		}
		return string.Concat((char)(byte)(value >> 24), (char)(byte)(value >> 16), (char)(byte)(value >> 8), (char)(byte)value);
	}

	public static implicit operator uint(Tag tag)
	{
		return tag.value;
	}

	public static implicit operator Tag(uint tag)
	{
		return new Tag(tag);
	}

	public override bool Equals(object obj)
	{
		if (obj is Tag tag)
		{
			return value.Equals(tag.value);
		}
		return false;
	}

	public bool Equals(Tag other)
	{
		return value == other.value;
	}

	public override int GetHashCode()
	{
		return (int)value;
	}
}
