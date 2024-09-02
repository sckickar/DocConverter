using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class RandomStream : Stream
{
	private IRandom m_random;

	private long m_position;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length => m_random.Length;

	public override long Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal RandomStream(IRandom source)
	{
		m_random = source;
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int length)
	{
		int num = m_random.Get(m_position, buffer, offset, length);
		if (num == -1)
		{
			return 0;
		}
		m_position += num;
		return num;
	}

	public override int ReadByte()
	{
		int num = m_random.Get(m_position);
		if (num >= 0)
		{
			m_position++;
		}
		return num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			m_position = offset;
			break;
		case SeekOrigin.Current:
			m_position += offset;
			break;
		default:
			m_position = offset + m_random.Length;
			break;
		}
		return m_position;
	}

	public override void SetLength(long value)
	{
		throw new Exception("Not supported.");
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new Exception("Not supported.");
	}
}
