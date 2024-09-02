using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace DocGen.ComponentModel;

internal class TraceProperties
{
	public static bool IsTraceProperty(PropertyInfo info)
	{
		TracePropertyAttribute tracePropertyAttribute = TracePropertyAttribute.Default;
		if (info.IsDefined(typeof(TracePropertyAttribute), inherit: true))
		{
			tracePropertyAttribute = (TracePropertyAttribute)info.GetCustomAttributes(typeof(TracePropertyAttribute), inherit: true)[0];
		}
		return tracePropertyAttribute.TraceProperty;
	}

	public static string ToString(object obj)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(obj.GetType().Name);
		stringBuilder.Append(" { ");
		bool flag = false;
		PropertyInfo[] properties = obj.GetType().GetProperties();
		if (properties != null)
		{
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
				if (propertyInfo.CanRead && converter.CanConvertTo(typeof(string)) && IsTraceProperty(propertyInfo))
				{
					if (!flag)
					{
						flag = true;
					}
					else
					{
						stringBuilder.Append(", ");
					}
					string text = (string)converter.ConvertTo(propertyInfo.GetValue(obj, null), typeof(string));
					stringBuilder.Append(propertyInfo.Name);
					stringBuilder.Append("=");
					if (propertyInfo.PropertyType == typeof(string))
					{
						stringBuilder.Append("\"" + text + "\"");
					}
					else
					{
						stringBuilder.Append(text);
					}
				}
			}
		}
		stringBuilder.Append(" }");
		return stringBuilder.ToString();
	}
}
