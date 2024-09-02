using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal class HeaderContinueRecordBuilder : ContinueRecordBuilder
{
	public override int MaximumSize => 8224 - HeaderFooterImageRecord.DEF_DATA_OFFSET;

	public HeaderContinueRecordBuilder(BiffContinueRecordRaw parent)
		: base(parent)
	{
		base.ContinueType = TBIFFRecord.HeaderFooterImage;
		base.FirstContinueType = TBIFFRecord.HeaderFooterImage;
	}

	public override int AppendBytes(byte[] data, int start, int length)
	{
		int num = 0;
		if (CheckIfSpaceNeeded(length))
		{
			int num2 = start + length;
			for (int i = start; i < num2; i += m_iMax)
			{
				UpdateContinueRecordSize();
				StartContinueRecord();
				num++;
				m_parent.SetBytes(m_iPos, HeaderFooterImageRecord.DEF_CONTINUE_START, 0, HeaderFooterImageRecord.DEF_DATA_OFFSET);
				UpdateCounters(HeaderFooterImageRecord.DEF_DATA_OFFSET);
				int num3 = ((num2 - i < m_iMax) ? (num2 - i) : m_iMax);
				m_parent.SetBytes(m_iPos, data, i, num3);
				UpdateCounters(num3);
			}
		}
		else
		{
			m_parent.SetBytes(m_iPos, data, start, length);
			UpdateCounters(length);
		}
		UpdateContinueRecordSize();
		return num;
	}
}
