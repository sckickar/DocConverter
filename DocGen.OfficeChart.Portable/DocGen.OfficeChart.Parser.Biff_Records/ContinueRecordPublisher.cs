using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class ContinueRecordPublisher
{
	private BinaryWriter m_writer;

	public ContinueRecordPublisher(BinaryWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		m_writer = writer;
	}

	public int PublishContinue(byte[] data, int start)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return PublishContinue(data, start, data.Length - start, 8224);
	}

	public int PublishContinue(byte[] data, int start, int length)
	{
		return PublishContinue(data, start, length, 8224);
	}

	public int PublishContinue(byte[] data, int start, int length, int maxSize)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length < start || start < 0)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (data.Length < start + length || length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (maxSize < 0 || maxSize > 8224)
		{
			throw new ArgumentOutOfRangeException("maxSize");
		}
		int num = 0;
		int num2 = start + length;
		int i;
		for (i = start; i < num2; i += maxSize)
		{
			int num3 = ((num2 - i < maxSize) ? (num2 - i) : maxSize);
			m_writer.Write((ushort)60);
			m_writer.Write((ushort)num3);
			m_writer.Write(data, i, num3);
			num += num3 + 4;
		}
		if (num2 > i)
		{
			int num4 = num2 - i;
			m_writer.Write((ushort)60);
			m_writer.Write((ushort)num4);
			m_writer.Write(data, i, num4);
			num += 4 + num4;
		}
		return num;
	}

	public int PublishContinue(byte[] data, int start, BiffRecordRawWithArray destination, int offset)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return PublishContinue(data, start, data.Length - start, 8224, destination, offset);
	}

	public int PublishContinue(byte[] data, int start, int length, BiffRecordRawWithArray destination, int offset)
	{
		return PublishContinue(data, start, length, 8224, destination, offset);
	}

	public int PublishContinue(byte[] data, int start, int length, int maxSize, BiffRecordRawWithArray destination, int offset)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length < start || start < 0)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (data.Length < start + length || length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (maxSize < 0 || maxSize > 8224)
		{
			throw new ArgumentOutOfRangeException("maxSize");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		int num = 0;
		int num2 = start + length;
		int i = start;
		bool autoGrowData = destination.AutoGrowData;
		destination.AutoGrowData = true;
		for (; i < num2; i += maxSize)
		{
			int num3 = ((num2 - i < maxSize) ? (num2 - i) : maxSize);
			destination.SetUInt16(offset, 60);
			destination.SetUInt16(offset + 2, (ushort)num3);
			destination.SetBytes(offset + 4, data, i, num3);
			num += num3 + 4;
			offset += num3 + 4;
		}
		if (num2 > i)
		{
			int num4 = num2 - i;
			destination.SetUInt16(offset, 60);
			destination.SetUInt16(offset + 2, (ushort)num4);
			destination.SetBytes(offset + 4, data, i, num4);
			num += num4 + 4;
			offset += num4 + 4;
		}
		destination.AutoGrowData = autoGrowData;
		return num;
	}
}
