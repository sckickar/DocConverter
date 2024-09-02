using System;
using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class StdFontReader
{
	private const int HeadersCount = 4;

	private const int LastHeader = 3;

	private byte[] data;

	private StdHeader[] headers;

	private long position;

	public void ReadHeaders()
	{
		long num = 0L;
		headers = new StdHeader[4];
		for (int i = 0; i < 3; i++)
		{
			position = num;
			StdHeader stdHeader = new StdHeader(num, 6);
			stdHeader.Read(this);
			headers[i] = stdHeader;
			num += stdHeader.HeaderLength + stdHeader.NextHeaderOffset;
		}
		headers[3] = new StdHeader(data.Length - 2, 2);
	}

	public uint ReadUInt()
	{
		byte[] array = new byte[4];
		ReadLE(array, array.Length);
		return BitConverter.ToUInt32(array, 0);
	}

	public byte Read()
	{
		return data[(int)checked((nint)unchecked(position++))];
	}

	public byte[] ReadData(byte[] data)
	{
		this.data = data;
		ReadHeaders();
		List<byte> list = new List<byte>(data.Length);
		for (int i = 0; i < this.data.Length; i++)
		{
			if (!IsPositionInHeader(i))
			{
				list.Add(this.data[i]);
			}
		}
		return list.ToArray();
	}

	private bool ReadLE(byte[] buffer, int count)
	{
		for (int i = 0; i < count; i++)
		{
			try
			{
				buffer[i] = Read();
			}
			catch (ArgumentOutOfRangeException)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsPositionInHeader(long position)
	{
		StdHeader[] array = headers;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsPositionInside(position))
			{
				return true;
			}
		}
		return false;
	}
}
