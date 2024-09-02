using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfXfaTextBoxField : PdfXfaStyledField, ICloneable
{
	private float m_minimumHeight;

	private float m_minimumWidth;

	private float m_maxHeight;

	private float m_maxWidth;

	private string m_text = string.Empty;

	private int m_maxLength;

	private int m_combLength;

	private char m_passwordChar;

	private PdfPaddings m_padding = new PdfPaddings(0f, 0f, 0f, 0f);

	private PdfXfaTextBoxType m_type;

	private PdfXfaCaption m_caption = new PdfXfaCaption();

	internal new PdfXfaForm parent;

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

	public PdfXfaTextBoxType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
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

	public float MinimumHeight
	{
		get
		{
			return m_minimumHeight;
		}
		set
		{
			m_minimumHeight = value;
		}
	}

	public float MinimumWidth
	{
		get
		{
			return m_minimumWidth;
		}
		set
		{
			m_minimumWidth = value;
		}
	}

	public float MaximumHeight
	{
		get
		{
			return m_maxHeight;
		}
		set
		{
			m_maxHeight = value;
		}
	}

	public float MaximumWidth
	{
		get
		{
			return m_maxWidth;
		}
		set
		{
			m_maxWidth = value;
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

	public int MaximumLength
	{
		get
		{
			return m_maxLength;
		}
		set
		{
			m_maxLength = value;
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

	public char PasswordCharacter
	{
		get
		{
			return m_passwordChar;
		}
		set
		{
			m_passwordChar = value;
		}
	}

	public PdfXfaTextBoxField(string fieldName, SizeF minimumSize)
	{
		MinimumHeight = minimumSize.Height;
		MinimumWidth = minimumSize.Width;
		base.Name = fieldName;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaTextBoxField(string fieldName, SizeF minimumSize, string defaultText)
	{
		MinimumHeight = minimumSize.Height;
		MinimumWidth = minimumSize.Width;
		base.Name = fieldName;
		Text = defaultText;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaTextBoxField(string fieldName, SizeF minimumSize, string defaultText, PdfXfaTextBoxType fieldType)
	{
		MinimumHeight = minimumSize.Height;
		MinimumWidth = minimumSize.Width;
		base.Name = fieldName;
		Text = defaultText;
		Type = fieldType;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaTextBoxField(string fieldName, SizeF minimumSize, PdfXfaTextBoxType fieldType)
	{
		MinimumHeight = minimumSize.Height;
		MinimumWidth = minimumSize.Width;
		base.Name = fieldName;
		Type = fieldType;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaTextBoxField(string fieldName, float minWidth, float minHeight)
	{
		MinimumHeight = minHeight;
		MinimumWidth = minWidth;
		base.Name = fieldName;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaTextBoxField(string fieldName, float minWidth, float minHeight, string defaultText)
	{
		MinimumHeight = minHeight;
		MinimumWidth = minWidth;
		base.Name = fieldName;
		Text = defaultText;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaTextBoxField(string fieldName, float minWidth, float minHeight, PdfXfaTextBoxType fieldType)
	{
		MinimumHeight = minHeight;
		MinimumWidth = minWidth;
		base.Name = fieldName;
		Type = fieldType;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaTextBoxField(string fieldName, float minWidth, float minHeight, string defaultText, PdfXfaTextBoxType fieldType)
	{
		MinimumHeight = minHeight;
		MinimumWidth = minWidth;
		base.Name = fieldName;
		Text = defaultText;
		Type = fieldType;
		base.HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	internal void Save(XfaWriter xfaWriter, PdfXfaType type)
	{
		if (base.Name == "" || base.Name == string.Empty)
		{
			base.Name = "textBox" + xfaWriter.m_fieldCount;
		}
		xfaWriter.Write.WriteStartElement("field");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		if (Type == PdfXfaTextBoxType.Password || Type == PdfXfaTextBoxType.Comb)
		{
			if (base.Height <= 0f)
			{
				base.Height = MinimumHeight;
			}
			if (base.Width <= 0f)
			{
				base.Width = MinimumWidth;
			}
		}
		SetSize(xfaWriter, type);
		xfaWriter.SetRPR(base.Rotate, base.Visibility, base.ReadOnly);
		if (Type == PdfXfaTextBoxType.Password)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("passwordChar", PasswordCharacter.ToString());
			xfaWriter.WriteUI("passwordEdit", dictionary, base.Border, 0, Padding);
		}
		else if (Type == PdfXfaTextBoxType.Comb)
		{
			if (CombLength > 0)
			{
				xfaWriter.WriteUI("textEdit", null, base.Border, CombLength, Padding);
			}
			else
			{
				int comb = 0;
				if (Text != null && Text != string.Empty)
				{
					comb = Text.Length;
				}
				xfaWriter.WriteUI("textEdit", null, base.Border, comb, Padding);
			}
		}
		else if (Type == PdfXfaTextBoxType.Multiline)
		{
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2.Add("multiLine", "1");
			xfaWriter.WriteUI("textEdit", dictionary2, base.Border, Padding);
		}
		else
		{
			xfaWriter.WriteUI("textEdit", null, base.Border, Padding);
		}
		if (Text != null && Text != string.Empty)
		{
			xfaWriter.WriteValue(Text, m_maxLength);
		}
		else if (m_maxLength > 0)
		{
			xfaWriter.WriteValue("", m_maxLength);
		}
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

	internal void Save(XfaWriter xfaWriter)
	{
		Save(xfaWriter, PdfXfaType.Dynamic);
	}

	internal PdfField SaveAcroForm(PdfPage page, RectangleF bounds, string name)
	{
		PdfTextBoxField pdfTextBoxField = new PdfTextBoxField(page, name);
		switch (Type)
		{
		case PdfXfaTextBoxType.Password:
			pdfTextBoxField.Password = true;
			break;
		case PdfXfaTextBoxType.Multiline:
			pdfTextBoxField.Multiline = true;
			break;
		}
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
		if (MaximumLength > 0)
		{
			pdfTextBoxField.MaxLength = MaximumLength;
		}
		SizeF size = GetSize();
		pdfTextBoxField.StringFormat.LineAlignment = (PdfVerticalAlignment)base.VerticalAlignment;
		pdfTextBoxField.TextAlignment = ConvertToPdfTextAlignment(base.HorizontalAlignment);
		if (Text != string.Empty)
		{
			pdfTextBoxField.Text = Text;
		}
		if (base.Visibility == PdfXfaVisibility.Invisible)
		{
			pdfTextBoxField.Visibility = PdfFormFieldVisibility.Hidden;
		}
		RectangleF bounds2 = default(RectangleF);
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

	private void SetSize(XfaWriter xfaWriter, PdfXfaType type)
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
		float fixedHeight = base.Height;
		float fixedWidth = base.Width;
		if (base.Height <= 0f)
		{
			fixedHeight = MinimumHeight;
		}
		if (base.Width <= 0f)
		{
			fixedWidth = MinimumWidth;
		}
		if (Caption.Position == PdfXfaPosition.Bottom || Caption.Position == PdfXfaPosition.Top)
		{
			if (type == PdfXfaType.Dynamic)
			{
				xfaWriter.SetSize(base.Height, base.Width, MinimumHeight, MinimumWidth, MaximumHeight, MaximumWidth);
			}
			else
			{
				xfaWriter.SetSize(fixedHeight, fixedWidth, 0f, 0f, 0f, 0f);
			}
			Caption.Width = sizeF.Height;
		}
		else if (Caption.Position == PdfXfaPosition.Left || Caption.Position == PdfXfaPosition.Right)
		{
			if (type == PdfXfaType.Dynamic)
			{
				xfaWriter.SetSize(base.Height, base.Width, MinimumHeight, MinimumWidth, MaximumHeight, MaximumWidth);
			}
			else
			{
				xfaWriter.SetSize(fixedHeight, fixedWidth, 0f, 0f, 0f, 0f);
			}
			Caption.Width = sizeF.Width;
		}
	}

	internal new SizeF GetSize()
	{
		SizeF result = default(SizeF);
		if (base.Height > 0f)
		{
			result.Height = base.Height;
		}
		else
		{
			result.Height = MinimumHeight;
		}
		if (base.Width > 0f)
		{
			result.Width = base.Width;
		}
		else
		{
			result.Width = MinimumWidth;
		}
		if (base.Rotate == PdfXfaRotateAngle.RotateAngle270 || base.Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			result = new SizeF(result.Height, result.Width);
		}
		return result;
	}

	public object Clone()
	{
		PdfXfaTextBoxField obj = (PdfXfaTextBoxField)MemberwiseClone();
		obj.Caption = (PdfXfaCaption)Caption.Clone();
		return obj;
	}
}
