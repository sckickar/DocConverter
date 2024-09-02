namespace DocGen.Pdf;

internal class DecodeIntResult
{
	private int m_intResult;

	private bool m_booleanResult;

	internal int IntResult => m_intResult;

	internal bool BooleanResult => m_booleanResult;

	internal DecodeIntResult(int intResult, bool booleanResult)
	{
		m_intResult = intResult;
		m_booleanResult = booleanResult;
	}
}
