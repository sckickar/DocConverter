using System;
using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class CompoundStreamWrapper : CompoundStream
{
	private CompoundStream m_stream;

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

	public CompoundStreamWrapper(CompoundStream wrapped)
		: base(wrapped.Name)
	{
		m_stream = wrapped;
	}

	public override void Flush()
	{
		m_stream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return m_stream.Read(buffer, offset, count);
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

	protected override void Dispose(bool disposing)
	{
		if (m_stream != null)
		{
			base.Dispose(disposing);
			m_stream = null;
			GC.SuppressFinalize(this);
		}
	}
}
