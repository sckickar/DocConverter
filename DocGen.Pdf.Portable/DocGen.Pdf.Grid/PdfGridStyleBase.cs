using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public abstract class PdfGridStyleBase : ICloneable
{
	private PdfBrush m_backgroundBrush;

	private PdfBrush m_textBrush;

	private PdfPen m_textPen;

	private PdfFont m_font;

	private PdfPaddings m_gridCellpadding;

	public PdfBrush BackgroundBrush
	{
		get
		{
			return m_backgroundBrush;
		}
		set
		{
			m_backgroundBrush = value;
		}
	}

	public PdfBrush TextBrush
	{
		get
		{
			return m_textBrush;
		}
		set
		{
			m_textBrush = value;
		}
	}

	public PdfPen TextPen
	{
		get
		{
			return m_textPen;
		}
		set
		{
			m_textPen = value;
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

	internal PdfPaddings GridCellPadding
	{
		get
		{
			return m_gridCellpadding;
		}
		set
		{
			m_gridCellpadding = value;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
