using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DVal)]
[CLSCompliant(false)]
internal class DValRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bPromtBoxVisible;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_bPromtBoxPosFixed;

	[BiffRecordPos(0, 2, TFieldType.Bit)]
	private bool m_bDataCached;

	[BiffRecordPos(2, 4, true)]
	private int m_iPromtBoxHPos;

	[BiffRecordPos(6, 4, true)]
	private int m_iPromtBoxVPos;

	[BiffRecordPos(10, 4)]
	private uint m_uiObjectId = uint.MaxValue;

	[BiffRecordPos(14, 4)]
	private uint m_uiDVNumber;

	public ushort Options => m_usOptions;

	public bool IsPromtBoxVisible
	{
		get
		{
			return m_bPromtBoxVisible;
		}
		set
		{
			m_bPromtBoxVisible = value;
		}
	}

	public bool IsPromtBoxPosFixed
	{
		get
		{
			return m_bPromtBoxPosFixed;
		}
		set
		{
			m_bPromtBoxPosFixed = value;
		}
	}

	public bool IsDataCached
	{
		get
		{
			return m_bDataCached;
		}
		set
		{
			m_bDataCached = value;
		}
	}

	public int PromtBoxHPos
	{
		get
		{
			return m_iPromtBoxHPos;
		}
		set
		{
			m_iPromtBoxHPos = value;
		}
	}

	public int PromtBoxVPos
	{
		get
		{
			return m_iPromtBoxVPos;
		}
		set
		{
			m_iPromtBoxVPos = value;
		}
	}

	public uint ObjectId
	{
		get
		{
			return m_uiObjectId;
		}
		set
		{
			m_uiObjectId = value;
		}
	}

	public uint DVNumber
	{
		get
		{
			return m_uiDVNumber;
		}
		set
		{
			m_uiDVNumber = value;
		}
	}

	public override int MinimumRecordSize => 18;

	public override int MaximumRecordSize => 18;

	public DValRecord()
	{
	}

	public DValRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DValRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bPromtBoxVisible = provider.ReadBit(iOffset, 0);
		m_bPromtBoxPosFixed = provider.ReadBit(iOffset, 1);
		m_bDataCached = provider.ReadBit(iOffset, 2);
		iOffset += 2;
		m_iPromtBoxHPos = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iPromtBoxVPos = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_uiObjectId = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiDVNumber = provider.ReadUInt32(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bPromtBoxVisible, 0);
		provider.WriteBit(iOffset, m_bPromtBoxPosFixed, 1);
		provider.WriteBit(iOffset, m_bDataCached, 2);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iPromtBoxHPos);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iPromtBoxVPos);
		iOffset += 4;
		provider.WriteUInt32(iOffset, m_uiObjectId);
		iOffset += 4;
		provider.WriteUInt32(iOffset, m_uiDVNumber);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DValRecord dValRecord))
		{
			return false;
		}
		if (dValRecord.IsPromtBoxVisible == IsPromtBoxVisible && dValRecord.IsPromtBoxPosFixed == IsPromtBoxPosFixed && dValRecord.IsDataCached == IsDataCached && dValRecord.PromtBoxHPos == PromtBoxHPos && dValRecord.PromtBoxVPos == PromtBoxVPos)
		{
			return dValRecord.ObjectId == ObjectId;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_bDataCached.GetHashCode() + m_bPromtBoxPosFixed.GetHashCode() + m_bPromtBoxPosFixed.GetHashCode() + m_iPromtBoxHPos.GetHashCode() + m_iPromtBoxVPos.GetHashCode() + m_uiObjectId.GetHashCode();
	}
}
