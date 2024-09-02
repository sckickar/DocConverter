using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class EncryptionVerifier
{
	private byte[] m_arrSalt;

	private byte[] m_arrEncryptedVerifier = new byte[16];

	private byte[] m_arrEncryptedVerifierHash;

	private int m_iVerifierHashSize;

	private SecurityHelper m_securityHelper = new SecurityHelper();

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

	internal byte[] EncryptedVerifier
	{
		get
		{
			return m_arrEncryptedVerifier;
		}
		set
		{
			m_arrEncryptedVerifier = value;
		}
	}

	internal byte[] EncryptedVerifierHash
	{
		get
		{
			return m_arrEncryptedVerifierHash;
		}
		set
		{
			m_arrEncryptedVerifierHash = value;
		}
	}

	internal int VerifierHashSize
	{
		get
		{
			return m_iVerifierHashSize;
		}
		set
		{
			m_iVerifierHashSize = value;
		}
	}

	internal EncryptionVerifier()
	{
	}

	internal EncryptionVerifier(Stream stream)
	{
		Parse(stream);
	}

	internal void Parse(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] buffer = new byte[4];
		int num = m_securityHelper.ReadInt32(stream, buffer);
		m_arrSalt = new byte[num];
		stream.Read(m_arrSalt, 0, num);
		stream.Read(m_arrEncryptedVerifier, 0, m_arrEncryptedVerifier.Length);
		m_iVerifierHashSize = m_securityHelper.ReadInt32(stream, buffer);
		int num2 = (int)(stream.Length - stream.Position);
		m_arrEncryptedVerifierHash = new byte[num2];
		stream.Read(m_arrEncryptedVerifierHash, 0, num2);
	}

	internal void Serialize(Stream stream)
	{
		int num = m_arrSalt.Length;
		m_securityHelper.WriteInt32(stream, num);
		stream.Write(m_arrSalt, 0, num);
		stream.Write(m_arrEncryptedVerifier, 0, m_arrEncryptedVerifier.Length);
		m_securityHelper.WriteInt32(stream, m_iVerifierHashSize);
		int count = m_arrEncryptedVerifierHash.Length;
		stream.Write(m_arrEncryptedVerifierHash, 0, count);
	}
}
