using System;
using System.IO;
using System.Text;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Header)]
[Biff(TBIFFRecord.Footer)]
[CLSCompliant(false)]
internal class HeaderFooterRecord : BiffRecordRaw
{
	private string m_strValue = string.Empty;

	public string Value
	{
		get
		{
			return m_strValue;
		}
		set
		{
			m_strValue = value;
		}
	}

	public override int MinimumRecordSize => 0;

	public HeaderFooterRecord()
	{
	}

	public HeaderFooterRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public HeaderFooterRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		if (m_iLength > 0)
		{
			m_strValue = provider.ReadString16Bit(iOffset, out var iFullLength);
			if (iFullLength != m_iLength)
			{
				throw new WrongBiffRecordDataException("Wrong string or data length.");
			}
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		if (m_iLength > 0)
		{
			provider.WriteString16BitUpdateOffset(ref iOffset, m_strValue);
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (m_strValue != null && m_strValue.Length != 0)
		{
			return 3 + Encoding.Unicode.GetByteCount(m_strValue);
		}
		return 0;
	}
}
