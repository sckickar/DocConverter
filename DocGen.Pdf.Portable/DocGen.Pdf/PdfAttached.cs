namespace DocGen.Pdf;

public class PdfAttached
{
	private bool m_top;

	private bool m_left;

	private bool m_bottom;

	private bool m_right;

	internal bool Top => m_top;

	internal bool Left => m_left;

	internal bool Bottom => m_bottom;

	internal bool Right => m_right;

	public PdfAttached(PdfEdge pageEdge)
	{
		SetEdge(new PdfEdge[1] { pageEdge });
	}

	public PdfAttached(PdfEdge edge1, PdfEdge edge2)
	{
		SetEdge(new PdfEdge[2] { edge1, edge2 });
	}

	public PdfAttached(PdfEdge edge1, PdfEdge edge2, PdfEdge edge3, PdfEdge edge4)
	{
		SetEdge(new PdfEdge[4] { edge1, edge2, edge3, edge4 });
	}

	public void SetEdge(PdfEdge[] edges)
	{
		for (int i = 0; i < edges.Length; i++)
		{
			switch (edges[i])
			{
			case PdfEdge.Top:
				m_top = true;
				break;
			case PdfEdge.Left:
				m_left = true;
				break;
			case PdfEdge.Bottom:
				m_bottom = true;
				break;
			case PdfEdge.Right:
				m_right = true;
				break;
			}
		}
	}
}
