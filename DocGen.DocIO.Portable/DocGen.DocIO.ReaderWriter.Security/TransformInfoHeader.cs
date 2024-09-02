using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class TransformInfoHeader
{
	private int m_iTransformType = 1;

	private string m_strTransformId;

	private string m_strTransformName;

	private int m_iReaderVersion = 1;

	private int m_iUpdaterVersion = 1;

	private int m_iWriterVersion = 1;

	private SecurityHelper m_securityHelper = new SecurityHelper();

	internal int TransformType
	{
		get
		{
			return m_iTransformType;
		}
		set
		{
			m_iTransformType = value;
		}
	}

	internal string TransformId
	{
		get
		{
			return m_strTransformId;
		}
		set
		{
			m_strTransformId = value;
		}
	}

	internal string TransformName
	{
		get
		{
			return m_strTransformName;
		}
		set
		{
			m_strTransformName = value;
		}
	}

	internal int ReaderVersion
	{
		get
		{
			return m_iReaderVersion;
		}
		set
		{
			m_iReaderVersion = value;
		}
	}

	internal int UpdaterVersion
	{
		get
		{
			return m_iUpdaterVersion;
		}
		set
		{
			m_iUpdaterVersion = value;
		}
	}

	internal int WriterVersion
	{
		get
		{
			return m_iWriterVersion;
		}
		set
		{
			m_iWriterVersion = value;
		}
	}

	internal TransformInfoHeader()
	{
	}

	internal TransformInfoHeader(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] buffer = new byte[4];
		m_securityHelper.ReadInt32(stream, buffer);
		m_iTransformType = m_securityHelper.ReadInt32(stream, buffer);
		m_strTransformId = m_securityHelper.ReadUnicodeString(stream);
		m_strTransformName = m_securityHelper.ReadUnicodeString(stream);
		m_iReaderVersion = m_securityHelper.ReadInt32(stream, buffer);
		m_iUpdaterVersion = m_securityHelper.ReadInt32(stream, buffer);
		m_iWriterVersion = m_securityHelper.ReadInt32(stream, buffer);
	}

	internal void Serialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long position = stream.Position;
		stream.Position += 4L;
		m_securityHelper.WriteInt32(stream, m_iTransformType);
		m_securityHelper.WriteUnicodeString(stream, m_strTransformId);
		long position2 = stream.Position;
		int value = (int)(position2 - position);
		stream.Position = position;
		m_securityHelper.WriteInt32(stream, value);
		stream.Position = position2;
		m_securityHelper.WriteUnicodeString(stream, m_strTransformName);
		m_securityHelper.WriteInt32(stream, m_iReaderVersion);
		m_securityHelper.WriteInt32(stream, m_iUpdaterVersion);
		m_securityHelper.WriteInt32(stream, m_iWriterVersion);
	}
}
