namespace DocGen.Layouting;

internal class HtmlToDocLayoutInfo
{
	private bool m_bRemoveLineBreak = true;

	internal bool RemoveLineBreak
	{
		get
		{
			return m_bRemoveLineBreak;
		}
		set
		{
			m_bRemoveLineBreak = value;
		}
	}
}
