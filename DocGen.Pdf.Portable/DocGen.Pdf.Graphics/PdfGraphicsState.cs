using System;

namespace DocGen.Pdf.Graphics;

public class PdfGraphicsState
{
	private PdfGraphics m_graphics;

	private PdfTransformationMatrix m_matrix;

	private TextRenderingMode m_textRenderingMode;

	private float m_characterSpacing;

	private float m_wordSpacing;

	private float m_textScaling = 100f;

	private PdfPen m_pen;

	private PdfBrush m_brush;

	private PdfFont m_font;

	private PdfColorSpace m_colorSpace;

	internal PdfGraphics Graphics => m_graphics;

	internal PdfTransformationMatrix Matrix => m_matrix;

	internal float CharacterSpacing
	{
		get
		{
			return m_characterSpacing;
		}
		set
		{
			m_characterSpacing = value;
		}
	}

	internal float WordSpacing
	{
		get
		{
			return m_wordSpacing;
		}
		set
		{
			m_wordSpacing = value;
		}
	}

	internal float TextScaling
	{
		get
		{
			return m_textScaling;
		}
		set
		{
			m_textScaling = value;
		}
	}

	internal PdfPen Pen
	{
		get
		{
			return m_pen;
		}
		set
		{
			m_pen = value;
		}
	}

	internal PdfBrush Brush
	{
		get
		{
			return m_brush;
		}
		set
		{
			m_brush = value;
		}
	}

	internal PdfFont Font
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

	internal PdfColorSpace ColorSpace
	{
		get
		{
			return m_colorSpace;
		}
		set
		{
			m_colorSpace = value;
		}
	}

	internal TextRenderingMode TextRenderingMode
	{
		get
		{
			return m_textRenderingMode;
		}
		set
		{
			m_textRenderingMode = value;
		}
	}

	private PdfGraphicsState()
	{
	}

	internal PdfGraphicsState(PdfGraphics graphics, PdfTransformationMatrix matrix)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		m_graphics = graphics;
		m_matrix = matrix;
	}
}
