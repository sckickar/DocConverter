using System;
using System.Xml;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class KeyData
{
	private int m_iSaltSize;

	private int m_iBlockSize;

	private int m_iKeyBits;

	private int m_iHashSize;

	private string m_sCipherAlgorithm;

	private string m_sCipherChaining;

	private string m_sHashAlgorithm;

	private byte[] m_arrSalt;

	internal int SaltSize
	{
		get
		{
			return m_iSaltSize;
		}
		set
		{
			m_iSaltSize = value;
		}
	}

	internal int BlockSize
	{
		get
		{
			return m_iBlockSize;
		}
		set
		{
			m_iBlockSize = value;
		}
	}

	internal int KeyBits
	{
		get
		{
			return m_iKeyBits;
		}
		set
		{
			m_iKeyBits = value;
		}
	}

	internal int HashSize
	{
		get
		{
			return m_iHashSize;
		}
		set
		{
			m_iHashSize = value;
		}
	}

	internal string CipherAlgorithm
	{
		get
		{
			return m_sCipherAlgorithm;
		}
		set
		{
			m_sCipherAlgorithm = value;
		}
	}

	internal string CipherChaining
	{
		get
		{
			return m_sCipherChaining;
		}
		set
		{
			m_sCipherChaining = value;
		}
	}

	internal string HashAlgorithm
	{
		get
		{
			return m_sHashAlgorithm;
		}
		set
		{
			m_sHashAlgorithm = value;
		}
	}

	internal byte[] Salt
	{
		get
		{
			return m_arrSalt;
		}
		set
		{
			m_arrSalt = value;
		}
	}

	internal KeyData()
	{
	}

	internal void Parse(XmlReader reader)
	{
		m_iSaltSize = GetAttributeValueAsInt(reader, "saltSize");
		m_iBlockSize = GetAttributeValueAsInt(reader, "blockSize");
		m_iKeyBits = GetAttributeValueAsInt(reader, "keyBits");
		m_iHashSize = GetAttributeValueAsInt(reader, "hashSize");
		m_sCipherAlgorithm = reader.GetAttribute("cipherAlgorithm");
		m_sCipherChaining = reader.GetAttribute("cipherChaining");
		m_sHashAlgorithm = reader.GetAttribute("hashAlgorithm");
		m_arrSalt = GetAttributeValueAsByte(reader, "saltValue");
	}

	internal static int GetAttributeValueAsInt(XmlReader reader, string attributeName)
	{
		if (reader != null && !string.IsNullOrEmpty(attributeName))
		{
			string attribute = reader.GetAttribute(attributeName);
			if (attribute != null)
			{
				return int.Parse(attribute);
			}
		}
		return -1;
	}

	internal static byte[] GetAttributeValueAsByte(XmlReader reader, string attributeName)
	{
		byte[] result = null;
		if (reader != null && !string.IsNullOrEmpty(attributeName))
		{
			string attribute = reader.GetAttribute(attributeName);
			if (attribute != null)
			{
				result = Convert.FromBase64String(attribute);
			}
		}
		return result;
	}

	internal void Serialize(XmlWriter writer)
	{
		writer.WriteStartElement("keyData");
		writer.WriteAttributeString("saltSize", m_iSaltSize.ToString());
		writer.WriteAttributeString("blockSize", m_iBlockSize.ToString());
		writer.WriteAttributeString("keyBits", m_iKeyBits.ToString());
		writer.WriteAttributeString("hashSize", m_iHashSize.ToString());
		writer.WriteAttributeString("cipherAlgorithm", m_sCipherAlgorithm);
		writer.WriteAttributeString("cipherChaining", m_sCipherChaining);
		writer.WriteAttributeString("hashAlgorithm", m_sHashAlgorithm);
		writer.WriteAttributeString("saltValue", Convert.ToBase64String(m_arrSalt));
		writer.WriteEndElement();
	}
}
