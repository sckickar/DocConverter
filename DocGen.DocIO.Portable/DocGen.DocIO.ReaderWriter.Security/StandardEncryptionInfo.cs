using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class StandardEncryptionInfo
{
	private int m_iVersionInfo;

	private int m_iFlags;

	private EncryptionHeader m_header = new EncryptionHeader();

	private EncryptionVerifier m_verifier = new EncryptionVerifier();

	private SecurityHelper m_securityHelper = new SecurityHelper();

	internal int VersionInfo
	{
		get
		{
			return m_iVersionInfo;
		}
		set
		{
			m_iVersionInfo = value;
		}
	}

	internal int Flags
	{
		get
		{
			return m_iFlags;
		}
		set
		{
			m_iFlags = value;
		}
	}

	internal EncryptionHeader Header => m_header;

	internal EncryptionVerifier Verifier => m_verifier;

	internal StandardEncryptionInfo()
	{
	}

	internal StandardEncryptionInfo(Stream stream)
	{
		byte[] buffer = new byte[4];
		m_iVersionInfo = m_securityHelper.ReadInt32(stream, buffer);
		m_iFlags = m_securityHelper.ReadInt32(stream, buffer);
		m_header.Parse(stream);
		m_verifier.Parse(stream);
	}

	internal void Serialize(Stream stream)
	{
		m_securityHelper.WriteInt32(stream, m_iVersionInfo);
		m_securityHelper.WriteInt32(stream, m_iFlags);
		m_header.Serialize(stream);
		m_verifier.Serialize(stream);
	}
}
