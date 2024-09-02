using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSerAuxErrBar)]
[CLSCompliant(false)]
internal class ChartSerAuxErrBarRecord : BiffRecordRaw
{
	public enum TErrorBarValue
	{
		XDirectionPlus = 1,
		XDirectionMinus,
		YDirectionPlus,
		YDirectionMinus
	}

	public const int DefaultRecordSize = 14;

	[BiffRecordPos(0, 1)]
	private byte m_ErrorBarValue;

	[BiffRecordPos(1, 1)]
	private byte m_ErrorBarType = 2;

	[BiffRecordPos(2, 1)]
	private byte m_TeeTop = 1;

	[BiffRecordPos(3, 1)]
	private byte m_Reserved = 1;

	[BiffRecordPos(4, 8, TFieldType.Float)]
	private double m_NumValue = 10.0;

	[BiffRecordPos(12, 2)]
	private ushort m_usValuesNumber;

	public TErrorBarValue ErrorBarValue
	{
		get
		{
			return (TErrorBarValue)m_ErrorBarValue;
		}
		set
		{
			m_ErrorBarValue = (byte)value;
		}
	}

	public OfficeErrorBarType ErrorBarType
	{
		get
		{
			return (OfficeErrorBarType)m_ErrorBarType;
		}
		set
		{
			m_ErrorBarType = (byte)value;
		}
	}

	public bool TeeTop
	{
		get
		{
			return m_TeeTop == 1;
		}
		set
		{
			m_TeeTop = (value ? ((byte)1) : ((byte)0));
		}
	}

	public byte Reserved => m_Reserved;

	public double NumValue
	{
		get
		{
			return m_NumValue;
		}
		set
		{
			m_NumValue = value;
		}
	}

	public ushort ValuesNumber
	{
		get
		{
			return m_usValuesNumber;
		}
		set
		{
			m_usValuesNumber = value;
		}
	}

	public ChartSerAuxErrBarRecord()
	{
	}

	public ChartSerAuxErrBarRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSerAuxErrBarRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_ErrorBarValue = provider.ReadByte(iOffset);
		m_ErrorBarType = provider.ReadByte(iOffset + 1);
		m_TeeTop = provider.ReadByte(iOffset + 2);
		m_Reserved = provider.ReadByte(iOffset + 3);
		m_NumValue = provider.ReadDouble(iOffset + 4);
		m_usValuesNumber = provider.ReadUInt16(iOffset + 12);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteByte(iOffset, m_ErrorBarValue);
		provider.WriteByte(iOffset + 1, m_ErrorBarType);
		provider.WriteByte(iOffset + 2, m_TeeTop);
		provider.WriteByte(iOffset + 3, m_Reserved);
		provider.WriteDouble(iOffset + 4, m_NumValue);
		provider.WriteUInt16(iOffset + 12, m_usValuesNumber);
		m_iLength = 14;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 14;
	}
}
