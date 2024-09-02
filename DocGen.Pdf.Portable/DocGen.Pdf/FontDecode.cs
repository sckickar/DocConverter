using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf;

internal class FontDecode
{
	internal MemoryStream m_fontStream = new MemoryStream();

	internal FontHeader m_fontHeader;

	internal int m_tableCount;

	private int tagOffset;

	private int offset;

	private int previousLength;

	internal MemoryStream CreateFontStream(List<TableEntry> entries)
	{
		m_tableCount = entries.Count;
		FontHeader head = default(FontHeader);
		head.scalarType = 65536;
		head.noOfTables = (short)m_tableCount;
		head.searchRange = (short)GetSearchRange(head.noOfTables);
		head.entrySelector = (short)GetEntrySelector(head.noOfTables);
		head.rangeShift = (short)GetRangeShift(head.noOfTables, head.searchRange);
		WriteHeader(head);
		tagOffset = 12 + entries.Count * 16;
		foreach (TableEntry entry in entries)
		{
			tagOffset += previousLength;
			entry.offset = tagOffset;
			previousLength = entry.length;
			WriteEntry(entry);
		}
		foreach (TableEntry entry2 in entries)
		{
			WriteBytes(entry2.bytes);
		}
		m_fontStream.Capacity = (int)m_fontStream.Length + 1;
		return m_fontStream;
	}

	private void WriteHeader(FontHeader head)
	{
		m_fontStream.Position = 0L;
		WriteInt(head.scalarType);
		WriteShort(head.noOfTables);
		WriteShort(head.searchRange);
		WriteShort(head.entrySelector);
		WriteShort(head.rangeShift);
		offset = (int)m_fontStream.Position;
	}

	private void WriteEntry(TableEntry entry)
	{
		WriteString(entry.id);
		WriteInt(entry.checkSum);
		WriteInt(entry.offset);
		WriteInt(entry.length);
	}

	private void WriteShort(short value)
	{
		byte[] array = new byte[2];
		array[1] = (byte)value;
		array[0] = (byte)(value >> 8);
		m_fontStream.Write(array, 0, 2);
	}

	private void WriteInt(int value)
	{
		byte[] array = new byte[4];
		array[3] = (byte)value;
		array[2] = (byte)(value >> 8);
		array[1] = (byte)(value >> 16);
		array[0] = (byte)(value >> 24);
		m_fontStream.Write(array, 0, 4);
	}

	private void WriteString(string value)
	{
		byte[] array = new byte[value.Length];
		int num = 0;
		foreach (char c in value)
		{
			array[num] = (byte)c;
			num++;
		}
		m_fontStream.Write(array, 0, 4);
	}

	public void WriteBytes(byte[] buffer)
	{
		m_fontStream.Write(buffer, 0, buffer.Length);
	}

	private int GetSearchRange(int noOfTables)
	{
		int num = 2;
		while (num * 2 <= noOfTables)
		{
			num *= 2;
		}
		return num * 16;
	}

	private int GetEntrySelector(int noOfTables)
	{
		int num = 2;
		while (num * 2 <= noOfTables)
		{
			num *= 2;
		}
		return (int)Math.Log(num, 2.0);
	}

	private int GetRangeShift(int noOfTables, int searchRange)
	{
		return noOfTables * 16 - searchRange;
	}
}
