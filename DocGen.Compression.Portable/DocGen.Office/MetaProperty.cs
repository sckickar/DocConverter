using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DocGen.Office;

public class MetaProperty
{
	private string m_id;

	private string m_name;

	private byte m_bFlags;

	private object m_value;

	private MetaPropertyType m_MetaPropertyType;

	private object m_ownerBase;

	public string Id => m_id;

	public bool IsReadOnly
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		internal set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	public bool IsRequired
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		internal set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	public string DisplayName => m_name;

	public MetaPropertyType Type => m_MetaPropertyType;

	public object Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (ChecksValidMetaValue(ref value))
			{
				m_value = value;
				SetValueInXmlDocument();
			}
		}
	}

	internal MetaProperties Parent
	{
		get
		{
			return (MetaProperties)m_ownerBase;
		}
		set
		{
			m_ownerBase = value;
		}
	}

	private void MapContentTypeProperties(string id, string nillable, string isreadonly, string displayName, string type, object value, MetaProperty metaProperty)
	{
		metaProperty.m_id = id;
		metaProperty.m_name = displayName;
		metaProperty.IsRequired = !bool.Parse(nillable);
		metaProperty.IsReadOnly = bool.Parse(isreadonly);
		metaProperty.m_name = displayName;
		foreach (MetaPropertyType value2 in Enum.GetValues(typeof(MetaPropertyType)))
		{
			if (type == "URL")
			{
				type = "Url";
			}
			if (type == value2.ToString())
			{
				metaProperty.m_MetaPropertyType = value2;
				break;
			}
		}
		metaProperty.m_value = GetValueAsPerMetaPropertyType(metaProperty.m_MetaPropertyType, value);
	}

	private void ParseMetaProperty(IEnumerable<XNode> nodes, string id, object value, MetaProperty property, XNamespace nameSpace)
	{
		string text = "";
		string displayName = "";
		string text2 = "";
		string text3 = "";
		bool flag = false;
		for (int i = 0; i < nodes.Count(); i++)
		{
			IEnumerable<XNode> enumerable = null;
			if (nodes.ElementAt(i) is XElement)
			{
				enumerable = ((XElement)nodes.ElementAt(i)).Elements();
			}
			foreach (XNode item in enumerable)
			{
				XElement xElement = null;
				if (item is XElement)
				{
					xElement = (XElement)item;
				}
				if (xElement != null && xElement.Name.LocalName == "element" && xElement.Attribute("name") != null && xElement.Attribute("name").Value == id)
				{
					if (xElement.Attribute("nillable") != null)
					{
						text = xElement.Attribute("nillable").Value;
					}
					if (xElement.Attribute(nameSpace + "displayName") != null)
					{
						displayName = xElement.Attribute(nameSpace + "displayName").Value;
					}
					if (xElement.Attribute(nameSpace + "readOnly") != null)
					{
						text2 = xElement.Attribute(nameSpace + "readOnly").Value;
					}
					foreach (XNode item2 in (IEnumerable<XNode>)xElement.Elements())
					{
						if ((!(item2 is XElement) || !(((XElement)item2).Name.LocalName == "simpleType")) && !(((XElement)item2).Name.LocalName == "complexType"))
						{
							continue;
						}
						foreach (XNode item3 in (IEnumerable<XNode>)((XElement)item2).Elements())
						{
							if (item3 is XElement && ((XElement)item3).Name.LocalName == "restriction")
							{
								if (((XElement)item3).Attribute("base") != null)
								{
									text3 = ((XElement)item3).Attribute("base").Value;
								}
								text3 = text3.Substring(4);
							}
							else
							{
								if (!(item3 is XElement) || !(((XElement)item3).Name.LocalName == "complexContent"))
								{
									continue;
								}
								foreach (XNode item4 in (IEnumerable<XNode>)((XElement)item3).Elements())
								{
									XElement xElement2 = null;
									if (item4 is XElement)
									{
										xElement2 = (XElement)item4;
									}
									if (xElement2 != null && xElement2.Name.LocalName == "extension")
									{
										if (xElement2.Attribute("base") != null)
										{
											text3 = xElement2.Attribute("base").Value;
										}
										text3 = text3.Substring(4);
									}
								}
							}
						}
					}
					if (string.IsNullOrEmpty(text))
					{
						text = "false";
					}
					if (string.IsNullOrEmpty(text2))
					{
						text2 = "false";
					}
					MapContentTypeProperties(id, text, text2, displayName, text3, value, property);
					flag = true;
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
	}

	internal MetaProperties ParseMetaProperty(XElement contentTypeSchema, XDocument contentTypeSchemaProperties)
	{
		object obj = null;
		MetaProperties metaProperties = new MetaProperties();
		metaProperties.m_contentTypeSchemaProperties = contentTypeSchemaProperties;
		if (contentTypeSchemaProperties.Root != null && contentTypeSchemaProperties.Root.FirstNode is XElement && ((XElement)contentTypeSchemaProperties.Root.FirstNode).Name.LocalName == "documentManagement")
		{
			XNamespace namespaceOfPrefix = contentTypeSchema.GetNamespaceOfPrefix("ma");
			foreach (XNode item in (IEnumerable<XNode>)((XElement)contentTypeSchemaProperties.Root.FirstNode).Elements())
			{
				MetaProperty metaProperty = new MetaProperty();
				XElement xElement = (XElement)item;
				if (xElement.HasElements && xElement.Elements().Count() > 1)
				{
					IEnumerable<XElement> enumerable = xElement.Elements();
					List<string> list = new List<string>();
					foreach (XNode item2 in (IEnumerable<XNode>)enumerable)
					{
						if (item2 is XElement)
						{
							list.Add(((XElement)item2).Value);
						}
					}
					obj = list.ToArray();
				}
				else if (xElement.HasElements && xElement.Elements().Count() == 1 && xElement.FirstNode.NodeType != XmlNodeType.Text && xElement.FirstNode is XElement)
				{
					IEnumerable<XNode> enumerable2 = ((XElement)xElement.FirstNode).Elements();
					if (enumerable2.Count() == 0 || (enumerable2.Count() == 1 && enumerable2.ElementAt(0).NodeType == XmlNodeType.Text))
					{
						obj = ((XElement)xElement.FirstNode).Value;
					}
					else
					{
						List<string> list2 = new List<string>();
						foreach (XNode item3 in enumerable2)
						{
							if (item3 is XElement && !((XElement)item3).IsEmpty)
							{
								list2.Add(((XElement)item3).Value);
							}
						}
						obj = list2.ToArray();
					}
				}
				else
				{
					obj = xElement.Value;
				}
				ParseMetaProperty(contentTypeSchema.Elements(), xElement.Name.LocalName, obj, metaProperty, namespaceOfPrefix);
				metaProperties.Add(metaProperty);
			}
		}
		return metaProperties;
	}

	private bool ChecksValidMetaValue(ref object value)
	{
		if (m_MetaPropertyType == MetaPropertyType.Unknown || m_MetaPropertyType == MetaPropertyType.Choice || m_MetaPropertyType == MetaPropertyType.Text || m_MetaPropertyType == MetaPropertyType.Note)
		{
			return ValidateTextNoteChoiceUnknownMetaType(ref value);
		}
		if (m_MetaPropertyType == MetaPropertyType.Boolean)
		{
			return ValidateBooleanMetaType(ref value);
		}
		if (m_MetaPropertyType == MetaPropertyType.DateTime)
		{
			return ValidateDateTimeMetaType(ref value);
		}
		if (m_MetaPropertyType == MetaPropertyType.Number || m_MetaPropertyType == MetaPropertyType.Currency)
		{
			return ValidateNumberAndCurrencyMetaType(ref value);
		}
		if (m_MetaPropertyType == MetaPropertyType.User)
		{
			return ValidateUserMetaType(ref value);
		}
		if (m_MetaPropertyType == MetaPropertyType.Url)
		{
			return ValidateUrlMetaType(ref value);
		}
		if (m_MetaPropertyType == MetaPropertyType.Lookup)
		{
			return ValidateLookupMetaType(ref value);
		}
		return false;
	}

	private object GetValueAsPerMetaPropertyType(MetaPropertyType type, object value)
	{
		switch (type)
		{
		case MetaPropertyType.Unknown:
		case MetaPropertyType.Choice:
		case MetaPropertyType.Lookup:
		case MetaPropertyType.Note:
		case MetaPropertyType.Text:
		case MetaPropertyType.Url:
		case MetaPropertyType.User:
			if (value != "")
			{
				return value;
			}
			return null;
		case MetaPropertyType.Boolean:
			if (!string.IsNullOrEmpty(value.ToString()))
			{
				return Convert.ToBoolean(value);
			}
			break;
		case MetaPropertyType.Currency:
		case MetaPropertyType.Number:
		{
			if (int.TryParse(value.ToString(), out var result) && !string.IsNullOrEmpty(value.ToString()))
			{
				return result;
			}
			if (long.TryParse(value.ToString(), out var result2) && !string.IsNullOrEmpty(value.ToString()))
			{
				return result2;
			}
			if (double.TryParse(value.ToString(), out var result3) && !string.IsNullOrEmpty(value.ToString()))
			{
				return result3;
			}
			if (value != "")
			{
				return value;
			}
			return null;
		}
		case MetaPropertyType.DateTime:
			if (!string.IsNullOrEmpty(value.ToString()))
			{
				return Convert.ToDateTime(value);
			}
			break;
		}
		return null;
	}

	private void SetValueInXmlDocument()
	{
		if (Parent.m_contentTypeSchemaProperties == null || Parent.m_contentTypeSchemaProperties.Root == null || !(Parent.m_contentTypeSchemaProperties.Root.FirstNode is XElement) || !(((XElement)Parent.m_contentTypeSchemaProperties.Root.FirstNode).Name.LocalName == "documentManagement"))
		{
			return;
		}
		foreach (XNode item in (IEnumerable<XNode>)((XElement)Parent.m_contentTypeSchemaProperties.Root.FirstNode).Elements())
		{
			if (!(item is XElement) || ((XElement)item).Name.LocalName != m_id)
			{
				continue;
			}
			if (m_MetaPropertyType == MetaPropertyType.DateTime)
			{
				if (m_value != null)
				{
					((XElement)item).Value = ((DateTime)m_value).ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
				}
				else
				{
					((XElement)item).Value = string.Empty;
				}
			}
			else if (m_MetaPropertyType == MetaPropertyType.Url)
			{
				if (m_value != null)
				{
					int num = 0;
					string[] array = (string[])m_value;
					foreach (string text in array)
					{
						((XElement)item).Elements().ElementAt(num).Value = Convert.ToString((object?)text) ?? string.Empty;
						num++;
					}
				}
			}
			else if (m_MetaPropertyType == MetaPropertyType.User)
			{
				if (m_value == null)
				{
					continue;
				}
				int num2 = 0;
				string[] array2 = (string[])m_value;
				XElement xElement = (XElement)item;
				if (xElement.HasElements && xElement.FirstNode != null)
				{
					IEnumerable<XNode> source = ((XElement)xElement.FirstNode).Elements();
					string[] array = array2;
					foreach (string text2 in array)
					{
						((XElement)source.ElementAt(num2)).Value = Convert.ToString((object?)text2) ?? string.Empty;
						num2++;
					}
				}
			}
			else if (m_MetaPropertyType == MetaPropertyType.Unknown || m_MetaPropertyType == MetaPropertyType.Choice || m_MetaPropertyType == MetaPropertyType.Text || m_MetaPropertyType == MetaPropertyType.Note || m_MetaPropertyType == MetaPropertyType.Number || m_MetaPropertyType == MetaPropertyType.Currency || m_MetaPropertyType == MetaPropertyType.Lookup)
			{
				((XElement)item).Value = Convert.ToString(m_value, CultureInfo.InvariantCulture) ?? string.Empty;
			}
			else if (m_MetaPropertyType == MetaPropertyType.Boolean)
			{
				((XElement)item).Value = Convert.ToString(m_value, CultureInfo.InvariantCulture).ToLower();
			}
		}
	}

	private bool ValidateBooleanMetaType(ref object value)
	{
		if ((value == null || value.GetType() == typeof(bool)) && !IsReadOnly)
		{
			return true;
		}
		if (IsReadOnly)
		{
			throw new Exception("The specified file is read only.");
		}
		throw new Exception("Data is invalid");
	}

	private bool ValidateLookupMetaType(ref object value)
	{
		if (!IsReadOnly)
		{
			if (value == null || value.GetType() == typeof(bool) || value.GetType() == typeof(DateTime))
			{
				if (value != null)
				{
					value = Convert.ToString(-1, CultureInfo.InvariantCulture);
				}
			}
			else if (value != null && value.GetType() == typeof(char))
			{
				value = ((int)(char)value).ToString(CultureInfo.InvariantCulture);
			}
			else if (value == "")
			{
				value = null;
			}
			else if (value.GetType() != typeof(double))
			{
				value = Convert.ToString(value, CultureInfo.InvariantCulture);
			}
			else if (value.GetType() == typeof(double))
			{
				value = ((int)Math.Round((double)value)).ToString(CultureInfo.InvariantCulture);
			}
			return true;
		}
		throw new Exception("The specified file is read only.");
	}

	private bool ValidateUserMetaType(ref object value)
	{
		if (value != null && value.GetType() == typeof(string[]) && ((string[])value).Length <= 3 && !IsReadOnly)
		{
			if (((string[])value).Length == 0)
			{
				value = null;
			}
			else if (((string[])value).Length == 1)
			{
				value = new string[2]
				{
					((string[])value)[0],
					""
				};
			}
			return true;
		}
		if (IsReadOnly)
		{
			throw new Exception("The specified file is read only.");
		}
		if (value != null && value != "")
		{
			throw new Exception("Type mismatch");
		}
		return false;
	}

	private bool ValidateUrlMetaType(ref object value)
	{
		if (value != null && value.GetType() == typeof(string) && !IsReadOnly)
		{
			string text = value as string;
			if (text != null)
			{
				if (!text.Contains("http://") && !text.Contains("https://"))
				{
					text = Uri.EscapeUriString(text);
				}
				if (text.Contains("http://") || text.Contains("https://"))
				{
					value = new string[2] { text, text };
					return true;
				}
				value = new string[2]
				{
					"http://" + text,
					"http://" + text
				};
				return true;
			}
		}
		else
		{
			if (value != null && value.GetType() == typeof(string[]) && !IsReadOnly && ((string[])value).Length == 2)
			{
				string[] array = (string[])value;
				if (array[0] != null && !array[0].Contains("http://") && !array[0].Contains("https://"))
				{
					array[0] = Uri.EscapeUriString(array[0].ToString()) ?? Uri.EscapeDataString(array[0].ToString());
					value = new string[2]
					{
						"http://" + array[0],
						array[1]
					};
					return true;
				}
				value = new string[2]
				{
					array[0],
					array[1]
				};
				return true;
			}
			if (IsReadOnly)
			{
				throw new Exception("The specified file is read only.");
			}
			if (value != null)
			{
				throw new Exception("Type mismatch");
			}
		}
		return false;
	}

	private bool ValidateDateTimeMetaType(ref object value)
	{
		if ((value == null || value.GetType() == typeof(DateTime) || value == "") && !IsReadOnly)
		{
			if (value == "")
			{
				value = null;
			}
			return true;
		}
		if (IsReadOnly)
		{
			throw new Exception("The specified file is read only.");
		}
		throw new Exception("Data is invalid");
	}

	private bool ValidateNumberAndCurrencyMetaType(ref object value)
	{
		if (!IsReadOnly)
		{
			if (value != null && value.GetType() == typeof(DateTime))
			{
				value = ((DateTime)value).ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
			}
			else if (value != null && value.GetType() == typeof(bool))
			{
				value = value.ToString().ToLower();
			}
			else if (value != null && value.GetType() == typeof(char))
			{
				value = (int)(char)value;
			}
			else if (value == "")
			{
				value = null;
			}
			return true;
		}
		if (IsReadOnly)
		{
			throw new Exception("The specified file is read only.");
		}
		return false;
	}

	private bool ValidateTextNoteChoiceUnknownMetaType(ref object value)
	{
		if (!IsReadOnly)
		{
			if (value != null && value.GetType() == typeof(DateTime))
			{
				value = ((DateTime)value).ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
			}
			else if (value != null && value.GetType() == typeof(bool))
			{
				value = value.ToString().ToLower();
			}
			else if (value != null && value.GetType() == typeof(char))
			{
				value = ((int)(char)value).ToString(CultureInfo.InvariantCulture);
			}
			else if (value != null && value.GetType() != typeof(string))
			{
				value = Convert.ToString(value, CultureInfo.InvariantCulture);
			}
			else if (value == "")
			{
				value = null;
			}
			return true;
		}
		throw new Exception("The specified file is read only.");
	}
}
