using DocGen.Pdf.IO;

namespace DocGen.Pdf.Primitives;

internal interface IPdfPrimitive
{
	ObjectStatus Status { get; set; }

	bool IsSaving { get; set; }

	int ObjectCollectionIndex { get; set; }

	IPdfPrimitive ClonedObject { get; }

	int Position { get; set; }

	void Save(IPdfWriter writer);

	IPdfPrimitive Clone(PdfCrossTable crossTable);
}
