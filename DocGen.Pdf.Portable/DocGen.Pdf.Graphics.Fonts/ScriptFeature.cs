namespace DocGen.Pdf.Graphics.Fonts;

internal class ScriptFeature
{
	private string m_name;

	private bool m_isComplete;

	private int m_mask;

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

	internal bool IsComplete
	{
		get
		{
			return m_isComplete;
		}
		set
		{
			m_isComplete = value;
		}
	}

	internal int Mask
	{
		get
		{
			return m_mask;
		}
		set
		{
			m_mask = value;
		}
	}

	internal ScriptFeature(string name, bool complete, int mask)
	{
		Name = name;
		IsComplete = complete;
		Mask = mask;
	}
}
