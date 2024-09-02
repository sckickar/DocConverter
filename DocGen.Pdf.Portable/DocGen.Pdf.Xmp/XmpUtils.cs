using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

internal class XmpUtils
{
	private const string c_False = "False";

	private const string c_True = "True";

	private const string c_realPattern = "^[+-]?[\\d]+([.]?[\\d])*$";

	private const string c_dateFormat = "yyyy-MM-dd'T'HH:mm:ss.ffzzz";

	private XmpUtils()
	{
		throw new NotImplementedException();
	}

	public static void SetTextValue(XElement parent, string value)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		ClearChildren(parent);
		XText content = new XText(value);
		if (parent.Value != string.Empty)
		{
			parent.Value = string.Empty;
		}
		parent.Add(content);
	}

	public static void SetBoolValue(XElement parent, bool value)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		string value2 = (value ? "True" : "False");
		SetTextValue(parent, value2);
	}

	public static bool GetBoolValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return value.Equals("True");
	}

	public static void SetRealValue(XElement parent, float value)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		string value2 = value.ToString(CultureInfo.InvariantCulture);
		SetTextValue(parent, value2);
	}

	public static float GetRealValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		double result = 0.0;
		double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		return (float)result;
	}

	public static void SetIntValue(XElement parent, int value)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		string value2 = value.ToString(CultureInfo.InvariantCulture);
		SetTextValue(parent, value2);
	}

	public static int GetIntValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		double result = 0.0;
		double.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
		return (int)result;
	}

	public static void SetUriValue(XElement parent, Uri value)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		string value2 = value.ToString();
		SetTextValue(parent, value2);
	}

	public static Uri GetUriValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return new Uri(value);
	}

	public static void SetDateTimeValue(XElement parent, DateTime value)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		string value2 = value.ToString("yyyy-MM-dd'T'HH:mm:ss.ffzzz");
		SetTextValue(parent, value2);
	}

	public static DateTime GetDateTimeValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		DateTime result = DateTime.Now;
		if (value != string.Empty)
		{
			try
			{
				string format = "yyyyMMddHHmmss";
				if (value.Length > 14)
				{
					string[] array = new string[4] { "-", "T", ":", "Z" };
					for (int i = 0; i < array.Length; i++)
					{
						if (value.Contains(array[i]))
						{
							value = value.Replace(array[i], "");
						}
					}
				}
				if (value.Length < 10)
				{
					format = "yyyyMMdd";
					value = value.Substring(0, 8);
				}
				else if (value.Length > 9 && value.Length < 12)
				{
					format = "yyyyMMddHH";
					value = value.Substring(0, 10);
				}
				else if (value.Length > 11 && value.Length < 14)
				{
					format = "yyyyMMddHHmm";
					value = value.Substring(0, 12);
				}
				else if (value.Length > 14)
				{
					value = value.Substring(0, 14);
				}
				DateTime.TryParseExact(value, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite, out result);
			}
			catch
			{
			}
		}
		return result;
	}

	public static void SetXmlValue(XElement parent, XElement child)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		ClearChildren(parent);
		parent.Add(child);
	}

	private static void ClearChildren(XNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		List<XElement> list = (node as XElement).Descendants().ToList();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[0].Remove();
		}
	}
}
