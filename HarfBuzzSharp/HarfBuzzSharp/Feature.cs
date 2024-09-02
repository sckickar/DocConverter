using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

public struct Feature : IEquatable<Feature>
{
	private const int MaxFeatureStringSize = 128;

	private uint tag;

	private uint value;

	private uint start;

	private uint end;

	public Tag Tag
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

	public uint Value
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

	public uint Start
	{
		readonly get
		{
			return start;
		}
		set
		{
			start = value;
		}
	}

	public uint End
	{
		readonly get
		{
			return end;
		}
		set
		{
			end = value;
		}
	}

	public Feature(Tag tag)
		: this(tag, 1u, 0u, uint.MaxValue)
	{
	}

	public Feature(Tag tag, uint value)
		: this(tag, value, 0u, uint.MaxValue)
	{
	}

	public Feature(Tag tag, uint value, uint start, uint end)
	{
		this.tag = tag;
		this.value = value;
		this.start = start;
		this.end = end;
	}

	public unsafe override string ToString()
	{
		fixed (Feature* feature = &this)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(128);
			HarfBuzzApi.hb_feature_to_string(feature, (void*)intPtr, 128u);
			string result = Marshal.PtrToStringAnsi(intPtr);
			Marshal.FreeHGlobal(intPtr);
			return result;
		}
	}

	public unsafe static bool TryParse(string s, out Feature feature)
	{
		fixed (Feature* feature2 = &feature)
		{
			return HarfBuzzApi.hb_feature_from_string(s, -1, feature2);
		}
	}

	public static Feature Parse(string s)
	{
		if (!TryParse(s, out var feature))
		{
			throw new FormatException("Unrecognized feature string format.");
		}
		return feature;
	}

	public readonly bool Equals(Feature obj)
	{
		if (tag == obj.tag && value == obj.value && start == obj.start)
		{
			return end == obj.end;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is Feature obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(Feature left, Feature right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Feature left, Feature right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(tag);
		hashCode.Add(value);
		hashCode.Add(start);
		hashCode.Add(end);
		return hashCode.ToHashCode();
	}
}
