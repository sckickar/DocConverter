using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using DocGen.DocIO.DLS.Entities;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS.XML;

public class XDLSReader : IXDLSAttributeReader, IXDLSContentReader
{
	private Dictionary<Type, object> s_enumHashEntryDict = new Dictionary<Type, object>();

	private XmlReader m_reader;

	private XDLSCustomRW m_customRW = new XDLSCustomRW();

	public string TagName => m_reader.LocalName;

	public XmlNodeType NodeType => m_reader.NodeType;

	public XmlReader InnerReader => m_reader;

	public IXDLSAttributeReader AttributeReader => this;

	public XDLSReader(XmlReader reader)
	{
		m_reader = reader;
	}

	public void Deserialize(IXDLSSerializable value)
	{
		while (m_reader.NodeType != XmlNodeType.Element)
		{
			m_reader.Read();
		}
		ReadElement(value);
		value.XDLSHolder.AfterDeserialization(value);
	}

	public bool HasAttribute(string name)
	{
		return m_reader.GetAttribute(name) != null;
	}

	public string ReadString(string name)
	{
		return m_reader.GetAttribute(name);
	}

	public int ReadInt(string name)
	{
		if (m_reader != null && !string.IsNullOrEmpty(name))
		{
			string attribute = m_reader.GetAttribute(name);
			if (string.IsNullOrEmpty(attribute))
			{
				return 0;
			}
			return XmlConvert.ToInt32(attribute);
		}
		return 0;
	}

	public short ReadShort(string name)
	{
		if (m_reader != null && !string.IsNullOrEmpty(name))
		{
			string attribute = m_reader.GetAttribute(name);
			if (string.IsNullOrEmpty(attribute))
			{
				return 0;
			}
			return XmlConvert.ToInt16(attribute);
		}
		return 0;
	}

	public double ReadDouble(string name)
	{
		if (m_reader != null && !string.IsNullOrEmpty(name))
		{
			string attribute = m_reader.GetAttribute(name);
			if (string.IsNullOrEmpty(attribute))
			{
				return 0.0;
			}
			return XmlConvert.ToDouble(attribute);
		}
		return 0.0;
	}

	public float ReadFloat(string name)
	{
		if (m_reader != null && !string.IsNullOrEmpty(name))
		{
			string attribute = m_reader.GetAttribute(name);
			if (string.IsNullOrEmpty(attribute))
			{
				return 0f;
			}
			return XmlConvert.ToSingle(attribute);
		}
		return 0f;
	}

	public bool ReadBoolean(string name)
	{
		string attribute = m_reader.GetAttribute(name);
		if (attribute != null)
		{
			return XmlConvert.ToBoolean(attribute);
		}
		return false;
	}

	public byte ReadByte(string name)
	{
		string attribute = m_reader.GetAttribute(name);
		if (attribute == null)
		{
			return 0;
		}
		return XmlConvert.ToByte(attribute);
	}

	public Enum ReadEnum(string name, Type enumType)
	{
		string attribute = m_reader.GetAttribute(name);
		if (!(enumType != null) || string.IsNullOrEmpty(attribute))
		{
			return null;
		}
		return (Enum)Enum.Parse(enumType, attribute, ignoreCase: true);
	}

