using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.ChartToImageConverter;
using DocGen.DocIO.DLS;
using DocGen.DocIO.Drawing;
using DocGen.DocIORenderer.Rendering;
using DocGen.DocToPdfConverter.Rendering;
using DocGen.Drawing;
using DocGen.Drawing.DocIOHelper;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Office;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.Pdf.Graphics;

namespace DocGen.DocIORenderer;

internal class RenderHelper : IHelper
{
	private bool m_isPdfConversion;

	private bool m_isEmbedFonts;

	private bool m_embedCompleteFonts;

	internal PDFDrawingContext m_pdfDrawingContext;

	internal bool IsPdfConversion
	{
		get
		{
			return m_isPdfConversion;
		}
		set
		{
			m_isPdfConversion = value;
		}
	}

	internal bool EmbedFonts
	{
		get
		{
			return m_isEmbedFonts;
		}
		set
		{
			m_isEmbedFonts = value;
		}
	}

	internal bool EmbedCompleteFonts
	{
		get
		{
			return m_embedCompleteFonts;
		}
		set
		{
			m_embedCompleteFonts = value;
		}
	}

	public RenderHelper()
	{
		m_pdfDrawingContext = new PDFDrawingContext(isEmpty: true);
	}

	public void CreateFont(Stream stream, string fontName, float fontSize, FontStyle fontStyle)
	{
		new FontExtension(stream, fontName, fontSize, fontStyle, GraphicsUnit.Point).Close();
	}

	public IFontFamily GetFontFamily(string name)
	{
		return new DocGen.Drawing.SkiaSharpHelper.FontFamily(name);
	}

	public IFontFamily GetFontFamily(string name, float fontSize)
	{
		return new DocGen.Drawing.SkiaSharpHelper.FontFamily(name, fontSize);
	}

	public Matrix MakeRotationDegrees(float angle, float x, float y)
	{
		return DocGen.Drawing.SkiaSharpHelper.Extension.MakeRotationDegrees(angle, x, y);
	}

	public Dictionary<string, float> ParseShapeFormula(DocGen.DocIO.DLS.AutoShapeType autoShapeType, Dictionary<string, string> shapeGuide, RectangleF bounds)
	{
		return new ShapePath().Get(bounds, shapeGuide).ParseShapeFormula(autoShapeType);
	}

	public float GetDescent(DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		FontExtension fontExtension = GetFontExtension(font.Name, font.Size, font.Style, scriptType);
		float cellDescent = fontExtension.GetCellDescent(font.Style);
		fontExtension.Close();
		return cellDescent;
	}

	public float GetLeading(DocGen.Drawing.Font font)
	{
		FontExtension fontExtension = GetFontExtension(font.Name, font.Size, font.Style, FontScriptType.English);
		float cellLeading = fontExtension.GetCellLeading(font.Style);
		fontExtension.Close();
		return cellLeading;
	}

	public float GetAscent(DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		FontExtension fontExtension = GetFontExtension(font.Name, font.Size, font.Style, scriptType);
		float cellAscent = fontExtension.GetCellAscent(font.Style);
		fontExtension.Close();
		return cellAscent;
	}

	public float GetLineSpacing(DocGen.Drawing.Font font)
	{
		FontExtension fontExtension = GetFontExtension(font.Name, font.Size, font.Style, FontScriptType.English);
		float lineSpacing = fontExtension.GetLineSpacing(font.Style);
		fontExtension.Close();
		return lineSpacing;
	}

	public float GetEmHeight(DocGen.Drawing.Font font)
	{
		FontExtension fontExtension = GetFontExtension(font.Name, font.Size, font.Style, FontScriptType.English);
		float emHeight = fontExtension.GetEmHeight(font.Style);
		fontExtension.Close();
		return emHeight;
	}

	public float GetFontHeight(DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		FontExtension fontExtension = GetFontExtension(font.Name, font.Size, font.Style, scriptType);
		float height = fontExtension.Height;
		fontExtension.Close();
		return height;
	}

