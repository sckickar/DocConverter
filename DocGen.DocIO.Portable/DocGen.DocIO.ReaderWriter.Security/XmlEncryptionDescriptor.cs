using System;
using System.IO;
using System.Xml;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class XmlEncryptionDescriptor
{
	private const string XMLNameSpace = "xmlns";

	private KeyData m_keyData = new KeyData();

	private DataIntegrity m_dataIntegrity = new DataIntegrity();

	private KeyEncryptors m_keyEncryptors = new KeyEncryptors();

	internal KeyData KeyData
	{
		get
		{
			return m_keyData;
		}
		set
		{
			m_keyData = value;
		}
	}

	internal DataIntegrity DataIntegrity
	{
		get
		{
			return m_dataIntegrity;
		}
		set
		{
			m_dataIntegrity = value;
		}
	}

	internal KeyEncryptors KeyEncryptors
	{
		get
		{
			return m_keyEncryptors;
		}
		set
		{
			m_keyEncryptors = value;
		}
	}

	public void Parse(Stream stream)
	{
		XmlReader xmlReader = CreateReader(stream);
		xmlReader.Read();
		while (xmlReader.LocalName != "encryption")
		{
			if (xmlReader.NodeType == XmlNodeType.Element)
			{
				switch (xmlReader.LocalName)
				{
				case "keyData":
					m_keyData.Parse(xmlReader);
					break;
				case "dataIntegrity":
					m_dataIntegrity.Parse(xmlReader);
					break;
				case "keyEncryptors":
					m_keyEncryptors.Parse(xmlReader);
					break;
				}
				xmlReader.Read();
			}
			else
			{
				xmlReader.Read();
			}
		}
	}

	public void Serialize(Stream stream)
	{
		XmlWriter xmlWriter = CreateWriter(stream);
		xmlWriter.WriteStartElement("encryption", "http://schemas.microsoft.com/office/2006/encryption");
		xmlWriter.WriteAttributeString("xmlns", "http://schemas.microsoft.com/office/2006/encryption");
		xmlWriter.WriteAttributeString("xmlns", "p", null, "http://schemas.microsoft.com/office/2006/keyEncryptor/password");
		if (m_keyData.KeyBits == 256)
		{
			xmlWriter.WriteAttributeString("xmlns", "c", null, "http://schemas.microsoft.com/office/2006/keyEncryptor/certificate");
		}
		m_keyData.Serialize(xmlWriter);
		m_dataIntegrity.Serialize(xmlWriter);
		m_keyEncryptors.Serialize(xmlWriter);
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
	}

	private XmlWriter CreateWriter(Stream data)
	{
		XmlWriterSettings settings = new XmlWriterSettings();
		XmlWriter xmlWriter = XmlWriter.Create(data, settings);
		xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
		return xmlWriter;
	}

	public XmlReader CreateReader(Stream data)
	{
		XmlReader xmlReader = XmlReader.Create(data);
		while (xmlReader.NodeType != XmlNodeType.Element)
		{
			xmlReader.Read();
		}
		return xmlReader;
	}
}
