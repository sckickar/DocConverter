using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAxisDisplayUnits)]
[CLSCompliant(false)]
internal class ChartAxisDisplayUnitsRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 16;

	[BiffRecordPos(4, 2)]
	private ushort m_displayUnit;

	[BiffRecordPos(6, 8, TFieldType.Float)]
	private double m_displayUnitValue;

	[BiffRecordPos(14, 1)]
	private byte m_isShowLabels;

	[BiffRecordPos(15, 1)]
	private byte m_reserved;

	public OfficeChartDisplayUnit DisplayUnit
	{
		get
		{
			return (OfficeChartDisplayUnit)m_displayUnit;
		}
		set
		{
			m_displayUnit = (ushort)value;
		}
	}

	public double DisplayUnitValue
	{
		get
		{
			return m_displayUnitValue;
		}
		set
		{
			m_displayUnitValue = value;
		}
	}

	public bool IsShowLabels
	{
		get
		{
			return m_isShowLabels == 3;
		}
		set
		{
			m_isShowLabels = (byte)((!value) ? 1 : 3);
		}
	}

	public byte Recerved
	{
		get
		{
			return m_reserved;
		}
		set
		{
			m_reserved = value;
		}
	}

	public override int MinimumRecordSize => 16;

	public override int MaximumRecordSize => 16;

	public ChartAxisDisplayUnitsRecord()
	{
	}

	public ChartAxisDisplayUnitsRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAxisDisplayUnitsRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		iOffset += 4;
		m_displayUnit = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_displayUnitValue = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_isShowLabels = provider.ReadByte(iOffset);
		iOffset++;
		m_reserved = provider.ReadByte(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, (ushort)base.TypeCode);
		iOffset += 2;
		provider.WriteUInt16(iOffset, 0);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_displayUnit);
		iOffset += 2;
		provider.WriteDouble(iOffset, m_displayUnitValue);
		iOffset += 8;
		_ = new byte[2] { m_isShowLabels, m_reserved };
		provider.WriteByte(iOffset, m_isShowLabels);
		iOffset++;
		provider.WriteByte(iOffset, m_reserved);
		m_iLength = 16;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 16;
	}
}
