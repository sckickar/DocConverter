using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfXfaDateTimeField : PdfXfaStyledField
{
	private DateTime m_value;

	private PdfXfaDatePattern m_datePatterns;

	private PdfXfaTimePattern m_timePatterns;

	private PdfXfaDateTimeFormat m_format;

	private bool m_requireValidation;

	internal bool isSet;

	private PdfXfaCaption m_caption = new PdfXfaCaption();

	internal new PdfXfaForm parent;

	private PdfPaddings m_padding = new PdfPaddings(0f, 0f, 0f, 0f);

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

	public DateTime Value
	{
		get
		{
			return m_value;
		}
		set
		{
			isSet = true;
			m_value = value;
		}
	}

	public PdfXfaDatePattern DatePattern
	{
		get
		{
			return m_datePatterns;
		}
		set
		{
			m_datePatterns = value;
		}
	}

	public PdfXfaDateTimeFormat Format
	{
		get
		{
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}

	public PdfXfaTimePattern TimePattern
	{
		get
		{
			return m_timePatterns;
		}
		set
		{
			m_timePatterns = value;
		}
	}

	public bool RequireValidation
	{
		get
		{
			return m_requireValidation;
		}
		set
		{
			m_requireValidation = value;
		}
	}

	public PdfXfaDateTimeField(string name, SizeF size)
	{
		base.Width = size.Width;
		base.Height = size.Height;
		base.Name = name;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaDateTimeField(string name, float width, float height)
	{
		base.Width = width;
		base.Height = height;
		base.Name = name;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		if (base.Name == "" || base.Name == string.Empty)
		{
			base.Name = "dateTimeField" + xfaWriter.m_fieldCount;
		}
		xfaWriter.Write.WriteStartElement("field");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		SetSize(xfaWriter);
		xfaWriter.SetRPR(base.Rotate, base.Visibility, base.ReadOnly);
		xfaWriter.Write.WriteStartElement("ui");
		xfaWriter.Write.WriteStartElement("dateTimeEdit");
		xfaWriter.Write.WriteStartElement("pictures");
		xfaWriter.Write.WriteString(GetDatePattern());
		xfaWriter.Write.WriteEndElement();
		xfaWriter.DrawBorder(base.Border);
		xfaWriter.WriteMargins(m_padding.Left, m_padding.Right, m_padding.Bottom, m_padding.Top);
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteEndElement();
		SetMFTP(xfaWriter);
		if (Caption != null)
		{
			Caption.Save(xfaWriter);
		}
		xfaWriter.Write.WriteStartElement("value");
		switch (Format)
		{
		case PdfXfaDateTimeFormat.Date:
			xfaWriter.Write.WriteStartElement("date");
			if (isSet)
			{
				xfaWriter.Write.WriteString(Value.ToString(xfaWriter.GetDatePattern(DatePattern)));
			}
			xfaWriter.Write.WriteEndElement();
			xfaWriter.Write.WriteEndElement();
			xfaWriter.WritePattern(GetDatePattern(), RequireValidation);
			break;
		case PdfXfaDateTimeFormat.Time:
			xfaWriter.Write.WriteStartElement("time");
			if (isSet)
			{
				xfaWriter.Write.WriteString(Value.ToString(xfaWriter.GetTimePattern(TimePattern)));
			}
			xfaWriter.Write.WriteEndElement();
			xfaWriter.Write.WriteEndElement();
			xfaWriter.WritePattern(GetDatePattern(), RequireValidation);
			break;
		case PdfXfaDateTimeFormat.DateTime:
			xfaWriter.Write.WriteStartElement("dateTime");
			if (isSet)
			{
				xfaWriter.Write.WriteString(Value.ToString(xfaWriter.GetDateTimePattern(DatePattern, TimePattern)));
			}
			xfaWriter.Write.WriteEndElement();
			xfaWriter.Write.WriteEndElement();
			break;
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
		pdfTextBoxField.StringFormat.Alignment = ConvertToPdfTextAlignment(base.HorizontalAlignment);
		if (base.Font == null)
		{
			pdfTextBoxField.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 11f, PdfFontStyle.Regular);
		}
		else
		{
			pdfTextBoxField.Font = base.Font;
		}
		if (base.Border != null)
		{
			base.Border.ApplyAcroBorder(pdfTextBoxField);
		}
		if (base.ReadOnly || parent.ReadOnly)
		{
			pdfTextBoxField.ReadOnly = true;
		}
		if (base.Visibility == PdfXfaVisibility.Invisible)
		{
			pdfTextBoxField.Visibility = PdfFormFieldVisibility.Hidden;
		}
		if (isSet)
		{
			if (Format == PdfXfaDateTimeFormat.Date)
			{
				pdfTextBoxField.Text = Value.ToString(GetDatePattern(DatePattern));
			}
			else if (Format == PdfXfaDateTimeFormat.DateTime)
			{
				pdfTextBoxField.Text = Value.ToString(GetDateTimePattern(DatePattern, TimePattern));
			}
			else if (Format == PdfXfaDateTimeFormat.Time)
			{
				pdfTextBoxField.Text = Value.ToString(GetTimePattern(TimePattern));
			}
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

	private string GetDatePattern(PdfXfaDatePattern pattern)
	{
		string result = string.Empty;
		switch (pattern)
		{
		case PdfXfaDatePattern.Default:
			result = "MMM d, yyyy";
			break;
		case PdfXfaDatePattern.Short:
			result = "M/d/yyyy";
			break;
		case PdfXfaDatePattern.Medium:
			result = "MMM d, yyyy";
			break;
		case PdfXfaDatePattern.Long:
			result = "MMMM d, yyyy";
			break;
		case PdfXfaDatePattern.Full:
			result = "dddd, MMMM dd, yyyy";
			break;
		case PdfXfaDatePattern.DDMMMMYYYY:
			result = "dd MMMM, yyyy";
			break;
		case PdfXfaDatePattern.DDMMMYY:
			result = "dd-MMM-yy";
			break;
		case PdfXfaDatePattern.EEEEDDMMMMYYYY:
			result = "dddd, dd MMMM, yyyy";
			break;
		case PdfXfaDatePattern.EEEE_MMMMD_YYYY:
			result = "dddd, MMMM d, yyyy";
			break;
		case PdfXfaDatePattern.EEEEMMMMDDYYYY:
			result = "dddd, MMMM dd, yyyy";
			break;
		case PdfXfaDatePattern.MDYY:
			result = "M/d/yy";
			break;
		case PdfXfaDatePattern.MDYYYY:
			result = "M/d/yyyy";
			break;
		case PdfXfaDatePattern.MMDDYY:
			result = "MM/dd/yy";
			break;
		case PdfXfaDatePattern.MMDDYYYY:
			result = "MM/dd/yyyy";
			break;
		case PdfXfaDatePattern.MMMD_YYYY:
			result = "MMM d, yyyy";
			break;
		case PdfXfaDatePattern.MMMMD_YYYY:
			result = "MMMM d, yyyy";
			break;
		case PdfXfaDatePattern.MMMMDDYYYY:
			result = "MMMM dd, yyyy}";
			break;
		case PdfXfaDatePattern.MMMMYYYY:
			result = "MMMM, yyyy";
			break;
		case PdfXfaDatePattern.YYMMDD:
			result = "yy/MM/dd";
			break;
		case PdfXfaDatePattern.YYYYMMDD:
			result = "yyyy-MM-dd";
			break;
		}
		return result;
	}

	private string GetTimePattern(PdfXfaTimePattern pattern)
	{
		string result = string.Empty;
		switch (pattern)
		{
		case PdfXfaTimePattern.Default:
			result = "h:mm:ss";
			break;
		case PdfXfaTimePattern.Short:
			result = "t";
			break;
		case PdfXfaTimePattern.Medium:
			result = "h:mm:ss";
			break;
		case PdfXfaTimePattern.Long:
			result = "T";
			break;
		case PdfXfaTimePattern.Full:
			result = "hh:mm:ss tt zzz";
			break;
		case PdfXfaTimePattern.H_MM_A:
			result = "h:MM tt";
			break;
		case PdfXfaTimePattern.H_MM_SS:
			result = "H:MM:ss";
			break;
		case PdfXfaTimePattern.H_MM_SS_A:
			result = "H:MM:ss tt";
			break;
		case PdfXfaTimePattern.H_MM_SS_A_Z:
			result = "H:MM:ss tt z";
			break;
		case PdfXfaTimePattern.HH_MM_SS:
			result = "HH:MM:ss";
			break;
		case PdfXfaTimePattern.HH_MM_SS_A:
			result = "hh:MM:ss tt";
			break;
		}
		return result;
	}

	private string GetDateTimePattern(PdfXfaDatePattern d, PdfXfaTimePattern t)
	{
		return GetDatePattern(d) + " " + GetTimePattern(t);
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
			xfaWriter.SetSize(base.Height, base.Width, 0f, 0f);
			Caption.Width = sizeF.Height;
		}
		else if (Caption.Position == PdfXfaPosition.Left || Caption.Position == PdfXfaPosition.Right)
		{
			xfaWriter.SetSize(base.Height, base.Width, 0f, 0f);
			Caption.Width = sizeF.Width;
		}
	}

	private string GetDatePattern()
	{
		string result = null;
		switch (Format)
		{
		case PdfXfaDateTimeFormat.Date:
			switch (DatePattern)
			{
			case PdfXfaDatePattern.Default:
				result = null;
				break;
			case PdfXfaDatePattern.Short:
				result = "date.short{}";
				break;
			case PdfXfaDatePattern.Medium:
				result = "date.medium{}";
				break;
			case PdfXfaDatePattern.Long:
				result = "date.long{}";
				break;
			case PdfXfaDatePattern.Full:
				result = "date.full{}";
				break;
			case PdfXfaDatePattern.DDMMMMYYYY:
				result = "date{DD MMMM, YYYY}";
				break;
			case PdfXfaDatePattern.DDMMMYY:
				result = "date{DD-MMM-YY}";
				break;
			case PdfXfaDatePattern.EEEEDDMMMMYYYY:
				result = "date{EEEE, DD MMMM, YYYY}";
				break;
			case PdfXfaDatePattern.EEEE_MMMMD_YYYY:
				result = "date{EEEE, MMMM D, YYYY}";
				break;
			case PdfXfaDatePattern.EEEEMMMMDDYYYY:
				result = "date{EEEE, MMMM DD, YYYY}";
				break;
			case PdfXfaDatePattern.MDYY:
				result = "date{M/D/YY}";
				break;
			case PdfXfaDatePattern.MDYYYY:
				result = "date{M/D/YYYY}";
				break;
			case PdfXfaDatePattern.MMDDYY:
				result = "date{MM/DD/YY}";
				break;
			case PdfXfaDatePattern.MMDDYYYY:
				result = "date{MM/DD/YYYY}";
				break;
			case PdfXfaDatePattern.MMMD_YYYY:
				result = "date{MMM D, YYYY}";
				break;
			case PdfXfaDatePattern.MMMMD_YYYY:
				result = "date{MMMM D, YYYY}";
				break;
			case PdfXfaDatePattern.MMMMDDYYYY:
				result = "date{MMMM DD, YYYY}";
				break;
			case PdfXfaDatePattern.MMMMYYYY:
				result = "date{MMMM, YYYY}";
				break;
			case PdfXfaDatePattern.YYMMDD:
				result = "date{YY/MM/DD}";
				break;
			case PdfXfaDatePattern.YYYYMMDD:
				result = "date{YYYY-MM-DD}";
				break;
			}
			break;
		case PdfXfaDateTimeFormat.Time:
			switch (TimePattern)
			{
			case PdfXfaTimePattern.Default:
				result = null;
				break;
			case PdfXfaTimePattern.Short:
				result = "time.short{}";
				break;
			case PdfXfaTimePattern.Medium:
				result = "time.medium{}";
				break;
			case PdfXfaTimePattern.Long:
				result = "time.long{}";
				break;
			case PdfXfaTimePattern.Full:
				result = "time.full{}";
				break;
			case PdfXfaTimePattern.H_MM_A:
				result = "time{h:MM A}";
				break;
			case PdfXfaTimePattern.H_MM_SS:
				result = "time{H:MM:SS}";
				break;
			case PdfXfaTimePattern.H_MM_SS_A:
				result = "time{H:MM:SS A}";
				break;
			case PdfXfaTimePattern.H_MM_SS_A_Z:
				result = "time{H:MM:SS A Z}";
				break;
			case PdfXfaTimePattern.HH_MM_SS:
				result = "time{HH:MM:SS}";
				break;
			case PdfXfaTimePattern.HH_MM_SS_A:
				result = "time{hh:MM:SS A}";
				break;
			}
			break;
		}
		return result;
	}

	public object Clone()
	{
		PdfXfaDateTimeField obj = (PdfXfaDateTimeField)MemberwiseClone();
		obj.Caption = Caption.Clone() as PdfXfaCaption;
		return obj;
	}
}
