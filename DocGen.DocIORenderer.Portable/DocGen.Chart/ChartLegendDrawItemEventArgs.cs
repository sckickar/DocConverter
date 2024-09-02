using System;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartLegendDrawItemEventArgs : EventArgs
{
	private Graphics graphics;

	private bool handled;

	private int index = -1;

	private ChartLegendItem legendItem;

	private Rectangle m_bounds;

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

	public int Index => index;

	public ChartLegendItem LegendItem => legendItem;

	public Point Location
	{
		get
		{
			return m_bounds.Location;
		}
		set
		{
			m_bounds.Location = value;
		}
	}

	public Size Size
	{
		get
		{
			return m_bounds.Size;
		}
		set
		{
			m_bounds.Size = value;
		}
	}

	public Rectangle Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	public ChartLegendDrawItemEventArgs(Graphics g, ChartLegendItem item, Point loc, int index)
	{
		graphics = g;
		legendItem = item;
		m_bounds = new Rectangle(loc, Size.Empty);
		this.index = index;
	}

	public ChartLegendDrawItemEventArgs(Graphics g, ChartLegendItem item, Rectangle bounds, int index)
	{
		graphics = g;
		legendItem = item;
		m_bounds = bounds;
		this.index = index;
	}
}
