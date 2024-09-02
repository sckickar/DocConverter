using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class Rect : BaseWordRecord
{
	private long m_left;

	private long m_right;

	private long m_top;

	private long m_bottom;

	public long Left
	{
		get
		{
			return m_left;
		}
		set
		{
			m_left = value;
		}
	}

	public long Right
	{
		get
		{
			return m_right;
		}
		set
		{
			m_right = value;
		}
	}

	public long Top
	{
		get
		{
			return m_top;
		}
		set
		{
			m_top = value;
		}
	}

	public long Bottom
	{
		get
		{
			return m_bottom;
		}
		set
		{
			m_bottom = value;
		}
	}

	public void Read(Stream stream)
	{
		m_left = stream.ReadByte();
		m_top = stream.ReadByte();
		m_right = stream.ReadByte();
		m_bottom = stream.ReadByte();
	}

	public void Write(Stream stream)
	{
		stream.WriteByte((byte)m_left);
		stream.WriteByte((byte)m_top);
		stream.WriteByte((byte)m_right);
		stream.WriteByte((byte)m_bottom);
	}
}