	public Stream GetFontStream(DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		FontExtension fontExtension = GetFontExtension(font.Name, font.Size, font.Style, scriptType);
		fontExtension.FontStream.Position = 0L;
		Stream fontStream = fontExtension.FontStream;
		fontExtension.Close();
		return fontStream;
	}

	public string GetFontName(string fontName, float fontSize, FontStyle fontStyle, FontScriptType scriptType)
	{
		bool hasStylesAndWeights = true;
		return GetFontName(fontName, fontSize, fontStyle, scriptType, ref hasStylesAndWeights);
	}

	public string GetFontName(string fontName, float fontSize, FontStyle fontStyle, FontScriptType scriptType, ref bool hasStylesAndWeights)
	{
		FontExtension fontExtension = GetFontExtension(fontName, fontSize, fontStyle, scriptType, ref hasStylesAndWeights);
		string result = string.Empty;
		if (fontExtension.Typeface != null)
		{
			result = fontExtension.Typeface.FamilyName;
		}
		fontExtension.Close();
		return result;
	}

	public DocGen.Drawing.Font GetFallbackFont(DocGen.Drawing.Font font, string text, FontScriptType scriptType, List<FallbackFont> fallbackFonts, Dictionary<string, Stream> fontStreams)
	{
		if (IsPdfConversion)
		{
			bool flag = EmbedFonts || EmbedCompleteFonts || Encoding.UTF8.GetByteCount(text) != text.Length;
			PdfTrueTypeFont pdfTrueTypeFont = new PdfTrueTypeFont(GetFontStream(font, scriptType), font.Size, flag, string.Empty, m_pdfDrawingContext.GetFontStyle(font.Style));
			PdfFont fallbackPdfFont = m_pdfDrawingContext.GetFallbackPdfFont(pdfTrueTypeFont, font, text, scriptType, new PdfStringFormat(), fallbackFonts, fontStreams, flag, EmbedCompleteFonts);
			if (pdfTrueTypeFont.Name != fallbackPdfFont.Name)
			{
				return new DocGen.Drawing.Font(fallbackPdfFont.Name, font.Size, font.Style);
			}
		}
		else
		{
			FontExtension fontExtension = new FontExtension(font.Name, font.Size, font.Style, font.Unit, scriptType);
			if (fontExtension.Typeface != null)
			{
				FontExtension fallbackFontExt = GetFallbackFontExt(text, fontExtension, font, scriptType, fallbackFonts, fontStreams);
				if (fallbackFontExt.Typeface != null && fontExtension.Typeface.FamilyName != fallbackFontExt.Typeface.FamilyName)
				{
					font = new DocGen.Drawing.Font(fallbackFontExt.Typeface.FamilyName, font.Size, font.Style);
					fallbackFontExt.Close();
				}
			}
			fontExtension.Close();
		}
		return font;
	}

	public DocGen.Drawing.Font GetRegularStyleFontToMeasure(DocGen.Drawing.Font font, string text, FontScriptType scriptType)
	{
		if (IsPdfConversion && !IsFontContainGlyph(text, font, scriptType))
		{
			return new DocGen.Drawing.Font(font.Name, font.Size, FontStyle.Regular);
		}
		return font;
	}

	internal bool IsFontContainGlyph(string inputText, DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		FontExtension fontExtension = new FontExtension(font.Name, font.Size, font.Style, font.Unit, scriptType);
		bool result = fontExtension.ContainsGlyphs(inputText);
		fontExtension.Close();
		return result;
	}

