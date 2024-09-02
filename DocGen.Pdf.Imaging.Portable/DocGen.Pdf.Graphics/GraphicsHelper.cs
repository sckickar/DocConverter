using System;
using System.Collections.Generic;
using SkiaSharp;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal class GraphicsHelper : IDisposable
{
	internal SKCanvas m_canvas;

	internal SKSurface m_surface;

	private CompositingMode m_compositingMode;

	private InterpolationMode m_interpolationMode;

	private GraphicsUnit m_pageUnit;

	private float m_dpiX = 96f;

	private int m_deafultClipIndex = -1;

	internal double m_pointToPixelScalingFactor = 1.3333333333333333;

	internal const string AscentDescentText = "lg";

	internal bool m_bStateSaved;

	internal static bool IsLinuxOS;

	internal bool isStandardUnicode;

	private float m_dpiY = 96f;

	private SmoothingMode m_smoothingMode;

	internal CompositingMode CompositingMode
	{
		get
		{
			return m_compositingMode;
		}
		set
		{
			m_compositingMode = value;
		}
	}

	internal InterpolationMode InterpolationMode
	{
		get
		{
			return m_interpolationMode;
		}
		set
		{
			m_interpolationMode = value;
		}
	}

	public GraphicsUnit PageUnit
	{
		get
		{
			return m_pageUnit;
		}
		set
		{
			if (m_pageUnit != value)
			{
				m_pageUnit = value;
				if (m_pageUnit == GraphicsUnit.Point)
				{
					m_canvas.Scale((float)m_pointToPixelScalingFactor);
				}
			}
		}
	}

	internal Matrix Transform
	{
		get
		{
			if (m_canvas != null)
			{
				Matrix matrix = new Matrix();
				matrix.Elements = new float[6]
				{
					m_canvas.TotalMatrix.ScaleX,
					m_canvas.TotalMatrix.SkewY,
					m_canvas.TotalMatrix.SkewX,
					m_canvas.TotalMatrix.ScaleY,
					m_canvas.TotalMatrix.TransX,
					m_canvas.TotalMatrix.TransY
				};
				matrix.SkMatrix = m_canvas.TotalMatrix;
				return matrix;
			}
			return null;
		}
		set
		{
			if (m_pageUnit == GraphicsUnit.Point)
			{
				Matrix matrix = new Matrix();
				SKMatrix tempmatrix = SKMatrix.MakeScale((float)m_pointToPixelScalingFactor, (float)m_pointToPixelScalingFactor);
				SetSkMatrix(matrix, tempmatrix, MatrixOrder.Prepend);
				SKMatrix target = GetSKMatrix(value);
				SKMatrix sKMatrix = GetSKMatrix(matrix);
				SKMatrix.PostConcat(ref target, sKMatrix);
				matrix.SkMatrix = target;
				matrix.Elements = new float[6] { target.ScaleX, target.SkewY, target.SkewX, target.ScaleY, target.TransX, target.TransY };
				m_canvas.SetMatrix(GetSKMatrix(matrix));
			}
			else
			{
				m_canvas.SetMatrix(GetSKMatrix(value));
			}
		}
	}

	internal float DpiX => m_dpiX;

	internal float DpiY => m_dpiY;

	public RectangleF ClipBounds
	{
		get
		{
			if (m_canvas != null)
			{
				return RenderHelper.GetClipRectangle(m_canvas.LocalClipBounds);
			}
			return RectangleF.Empty;
		}
	}

	public SmoothingMode SmoothingMode
	{
		get
		{
			return m_smoothingMode;
		}
		set
		{
			m_smoothingMode = value;
		}
	}

	public GraphicsHelper(Bitmap image)
	{
		m_surface = SKSurface.Create(image.m_sKBitmap.Info);
		m_canvas = m_surface.Canvas;
		image.m_graphics = this;
	}

	internal GraphicsHelper FromImage(Bitmap bmp)
	{
		return new GraphicsHelper(bmp);
	}

	internal void DrawImage(Bitmap bitmap, RectangleF rectangle)
	{
		m_canvas.DrawBitmap(bitmap.m_sKBitmap, GetImageRect(rectangle), new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	internal void DrawImage(Bitmap image, int x, int y)
	{
		m_canvas.DrawBitmap(image.m_sKBitmap, x, y, new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	internal void DrawImage(Bitmap image, Rectangle destRect, float x, float y, float width, float height, GraphicsUnit unit, ImageAttributes attr)
	{
		SetImageAttributes(image, attr);
		RectangleF rectangle = new RectangleF(x, y, width, height);
		m_canvas.DrawBitmap(image.m_sKBitmap, GetImageRect(rectangle), GetImageRect(destRect), new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	internal SizeF MeasureString(string text, SKPaint paint)
	{
		if (string.IsNullOrEmpty(text))
		{
			return SizeF.Empty;
		}
		SKRect bounds = default(SKRect);
		return new SizeF(paint.MeasureText(text, ref bounds), paint.FontSpacing);
	}

	internal void SetClip(GraphicsPath path)
	{
		if (m_deafultClipIndex == -1)
		{
			m_deafultClipIndex = m_canvas.Save();
		}
		m_canvas.ClipPath(path);
	}

	internal void FillPath(PdfColor color, GraphicsPath gPath)
	{
		SKPaint sKPaint = new SKPaint();
		sKPaint.Color = new SKColor(color.R, color.G, color.B, color.A);
		sKPaint.Style = SKPaintStyle.Fill;
		sKPaint.IsAntialias = true;
		m_canvas.DrawPath(gPath, sKPaint);
	}

	internal void FillRectangle(PdfColor color, float x, float y, float width, float height)
	{
		SKPaint sKPaint = new SKPaint();
		sKPaint.Style = SKPaintStyle.Fill;
		sKPaint.Color = new SKColor(color.R, color.G, color.B, color.A);
		m_canvas.DrawRect(new SKRect(x, y, x + width, y + height), sKPaint);
	}

	internal void SetSkMatrix(Matrix matrix, SKMatrix tempmatrix, MatrixOrder matrixOrder)
	{
		SKMatrix sKMatrix = GetSKMatrix(matrix);
		if (matrixOrder == MatrixOrder.Append)
		{
			SKMatrix.PreConcat(ref tempmatrix, sKMatrix);
		}
		else
		{
			SKMatrix.PostConcat(ref tempmatrix, sKMatrix);
		}
		matrix.SkMatrix = tempmatrix;
		matrix.Elements = new float[6] { tempmatrix.ScaleX, tempmatrix.SkewY, tempmatrix.SkewX, tempmatrix.ScaleY, tempmatrix.TransX, tempmatrix.TransY };
	}

	internal SKMatrix GetSKMatrix(Matrix matrix)
	{
		if (matrix.SkMatrix == null)
		{
			SKMatrix sKMatrix = default(SKMatrix);
			sKMatrix.ScaleX = matrix.Elements[0];
			sKMatrix.SkewY = matrix.Elements[1];
			sKMatrix.SkewX = matrix.Elements[2];
			sKMatrix.ScaleY = matrix.Elements[3];
			sKMatrix.TransX = matrix.Elements[4];
			sKMatrix.TransY = matrix.Elements[5];
			sKMatrix.Persp2 = 1f;
			matrix.SkMatrix = sKMatrix;
		}
		return (SKMatrix)matrix.SkMatrix;
	}

	internal SKRect GetImageRect(RectangleF rectangle)
	{
		return new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
	}

	internal void Clear(Color color)
	{
		m_canvas.Clear(new SKColor(color.R, color.G, color.B, color.A));
	}

	private void SetImageAttributes(Bitmap image, ImageAttributes attr)
	{
		if (attr != null && attr.ColorMatrices != null && attr.ColorMatrices.Count > 0)
		{
			ColorMatrix colorMatrix = null;
			foreach (KeyValuePair<ColorAdjustType, ColorMatrix> colorMatrix2 in attr.ColorMatrices)
			{
				if (colorMatrix2.Value != null)
				{
					colorMatrix = colorMatrix2.Value;
					break;
				}
			}
			if (colorMatrix == null || (colorMatrix.Matrix00 == 1f && colorMatrix.Matrix11 == 1f && colorMatrix.Matrix22 == 1f && colorMatrix.Matrix33 == 1f && colorMatrix.Matrix44 == 1f))
			{
				return;
			}
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					Color pixel = image.GetPixel(i, j);
					int num = (int)(((double)(int)pixel.R / 255.0 * (double)colorMatrix.Matrix00 + (double)colorMatrix.Matrix40) * 255.0);
					int num2 = (int)(((double)(int)pixel.G / 255.0 * (double)colorMatrix.Matrix11 + (double)colorMatrix.Matrix41) * 255.0);
					int num3 = (int)(((double)(int)pixel.B / 255.0 * (double)colorMatrix.Matrix22 + (double)colorMatrix.Matrix42) * 255.0);
					int num4 = (int)(((double)(int)pixel.A / 255.0 * (double)colorMatrix.Matrix33 + (double)colorMatrix.Matrix43) * 255.0);
					if (num > 255)
					{
						num = 255;
					}
					if (num2 > 255)
					{
						num2 = 255;
					}
					if (num3 > 255)
					{
						num3 = 255;
					}
					if (num4 > 255)
					{
						num4 = 255;
					}
					image.SetPixel(i, j, Color.FromArgb(num4, num, num2, num3));
				}
			}
		}
		else
		{
			if (attr == null || attr.ColorMap == null || attr.ColorMap.Length == 0)
			{
				return;
			}
			for (int k = 0; k < image.Width; k++)
			{
				for (int l = 0; l < image.Height; l++)
				{
					ColorMap[] colorMap = attr.ColorMap;
					foreach (ColorMap colorMap2 in colorMap)
					{
						if (image.GetPixel(k, l).Equals(colorMap2.OldColor))
						{
							image.SetPixel(k, l, colorMap2.NewColor);
						}
					}
				}
			}
		}
	}

	internal void SetClip(Rectangle rect, CombineMode mode)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(rect);
		SetClip(graphicsPath, mode);
	}

	internal void SetClip(GraphicsPath path, CombineMode mode)
	{
		if (m_deafultClipIndex == -1)
		{
			m_deafultClipIndex = m_canvas.Save();
		}
		if (mode == CombineMode.Intersect || mode == CombineMode.Exclude)
		{
			m_canvas.ClipPath(path, (mode != CombineMode.Exclude) ? SKClipOperation.Intersect : SKClipOperation.Difference);
		}
		else
		{
			SetClip(path);
		}
	}

	internal void ScaleTransform(float x, float y)
	{
		m_canvas.Scale(x, y);
	}

	public void TranslateTransform(float x, float y)
	{
		m_canvas.Translate(x, y);
	}

	public void RotateTransform(float degree)
	{
		m_canvas.RotateDegrees(degree);
	}

	internal void MultiplyTransform(Matrix matrix)
	{
		SKMatrix target = matrix.GetSKMatrix();
		SKMatrix.PostConcat(ref target, m_canvas.TotalMatrix);
		m_canvas.SetMatrix(target);
	}

	internal GraphicsState Save()
	{
		return new GraphicsState(m_canvas.Save());
	}

	internal void Restore(GraphicsState state)
	{
		if (state.m_nativeState != 0)
		{
			m_canvas.RestoreToCount(state.m_nativeState);
		}
		else
		{
			m_canvas.Restore();
		}
	}

	internal nint GetHdc()
	{
		return IntPtr.Zero;
	}

	public void ResetClip()
	{
		if (m_deafultClipIndex != -1)
		{
			SKMatrix totalMatrix = m_canvas.TotalMatrix;
			m_canvas.RestoreToCount(m_deafultClipIndex);
			m_deafultClipIndex = m_canvas.Save();
			m_canvas.SetMatrix(totalMatrix);
		}
	}

	internal static RectangleF GetClipRectangle(SKRect rect)
	{
		return new RectangleF(rect.Left + 1f, rect.Top + 1f, rect.Width - 2f, rect.Height - 2f);
	}

	internal void DrawString(string str, SKPaint sKPaint, PointF pointF)
	{
		m_canvas.DrawText(str, pointF.X, pointF.Y, sKPaint);
	}

	internal void DrawImage(ImageHelper image, float x, float y, float width, float height)
	{
		m_canvas.DrawBitmap(image.SKBitmap, RenderHelper.ImageRect(x, y, width, height), new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	internal void DrawImage(ImageHelper image, Rectangle rectangle)
	{
		DrawImage(image, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void Dispose()
	{
		if (m_canvas != null)
		{
			m_canvas.Dispose();
		}
		m_canvas = null;
	}
}
