using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace DocGen.Chart;

internal class ChartInstanceConverter : TypeConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return new InstanceDescriptor(value.GetType().GetConstructor(Type.EmptyTypes), null, isComplete: false);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
