using System;
using System.Reflection;

namespace DocGen.Styles;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class CloneablePropertyAttribute : Attribute
{
	private bool cloneableProperty;

	public static readonly CloneablePropertyAttribute Yes;

	public static readonly CloneablePropertyAttribute No;

	public static readonly CloneablePropertyAttribute Default;

	public bool CloneableProperty => cloneableProperty;

	static CloneablePropertyAttribute()
	{
		Yes = new CloneablePropertyAttribute(cloneableProperty: true);
		No = new CloneablePropertyAttribute(cloneableProperty: false);
		Default = Yes;
	}

	public CloneablePropertyAttribute(bool cloneableProperty)
	{
		this.cloneableProperty = cloneableProperty;
	}

	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}

	public override int GetHashCode()
	{
		return cloneableProperty.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is CloneablePropertyAttribute cloneablePropertyAttribute)
		{
			return cloneablePropertyAttribute.CloneableProperty == cloneableProperty;
		}
		return false;
	}

	public static bool IsCloneableProperty(PropertyInfo info)
	{
		CloneablePropertyAttribute cloneablePropertyAttribute = Default;
		if (info != null && info.IsDefined(typeof(CloneablePropertyAttribute), inherit: true))
		{
			cloneablePropertyAttribute = (CloneablePropertyAttribute)info.GetCustomAttributes(typeof(CloneablePropertyAttribute), inherit: true)[0];
		}
		return cloneablePropertyAttribute.CloneableProperty;
	}
}
