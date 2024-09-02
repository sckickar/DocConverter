using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class OctetStream : Stream
{
	private Asn1Parser m_helper;

	private bool m_first = true;

	private Stream m_stream;

	private bool m_closed;

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

	internal OctetStream(Asn1Parser helper)
	{
		m_helper = helper;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (m_stream == null)
		{
			if (!m_first)
			{
				return 0;
			}
			IAsn1Octet asn1Octet = (IAsn1Octet)m_helper.ReadObject();
			if (asn1Octet == null)
			{
				return 0;
			}
			m_first = false;
			m_stream = asn1Octet.GetOctetStream();
		}
		int num = 0;
		while (true)
		{
			int num2 = m_stream.Read(buffer, offset + num, count - num);
			if (num2 > 0)
			{
				num += num2;
				if (num == count)
				{
					return num;
				}
				continue;
			}
			IAsn1Octet asn1Octet2 = (IAsn1Octet)m_helper.ReadObject();
			if (asn1Octet2 == null)
			{
				break;
			}
			m_stream = asn1Octet2.GetOctetStream();
		}
		m_stream = null;
		return num;
	}

	public override int ReadByte()
	{
		if (m_stream == null)
		{
			if (!m_first)
			{
				return 0;
			}
			IAsn1Octet asn1Octet = (IAsn1Octet)m_helper.ReadObject();
			if (asn1Octet == null)
			{
				return 0;
			}
			m_first = false;
			m_stream = asn1Octet.GetOctetStream();
		}
		while (true)
		{
			int num = m_stream.ReadByte();
			if (num >= 0)
			{
				return num;
			}
			IAsn1Octet asn1Octet2 = (IAsn1Octet)m_helper.ReadObject();
			if (asn1Octet2 == null)
			{
				break;
			}
			m_stream = asn1Octet2.GetOctetStream();
		}
		m_stream = null;
		return -1;
	}

	public new void Close()
	{
		m_closed = true;
	}

	public sealed override void Flush()
	{
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