	private FontExtension GetFallbackFontExt(string inputText, FontExtension originalFontExt, DocGen.Drawing.Font systemFont, FontScriptType scriptType, List<FallbackFont> fallbackFonts, Dictionary<string, Stream> fontStreams)
	{
		if (!string.IsNullOrEmpty(inputText) && !originalFontExt.IsContainGlyphs(inputText))
		{
			for (int i = 0; i < fallbackFonts.Count; i++)
			{
				FallbackFont fallbackFont = fallbackFonts[i];
				if (string.IsNullOrEmpty(fallbackFont.FontNames) || !fallbackFont.IsWithInRange(inputText))
				{
					continue;
				}
				string[] array = fallbackFont.FontNames.Split(',');
				for (int j = 0; j < array.Length; j++)
				{
					string text = array[j].Trim();
					FontExtension fontExtension = new FontExtension(text, systemFont.Size, systemFont.Style, systemFont.Unit, scriptType);
					if (fontExtension.Typeface != null && text != fontExtension.Typeface.FamilyName)
					{
						FontExtension fallBackFontFromSubstitutionOrEmbedded = GetFallBackFontFromSubstitutionOrEmbedded(text, scriptType, systemFont, fontStreams);
						if (fallBackFontFromSubstitutionOrEmbedded != null)
						{
							fontExtension = fallBackFontFromSubstitutionOrEmbedded;
						}
					}
					if (fontExtension.IsContainGlyphs(inputText))
					{
						return fontExtension;
					}
					fontExtension.Close();
				}
			}
		}
		return originalFontExt;
	}

	private FontExtension GetFallBackFontFromSubstitutionOrEmbedded(string fallbackFontName, FontScriptType scriptType, DocGen.Drawing.Font systemFont, Dictionary<string, Stream> fontStreams)
	{
		if (fontStreams != null && fontStreams.Count > 0)
		{
			Stream stream = null;
			string text = fallbackFontName + "_" + systemFont.Style.ToString().Replace(", ", "");
			if (fontStreams.ContainsKey(text))
			{
				stream = fontStreams[text];
			}
			else if (fontStreams.ContainsKey(text.ToLower()))
			{
				stream = fontStreams[text.ToLower()];
			}
			else if (fontStreams.ContainsKey(fallbackFontName))
			{
				stream = fontStreams[fallbackFontName];
			}
			if (stream != null && stream.CanRead)
			{
				stream.Position = 0L;
				return new FontExtension(stream, fallbackFontName, systemFont.Size, systemFont.Style, systemFont.Unit, scriptType);
			}
		}
		return null;
	}

	public string GetUnicodeFamilyName(string text, string fontName)
	{
		return DocGen.Drawing.SkiaSharpHelper.Extension.GetUnicodeFamilyName(text, fontName);
	}

	public IBitmap GetBitmap()
	{
		return new DocGen.Drawing.SkiaSharpHelper.Bitmap();
	}

	public IBitmap GetBitmap(int width, int height)
	{
		return new DocGen.Drawing.SkiaSharpHelper.Bitmap(width, height);
	}

	public IGraphics GetGraphics(IImage image)
	{
		return new Graphics(image as DocGen.Drawing.SkiaSharpHelper.Image);
	}

	public ISolidBrush GetSolidBrush(Color color)
	{
		return new SolidBrush(color);
	}

	public ITextureBrush GetTextureBrush(IImage image, RectangleF bounds, IImageAttributes imageAttributes)
	{
		TextureBrush textureBrush = new TextureBrush(image as DocGen.Drawing.SkiaSharpHelper.Image, bounds, imageAttributes as DocGen.Drawing.SkiaSharpHelper.ImageAttributes);
		textureBrush.TranslateTransform(bounds.X, bounds.Y);
		textureBrush.WrapMode = WrapMode.Clamp;
		return textureBrush;
	}

	public IHatchBrush GetHatchBrush(HatchStyle hatchstyle, Color foreColor, Color backColor)
	{
		return new HatchBrush(hatchstyle, foreColor, backColor);
	}

	public IGraphicsPath GetGraphicsPath()
	{
		return new DocGen.Drawing.SkiaSharpHelper.GraphicsPath();
	}

	public IPen GetPen(Color color)
	{
		return new Pen(color);
	}

	public IPen GetPen(Color color, float width)
	{
		return new Pen(color, width);
	}

	public void ApplyScale(Matrix matrix, float x, float y)
	{
		matrix.Scale(x, y);
	}

	public void MultiplyMatrix(Matrix srcMatrix, Matrix matrix, MatrixOrder order)
	{
		srcMatrix.Multiply(matrix, order);
	}

	public void TranslateMatrix(Matrix matrix, float offsetX, float offsetY, MatrixOrder order)
	{
		matrix.Translate(offsetX, offsetY, order);
	}

