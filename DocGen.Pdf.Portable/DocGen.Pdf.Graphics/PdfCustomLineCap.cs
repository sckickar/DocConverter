using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal class PdfCustomLineCap
{
	private PdfPath m_fillPath;

	private PdfPath m_strokePath;

	private PdfLineCap m_baseCap;

	private PdfLineJoin m_strokeJoin;

	private float m_widthScale = 1f;

	private PdfLineCap m_startCap;

	private PdfLineCap m_endCap;

	private float m_baseInset;

	public PdfLineCap BaseCap
	{
		get
		{
			return m_baseCap;
		}
		set
		{
			m_baseCap = value;
		}
	}

	public float BaseInset
	{
		get
		{
			return m_baseInset;
		}
		set
		{
			m_baseInset = value;
		}
	}

	public PdfLineJoin StrokeJoin
	{
		get
		{
			return m_strokeJoin;
		}
		set
		{
			m_strokeJoin = value;
		}
	}

	public float WidthScale
	{
		get
		{
			return m_widthScale;
		}
		set
		{
			m_widthScale = value;
		}
	}

	public PdfCustomLineCap(PdfPath fillPath, PdfPath strokePath)
	{
		m_fillPath = fillPath;
		m_strokePath = strokePath;
	}

	public PdfCustomLineCap(PdfPath fillPath, PdfPath strokePath, PdfLineCap baseCap)
	{
		m_fillPath = fillPath;
		m_strokePath = strokePath;
		m_baseCap = baseCap;
	}

	public PdfCustomLineCap(PdfPath fillPath, PdfPath strokePath, PdfLineCap baseCap, float baseInset)
	{
		m_fillPath = fillPath;
		m_strokePath = strokePath;
		m_baseCap = baseCap;
		m_baseInset = baseInset;
	}

	internal PdfCustomLineCap(PdfPath pdfPath, PdfLineCap baseCap, float baseInset, bool isFillPath)
	{
		if (isFillPath)
		{
			m_fillPath = pdfPath;
		}
		else
		{
			m_strokePath = pdfPath;
		}
		m_baseCap = baseCap;
		m_baseInset = baseInset;
	}

	public void SetStrokeCaps(PdfLineCap startCap, PdfLineCap endCap)
	{
		m_startCap = startCap;
		m_endCap = endCap;
	}

	internal void DrawCustomCap(PdfGraphics graphics, PointF[] points, PdfPen pen, bool isStartCap)
	{
		PointF[] array = null;
		if (m_fillPath != null)
		{
			array = m_fillPath.PathPoints;
		}
		else if (m_strokePath != null)
		{
			array = m_strokePath.PathPoints;
		}
		if (array != null)
		{
			PdfGraphicsState state = graphics.Save();
			PointF empty = PointF.Empty;
			PointF empty2 = PointF.Empty;
			if (isStartCap)
			{
				empty = points[1];
				empty2 = points[0];
			}
			else
			{
				empty = points[^2];
				empty2 = points[^1];
			}
			float num = empty.X - empty2.X;
			float num2 = empty.Y - empty2.Y;
			float num3 = (float)Math.Sqrt(num * num + num2 * num2);
			_ = num / num2;
			float num4 = (float)Math.Atan(num2 / num);
			if (num / num3 < 0f)
			{
				num4 += (float)Math.PI;
			}
			graphics.TranslateTransform(empty2.X, empty2.Y);
			graphics.RotateTransform((float)((double)num4 * 180.0 / Math.PI) + 90f);
			float num5 = pen.Width / 2f;
			if (m_fillPath != null)
			{
				m_fillPath.Scale(num5, num5);
				graphics.DrawPath(new PdfSolidBrush(pen.Color), m_fillPath);
			}
			if (m_strokePath != null)
			{
				m_strokePath.Scale(num5, num5);
				PdfPen pdfPen = new PdfPen(new PdfSolidBrush(pen.Color), pen.Width);
				pdfPen.LineCap = pen.LineCap;
				pdfPen.LineJoin = pen.LineJoin;
				pdfPen.EndCap = pen.EndCap;
				pdfPen.StartCap = pen.StartCap;
				graphics.DrawPath(pdfPen, m_strokePath);
			}
			graphics.Restore(state);
		}
	}
}
