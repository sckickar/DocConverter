using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;
using DocGen.Drawing.DocIOHelper;
using DocGen.Office;
using DocGen.SkiaSharpHelper.Portable;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class Graphics : IDisposable, IGraphics
{
	internal const string AscentDescentText = "lg";

	private SKCanvas m_canvas;

	private SKSurface m_surface;

	private bool m_rtfProcess;

	private GraphicsUnit m_pageUnit;

	private float m_dpiX = 96f;

	private float m_dpiY = 96f;

	private CompositingMode m_compositingMode;

	private InterpolationMode m_interpolationMode;

	private PixelOffsetMode m_pixelOffsetMode;

	private SmoothingMode m_smoothingMode;

	private CompositingQuality m_compositingQuality;

	private int m_deafultClipIndex = -1;

	private char[] _numberFormatChar = new char[1] { '€' };

	private DocGen.SkiaSharpHelper.Portable.Bidi m_biDirection;

	private static GraphicsUnit m_currentGraphicsUnit = GraphicsUnit.Pixel;

	private static readonly object m_threadLocker = new object();

	private bool m_isRtfTextImageConversion;

	internal static bool IsLinuxOS;

	private TextRenderingHint m_hint;

	internal DocGen.SkiaSharpHelper.Portable.Bidi BiDirection
	{
		get
		{
			if (m_biDirection == null)
			{
				m_biDirection = new DocGen.SkiaSharpHelper.Portable.Bidi();
			}
			return m_biDirection;
		}
	}

	internal static GraphicsUnit CurrentGraphicsUnit => m_currentGraphicsUnit;

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

	public Matrix Transform
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
				matrix.Scale(1.3333334f, 1.3333334f);
				matrix.Multiply(value, MatrixOrder.Prepend);
				m_canvas.SetMatrix(matrix.GetSKMatrix());
			}
			else
			{
				m_canvas.SetMatrix(value.GetSKMatrix());
			}
		}
	}

	internal float DpiX => m_dpiX;

	internal float DpiY => m_dpiY;

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

	internal CompositingQuality CompositingQuality
	{
		get
		{
			return m_compositingQuality;
		}
		set
		{
			m_compositingQuality = value;
		}
	}

	public InterpolationMode InterpolationMode
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

	internal PixelOffsetMode PixelOffsetMode
	{
		get
		{
			return m_pixelOffsetMode;
		}
		set
		{
			m_pixelOffsetMode = value;
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

	internal SKSurface SkSurface => m_surface;

	public TextRenderingHint TextRenderingHint
	{
		get
		{
			return m_hint;
		}
		set
		{
			m_hint = value;
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
					m_canvas.Scale(1.3333334f);
				}
				m_currentGraphicsUnit = value;
			}
		}
	}

	internal bool IsRtfTextImageConversion
	{
		get
		{
			return m_isRtfTextImageConversion;
		}
		set
		{
			m_isRtfTextImageConversion = value;
		}
	}

	private void SetMatrixValues(Matrix matrix)
	{
	}

	public Graphics(Image image)
	{
		m_surface = SKSurface.Create(image.SKBitmap.Info);
		m_canvas = m_surface.Canvas;
		image.Graphics = this;
		IsLinuxOS = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
	}

	internal Graphics()
	{
		IsLinuxOS = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
	}

	public void Dispose()
	{
		if (m_canvas != null)
		{
			m_canvas.Dispose();
		}
		m_canvas = null;
	}

	public void ResetTransform()
	{
		m_canvas.ResetMatrix();
		if (m_pageUnit == GraphicsUnit.Point)
		{
			m_canvas.Scale(1.3333334f);
		}
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

	public void Clear(Color color)
	{
		m_canvas.Clear(new SKColor(color.R, color.G, color.B, color.A));
	}

	internal nint GetHdc()
	{
		return IntPtr.Zero;
	}

	internal void ReleaseHdc()
	{
	}

	internal void DrawString(string text, Font font, IBrush brush, Rectangle rectangle, StringFormat stringFormat)
	{
		DrawString(text, font, brush, new RectangleF(rectangle.Location, rectangle.Size), stringFormat);
	}

	internal void DrawString(string text, Stream stream, Font font, Brush brush, RectangleF rectangle, StringFormat stringFormat)
	{
		if (!string.IsNullOrEmpty(text))
		{
			FontExtension fontExtension = null;
			fontExtension = ((stream == null) ? new FontExtension(font.Name, font.Size, font.Style, m_pageUnit) : new FontExtension(stream, font.Name, font.Size, font.Style, m_pageUnit));
			DrawString(fontExtension, text, stream, font, brush, rectangle, stringFormat);
		}
	}

	internal void DrawString(string text, Stream stream, Font font, Brush brush, RectangleF rectangle, StringFormat stringFormat, FontScriptType scriptType)
	{
		if (!string.IsNullOrEmpty(text))
		{
			FontExtension fontExtension = null;
			fontExtension = ((stream == null) ? new FontExtension(font.Name, font.Size, font.Style, m_pageUnit, scriptType) : new FontExtension(stream, font.Name, font.Size, font.Style, m_pageUnit, scriptType));
			fontExtension.Color = Extension.GetSKColor(brush.Color);
			if (brush.Shader != null)
			{
				fontExtension.Shader = brush.Shader;
			}
			fontExtension.SetTextAlign(stringFormat.Alignment);
			SKRect sKRect = default(SKRect);
			sKRect = ((stream == null) ? MeasureString(text, font, stringFormat) : MeasureString(text, stream, font, stringFormat));
			float num = rectangle.Y - sKRect.Top;
			switch (stringFormat.LineAlignment)
			{
			case StringAlignment.Center:
				num += (rectangle.Height - sKRect.Height) / 2f;
				break;
			case StringAlignment.Far:
				num += rectangle.Height - sKRect.Height;
				break;
			}
			float num2 = sKRect.Height + sKRect.Top;
			float num3 = num + num2 / 2f;
			if (CheckForArabicOrHebrew(text) || (stringFormat.FormatFlags & StringFormatFlags.DirectionRightToLeft) != 0)
			{
				string inputText = new DocGen.SkiaSharpHelper.Portable.ArabicShapeRenderer().Shape(text.ToCharArray(), 0);
				text = BiDirection.GetLogicalToVisualString(inputText, isRTL: true);
			}
			m_canvas.DrawText(text, rectangle.X, num3, fontExtension);
			if (font.Underline)
			{
				m_canvas.DrawLine(rectangle.X, num3 + num2 / 4f, rectangle.Width + rectangle.X, num3 + num2 / 4f, fontExtension);
			}
			if (font.Strikeout)
			{
				m_canvas.DrawLine(rectangle.X, num, rectangle.Width + rectangle.X, num, fontExtension);
			}
		}
	}

	private void DrawString(FontExtension paint, string text, Stream stream, Font font, Brush brush, RectangleF rectangle, StringFormat stringFormat)
	{
		paint.Color = Extension.GetSKColor(brush.Color);
		if (brush.Shader != null)
		{
			paint.Shader = brush.Shader;
		}
		SKRect sKRect = default(SKRect);
		sKRect = ((stream == null) ? MeasureString(text, font, stringFormat) : MeasureString(text, stream, font, stringFormat));
		rectangle.X -= sKRect.Left;
		float num = rectangle.Y - sKRect.Top;
		switch (stringFormat.Alignment)
		{
		case StringAlignment.Near:
			rectangle.X += 1f;
			break;
		case StringAlignment.Center:
			rectangle.X += (rectangle.Width - sKRect.Width) / 2f;
			break;
		case StringAlignment.Far:
			rectangle.X += rectangle.Width - sKRect.Width;
			rectangle.X -= 1f;
			break;
		}
		switch (stringFormat.LineAlignment)
		{
		case StringAlignment.Center:
			num += (rectangle.Height - sKRect.Height) / 2f;
			break;
		case StringAlignment.Far:
			num += rectangle.Height - sKRect.Height;
			break;
		}
		float num2 = sKRect.Height + sKRect.Top;
		float num3 = num + num2 / 2f;
		if (CheckForArabicOrHebrew(text))
		{
			string inputText = new DocGen.SkiaSharpHelper.Portable.ArabicShapeRenderer().Shape(text.ToCharArray(), 0);
			text = BiDirection.GetLogicalToVisualString(inputText, isRTL: true);
		}
		m_canvas.DrawText(text, rectangle.X, num3, paint);
		if (font.Underline)
		{
			m_canvas.DrawLine(rectangle.X, num3 + num2 / 4f, rectangle.Width + rectangle.X, num3 + num2 / 4f, paint);
		}
		if (font.Strikeout)
		{
			m_canvas.DrawLine(rectangle.X, num, rectangle.Width + rectangle.X, num, paint);
		}
	}

	private bool CheckForArabicOrHebrew(string unicodeText)
	{
		char[] array = null;
		array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && array[i] >= '\u0590' && array[i] <= 'ۿ')
			{
				return true;
			}
		}
		return false;
	}

	public void DrawString(string text, Font font, IBrush brush, RectangleF rectangle, StringFormat stringFormat)
	{
		DrawString(text, null, font, brush as Brush, rectangle, stringFormat);
	}

	public void DrawString(string text, Font font, IBrush brush, RectangleF rectangle, StringFormat stringFormat, FontScriptType scriptType)
	{
		DrawString(text, null, font, brush as Brush, rectangle, stringFormat, scriptType);
	}

	public void DrawString(string text, Font font, IBrush brush, float x, float y, StringFormat stringFormat)
	{
		FontExtension fontExtension = new FontExtension(font.Name, font.Size, font.Style, m_pageUnit);
		fontExtension.Color = Extension.GetSKColor(brush.Color);
		if ((brush as Brush).Shader != null)
		{
			fontExtension.Shader = (brush as Brush).Shader;
		}
		m_canvas.DrawText(text, x, y, fontExtension);
	}

	public void DrawString(string text, Font font, IBrush solidBrush, int x, int y, FontStyle fontStyle, FontScriptType scriptType)
	{
		FontExtension fontExtension = new FontExtension(font.Name, font.Size, font.Style, GraphicsUnit.Point, scriptType);
		fontExtension.Color = ((solidBrush != null) ? Extension.GetSKColor(solidBrush.Color) : SKColor.Empty);
		if (text != string.Empty)
		{
			SKRect bounds = default(SKRect);
			fontExtension.MeasureText(text, ref bounds);
			y -= (int)bounds.Top;
		}
		m_canvas.DrawText(text, x, y, fontExtension);
	}

	internal void DrawString(string text, Font font, Brush brush, PointF pt, StringFormat stringFormat)
	{
		DrawString(text, font, brush, pt.X, pt.Y, stringFormat);
	}

	public void DrawImage(IImage image, float x, float y, float width, float height)
	{
		m_canvas.DrawBitmap((image as Image).SKBitmap, RenderHelper.ImageRect(x, y, width, height), new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	internal void DrawImage(DocGen.Drawing.Image image, float x, float y, float width, float height)
	{
		m_canvas.DrawBitmap(Image.FromImage(image).SKBitmap, RenderHelper.ImageRect(x, y, width, height), new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	public void DrawImage(byte[] imageBytes, Rectangle rectangle)
	{
		DrawImage(DocGen.Drawing.Image.FromStream(new MemoryStream(imageBytes)), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	internal void DrawImage(DocGen.Drawing.Image image, RectangleF rectangle)
	{
		DrawImage(image, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	internal void DrawImage(DocGen.Drawing.Image image, Rectangle rectangle)
	{
		DrawImage(image, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	internal void DrawImage(Image image, int x, int y)
	{
		DrawImage(image, new PointF(x, y));
	}

	internal void DrawImage(Image image, PointF point)
	{
		m_canvas.DrawBitmap(image.SKBitmap, point.X, point.Y, new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	public void DrawImage(IImage image, RectangleF rectangle)
	{
		m_canvas.DrawBitmap((image as Image).SKBitmap, RenderHelper.ImageRect(rectangle), new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	internal void DrawImage(Bitmap bitmap, RectangleF rectangle)
	{
		m_canvas.DrawBitmap(bitmap.SKBitmap, RenderHelper.ImageRect(rectangle), new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	public void DrawPath(IPen pen, IGraphicsPath path)
	{
		m_canvas.DrawPath(path as GraphicsPath, (pen as Pen).SKPaint);
	}

	internal static Graphics FromHwnd(nint intPtr)
	{
		return new RenderHelper().GetGraphics();
	}

	public SizeF MeasureString(string text, Font font)
	{
		return MeasureString(text, font, new PointF(0f, 0f), StringFormat.GenericDefault);
	}

	public SizeF MeasureString(string text, Font font, FontStyle fontStyle, FontScriptType scriptType)
	{
		if (Extension.IsHarfBuzzSupportedScript(scriptType))
		{
			return new FontExtension(font.Name, font.Size, font.Style, GraphicsUnit.Point, scriptType).MeasureText(text, scriptType);
		}
		if (IsLinuxOS)
		{
			if (IsUnicodeText(text))
			{
				float num = 0f;
				string name = font.Name;
				string unicodeFamilyName = Extension.GetUnicodeFamilyName(text[0].ToString(), name);
				if (unicodeFamilyName == name || unicodeFamilyName == null)
				{
					return new SizeF(MeasureText(text, font.Name, font.Size, font.Style, null).Width, font.Size);
				}
				for (int i = 0; i < text.Length; i++)
				{
					lock (m_threadLocker)
					{
						unicodeFamilyName = Extension.GetUnicodeFamilyName(text[i].ToString(), name);
						float width = MeasureText(text[i].ToString(), unicodeFamilyName, font.Size, font.Style, null, isUnicode: true).Width;
						if (width == 0f)
						{
							width = MeasureText(text[i].ToString(), name, font.Size, font.Style, null, isUnicode: true).Width;
						}
						num += width;
					}
				}
				return new SizeF(num, font.Size);
			}
			return new SizeF(MeasureText(text, font.Name, font.Size, font.Style, null).Width, font.Size);
		}
		FontExtension fontExtension = new FontExtension(font.Name, font.Size, font.Style, GraphicsUnit.Point, scriptType);
		float width2 = 0f;
		if (text != string.Empty)
		{
			width2 = fontExtension.MeasureText(text);
		}
		return new SizeF(width2, font.Size);
	}

	public SizeF MeasureString(string text, Font font, PointF pointF, StringFormat format, FontStyle fontStyle, FontScriptType scriptType)
	{
		if (Extension.IsHarfBuzzSupportedScript(scriptType))
		{
			return new FontExtension(font.Name, font.Size, font.Style, GraphicsUnit.Point, scriptType).MeasureText(text, scriptType);
		}
		if (IsLinuxOS)
		{
			if (IsUnicodeText(text))
			{
				float num = 0f;
				float height = 0f;
				string name = font.Name;
				string unicodeFamilyName = Extension.GetUnicodeFamilyName(text[0].ToString(), name);
				if (unicodeFamilyName == name || unicodeFamilyName == null)
				{
					return MeasureText(text, font.Name, font.Size, font.Style, format);
				}
				for (int i = 0; i < text.Length; i++)
				{
					lock (m_threadLocker)
					{
						unicodeFamilyName = Extension.GetUnicodeFamilyName(text[i].ToString(), name);
						if (unicodeFamilyName == null)
						{
							return MeasureText(text, font.Name, font.Size, font.Style, format);
						}
						SizeF sizeF = MeasureText(text[i].ToString(), unicodeFamilyName, font.Size, font.Style, format, isUnicode: true);
						float width = sizeF.Width;
						if (width == 0f)
						{
							width = MeasureText(text[i].ToString(), name, font.Size, font.Style, format, isUnicode: true).Width;
						}
						num += width;
						if (i == 0)
						{
							height = sizeF.Height;
						}
					}
				}
				return new SizeF(num, height);
			}
			return MeasureText(text, font.Name, font.Size, font.Style, format);
		}
		return new FontExtension(font.Name, font.Size, font.Style, GraphicsUnit.Point, scriptType).MeasureText(text, format, font);
	}

	private SizeF MeasureText(string text, string fontName, float fontSize, FontStyle fontStyle, StringFormat format)
	{
		FontExtension fontExtension = new FontExtension(fontName, fontSize, fontStyle, GraphicsUnit.Point);
		TrueTypeFontStringFormat ttfStringFormat = GetTtfStringFormat(format);
		ttfStringFormat.MeasureTrailingSpaces = true;
		if (fontExtension.TTFFont != null)
		{
			fontExtension.TTFFont.Size = fontSize;
			return fontExtension.TTFFont.MeasureString(text, ttfStringFormat);
		}
		return default(SizeF);
	}

	public bool IsUnicodeText(string text)
	{
		bool result = false;
		if (text != null)
		{
			foreach (char c in text)
			{
				if ((c >= '\u3000' && c <= 'ヿ') || (c >= '\uff00' && c <= '\uffef') || (c >= '一' && c <= '龯') || (c >= '㐀' && c <= '䶿') || (c >= '가' && c <= '\uffef') || (c >= '\u0d80' && c <= '\u0dff'))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private SizeF MeasureText(string text, string fontName, float fontSize, FontStyle fontStyle, StringFormat format, bool isUnicode)
	{
		if (isUnicode)
		{
			FontExtension fontExtension = new FontExtension(Extension.GetUnicodeFontStream(text, fontName), fontName, fontSize, fontStyle, GraphicsUnit.Point, isUnicode: true);
			TrueTypeFontStringFormat ttfStringFormat = GetTtfStringFormat(format);
			ttfStringFormat.MeasureTrailingSpaces = true;
			if (fontExtension.TTFFont != null)
			{
				fontExtension.TTFFont.Size = fontSize;
				return fontExtension.TTFFont.MeasureString(text, ttfStringFormat);
			}
			return default(SizeF);
		}
		return MeasureText(text, fontName, fontSize, fontStyle, format);
	}

	private SizeF MeasureString(string text, Font font, StringFormat format, FontExtension fontImpl)
	{
		float width = 0f;
		float height = 0f;
		TrueTypeFontStringFormat ttfStringFormat = GetTtfStringFormat(format);
		ttfStringFormat.MeasureTrailingSpaces = true;
		if (fontImpl.TTFFont != null)
		{
			fontImpl.TTFFont.Size = font.Size;
			float boundWidth;
			SizeF sizeF = fontImpl.TTFFont.MeasureString(text, ttfStringFormat, out boundWidth);
			width = ((((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0 && (text.StartsWith(" ") || text.EndsWith(" "))) || text.Length <= 2) ? sizeF.Width : boundWidth);
			height = fontImpl.TTFFont.Metrics.GetHeight(ttfStringFormat);
		}
		return new SizeF(width, height);
	}

	internal SKRect MeasureString(string text, Font font, StringFormat format)
	{
		return MeasureString(text, null, font, format);
	}

	internal SizeF MeasureString(string text, Stream stream, Font font, StringFormat format, bool isSkiaMeasuring)
	{
		if (!IsLinuxOS || isSkiaMeasuring)
		{
			SKRect sKRect = MeasureString(!isSkiaMeasuring, text, stream, font, format);
			return new SizeF(sKRect.Width, sKRect.Height);
		}
		FontExtension fontExtension = null;
		fontExtension = ((stream == null) ? new FontExtension(font.Name, font.Size, font.Style, m_pageUnit) : new FontExtension(stream, font.Name, font.Size, font.Style, m_pageUnit));
		return MeasureString(text, font, format, fontExtension);
	}

	internal SKRect MeasureString(string text, Stream stream, Font font, StringFormat format)
	{
		return MeasureString(isPPTXToPdfConversion: false, text, stream, font, format);
	}

	internal SKRect MeasureString(bool isPPTXToPdfConversion, string text, Stream stream, Font font, StringFormat format)
	{
		if (string.IsNullOrEmpty(text))
		{
			return SKRect.Empty;
		}
		FontExtension fontExtension = null;
		fontExtension = ((stream == null) ? new FontExtension(font.Name, font.Size, font.Style, m_pageUnit) : new FontExtension(stream, font.Name, font.Size, font.Style, m_pageUnit));
		SKRect bounds = default(SKRect);
		float right = fontExtension.MeasureText(text, ref bounds);
		float width = bounds.Width;
		fontExtension.MeasureText(text + "lg", ref bounds);
		if (IsRtfTextImageConversion)
		{
			char[] array = text.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == '、' || array[i] == '：')
				{
					return new SKRect(0f, bounds.Top, right, fontExtension.FontSpacing + bounds.Top);
				}
			}
		}
		if (((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) == 0 || (!text.StartsWith(" ") && !text.EndsWith(" "))) && (!isPPTXToPdfConversion || text.Length != 1))
		{
			return new SKRect(bounds.Left, bounds.Top, width + bounds.Left, fontExtension.FontSpacing + bounds.Top);
		}
		return new SKRect(0f, bounds.Top, right, fontExtension.FontSpacing + bounds.Top);
	}

	private TrueTypeFontStringFormat GetTtfStringFormat(StringFormat format)
	{
		TrueTypeFontStringFormat trueTypeFontStringFormat = new TrueTypeFontStringFormat();
		if (format != null)
		{
			switch (format.Alignment)
			{
			case StringAlignment.Far:
				trueTypeFontStringFormat.Alignment = TextAlignment.Right;
				break;
			case StringAlignment.Center:
				trueTypeFontStringFormat.Alignment = TextAlignment.Center;
				break;
			default:
				trueTypeFontStringFormat.Alignment = TextAlignment.Left;
				break;
			}
		}
		return trueTypeFontStringFormat;
	}

	internal SizeF MeasureString(string text, Font font, PointF pointF, StringFormat format)
	{
		SKRect sKRect = MeasureString(text, font, format);
		return new SizeF(sKRect.Width, sKRect.Height);
	}

	internal SizeF MeasureString(string text, Stream stream, Font font, PointF pointF, StringFormat format)
	{
		SKRect sKRect = MeasureString(text, stream, font, format);
		return new SizeF(sKRect.Width, sKRect.Height);
	}

	public static Graphics FromImage(Bitmap bmp)
	{
		return new Graphics(bmp);
	}

	public void FillRectangle(IBrush brush, Rectangle rectangle)
	{
		SKPaint sKPaint = new SKPaint();
		sKPaint.Color = Extension.GetSKColor(brush.Color);
		m_canvas.DrawRect(new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom), sKPaint);
	}

	public void FillRectangle(IBrush brush, RectangleF rectangle)
	{
		SKPaint sKPaint = new SKPaint();
		sKPaint.Color = Extension.GetSKColor(brush.Color);
		m_canvas.DrawRect(new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom), sKPaint);
	}

	public void FillRectangle(IBrush brush, float x, float y, float width, float height)
	{
		SKPaint sKPaint = new SKPaint();
		sKPaint.Color = Extension.GetSKColor(brush.Color);
		m_canvas.DrawRect(new SKRect(x, y, x + width, y + height), sKPaint);
	}

	internal void DrawRectangle(Pen pen, Rectangle rectangle)
	{
		DrawRectangle(pen, (RectangleF)rectangle);
	}

	public void FillRectangle(Color color, Rectangle rectangle)
	{
		SKPaint sKPaint = new SKPaint();
		sKPaint.Color = new SKColor(color.R, color.G, color.B, color.A);
		m_canvas.DrawRect(new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom), sKPaint);
	}

	internal void DrawRectangle(Pen pen, RectangleF rectangle)
	{
		if (pen.Alignment == PenAlignment.Inset)
		{
			float width = pen.Width;
			float num = width / 2f;
			rectangle = new RectangleF(rectangle.X + num, rectangle.Y + num, rectangle.Width - width, rectangle.Height - width);
		}
		m_canvas.DrawRect(RenderHelper.SKRect(rectangle), pen.SKPaint);
	}

	public static Graphics FromImage(Image result)
	{
		return new Graphics(result);
	}

	public void DrawLine(Pen pen, float x0, float y0, float x1, float y1)
	{
		m_canvas.DrawLine(x0, y0, x1, y1, pen.SKPaint);
	}

	public void DrawLine(IPen pen, PointF pt1, PointF pt2)
	{
		m_canvas.DrawLine(pt1.X, pt1.Y, pt2.X, pt2.Y, (pen as Pen).SKPaint);
	}

	public void TranslateTransform(float x, float y)
	{
		m_canvas.Translate(x, y);
	}

	public void RotateTransform(float degree)
	{
		m_canvas.RotateDegrees(degree);
	}

	internal void RotateAt(float degree)
	{
		m_canvas.RotateDegrees(degree);
	}

	internal void RotateAt(float degree, float x, float y)
	{
		m_canvas.RotateDegrees(degree, x, y);
	}

	internal void TransformPoints(PointF[] points)
	{
		for (int i = 0; i < points.Length; i++)
		{
			PointF pointF = points[i];
			m_canvas.Translate(pointF.X, pointF.Y);
		}
	}

	public void ExportAsImage(ImageFormat imageFormat, MemoryStream memoryStream)
	{
		SkSurface.Snapshot().Encode(GetImageFormat(imageFormat), 100).SaveTo(memoryStream);
	}

	internal SKEncodedImageFormat GetImageFormat(ImageFormat imageFormat)
	{
		return imageFormat switch
		{
			ImageFormat.Jpeg => SKEncodedImageFormat.Jpeg, 
			ImageFormat.Png => SKEncodedImageFormat.Png, 
			_ => SKEncodedImageFormat.Png, 
		};
	}

	internal void DrawBezier(Pen pen, PointF pf0, PointF pf1, PointF pf2, PointF pf3)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddBezier(pf0, pf1, pf2, pf3);
		m_canvas.DrawPath(graphicsPath, pen.SKPaint);
	}

	internal void DrawBezier(Pen pen, float f0, float f1, float f2, float f3, float f4, float f5, float f6, float f7)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddBezier(f0, f1, f2, f3, f4, f5, f6, f7);
		m_canvas.DrawPath(graphicsPath, pen.SKPaint);
	}

	internal void DrawBeziers(Pen pen, PointF[] points)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddBeziers(points);
		m_canvas.DrawPath(graphicsPath, pen.SKPaint);
	}

	internal void DrawEllipse(Pen pen, Rectangle rect)
	{
		m_canvas.DrawOval(RenderHelper.SKRect(rect), pen.SKPaint);
	}

	internal void FillEllipse(Brush brush, float x, float y, float width, float height)
	{
		m_canvas.DrawOval(RenderHelper.ImageRect(x, y, width, height), brush.SKPaint);
	}

	internal void FillEllipse(Brush brush, Rectangle rect)
	{
		m_canvas.DrawOval(RenderHelper.SKRect(rect), brush.SKPaint);
	}

	internal void DrawEllipse(Pen pen, float x, float y, float width, float height)
	{
		m_canvas.DrawOval(RenderHelper.ImageRect(x, y, width, height), pen.SKPaint);
	}

	internal void DrawLines(Pen pen, PointF[] points)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddLines(points);
		m_canvas.DrawPath(graphicsPath, pen.SKPaint);
	}

	internal void DrawLines(Pen pen, Point[] points)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddLines(points);
		m_canvas.DrawPath(graphicsPath, pen.SKPaint);
	}

	internal void DrawPolygon(Pen pen, PointF[] points)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddPolygon(points);
		m_canvas.DrawPath(graphicsPath, pen.SKPaint);
	}

	public void FillPolygon(IBrush pen, PointF[] points)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddPolygon(points);
		m_canvas.DrawPath(graphicsPath, (pen as Brush).SKPaint);
	}

	public void FillPath(IBrush brush, IGraphicsPath gPath)
	{
		m_canvas.DrawPath(gPath as GraphicsPath, (brush as Brush).SKPaint);
	}

	internal void DrawString(string text, Font font, Brush brush, PointF location)
	{
	}

	internal void DrawString(string text, Font font, Brush brush, float x, float y)
	{
		FontExtension fontExtension = new FontExtension(font.Name, font.Size, font.Style, m_pageUnit);
		fontExtension.Color = Extension.GetSKColor(brush.Color);
		SKRect sKRect = MeasureString(text, font, StringFormat.GenericDefault);
		x -= sKRect.Left;
		y -= sKRect.Top;
		m_canvas.DrawText(text, x, y, fontExtension);
	}

	public void DrawString(string text, Font font, IBrush brush, RectangleF rectangle)
	{
		new FontExtension(font.Name, font.Size, font.Style, m_pageUnit).Color = Extension.GetSKColor(brush.Color);
		List<Tuple<string, SKRect>> list = SplitText(text, font, rectangle.Width, StringFormat.GenericDefault);
		_ = rectangle.Width;
		float height = rectangle.Height;
		SetClip(rectangle);
		SKRect sKRect = MeasureString(text, font, StringFormat.GenericDefault);
		rectangle.Y += (sKRect.Height + sKRect.Top) / 2f;
		float num = rectangle.Y;
		float num2 = 0f;
		foreach (Tuple<string, SKRect> item in list)
		{
			if (num2 >= height)
			{
				break;
			}
			DrawString(item.Item1, font, brush as Brush, rectangle.X, num);
			num += item.Item2.Height;
			num2 += item.Item2.Height;
		}
		ResetClip();
	}

	internal void DrawImage(DocGen.Drawing.Image image, PointF[] points, Rectangle rect, GraphicsUnit unit, ImageAttributes imgattr)
	{
		GraphicsState state = Save();
		Image image2 = Image.FromImage(image);
		SetImageAttributes(image2, imgattr);
		RectangleF rectangleF = new RectangleF(0f, 0f, image.Width, image.Height);
		Matrix matrix = new Matrix(rectangleF, points);
		m_canvas.SetMatrix(matrix.GetSKMatrix());
		DrawImage(image2, rectangleF, rect, unit);
		Restore(state);
	}

	public void DrawImage(IImage image, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit)
	{
		m_canvas.DrawBitmap((image as Image).SKBitmap, RenderHelper.ImageRect(srcRect), RenderHelper.ImageRect(destRect), new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
	}

	public void DrawImage(IImage image, Rectangle destRect, float x, float y, float width, float height, GraphicsUnit unit, IImageAttributes attr)
	{
		SetImageAttributes(image as Image, attr as ImageAttributes);
		DrawImage(image as Bitmap, destRect, new RectangleF(x, y, width, height), unit);
	}

	internal void DrawImage(DocGen.Drawing.Image image, PointF[] points)
	{
		GraphicsState state = Save();
		Matrix matrix = new Matrix(new RectangleF(0f, 0f, image.Width, image.Height), points);
		m_canvas.SetMatrix(matrix.GetSKMatrix());
		m_canvas.DrawBitmap(Image.FromImage(image).SKBitmap, 0f, 0f, new SKPaint
		{
			FilterQuality = SKFilterQuality.High
		});
		Restore(state);
	}

	private static void SetImageAttributes(Image image, ImageAttributes attr)
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
			Bitmap bitmap = image as Bitmap;
			SKData data = bitmap.SKBitmap.Encode(SKEncodedImageFormat.Png, 100);
			image.SetSKBitmap(SKBitmap.Decode(data, new SKImageInfo(bitmap.SKBitmap.Width, bitmap.SKBitmap.Height)
			{
				AlphaType = SKAlphaType.Premul
			}));
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					Color pixel = bitmap.GetPixel(i, j);
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
					bitmap.SetPixel(i, j, Color.FromArgb(num4, num, num2, num3));
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
						if ((image as Bitmap).GetPixel(k, l).Equals(colorMap2.OldColor))
						{
							(image as Bitmap).SetPixel(k, l, colorMap2.NewColor);
						}
					}
				}
			}
		}
	}

	internal SizeF MeasureString(string text, Font font, SizeF pointF, StringFormat format)
	{
		return MeasureString(text, font, (int)pointF.Width, format);
	}

	public SizeF MeasureString(string text, Font font, int width)
	{
		return MeasureString(text, font, width, StringFormat.GenericDefault);
	}

	internal List<Tuple<string, SKRect>> SplitText(string text, Font font, float width, StringFormat format)
	{
		SKRect item = SKRect.Empty;
		List<Tuple<string, SKRect>> list = new List<Tuple<string, SKRect>>();
		SKRect item2 = MeasureString(text, font, format);
		if (item2.Width <= width)
		{
			list.Add(new Tuple<string, SKRect>(text, item2));
			return list;
		}
		string[] array = text.Split();
		foreach (string obj in array)
		{
			float width2 = 0f;
			string text2 = string.Empty;
			string text3 = obj;
			for (int j = 1; j <= text3.Length; j++)
			{
				text2 = text3.Substring(0, j);
				item = MeasureString(text2, font, format);
				if (item.Width > width)
				{
					item = SKRect.Create(width2, item.Height);
					if (j == 1 && text2.Length == 1)
					{
						list.Add(new Tuple<string, SKRect>(text2, item));
						text3 = text3.Substring(j);
					}
					else
					{
						list.Add(new Tuple<string, SKRect>(text3.Substring(0, j - 1), item));
						text3 = text3.Substring(j - 1);
					}
					j = 1;
					break;
				}
				width2 = item.Width;
			}
			if (!item.IsEmpty && text2.Length > 0)
			{
				list.Add(new Tuple<string, SKRect>(text2, item));
			}
		}
		return list;
	}

	internal SizeF MeasureString(string text, Font font, int width, StringFormat format)
	{
		SKRect sKRect = MeasureString(text, font, format);
		float num = sKRect.Width;
		if (num > (float)width)
		{
			num = 0f;
			foreach (Tuple<string, SKRect> item in SplitText(text, font, width, format))
			{
				if (num < item.Item2.Width)
				{
					num = item.Item2.Width;
				}
			}
		}
		return new SizeF(num, sKRect.Height);
	}

	internal SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled)
	{
		float width = layoutArea.Width;
		float height = layoutArea.Height;
		SKRect sKRect = MeasureString(text, font, stringFormat);
		charactersFitted = text.Length;
		linesFilled = 1;
		float num = sKRect.Width;
		float height2 = sKRect.Height;
		if (num > width)
		{
			num = 0f;
			height2 = 0f;
			int num2 = 0;
			int num3 = 0;
			foreach (Tuple<string, SKRect> item in SplitText(text, font, width, stringFormat))
			{
				if (height2 >= height)
				{
					break;
				}
				height2 += item.Item2.Height;
				if (num < item.Item2.Width)
				{
					num = item.Item2.Width;
				}
				num3 += item.Item1.Length;
				num2++;
			}
			charactersFitted = num3;
			linesFilled = num2;
		}
		return new SizeF(num, height);
	}

	public void SetClip(RectangleF rect, CombineMode mode)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(rect);
		SetClip(graphicsPath, mode);
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

	internal void SetClip(Rectangle rect)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(rect);
		SetClip(graphicsPath);
	}

	internal void SetClip(GraphicsPath path)
	{
		if (m_deafultClipIndex == -1)
		{
			m_deafultClipIndex = m_canvas.Save();
		}
		else
		{
			SKMatrix totalMatrix = m_canvas.TotalMatrix;
			m_canvas.RestoreToCount(m_deafultClipIndex);
			m_deafultClipIndex = m_canvas.Save();
			m_canvas.SetMatrix(totalMatrix);
		}
		m_canvas.ClipPath(path);
	}

	internal void SetClip(RectangleF rect)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(rect);
		SetClip(graphicsPath);
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

	internal void IntersectClip(RectangleF rect)
	{
		if (m_deafultClipIndex == -1)
		{
			m_deafultClipIndex = m_canvas.Save();
		}
		m_canvas.ClipRect(RenderHelper.SKRect(rect));
	}

	internal void IntersectClip(Rectangle rect)
	{
		if (m_deafultClipIndex == -1)
		{
			m_deafultClipIndex = m_canvas.Save();
		}
		m_canvas.ClipRect(RenderHelper.SKRect(rect));
	}

	internal GraphicsContainer BeginContainer()
	{
		return new GraphicsContainer(m_canvas.Save());
	}

	internal void EndContainer(GraphicsContainer container)
	{
		m_canvas.RestoreToCount(container.m_nativeGraphicsContainer);
	}

	internal void MultiplyTransform(Matrix matrix)
	{
		SKMatrix target = matrix.GetSKMatrix();
		SKMatrix.PostConcat(ref target, m_canvas.TotalMatrix);
		m_canvas.SetMatrix(target);
	}

	public void DrawRectangle(IPen p, float x, float y, float width, float height)
	{
		m_canvas.DrawRect(new SKRect(x, y, x + width, y + height), (p as Pen).SKPaint);
	}
}
