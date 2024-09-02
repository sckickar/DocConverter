using System;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.DocIO.DLS.Entities;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS.XML;

public class XDLSWriter : IXDLSAttributeWriter, IXDLSContentWriter
{
	private const string DEF_SHARP = "#";

	private const string DEF_HEX_FORMAT = "X2";

	private readonly XmlWriter m_writer;

	private string m_rootTagName = "DLS";

	private XDLSCustomRW m_customRW = new XDLSCustomRW();

	private Metafile m_srcMetafile;

	public XmlWriter InnerWriter => m_writer;

	public XDLSWriter(XmlWriter writer)
	{
		m_writer = writer;
	}

	public void Serialize(IXDLSSerializable value)
	{
		value.XDLSHolder.BeforeSerialization();
		WriteElement(m_rootTagName, value, isWriteID: false);
	}

	private void WriteElement(string tagName, IXDLSSerializable value, bool isWriteID)
	{
		if (!value.XDLSHolder.SkipMe)
		{
			m_writer.WriteStartElement(tagName);
			if (isWriteID && value.XDLSHolder.EnableID)
			{
				WriteValue("id", value.XDLSHolder.ID);
			}
			value.WriteXmlAttributes(this);
			value.WriteXmlContent(this);
			m_writer.WriteEndElement();
		}
	}

	private void WriteCollectionElement(string tagName, IXDLSSerializableCollection value)
	{
		if (value.Count <= 0)
		{
			return;
		}
		m_writer.WriteStartElement(tagName);
		foreach (IXDLSSerializable item in value)
		{
			if (item != null)
			{
				WriteElement(value.TagItemName, item, isWriteID: true);
			}
		}
		m_writer.WriteEndElement();
	}

	protected virtual void WriteCustomElement(string tagName, object value)
	{
		if (!m_customRW.Write(m_writer, tagName, value))
		{
			WriteDefElement(tagName, value);
		}
	}

	private void WriteDefElement(string tagName, object value)
	{
	}

	public void WriteValue(string name, float value)
	{
		m_writer.WriteAttributeString(name, XmlConvert.ToString(value));
	}

	public void WriteValue(string name, double value)
	{
		m_writer.WriteAttributeString(name, XmlConvert.ToString(value));
	}

	public void WriteValue(string name, int value)
	{
		m_writer.WriteAttributeString(name, XmlConvert.ToString(value));
	}

	public void WriteValue(string name, string value)
	{
		m_writer.WriteAttributeString(name, value);
	}

	public void WriteValue(string name, Enum value)
	{
		m_writer.WriteAttributeString(name, value.ToString());
	}

	public void WriteValue(string name, bool value)
	{
		m_writer.WriteAttributeString(name, XmlConvert.ToString(value));
	}

	public void WriteValue(string name, Color value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name - string can not be empty");
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (!value.IsEmpty)
		{
			stringBuilder.Append("#");
			stringBuilder.Append(value.A.ToString("X2"));
			stringBuilder.Append(value.R.ToString("X2"));
			stringBuilder.Append(value.G.ToString("X2"));
			stringBuilder.Append(value.B.ToString("X2"));
		}
		m_writer.WriteAttributeString(name, stringBuilder.ToString());
	}

	public void WriteValue(string name, DateTime value)
	{
		string value2 = XmlConvert.ToString(value, "yyyy-MM-ddTHH:mm:ssZ");
		m_writer.WriteAttributeString(name, value2);
	}

	public void WriteChildStringElement(string name, string value)
	{
		m_writer.WriteStartElement(name);
		m_writer.WriteString(value);
		m_writer.WriteEndElement();
	}

	public void WriteChildBinaryElement(string name, byte[] value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name - string can not be empty");
		}
		InnerWriter.WriteStartElement(name);
		InnerWriter.WriteBase64(value, 0, value.Length);
		InnerWriter.WriteEndElement();
	}

	public void WriteChildElement(string name, object value)
	{
		if (value is IXDLSSerializable value2)
		{
			WriteElement(name, value2, isWriteID: false);
		}
		else if (value is IXDLSSerializableCollection value3)
		{
			WriteCollectionElement(name, value3);
		}
		else if (value is string)
		{
			m_writer.WriteStartElement(name);
			WriteValue("type", "String");
			WriteValue("value", (string)value);
			m_writer.WriteEndElement();
		}
		else if (value is int)
		{
			m_writer.WriteStartElement(name);
			WriteValue("type", "Int32");
			WriteValue("value", (int)value);
			m_writer.WriteEndElement();
		}
		else if (value is float)
		{
			m_writer.WriteStartElement(name);
			WriteValue("type", "Single");
			WriteValue("value", (float)value);
			m_writer.WriteEndElement();
		}
		else if (value is bool)
		{
			m_writer.WriteStartElement(name);
			WriteValue("type", "Boolean");
			WriteValue("value", value.ToString());
			m_writer.WriteEndElement();
		}
		else if (value is Enum)
		{
			m_writer.WriteStartElement(name);
			WriteValue("type", value.GetType().ToString());
			WriteValue("value", value.ToString());
			m_writer.WriteEndElement();
		}
		else
		{
			WriteCustomElement(name, value);
		}
	}

	public void WriteChildRefElement(string name, int refToElement)
	{
		m_writer.WriteStartElement(name);
		WriteValue("ref", refToElement);
		m_writer.WriteEndElement();
	}

	internal void WriteImage(DocGen.DocIO.DLS.Entities.Image image)
	{
		if (image != null)
		{
			MemoryStream memoryStream = CreateStreamFromImage(image);
			byte[] array = new byte[memoryStream.Length];
			memoryStream.Position = 0L;
			memoryStream.Read(array, 0, array.Length);
			WriteChildBinaryElement("image", array);
		}
	}

	private MemoryStream CreateStreamFromImage(DocGen.DocIO.DLS.Entities.Image image)
	{
		return null;
	}
}
