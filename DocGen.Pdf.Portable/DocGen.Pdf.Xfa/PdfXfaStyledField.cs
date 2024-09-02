using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public abstract class PdfXfaStyledField : PdfXfaField
{
	private string m_toolTip = string.Empty;

	private PdfFont m_font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);

	private PdfColor m_foreColor;

	private PdfXfaHorizontalAlignment m_hAlign = PdfXfaHorizontalAlignment.Center;

	private PdfXfaVerticalAlignment m_vAlign = PdfXfaVerticalAlignment.Middle;

	private PdfXfaRotateAngle m_rotate;

	private PdfXfaBorder m_border = new PdfXfaBorder();

	private bool m_readOnly;

	private float m_width;

	private float m_height;

	internal PdfXfaForm parent;

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

	public PdfXfaBorder Border
	{
		get
		{
			return m_border;
		}
		set
		{
			if (value != null)
			{
				m_border = value;
			}
		}
	}

	public string ToolTip
	{
		get
		{
			return m_toolTip;
		}
		set
		{
			if (value != null)
			{
				m_toolTip = value;
			}
		}
	}

	public bool ReadOnly
	{
		get
		{
			return m_readOnly;
		}
		set
		{
			m_readOnly = value;
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

	internal void SetMFTP(XfaWriter xfaWriter)
	{
		xfaWriter.WriteFontInfo(Font, ForeColor);
		xfaWriter.WriteMargins(base.Margins);
		if (ToolTip != null && ToolTip != "")
		{
			xfaWriter.WriteToolTip(ToolTip);
		}
		xfaWriter.WritePragraph(VerticalAlignment, HorizontalAlignment);
	}

	internal SizeF GetSize()
	{
		if (Rotate == PdfXfaRotateAngle.RotateAngle270 || Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			return new SizeF(Height, Width);
		}
		return new SizeF(Width, Height);
	}

	internal int GetRotationAngle()
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
}
