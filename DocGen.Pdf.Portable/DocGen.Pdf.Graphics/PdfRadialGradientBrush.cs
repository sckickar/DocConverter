using System;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public class PdfRadialGradientBrush : PdfGradientBrush
{
	private PointF m_pointStart;

	private float m_radiusStart;

	private PointF m_pointEnd;

	private float m_radiusEnd;

	private PdfColor[] m_colours;

	private PdfColorBlend m_colourBlend;

	private PdfBlend m_blend;

	private RectangleF m_boundaries;

	public PdfBlend Blend
	{
		get
		{
			return m_blend;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Blend");
			}
			if (m_colours == null)
			{
				throw new NotSupportedException("There is no starting and ending colours specified.");
			}
			m_blend = value;
			m_colourBlend = m_blend.GenerateColorBlend(m_colours, base.ColorSpace);
			ResetFunction();
		}
	}

	public PdfColorBlend InterpolationColors
	{
		get
		{
			return m_colourBlend;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("InterpolationColors");
			}
			m_blend = null;
			m_colours = null;
			m_colourBlend = value;
			ResetFunction();
		}
	}

	public PdfColor[] LinearColors
	{
		get
		{
			return m_colours;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("LinearColors");
			}
			if (value.Length < 2)
			{
				throw new ArgumentException("The array is too small", "LinearColors");
			}
			if (m_colours == null)
			{
				m_colours = new PdfColor[2]
				{
					value[0],
					value[1]
				};
			}
			else
			{
				m_colours[0] = value[0];
				m_colours[1] = value[1];
			}
			if (m_blend == null)
			{
				m_colourBlend = new PdfColorBlend(2);
				m_colourBlend.Colors = m_colours;
				m_colourBlend.Positions = new float[2] { 0f, 1f };
			}
			else
			{
				m_colourBlend = m_blend.GenerateColorBlend(m_colours, base.ColorSpace);
			}
			ResetFunction();
		}
	}

	public RectangleF Rectangle
	{
		get
		{
			return m_boundaries;
		}
		set
		{
			m_boundaries = value;
			base.BBox = PdfArray.FromRectangle(value);
		}
	}

	public PdfExtend Extend
	{
		get
		{
			PdfExtend pdfExtend = PdfExtend.None;
			if (base.Shading["Extend"] is PdfArray pdfArray)
			{
				PdfBoolean obj = pdfArray[0] as PdfBoolean;
				PdfBoolean pdfBoolean = pdfArray[1] as PdfBoolean;
				if (obj.Value)
				{
					pdfExtend |= PdfExtend.Start;
				}
				if (pdfBoolean.Value)
				{
					pdfExtend |= PdfExtend.End;
				}
			}
			return pdfExtend;
		}
		set
		{
			PdfBoolean pdfBoolean;
			PdfBoolean pdfBoolean2;
			if (!(base.Shading["Extend"] is PdfArray pdfArray))
			{
				pdfBoolean = new PdfBoolean(value: false);
				pdfBoolean2 = new PdfBoolean(value: false);
				PdfArray pdfArray2 = new PdfArray();
				pdfArray2.Add(pdfBoolean);
				pdfArray2.Add(pdfBoolean2);
				base.Shading["Extend"] = pdfArray2;
			}
			else
			{
				pdfBoolean = pdfArray[0] as PdfBoolean;
				pdfBoolean2 = pdfArray[1] as PdfBoolean;
			}
			pdfBoolean.Value = (value & PdfExtend.Start) > PdfExtend.None;
			pdfBoolean2.Value = (value & PdfExtend.End) > PdfExtend.None;
		}
	}

	public PdfRadialGradientBrush(PointF centreStart, float radiusStart, PointF centreEnd, float radiusEnd, PdfColor colorStart, PdfColor colorEnd)
		: this(colorStart, colorEnd)
	{
		if (radiusStart < 0f)
		{
			throw new ArgumentOutOfRangeException("radiusStart", "The radius can't be less then zero.");
		}
		if (radiusEnd < 0f)
		{
			throw new ArgumentOutOfRangeException("radiusEnd", "The radius can't be less then zero.");
		}
		m_pointEnd = centreEnd;
		m_pointStart = centreStart;
		m_radiusStart = radiusStart;
		m_radiusEnd = radiusEnd;
		SetPoints(m_pointStart, m_pointEnd, m_radiusStart, m_radiusEnd);
	}

	private PdfRadialGradientBrush(PdfColor color1, PdfColor color2)
		: base(new PdfDictionary())
	{
		m_colours = new PdfColor[2] { color1, color2 };
		m_colourBlend = new PdfColorBlend(2);
		m_colourBlend.Positions = new float[2] { 0f, 1f };
		m_colourBlend.Colors = m_colours;
		InitShading();
	}

	private void SetPoints(PointF pointStart, PointF pointEnd, float radiusStart, float radiusEnd)
	{
		PdfArray pdfArray = new PdfArray();
		pdfArray.Add(new PdfNumber(pointStart.X));
		pdfArray.Add(new PdfNumber(PdfGraphics.UpdateY(pointStart.Y)));
		pdfArray.Add(new PdfNumber(radiusStart));
		pdfArray.Add(new PdfNumber(pointEnd.X));
		pdfArray.Add(new PdfNumber(PdfGraphics.UpdateY(pointEnd.Y)));
		if (radiusStart != radiusEnd)
		{
			pdfArray.Add(new PdfNumber(radiusEnd));
		}
		else
		{
			pdfArray.Add(new PdfNumber(0));
		}
		base.Shading["Coords"] = pdfArray;
	}

	private void InitShading()
	{
		base.ColorSpace = base.ColorSpace;
		base.Function = m_colourBlend.GetFunction(base.ColorSpace);
		base.Shading["ShadingType"] = new PdfNumber(3);
	}

	public override PdfBrush Clone()
	{
		PdfRadialGradientBrush pdfRadialGradientBrush = MemberwiseClone() as PdfRadialGradientBrush;
		pdfRadialGradientBrush.ResetPatternDictionary(new PdfDictionary(base.PatternDictionary));
		pdfRadialGradientBrush.Shading = new PdfDictionary();
		pdfRadialGradientBrush.InitShading();
		pdfRadialGradientBrush.SetPoints(m_pointStart, m_pointEnd, m_radiusStart, m_radiusEnd);
		if (base.Matrix != null)
		{
			pdfRadialGradientBrush.Matrix = base.Matrix.Clone();
		}
		if (m_colours != null)
		{
			pdfRadialGradientBrush.m_colours = m_colours.Clone() as PdfColor[];
		}
		if (Blend != null)
		{
			pdfRadialGradientBrush.Blend = Blend.ClonePdfBlend();
		}
		else if (InterpolationColors != null)
		{
			pdfRadialGradientBrush.InterpolationColors = InterpolationColors.CloneColorBlend();
		}
		pdfRadialGradientBrush.Extend = Extend;
		CloneBackgroundValue(pdfRadialGradientBrush);
		CloneAntiAliasingValue(pdfRadialGradientBrush);
		return pdfRadialGradientBrush;
	}

	internal override void ResetFunction()
	{
		base.Function = m_colourBlend.GetFunction(base.ColorSpace);
	}
}