	public Color ReadColor(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name - string can not be empty");
		}
		string attribute = m_reader.GetAttribute(name);
		return GetHexColor(attribute);
	}

	private Color GetHexColor(string color)
	{
		color = color.Replace("#", string.Empty);
		try
		{
			string s = color.Substring(0, 2);
			string s2 = color.Substring(2, 2);
			string s3 = color.Substring(4, 2);
			string s4 = color.Substring(6, 2);
			int alpha = int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			int red = int.Parse(s2, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			int green = int.Parse(s3, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			int blue = int.Parse(s4, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			return Color.FromArgb(alpha, red, green, blue);
		}
		catch (Exception)
		{
		}
		return Color.Empty;
	}

	public string GetAttributeValue(string name)
	{
		return m_reader.GetAttribute(name);
	}

	public bool ReadChildElement(object value)
	{
		if (value is IXDLSSerializable value2)
		{
			ReadElement(value2);
		}
		else
		{
			if (!(value is IXDLSSerializableCollection coll))
			{
				return false;
			}
			ReadElementCollection(coll);
		}
		return true;
	}

	public object ReadChildElement(Type type)
	{
		return m_customRW.Read(m_reader, type);
	}

	public string ReadChildStringContent()
	{
		return m_reader.ReadElementContentAsString();
	}

	public byte[] ReadChildBinaryElement()
	{
		XmlReader reader = m_reader;
		int num = 0;
		byte[] array = new byte[0];
		byte[] array2 = new byte[1000];
		do
		{
			num = reader.ReadElementContentAsBase64(array2, 0, array2.Length);
			byte[] array3 = new byte[array.Length + num];
			array.CopyTo(array3, 0);
			Array.Copy(array2, 0, array3, array.Length, num);
			array = array3;
			if (num < array2.Length)
			{
				break;
			}
			array2 = new byte[array.Length * 2];
		}
		while (!reader.EOF);
		return array;
	}

	internal DocGen.DocIO.DLS.Entities.Image ReadImage()
	{
		return ReadImage(isMetafile: false);
	}

	internal DocGen.DocIO.DLS.Entities.Image ReadImage(bool isMetafile)
	{
		return null;
	}

	private void ReadElement(IXDLSSerializable value)
	{
		if (value == null)
		{
			m_reader.Skip();
			return;
		}
		if (m_reader != null && m_reader.HasAttributes)
		{
			if (value.XDLSHolder != null && m_reader.MoveToAttribute("id"))
			{
				string attribute = m_reader.GetAttribute("id");
				if (attribute != null)
				{
					value.XDLSHolder.ID = XmlConvert.ToInt32(attribute);
				}
			}
			value.ReadXmlAttributes(this);
			m_reader.MoveToElement();
		}
		bool flag = false;
		string localName = m_reader.LocalName;
		bool flag2 = true;
		if (!m_reader.IsEmptyElement)
		{
			string localName2 = m_reader.LocalName;
			m_reader.Read();
			flag2 = false;
			if (!(localName2 == m_reader.LocalName) || m_reader.NodeType != XmlNodeType.EndElement)
			{
				flag = true;
			}
			else
			{
				m_reader.ReadStartElement();
			}
		}
		if (flag2)
		{
			m_reader.ReadStartElement();
		}
		int num = 0;
		if (!flag)
		{
			return;
		}
		while ((m_reader.NodeType != XmlNodeType.EndElement || !(m_reader.LocalName == localName) || num != 0) && !m_reader.EOF)
		{
			if (m_reader.NodeType != XmlNodeType.Whitespace && m_reader.IsStartElement() && m_reader.LocalName == localName)
			{
				num++;
			}
			else if (m_reader.NodeType == XmlNodeType.EndElement && m_reader.LocalName == localName)
			{
				num--;
			}
			if (m_reader.NodeType != XmlNodeType.Element)
			{
				m_reader.Read();
			}
			else if (!value.ReadXmlContent(this))
			{
				m_reader.Skip();
			}
		}
		if (m_reader.NodeType == XmlNodeType.EndElement)
		{
			m_reader.ReadEndElement();
		}
	}

	private void ReadElementCollection(IXDLSSerializableCollection coll)
	{
		bool flag = false;
		string localName = m_reader.LocalName;
		bool flag2 = true;
		if (!m_reader.IsEmptyElement)
		{
			string localName2 = m_reader.LocalName;
			m_reader.Read();
			flag2 = false;
			if (!(localName2 == m_reader.LocalName) || m_reader.NodeType != XmlNodeType.EndElement)
			{
				flag = true;
			}
			else
			{
				m_reader.ReadStartElement();
			}
		}
		if (flag2)
		{
			m_reader.ReadStartElement();
		}
		int num = 0;
		if (!flag)
		{
			return;
		}
		while ((m_reader.NodeType != XmlNodeType.EndElement || !(m_reader.LocalName == localName) || num != 0) && !m_reader.EOF)
		{
			if (m_reader.NodeType != XmlNodeType.Whitespace && m_reader.IsStartElement() && m_reader.LocalName == localName)
			{
				num++;
			}
			else if (m_reader.NodeType == XmlNodeType.EndElement && m_reader.LocalName == localName)
			{
				num--;
			}
			if (m_reader.NodeType != XmlNodeType.Element)
			{
				m_reader.Read();
			}
			else if (m_reader.LocalName == coll.TagItemName)
			{
				IXDLSSerializable value = coll.AddNewItem(this);
				ReadElement(value);
			}
		}
		if (m_reader.NodeType == XmlNodeType.EndElement)
		{
			m_reader.ReadEndElement();
		}
	}
}
