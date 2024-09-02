using System;
using System.IO;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.String)]
[CLSCompliant(false)]
internal class StringRecord : BiffRecordRaw
{
	private const int DEF_FIXED_SIZE = 3;

	[BiffRecordPos(0, 2)]
	private ushort m_usStringLength;

	private string m_strValue;

	private bool m_bIsUnicode;

	public string Value
	{
		get
		{
			return m_strValue;
		}
		set
		{
			m_strValue = value;
			m_usStringLength = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			m_bIsUnicode = !BiffRecordRawWithArray.IsAsciiString(value);
		}
	}

	public override int MinimumRecordSize => 4;

	public StringRecord()
	{
	}

	public StringRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public StringRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usStringLength = provider.ReadUInt16(iOffset);
		m_strValue = provider.ReadString(iOffset + 2, m_usStringLength, out var iBytesInString, isByteCounted: false);
		m_bIsUnicode = m_strValue.Length * 2 > iBytesInString;
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usStringLength);
		int offset = iOffset + 2;
		provider.WriteStringNoLenUpdateOffset(ref offset, m_strValue, m_bIsUnicode);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = (m_bIsUnicode ? Encoding.Unicode.GetByteCount(m_strValue) : m_strValue.Length);
		return 3 + num;
	}
}
