using System;
using System.Xml;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class KeyEncryptors
{
	private EncryptedKey m_encryptedKey = new EncryptedKey();

	internal EncryptedKey EncryptedKey => m_encryptedKey;

	internal KeyEncryptors()
	{
	}

	internal void Parse(XmlReader reader)
	{
		reader.Read();
		while (reader.LocalName != "keyEncryptors")
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "encryptedKey")
				{
					m_encryptedKey.Parse(reader);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	internal void Serialize(XmlWriter writer)
	{
		writer.WriteStartElement("keyEncryptors");
		writer.WriteStartElement("keyEncryptor");
		writer.WriteAttributeString("uri", "http://schemas.microsoft.com/office/2006/keyEncryptor/password");
		m_encryptedKey.Serialize(writer);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}
}
