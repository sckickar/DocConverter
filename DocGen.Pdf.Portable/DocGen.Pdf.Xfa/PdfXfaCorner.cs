using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaCorner
{
	private bool m_isInverted;

	private float m_thickness;

	private float m_radius;

	private PdfXfaBorderStyle m_borderStyle;

	private PdfColor m_borderColor = new PdfColor(Color.Black);

	private PdfXfaVisibility m_visibility;

	private PdfXfaCornerShape m_shape;

	public PdfXfaCornerShape Shape
	{
		get
		{
			return m_shape;
		}
		set
		{
			m_shape = value;
		}
	}

	public bool IsInverted
	{
		get
		{
			return m_isInverted;
		}
		set
		{
			m_isInverted = value;
		}
	}

	public PdfColor BorderColor
	{
		get
		{
			return m_borderColor;
		}
		set
		{
			m_borderColor = value;
		}
	}

	public PdfXfaVisibility Visibility
	{
		get
		{
			return m_visibility;
		}
		set
		{
			m_visibility = value;
		}
	}

	public float Thickness
	{
		get
		{
			return m_thickness;
		}
		set
		{
			m_thickness = value;
		}
	}

	public float Radius
	{
		get
		{
			return m_radius;
		}
		set
		{
			m_radius = value;
		}
	}

	public PdfXfaBorderStyle BorderStyle
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
}
