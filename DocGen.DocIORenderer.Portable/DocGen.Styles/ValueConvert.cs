using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using DocGen.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Styles;

internal class ValueConvert
{
	private static Hashtable cachedDefaultValues = new Hashtable();

	private static bool allowFormatValueTrimEnd = false;

	public static bool AllowFormatValueTrimEnd
	{
		get
		{
			return allowFormatValueTrimEnd;
		}
		set
		{
			allowFormatValueTrimEnd = value;
		}
	}

	protected ValueConvert()
	{
	}

	public static object ChangeType(object value, Type type, IFormatProvider provider)
	{
		return ChangeType(value, type, provider, returnDbNUllIfNotValid: false);
	}

	public static object ChangeType(object value, Type type, IFormatProvider provider, bool returnDbNUllIfNotValid)
	{
		return ChangeType(value, type, provider, "", returnDbNUllIfNotValid);
	}

	public static object ChangeType(object value, Type type, IFormatProvider provider, string format, bool returnDbNUllIfNotValid)
	{
		Type underlyingType = Nullable.GetUnderlyingType(type);
		if (underlyingType != null)
		{
			value = ChangeType(value, underlyingType, provider, returnDbNUllIfNotValid: true);
			return NullableHelper.FixDbNUllasNull(value, type);
		}
		if (value != null && !type.IsAssignableFrom(value.GetType()))
		{
			try
			{
				if (value is string)
				{
					value = ((format == null || format.Length <= 0) ? Parse((string)value, type, provider, "", returnDbNUllIfNotValid) : Parse((string)value, type, provider, format, returnDbNUllIfNotValid));
				}
				else if (!(value is DBNull))
				{
					if (!type.IsEnum)
					{
						value = ((!(type == typeof(string)) || value is IConvertible) ? NullableHelper.ChangeType(value, type, provider) : ((value != null) ? value.ToString() : ""));
					}
					else
					{
						value = Convert.ChangeType(value, typeof(int), provider);
						value = Enum.ToObject(type, (int)value);
					}
				}
			}
			catch
			{
				if (returnDbNUllIfNotValid)
				{
					return Convert.DBNull;
				}
				throw;
			}
		}
		if ((value == null || value is DBNull) && type == typeof(string))
		{
			return "";
		}
		return value;
	}

	private static object Parse(string s, Type resultType, IFormatProvider provider)
	{
		return Parse(s, resultType, provider, "");
	}

	public static object Parse(string s, Type resultType, IFormatProvider provider, string format)
	{
		return Parse(s, resultType, provider, format, returnDbNUllIfNotValid: false);
	}

	public static object Parse(string s, Type resultType, IFormatProvider provider, string format, bool returnDbNUllIfNotValid)
	{
		return NullableHelper.FixDbNUllasNull(_Parse(s, resultType, provider, format, returnDbNUllIfNotValid), resultType);
	}

	public static object Parse(string s, Type resultType, IFormatProvider provider, string[] formats, bool returnDbNUllIfNotValid)
	{
		return NullableHelper.FixDbNUllasNull(_Parse(s, resultType, provider, "", formats, returnDbNUllIfNotValid), resultType);
	}

	private static object _Parse(string s, Type resultType, IFormatProvider provider, string format, bool returnDbNUllIfNotValid)
	{
		return _Parse(s, resultType, provider, format, null, returnDbNUllIfNotValid);
	}

