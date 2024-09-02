namespace DocGen.Pdf.Graphics;

internal class PdfMetafileLayoutFormat : PdfLayoutFormat
{
	private bool m_splitLines;

	private bool m_useImageResolution;

	private bool m_splitImages;

	private bool m_htmlPageBreak;

	internal bool m_enableDirectLayout;

	public bool SplitTextLines
	{
		get
		{
			return m_splitLines;
		}
		set
		{
			m_splitLines = value;
		}
	}

	public bool UseImageResolution
	{
		get
		{
			return m_useImageResolution;
		}
		set
		{
			m_useImageResolution = value;
		}
	}

	public bool SplitImages
	{
		get
		{
			return m_splitImages;
		}
		set
		{
			m_splitImages = value;
		}
	}

	public bool IsHTMLPageBreak
	{
		get
		{
			return m_htmlPageBreak;
		}
		set
		{
			m_htmlPageBreak = value;
		}
	}
}
