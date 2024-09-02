using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal abstract class BiffRecordWithContinue : BiffRecordRawWithDataProvider
{
	private int DEF_WORD_MASK = 65535;

	internal List<int> m_arrContinuePos = new List<int>();

	protected int m_iFirstLength = -1;

	public virtual TBIFFRecord FirstContinueType => TBIFFRecord.Continue;

	protected virtual bool AddHeaderToProvider => false;

	public BiffRecordWithContinue()
	{
	}

	public override object Clone()
	{
		BiffRecordWithContinue biffRecordWithContinue = (BiffRecordWithContinue)base.Clone();
		biffRecordWithContinue.m_arrContinuePos = CloneUtils.CloneCloneable(m_arrContinuePos);
		if (m_provider != null)
		{
			biffRecordWithContinue.m_provider = ApplicationImpl.CreateDataProvider();
			biffRecordWithContinue.m_provider.EnsureCapacity(m_provider.Capacity);
			m_provider.CopyTo(0, biffRecordWithContinue.m_provider, 0, m_provider.Capacity);
		}
		return biffRecordWithContinue;
	}
}
