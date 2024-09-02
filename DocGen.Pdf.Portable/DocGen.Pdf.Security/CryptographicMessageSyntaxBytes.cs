using System.IO;

namespace DocGen.Pdf.Security;

internal class CryptographicMessageSyntaxBytes
{
	private readonly byte[] m_bytes;

	internal CryptographicMessageSyntaxBytes(byte[] bytes)
	{
		m_bytes = bytes;
	}

	internal virtual void Write(Stream stream)
	{
		stream.Write(m_bytes, 0, m_bytes.Length);
	}
}
