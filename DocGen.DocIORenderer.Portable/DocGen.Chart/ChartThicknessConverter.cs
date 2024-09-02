using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace DocGen.Chart;

internal class ChartThicknessConverter : TypeConverter
{
	private const string PROP_LEFT = "Left";

	private const string PROP_TOP = "Top";

	private const string PROP_RIGHT = "Right";

	private const string PROP_BOTTOM = "Bottom";

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (!base.CanConvertTo(context, destinationType))
		{
			return destinationType == typeof(InstanceDescriptor);
		}
		return true;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (!base.CanConvertFrom(context, sourceType))
		{
			return sourceType == typeof(string);
		}
		return true;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			return ChartThickness.Parse(value as string);
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value is ChartThickness chartThickness)
		{
			if (destinationType == typeof(string))
			{
				return chartThickness.ToString();
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				ConstructorInfo constructor = typeof(ChartThickness).GetConstructor(new Type[4]
				{
					typeof(float),
					typeof(float),
					typeof(float),
					typeof(float)
				});
				if (constructor != null)
				{
					return new InstanceDescriptor(constructor, new object[4] { chartThickness.Left, chartThickness.Top, chartThickness.Right, chartThickness.Bottom });
				}
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
	{
		return new ChartThickness((float)propertyValues["Left"], (float)propertyValues["Top"], (float)propertyValues["Right"], (float)propertyValues["Bottom"]);
	}

	public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		return TypeDescriptor.GetProperties(typeof(ChartThickness), attributes).Sort(new string[4] { "Left", "Top", "Right", "Bottom" });
	}

	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		return true;
	}
}
