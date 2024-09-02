using System;

namespace HarfBuzzSharp;

public struct OpenTypeNameEntry : IEquatable<OpenTypeNameEntry>
{
	private OpenTypeNameId name_id;

	private int var;

	private IntPtr language;

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

	public int Var
	{
		readonly get
		{
			return var;
		}
		set
		{
			var = value;
		}
	}

	public IntPtr Language
	{
		readonly get
		{
			return language;
		}
		set
		{
			language = value;
		}
	}

	public readonly bool Equals(OpenTypeNameEntry obj)
	{
		if (name_id == obj.name_id && var == obj.var)
		{
			return language == obj.language;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is OpenTypeNameEntry obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(OpenTypeNameEntry left, OpenTypeNameEntry right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(OpenTypeNameEntry left, OpenTypeNameEntry right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(name_id);
		hashCode.Add(var);
		hashCode.Add(language);
		return hashCode.ToHashCode();
	}
}
