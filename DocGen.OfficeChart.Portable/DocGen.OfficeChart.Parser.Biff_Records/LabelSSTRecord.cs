using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.LabelSST)]
[CLSCompliant(false)]
internal class LabelSSTRecord : CellPositionBase, ICloneable
{
	private const int DEF_RECORD_SIZE = 10;

	internal const int DEF_INDEX_OFFSET = 6;

	[BiffRecordPos(6, 4, true)]
	private int m_iSSTIndex;

	public int SSTIndex
	{
		get
		{
			return m_iSSTIndex;
		}
		set
		{
			m_iSSTIndex = value;
		}
	}

	public override int MinimumRecordSize => 10;

	public override int MaximumRecordSize => 10;

	public override int MaximumMemorySize => 10;

	protected override void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iSSTIndex = provider.ReadInt32(iOffset);
	}

	protected override void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteInt32(iOffset, m_iSSTIndex);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 10;
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}

	public static void SetSSTIndex(DataProvider provider, int iOffset, int iNewIndex, OfficeVersion version)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		iOffset += 10;
		if (version != 0)
		{
			iOffset += 4;
		}
		provider.WriteInt32(iOffset, iNewIndex);
	}

	public static int GetSSTIndex(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		iOffset += 10;
		if (version != 0)
		{
			iOffset += 4;
		}
		return provider.ReadInt32(iOffset);
	}
}
