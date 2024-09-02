namespace DocGen.Pdf.Graphics.Fonts;

internal struct FeatureRecord
{
	private string m_tag;

	private int[] m_indexes;

	internal string Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	internal int[] Indexes
	{
		get
		{
			return m_indexes;
		}
		set
		{
			m_indexes = value;
		}
	}
}
