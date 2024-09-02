using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.VerticalPageBreaks)]
[CLSCompliant(false)]
internal class VerticalPageBreaksRecord : BiffRecordRaw
{
	public class TVPageBreak : ICloneable
	{
		private ushort m_usCol;

		private uint m_uiStartRow;

		private uint m_uiEndRow;

		public ushort Column
		{
			get
			{
				return m_usCol;
			}
			set
			{
				m_usCol = value;
			}
		}

		public uint StartRow
		{
			get
			{
				return m_uiStartRow;
			}
			set
			{
				m_uiStartRow = value;
			}
		}

		public uint EndRow
		{
			get
			{
				return m_uiEndRow;
			}
			set
			{
				m_uiEndRow = value;
			}
		}

		public TVPageBreak()
		{
		}

		public TVPageBreak(ushort Col, ushort StartRow, ushort EndRow)
		{
			m_usCol = Col;
			m_uiStartRow = StartRow;
			m_uiEndRow = EndRow;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	internal const int DEF_FIXED_PART_SIZE = 2;

	internal const int DEF_SUBITEM_SIZE = 6;

	[BiffRecordPos(0, 2)]
	private ushort m_usBreaksCount;

	private TVPageBreak[] m_arrPageBreaks;

	public TVPageBreak[] PageBreaks
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

	public VerticalPageBreaksRecord()
	{
	}

	public VerticalPageBreaksRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public VerticalPageBreaksRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usBreaksCount = provider.ReadUInt16(iOffset);
		m_arrPageBreaks = new TVPageBreak[m_usBreaksCount];
		iOffset += 2;
		int num = 0;
		while (num < m_usBreaksCount)
		{
			ushort col = provider.ReadUInt16(iOffset);
			ushort startRow = provider.ReadUInt16(iOffset + 2);
			ushort endRow = provider.ReadUInt16(iOffset + 4);
			m_arrPageBreaks[num] = new TVPageBreak(col, startRow, endRow);
			num++;
			iOffset += 6;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usBreaksCount);
		m_iLength = 2;
		int num = 0;
		while (num < m_usBreaksCount)
		{
			provider.WriteUInt16(iOffset + m_iLength, m_arrPageBreaks[num].Column);
			provider.WriteUInt16(iOffset + m_iLength + 2, (ushort)m_arrPageBreaks[num].StartRow);
			provider.WriteUInt16(iOffset + m_iLength + 4, (ushort)m_arrPageBreaks[num].EndRow);
			num++;
			m_iLength += 6;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2 + 6 * m_usBreaksCount;
	}
}