	private static object _Parse(string s, Type resultType, IFormatProvider provider, string format, string[] formats, bool returnDbNUllIfNotValid)
	{
		if (resultType == null)
		{
			return s;
		}
		try
		{
			if (typeof(double).IsAssignableFrom(resultType))
			{
				if (IsEmpty(s))
				{
					return Convert.DBNull;
				}
				if (double.TryParse(s, NumberStyles.Any, provider, out var result))
				{
					return Convert.ChangeType(result, resultType, provider);
				}
				if (returnDbNUllIfNotValid && (resultType == typeof(double) || resultType == typeof(float)))
				{
					return Convert.DBNull;
				}
			}
			else if (typeof(decimal).IsAssignableFrom(resultType))
			{
				if (IsEmpty(s))
				{
					return Convert.DBNull;
				}
				if (decimal.TryParse(s, NumberStyles.Any, provider, out var result2))
				{
					return Convert.ChangeType(result2, resultType, provider);
				}
			}
			else if (typeof(DateTime).IsAssignableFrom(resultType))
			{
				if (IsEmpty(s))
				{
					return Convert.DBNull;
				}
				if (formats == null || (formats.GetLength(0) == 0 && format.Length > 0))
				{
					formats = new string[7] { format, "G", "g", "f", "F", "d", "D" };
				}
				if (formats != null && formats.GetLength(0) > 0 && DateTime.TryParseExact(s, formats, provider, DateTimeStyles.AllowWhiteSpaces, out var result3))
				{
					return result3;
				}
				if (DateTime.TryParse(s, provider, DateTimeStyles.AllowWhiteSpaces, out var result4))
				{
					return result4;
				}
				if (returnDbNUllIfNotValid)
				{
					return Convert.DBNull;
				}
			}
			else if (typeof(bool).IsAssignableFrom(resultType))
			{
				if (IsEmpty(s))
				{
					return Convert.DBNull;
				}
				if (s == "1" || s.ToUpper() == bool.TrueString.ToUpper())
				{
					return true;
				}
				if (s == "0" || s.ToUpper() == bool.TrueString.ToUpper())
				{
					return false;
				}
			}
			else if (typeof(long).IsAssignableFrom(resultType))
			{
				if (IsEmpty(s))
				{
					return Convert.DBNull;
				}
				if (long.TryParse(s, NumberStyles.Any, provider, out var result5))
				{
					return Convert.ChangeType(result5, resultType, provider);
				}
				if (returnDbNUllIfNotValid && resultType.IsPrimitive && !resultType.IsEnum)
				{
					return Convert.DBNull;
				}
			}
			else if (typeof(ulong).IsAssignableFrom(resultType))
			{
				if (IsEmpty(s))
				{
					return Convert.DBNull;
				}
				if (ulong.TryParse(s, NumberStyles.Any, provider, out var result6))
				{
					return Convert.ChangeType(result6, resultType, provider);
				}
				if (returnDbNUllIfNotValid && resultType.IsPrimitive && !resultType.IsEnum)
				{
					return Convert.DBNull;
				}
			}
			else if (typeof(int).IsAssignableFrom(resultType) || typeof(short).IsAssignableFrom(resultType) || typeof(float).IsAssignableFrom(resultType) || typeof(uint).IsAssignableFrom(resultType) || typeof(ushort).IsAssignableFrom(resultType) || typeof(byte).IsAssignableFrom(resultType))
			{
				if (IsEmpty(s))
				{
					return Convert.DBNull;
				}
				if (double.TryParse(s, NumberStyles.Any, provider, out var result7))
				{
					return Convert.ChangeType(result7, resultType, provider);
				}
				if (returnDbNUllIfNotValid && resultType.IsPrimitive && !resultType.IsEnum)
				{
					return Convert.DBNull;
				}
			}
			else if (resultType == typeof(Type))
			{
				return Type.GetType(s);
			}
			TypeConverter converter = TypeDescriptor.GetConverter(resultType);
			if (converter is NullableConverter)
			{
				Type underlyingType = NullableHelper.GetUnderlyingType(resultType);
				if (underlyingType != null)
				{
					return _Parse(s, underlyingType, provider, format, formats, returnDbNUllIfNotValid);
				}
			}
			if (converter != null && converter.CanConvertFrom(typeof(string)) && s != null && s.Length > 0)
			{
				return (!(provider is CultureInfo)) ? converter.ConvertFrom(s) : converter.ConvertFrom(null, (CultureInfo)provider, s);
			}
		}
		catch
		{
			if (returnDbNUllIfNotValid)
			{
				return Convert.DBNull;
			}
			throw;
		}
		return Convert.DBNull;
	}

	public static string FormatValue(object value, Type valueType, string format, CultureInfo ci, NumberFormatInfo nfi)
	{
		string text;
		try
		{
			if (value is string)
			{
				return (string)value;
			}
			if (value is byte[] || value is Image)
			{
				return "";
			}
			object obj;
			if (value == null || valueType == null || value.GetType() == valueType)
			{
				obj = value;
			}
			else
			{
				try
				{
					obj = ChangeType(value, valueType, ci, returnDbNUllIfNotValid: true);
				}
				catch (Exception ex)
				{
					obj = value;
					if (!(ex is FormatException) && !(ex.InnerException is FormatException))
					{
						throw;
					}
				}
			}
			if (obj == null || obj is DBNull)
			{
				text = string.Empty;
			}
			else if (obj is IFormattable)
			{
				IFormattable formattable = (IFormattable)obj;
				IFormatProvider formatProvider = null;
				if (nfi != null && !(obj is DateTime))
				{
					formatProvider = nfi;
				}
				else if (ci != null)
				{
					IFormatProvider formatProvider2;
					if (!(obj is DateTime))
					{
						IFormatProvider numberFormat = ci.NumberFormat;
						formatProvider2 = numberFormat;
					}
					else
					{
						IFormatProvider numberFormat = ci.DateTimeFormat;
						formatProvider2 = numberFormat;
					}
					formatProvider = formatProvider2;
				}
				text = (((format == null || format.Length <= 0) && nfi == null) ? formattable.ToString() : formattable.ToString(format, formatProvider));
			}
			else
			{
				TypeConverter converter = TypeDescriptor.GetConverter(obj.GetType());
				text = (converter.CanConvertTo(typeof(string)) ? ((string)converter.ConvertTo(null, ci, obj, typeof(string))) : ((!(obj is IConvertible)) ? obj.ToString() : Convert.ToString(obj, ci)));
			}
		}
		catch
		{
			text = string.Empty;
			throw;
		}
		if (text == null)
		{
			text = string.Empty;
		}
		if (allowFormatValueTrimEnd)
		{
			text = text.TrimEnd();
		}
		return text;
	}

