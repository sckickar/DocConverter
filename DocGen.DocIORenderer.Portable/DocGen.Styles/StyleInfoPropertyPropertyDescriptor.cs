using System;
using System.ComponentModel;

namespace DocGen.Styles;

internal class StyleInfoPropertyPropertyDescriptor : PropertyDescriptor
{
	private StyleInfoProperty sip;

	private Type type;

	public override Type ComponentType => type;

	public override string DisplayName => sip.PropertyName;

	public override bool IsReadOnly => Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);

	public override Type PropertyType => sip.PropertyType;

	public StyleInfoPropertyPropertyDescriptor(StyleInfoProperty sip, Type type, Attribute[] atts)
		: base(sip.PropertyName, atts)
	{
		this.type = type;
		this.sip = sip;
	}

	public override bool CanResetValue(object comp)
	{
		if (!(comp is StyleInfoBase styleInfoBase))
		{
			return false;
		}
		return styleInfoBase.HasValue(sip);
	}

	public override object GetValue(object comp)
	{
		if (!(comp is StyleInfoBase styleInfoBase))
		{
			return null;
		}
		return styleInfoBase.GetValue(sip);
	}

	public override void ResetValue(object comp)
	{
		if (comp is StyleInfoBase styleInfoBase)
		{
			styleInfoBase.ResetValue(sip);
		}
	}

	public override void SetValue(object comp, object value)
	{
		if (comp is StyleInfoBase styleInfoBase)
		{
			styleInfoBase.SetValue(sip, value);
		}
	}

	public override bool ShouldSerializeValue(object comp)
	{
		if (!(comp is StyleInfoBase styleInfoBase))
		{
			return false;
		}
		return styleInfoBase.HasValue(sip);
	}
}
