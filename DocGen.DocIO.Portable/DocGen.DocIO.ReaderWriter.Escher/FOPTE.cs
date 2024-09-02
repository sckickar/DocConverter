using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class FOPTE : BaseWordRecord
{
	private ushort m_pid;

	private ushort m_bid;

	private ushort m_complex;

	private uint m_op;

	private byte[] m_name;

	public ushort Pid
	{
		get
		{
			return m_pid;
		}
		set
		{
			m_pid = value;
		}
	}

	public bool IsBid
	{
		get
		{
			return m_bid == 1;
		}
		set
		{
			if (value)
			{
				m_bid = 1;
			}
			else
			{
				m_bid = 0;
			}
		}
	}

	public bool IsComplex
	{
		get
		{
			return m_complex == 1;
		}
		set
		{
			if (value)
			{
				m_complex = 1;
			}
			else
			{
				m_complex = 0;
			}
		}
	}

	public uint Op
	{
		get
		{
			return m_op;
		}
		set
		{
			m_op = value;
		}
	}

	public byte[] NameBytes
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	public int Read(Stream stream)
	{
		ushort num = BaseWordRecord.ReadUInt16(stream);
		m_pid = (ushort)(num & 0x3FFFu);
		m_bid = (ushort)((num & 0x4000) >> 14);
		m_complex = (ushort)((num & 0x8000) >> 15);
		m_op = BaseWordRecord.ReadUInt32(stream);
		int num2 = 6;
		if (IsComplex)
		{
			num2 += (int)m_op;
			m_name = new byte[m_op];
		}
		return num2;
	}

	public void Write(Stream stream)
	{
		short num = (short)m_pid;
		num += (short)(m_bid << 14);
		num += (short)(m_complex << 15);
		BaseWordRecord.WriteUInt16(stream, (ushort)num);
		BaseWordRecord.WriteUInt32(stream, m_op);
	}
}
