using System;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation;

[Preserve(AllMembers = true)]
internal class EvaluateEventArgs : EventArgs
{
	private IRange m_range;

	private Ptg[] m_FormulaTokens;

	public IRange Range => m_range;

	public Ptg[] PtgArray => m_FormulaTokens;

	public new static EvaluateEventArgs Empty => new EvaluateEventArgs();

	private EvaluateEventArgs()
	{
	}

	public EvaluateEventArgs(IRange range, Ptg[] array)
	{
		m_range = range;
		m_FormulaTokens = array;
	}
}
