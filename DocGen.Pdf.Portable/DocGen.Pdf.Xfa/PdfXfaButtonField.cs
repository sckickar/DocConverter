using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfXfaButtonField : PdfXfaStyledField
{
	private PdfHighlightMode m_highlight;

	private string m_rolloverText;

	private string m_downText;

	private string m_content = string.Empty;

	internal new PdfXfaForm parent;

	public PdfHighlightMode Highlight
	{
		get
		{
			return m_highlight;
		}
		set
		{
			m_highlight = value;
		}
	}

	public string MouseRolloverText
	{
		get
		{
			return m_rolloverText;
		}
		set
		{
			if (value != null)
			{
				m_rolloverText = value;
			}
		}
	}

	public string MouseDownText
	{
		get
		{
			return m_downText;
		}
		set
		{
			if (value != null)
			{
				m_downText = value;
			}
		}
	}

	public string Content
	{
		get
		{
			return m_content;
		}
		set
		{
			if (value != null)
			{
				m_content = value;
			}
		}
	}

	public PdfXfaButtonField(string name, SizeF buttonSize)
	{
		base.Height = buttonSize.Height;
		base.Width = buttonSize.Width;
		base.Name = name;
	}

	public PdfXfaButtonField(string name, float width, float height)
	{
		base.Height = height;
		base.Width = width;
		base.Name = name;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		if (base.Name == "" || base.Name == string.Empty)
		{
			base.Name = "button" + xfaWriter.m_fieldCount++;
		}
		xfaWriter.Write.WriteStartElement("field");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		xfaWriter.SetSize(base.Height + base.Margins.Bottom + base.Margins.Top, base.Width + base.Margins.Right + base.Margins.Left, 0f, 0f);
		xfaWriter.SetRPR(base.Rotate, base.Visibility, base.ReadOnly);
		string value = ((Highlight == PdfHighlightMode.NoHighlighting) ? null : ((Highlight != PdfHighlightMode.Invert) ? Highlight.ToString().ToLower() : "inverted"));
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("highlight", value);
		xfaWriter.WriteUI("button", dictionary, null);
		if (base.Border != null)
		{
			if (base.Border.FillColor != null)
			{
				xfaWriter.DrawBorder(base.Border, base.Border.FillColor);
			}
			else
			{
				xfaWriter.DrawBorder(base.Border, new PdfXfaSolidBrush(new PdfColor(212, 208, 200)));
			}
		}
		SetMFTP(xfaWriter);
		if (Content != null)
		{
			xfaWriter.WriteCaption(Content, 0f, base.HorizontalAlignment, base.VerticalAlignment);
		}
		xfaWriter.WriteItems(MouseRolloverText, MouseDownText);
		xfaWriter.Write.WriteEndElement();
	}

	internal PdfField SaveAcroForm(PdfPage page, RectangleF bounds, string name)
	{
		PdfButtonField pdfButtonField = new PdfButtonField(page, name);
		if (base.Border != null)
		{
			base.Border.ApplyAcroBorder(pdfButtonField);
		}
		if (base.Border != null && base.Border.FillColor == null)
		{
			pdfButtonField.BackColor = new PdfColor(212, 208, 200);
		}
		pdfButtonField.TextAlignment = ConvertToPdfTextAlignment(base.HorizontalAlignment);
		if (base.ReadOnly || parent.ReadOnly || parent.m_isReadOnly)
		{
			pdfButtonField.ReadOnly = true;
		}
		RectangleF bounds2 = default(RectangleF);
		SizeF size = GetSize();
		bounds2.Location = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
		bounds2.Size = new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom));
		if (base.Visibility == PdfXfaVisibility.Invisible)
		{
			pdfButtonField.Visibility = PdfFormFieldVisibility.Hidden;
		}
		pdfButtonField.Text = Content;
		if (base.Font == null)
		{
			pdfButtonField.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		else
		{
			pdfButtonField.Font = base.Font;
		}
		pdfButtonField.Bounds = bounds2;
		pdfButtonField.Widget.WidgetAppearance.RotationAngle = GetRotationAngle();
		if (base.ForeColor != PdfColor.Empty)
		{
			pdfButtonField.ForeColor = base.ForeColor;
		}
		return pdfButtonField;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
