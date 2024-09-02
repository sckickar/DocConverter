namespace DocGen.Office.Markdown;

internal class MdThematicBreak : IMdBlock
{
	private const string m_horizontalRuleChar = "---";

	internal string HorizontalRuleChar => "---";

	public void Close()
	{
	}
}
