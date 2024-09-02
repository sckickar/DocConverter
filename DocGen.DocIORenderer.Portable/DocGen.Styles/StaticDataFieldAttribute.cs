using System;

namespace DocGen.Styles;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class StaticDataFieldAttribute : Attribute
{
	private string fieldName;

	public static readonly StaticDataFieldAttribute Default = new StaticDataFieldAttribute("staticDataStore");

	public string FieldName => fieldName;

	public StaticDataFieldAttribute(string fieldName)
	{
		this.fieldName = fieldName;
	}

	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}

	public override int GetHashCode()
	{
		return fieldName.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is StaticDataFieldAttribute staticDataFieldAttribute)
		{
			return staticDataFieldAttribute.fieldName == fieldName;
		}
		return false;
	}
}
