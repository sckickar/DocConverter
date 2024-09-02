using System;
using System.Xml;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS.XML;

public class XDLSCustomRW
{
	private XmlReader m_reader;

	private XmlWriter m_writer;

	public bool Write(XmlWriter writer, string tagName, object value)
	{
		m_writer = writer;
		if (value is Color)
		{
			WriteColor(tagName, (Color)value);
		}
		else
		{
			if (!(value is Font))
			{
				return false;
			}
			WriteFont(tagName, (Font)value);
		}
		return true;
	}

	public object Read(XmlReader reader, Type type)
	{
		m_reader = reader;
		if (type.Equals(typeof(Color)))
		{
			return ReadColor();
		}
		if (type.Equals(typeof(Font)))
		{
			return ReadFont();
		}
		return null;
	}

	private void WriteColor(string name, Color color)
	{
		m_writer.WriteStartElement(name);
		m_writer.WriteAttributeString("type", "Color");
		m_writer.WriteAttributeString("argb", XmlConvert.ToString(color.ToArgb()));
		m_writer.WriteEndElement();
	}

	private void WriteFont(string name, Font font)
	{
		m_writer.WriteStartElement(name);
		m_writer.WriteAttributeString("type", "Font");
		m_writer.WriteAttributeString("fontName", font.Name);
		m_writer.WriteAttributeString("size", font.SizeInPoints.ToString());
		m_writer.WriteEndElement();
	}

	private Font ReadFont()
	{
		string attribute = m_reader.GetAttribute("fontName");
		string attribute2 = m_reader.GetAttribute("size");
		m_reader.GetAttribute("style");
		m_reader.Read();
		if (attribute == null || attribute2 == null)
		{
			return null;
		}
		return new Font(attribute, int.Parse(attribute2));
	}

	private Color ReadColor()
	{
		string attribute = m_reader.GetAttribute("argb");
		m_reader.Read();
		if (attribute == null)
		{
			return Color.Empty;
		}
		return Color.FromArgb(int.Parse(attribute));
	}
}
