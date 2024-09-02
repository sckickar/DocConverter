using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.PrinterSettings)]
[CLSCompliant(false)]
internal class PrinterSettingsRecord : BiffRecordWithContinue
{
	protected override bool AddHeaderToProvider => true;

	public override bool NeedDataArray => true;

	public override void ParseStructure()
	{
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_iFirstLength = ((m_iLength > 8224) ? 8224 : (-1));
	}

	public override object Clone()
	{
		PrinterSettingsRecord printerSettingsRecord = (PrinterSettingsRecord)base.Clone();
		if (m_provider != null && !m_provider.IsCleared)
		{
			printerSettingsRecord.m_provider.EnsureCapacity(m_iLength);
			m_provider.CopyTo(0, printerSettingsRecord.m_provider, 0, m_iLength);
		}
		return printerSettingsRecord;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return m_iLength;
	}
}
