using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class DataSpaceMapEntry
{
	private List<DataSpaceReferenceComponent> m_lstComponents = new List<DataSpaceReferenceComponent>();

	private string m_strDataSpaceName;

	private SecurityHelper m_securityHelper = new SecurityHelper();

	internal List<DataSpaceReferenceComponent> Components => m_lstComponents;

	internal string DataSpaceName
	{
		get
		{
			return m_strDataSpaceName;
		}
		set
		{
			m_strDataSpaceName = value;
		}
	}

	internal DataSpaceMapEntry()
	{
	}

	internal DataSpaceMapEntry(Stream stream)
	{
		byte[] buffer = new byte[4];
		m_securityHelper.ReadInt32(stream, buffer);
		int num = m_securityHelper.ReadInt32(stream, buffer);
		for (int i = 0; i < num; i++)
		{
			DataSpaceReferenceComponent item = new DataSpaceReferenceComponent(stream);
			m_lstComponents.Add(item);
		}
		m_strDataSpaceName = m_securityHelper.ReadUnicodeString(stream);
	}

	internal void Serialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long position = stream.Position;
		stream.Position += 4L;
		int count = m_lstComponents.Count;
		m_securityHelper.WriteInt32(stream, count);
		for (int i = 0; i < count; i++)
		{
			m_lstComponents[i].Serialize(stream);
		}
		m_securityHelper.WriteUnicodeString(stream, m_strDataSpaceName);
		long position2 = stream.Position;
		stream.Position = position;
		m_securityHelper.WriteInt32(stream, (int)(position2 - position));
		stream.Position = position2;
	}
}
