namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class TextStyle : DataStyle
{
	private bool m_textContent;

	internal bool TextContent
	{
		get
		{
			return m_textContent;
		}
		set
		{
			m_textContent = value;
		}
	}

	internal TextStyle()
	{
	}
}
