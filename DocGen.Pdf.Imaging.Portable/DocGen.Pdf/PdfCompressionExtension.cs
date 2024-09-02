using DocGen.Pdf.Parsing;

namespace DocGen.Pdf;

public static class PdfCompressionExtension
{
	public static void Compress(this PdfLoadedDocument ldoc, PdfCompressionOptions options)
	{
		if (options != null && (options.CompressImages || options.OptimizeFont || options.OptimizePageContents || options.RemoveMetadata))
		{
			ldoc.isCompressPdf = true;
			new PdfOptimizer(ldoc, options).Close();
		}
	}
}
