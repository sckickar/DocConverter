using System;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.IO;

namespace DocGen.Pdf.Graphics;

internal sealed class PathGradientBrush : PdfBrush
{
	private PdfColorBlend m_interpolationColors;

	private PointF m_centerPoint;

	private GraphicsPath m_path;

	private SKPaint m_sKPaint;

	internal PdfColorBlend InterpolationColors
	{
		get
		{
			return m_interpolationColors;
		}
		set
		{
			m_interpolationColors = value;
			UpdatePathGradientBrush();
		}
	}

	internal PathGradientBrush(GraphicsPath path)
	{
		m_path = path;
		m_interpolationColors = new PdfColorBlend();
		m_centerPoint = PointF.Empty;
		m_sKPaint = new SKPaint();
		m_sKPaint.Color = new SKColor(Color.White.R, Color.White.G, Color.White.B, Color.White.A);
		m_sKPaint.IsAntialias = true;
	}

	public override PdfBrush Clone()
	{
		throw new NotImplementedException();
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace)
	{
		throw new NotImplementedException();
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased, bool indexed)
	{
		throw new NotImplementedException();
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check)
	{
		throw new NotImplementedException();
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased)
	{
		throw new NotImplementedException();
	}

	internal override void ResetChanges(PdfStreamWriter streamWriter)
	{
		throw new NotImplementedException();
	}

	private void UpdatePathGradientBrush()
	{
		SKRect bounds = m_path.Bounds;
		RectangleF rectangleF = new RectangleF(bounds.Left + 1f, bounds.Top + 1f, bounds.Width - 2f, bounds.Height - 2f);
		_ = m_centerPoint;
		if (m_centerPoint.X == 0f && m_centerPoint.Y == 0f)
		{
			m_centerPoint = new PointF(rectangleF.X + rectangleF.Width / 2f, rectangleF.Y + rectangleF.Height / 2f);
		}
		PdfColor[] colors = m_interpolationColors.Colors;
		PdfColor[] array = new PdfColor[colors.Length];
		for (int i = 0; i < colors.Length; i++)
		{
			array[i] = colors[^(i + 1)];
		}
		SKColor[] array2 = new SKColor[array.Length];
		for (int j = 0; j < array.Length; j++)
		{
			PdfColor pdfColor = array[j];
			array2[j] = new SKColor(pdfColor.R, pdfColor.G, pdfColor.B, pdfColor.A);
		}
		m_sKPaint.Shader = SKShader.CreateRadialGradient(new SKPoint(m_centerPoint.X, m_centerPoint.Y), rectangleF.Width / 2f, array2, m_interpolationColors.Positions, SKShaderTileMode.Clamp);
	}
}
