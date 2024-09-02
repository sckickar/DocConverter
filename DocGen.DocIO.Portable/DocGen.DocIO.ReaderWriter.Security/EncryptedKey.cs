using System;
using System.Xml;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class EncryptedKey
{
	private int m_iSpinCount;

	private int m_iSaltSize;

	private int m_iBlockSize;

	private int m_iKeyBits;

	private int m_iHashSize;

	private string m_sCipherAlgorithm;

	private string m_sCipherChaining;

	private string m_sHashAlgorithm;

	private byte[] m_arrSalt;

	private byte[] m_encryptedVerifierHashInput;

	private byte[] m_encryptedVerifierHashValue;

	private byte[] m_encryptedKeyValue;

	internal int SpinCount
	{
		get
		{
			return m_iSpinCount;
		}
		set
		{
			m_iSpinCount = value;
		}
	}

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

	internal byte[] EncryptedVerifierHashInput
	{
		get
		{
			return m_encryptedVerifierHashInput;
		}
		set
		{
			m_encryptedVerifierHashInput = value;
		}
	}

	internal byte[] EncryptedVerifierHashValue
	{
		get
		{
			return m_encryptedVerifierHashValue;
		}
		set
		{
			m_encryptedVerifierHashValue = value;
		}
	}

	internal byte[] EncryptedKeyValue
	{
		get
		{
			return m_encryptedKeyValue;
		}
		set
		{
			m_encryptedKeyValue = value;
		}
	}

	internal EncryptedKey()
	{
	}

	internal void Parse(XmlReader reader)
	{
		m_iSpinCount = KeyData.GetAttributeValueAsInt(reader, "spinCount");
		m_iSaltSize = KeyData.GetAttributeValueAsInt(reader, "saltSize");
		m_iBlockSize = KeyData.GetAttributeValueAsInt(reader, "blockSize");
		m_iKeyBits = KeyData.GetAttributeValueAsInt(reader, "keyBits");
		m_iHashSize = KeyData.GetAttributeValueAsInt(reader, "hashSize");
		m_sCipherAlgorithm = reader.GetAttribute("cipherAlgorithm");
		m_sCipherChaining = reader.GetAttribute("cipherChaining");
		m_sHashAlgorithm = reader.GetAttribute("hashAlgorithm");
		m_arrSalt = KeyData.GetAttributeValueAsByte(reader, "saltValue");
		m_encryptedVerifierHashInput = KeyData.GetAttributeValueAsByte(reader, "encryptedVerifierHashInput");
		m_encryptedVerifierHashValue = KeyData.GetAttributeValueAsByte(reader, "encryptedVerifierHashValue");
		m_encryptedKeyValue = KeyData.GetAttributeValueAsByte(reader, "encryptedKeyValue");
	}

	internal void Serialize(XmlWriter writer)
	{
		writer.WriteStartElement("p", "encryptedKey", "http://schemas.microsoft.com/office/2006/keyEncryptor/password");
		writer.WriteAttributeString("spinCount", m_iSpinCount.ToString());
		writer.WriteAttributeString("saltSize", m_iSaltSize.ToString());
		writer.WriteAttributeString("blockSize", m_iBlockSize.ToString());
		writer.WriteAttributeString("keyBits", m_iKeyBits.ToString());
		writer.WriteAttributeString("hashSize", m_iHashSize.ToString());
		writer.WriteAttributeString("cipherAlgorithm", m_sCipherAlgorithm);
		writer.WriteAttributeString("cipherChaining", m_sCipherChaining);
		writer.WriteAttributeString("hashAlgorithm", m_sHashAlgorithm);
		writer.WriteAttributeString("saltValue", Convert.ToBase64String(m_arrSalt));
		writer.WriteAttributeString("encryptedVerifierHashInput", Convert.ToBase64String(m_encryptedVerifierHashInput));
		writer.WriteAttributeString("encryptedVerifierHashValue", Convert.ToBase64String(m_encryptedVerifierHashValue));
		writer.WriteAttributeString("encryptedKeyValue", Convert.ToBase64String(m_encryptedKeyValue));
		writer.WriteEndElement();
	}
}
