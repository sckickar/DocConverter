using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfPageTransition : IPdfWrapper, ICloneable
{
	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfTransitionStyle m_style = PdfTransitionStyle.Replace;

	private float m_duration = 1f;

	private PdfTransitionDimension m_dimension;

	private PdfTransitionMotion m_motion;

	private PdfTransitionDirection m_direction;

	private float m_scale = 1f;

	private float m_pageDuration;

	public PdfTransitionStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			m_style = value;
			m_dictionary.SetProperty("S", new PdfName(StyleToString(m_style)));
		}
	}

	public float Duration
	{
		get
		{
			return m_duration;
		}
		set
		{
			m_duration = value;
			m_dictionary.SetProperty("D", new PdfNumber(m_duration));
		}
	}

	public PdfTransitionDimension Dimension
	{
		get
		{
			return m_dimension;
		}
		set
		{
			m_dimension = value;
			m_dictionary.SetProperty("Dm", new PdfName(DimensionToString(m_dimension)));
		}
	}

	public PdfTransitionMotion Motion
	{
		get
		{
			return m_motion;
		}
		set
		{
			m_motion = value;
			m_dictionary.SetProperty("M", new PdfName(MotionToString(m_motion)));
		}
	}

	public PdfTransitionDirection Direction
	{
		get
		{
			return m_direction;
		}
		set
		{
			m_direction = value;
			m_dictionary.SetProperty("Di", new PdfNumber((int)m_direction));
		}
	}

	public float Scale
	{
		get
		{
			return m_scale;
		}
		set
		{
			m_scale = value;
			m_dictionary.SetProperty("SS", new PdfNumber(m_scale));
		}
	}

	public float PageDuration
	{
		get
		{
			return m_pageDuration;
		}
		set
		{
			m_pageDuration = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfPageTransition()
	{
		m_dictionary.SetProperty("Type", new PdfName("Trans"));
	}

	private string MotionToString(PdfTransitionMotion motion)
	{
		if (motion == PdfTransitionMotion.Inward || motion != PdfTransitionMotion.Outward)
		{
			return "I";
		}
		return "O";
	}

	private string DimensionToString(PdfTransitionDimension dimension)
	{
		if (dimension == PdfTransitionDimension.Horizontal || dimension != PdfTransitionDimension.Vertical)
		{
			return "H";
		}
		return "V";
	}

	private string StyleToString(PdfTransitionStyle style)
	{
		if (style == PdfTransitionStyle.Replace)
		{
			return "R";
		}
		return style.ToString();
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
