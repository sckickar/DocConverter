using DocGen.Drawing;

internal class TextProperties
{
	private Color m_strokingBrush;

	private RectangleF m_bounds;

	internal Color StrokingBrush => m_strokingBrush;

	internal RectangleF Bounds => m_bounds;

	internal TextProperties(Color brush, RectangleF bounds)
	{
		m_strokingBrush = brush;
		m_bounds = bounds;
	}

	internal TextProperties(RectangleF bounds)
	{
		m_bounds = bounds;
	}
}
