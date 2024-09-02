using System;

namespace SkiaSharp;

internal struct SKRunBufferInternal : IEquatable<SKRunBufferInternal>
{
	public unsafe void* glyphs;

	public unsafe void* pos;

	public unsafe void* utf8text;

	public unsafe void* clusters;

	public unsafe readonly bool Equals(SKRunBufferInternal obj)
	{
		if (glyphs == obj.glyphs && pos == obj.pos && utf8text == obj.utf8text)
		{
			return clusters == obj.clusters;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKRunBufferInternal obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKRunBufferInternal left, SKRunBufferInternal right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKRunBufferInternal left, SKRunBufferInternal right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(glyphs);
		hashCode.Add(pos);
		hashCode.Add(utf8text);
		hashCode.Add(clusters);
		return hashCode.ToHashCode();
	}
}
