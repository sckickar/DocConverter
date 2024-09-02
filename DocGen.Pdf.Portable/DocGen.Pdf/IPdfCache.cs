using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal interface IPdfCache
{
	bool EqualsTo(IPdfCache obj);

	IPdfPrimitive GetInternals();

	void SetInternals(IPdfPrimitive internals);
}
