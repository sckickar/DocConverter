using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public abstract class PdfGraphicsElement
{
	public void Draw(PdfGraphics graphics)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		Draw(graphics, PointF.Empty);
	}

	public void Draw(PdfGraphics graphics, PointF location)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		Draw(graphics, location.X, location.Y);
	}

	public virtual void Draw(PdfGraphics graphics, float x, float y)
	{
		bool num = x != 0f || y != 0f;
		PdfGraphicsState state = null;
		if (num)
		{
			state = graphics.Save();
			graphics.TranslateTransform(x, y);
		}
		DrawInternal(graphics);
		if (num)
		{
			graphics.Restore(state);
		}
	}

	protected abstract void DrawInternal(PdfGraphics graphics);
}
