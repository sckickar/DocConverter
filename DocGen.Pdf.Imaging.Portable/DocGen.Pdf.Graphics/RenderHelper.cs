using SkiaSharp;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal class RenderHelper
{
	internal const double PointToPixelScalingFactor = 1.3333333333333333;

	public GraphicsHelper GetGraphics()
	{
		return new GraphicsHelper(new Bitmap(1, 1));
	}

	internal static SKRect SKRect(Rectangle rectangle)
	{
		return new SKRect(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);
	}

	internal static SKRect SKRect(RectangleF rectangle)
	{
		return new SKRect(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);
	}

	internal static SKPath SKPath(Rectangle rectangle)
	{
		SKPath sKPath = new SKPath();
		sKPath.AddRect(SKRect(rectangle));
		return sKPath;
	}

	internal static SKPath SKPath(RectangleF rectangle)
	{
		SKPath sKPath = new SKPath();
		sKPath.AddRect(SKRect(rectangle));
		return sKPath;
	}

	internal static SKRect ImageRect(RectangleF rectangle)
	{
		return new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
	}

	internal static SKRect ImageRect(float x, float y, float width, float height)
	{
		return new SKRect(x, y, x + width, y + height);
	}

	internal static RectangleF GetClipRectangle(SKRect rect)
	{
		return new RectangleF(rect.Left + 1f, rect.Top + 1f, rect.Width - 2f, rect.Height - 2f);
	}
}
