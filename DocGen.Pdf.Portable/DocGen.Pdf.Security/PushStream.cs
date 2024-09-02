using System.IO;

namespace DocGen.Pdf.Security;

internal class PushStream : Stream
{
	private int m_buffer = -1;

	internal Stream m_stream;

	public override bool CanRead => m_stream.CanRead;

	public override bool CanSeek => m_stream.CanSeek;

	public override bool CanWrite => m_stream.CanWrite;

	public override long Length => m_stream.Length;

	public override long Position
	{
		get
		{
			return m_stream.Position;
		}
		set
		{
			m_stream.Position = value;
		}
	}

	internal PushStream(Stream stream)
	{
		m_stream = stream;
	}

	public override int ReadByte()
	{
		if (m_buffer != -1)
		{
			int buffer = m_buffer;
			m_buffer = -1;
			return buffer;
		}
		return m_stream.ReadByte();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (m_buffer != -1 && count > 0)
		{
			buffer[offset] = (byte)m_buffer;
			m_buffer = -1;
			return 1;
		}
		return m_stream.Read(buffer, offset, count);
	}

	public virtual void Unread(int b)
	{
		m_buffer = b & 0xFF;
	}

	public new void Close()
	{
		m_stream.Dispose();
	}

	public override void Flush()
	{
		m_stream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return m_stream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		m_stream.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		m_stream.Write(buffer, offset, count);
	}

	public override void WriteByte(byte value)
	{
		m_stream.WriteByte(value);
	}
}
