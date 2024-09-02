using System;
using System.Reflection;

namespace DocGen.Styles;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class SerializePropertyAttribute : Attribute
{
	private bool serializeProperty;

	public static readonly SerializePropertyAttribute Yes;

	public static readonly SerializePropertyAttribute No;

	public static readonly SerializePropertyAttribute Default;

	public bool SerializeProperty => serializeProperty;

	static SerializePropertyAttribute()
	{
		Yes = new SerializePropertyAttribute(serializeProperty: true);
		No = new SerializePropertyAttribute(serializeProperty: false);
		Default = Yes;
	}

	public SerializePropertyAttribute(bool serializeProperty)
	{
		this.serializeProperty = serializeProperty;
	}

	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}

	public override int GetHashCode()
	{
		return serializeProperty.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is SerializePropertyAttribute serializePropertyAttribute)
		{
			return serializePropertyAttribute.SerializeProperty == serializeProperty;
		}
		return false;
	}

	public static bool IsSerializeProperty(PropertyInfo info)
	{
		SerializePropertyAttribute serializePropertyAttribute = Default;
		if (info != null && info.IsDefined(typeof(SerializePropertyAttribute), inherit: true))
		{
			serializePropertyAttribute = (SerializePropertyAttribute)info.GetCustomAttributes(typeof(SerializePropertyAttribute), inherit: true)[0];
		}
		return serializePropertyAttribute.SerializeProperty;
	}
}
