using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontReaderBase
{
	private readonly byte[] data;

	private readonly Stack<long> beginReadingPositions;

	private long position;

	public bool EndOfFile
	{
		get
		{
			if (data != null)
			{
				return Position >= Length;
			}
			return true;
		}
	}

	public int Length => data.Length;

	public virtual long Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	public SystemFontReaderBase(byte[] data)
	{
		position = 0L;
		this.data = data;
		beginReadingPositions = new Stack<long>();
	}

	public virtual void BeginReadingBlock()
	{
		beginReadingPositions.Push(position);
	}

	public virtual void EndReadingBlock()
	{
		if (beginReadingPositions.Count > 0)
		{
			position = beginReadingPositions.Pop();
		}
	}

	public virtual byte Read()
	{
		return data[(int)checked((nint)unchecked(position++))];
	}

	public virtual byte Peek(int skip)
	{
		return data[(int)checked((nint)unchecked(position + skip))];
	}

	public int Read(byte[] buffer, int count)
	{
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (Position >= Length)
			{
				return num;
			}
			buffer[i] = Read();
			num++;
		}
		return num;
	}

	public int ReadBE(byte[] buffer, int count)
	{
		int num = 0;
		for (int num2 = count - 1; num2 >= 0; num2--)
		{
			if (Position >= Length)
			{
				return num;
			}
			buffer[num2] = Read();
			num++;
		}
		return num;
	}

	public virtual void Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			position = offset;
			break;
		case SeekOrigin.Current:
			position += offset;
			break;
		case SeekOrigin.End:
			position = Length - offset;
			break;
		}
	}
}
