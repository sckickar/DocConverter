namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTextRange : OParagraphItem
{
	private string m_text;

	internal new string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}
}
