using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal class FutureHeader
{
	[BiffRecordPos(0, 2)]
	private ushort m_usType;

	[BiffRecordPos(2, 2)]
	private ushort m_usAttributes;

	public ushort Type
	{
		get
		{
			return m_usType;
		}
		set
		{
			m_usType = value;
		}
	}

	public ushort Attributes
	{
		get
		{
			return m_usAttributes;
		}
		set
		{
			m_usAttributes = value;
		}
	}

	public void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, Type);
		iOffset += 2;
		provider.WriteUInt16(iOffset, Attributes);
		iOffset += 2;
		provider.WriteInt64(iOffset, 0L);
	}

	public int GetStoreSize()
	{
		return 12;
	}
}
