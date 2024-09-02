using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Interactive;

public abstract class PdfSignatureAppearanceField : PdfSignatureStyledField
{
	public PdfAppearance Appearance => base.Widget.Appearance;

	protected PdfSignatureAppearanceField()
	{
	}

	protected PdfSignatureAppearanceField(PdfPageBase page, string name)
		: base(page, name)
	{
	}

	internal override void Save()
	{
		base.Save();
		if (Form != null && !Form.NeedAppearances && base.Widget.ObtainAppearance() == null)
		{
			DrawAppearance(base.Widget.Appearance.Normal);
		}
	}

	internal override void Draw()
	{
		base.Draw();
	}

	protected virtual void DrawAppearance(PdfTemplate template)
	{
	}
}
