using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal abstract class CIEBasedColorspace : Colorspace
{
	internal abstract PdfArray WhitePoint { get; set; }

	internal abstract PdfArray BlackPoint { get; set; }
}
