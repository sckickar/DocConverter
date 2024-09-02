using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal static class LayoutHelper
{
	public static RectangleF AlignRectangle(RectangleF bounds, SizeF size, ContentAlignment alignment)
	{
		switch (alignment)
		{
		case ContentAlignment.BottomCenter:
			bounds = new RectangleF(bounds.Left + 0.5f * (bounds.Width - size.Width), bounds.Bottom - size.Height, size.Width, size.Height);
			break;
		case ContentAlignment.BottomLeft:
			bounds = new RectangleF(bounds.Left, bounds.Bottom - size.Height, size.Width, size.Height);
			break;
		case ContentAlignment.BottomRight:
			bounds = new RectangleF(bounds.Right - size.Width, bounds.Bottom - size.Height, size.Width, size.Height);
			break;
		case ContentAlignment.MiddleCenter:
			bounds = new RectangleF(bounds.Left + 0.5f * (bounds.Width - size.Width), bounds.Top + 0.5f * (bounds.Height - size.Height), size.Width, size.Height);
			break;
		case ContentAlignment.MiddleLeft:
			bounds = new RectangleF(bounds.Left, bounds.Top + 0.5f * (bounds.Height - size.Height), size.Width, size.Height);
			break;
		case ContentAlignment.MiddleRight:
			bounds = new RectangleF(bounds.Right - size.Width, bounds.Top + 0.5f * (bounds.Height - size.Height), size.Width, size.Height);
			break;
		case ContentAlignment.TopCenter:
			bounds = new RectangleF(bounds.Left + 0.5f * (bounds.Width - size.Width), bounds.Top, size.Width, size.Height);
			break;
		case ContentAlignment.TopLeft:
			bounds = new RectangleF(bounds.Left, bounds.Top, size.Width, size.Height);
			break;
		case ContentAlignment.TopRight:
			bounds = new RectangleF(bounds.Left, bounds.Right - size.Width, size.Width, size.Height);
			break;
		}
		return bounds;
	}

	public static RectangleF AlignRectangle(RectangleF bounds, SizeF size, ChartAlignment horizontal, ChartAlignment vertical)
	{
		switch (horizontal)
		{
		case ChartAlignment.Center:
			bounds.X += 0.5f * (bounds.Width - size.Width);
			break;
		case ChartAlignment.Far:
			bounds.X += bounds.Width - size.Width;
			break;
		}
		switch (vertical)
		{
		case ChartAlignment.Center:
			bounds.Y += 0.5f * (bounds.Height - size.Height);
			break;
		case ChartAlignment.Far:
			bounds.Y += bounds.Height - size.Height;
			break;
		}
		bounds.Size = size;
		return bounds;
	}

	public static RectangleF AlignRectangle(PointF point, SizeF size, ContentAlignment alignment)
	{
		switch (alignment)
		{
		case ContentAlignment.BottomCenter:
			point = new PointF(point.X - 0.5f * size.Width, point.Y);
			break;
		case ContentAlignment.BottomLeft:
			point = new PointF(point.X - size.Width, point.Y);
			break;
		case ContentAlignment.MiddleCenter:
			point = new PointF(point.X - 0.5f * size.Width, point.Y - 0.5f * size.Height);
			break;
		case ContentAlignment.MiddleLeft:
			point = new PointF(point.X - size.Width, point.Y - 0.5f * size.Height);
			break;
		case ContentAlignment.MiddleRight:
			point = new PointF(point.X, point.Y - 0.5f * size.Height);
			break;
		case ContentAlignment.TopCenter:
			point = new PointF(point.X - 0.5f * size.Width, point.Y - size.Height);
			break;
		case ContentAlignment.TopLeft:
			point = new PointF(point.X - size.Width, point.Y - size.Height);
			break;
		case ContentAlignment.TopRight:
			point = new PointF(point.X, point.Y - size.Height);
			break;
		}
		return new RectangleF(point, size);
	}

	public static RectangleF AlignRectangle(PointF point, SizeF size, ChartAlignment horizontal, ChartAlignment vertical)
	{
		switch (horizontal)
		{
		case ChartAlignment.Center:
			point.X -= 0.5f * size.Width;
			break;
		case ChartAlignment.Far:
			point.X -= size.Width;
			break;
		}
		switch (vertical)
		{
		case ChartAlignment.Center:
			point.Y -= 0.5f * size.Height;
			break;
		case ChartAlignment.Far:
			point.Y -= size.Height;
			break;
		}
		return new RectangleF(point, size);
	}
}
