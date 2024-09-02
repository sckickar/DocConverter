using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaTextElement : PdfXfaField
{
	private string m_text = string.Empty;

	private PdfXfaRotateAngle m_rotate;

	private PdfFont m_font;

	private PdfColor m_foreColor;

	private float m_width;

	private float m_height;

	private PdfXfaHorizontalAlignment m_hAlign;

	private PdfXfaVerticalAlignment m_vAlign;

	internal PdfXfaForm parent;

	public PdfXfaHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return m_hAlign;
		}
		set
		{
			m_hAlign = value;
		}
	}

	public PdfXfaVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_vAlign;
		}
		set
		{
			m_vAlign = value;
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
			m_text = value;
		}
	}

	public PdfXfaRotateAngle Rotate
	{
		get
		{
			return m_rotate;
		}
		set
		{
			m_rotate = value;
		}
	}

	public PdfFont Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}

	public PdfColor ForeColor
	{
		get
		{
			return m_foreColor;
		}
		set
		{
			if (value != PdfColor.Empty)
			{
				m_foreColor = value;
			}
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	public float Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	public PdfXfaTextElement()
	{
		base.Name = "";
		Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
	}

	public PdfXfaTextElement(string text)
	{
		Text = text;
		Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
	}

	public PdfXfaTextElement(string text, float width, float height)
	{
		Text = text;
		Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		Width = width;
		Height = height;
	}

	public PdfXfaTextElement(string text, PdfFont font)
	{
		m_font = font;
		Text = text;
		base.Name = "";
	}

	public PdfXfaTextElement(string text, PdfFont font, float width, float height)
	{
		m_font = font;
		Text = text;
		base.Name = "";
		Width = width;
		Height = height;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		SizeF sizeF = Font.MeasureString(m_text);
		if (Height <= 0f)
		{
			Height = sizeF.Height;
		}
		if (Width <= 0f)
		{
			Width = sizeF.Width;
		}
		if (base.Name == "" || base.Name == string.Empty)
		{
			base.Name = "StaticText" + xfaWriter.m_fieldCount++;
		}
		xfaWriter.Write.WriteStartElement("draw");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		xfaWriter.SetRPR(Rotate, base.Visibility, isReadOnly: false);
		xfaWriter.SetSize(Height, Width, 0f, 0f);
		Dictionary<string, string> values = new Dictionary<string, string>();
		xfaWriter.WriteUI("textEdit", values, null);
		xfaWriter.WriteValue(m_text, 0);
		xfaWriter.WriteFontInfo(Font, ForeColor);
		xfaWriter.WriteMargins(base.Margins);
		xfaWriter.WritePragraph(m_vAlign, m_hAlign);
		xfaWriter.Write.WriteEndElement();
	}

	internal void SaveAcroForm(PdfPage page, RectangleF bounds)
	{
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.LineAlignment = (PdfVerticalAlignment)VerticalAlignment;
		pdfStringFormat.Alignment = ConvertToPdfTextAlignment(HorizontalAlignment);
		RectangleF rectangleF = default(RectangleF);
		PdfBrush brush = PdfBrushes.Black;
		if (ForeColor != PdfColor.Empty && (ForeColor.Red != 0f || ForeColor.Green != 0f || ForeColor.Blue != 0f))
		{
			brush = new PdfSolidBrush(ForeColor);
		}
		SizeF size = GetSize();
		rectangleF = new RectangleF(new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top), new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom)));
		PdfGraphics graphics = page.Graphics;
		graphics.Save();
		_ = SizeF.Empty;
		if (Font != null)
		{
			Font.MeasureString(Text);
		}
		graphics.TranslateTransform(rectangleF.X, rectangleF.Y);
		graphics.RotateTransform(-GetRotationAngle());
		RectangleF layoutRectangle = RectangleF.Empty;
		switch (GetRotationAngle())
		{
		case 180:
			layoutRectangle = new RectangleF(0f - rectangleF.Width, 0f - rectangleF.Height, rectangleF.Width, rectangleF.Height);
			break;
		case 90:
			layoutRectangle = new RectangleF(0f - rectangleF.Height, 0f, rectangleF.Height, rectangleF.Width);
			break;
		case 270:
			layoutRectangle = new RectangleF(0f, 0f - rectangleF.Width, rectangleF.Height, rectangleF.Width);
			break;
		case 0:
			layoutRectangle = new RectangleF(0f, 0f, rectangleF.Width, rectangleF.Height);
			break;
		}
		graphics.DrawString(Text, Font, brush, layoutRectangle, pdfStringFormat);
		graphics.Restore();
	}

	private int GetRotationAngle()
	{
		int result = 0;
		if (Rotate != 0)
		{
			switch (Rotate)
			{
			case PdfXfaRotateAngle.RotateAngle180:
				result = 180;
				break;
			case PdfXfaRotateAngle.RotateAngle270:
				result = 270;
				break;
			case PdfXfaRotateAngle.RotateAngle90:
				result = 90;
				break;
			}
		}
		return result;
	}

	internal SizeF GetSize()
	{
		if (Rotate == PdfXfaRotateAngle.RotateAngle270 || Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			return new SizeF(Height, Width);
		}
		return new SizeF(Width, Height);
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
