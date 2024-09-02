using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartWrapper)]
[CLSCompliant(false)]
internal class ChartWrapperRecord : BiffRecordRaw, ICloneable
{
	private const int DEF_RECORD_OFFSET = 4;

	private BiffRecordRaw m_record;

	public BiffRecordRaw Record
	{
		get
		{
			return m_record;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_record = value;
		}
	}

	public ChartWrapperRecord()
	{
	}

	public ChartWrapperRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartWrapperRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_record = BiffRecordFactory.GetRecord(provider, iOffset + 4, version);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		int storeSize = m_record.GetStoreSize(version);
		m_iLength = 4 + storeSize + 4;
		provider.WriteUInt16(iOffset + 4, (ushort)m_record.TypeCode);
		provider.WriteUInt16(iOffset + 4 + 2, (ushort)storeSize);
		m_record.InfillInternalData(provider, iOffset + 4 + 4, version);
		provider.WriteUInt16(iOffset, (ushort)base.TypeCode);
		provider.WriteUInt16(iOffset + 2, 0);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8 + m_record.GetStoreSize(version);
	}

	public new object Clone()
	{
		object obj = base.Clone();
		if (m_record != null)
		{
			((ChartWrapperRecord)obj).m_record = (BiffRecordRaw)m_record.Clone();
		}
		return obj;
	}
}
