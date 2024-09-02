using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal abstract class BaseStream : Stream
{
	protected readonly Stream m_input;

	private int m_limit;

	private bool m_closed;

	internal int Remaining => m_limit;

	public sealed override bool CanRead => !m_closed;

	public sealed override bool CanSeek => false;

	public sealed override bool CanWrite => false;

	public sealed override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public sealed override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	internal BaseStream(Stream stream, int limit)
	{
		m_input = stream;
		m_limit = limit;
	}

	protected virtual void SetParentEndOfFileDetect(bool isDetect)
	{
		if (m_input is Asn1LengthStream)
		{
			((Asn1LengthStream)m_input).SetEndOfFileOnStart(isDetect);
		}
	}

	public new void Close()
	{
		m_closed = true;
	}

	public sealed override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = offset;
		try
		{
			int num2 = offset + count;
			while (num < num2)
			{
				int num3 = ReadByte();
				if (num3 != -1)
				{
					buffer[num++] = (byte)num3;
					continue;
				}
				break;
			}
		}
		catch (IOException)
		{
			if (num == offset)
			{
				throw;
			}
		}
		return num - offset;
	}

	public sealed override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public sealed override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public sealed override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}
