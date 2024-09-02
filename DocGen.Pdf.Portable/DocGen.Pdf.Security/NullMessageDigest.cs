using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class NullMessageDigest : IMessageDigest
{
	private readonly MemoryStream m_stream = new MemoryStream();

	public string AlgorithmName => "NULL";

	public int MessageDigestSize => 0;

	public int ByteLength => 0;

	public int GetByteLength()
	{
		return 0;
	}

	public int GetDigestSize()
	{
		return (int)m_stream.Length;
	}

	public void Update(byte b)
	{
		m_stream.WriteByte(b);
	}

	public void BlockUpdate(byte[] inBytes, int inOff, int len)
	{
		m_stream.Write(inBytes, inOff, len);
	}

	public int DoFinal(byte[] outBytes, int outOff)
	{
		byte[] array = m_stream.ToArray();
		array.CopyTo(outBytes, outOff);
		Reset();
		return array.Length;
	}

	public void Reset()
	{
		m_stream.SetLength(0L);
	}

	public void Update(byte[] bytes, int offset, int length)
	{
		throw new NotImplementedException();
	}
}
