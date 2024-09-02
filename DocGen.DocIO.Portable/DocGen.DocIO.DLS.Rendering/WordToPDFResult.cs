using System.Collections.Generic;

namespace DocGen.DocIO.DLS.Rendering;

internal class WordToPDFResult
{
	private List<PageResult> m_pages;

	public List<PageResult> Pages => m_pages;

	public WordToPDFResult()
	{
		m_pages = new List<PageResult>();
	}
}
