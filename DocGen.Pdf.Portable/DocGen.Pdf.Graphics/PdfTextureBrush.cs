using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal class PdfTextureBrush : PdfTilingBrush
{
	private PdfImage m_textureImage;

	private RectangleF m_imageBounds = RectangleF.Empty;

	public PdfTextureBrush(PdfImage image, RectangleF dstRect, float transparency)
		: base(dstRect)
	{
		m_textureImage = image;
		m_imageBounds = dstRect;
		PdfGraphicsState state = base.Graphics.Save();
		base.Graphics.SetTransparency(transparency);
		base.Graphics.DrawImage(m_textureImage, 0f, 0f, dstRect.Width, dstRect.Height);
		base.Graphics.Restore(state);
	}
}
