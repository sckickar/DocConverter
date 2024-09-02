using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal abstract class ChartSegment
{
	protected RectangleF m_bounds;

	internal RectangleF Clip = RectangleF.Empty;

	internal bool Update;

	protected int m_zOrder;

	public RectangleF Bounds
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

	public int ZOrder
	{
		get
		{
			return m_zOrder;
		}
		set
		{
			m_zOrder = value;
		}
	}

	public virtual void Draw(Graphics g)
	{
	}
}
