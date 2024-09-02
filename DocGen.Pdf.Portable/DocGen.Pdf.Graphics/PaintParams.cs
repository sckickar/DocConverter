using DocGen.Drawing;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Graphics;

internal class PaintParams
{
	private PdfBrush m_backBrush;

	private PdfBrush m_foreBrush;

	private float m_borderWidth = 1f;

	private PdfPen m_borderPen;

	private PdfBorderStyle m_borderStyle;

	private RectangleF m_bounds = RectangleF.Empty;

	private PdfBrush m_shadowBrush;

	private int m_rotationAngle;

	private bool m_insertSpace;

	private PdfPageRotateAngle m_pageRotationAngle;

	private bool m_isFlatten;

	internal bool m_complexScript;

	internal PdfTextDirection m_textDirection;

	internal float m_lineSpacing;

	private bool m_isRequiredField;

	public PdfBrush BackBrush
	{
		get
		{
			return m_backBrush;
		}
		set
		{
			m_backBrush = value;
		}
	}

	public PdfBrush ForeBrush
	{
		get
		{
			return m_foreBrush;
		}
		set
		{
			m_foreBrush = value;
		}
	}

	public PdfPen BorderPen
	{
		get
		{
			return m_borderPen;
		}
		set
		{
			m_borderPen = value;
		}
	}

	public PdfBorderStyle BorderStyle
	{
		get
		{
			return m_borderStyle;
		}
		set
		{
			m_borderStyle = value;
		}
	}

	public float BorderWidth
	{
		get
		{
			return m_borderWidth;
		}
		set
		{
			m_borderWidth = value;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	public PdfBrush ShadowBrush
	{
		get
		{
			return m_shadowBrush;
		}
		set
		{
			m_shadowBrush = value;
		}
	}

	public int RotationAngle
	{
		get
		{
			return m_rotationAngle;
		}
		set
		{
			m_rotationAngle = value;
		}
	}

	internal bool InsertSpace
	{
		get
		{
			return m_insertSpace;
		}
		set
		{
			m_insertSpace = value;
		}
	}

	internal PdfPageRotateAngle PageRotationAngle
	{
		get
		{
			return m_pageRotationAngle;
		}
		set
		{
			m_pageRotationAngle = value;
		}
	}

	internal bool isFlatten
	{
		get
		{
			return m_isFlatten;
		}
		set
		{
			m_isFlatten = value;
		}
	}

	internal bool IsRequired
	{
		get
		{
			return m_isRequiredField;
		}
		set
		{
			m_isRequiredField = value;
		}
	}

	public PaintParams()
	{
	}

	public PaintParams(RectangleF bounds, PdfBrush backBrush, PdfBrush foreBrush, PdfPen borderPen, PdfBorderStyle style, float borderWidth, PdfBrush shadowBrush, int rotationAngle)
	{
		m_bounds = bounds;
		m_backBrush = backBrush;
		m_foreBrush = foreBrush;
		m_borderPen = borderPen;
		m_borderStyle = style;
		m_borderWidth = borderWidth;
		m_shadowBrush = shadowBrush;
		m_rotationAngle = rotationAngle;
	}
}
