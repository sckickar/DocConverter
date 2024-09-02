using System;

namespace DocGen.Pdf.Security;

internal class WindowRandom : IRandom, IDisposable
{
	private readonly IRandom m_source;

	private readonly long m_offset;

	private readonly long m_length;

	public virtual long Length => m_length;

	internal WindowRandom(IRandom source, long offset, long length)
	{
		m_source = source;
		m_offset = offset;
		m_length = length;
	}

	public virtual int Get(long position)
	{
		if (position >= m_length)
		{
			return -1;
		}
		return m_source.Get(m_offset + position);
	}

	public virtual int Get(long position, byte[] bytes, int off, int len)
	{
		if (position >= m_length)
		{
			return -1;
		}
		long num = Math.Min(len, m_length - position);
		return m_source.Get(m_offset + position, bytes, off, (int)num);
	}

	public virtual void Close()
	{
		m_source.Close();
	}

	public virtual void Dispose()
	{
		Close();
	}
}
