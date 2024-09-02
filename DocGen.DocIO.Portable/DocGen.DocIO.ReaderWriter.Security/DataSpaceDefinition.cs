using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class DataSpaceDefinition
{
	private const int DefaultHeaderLength = 8;

	private int m_iHeaderLength = 8;

	private List<string> m_lstTransformRefs = new List<string>();

	private SecurityHelper m_securityHelper = new SecurityHelper();

	internal List<string> TransformRefs => m_lstTransformRefs;

	internal DataSpaceDefinition()
	{
	}

	internal DataSpaceDefinition(Stream stream)
	{
		byte[] buffer = new byte[4];
		m_iHeaderLength = m_securityHelper.ReadInt32(stream, buffer);
		int num = m_securityHelper.ReadInt32(stream, buffer);
		if (m_iHeaderLength != 8)
		{
			stream.Position += m_iHeaderLength - 8;
		}
		for (int i = 0; i < num; i++)
		{
			string item = m_securityHelper.ReadUnicodeString(stream);
			m_lstTransformRefs.Add(item);
		}
	}

	internal void Serialize(Stream stream)
	{
		m_securityHelper.WriteInt32(stream, m_iHeaderLength);
		int count = m_lstTransformRefs.Count;
		m_securityHelper.WriteInt32(stream, count);
		for (int i = 0; i < count; i++)
		{
			string value = m_lstTransformRefs[i];
			m_securityHelper.WriteUnicodeString(stream, value);
		}
	}
}
