namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class EmbeddedTextType
{
	private int m_position;

	private string m_content;

	internal int Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal string Content
	{
		get
		{
			return m_content;
		}
		set
		{
			m_content = value;
		}
	}
}
