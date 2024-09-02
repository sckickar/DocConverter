using DocGen.Pdf.Parsing;

namespace DocGen.Pdf;

public static class PdfConformanceExtension
{
	public static void ConvertToPDFA(this PdfLoadedDocument ldoc, PdfConformanceLevel conformanceLevel)
	{
		ldoc.Conformance = conformanceLevel;
		if (ldoc.ConformanceEnabled)
		{
			if (ldoc.m_documentInfo == null)
			{
				ldoc.ReadDocumentInfo();
			}
			PdfDocument.ConformanceLevel = ldoc.m_previousConformance;
			PdfA1BConverter pdfA1BConverter = new PdfA1BConverter();
			pdfA1BConverter.PdfALevel = conformanceLevel;
			pdfA1BConverter.Convert(ldoc);
			ldoc.existingConformanceLevel = conformanceLevel;
		}
	}
}
