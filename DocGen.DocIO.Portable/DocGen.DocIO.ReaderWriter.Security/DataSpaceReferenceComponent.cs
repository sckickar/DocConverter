using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class DataSpaceReferenceComponent
{
	private int m_iComponentType;

	private string m_strName;

	private SecurityHelper m_securityHelper = new SecurityHelper();

	internal int ComponentType => m_iComponentType;

	internal string Name => m_strName;

	internal DataSpaceReferenceComponent(int type, string name)
	{
		m_iComponentType = type;
		m_strName = name;
	}

	internal DataSpaceReferenceComponent(Stream stream)
	{
		byte[] buffer = new byte[4];
		m_iComponentType = m_securityHelper.ReadInt32(stream, buffer);
		m_strName = m_securityHelper.ReadUnicodeString(stream);
	}

	internal void Serialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_securityHelper.WriteInt32(stream, m_iComponentType);
		m_securityHelper.WriteUnicodeString(stream, m_strName);
	}
}
