using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace DocGen.Chart;

internal class MinMaxInfoConverter : TypeConverter
{
	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(MinMaxInfo), attributes);
		string[] names = new string[3] { "Min", "Max", "Interval" };
		return properties.Sort(names);
	}

	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
	{
		return new MinMaxInfo((double)propertyValues["Min"], (double)propertyValues["Max"], (double)propertyValues["Interval"]);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		bool result = base.CanConvertFrom(context, sourceType);
		if (sourceType == typeof(string))
		{
			result = true;
		}
		return result;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		base.CanConvertTo(context, destinationType);
		_ = destinationType == typeof(string);
		return true;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		object result = null;
		if (value is string)
		{
			string text = ((string)value).Trim();
			if (text.Length != 0)
			{
				culture = ((culture == null) ? CultureInfo.CurrentCulture : culture);
				char[] separator = new char[1] { culture.TextInfo.ListSeparator[0] };
				string[] array = text.Split(separator);
				double[] array2 = new double[array.Length];
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(double));
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = (double)converter.ConvertFromString(context, culture, array[i]);
				}
				if (array2.Length == 3)
				{
					result = new MinMaxInfo(array2[0], array2[1], array2[2]);
				}
			}
		}
		else
		{
			result = base.ConvertFrom(context, culture, value);
		}
		return result;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		object result = null;
		if (value is MinMaxInfo)
		{
			MinMaxInfo minMaxInfo = value as MinMaxInfo;
			if (destinationType == typeof(string))
			{
				culture = ((culture == null) ? CultureInfo.CurrentCulture : culture);
				string separator = culture.TextInfo.ListSeparator[0] + " ";
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(double));
				result = string.Join(separator, converter.ConvertToString(context, culture, minMaxInfo.Min), converter.ConvertToString(context, culture, minMaxInfo.Max), converter.ConvertToString(context, culture, minMaxInfo.Interval));
			}
			else if (destinationType == typeof(InstanceDescriptor))
			{
				Type[] types = new Type[3]
				{
					typeof(double),
					typeof(double),
					typeof(double)
				};
				ConstructorInfo constructor = typeof(MinMaxInfo).GetConstructor(types);
				if (constructor != null)
				{
					object[] arguments = new object[3] { minMaxInfo.Min, minMaxInfo.Max, minMaxInfo.Interval };
					result = new InstanceDescriptor(constructor, arguments);
				}
			}
		}
		else
		{
			result = base.ConvertTo(context, culture, value, destinationType);
		}
		return result;
	}
}
