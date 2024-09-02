using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf;

internal class TiffDecode
{
	internal MemoryStream m_stream;

	internal TiffHeader m_tiffHeader;

	internal TiffDirectoryEntry m_directory = new TiffDirectoryEntry();

	internal const int LittleEndianVersion = 42;

	internal const int BigEndianVersion = 43;

	internal const short BigEndian = 19789;

	internal const short LittleEndian = 18761;

	internal const short MdiLittleEndian = 20549;

	internal List<TiffDirectoryEntry> directoryEntries = new List<TiffDirectoryEntry>();

	public TiffDecode()
	{
		m_stream = new MemoryStream();
	}

	internal void SetField(int count, int offset, TiffTag tag, TiffType type)
	{
		TiffDirectoryEntry tiffDirectoryEntry = new TiffDirectoryEntry();
		tiffDirectoryEntry.DirectoryCount = count;
		tiffDirectoryEntry.DirectoryOffset = (uint)offset;
		tiffDirectoryEntry.DirectoryTag = tag;
		tiffDirectoryEntry.DirectoryType = type;
		directoryEntries.Add(tiffDirectoryEntry);
	}

	internal void WriteHeader(TiffHeader header)
	{
		WriteShort(header.m_byteOrder);
		WriteShort(header.m_version);
		WriteInt((int)header.m_dirOffset);
	}

	internal void WriteDirEntry(List<TiffDirectoryEntry> entries)
	{
		int count = entries.Count;
		WriteShort((short)count);
		for (int i = 0; i < count; i++)
		{
			WriteShort((short)entries[i].DirectoryTag);
			WriteShort((short)entries[i].DirectoryType);
			WriteInt(entries[i].DirectoryCount);
			WriteInt((int)entries[i].DirectoryOffset);
		}
		WriteInt(0);
	}

	private void WriteShort(short value)
	{
		byte[] buffer = new byte[2]
		{
			(byte)value,
			(byte)(value >> 8)
		};
		m_stream.Write(buffer, 0, 2);
	}

	private void WriteInt(int value)
	{
		byte[] buffer = new byte[4]
		{
			(byte)value,
			(byte)(value >> 8),
			(byte)(value >> 16),
			(byte)(value >> 24)
		};
		m_stream.Write(buffer, 0, 4);
	}
}
