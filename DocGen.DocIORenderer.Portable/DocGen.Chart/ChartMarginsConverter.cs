using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace DocGen.Chart;

internal class ChartMarginsConverter : TypeConverter
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
			string text = ((string)value).Trim();
			if (text != "")
			{
				if (culture == null)
				{
					culture = CultureInfo.CurrentCulture;
				}
				string[] array = text.Split(new char[1] { culture.TextInfo.ListSeparator[0] });
				int[] array2 = new int[array.Length];
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = (int)converter.ConvertFromString(context, culture, array[i]);
				}
				return new ChartMargins(array2[0], array2[1], array2[2], array2[3]);
			}
			return null;
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value is ChartMargins)
		{
			ChartMargins chartMargins = value as ChartMargins;
			if (destinationType == typeof(string))
			{
				if (culture == null)
				{
					culture = CultureInfo.CurrentCulture;
				}
				string separator = culture.TextInfo.ListSeparator + " ";
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
				string[] value2 = new string[4]
				{
					converter.ConvertToString(context, culture, chartMargins.Left),
					converter.ConvertToString(context, culture, chartMargins.Top),
					converter.ConvertToString(context, culture, chartMargins.Right),
					converter.ConvertToString(context, culture, chartMargins.Bottom)
				};
				return string.Join(separator, value2);
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				ConstructorInfo constructor = typeof(ChartMargins).GetConstructor(new Type[4]
				{
					typeof(int),
					typeof(int),
					typeof(int),
					typeof(int)
				});
				if (constructor != null)
				{
					return new InstanceDescriptor(constructor, new object[4] { chartMargins.Left, chartMargins.Top, chartMargins.Right, chartMargins.Bottom });
				}
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
	{
		return new ChartMargins((int)propertyValues["Left"], (int)propertyValues["Top"], (int)propertyValues["Right"], (int)propertyValues["Bottom"]);
	}

	public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		return TypeDescriptor.GetProperties(typeof(ChartMargins), attributes).Sort(new string[4] { "Left", "Top", "Right", "Bottom" });
	}

	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		return true;
	}
}
