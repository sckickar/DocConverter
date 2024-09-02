using System;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class Control : IChartDockControl
{
	private Font m_Font = SystemFonts.DefaultFont;

	public static Font DefaultFont => SystemFonts.DefaultFont;

	public Rectangle ClientRectangle { get; set; }

	public Color BackColor { get; set; }

	public Control Parent { get; set; }

	public int Height { get; set; }

	public int Width { get; set; }

	public virtual ChartDock Position { get; set; }

	public virtual ChartAlignment Alignment { get; set; }

	public ChartOrientation Orientation { get; set; }

	public virtual ChartDockingFlags Behavior { get; set; }

	public Point Location { get; set; }

	public Size Size
	{
		get
		{
			return new Size(Width, Height);
		}
		set
		{
			Width = value.Width;
			Height = value.Height;
		}
	}

	public bool Visible { get; set; }

	public bool Enabled { get; set; }

	public event LocationEventHandler LocationChanging;

	public event EventHandler ChartDockChanged;

	public event EventHandler ChartAlignmentChanged;

	public event EventHandler SizeChanged;

	public event EventHandler LocationChanged;

	public event EventHandler VisibleChanged;

	public virtual SizeF Measure(Graphics g, SizeF size)
	{
		return SizeF.Empty;
	}

	protected virtual void OnVisibleChanged(EventArgs e)
	{
	}

	public virtual void Render(Graphics g)
	{
	}
}
