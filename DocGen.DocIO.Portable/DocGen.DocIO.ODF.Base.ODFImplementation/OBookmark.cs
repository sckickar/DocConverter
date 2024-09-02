namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OBookmark : OParagraphItem
{
	private string m_name;

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}
}
