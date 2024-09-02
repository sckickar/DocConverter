using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal class UnknownEndRecord : BiffRecordRaw
{
	private const int DEF_UNKNOWN1 = 449;

	private const int DEF_UNKNOWN2 = 144525;

	private const int DefaultRecordSize = 8;

	[BiffRecordPos(0, 4, true)]
	private int m_iUnknown1;

	[BiffRecordPos(4, 4, true)]
	private int m_iUnknown2;

	public int Unknown1
	{
		get
		{
			return m_iUnknown1;
		}
		set
		{
			m_iUnknown1 = value;
		}
	}

	public int Unknown2
	{
		get
		{
			return m_iUnknown2;
		}
		set
		{
			m_iUnknown2 = value;
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public UnknownEndRecord()
	{
	}

	public UnknownEndRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public UnknownEndRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iUnknown1 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iUnknown2 = provider.ReadInt32(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iUnknown1 = 449;
		m_iUnknown2 = 144525;
		m_iLength = 8;
		provider.WriteInt32(iOffset, m_iUnknown1);
		provider.WriteInt32(iOffset + 4, m_iUnknown2);
	}
}
