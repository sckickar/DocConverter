using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.UnkMarker)]
[CLSCompliant(false)]
internal class UnknownMarkerRecord : BiffRecordRaw
{
	private const ushort DEF_RESERVED1 = 55;

	[BiffRecordPos(0, 2)]
	private ushort m_usReserved0;

	[BiffRecordPos(2, 2)]
	private ushort m_usReserved1 = 55;

	[BiffRecordPos(4, 2)]
	private ushort m_usReserved2;

	public ushort Reserved0
	{
		get
		{
			return m_usReserved0;
		}
		set
		{
			m_usReserved0 = value;
		}
	}

	public ushort Reserved1
	{
		get
		{
			return m_usReserved1;
		}
		set
		{
			m_usReserved1 = value;
		}
	}

	public ushort Reserved2
	{
		get
		{
			return m_usReserved2;
		}
		set
		{
			m_usReserved2 = value;
		}
	}

	public override int MinimumRecordSize => 6;

	public override int MaximumRecordSize => 6;

	public UnknownMarkerRecord()
	{
	}

	public UnknownMarkerRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public UnknownMarkerRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usReserved0 = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usReserved1 = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usReserved2 = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usReserved0 = 0;
		m_usReserved1 = 55;
		m_usReserved2 = 0;
		provider.WriteUInt16(iOffset, m_usReserved0);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usReserved1);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usReserved2);
	}
}
