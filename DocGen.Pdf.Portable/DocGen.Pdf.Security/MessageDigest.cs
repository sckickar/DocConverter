using System;

namespace DocGen.Pdf.Security;

internal abstract class MessageDigest : IMessageDigest
{
	private const int c_byteLength = 64;

	private byte[] m_buf;

	private int m_bufOff;

	private long m_byteCount;

	public int ByteLength => 64;

	public abstract string AlgorithmName { get; }

	public abstract int MessageDigestSize { get; }

	internal MessageDigest()
	{
		m_buf = new byte[4];
	}

	internal MessageDigest(MessageDigest t)
	{
		m_buf = new byte[t.m_buf.Length];
		Array.Copy(t.m_buf, 0, m_buf, 0, t.m_buf.Length);
		m_bufOff = t.m_bufOff;
		m_byteCount = t.m_byteCount;
	}

	public void Update(byte input)
	{
		m_buf[m_bufOff++] = input;
		if (m_bufOff == m_buf.Length)
		{
			ProcessWord(m_buf, 0);
			m_bufOff = 0;
		}
		m_byteCount++;
	}

	public void Update(byte[] bytes, int offset, int length)
	{
		while (m_bufOff != 0 && length > 0)
		{
			Update(bytes[offset]);
			offset++;
			length--;
		}
		while (length > m_buf.Length)
		{
			ProcessWord(bytes, offset);
			offset += m_buf.Length;
			length -= m_buf.Length;
			m_byteCount += m_buf.Length;
		}
		while (length > 0)
		{
			Update(bytes[offset]);
			offset++;
			length--;
		}
	}

	public void BlockUpdate(byte[] bytes, int offset, int length)
	{
		while (m_bufOff != 0 && length > 0)
		{
			Update(bytes[offset]);
			offset++;
			length--;
		}
		while (length > m_buf.Length)
		{
			ProcessWord(bytes, offset);
			offset += m_buf.Length;
			length -= m_buf.Length;
			m_byteCount += m_buf.Length;
		}
		while (length > 0)
		{
			Update(bytes[offset]);
			offset++;
			length--;
		}
	}

	internal void Finish()
	{
		long bitLength = m_byteCount << 3;
		Update(128);
		while (m_bufOff != 0)
		{
			Update(0);
		}
		ProcessLength(bitLength);
		ProcessBlock();
	}

	public virtual void Reset()
	{
		m_byteCount = 0L;
		m_bufOff = 0;
		Array.Clear(m_buf, 0, m_buf.Length);
	}

	internal abstract void ProcessWord(byte[] input, int inOff);

	internal abstract void ProcessLength(long bitLength);

	internal abstract void ProcessBlock();

	public abstract int DoFinal(byte[] bytes, int offset);
}
