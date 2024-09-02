namespace DocGen.Office;

internal class LineInfo
{
	internal string m_text;

	internal float m_width;

	internal LineType m_lineType;

	internal OtfGlyphInfoList OpenTypeGlyphList;

	public LineType LineType
	{
		get
		{
			return m_lineType;
		}
		internal set
		{
			m_lineType = value;
		}
	}

	public string Text
	{
		get
		{
			return m_text;
		}
		internal set
		{
			m_text = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		internal set
		{
			m_width = value;
		}
	}
}