	public static object GetDefaultValue(Type type)
	{
		if (type == null)
		{
			return "0";
		}
		lock (cachedDefaultValues)
		{
			object obj;
			if (cachedDefaultValues.Contains(type))
			{
				obj = cachedDefaultValues[type];
			}
			else
			{
				switch (type.FullName)
				{
				case "System.Double":
				case "System.Single":
				case "System.Decimal":
					obj = 123.4567;
					break;
				case "System.Boolean":
					obj = true;
					break;
				case "System.Drawing.Color":
					obj = System.Drawing.Color.Black;
					break;
				case "System.String":
					obj = string.Empty;
					break;
				case "System.DateTime":
					obj = DateTime.Now;
					break;
				case "System.UInt16":
				case "System.UInt32":
				case "System.UInt64":
				case "System.Int64":
				case "System.SByte":
				case "System.Int16":
				case "System.Int32":
				case "System.Byte":
					obj = 123;
					break;
				case "System.Char":
					obj = 'A';
					break;
				case "System.DBNull":
					obj = Convert.DBNull;
					break;
				default:
					obj = "";
					break;
				}
				cachedDefaultValues[type] = obj;
			}
			return obj;
		}
	}

	private static bool ParseValueWithTypeInformation(string valueAsString, out object retVal)
	{
		return ParseValueWithTypeInformation(valueAsString, out retVal);
	}

	public static bool ParseValueWithTypeInformation(string valueAsString, out object retVal, bool allowConvertFromBase64)
	{
		retVal = null;
		if (valueAsString.StartsWith("'") && valueAsString.EndsWith("'"))
		{
			retVal = valueAsString.Substring(1, valueAsString.Length - 2);
			return true;
		}
		if (valueAsString.StartsWith("<"))
		{
			int num = valueAsString.IndexOf(">");
			if (num > 1)
			{
				string text = valueAsString.Substring(1, num - 1);
				if (text == "null")
				{
					retVal = null;
					return true;
				}
				if (text == "System.DBNull")
				{
					retVal = DBNull.Value;
					return true;
				}
				valueAsString = valueAsString.Substring(num + 1).Trim();
				if (valueAsString.StartsWith("'") && valueAsString.EndsWith("'"))
				{
					valueAsString = valueAsString.Substring(1, valueAsString.Length - 2);
					Type type = new ValueConvert().GetType();
					if (type != null)
					{
						bool flag = false;
						if (allowConvertFromBase64)
						{
							flag = TryConvertFromBase64String(type, valueAsString, out retVal);
						}
						if (!flag)
						{
							retVal = Parse(valueAsString, type, CultureInfo.InvariantCulture, "");
						}
						return true;
					}
				}
			}
		}
		retVal = valueAsString;
		return false;
	}

	public static bool TryConvertFromBase64String(Type type, string valueAsString, out object retVal)
	{
		bool result = false;
		retVal = null;
		TypeConverter converter = TypeDescriptor.GetConverter(type);
		if (converter != null)
		{
			if (converter.CanConvertFrom(typeof(byte[])))
			{
				byte[] value = Convert.FromBase64String(valueAsString);
				retVal = converter.ConvertFrom(value);
				result = true;
			}
			else if (converter.CanConvertFrom(typeof(MemoryStream)))
			{
				MemoryStream value2 = new MemoryStream(Convert.FromBase64String(valueAsString));
				retVal = converter.ConvertFrom(value2);
				result = true;
			}
		}
		return result;
	}

	private static string FormatValueWithTypeInformation(object value)
	{
		return FormatValueWithTypeInformation(value, allowConvertToBase64: false);
	}

	public static string FormatValueWithTypeInformation(object value, bool allowConvertToBase64)
	{
		if (value is string)
		{
			return "'" + (string)value + "'";
		}
		if (value is DBNull)
		{
			return "<System.DBNull>";
		}
		if (value == null)
		{
			return "<null>";
		}
		string text = null;
		if (allowConvertToBase64)
		{
			text = TryConvertToBase64String(value);
		}
		if (text == null)
		{
			text = FormatValue(value, typeof(string), "", CultureInfo.InvariantCulture, null);
		}
		return "<" + GetTypeName(value.GetType()) + "> '" + text + "'";
	}

	public static string TryConvertToBase64String(object value)
	{
		string result = null;
		TypeConverter converter = TypeDescriptor.GetConverter(value);
		if (converter != null)
		{
			if (converter.CanConvertTo(typeof(byte[])))
			{
				byte[] array = (byte[])converter.ConvertTo(value, typeof(byte[]));
				if (array != null)
				{
					result = Convert.ToBase64String(array);
				}
			}
			else if (converter.CanConvertTo(typeof(MemoryStream)))
			{
				result = Convert.ToBase64String(((MemoryStream)converter.ConvertTo(value, typeof(MemoryStream))).ToArray());
			}
		}
		return result;
	}

	public static string GetTypeName(Type type)
	{
		if (!type.IsPrimitive && type.Module != typeof(object).Module)
		{
			return type.FullName + ", " + Path.GetFileNameWithoutExtension(type.Module.ScopeName);
		}
		return type.FullName;
	}

	public static bool IsEmpty(string str)
	{
		if (str != null)
		{
			return str.Length == 0;
		}
		return true;
	}
}
