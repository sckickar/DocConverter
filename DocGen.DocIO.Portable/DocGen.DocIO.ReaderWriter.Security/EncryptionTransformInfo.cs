using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class EncryptionTransformInfo
{
	private string m_strName;

	private int m_iBlockSize;

	private int m_iCipherMode;

	private int m_iReserved = 4;

	private SecurityHelper m_securityHelper = new SecurityHelper();

	internal string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
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

	internal int CipherMode => m_iCipherMode;

	internal int Reserved => m_iReserved;

	internal EncryptionTransformInfo()
	{
	}

	internal EncryptionTransformInfo(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] buffer = new byte[4];
		m_strName = m_securityHelper.ReadUnicodeString(stream);
		m_iBlockSize = m_securityHelper.ReadInt32(stream, buffer);
		m_iCipherMode = m_securityHelper.ReadInt32(stream, buffer);
		m_iReserved = m_securityHelper.ReadInt32(stream, buffer);
	}

	internal void Serialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_securityHelper.WriteUnicodeString(stream, m_strName);
		m_securityHelper.WriteInt32(stream, m_iBlockSize);
		m_securityHelper.WriteInt32(stream, m_iCipherMode);
		m_securityHelper.WriteInt32(stream, m_iReserved);
	}
}
