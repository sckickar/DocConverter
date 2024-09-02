namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OBreak : OParagraphItem
{
	private OBreakType m_breakType;

	internal OBreakType BreakType
	{
		get
		{
			return m_breakType;
		}
		set
		{
			m_breakType = value;
		}
	}
}
