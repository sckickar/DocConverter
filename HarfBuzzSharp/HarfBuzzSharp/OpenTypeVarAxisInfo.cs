using System;

namespace HarfBuzzSharp;

public struct OpenTypeVarAxisInfo : IEquatable<OpenTypeVarAxisInfo>
{
	private uint axis_index;

	private uint tag;

	private OpenTypeNameId name_id;

	private OpenTypeVarAxisFlags flags;

	private float min_value;

	private float default_value;

	private float max_value;

	private uint reserved;

	public uint AxisIndex
	{
		readonly get
		{
			return axis_index;
		}
		set
		{
			axis_index = value;
		}
	}

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

	public OpenTypeVarAxisFlags Flags
	{
		readonly get
		{
			return flags;
		}
		set
		{
			flags = value;
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

	public readonly bool Equals(OpenTypeVarAxisInfo obj)
	{
		if (axis_index == obj.axis_index && tag == obj.tag && name_id == obj.name_id && flags == obj.flags && min_value == obj.min_value && default_value == obj.default_value && max_value == obj.max_value)
		{
			return reserved == obj.reserved;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is OpenTypeVarAxisInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(axis_index);
		hashCode.Add(tag);
		hashCode.Add(name_id);
		hashCode.Add(flags);
		hashCode.Add(min_value);
		hashCode.Add(default_value);
		hashCode.Add(max_value);
		hashCode.Add(reserved);
		return hashCode.ToHashCode();
	}
}
