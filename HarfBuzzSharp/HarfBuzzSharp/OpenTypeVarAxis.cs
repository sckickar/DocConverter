using System;

namespace HarfBuzzSharp;

public struct OpenTypeVarAxis : IEquatable<OpenTypeVarAxis>
{
	private uint tag;

	private OpenTypeNameId name_id;

	private float min_value;

	private float default_value;

	private float max_value;

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

	public OpenTypeNameId NameId
	{
		readonly get
		{
			return name_id;
		}
		set
		{
			name_id = value;
		}
	}

	public float MinValue
	{
		readonly get
		{
			return min_value;
		}
		set
		{
			min_value = value;
		}
	}

	public float DefaultValue
	{
		readonly get
		{
			return default_value;
		}
		set
		{
			default_value = value;
		}
	}

	public float MaxValue
	{
		readonly get
		{
			return max_value;
		}
		set
		{
			max_value = value;
		}
	}

	public readonly bool Equals(OpenTypeVarAxis obj)
	{
		if (tag == obj.tag && name_id == obj.name_id && min_value == obj.min_value && default_value == obj.default_value)
		{
			return max_value == obj.max_value;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is OpenTypeVarAxis obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(OpenTypeVarAxis left, OpenTypeVarAxis right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(OpenTypeVarAxis left, OpenTypeVarAxis right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(tag);
		hashCode.Add(name_id);
		hashCode.Add(min_value);
		hashCode.Add(default_value);
		hashCode.Add(max_value);
		return hashCode.ToHashCode();
	}
}
