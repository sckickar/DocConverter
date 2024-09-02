using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class VersionInfo
{
	private string m_strFeatureId = "Microsoft.Container.DataSpaces";

	private int m_iReaderVersion = 1;

	private int m_iUpdaterVersion = 1;

	private int m_iWriterVersion = 1;

	private SecurityHelper m_securityHelper = new SecurityHelper();

	internal string FeatureId
	{
		get
		{
			return m_strFeatureId;
		}
		set
		{
			m_strFeatureId = value;
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

	internal VersionInfo()
	{
	}

	internal void Serialize(Stream stream)
	{
		m_securityHelper.WriteUnicodeString(stream, m_strFeatureId);
		m_securityHelper.WriteInt32(stream, m_iReaderVersion);
		m_securityHelper.WriteInt32(stream, m_iUpdaterVersion);
		m_securityHelper.WriteInt32(stream, m_iWriterVersion);
	}
}
