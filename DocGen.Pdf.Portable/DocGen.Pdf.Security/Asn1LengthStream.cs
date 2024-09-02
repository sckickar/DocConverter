using System.IO;

namespace DocGen.Pdf.Security;

internal class Asn1LengthStream : BaseStream
{
	private int m_byte;

	private bool m_isEndOfFile = true;

	internal Asn1LengthStream(Stream stream, int limit)
		: base(stream, limit)
	{
		m_byte = RequireByte();
		CheckEndOfFile();
	}

	internal void SetEndOfFileOnStart(bool isEOF)
	{
		m_isEndOfFile = isEOF;
		if (m_isEndOfFile)
		{
			CheckEndOfFile();
		}
	}

	private bool CheckEndOfFile()
	{
		if (m_byte == 0)
		{
			if (RequireByte() != 0)
			{
				throw new IOException("Invalid content");
			}
			m_byte = -1;
			SetParentEndOfFileDetect(isDetect: true);
			return true;
		}
		return m_byte < 0;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (m_isEndOfFile || count <= 1)
		{
			return base.Read(buffer, offset, count);
		}
		if (m_byte < 0)
		{
			return 0;
		}
		int num = m_input.Read(buffer, offset + 1, count - 1);
		if (num <= 0)
		{
			throw new EndOfStreamException();
		}
		buffer[offset] = (byte)m_byte;
		m_byte = RequireByte();
		return num + 1;
	}

	public override int ReadByte()
	{
		if (m_isEndOfFile && CheckEndOfFile())
		{
			return -1;
		}
		int @byte = m_byte;
		m_byte = RequireByte();
		return @byte;
	}

	private int RequireByte()
	{
		int num = m_input.ReadByte();
		if (num < 0)
		{
			throw new EndOfStreamException();
		}
		return num;
	}
}