	public void RotateMatrix(Matrix matrix, float angle, PointF point, MatrixOrder order)
	{
		matrix.RotateAt(angle, point, order);
	}

	public void RotateMatrix(Matrix matrix, float angle)
	{
		matrix.Rotate(angle);
	}

	public IImageAttributes GetImageAttributes()
	{
		return new DocGen.Drawing.SkiaSharpHelper.ImageAttributes();
	}

	public IColorMatrix GetColorMatrix(float[][] newColorMatrix)
	{
		return new DocGen.Drawing.SkiaSharpHelper.ColorMatrix(newColorMatrix);
	}

	public IColorMatrix GetColorMatrix()
	{
		return new DocGen.Drawing.SkiaSharpHelper.ColorMatrix();
	}

	public IImage CreateImageFromStream(MemoryStream stream)
	{
		return DocGen.Drawing.SkiaSharpHelper.Image.FromStream(stream);
	}

	public bool HasBitmap(IImage image)
	{
		return (image as DocGen.Drawing.SkiaSharpHelper.Image).SKBitmap != null;
	}

	public byte[] ConvertTiffToPng(byte[] imageBytes)
	{
		MemoryStream memoryStream = new MemoryStream(imageBytes);
		MemoryStream memoryStream2 = TiffImageConverter.ConvertToPng(memoryStream);
		byte[] result = memoryStream2.ToArray();
		memoryStream2.Dispose();
		memoryStream.Dispose();
		return result;
	}

	public void ConvertChartAsImage(IOfficeChart officeChart, Stream imageAsStream, ChartRenderingOptions imageOptions)
	{
		new ChartToImageconverter().SaveAsImage(officeChart, imageAsStream, imageOptions);
		DrawChartShape(officeChart, imageAsStream);
	}

	internal void DrawChartShape(IOfficeChart officeChart, Stream imageAsStream)
	{
		int num = 0;
		int num2 = 0;
		ChartImpl chartImpl = ((officeChart is ChartImpl) ? (officeChart as ChartImpl) : (officeChart as ChartShapeImpl).ChartObject);
		if (!(officeChart is ChartShapeImpl))
		{
			num = (int)officeChart.Width;
			num2 = (int)officeChart.Height;
		}
		else
		{
			num = (officeChart as ChartShapeImpl).Width;
			num2 = (officeChart as ChartShapeImpl).Height;
		}
		if (chartImpl != null && chartImpl.Shapes.Count > 0)
		{
			new ConvertChartShapes(chartImpl.ParentWorkbook, chartImpl).DrawChartShapes(imageAsStream, num, num2);
		}
	}

	internal static void ClearTypeFaceCache(WordDocument document)
	{
		new FontExtension("Times New Roman", 12f, FontStyle.Regular, GraphicsUnit.Point).Dispose();
		document.FontSettings.FontStreams.Clear();
	}

	public IFontExtension GetFontExtension(string fontName, float fontSize, FontStyle fontStyle, GraphicsUnit graphicsUnit, FontScriptType scriptType)
	{
		return new FontExtension(fontName, fontSize, fontStyle, graphicsUnit, scriptType);
	}

	public bool IsValidFontStream(Stream fontStream)
	{
		try
		{
			new PdfTrueTypeFont(fontStream, 11f);
			return true;
		}
		catch (Exception ex)
		{
			if (ex.Message == "Can't read TTF font data")
			{
				return false;
			}
			return true;
		}
	}

	private FontExtension GetFontExtension(string fontName, float fontSize, FontStyle fontStyle, FontScriptType scriptType)
	{
		bool hasStylesAndWeights = true;
		return GetFontExtension(fontName, fontSize, fontStyle, scriptType, ref hasStylesAndWeights);
	}

	private FontExtension GetFontExtension(string fontName, float fontSize, FontStyle fontStyle, FontScriptType scriptType, ref bool hasStylesAndWeights)
	{
		return new FontExtension(fontName, fontSize, fontStyle, GraphicsUnit.Point, scriptType, ref hasStylesAndWeights);
	}
}
