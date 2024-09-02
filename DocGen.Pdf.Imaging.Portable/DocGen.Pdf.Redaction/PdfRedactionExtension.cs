using System.Collections.Generic;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Redaction;

public static class PdfRedactionExtension
{
	private static List<PdfRedaction> m_redactions = new List<PdfRedaction>();

	public static List<PdfRedactionResult> Redact(this PdfLoadedDocument ldoc)
	{
		List<PdfRedactionResult> list = new List<PdfRedactionResult>();
		foreach (PdfLoadedPage page in ldoc.Pages)
		{
			foreach (PdfAnnotation annotation in page.Annotations)
			{
				if (annotation is PdfLoadedRedactionAnnotation)
				{
					PdfLoadedRedactionAnnotation pdfLoadedRedactionAnnotation = annotation as PdfLoadedRedactionAnnotation;
					pdfLoadedRedactionAnnotation.Flatten = true;
					PdfTemplate appearance = pdfLoadedRedactionAnnotation.CreateNormalAppearance(pdfLoadedRedactionAnnotation.OverlayText, pdfLoadedRedactionAnnotation.Font, pdfLoadedRedactionAnnotation.RepeatText, pdfLoadedRedactionAnnotation.TextColor, pdfLoadedRedactionAnnotation.TextAlignment, pdfLoadedRedactionAnnotation.Border);
					PdfRedaction pdfRedaction = new PdfRedaction(pdfLoadedRedactionAnnotation.Bounds);
					pdfRedaction.Appearance = appearance;
					pdfRedaction.page = pdfLoadedRedactionAnnotation.Page;
					m_redactions.Add(pdfRedaction);
				}
			}
			List<PdfRedaction> redactions = page.GetRedactions();
			if (redactions.Count <= 0)
			{
				continue;
			}
			ldoc.FileStructure.IncrementalUpdate = false;
			PdfTextParserNet pdfTextParserNet = new PdfTextParserNet(page, redactions);
			if (ldoc != null && ldoc.RaiseTrackRedactionProgress)
			{
				pdfTextParserNet.redactionTrackProcess = true;
			}
			pdfTextParserNet.Process();
			foreach (PdfRedaction item in redactions)
			{
				list.Add(new PdfRedactionResult(ldoc.Pages.IndexOf(page) + 1, item.Success, item.Bounds));
			}
			ldoc.isCompressPdf = true;
		}
		m_redactions.Clear();
		return list;
	}

	public static void AddRedaction(this PdfLoadedPage page, PdfRedaction redact)
	{
		redact.page = page;
		m_redactions.Add(redact);
	}

	internal static List<PdfRedaction> GetRedactions(this PdfLoadedPage page)
	{
		List<PdfRedaction> list = new List<PdfRedaction>();
		foreach (PdfRedaction redaction in m_redactions)
		{
			if (page.Equals(redaction.page))
			{
				list.Add(redaction);
			}
		}
		return list;
	}
}
