using System;
using System.IO;
using System.Text;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Format)]
[CLSCompliant(false)]
internal class FormatRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usIndex;

	[BiffRecordPos(2, 2)]
	private ushort m_usFormatStringLen;

	private string m_strFormatString = string.Empty;

	public int Index
	{
		get
		{
			return m_usIndex;
		}
		set
		{
			m_usIndex = (ushort)value;
		}
	}

	public string FormatString
	{
		get
		{
			return m_strFormatString;
		}
		set
		{
			if (m_strFormatString != value)
			{
				m_strFormatString = value;
				m_usFormatStringLen = (ushort)m_strFormatString.Length;
			}
		}
	}

	public override int MinimumRecordSize => 5;

	public FormatRecord()
	{
	}

	public FormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public FormatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usIndex = provider.ReadUInt16(iOffset);
		m_usFormatStringLen = provider.ReadUInt16(iOffset + 2);
		FormatString = provider.ReadString(iOffset + 4, m_usFormatStringLen, out var iBytesInString, isByteCounted: false);
		if (m_iLength != 5 + iBytesInString)
		{
			throw new WrongBiffRecordDataException("m_iLength and String length do not fit each other.");
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usIndex);
		provider.WriteUInt16(iOffset + 2, m_usFormatStringLen);
		provider.WriteByte(iOffset + 4, 1);
		provider.WriteBytes(iOffset + 5, Encoding.Unicode.GetBytes(m_strFormatString), 0, m_usFormatStringLen * 2);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 5 + m_usFormatStringLen * 2;
	}
}
