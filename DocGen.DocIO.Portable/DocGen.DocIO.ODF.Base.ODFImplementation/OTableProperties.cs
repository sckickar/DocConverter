namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTableProperties : CommonTableParaProperties
{
	private bool m_hasColor;

	private bool m_mayBreakBetweenRows;

	private bool m_display;

	private float m_tableWidth;

	private HoriAlignment m_horizontalAlignment;

	internal float TableWidth
	{
		get
		{
			return m_tableWidth;
		}
		set
		{
			m_tableWidth = value;
		}
	}

	internal bool HasColor
	{
		get
		{
			return m_hasColor;
		}
		set
		{
			m_hasColor = value;
		}
	}

	internal bool Display
	{
		get
		{
			return m_display;
		}
		set
		{
			m_display = value;
		}
	}

	internal HoriAlignment HoriAlignment
	{
		get
		{
			return m_horizontalAlignment;
		}
		set
		{
			m_horizontalAlignment = value;
		}
	}
}
