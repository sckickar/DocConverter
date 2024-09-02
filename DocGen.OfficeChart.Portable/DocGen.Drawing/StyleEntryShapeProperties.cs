using DocGen.OfficeChart;

namespace DocGen.Drawing;

internal class StyleEntryShapeProperties
{
	private byte m_flag;

	private OfficeFillType m_shapeFillType;

	private double m_borderWeight;

	private ColorModel m_shapeFillColorModelType;

	private string m_shapeFillColorValue;

	private double m_shapeFillLumModValue = -1.0;

	private double m_shapeFillLumOffValue1 = -1.0;

	private double m_shapeFillLumOffValue2 = -1.0;

	private ColorModel m_borderFillColorModelType;

	private string m_borderFillColorValue;

	private double m_borderFillLumModValue = -1.0;

	private double m_borderFillLumOffValue1 = -1.0;

	private double m_borderFillLumOffValue2 = -1.0;

	private Excel2007ShapeLineStyle m_borderLineStyle;

	private bool m_borderIsRound;

	private EndLineCap m_lineCap;

	private bool m_isInsetPenAlignment;

	internal OfficeFillType ShapeFillType
	{
		get
		{
			return m_shapeFillType;
		}
		set
		{
			m_shapeFillType = value;
			m_flag |= 1;
		}
	}

	internal double BorderWeight
	{
		get
		{
			return m_borderWeight;
		}
		set
		{
			m_borderWeight = value;
			m_flag |= 4;
			m_flag |= 2;
		}
	}

	internal ColorModel ShapeFillColorModelType
	{
		get
		{
			return m_shapeFillColorModelType;
		}
		set
		{
			m_shapeFillColorModelType = value;
			if (value != 0)
			{
				ShapeFillType = OfficeFillType.SolidColor;
			}
		}
	}

	internal string ShapeFillColorValue
	{
		get
		{
			return m_shapeFillColorValue;
		}
		set
		{
			m_shapeFillColorValue = value;
		}
	}

	internal double ShapeFillLumModValue
	{
		get
		{
			return m_shapeFillLumModValue;
		}
		set
		{
			m_shapeFillLumModValue = value;
		}
	}

	internal double ShapeFillLumOffValue1
	{
		get
		{
			return m_shapeFillLumOffValue1;
		}
		set
		{
			m_shapeFillLumOffValue1 = value;
		}
	}

	internal double ShapeFillLumOffValue2
	{
		get
		{
			return m_shapeFillLumOffValue2;
		}
		set
		{
			m_shapeFillLumOffValue2 = value;
		}
	}

	internal ColorModel BorderFillColorModelType
	{
		get
		{
			return m_borderFillColorModelType;
		}
		set
		{
			m_borderFillColorModelType = value;
			m_flag |= 2;
		}
	}

	internal string BorderFillColorValue
	{
		get
		{
			return m_borderFillColorValue;
		}
		set
		{
			m_borderFillColorValue = value;
			m_flag |= 2;
		}
	}

	internal double BorderFillLumModValue
	{
		get
		{
			return m_borderFillLumModValue;
		}
		set
		{
			m_borderFillLumModValue = value;
		}
	}

	internal double BorderFillLumOffValue1
	{
		get
		{
			return m_borderFillLumOffValue1;
		}
		set
		{
			m_borderFillLumOffValue1 = value;
		}
	}

	internal double BorderFillLumOffValue2
	{
		get
		{
			return m_borderFillLumOffValue2;
		}
		set
		{
			m_borderFillLumOffValue2 = value;
		}
	}

	internal Excel2007ShapeLineStyle BorderLineStyle
	{
		get
		{
			return m_borderLineStyle;
		}
		set
		{
			m_borderLineStyle = value;
			m_flag |= 16;
			m_flag |= 2;
		}
	}

	internal bool BorderIsRound
	{
		get
		{
			return m_borderIsRound;
		}
		set
		{
			m_borderIsRound = value;
		}
	}

	internal EndLineCap LineCap
	{
		get
		{
			return m_lineCap;
		}
		set
		{
			m_lineCap = value;
			m_flag |= 8;
			m_flag |= 2;
		}
	}

	internal bool IsInsetPenAlignment
	{
		get
		{
			return m_isInsetPenAlignment;
		}
		set
		{
			m_isInsetPenAlignment = value;
			m_flag |= 32;
			m_flag |= 2;
		}
	}

	internal byte FlagOptions => m_flag;

	internal StyleEntryShapeProperties()
	{
	}
}
