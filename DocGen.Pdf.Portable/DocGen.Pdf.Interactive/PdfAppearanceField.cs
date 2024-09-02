using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Interactive;

public abstract class PdfAppearanceField : PdfStyledField
{
	public PdfAppearance Appearance => base.Widget.Appearance;

	protected PdfAppearanceField()
	{
	}

	protected PdfAppearanceField(PdfPageBase page, string name)
		: base(page, name)
	{
	}

	internal override void Save()
	{
		base.Save();
		if (Page != null && Page.FormFieldsTabOrder == PdfFormFieldsTabOrder.Manual && Page is PdfPage)
		{
			PdfPage pdfPage = Page as PdfPage;
			if (this != null)
			{
				pdfPage.Annotations.Remove(Widget);
				pdfPage.Annotations.Insert(base.TabIndex, Widget);
			}
		}
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
