using System;
using System.Reflection;

namespace DocGen.Styles;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class DisposeablePropertyAttribute : Attribute
{
	private bool disposeableProperty;

	public static readonly DisposeablePropertyAttribute Yes;

	public static readonly DisposeablePropertyAttribute No;

	public static readonly DisposeablePropertyAttribute Default;

	public bool DisposeableProperty => disposeableProperty;

	static DisposeablePropertyAttribute()
	{
		Yes = new DisposeablePropertyAttribute(disposeableProperty: true);
		No = new DisposeablePropertyAttribute(disposeableProperty: false);
		Default = Yes;
	}

	public DisposeablePropertyAttribute(bool disposeableProperty)
	{
		this.disposeableProperty = disposeableProperty;
	}

	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}

	public override int GetHashCode()
	{
		return disposeableProperty.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is DisposeablePropertyAttribute disposeablePropertyAttribute)
		{
			return disposeablePropertyAttribute.DisposeableProperty == disposeableProperty;
		}
		return false;
	}

	[Obsolete("IsDisposeableProperty is deprecated, please use IsDisposableProperty instead.")]
	public static bool IsDisposeableProperty(PropertyInfo info)
	{
		return IsDisposableProperty(info);
	}

	public static bool IsDisposableProperty(PropertyInfo info)
	{
		DisposeablePropertyAttribute disposeablePropertyAttribute = Default;
		if (info != null && info.IsDefined(typeof(DisposeablePropertyAttribute), inherit: true))
		{
			disposeablePropertyAttribute = (DisposeablePropertyAttribute)info.GetCustomAttributes(typeof(DisposeablePropertyAttribute), inherit: true)[0];
		}
		return disposeablePropertyAttribute.DisposeableProperty;
	}
}
