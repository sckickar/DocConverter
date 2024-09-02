using DocGen.Pdf.IO;

namespace DocGen.Pdf.Graphics;

public abstract class PdfBrush : ICloneable
{
	internal abstract bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace);

	internal abstract bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased, bool indexed);

	internal abstract bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check);

	internal abstract bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased);

	internal abstract void ResetChanges(PdfStreamWriter streamWriter);

	object ICloneable.Clone()
	{
		return Clone();
	}

	public abstract PdfBrush Clone();
}
