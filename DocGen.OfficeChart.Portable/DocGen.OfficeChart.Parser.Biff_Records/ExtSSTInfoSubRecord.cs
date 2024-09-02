using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ExtSSTInfoSub)]
[CLSCompliant(false)]
internal class ExtSSTInfoSubRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 8;

	[BiffRecordPos(0, 4, true)]
	private int m_iStreamPos;

	[BiffRecordPos(4, 2)]
	private ushort m_usBucketSSTOffset;

	[BiffRecordPos(6, 2)]
	private ushort m_usReserved;

	public int StreamPosition
	{
		get
		{
			return m_iStreamPos;
		}
		set
		{
			m_iStreamPos = value;
		}
	}

	public ushort BucketSSTOffset
	{
		get
		{
			return m_usBucketSSTOffset;
		}
		set
		{
			if (value > 8228)
			{
				throw new ArgumentOutOfRangeException("BucketSSTOffset", "Bucket SST Offset cannot be larger then MAX record size. On each Continue Record offset must be started from zero.");
			}
			m_usBucketSSTOffset = value;
		}
	}

	public ushort Reserved => m_usReserved;

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public ExtSSTInfoSubRecord()
	{
	}

	public ExtSSTInfoSubRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ExtSSTInfoSubRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iStreamPos = provider.ReadInt32(iOffset);
		m_usBucketSSTOffset = provider.ReadUInt16(iOffset + 4);
		m_usReserved = provider.ReadUInt16(iOffset + 6);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteInt32(iOffset, m_iStreamPos);
		provider.WriteUInt16(iOffset + 4, m_usBucketSSTOffset);
		provider.WriteUInt16(iOffset + 6, m_usReserved);
		m_iLength = 8;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8;
	}
}
