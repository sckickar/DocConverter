using System;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.LabelRanges)]
[CLSCompliant(false)]
internal class LabelRangesRecord : BiffRecordRawWithArray
{
	[BiffRecordPos(0, 2)]
	private ushort m_usRowRangesCount;

	private TAddr[] m_arrRowRanges;

	private ushort m_usColRangesCount;

	private TAddr[] m_arrColRanges;

	public ushort RowRangesCount => m_usRowRangesCount;

	public TAddr[] RowRanges
	{
		get
		{
			return m_arrRowRanges;
		}
		set
		{
			m_arrRowRanges = value;
			m_usRowRangesCount = (ushort)((value != null) ? ((ushort)value.Length) : 0);
		}
	}

	public ushort ColRangesCount => m_usColRangesCount;

	public TAddr[] ColRanges
	{
		get
		{
			return m_arrColRanges;
		}
		set
		{
			m_arrColRanges = value;
			m_usColRangesCount = (ushort)((value != null) ? ((ushort)value.Length) : 0);
		}
	}

	public override int MinimumRecordSize => 4;

	public LabelRangesRecord()
	{
	}

	public LabelRangesRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public LabelRangesRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		AutoExtractFields();
		m_arrRowRanges = new TAddr[m_usRowRangesCount];
		int num = 2;
		int num2 = 0;
		while (num2 < m_usRowRangesCount)
		{
			m_arrRowRanges[num2] = GetAddr(num);
			num2++;
			num += 8;
		}
		m_usColRangesCount = GetUInt16(num);
		m_arrColRanges = new TAddr[m_usColRangesCount];
		num += 2;
		int num3 = 0;
		while (num3 < m_usColRangesCount)
		{
			m_arrColRanges[num3] = GetAddr(num);
			num3++;
			num += 8;
		}
		if (num != m_iLength)
		{
			throw new WrongBiffRecordDataException();
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		AutoExtractFields();
		int num = 2;
		int num2 = 0;
		while (num2 < m_usRowRangesCount)
		{
			SetAddr(num, m_arrRowRanges[num2]);
			num2++;
			num += 8;
		}
		SetUInt16(num, m_usColRangesCount);
		num += 2;
		int num3 = 0;
		while (num3 < m_usColRangesCount)
		{
			SetAddr(num, m_arrColRanges[num3]);
			num3++;
			num += 8;
		}
	}
}
