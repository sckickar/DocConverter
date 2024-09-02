namespace DocGen.DocIO.DLS.Convertors;

internal class TabFormat
{
	private float m_tabPosition = 36f;

	private TabJustification m_tabJustification;

	private TabLeader m_tabLeader;

	internal float TabPosition
	{
		get
		{
			return m_tabPosition;
		}
		set
		{
			m_tabPosition = value;
		}
	}

	internal TabJustification TabJustification
	{
		get
		{
			return m_tabJustification;
		}
		set
		{
			m_tabJustification = value;
		}
	}

	internal TabLeader TabLeader
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
