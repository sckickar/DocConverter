using DocGen.Drawing;

namespace DocGen.Pdf.Interactive;

public class PdfActionAnnotation : PdfActionLinkAnnotation
{
	public PdfActionAnnotation(RectangleF rectangle, PdfAction action)
		: base(rectangle, action)
	{
	}

	protected override void Save()
	{
		base.Save();
		base.Dictionary.SetProperty("A", Action);
	}
}
