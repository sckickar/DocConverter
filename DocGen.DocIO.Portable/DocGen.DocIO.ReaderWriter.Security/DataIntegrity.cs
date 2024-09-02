using System;
using System.Xml;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class DataIntegrity
{
	private byte[] m_encryptedHmacKey;

	private byte[] m_encryptedHmacValue;

	internal byte[] EncryptedHmacKey
	{
		get
		{
			return m_encryptedHmacKey;
		}
		set
		{
			m_encryptedHmacKey = value;
		}
	}

	internal byte[] EncryptedHmacValue
	{
		get
		{
			return m_encryptedHmacValue;
		}
		set
		{
			m_encryptedHmacValue = value;
		}
	}

	internal DataIntegrity()
	{
	}

	internal void Parse(XmlReader reader)
	{
		m_encryptedHmacKey = KeyData.GetAttributeValueAsByte(reader, "encryptedHmacKey");
		m_encryptedHmacValue = KeyData.GetAttributeValueAsByte(reader, "encryptedHmacValue");
	}

	internal void Serialize(XmlWriter writer)
	{
		writer.WriteStartElement("dataIntegrity");
		writer.WriteAttributeString("encryptedHmacKey", Convert.ToBase64String(m_encryptedHmacKey));
		writer.WriteAttributeString("encryptedHmacValue", Convert.ToBase64String(m_encryptedHmacValue));
		writer.WriteEndElement();
	}
}
