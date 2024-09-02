using System;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Pdf.Graphics;

internal class Brush : IClone, IDisposable, IBrush
{
	private SKPaint m_sKPaint;

	internal SKPaint SKPaint => m_sKPaint;

	public float StrokeWidth
	{
		get
		{
			return m_sKPaint.StrokeWidth;
		}
		set
		{
			m_sKPaint.StrokeWidth = value;
		}
	}

	public SKShader Shader
	{
		get
		{
			return m_sKPaint.Shader;
		}
		set
		{
			m_sKPaint.Shader = value;
		}
	}

	public SKFilterQuality FilterQuality
	{
		get
		{
			return m_sKPaint.FilterQuality;
		}
		set
		{
			m_sKPaint.FilterQuality = value;
		}
	}

	public Color Color
	{
		get
		{
			return Extension.GetColor(m_sKPaint.Color);
		}
		set
		{
			m_sKPaint.Color = Extension.GetSKColor(value);
		}
	}

	public Brush(Color textColor)
	{
		m_sKPaint = new SKPaint();
		m_sKPaint.Color = new SKColor(textColor.R, textColor.G, textColor.B, textColor.A);
		m_sKPaint.IsAntialias = true;
	}

	public object Clone()
	{
		Brush brush = MemberwiseClone() as Brush;
		if (m_sKPaint != null)
		{
			brush.m_sKPaint = m_sKPaint.Clone();
		}
		return brush;
	}

	public void Dispose()
	{
		if (m_sKPaint != null)
		{
			m_sKPaint.Dispose();
		}
	}
}
