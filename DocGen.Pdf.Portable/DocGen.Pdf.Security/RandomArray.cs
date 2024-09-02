using System;

namespace DocGen.Pdf.Security;

internal class RandomArray : IRandom, IDisposable
{
	private byte[] m_array;

	public virtual long Length => m_array.Length;

	internal RandomArray(byte[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException();
		}
		m_array = array;
	}

	public virtual void Close()
	{
		m_array = null;
	}

	public virtual int Get(long offset)
	{
		if (offset >= m_array.Length)
		{
			return -1;
		}
		return 0xFF & m_array[(int)offset];
	}

	public virtual int Get(long offset, byte[] bytes, int off, int length)
	{
		if (m_array == null)
		{
			throw new InvalidOperationException("Closed array");
		}
		if (offset >= m_array.Length)
		{
			return -1;
		}
		if (offset + length > m_array.Length)
		{
			length = (int)(m_array.Length - offset);
		}
		Array.Copy(m_array, (int)offset, bytes, off, length);
		return length;
	}

	public virtual void Dispose()
	{
		Close();
	}
}
