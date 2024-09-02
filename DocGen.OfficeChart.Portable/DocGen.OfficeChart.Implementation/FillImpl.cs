using System;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class FillImpl
{
	private ChartColor m_color = new ChartColor(OfficeKnownColors.BlackCustom);

	private ChartColor m_patternColor = new ChartColor((OfficeKnownColors)65);

	private OfficePattern m_pattern;

	private OfficeGradientStyle m_gradientStyle;

	private OfficeGradientVariants m_gradientVariant;

	private OfficeFillType m_fillType;

	public ChartColor ColorObject => m_color;

	public ChartColor PatternColorObject => m_patternColor;

	public OfficePattern Pattern
	{
		get
		{
			return m_pattern;
		}
		set
		{
			m_pattern = value;
		}
	}

	public OfficeGradientStyle GradientStyle
	{
		get
		{
			return m_gradientStyle;
		}
		set
		{
			m_gradientStyle = value;
		}
	}

	public OfficeGradientVariants GradientVariant
	{
		get
		{
			return m_gradientVariant;
		}
		set
		{
			m_gradientVariant = value;
		}
	}

	public OfficeFillType FillType
	{
		get
		{
			return m_fillType;
		}
		set
		{
			m_fillType = value;
		}
	}

	public FillImpl()
	{
	}

	public FillImpl(ExtendedFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		IGradient gradient = format.Gradient;
		if (gradient != null)
		{
			m_gradientStyle = gradient.GradientStyle;
			m_gradientVariant = gradient.GradientVariant;
			m_color = gradient.BackColorObject;
			m_patternColor = gradient.ForeColorObject;
		}
		else
		{
			m_color = format.ColorObject;
			m_patternColor = format.PatternColorObject;
		}
		m_pattern = format.FillPattern;
	}

	public FillImpl(OfficePattern pattern, Color color, Color patternColor)
	{
		m_pattern = pattern;
		if (pattern != 0)
		{
			m_color.SetRGB(color);
		}
		if (pattern != OfficePattern.Solid)
		{
			m_patternColor.SetRGB(patternColor);
		}
		m_fillType = ((pattern != OfficePattern.Solid) ? OfficeFillType.Pattern : OfficeFillType.SolidColor);
	}

	public FillImpl(OfficePattern pattern, ChartColor color, ChartColor patternColor)
	{
		m_pattern = pattern;
		if (pattern != 0)
		{
			m_color = color;
		}
		if (pattern != OfficePattern.Solid)
		{
			m_patternColor = patternColor;
		}
		m_fillType = ((pattern != OfficePattern.Solid) ? OfficeFillType.Pattern : OfficeFillType.SolidColor);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is FillImpl fillImpl))
		{
			return false;
		}
		if (ColorObject == fillImpl.ColorObject && PatternColorObject == fillImpl.PatternColorObject && Pattern == fillImpl.Pattern && GradientStyle == fillImpl.GradientStyle && GradientVariant == fillImpl.GradientVariant)
		{
			return FillType == fillImpl.FillType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ColorObject.GetHashCode() ^ PatternColorObject.GetHashCode() ^ Pattern.GetHashCode() ^ GradientStyle.GetHashCode() ^ GradientVariant.GetHashCode() ^ FillType.GetHashCode();
	}

	public FillImpl Clone()
	{
		return (FillImpl)MemberwiseClone();
	}

	internal void Dispose()
	{
		m_color.Dispose();
		m_patternColor.Dispose();
		m_color = null;
		m_patternColor = null;
	}
}
