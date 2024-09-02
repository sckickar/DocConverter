using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.FileSharing)]
[CLSCompliant(false)]
internal class FileSharingRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usRecommendReadOnly;

	[BiffRecordPos(2, 2)]
	private ushort m_usHashPassword;

	[BiffRecordPos(4, 2, TFieldType.String16Bit)]
	private string m_strCreatorName;

	public ushort RecommendReadOnly
	{
		get
		{
			return m_usRecommendReadOnly;
		}
		set
		{
			m_usRecommendReadOnly = value;
		}
	}

	public ushort HashPassword
	{
		get
		{
			return m_usHashPassword;
		}
		set
		{
			m_usHashPassword = value;
		}
	}

	public string CreatorName
	{
		get
		{
			return m_strCreatorName;
		}
		set
		{
			m_strCreatorName = value;
		}
	}

	public override int MinimumRecordSize => 6;

	public FileSharingRecord()
	{
	}

	public FileSharingRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public FileSharingRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usRecommendReadOnly = provider.ReadUInt16(iOffset);
		m_usHashPassword = provider.ReadUInt16(iOffset + 2);
		m_strCreatorName = provider.ReadString16Bit(iOffset + 4, out var _);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usRecommendReadOnly);
		provider.WriteUInt16(iOffset + 2, m_usHashPassword);
		m_iLength = 4;
		m_iLength += provider.WriteString16Bit(iOffset + 4, m_strCreatorName);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = ((m_strCreatorName != null) ? m_strCreatorName.Length : 0);
		return 4 + ((num > 0) ? (3 + num * 2) : 2);
	}
}
