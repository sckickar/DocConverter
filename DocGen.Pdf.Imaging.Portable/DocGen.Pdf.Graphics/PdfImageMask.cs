using System;
using SkiaSharp;

namespace DocGen.Pdf.Graphics;

public class PdfImageMask : PdfMask
{
	private PdfTiffImage m_imageMask;

	private bool m_softMask;

	public PdfTiffImage Mask => m_imageMask;

	public bool SoftMask => m_softMask;

	public PdfImageMask(PdfTiffImage imageMask)
	{
		if (imageMask == null)
		{
			throw new ArgumentNullException("imageMask");
		}
		m_imageMask = imageMask;
		if (imageMask.SKBitmap != null)
		{
			SKBitmap sKBitmap = imageMask.SKBitmap;
			if (sKBitmap.ColorType == SKColorType.Bgra1010102 || sKBitmap.ColorType == SKColorType.Bgra8888 || sKBitmap.ColorType == SKColorType.Rgba1010102 || sKBitmap.ColorType == SKColorType.Rgba16161616 || sKBitmap.ColorType == SKColorType.Rgba8888 || sKBitmap.ColorType == SKColorType.RgbaF16 || sKBitmap.ColorType == SKColorType.RgbaF16Clamped || sKBitmap.ColorType == SKColorType.RgbaF32 || sKBitmap.ColorType == SKColorType.Argb4444 || sKBitmap.ColorType == SKColorType.Gray8 || sKBitmap.ColorType == SKColorType.Alpha8)
			{
				m_softMask = true;
			}
		}
	}
}
