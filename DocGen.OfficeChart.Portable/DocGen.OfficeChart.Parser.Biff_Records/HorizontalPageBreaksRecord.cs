using System;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.HorizontalPageBreaks)]
[CLSCompliant(false)]
internal class HorizontalPageBreaksRecord : BiffRecordRawWithArray
{
	public class THPageBreak : ICloneable
	{
		private ushort m_usRow;

		private ushort m_usStartCol;

		private ushort m_usEndCol;

		public ushort Row
		{
			get
			{
				return m_usRow;
			}
			set
			{
				m_usRow = value;
			}
		}

		public ushort StartColumn
		{
			get
			{
				return m_usStartCol;
			}
			set
			{
				m_usStartCol = value;
			}
		}

		public ushort EndColumn
		{
			get
			{
				return m_usEndCol;
			}
			set
			{
				m_usEndCol = value;
			}
		}

		public THPageBreak()
		{
		}

		public THPageBreak(ushort Row, ushort StartCol, ushort EndCol)
		{
			m_usRow = Row;
			m_usStartCol = StartCol;
			m_usEndCol = EndCol;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	private const int DEF_FIXED_PART_SIZE = 2;

	internal const int DEF_SUBITEM_SIZE = 6;

	internal const int FixedSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usBreaksCount;

	private THPageBreak[] m_arrPageBreaks;

	public THPageBreak[] PageBreaks
	{
		get
		{
			return m_arrPageBreaks;
		}
		set
		{
			m_arrPageBreaks = value;
			m_usBreaksCount = (ushort)((value != null) ? ((ushort)value.Length) : 0);
		}
	}

	public override int MinimumRecordSize => 2;

	public HorizontalPageBreaksRecord()
	{
	}

	public HorizontalPageBreaksRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public HorizontalPageBreaksRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		m_usBreaksCount = GetUInt16(0);
		m_arrPageBreaks = new THPageBreak[m_usBreaksCount];
		int num = 2;
		int num2 = 0;
		while (num2 < m_usBreaksCount)
		{
			ushort uInt = GetUInt16(num);
			ushort uInt2 = GetUInt16(num + 2);
			ushort uInt3 = GetUInt16(num + 4);
			m_arrPageBreaks[num2] = new THPageBreak(uInt, uInt2, uInt3);
			num2++;
			num += 6;
		}
		if (num != m_iLength)
		{
			throw new WrongBiffRecordDataException();
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_data = new byte[GetStoreSize(OfficeVersion.Excel97to2003)];
		SetUInt16(0, m_usBreaksCount);
		m_iLength = 2;
		int num = 0;
		while (num < m_usBreaksCount)
		{
			SetUInt16(m_iLength, m_arrPageBreaks[num].Row);
			SetUInt16(m_iLength + 2, m_arrPageBreaks[num].StartColumn);
			SetUInt16(m_iLength + 4, m_arrPageBreaks[num].EndColumn);
			num++;
			m_iLength += 6;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2 + 6 * m_usBreaksCount;
	}
}
