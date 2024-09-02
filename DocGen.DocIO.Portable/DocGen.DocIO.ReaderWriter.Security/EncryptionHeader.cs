using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class EncryptionHeader
{
	private int m_iFlags;

	private int m_iSizeExtra;

	private int m_iAlgorithmId;

	private int m_iAlgorithmIdHash;

	private int m_iKeySize;

	private int m_iProviderType;

	private int m_iReserved1;

	private int m_iReserved2;

	private string m_strCSPName;

	private SecurityHelper m_securityHelper = new SecurityHelper();

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

	internal int SizeExtra
	{
		get
		{
			return m_iSizeExtra;
		}
		set
		{
			m_iSizeExtra = value;
		}
	}

	internal int AlgorithmId
	{
		get
		{
			return m_iAlgorithmId;
		}
		set
		{
			m_iAlgorithmId = value;
		}
	}

	internal int AlgorithmIdHash
	{
		get
		{
			return m_iAlgorithmIdHash;
		}
		set
		{
			m_iAlgorithmIdHash = value;
		}
	}

	internal int KeySize
	{
		get
		{
			return m_iKeySize;
		}
		set
		{
			m_iKeySize = value;
		}
	}

	internal int ProviderType
	{
		get
		{
			return m_iProviderType;
		}
		set
		{
			m_iProviderType = value;
		}
	}

	internal int Reserved1
	{
		get
		{
			return m_iReserved1;
		}
		set
		{
			m_iReserved1 = value;
		}
	}

	internal int Reserved2
	{
		get
		{
			return m_iReserved2;
		}
		set
		{
			m_iReserved2 = value;
		}
	}

	internal string CSPName
	{
		get
		{
			return m_strCSPName;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_strCSPName = value;
		}
	}

	internal EncryptionHeader()
	{
	}

	internal EncryptionHeader(Stream stream)
	{
		Parse(stream);
	}

	internal void Parse(Stream stream)
	{
		byte[] buffer = new byte[4];
		long position = stream.Position;
		int num = m_securityHelper.ReadInt32(stream, buffer);
		m_iFlags = m_securityHelper.ReadInt32(stream, buffer);
		m_iSizeExtra = m_securityHelper.ReadInt32(stream, buffer);
		m_iAlgorithmId = m_securityHelper.ReadInt32(stream, buffer);
		m_iAlgorithmIdHash = m_securityHelper.ReadInt32(stream, buffer);
		m_iKeySize = m_securityHelper.ReadInt32(stream, buffer);
		m_iProviderType = m_securityHelper.ReadInt32(stream, buffer);
		m_iReserved1 = m_securityHelper.ReadInt32(stream, buffer);
		m_iReserved2 = m_securityHelper.ReadInt32(stream, buffer);
		m_strCSPName = m_securityHelper.ReadUnicodeStringZero(stream);
		stream.Position = position + num + 4;
	}

	internal void Serialize(Stream stream)
	{
		long position = stream.Position;
		stream.Position += 4L;
		m_securityHelper.WriteInt32(stream, m_iFlags);
		m_securityHelper.WriteInt32(stream, m_iSizeExtra);
		m_securityHelper.WriteInt32(stream, m_iAlgorithmId);
		m_securityHelper.WriteInt32(stream, m_iAlgorithmIdHash);
		m_securityHelper.WriteInt32(stream, m_iKeySize);
		m_securityHelper.WriteInt32(stream, m_iProviderType);
		m_securityHelper.WriteInt32(stream, m_iReserved1);
		m_securityHelper.WriteInt32(stream, m_iReserved2);
		m_securityHelper.WriteUnicodeStringZero(stream, m_strCSPName);
		long position2 = stream.Position;
		int value = (int)(position2 - position) - 4;
		stream.Position = position;
		m_securityHelper.WriteInt32(stream, value);
		stream.Position = position2;
	}
}
