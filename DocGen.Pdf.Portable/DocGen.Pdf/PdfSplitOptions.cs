namespace DocGen.Pdf;

public class PdfSplitOptions
{
	private bool m_splitTags;

	private bool m_removeUnusedResources;

	public bool SplitTags
	{
		get
		{
			return m_splitTags;
		}
		set
		{
			m_splitTags = value;
		}
	}

	public bool RemoveUnusedResources
	{
		get
		{
			return m_removeUnusedResources;
		}
		set
		{
			m_removeUnusedResources = value;
		}
	}
}
