using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Interactive;

public class PdfComboBoxField : PdfListField
{
	private bool m_editable;

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

	public bool Editable
	{
		get
		{
			return m_editable;
		}
		set
		{
			if (m_editable != value)
			{
				m_editable = value;
				if (m_editable)
				{
					Flags |= FieldFlags.Edit;
				}
				else
				{
					Flags &= FieldFlags.Edit;
				}
			}
			NotifyPropertyChanged("Editable");
		}
	}

	public PdfComboBoxField(PdfPageBase page, string name)
		: base(page, name)
	{
	}

	internal PdfComboBoxField()
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
		string s = string.Empty;
		if (base.SelectedIndex != -1)
		{
			s = base.SelectedItem.Text;
		}
		FieldPainter.DrawComboBox(pdfTemplate.Graphics, paintParams);
		PointF empty = PointF.Empty;
		float borderWidth = paintParams.BorderWidth;
		float num = 2f * borderWidth;
		int num2;
		if (paintParams.BorderStyle != PdfBorderStyle.Inset)
		{
			num2 = ((paintParams.BorderStyle == PdfBorderStyle.Beveled) ? 1 : 0);
			if (num2 == 0)
			{
				empty.X = num;
				empty.Y = 1f * borderWidth;
				goto IL_0128;
			}
		}
		else
		{
			num2 = 1;
		}
		empty.X = 2f * num;
		empty.Y = 2f * borderWidth;
		goto IL_0128;
		IL_0128:
		_ = paintParams.ForeBrush;
		float num3 = paintParams.Bounds.Width - num;
		RectangleF bounds2 = paintParams.Bounds;
		if (num2 != 0)
		{
			bounds2.Height -= num;
		}
		else
		{
			bounds2.Height -= borderWidth;
		}
		RectangleF layoutRectangle = new RectangleF(empty.X, empty.Y, num3 - empty.X, bounds2.Height);
		pdfTemplate.Graphics.DrawString(s, pdfFont, base.ForeBrush, layoutRectangle, base.StringFormat);
		RectangleF bounds3 = Bounds;
		Page.Graphics.DrawPdfTemplate(pdfTemplate, bounds3.Location, bounds3.Size);
	}

	protected override void Initialize()
	{
		base.Initialize();
		Flags |= FieldFlags.Combo;
	}

	protected override void DrawAppearance(PdfTemplate template)
	{
		base.DrawAppearance(template);
		PaintParams paintParams = new PaintParams(new RectangleF(PointF.Empty, base.Size), base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		if (base.SelectedIndex != -1)
		{
			PdfFont font = ((base.Font == null) ? new PdfStandardFont(PdfFontFamily.TimesRoman, GetFontHeight(PdfFontFamily.Helvetica)) : base.Font);
			FieldPainter.DrawComboBox(template.Graphics, paintParams, base.SelectedItem.Text, font, base.StringFormat);
		}
		else
		{
			FieldPainter.DrawComboBox(template.Graphics, paintParams);
		}
	}

	internal float GetFontHeight(PdfFontFamily family)
	{
		float num = 0f;
		float num2 = 0f;
		if (base.SelectedIndex != -1)
		{
			float width = new PdfStandardFont(family, 12f).MeasureString(base.SelectedValue).Width;
			num2 = ((width == 0f) ? 12f : (12f * (Bounds.Size.Width - 4f * base.BorderWidth) / width));
			if (base.SelectedIndex != -1)
			{
				PdfFont pdfFont = new PdfStandardFont(family, num2);
				string selectedValue = base.SelectedValue;
				SizeF sizeF = pdfFont.MeasureString(selectedValue);
				if (sizeF.Width > Bounds.Width || sizeF.Height > Bounds.Height)
				{
					float num3 = Bounds.Width - 4f * base.BorderWidth;
					float num4 = Bounds.Height - 4f * base.BorderWidth;
					float num5 = 0.248f;
					for (float num6 = 1f; num6 <= Bounds.Height; num6 += 1f)
					{
						pdfFont.Size = num6;
						SizeF sizeF2 = pdfFont.MeasureString(selectedValue);
						if (!(sizeF2.Width > Bounds.Width) && !(sizeF2.Height > num4))
						{
							continue;
						}
						num = num6;
						do
						{
							num = (pdfFont.Size = num - 0.001f);
							float lineWidth = pdfFont.GetLineWidth(selectedValue, base.StringFormat);
							if (num < num5)
							{
								pdfFont.Size = num5;
								break;
							}
							sizeF2 = pdfFont.MeasureString(selectedValue, base.StringFormat);
							if (lineWidth < num3 && sizeF2.Height < num4)
							{
								pdfFont.Size = num;
								break;
							}
						}
						while (num > num5);
						num2 = num;
						break;
					}
				}
			}
			else if (num2 > 12f)
			{
				num2 = 12f;
			}
		}
		return num2;
	}
}
