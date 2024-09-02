namespace DocGen.Pdf;

public class PdfMergeOptions
{
	private bool m_optimizeResources;

	private bool m_extendMargin;

	private bool m_mergeAccessibilityTags;

	public bool OptimizeResources
	{
		get
		{
			return m_optimizeResources;
		}
		set
		{
			m_optimizeResources = value;
		}
	}

	public bool ExtendMargin
	{
		get
		{
			return m_extendMargin;
		}
		set
		{
			m_extendMargin = value;
		}
	}

	public bool MergeAccessibilityTags
	{
		get
		{
			return m_mergeAccessibilityTags;
		}
		set
		{
			m_mergeAccessibilityTags = value;
		}
	}
}
