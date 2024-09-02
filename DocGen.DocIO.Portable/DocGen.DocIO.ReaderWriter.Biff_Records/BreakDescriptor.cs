using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class BreakDescriptor : BaseWordRecord
{
	internal const int DEF_BKD_SIZE = 6;

	internal short m_ipgd;

	internal short m_dcpDepend;

	internal byte m_iCol;

	internal byte m_options;

	internal short Ipgd
	{
		get
		{
			return m_ipgd;
		}
		set
		{
			m_ipgd = value;
		}
	}

	internal short DcpDepend
	{
		get
		{
			return m_dcpDepend;
		}
		set
		{
			m_dcpDepend = value;
		}
	}

	internal byte Options
	{
		get
		{
			return m_options;
		}
		set
		{
			m_options = value;
		}
	}

	internal bool TableBreak
	{
		get
		{
			return (m_options & 1) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= 1;
			}
			else
			{
				m_options = (byte)(m_options & 0xFFFFFFFEu);
			}
		}
	}

	internal bool ColumnBreak
	{
		get
		{
			return (m_options & 2) >> 1 != 0;
		}
		set
		{
			if (value)
			{
				m_options |= 2;
			}
			else
			{
				m_options = (byte)(m_options & 0xFFFFFFFDu);
			}
		}
	}

	internal bool Marked
	{
		get
		{
			return (m_options & 4) >> 2 != 0;
		}
		set
		{
			if (value)
			{
				m_options |= 4;
			}
			else
			{
				m_options = (byte)(m_options & 0xFFFFFFFBu);
			}
		}
	}

	internal bool Unk
	{
		get
		{
			return (m_options & 8) >> 3 != 0;
		}
		set
		{
			if (value)
			{
				m_options |= 8;
			}
			else
			{
				m_options = (byte)(m_options & 0xFFFFFFF7u);
			}
		}
	}

	internal bool TextOverflow
	{
		get
		{
			return (m_options & 0x10) >> 4 != 0;
		}
		set
		{
			if (value)
			{
				m_options |= 16;
			}
			else
			{
				m_options = (byte)(m_options & 0xFFFFFFEFu);
			}
		}
	}

	internal BreakDescriptor()
	{
	}

	internal BreakDescriptor(Stream stream)
	{
		Read(stream);
	}

	internal void Read(Stream stream)
	{
		m_ipgd = BaseWordRecord.ReadInt16(stream);
		m_dcpDepend = BaseWordRecord.ReadInt16(stream);
		m_iCol = (byte)stream.ReadByte();
		m_options = (byte)stream.ReadByte();
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteInt16(stream, m_ipgd);
		BaseWordRecord.WriteInt16(stream, m_dcpDepend);
		stream.WriteByte(m_iCol);
		stream.WriteByte(m_options);
	}
}
