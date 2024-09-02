using System;
using SkiaSharp;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Drawing.SkiaSharpHelper;

internal sealed class TextureBrush : Brush, ITextureBrush, IBrush, IDisposable
{
	private WrapMode m_wrapMode;

	private SKBitmap m_sourceBitmap;

	private float m_scaleX = 1f;

	private float m_scaleY = 1f;

	private float m_transX;

	private float m_transY;

	internal WrapMode WrapMode
	{
		get
		{
			return m_wrapMode;
		}
		set
		{
			m_wrapMode = value;
			UpdateTextureBrush();
		}
	}

	internal TextureBrush(Image image, RectangleF dstRect, ImageAttributes imageAttr)
		: base(Color.White)
	{
		if (imageAttr.Transparency < 1f)
		{
			base.Color = Color.FromArgb((int)((float)(int)Color.White.A * imageAttr.Transparency), Color.White.R, Color.White.G, Color.White.B);
		}
		m_sourceBitmap = image.SKBitmap.Resize(new SKImageInfo((int)dstRect.Width, (int)dstRect.Height), SKBitmapResizeMethod.Box);
		base.Shader = SKShader.CreateBitmap(m_sourceBitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
		base.FilterQuality = SKFilterQuality.High;
	}

	internal void TranslateTransform(float dx, float dy)
	{
		m_transX = dx;
		m_transY = dy;
		UpdateTextureBrush();
	}

	internal void ScaleTransform(float sx, float sy)
	{
		m_scaleX = sx;
		m_scaleY = sy;
		UpdateTextureBrush();
	}

	private void UpdateTextureBrush()
	{
		SKMatrix localMatrix = default(SKMatrix);
		localMatrix.ScaleX = m_scaleX;
		localMatrix.ScaleY = m_scaleY;
		localMatrix.TransX = m_transX;
		localMatrix.TransY = m_transY;
		localMatrix.SkewX = 0f;
		localMatrix.SkewY = 0f;
		localMatrix.Persp2 = 1f;
		switch (m_wrapMode)
		{
		case WrapMode.Clamp:
			base.Shader = SKShader.CreateBitmap(m_sourceBitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, localMatrix);
			break;
		case WrapMode.TileFlipX:
			base.Shader = SKShader.CreateBitmap(m_sourceBitmap, SKShaderTileMode.Mirror, SKShaderTileMode.Repeat, localMatrix);
			break;
		case WrapMode.TileFlipY:
			base.Shader = SKShader.CreateBitmap(m_sourceBitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Mirror, localMatrix);
			break;
		case WrapMode.TileFlipXY:
			base.Shader = SKShader.CreateBitmap(m_sourceBitmap, SKShaderTileMode.Mirror, SKShaderTileMode.Mirror, localMatrix);
			break;
		case WrapMode.Tile:
			base.Shader = SKShader.CreateBitmap(m_sourceBitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, localMatrix);
			break;
		}
	}
}
