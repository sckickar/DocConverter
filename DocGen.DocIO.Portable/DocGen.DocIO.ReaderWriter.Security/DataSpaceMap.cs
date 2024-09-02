using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class DataSpaceMap
{
	private const int DefaultHeaderSize = 8;

	private int m_iHeaderSize = 8;

	private List<DataSpaceMapEntry> m_lstMapEntries = new List<DataSpaceMapEntry>();

	private SecurityHelper m_securityHelper = new SecurityHelper();

	internal List<DataSpaceMapEntry> MapEntries => m_lstMapEntries;

	internal DataSpaceMap()
	{
	}

	internal DataSpaceMap(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] buffer = new byte[4];
		m_iHeaderSize = m_securityHelper.ReadInt32(stream, buffer);
		int num = m_securityHelper.ReadInt32(stream, buffer);
		if (m_lstMapEntries.Capacity < num)
		{
			m_lstMapEntries.Capacity = num;
		}
		if (m_iHeaderSize != 8)
		{
			stream.Position += m_iHeaderSize - 8;
		}
		for (int i = 0; i < num; i++)
		{
			DataSpaceMapEntry item = new DataSpaceMapEntry(stream);
			m_lstMapEntries.Add(item);
		}
	}

	internal void Serialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_securityHelper.WriteInt32(stream, m_iHeaderSize);
		int count = m_lstMapEntries.Count;
		m_securityHelper.WriteInt32(stream, count);
		for (int i = 0; i < count; i++)
		{
			m_lstMapEntries[i].Serialize(stream);
		}
	}
}
