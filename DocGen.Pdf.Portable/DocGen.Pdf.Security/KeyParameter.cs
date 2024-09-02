using System;

namespace DocGen.Pdf.Security;

internal class KeyParameter : ICipherParam
{
	private readonly byte[] m_bytes;

	internal byte[] Keys => (byte[])m_bytes.Clone();

	internal KeyParameter(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		m_bytes = (byte[])bytes.Clone();
	}

	internal KeyParameter(byte[] bytes, int offset, int length)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (offset < 0 || offset > bytes.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (length < 0 || offset + length > bytes.Length)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		m_bytes = new byte[length];
		Array.Copy(bytes, offset, m_bytes, 0, length);
	}
}
