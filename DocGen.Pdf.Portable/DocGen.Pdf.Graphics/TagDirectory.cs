namespace DocGen.Pdf.Graphics;

internal class TagDirectory
{
	private string m_name;

	private TagDirectory m_parent;

	internal string Name => m_name;

	internal TagDirectory Parent
	{
		get
		{
			return m_parent;
		}
		set
		{
			m_parent = value;
		}
	}

	internal TagDirectory(string name)
	{
		m_name = name;
	}
}
