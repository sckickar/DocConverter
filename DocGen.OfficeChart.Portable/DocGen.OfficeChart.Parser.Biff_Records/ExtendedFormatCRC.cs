using System;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ExtendedFormatCRC)]
[CLSCompliant(false)]
internal class ExtendedFormatCRC : BiffRecordRaw
{
	private FutureHeader m_header;

	private ushort m_usXFCount;

	private uint m_uiCRC;

	private WorkbookImpl m_book;

	public ushort XFCount
	{
		get
		{
			return m_usXFCount;
		}
		set
		{
			m_usXFCount = value;
		}
	}

	public uint CRCChecksum
	{
		get
		{
			return m_uiCRC;
		}
		set
		{
			m_uiCRC = value;
		}
	}

	public ExtendedFormatCRC()
	{
		m_header = new FutureHeader();
		m_header.Type = 2172;
		m_usXFCount = 16;
	}

	public override void ParseStructure(DataProvider arrData, int iOffset, int iLength, OfficeVersion version)
	{
		m_header.Type = arrData.ReadUInt16(iOffset);
		iOffset += 2;
		m_header.Attributes = arrData.ReadByte(iOffset);
		iOffset += 2;
		arrData.ReadByte(iOffset);
		iOffset += 8;
		arrData.ReadByte(iOffset);
		iOffset += 2;
		m_usXFCount = arrData.ReadUInt16(iOffset);
		iOffset += 2;
		m_uiCRC = arrData.ReadUInt32(iOffset);
		iOffset += 4;
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		ushort num = 0;
		provider.WriteUInt16(iOffset, m_header.Type);
		provider.WriteUInt16(iOffset + 2, m_header.Attributes);
		provider.WriteInt64(iOffset + 4, num);
		provider.WriteUInt16(iOffset + 12, num);
		provider.WriteUInt16(iOffset + 14, m_usXFCount);
		provider.WriteUInt32(iOffset + 16, m_uiCRC);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 20;
	}

	public override int GetHashCode()
	{
		return m_header.Type.GetHashCode() ^ m_header.Attributes.GetHashCode() ^ m_usXFCount.GetHashCode() ^ m_uiCRC.GetHashCode();
	}

	public override object Clone()
	{
		return new ExtendedFormatCRC();
	}
}
