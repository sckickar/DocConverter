using System;
using System.IO;
using System.Text;

namespace DocGen.DocIO.DLS.Convertors;

public class RtfReader
{
	private const byte b_endTag = 125;

	private byte[] m_rtfData;

	private Encoding m_encoding;

	private int m_position;

	private long m_length;

	public byte[] RtfData => m_rtfData;

	public Encoding Encoding => m_encoding;

	public int Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	public long Length
	{
		get
		{
			return m_length;
		}
		set
		{
			m_length = value;
		}
	}

	public RtfReader(Stream stream)
	{
		m_rtfData = new byte[stream.Length];
		stream.Read(m_rtfData, 0, (int)stream.Length);
		m_length = m_rtfData.Length;
		m_encoding = WordDocument.GetEncoding("Windows-1252");
	}

	public char ReadChar()
	{
		byte result = m_rtfData[m_position];
		m_position++;
		return (char)result;
	}

	public string ReadImageBytes()
	{
		int num = Array.IndexOf(m_rtfData, (byte)125, m_position);
		string @string = Encoding.GetString(m_rtfData, m_position, num - m_position);
		m_position = num;
		return @string.Replace(ControlChar.CrLf, "");
	}

	public void Close()
	{
		m_rtfData = null;
		m_encoding = null;
	}
}
