using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfTextBoxField : PdfAppearanceField
{
	private const string m_passwordValue = "*";

	private string m_text = string.Empty;

	private string m_defaultValue = string.Empty;

	private bool m_spellCheck;

	private bool m_insertSpaces;

	private bool m_multiline;

	private bool m_password;

	private bool m_scrollable = true;

	private int m_maxLength;

	private bool m_autoResizeText;

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
				base.Dictionary.SetString("V", m_text);
				NotifyPropertyChanged("Text");
			}
		}
	}

	public string DefaultValue
	{
		get
		{
			return m_defaultValue;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("DefaultValue");
			}
			if (m_defaultValue != value)
			{
				m_defaultValue = value;
				base.Dictionary.SetString("DV", m_defaultValue);
				NotifyPropertyChanged("DefaultValue");
			}
		}
	}

	public bool SpellCheck
	{
		get
		{
			return m_spellCheck;
		}
		set
		{
			if (m_spellCheck != value)
			{
				m_spellCheck = value;
				if (m_spellCheck)
				{
					Flags &= ~FieldFlags.DoNotSpellCheck;
				}
				else
				{
					Flags |= FieldFlags.DoNotSpellCheck;
				}
			}
			NotifyPropertyChanged("SpellCheck");
		}
	}

	public bool InsertSpaces
	{
		get
		{
			m_insertSpaces = (FieldFlags.Comb & Flags) != 0 && (Flags & FieldFlags.Multiline) == 0 && (Flags & FieldFlags.Password) == 0 && (Flags & FieldFlags.FileSelect) == 0;
			return m_insertSpaces;
		}
		set
		{
			if (m_insertSpaces != value)
			{
				m_insertSpaces = value;
				if (m_insertSpaces)
				{
					Flags |= FieldFlags.Comb;
				}
				else
				{
					Flags &= ~FieldFlags.Comb;
				}
			}
			NotifyPropertyChanged("InsertSpaces");
		}
	}

	public bool Multiline
	{
		get
		{
			return m_multiline;
		}
		set
		{
			if (m_multiline != value)
			{
				m_multiline = value;
				if (m_multiline)
				{
					Flags |= FieldFlags.Multiline;
					base.StringFormat.LineAlignment = PdfVerticalAlignment.Top;
				}
				else
				{
					Flags &= ~FieldFlags.Multiline;
					base.StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
				}
			}
			NotifyPropertyChanged("Multiline");
		}
	}

	public bool Password
	{
		get
		{
			return m_password;
		}
		set
		{
			if (m_password != value)
			{
				m_password = value;
				if (m_password)
				{
					Flags |= FieldFlags.Password;
				}
				else
				{
					Flags &= ~FieldFlags.Password;
				}
			}
			NotifyPropertyChanged("Password");
		}
	}

	public bool Scrollable
	{
		get
		{
			return m_scrollable;
		}
		set
		{
			if (m_scrollable != value)
			{
				m_scrollable = value;
				if (m_scrollable)
				{
					Flags &= ~FieldFlags.DoNotScroll;
				}
				else
				{
					Flags |= FieldFlags.DoNotScroll;
				}
			}
			NotifyPropertyChanged("Scrollable");
		}
	}

	public int MaxLength
	{
		get
		{
			return m_maxLength;
		}
		set
		{
			if (m_maxLength != value)
			{
				m_maxLength = value;
				base.Dictionary.SetNumber("MaxLen", m_maxLength);
				NotifyPropertyChanged("MaxLength");
			}
		}
	}

	public bool AutoResizeText
	{
		get
		{
			return m_autoResizeText;
		}
		set
		{
			m_autoResizeText = value;
			if (m_widget != null)
			{
				m_widget.isAutoResize = value;
			}
			NotifyPropertyChanged("AutoResizeText");
		}
	}

	public PdfTextBoxField(PdfPageBase page, string name)
		: base(page, name)
	{
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.None || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_X1A2001)
		{
			base.Font = PdfDocument.DefaultFont;
		}
	}

	internal PdfTextBoxField()
	{
	}

	internal override void Draw()
	{
		if (fieldItems != null && fieldItems.Count > 0)
		{
			foreach (PdfTextBoxField fieldItem in fieldItems)
			{
				fieldItem.Text = Text;
				fieldItem.Draw();
			}
		}
		base.Draw();
		if (base.Widget.ObtainAppearance() != null)
		{
			Page.Graphics.DrawPdfTemplate(base.Appearance.Normal, base.Location);
			return;
		}
		PaintParams paintParams = new PaintParams(Bounds, base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		if (AutoResizeText && base.Flatten)
		{
			SetFittingFontSize(paintParams, Text);
		}
		FieldPainter.DrawTextBox(Page.Graphics, paintParams, Text, base.Font, base.StringFormat, Multiline, Scrollable);
	}

	protected override void Initialize()
	{
		base.Initialize();
		Flags |= FieldFlags.DoNotSpellCheck;
		base.Dictionary.SetProperty("FT", new PdfName("Tx"));
	}

	protected override void DrawAppearance(PdfTemplate template)
	{
		if (fieldItems != null && fieldItems.Count > 0)
		{
			foreach (PdfTextBoxField fieldItem in fieldItems)
			{
				fieldItem.Text = Text;
				fieldItem.DrawAppearance(fieldItem.Widget.Appearance.Normal);
			}
		}
		base.DrawAppearance(template);
		PaintParams paintParams = new PaintParams(new RectangleF(PointF.Empty, base.Size), base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		paintParams.InsertSpace = InsertSpaces;
		string text = Text;
		paintParams.IsRequired = Required;
		if (Password)
		{
			text = string.Empty;
			for (int i = 0; i < Text.Length; i++)
			{
				text += "*";
			}
		}
		template.m_writeTransformation = false;
		PdfGraphics graphics = template.Graphics;
		if (!Required)
		{
			graphics.StreamWriter.BeginMarkupSequence("Tx");
			graphics.InitializeCoordinates();
		}
		if (AutoResizeText)
		{
			SetFittingFontSize(paintParams, text);
		}
		FieldPainter.DrawTextBox(graphics, paintParams, text, ObtainFont(), base.StringFormat, Multiline, Scrollable, MaxLength);
		if (!Required)
		{
			graphics.StreamWriter.EndMarkupSequence();
		}
	}

	private void SetFittingFontSize(PaintParams prms, string text)
	{
		float num = 0f;
		float num2 = 0f;
		num2 = ((prms.BorderStyle != PdfBorderStyle.Beveled && prms.BorderStyle != PdfBorderStyle.Inset) ? (Bounds.Width - 4f * prms.BorderWidth) : (Bounds.Width - 8f * prms.BorderWidth));
		float num3 = Bounds.Height - 2f * base.BorderWidth;
		float num4 = 0.248f;
		PdfFont pdfFont = null;
		pdfFont = ((!(base.Font is PdfStandardFont)) ? ((PdfFont)(base.Font as PdfTrueTypeFont)) : ((PdfFont)(base.Font as PdfStandardFont)));
		if (text.EndsWith(" "))
		{
			base.StringFormat.MeasureTrailingSpaces = true;
		}
		for (float num5 = 0f; num5 <= Bounds.Height; num5 += 1f)
		{
			if (base.Font is PdfStandardFont)
			{
				base.Font.Size = num5;
			}
			else
			{
				base.Font.Size = num5;
			}
			SizeF sizeF = base.Font.MeasureString(text, base.StringFormat);
			if (text == null || (!(sizeF.Width > Bounds.Width) && !(sizeF.Height > num3)))
			{
				continue;
			}
			num = num5;
			do
			{
				num = (pdfFont.Size = num - 0.001f);
				float lineWidth = base.Font.GetLineWidth(text, base.StringFormat);
				if (num < num4)
				{
					base.Font.Size = num4;
					break;
				}
				sizeF = base.Font.MeasureString(text, base.StringFormat);
				if (lineWidth < num2 && sizeF.Height < num3)
				{
					base.Font.Size = num;
					break;
				}
			}
			while (num > num4);
			break;
		}
	}
}
