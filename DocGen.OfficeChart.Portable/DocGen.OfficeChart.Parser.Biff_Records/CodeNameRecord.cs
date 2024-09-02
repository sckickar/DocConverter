using System;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CodeName)]
[CLSCompliant(false)]
internal class CodeNameRecord : BiffRecordRawWithArray
{
	private string m_strName;

	public string CodeName
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
		}
	}

	public CodeNameRecord()
	{
	}

	public CodeNameRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CodeNameRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		if (base.Length > 0)
		{
			AutoExtractFields();
			int uInt = GetUInt16(0);
			m_strName = GetString(2, uInt, out var iBytesInString);
			if (3 + iBytesInString != base.Length)
			{
				throw new WrongBiffRecordDataException("Wrong string or data length.");
			}
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_iLength = GetStoreSize(OfficeVersion.Excel97to2003);
		if (m_iLength > 0)
		{
			m_data = new byte[m_iLength];
			SetString16BitLen(0, m_strName);
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (m_strName == null || m_strName.Length == 0)
		{
			return 0;
		}
		return 3 + m_strName.Length * 2;
	}
}
