using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfXfaNumericField : PdfXfaStyledField
{
	private double m_value = double.NaN;

	private int m_combLength;

	private PdfXfaNumericType m_fieldType;

	private PdfPaddings m_padding = new PdfPaddings(0f, 0f, 0f, 0f);

	private PdfXfaCaption m_caption = new PdfXfaCaption();

	private string m_patternString = string.Empty;

	internal new PdfXfaForm parent;

	private string m_culture = string.Empty;

	public string Culture
	{
		get
		{
			return m_culture;
		}
		set
		{
			if (value != null)
			{
				m_culture = value;
			}
		}
	}

	public PdfXfaCaption Caption
	{
		get
		{
			return m_caption;
		}
		set
		{
			m_caption = value;
		}
	}

	public PdfPaddings Padding
	{
		get
		{
			return m_padding;
		}
		set
		{
			if (value != null)
			{
				m_padding = value;
			}
		}
	}

	public double NumericValue
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	public int CombLength
	{
		get
		{
			return m_combLength;
		}
		set
		{
			m_combLength = value;
		}
	}

	public PdfXfaNumericType FieldType
	{
		get
		{
			return m_fieldType;
		}
		set
		{
			m_fieldType = value;
		}
	}

	public string PatternString
	{
		get
		{
			return m_patternString;
		}
		set
		{
			if (value != null)
			{
				m_patternString = value;
			}
		}
	}

	public PdfXfaNumericField(string name, SizeF size)
	{
		base.Height = size.Height;
		base.Width = size.Width;
		base.Name = name;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaNumericField(string name, float width, float height)
	{
		base.Height = height;
		base.Width = width;
		base.Name = name;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		if (base.Name == "" || base.Name == string.Empty)
		{
			base.Name = "numeric" + xfaWriter.m_fieldCount;
		}
		xfaWriter.Write.WriteStartElement("field");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		SetSize(xfaWriter);
		if (Culture != null && Culture != string.Empty)
		{
			xfaWriter.Write.WriteAttributeString("locale", Culture);
		}
		xfaWriter.SetRPR(base.Rotate, base.Visibility, base.ReadOnly);
		if (CombLength > 0)
		{
			xfaWriter.WriteUI("numericEdit", null, base.Border, CombLength, Padding);
		}
		else
		{
			xfaWriter.WriteUI("numericEdit", null, base.Border, Padding);
		}
		string value = FieldType.ToString().ToLower();
		if (FieldType == PdfXfaNumericType.Currency || FieldType == PdfXfaNumericType.Percent || PatternString != string.Empty)
		{
			value = PdfXfaNumericType.Float.ToString().ToLower();
		}
		if (double.IsNaN(NumericValue))
		{
			xfaWriter.WriteValue("", value, 0);
		}
		else
		{
			if (FieldType == PdfXfaNumericType.Integer)
			{
				NumericValue = (int)NumericValue;
			}
			xfaWriter.WriteValue(NumericValue.ToString(), value, 0);
		}
		xfaWriter.Write.WriteStartElement("format");
		xfaWriter.Write.WriteStartElement("picture");
		if (m_patternString != string.Empty)
		{
			xfaWriter.Write.WriteString("num{" + m_patternString + "}");
		}
		else
		{
			xfaWriter.Write.WriteString("num." + FieldType.ToString().ToLower() + "{}");
		}
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteEndElement();
		SetMFTP(xfaWriter);
		if (Caption != null)
		{
			Caption.Save(xfaWriter);
		}
		if (parent != null && parent.m_formType == PdfXfaType.Static)
		{
			xfaWriter.Write.WriteStartElement("keep");
			xfaWriter.Write.WriteAttributeString("intact", "contentArea");
			xfaWriter.Write.WriteEndElement();
		}
		xfaWriter.Write.WriteEndElement();
	}

	internal PdfField SaveAcroForm(PdfPage page, RectangleF bounds, string name)
	{
		PdfTextBoxField pdfTextBoxField = new PdfTextBoxField(page, name);
		pdfTextBoxField.StringFormat.LineAlignment = (PdfVerticalAlignment)base.VerticalAlignment;
		pdfTextBoxField.TextAlignment = ConvertToPdfTextAlignment(base.HorizontalAlignment);
		if (base.Font == null)
		{
			pdfTextBoxField.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		else
		{
			pdfTextBoxField.Font = base.Font;
		}
		if (base.ReadOnly || parent.ReadOnly || parent.m_isReadOnly)
		{
			pdfTextBoxField.ReadOnly = true;
		}
		if (base.Border != null)
		{
			base.Border.ApplyAcroBorder(pdfTextBoxField);
		}
		if (base.ForeColor != PdfColor.Empty)
		{
			pdfTextBoxField.ForeColor = base.ForeColor;
		}
		if (base.Visibility == PdfXfaVisibility.Invisible)
		{
			pdfTextBoxField.Visibility = PdfFormFieldVisibility.Hidden;
		}
		if (!double.IsNaN(NumericValue))
		{
			pdfTextBoxField.Text = NumericValue.ToString();
		}
		RectangleF bounds2 = default(RectangleF);
		SizeF size = GetSize();
		bounds2.Location = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
		bounds2.Size = new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom));
		if (base.Visibility != PdfXfaVisibility.Invisible)
		{
			Caption.DrawText(page, bounds2, GetRotationAngle());
		}
		pdfTextBoxField.Bounds = GetBounds(bounds2, base.Rotate, Caption);
		pdfTextBoxField.Widget.WidgetAppearance.RotationAngle = GetRotationAngle();
		return pdfTextBoxField;
	}

	private void SetSize(XfaWriter xfaWriter)
	{
		SizeF sizeF = default(SizeF);
		if (Caption.Width > 0f)
		{
			float width2 = (sizeF.Height = Caption.Width);
			sizeF.Width = width2;
		}
		else
		{
			sizeF = Caption.MeasureString();
		}
		if (Caption.Position == PdfXfaPosition.Bottom || Caption.Position == PdfXfaPosition.Top)
		{
			xfaWriter.SetSize(base.Height, base.Width, 0f, 0f, 0f, 0f);
			Caption.Width = sizeF.Height;
		}
		else if (Caption.Position == PdfXfaPosition.Left || Caption.Position == PdfXfaPosition.Right)
		{
			xfaWriter.SetSize(base.Height, base.Width, 0f, 0f, 0f, 0f);
			Caption.Width = sizeF.Width;
		}
	}

	public object Clone()
	{
		PdfXfaNumericField obj = (PdfXfaNumericField)MemberwiseClone();
		obj.Caption = Caption.Clone() as PdfXfaCaption;
		return obj;
	}
}
