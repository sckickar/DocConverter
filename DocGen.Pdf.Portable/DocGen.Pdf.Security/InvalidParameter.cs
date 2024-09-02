using System;

namespace DocGen.Pdf.Security;

internal class InvalidParameter : ICipherParam
{
	private readonly ICipherParam m_parameters;

	private readonly byte[] m_bytes;

	internal byte[] InvalidBytes => (byte[])m_bytes.Clone();

	internal ICipherParam Parameters => m_parameters;

	internal InvalidParameter(ICipherParam parameters, byte[] bytes)
		: this(parameters, bytes, 0, bytes.Length)
	{
	}

	internal InvalidParameter(ICipherParam parameters, byte[] bytes, int offset, int length)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		m_parameters = parameters;
		m_bytes = new byte[length];
		Array.Copy(bytes, offset, m_bytes, 0, length);
	}
}
