using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DefaultRowHeight)]
[CLSCompliant(false)]
internal class DefaultRowHeightRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 4;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptionFlags;

	[BiffRecordPos(2, 2)]
	private ushort m_usRowHeigth = 255;

	private bool m_customHeight;

	public ushort OptionFlags
	{
		get
		{
			return m_usOptionFlags;
		}
		set
		{
			m_usOptionFlags = value;
		}
	}

	public ushort Height
	{
		get
		{
			return m_usRowHeigth;
		}
		set
		{
			m_usRowHeigth = value;
		}
	}

	public override int MinimumRecordSize => 4;

	public override int MaximumRecordSize => 4;

	internal bool CustomHeight => m_customHeight;

	public DefaultRowHeightRecord()
	{
	}

	public DefaultRowHeightRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DefaultRowHeightRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptionFlags = provider.ReadUInt16(iOffset);
		m_customHeight = provider.ReadBit(iOffset, 0);
		iOffset += 2;
		m_usRowHeigth = provider.ReadUInt16(2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 4;
		provider.WriteUInt16(iOffset, m_usOptionFlags);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usRowHeigth);
	}
}
