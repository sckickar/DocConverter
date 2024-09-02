using System;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartLegendDrawItemTextEventArgs : EventArgs
{
	private Graphics graphics;

	private bool handled;

	private string text;

	private RectangleF m_textrect;

	public Graphics Graphics => graphics;

	public bool Handled
	{
		get
		{
			return handled;
		}
		set
		{
			handled = value;
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	public RectangleF TextRect => m_textrect;

	public ChartLegendDrawItemTextEventArgs(Graphics g, string text, RectangleF m_textRect)
	{
		graphics = g;
		this.text = text;
		m_textrect = m_textRect;
	}
}
