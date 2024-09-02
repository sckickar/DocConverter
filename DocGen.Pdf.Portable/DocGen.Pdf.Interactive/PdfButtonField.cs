using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfButtonField : PdfAppearanceField
{
	private string m_text = string.Empty;

	public new bool ComplexScript
	{
		get
		{
			return base.ComplexScript;
		}
		set
		{
			base.ComplexScript = value;
			NotifyPropertyChanged("ComplexScript");
		}
	}

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Text");
			}
			if (m_text != value)
			{
				m_text = value;
				base.Widget.WidgetAppearance.NormalCaption = m_text;
				NotifyPropertyChanged("Text");
			}
		}
	}

	public PdfButtonField(PdfPageBase page, string name)
		: base(page, name)
	{
		base.StringFormat.Alignment = PdfTextAlignment.Center;
		base.Widget.WidgetAppearance.NormalCaption = name;
		base.Widget.TextAlignment = PdfTextAlignment.Center;
	}

	internal PdfButtonField()
	{
	}

	public void AddPrintAction()
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetProperty("N", new PdfName("Print"));
		pdfDictionary.SetProperty("S", new PdfName("Named"));
		(((base.Dictionary["Kids"] as PdfArray)[0] as PdfReferenceHolder).Object as PdfDictionary).SetProperty("A", pdfDictionary);
	}

	internal override void Draw()
	{
		base.Draw();
		if (base.Widget.ObtainAppearance() != null)
		{
			Page.Graphics.DrawPdfTemplate(base.Appearance.Normal, base.Location);
			return;
		}
		RectangleF bounds = Bounds;
		bounds.Location = PointF.Empty;
		PdfFont pdfFont = base.Font;
		if (pdfFont == null)
		{
			pdfFont = PdfDocument.DefaultFont;
		}
		PaintParams paintParams = new PaintParams(bounds, base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		PdfTemplate pdfTemplate = new PdfTemplate(bounds.Size);
		FieldPainter.DrawButton(pdfTemplate.Graphics, paintParams, Text, pdfFont, base.StringFormat);
		RectangleF bounds2 = Bounds;
		Page.Graphics.DrawPdfTemplate(pdfTemplate, bounds2.Location, bounds2.Size);
	}

	internal override void Save()
	{
		base.Save();
		if (Form != null && !Form.NeedAppearances && base.Widget.Appearance.GetPressedTemplate() == null)
		{
			DrawPressedAppearance(base.Widget.Appearance.Pressed);
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("FT", new PdfName("Btn"));
		base.BackColor = new PdfColor(byte.MaxValue, 211, 211, 211);
		Flags |= FieldFlags.PushButton;
	}

	protected override void DrawAppearance(PdfTemplate template)
	{
		base.DrawAppearance(template);
		PaintParams paintParams = new PaintParams(new RectangleF(PointF.Empty, base.Size), base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		FieldPainter.DrawButton(template.Graphics, paintParams, Text, ObtainFont(), base.StringFormat);
	}

	protected void DrawPressedAppearance(PdfTemplate template)
	{
		PaintParams paintParams = new PaintParams(new RectangleF(PointF.Empty, base.Size), base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		FieldPainter.DrawPressedButton(template.Graphics, paintParams, Text, ObtainFont(), base.StringFormat);
	}
}
