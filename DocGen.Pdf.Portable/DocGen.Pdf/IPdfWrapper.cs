using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal interface IPdfWrapper
{
	IPdfPrimitive Element { get; }
}
