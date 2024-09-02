using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Continue)]
[CLSCompliant(false)]
internal class ContinueRecord : BiffRecordRawWithArray, ILengthSetter
{
	public override bool NeedDataArray => true;

	public void SetLength(int len)
	{
		m_iLength = len;
	}

	public void SetData(byte[] arrData)
	{
		m_data = arrData;
	}

	public override void ParseStructure()
	{
	}

	public override void InfillInternalData(OfficeVersion version)
	{
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return m_iLength;
	}
}
