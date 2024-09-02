using DocGen.DocIO.ODF.Base;

namespace DocGen.DocIO.ODFConverter.Base.ODFImplementation;

internal class TabStops
{
	private TextAlign m_textAlignType;

	private double m_textPosition;

	private TabStopLeader m_tabLeader;

	internal double TextPosition
	{
		get
		{
			return m_textPosition;
		}
		set
		{
			m_textPosition = value;
		}
	}

	internal TextAlign TextAlignType
	{
		get
		{
			return m_textAlignType;
		}
		set
		{
			m_textAlignType = value;
		}
	}

	internal TabStopLeader TabStopLeader
	{
		get
		{
			return m_tabLeader;
		}
		set
		{
			m_tabLeader = value;
		}
	}
}
