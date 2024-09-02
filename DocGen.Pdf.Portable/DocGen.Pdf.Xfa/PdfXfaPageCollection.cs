using System.Collections.Generic;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaPageCollection
{
	internal List<PdfXfaPage> m_pages = new List<PdfXfaPage>();

	internal PdfXfaDocument m_parent;

	public PdfXfaPage this[int index] => m_pages[index];

	public PdfXfaPage Add()
	{
		PdfXfaPage pdfXfaPage = new PdfXfaPage();
		pdfXfaPage.pageId = m_parent.Pages.m_pages.Count + 1;
		SetPageSettings(pdfXfaPage);
		m_pages.Add(pdfXfaPage);
		return pdfXfaPage;
	}

	private void SetPageSettings(PdfXfaPage page)
	{
		if (m_parent != null)
		{
			page.pageSettings.Margins = (PdfMargins)m_parent.PageSettings.Margins.Clone();
			page.pageSettings.PageOrientation = m_parent.PageSettings.PageOrientation;
			page.pageSettings.PageSize = m_parent.PageSettings.PageSize;
		}
	}

	internal object Clone()
	{
		return MemberwiseClone();
	}
}
