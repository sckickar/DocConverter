using System;
using System.Collections.Generic;
using System.Reflection;

namespace DocGen.Drawing;

internal class ColorConverter
{
	private static Dictionary<string, object> colorConstants;

	private static Dictionary<string, object> systemColorConstants;

	internal static Dictionary<string, object> Colors
	{
		get
		{
			if (colorConstants == null && colorConstants == null)
			{
				Dictionary<string, object> hash = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				FillConstants(hash, typeof(Color));
				colorConstants = hash;
			}
			return colorConstants;
		}
	}

	internal static Dictionary<string, object> SystemColors
	{
		get
		{
			if (systemColorConstants == null && systemColorConstants == null)
			{
				Dictionary<string, object> hash = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				FillConstants(hash, typeof(SystemColors));
				systemColorConstants = hash;
			}
			return systemColorConstants;
		}
	}

	public static Color FromKnownColor(KnownColor color)
	{
		return FromKnownColor(color.ToString());
	}

	internal static Color FromKnownColor(string color)
	{
		Color result = Color.Empty;
		if (SystemColors.ContainsKey(color.ToLower()))
		{
			object obj = SystemColors[color.ToLower()];
			if (obj != null)
			{
				result = (Color)obj;
			}
		}
		else if (Colors.ContainsKey(color.ToLower()))
		{
			object obj2 = Colors[color.ToLower()];
			if (obj2 != null)
			{
				result = (Color)obj2;
			}
		}
		return result;
	}

	private static void FillConstants(Dictionary<string, object> hash, Type enumType)
	{
		MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.Static;
		PropertyInfo[] array = new List<PropertyInfo>(enumType.GetRuntimeProperties()).ToArray();
		foreach (PropertyInfo propertyInfo in array)
		{
			if (propertyInfo.PropertyType == typeof(Color))
			{
				MethodInfo getMethod = propertyInfo.GetMethod;
				if (getMethod != null && (getMethod.Attributes & methodAttributes) == methodAttributes)
				{
					object[] index = null;
					hash[propertyInfo.Name.ToLower()] = propertyInfo.GetValue(null, index);
				}
			}
		}
	}
}
