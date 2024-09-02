using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DCONBIN)]
[CLSCompliant(false)]
internal class DConBinRecord : BiffRecordRawWithArray
{
	[BiffRecordPos(0, TFieldType.String16Bit)]
	private string m_strName;

	private string m_strWorkbookName;

	private byte[] arrdata;

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_strName = value;
		}
	}

	public string WorkbookName
	{
		get
		{
			return m_strWorkbookName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_strWorkbookName = value;
		}
	}

	public DConBinRecord()
	{
	}

	public DConBinRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DConBinRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		arrdata = m_data;
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		AutoGrowData = true;
		SetBytes(0, arrdata);
	}
}
