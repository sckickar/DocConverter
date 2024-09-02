using System;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.MergeCells)]
[CLSCompliant(false)]
internal class MergeCellsRecord : BiffRecordRaw
{
	[CLSCompliant(false)]
	public class MergedRegion : ICloneable
	{
		private int m_iRowFrom;

		private int m_iRowTo;

		private int m_iColFrom;

		private int m_iColTo;

		public int RowFrom
		{
			get
			{
				return m_iRowFrom;
			}
			set
			{
				m_iRowFrom = value;
			}
		}

		public int RowTo
		{
			get
			{
				return m_iRowTo;
			}
			set
			{
				m_iRowTo = value;
			}
		}

		public int ColumnFrom => m_iColFrom;

		public int ColumnTo
		{
			get
			{
				return m_iColTo;
			}
			set
			{
				m_iColTo = value;
			}
		}

		public int CellsCount => (m_iRowTo - m_iRowFrom + 1) * (m_iColTo - m_iColFrom + 1);

		private MergedRegion()
		{
		}

		public MergedRegion(MergedRegion region)
			: this(region.RowFrom, region.RowTo, region.ColumnFrom, region.ColumnTo)
		{
		}

		public MergedRegion(int rowFrom, int rowTo, int colFrom, int colTo)
		{
			m_iRowFrom = rowFrom;
			m_iRowTo = rowTo;
			m_iColFrom = colFrom;
			m_iColTo = colTo;
		}

		public void MoveRegion(int iRowDelta, int iColDelta)
		{
			m_iRowTo += iRowDelta;
			m_iRowFrom += iRowDelta;
			m_iColFrom += iColDelta;
			m_iColTo += iColDelta;
		}

		internal Rectangle GetRectangle()
		{
			return Rectangle.FromLTRB(m_iColFrom, m_iRowFrom, m_iColTo, m_iRowTo);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public static bool Equals(MergedRegion region1, MergedRegion region2)
		{
			if (region1 == null && region2 == null)
			{
				return true;
			}
			if (region1 == null || region2 == null)
			{
				return false;
			}
			return region1.Equals(region2);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (!(obj is MergedRegion mergedRegion))
			{
				throw new ArgumentException("obj");
			}
			if (m_iColFrom == mergedRegion.m_iColFrom && m_iColTo == mergedRegion.m_iColTo && m_iRowFrom == mergedRegion.m_iRowFrom)
			{
				return m_iRowTo == mergedRegion.m_iRowTo;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return m_iColFrom.GetHashCode() | m_iColTo.GetHashCode() | m_iRowTo.GetHashCode() | m_iRowFrom.GetHashCode();
		}
	}

	public const int DEF_MAXIMUM_REGIONS = 1027;

	private const int DEF_FIXED_SIZE = 2;

	private const int DEF_SUB_ITEM_SIZE = 8;

	[BiffRecordPos(0, 2)]
	private ushort m_usNumber;

	private MergedRegion[] m_arrRegions;

	public ushort RangesNumber => m_usNumber;

	public MergedRegion[] Regions
	{
		get
		{
			return m_arrRegions;
		}
		set
		{
			m_arrRegions = value;
			m_usNumber = (ushort)m_arrRegions.Length;
		}
	}

	public override int MinimumRecordSize => 2;

	public MergeCellsRecord()
	{
	}

	public MergeCellsRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public MergeCellsRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usNumber = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_arrRegions = new MergedRegion[m_usNumber];
		InternalDataIntegrityCheck();
		int num = 0;
		while (num < m_usNumber)
		{
			m_arrRegions[num] = new MergedRegion(provider.ReadUInt16(iOffset), provider.ReadUInt16(iOffset + 2), provider.ReadUInt16(iOffset + 4), provider.ReadUInt16(iOffset + 6));
			num++;
			iOffset += 8;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usNumber);
		m_iLength = GetStoreSize(version);
		iOffset += 2;
		int num = 0;
		while (num < m_usNumber)
		{
			provider.WriteUInt16(iOffset, (ushort)m_arrRegions[num].RowFrom);
			provider.WriteUInt16(iOffset + 2, (ushort)m_arrRegions[num].RowTo);
			provider.WriteUInt16(iOffset + 4, (ushort)m_arrRegions[num].ColumnFrom);
			provider.WriteUInt16(iOffset + 6, (ushort)m_arrRegions[num].ColumnTo);
			num++;
			iOffset += 8;
		}
	}

	private void InternalDataIntegrityCheck()
	{
		if (m_iLength != m_usNumber * 8 + 2 || (m_iLength - 2) % 8 != 0)
		{
			throw new WrongBiffRecordDataException("MergeCellsRecord");
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2 + m_arrRegions.Length * 8;
	}

	public void SetRegions(int iStartIndex, int iCount, MergedRegion[] arrRegions)
	{
		if (arrRegions == null)
		{
			throw new ArgumentNullException("arrRegions");
		}
		int num = arrRegions.Length;
		if (iStartIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iStartIndex");
		}
		if (iCount < 0 || iStartIndex + iCount > num)
		{
			throw new ArgumentOutOfRangeException("iRegionsCount");
		}
		if (m_usNumber != iCount)
		{
			m_arrRegions = new MergedRegion[iCount];
			m_usNumber = (ushort)iCount;
		}
		Array.Copy(arrRegions, iStartIndex, m_arrRegions, 0, iCount);
	}
}
