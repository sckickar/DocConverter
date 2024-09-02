using System;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public class PdfLinearGradientBrush : PdfGradientBrush
{
	private PointF m_pointStart;

	private PointF m_pointEnd;

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

	public RectangleF Rectangle => m_boundaries;

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

	public PdfLinearGradientBrush(PointF point1, PointF point2, PdfColor color1, PdfColor color2)
		: this(color1, color2)
	{
		m_pointStart = point1;
		m_pointEnd = point2;
		SetPoints(m_pointStart, m_pointEnd);
	}

	public PdfLinearGradientBrush(RectangleF rect, PdfColor color1, PdfColor color2, PdfLinearGradientMode mode)
		: this(color1, color2)
	{
		m_boundaries = rect;
		switch (mode)
		{
		case PdfLinearGradientMode.BackwardDiagonal:
			m_pointStart = new PointF(rect.Right, rect.Top);
			m_pointEnd = new PointF(rect.Left, rect.Bottom);
			break;
		case PdfLinearGradientMode.ForwardDiagonal:
			m_pointStart = new PointF(rect.Left, rect.Top);
			m_pointEnd = new PointF(rect.Right, rect.Bottom);
			break;
		case PdfLinearGradientMode.Horizontal:
			m_pointStart = new PointF(rect.Left, rect.Top);
			m_pointEnd = new PointF(rect.Right, rect.Top);
			break;
		case PdfLinearGradientMode.Vertical:
			m_pointStart = new PointF(rect.Left, rect.Top);
			m_pointEnd = new PointF(rect.Left, rect.Bottom);
			break;
		default:
			throw new ArgumentException("Unsupported linear gradient mode: " + mode, "mode");
		}
		SetPoints(m_pointStart, m_pointEnd);
	}

	public PdfLinearGradientBrush(RectangleF rect, PdfColor color1, PdfColor color2, float angle)
		: this(color1, color2)
	{
		m_boundaries = rect;
		angle %= 360f;
		if (angle == 0f)
		{
			m_pointStart = new PointF(rect.Left, rect.Top);
			m_pointEnd = new PointF(rect.Right, rect.Top);
		}
		else if (angle == 90f)
		{
			m_pointStart = new PointF(rect.Left, rect.Top);
			m_pointEnd = new PointF(rect.Left, rect.Bottom);
		}
		else if (angle == 180f)
		{
			m_pointEnd = new PointF(rect.Left, rect.Top);
			m_pointStart = new PointF(rect.Right, rect.Top);
		}
		else if (angle == 270f)
		{
			m_pointEnd = new PointF(rect.Left, rect.Top);
			m_pointStart = new PointF(rect.Left, rect.Bottom);
		}
		else
		{
			double num = Math.PI / 180.0;
			double num2 = (double)angle * num;
			double num3 = Math.Tan(num2);
			float x = m_boundaries.Left + (m_boundaries.Right - m_boundaries.Left) / 2f;
			float y = m_boundaries.Top + (m_boundaries.Bottom - m_boundaries.Top) / 2f;
			PointF pointF = new PointF(x, y);
			x = m_boundaries.Width / 2f * (float)Math.Cos(num2);
			y = (float)(num3 * (double)x);
			x += pointF.X;
			y += pointF.Y;
			PointF pointF2 = SubPoints(new PointF(x, y), pointF);
			float num4 = MulPoints(SubPoints(ChoosePoint(angle), pointF), pointF2) / MulPoints(pointF2, pointF2);
			m_pointEnd = AddPoints(pointF, MulPoint(pointF2, num4));
			m_pointStart = AddPoints(pointF, MulPoint(pointF2, 0f - num4));
		}
		SetPoints(m_pointEnd, m_pointStart);
	}

	private PdfLinearGradientBrush(PdfColor color1, PdfColor color2)
		: base(new PdfDictionary())
	{
		m_colours = new PdfColor[2] { color1, color2 };
		m_colourBlend = new PdfColorBlend(2);
		m_colourBlend.Positions = new float[2] { 0f, 1f };
		m_colourBlend.Colors = m_colours;
		InitShading();
	}

	private static PointF AddPoints(PointF point1, PointF point2)
	{
		float x = point1.X + point2.X;
		float y = point1.Y + point2.Y;
		return new PointF(x, y);
	}

	private static PointF SubPoints(PointF point1, PointF point2)
	{
		float x = point1.X - point2.X;
		float y = point1.Y - point2.Y;
		return new PointF(x, y);
	}

	private static float MulPoints(PointF point1, PointF point2)
	{
		return point1.X * point2.X + point1.Y * point2.Y;
	}

	private static PointF MulPoint(PointF point, float value)
	{
		point.X *= value;
		point.Y *= value;
		return point;
	}

	private PointF ChoosePoint(float angle)
	{
		PointF empty = PointF.Empty;
		if (angle < 90f && angle > 0f)
		{
			empty = new PointF(m_boundaries.Right, m_boundaries.Bottom);
		}
		else if (angle < 180f && angle > 90f)
		{
			empty = new PointF(m_boundaries.Left, m_boundaries.Bottom);
		}
		else if (angle < 270f && angle > 180f)
		{
			empty = new PointF(m_boundaries.Left, m_boundaries.Top);
		}
		else
		{
			if (!(angle > 270f))
			{
				throw new PdfException("Internal error.");
			}
			empty = new PointF(m_boundaries.Right, m_boundaries.Top);
		}
		return empty;
	}

	private void SetPoints(PointF point1, PointF point2)
	{
		PdfArray pdfArray = new PdfArray();
		pdfArray.Add(new PdfNumber(point1.X));
		pdfArray.Add(new PdfNumber(PdfGraphics.UpdateY(point1.Y)));
		pdfArray.Add(new PdfNumber(point2.X));
		pdfArray.Add(new PdfNumber(PdfGraphics.UpdateY(point2.Y)));
		base.Shading["Coords"] = pdfArray;
	}

	private void InitShading()
	{
		base.ColorSpace = base.ColorSpace;
		base.Function = m_colourBlend.GetFunction(base.ColorSpace);
		base.Shading["ShadingType"] = new PdfNumber(2);
	}

	public override PdfBrush Clone()
	{
		PdfLinearGradientBrush pdfLinearGradientBrush = MemberwiseClone() as PdfLinearGradientBrush;
		pdfLinearGradientBrush.ResetPatternDictionary(new PdfDictionary(base.PatternDictionary));
		pdfLinearGradientBrush.Shading = new PdfDictionary();
		pdfLinearGradientBrush.InitShading();
		pdfLinearGradientBrush.SetPoints(pdfLinearGradientBrush.m_pointStart, pdfLinearGradientBrush.m_pointEnd);
		if (base.Matrix != null)
		{
			pdfLinearGradientBrush.Matrix = base.Matrix.Clone();
		}
		if (m_colours != null)
		{
			pdfLinearGradientBrush.m_colours = m_colours.Clone() as PdfColor[];
		}
		if (Blend != null)
		{
			pdfLinearGradientBrush.Blend = Blend.ClonePdfBlend();
		}
		else if (InterpolationColors != null)
		{
			pdfLinearGradientBrush.InterpolationColors = InterpolationColors.CloneColorBlend();
		}
		pdfLinearGradientBrush.Extend = Extend;
		CloneBackgroundValue(pdfLinearGradientBrush);
		CloneAntiAliasingValue(pdfLinearGradientBrush);
		return pdfLinearGradientBrush;
	}

	internal override void ResetFunction()
	{
		base.Function = m_colourBlend.GetFunction(base.ColorSpace);
	}
}
