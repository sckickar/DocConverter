using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

public interface AnalyzerResult
{
	List<PdfException> Errors { get; }
}
