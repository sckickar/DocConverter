using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Interactive;

public class PdfListBoxField : PdfListField
{
	private bool m_multiselect;

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

	public bool MultiSelect
	{
		get
		{
			return m_multiselect;
		}
		set
		{
			if (m_multiselect != value)
			{
				m_multiselect = value;
				if (m_multiselect)
				{
					Flags |= FieldFlags.MultiSelect;
				}
				else
				{
					Flags -= 2097152;
				}
			}
			NotifyPropertyChanged("MultiSelect");
		}
	}

	public PdfListBoxField(PdfPageBase page, string name)
		: base(page, name)
	{
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
		if (base.SelectedIndexes.Length > 1)
		{
			FieldPainter.DrawListBox(pdfTemplate.Graphics, paintParams, base.Items, base.SelectedIndexes, pdfFont, base.StringFormat);
		}
		else
		{
			FieldPainter.DrawListBox(pdfTemplate.Graphics, paintParams, base.Items, new int[1] { base.SelectedIndex }, pdfFont, base.StringFormat);
		}
		Page.Graphics.DrawPdfTemplate(pdfTemplate, Bounds.Location, bounds.Size);
	}

	protected override void Initialize()
	{
		base.Initialize();
	}

	protected override void DrawAppearance(PdfTemplate template)
	{
		base.DrawAppearance(template);
		PaintParams paintParams = new PaintParams(new RectangleF(PointF.Empty, base.Size), base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		if (!m_isBCSet)
		{
			paintParams.BackBrush = new PdfSolidBrush(PdfColor.Empty);
		}
		PdfFont pdfFont = base.Font;
		if (pdfFont == null)
		{
			pdfFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12f);
		}
		paintParams.IsRequired = Required;
		template.Graphics.StreamWriter.Clear();
		if (!Required)
		{
			template.Graphics.StreamWriter.BeginMarkupSequence("Tx");
			template.Graphics.InitializeCoordinates();
		}
		if (base.SelectedIndexes.Length > 1)
		{
			FieldPainter.DrawListBox(template.Graphics, paintParams, base.Items, base.SelectedIndexes, pdfFont, base.StringFormat);
		}
		else
		{
			FieldPainter.DrawListBox(template.Graphics, paintParams, base.Items, new int[1] { base.SelectedIndex }, pdfFont, base.StringFormat);
		}
		if (!Required)
		{
			template.Graphics.StreamWriter.EndMarkupSequence();
		}
	}
}
