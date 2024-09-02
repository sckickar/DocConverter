using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class AgileEncryptionInfo
{
	private int m_iVersionInfo;

	private int m_iReserved;

	private XmlEncryptionDescriptor m_xmlEncryptionDescriptor = new XmlEncryptionDescriptor();

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

	internal int Reserved
	{
		get
		{
			return m_iReserved;
		}
		set
		{
			m_iReserved = value;
		}
	}

	internal XmlEncryptionDescriptor XmlEncryptionDescriptor => m_xmlEncryptionDescriptor;

	internal AgileEncryptionInfo()
	{
	}

	internal AgileEncryptionInfo(Stream stream)
	{
		byte[] buffer = new byte[4];
		m_iVersionInfo = m_securityHelper.ReadInt32(stream, buffer);
		m_iReserved = m_securityHelper.ReadInt32(stream, buffer);
		m_xmlEncryptionDescriptor.Parse(stream);
	}

	internal void Serialize(Stream stream)
	{
		m_securityHelper.WriteInt32(stream, m_iVersionInfo);
		m_securityHelper.WriteInt32(stream, m_iReserved);
		m_xmlEncryptionDescriptor.Serialize(stream);
	}
}
