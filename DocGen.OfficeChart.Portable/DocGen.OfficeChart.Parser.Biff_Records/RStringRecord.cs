using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.RString)]
[CLSCompliant(false)]
internal class RStringRecord : BiffRecordRawWithArray, ICellPositionFormat, IStringValue
{
	[CLSCompliant(false)]
	public struct TFormattingRun
	{
		public ushort FirstChar;

		public ushort FormatIndex;
	}

	[BiffRecordPos(0, 2)]
	private int m_iRow;

	[BiffRecordPos(2, 2)]
	private int m_iColumn;

	[BiffRecordPos(4, 2)]
	private ushort m_usExtFormat;

	private string m_strValue;

	private ushort m_usFRunsNumber;

	private TFormattingRun[] m_arrFormattingRuns;

	public int Row
	{
		get
		{
			return m_iRow;
		}
		set
		{
			m_iRow = value;
		}
	}

	public int Column
	{
		get
		{
			return m_iColumn;
		}
		set
		{
			m_iColumn = value;
		}
	}

	public ushort ExtendedFormatIndex
	{
		get
		{
			return m_usExtFormat;
		}
		set
		{
			m_usExtFormat = value;
		}
	}

	public string Value
	{
		get
		{
			return m_strValue;
		}
		set
		{
			m_strValue = value;
		}
	}

	public TFormattingRun[] FormattingRun
	{
		get
		{
			return m_arrFormattingRuns;
		}
		set
		{
			m_arrFormattingRuns = value;
			m_usFRunsNumber = (ushort)((value != null) ? ((ushort)value.Length) : 0);
		}
	}

	public override int MinimumRecordSize => 8;

	string IStringValue.StringValue => Value;

	public RStringRecord()
	{
	}

	public RStringRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public RStringRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		m_iRow = GetUInt16(0);
		m_iColumn = GetUInt16(2);
		m_usExtFormat = GetUInt16(4);
		int offset = 6;
		m_strValue = GetString16BitUpdateOffset(ref offset);
		m_usFRunsNumber = GetUInt16(offset);
		offset += 2;
		m_arrFormattingRuns = new TFormattingRun[m_usFRunsNumber];
		int num = 0;
		while (num < m_usFRunsNumber)
		{
			m_arrFormattingRuns[num].FirstChar = GetUInt16(offset);
			m_arrFormattingRuns[num].FormatIndex = GetUInt16(offset + 2);
			num++;
			offset += 4;
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		AutoGrowData = true;
		SetUInt16(0, (ushort)m_iRow);
		SetUInt16(2, (ushort)m_iColumn);
		SetUInt16(4, m_usExtFormat);
		m_iLength = 6;
		SetString16BitUpdateOffset(ref m_iLength, m_strValue);
		SetUInt16(m_iLength, m_usFRunsNumber);
		m_iLength += 2;
		int num = 0;
		while (num < m_usFRunsNumber)
		{
			SetUInt16(m_iLength, m_arrFormattingRuns[num].FirstChar);
			SetUInt16(m_iLength + 2, m_arrFormattingRuns[num].FormatIndex);
			num++;
			m_iLength += 4;
		}
	}
}
