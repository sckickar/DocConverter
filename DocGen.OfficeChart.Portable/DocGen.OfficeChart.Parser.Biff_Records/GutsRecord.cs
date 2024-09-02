using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Guts)]
[CLSCompliant(false)]
internal class GutsRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usLeftRowGutter;

	[BiffRecordPos(2, 2)]
	private ushort m_usTopColGutter;

	[BiffRecordPos(4, 2)]
	private ushort m_usMaxRowLevel;

	[BiffRecordPos(6, 2)]
	private ushort m_usMaxColLevel;

	public ushort LeftRowGutter
	{
		get
		{
			return m_usLeftRowGutter;
		}
		set
		{
			m_usLeftRowGutter = value;
		}
	}

	public ushort TopColumnGutter
	{
		get
		{
			return m_usTopColGutter;
		}
		set
		{
			m_usTopColGutter = value;
		}
	}

	public ushort MaxRowLevel
	{
		get
		{
			return m_usMaxRowLevel;
		}
		set
		{
			m_usMaxRowLevel = value;
		}
	}

	public ushort MaxColumnLevel
	{
		get
		{
			return m_usMaxColLevel;
		}
		set
		{
			m_usMaxColLevel = value;
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public GutsRecord()
	{
	}

	public GutsRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public GutsRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usLeftRowGutter = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usTopColGutter = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usMaxRowLevel = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usMaxColLevel = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usLeftRowGutter);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usTopColGutter);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usMaxRowLevel);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usMaxColLevel);
	}
}
