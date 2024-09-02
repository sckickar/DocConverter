using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.PdfViewer.Base;

internal class TextElementHelperNet
{
	internal float TextHorizontalScaling = 100f;

	private SystemFontOpenTypeFontSource openTypeFontSource;

	private string[] names;

	private SystemFontFontsManager systemFontsManager;

	private CharCode? firstCode;

	internal static StdFontsAssistant manager = new StdFontsAssistant();

	internal Matrix documentMatrix;

	internal int Rise;

	internal Matrix textLineMatrix;

	internal Matrix Ctm;

	internal Matrix transformMatrix;

	private FontSource FontSource;

	private TransformationStack transformations;

	internal string FontID = string.Empty;

	private bool m_isMpdfFont;

	internal static Dictionary<string, FontSource> fontSourceCache = new Dictionary<string, FontSource>();

	internal Dictionary<string, double> ReverseMapTable = new Dictionary<string, double>();

	internal bool IsType1Font;

	internal bool Is1C;

	internal CffGlyphs m_cffGlyphs = new CffGlyphs();

	internal Dictionary<string, byte[]> m_type1FontGlyphs = new Dictionary<string, byte[]>();

	internal Font textFont;

	internal string renderedText = string.Empty;

	private float CharSizeMultiplier = 0.001f;

	internal string m_fontName;

	internal FontStyle m_fontStyle;

	internal float m_fontSize;

	internal string m_fontEncoding;

	internal string m_text;

	private bool m_spaceCheck;

	private GraphicsPath pathGeom = new GraphicsPath();

	internal PdfBrush m_pathBrush;

	internal PdfBrush m_pathNonStrokeBrush;

	internal SKPaint skpaint;

	internal GraphicsPath GlyfDatapath;

	internal float m_wordSpacing;

	internal float m_characterSpacing;

	internal float m_textScaling = 100f;

	internal int m_renderingMode;

	private Font m_font;

	internal bool isNegativeFont;

	private static Dictionary<string, string> fontList = new Dictionary<string, string>();

	private PdfViewerExceptions exceptions = new PdfViewerExceptions();

	internal Dictionary<int, int> FontGlyphWidths;

	internal float DefaultGlyphWidth;

	internal bool IsTransparentText;

	internal bool IsCID;

	internal bool IsFindText;

	internal bool IsPdfium;

	internal Dictionary<double, string> CharacterMapTable;

	internal Dictionary<int, string> differenceTable = new Dictionary<int, string>();

	internal Dictionary<string, string> differenceMappedTable = new Dictionary<string, string>();

	internal Dictionary<int, int> OctDecMapTable;

	internal Dictionary<int, int> EncodedTextBytes;

	internal Dictionary<int, int> CidToGidReverseMapTable;

	internal Dictionary<int, string> UnicodeCharMapTable;

	internal FontFile2 Fontfile2Glyph;

	internal float Textscalingfactor;

	internal FontStructureHelperNet structure;

	internal bool IsContainFontfile2;

	public float currentGlyphWidth;

	internal bool Isembeddedfont;

	internal int FontFlag;

	internal float LineWidth;

	internal Dictionary<SystemFontFontDescriptor, SystemFontOpenTypeFontSource> testdict;

	internal ImageHelper type3GlyphImage;

	internal Matrix type3TextMatrix;

	internal List<Glyph> textElementGlyphList;

	internal float pageRotation;

	internal float zoomFactor = 1f;

	internal List<object> htmldata;

	internal Dictionary<string, string> SubstitutedFontsList = new Dictionary<string, string>();

	private readonly object fontResourceLocker = new object();

	private bool m_isExtractTextData;

	private string m_embeddedFontFamily;

	internal bool m_isRectation;

	private int dpiY = 96;

	private long[] MacRomanToUnicode = new long[128]
	{
		196L, 197L, 199L, 201L, 209L, 214L, 220L, 225L, 224L, 226L,
		228L, 227L, 229L, 231L, 233L, 232L, 234L, 235L, 237L, 236L,
		238L, 239L, 241L, 243L, 242L, 244L, 246L, 245L, 250L, 249L,
		251L, 252L, 8224L, 176L, 162L, 163L, 167L, 8226L, 182L, 223L,
		174L, 169L, 8482L, 180L, 168L, 8800L, 198L, 216L, 8734L, 177L,
		8804L, 8805L, 165L, 181L, 8706L, 8721L, 8719L, 960L, 8747L, 170L,
		186L, 937L, 230L, 248L, 191L, 161L, 172L, 8730L, 402L, 8776L,
		8710L, 171L, 187L, 8230L, 160L, 192L, 195L, 213L, 338L, 339L,
		8211L, 8212L, 8220L, 8221L, 8216L, 8217L, 247L, 9674L, 255L, 376L,
		8260L, 8364L, 8249L, 8250L, 64257L, 64258L, 8225L, 183L, 8218L, 8222L,
		8240L, 194L, 202L, 193L, 203L, 200L, 205L, 206L, 207L, 204L,
		211L, 212L, 63743L, 210L, 218L, 219L, 217L, 305L, 710L, 732L,
		175L, 728L, 729L, 730L, 184L, 733L, 731L, 711L
	};

	private string m_zapfPostScript;

	internal Color m_brushColor;

	private static readonly object m_locker = new object();

	private Dictionary<int, string> m_macEncodeTable;

	internal bool IsExtractTextData
	{
		get
		{
			return m_isExtractTextData;
		}
		set
		{
			m_isExtractTextData = value;
		}
	}

	internal string FontName
	{
		get
		{
			return m_fontName;
		}
		set
		{
			m_fontName = value;
		}
	}

	internal Font Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}

	internal CharCode CharID { get; set; }

	internal FontStyle FontStyle
	{
		get
		{
			return m_fontStyle;
		}
		set
		{
			m_fontStyle = value;
		}
	}

	internal float FontSize
	{
		get
		{
			return m_fontSize;
		}
		set
		{
			m_fontSize = value;
		}
	}

	public string ZapfPostScript
	{
		get
		{
			return m_zapfPostScript;
		}
		set
		{
			m_zapfPostScript = value;
		}
	}

	internal string FontEncoding
	{
		get
		{
			return m_fontEncoding;
		}
		set
		{
			m_fontEncoding = value;
		}
	}

	internal string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal PdfBrush PathBrush
	{
		get
		{
			return m_pathBrush;
		}
		set
		{
			m_pathBrush = value;
		}
	}

	internal PdfBrush PathNonStrokeBrush
	{
		get
		{
			return m_pathNonStrokeBrush;
		}
		set
		{
			m_pathNonStrokeBrush = value;
		}
	}

	internal float WordSpacing
	{
		get
		{
			return m_wordSpacing;
		}
		set
		{
			m_wordSpacing = value;
		}
	}

	internal float CharacterSpacing
	{
		get
		{
			return m_characterSpacing;
		}
		set
		{
			m_characterSpacing = value;
		}
	}

	internal float TextScaling
	{
		get
		{
			return m_textScaling;
		}
		set
		{
			m_textScaling = value;
		}
	}

	internal int RenderingMode
	{
		get
		{
			return m_renderingMode;
		}
		set
		{
			m_renderingMode = value;
		}
	}

	internal Color BrushColor
	{
		get
		{
			return m_brushColor;
		}
		set
		{
			m_brushColor = value;
		}
	}

	internal bool IsNonsymbolic => GetFlag(6);

	private bool IsTextGlyphAdded { get; set; }

	internal SystemFontFontsManager SystemFontsManager
	{
		get
		{
			if (systemFontsManager == null)
			{
				systemFontsManager = new SystemFontFontsManager();
			}
			return systemFontsManager;
		}
	}

	internal TextElementHelperNet(string text)
	{
		m_text = text;
	}

	internal TextElementHelperNet(string text, Matrix transformMatrix)
	{
		m_text = text;
		transformations = new TransformationStack();
		if (transformations != null)
		{
			transformations.Clear();
		}
		transformations = new TransformationStack(transformMatrix);
		textElementGlyphList = new List<Glyph>();
	}

	internal TextElementHelperNet(ImageHelper img, Matrix transformMatrix)
	{
		type3GlyphImage = img;
		transformations = new TransformationStack();
		if (transformations != null)
		{
			transformations.Clear();
		}
		transformations = new TransformationStack(transformMatrix);
		textElementGlyphList = new List<Glyph>();
	}

	private Matrix GetTextRenderingMatrix()
	{
		Matrix matrix = default(Matrix);
		matrix.M11 = FontSize * (TextHorizontalScaling / 100f);
		matrix.M22 = 0f - FontSize;
		matrix.OffsetY = FontSize + (float)Rise;
		return matrix * textLineMatrix * Ctm;
	}

	private string[] GetStandardFontEncodingNames()
	{
		if (structure.FontEncoding != "MacRomanEncoding")
		{
			return PredefinedTextEncoding.GetPredefinedEncoding("WinAnsiEncoding").GetNames();
		}
		return PredefinedTextEncoding.GetPredefinedEncoding("MacRomanEncoding").GetNames();
	}

	public virtual double GetGlyphWidth(Glyph glyph)
	{
		int intValue = glyph.CharId.IntValue;
		if (FontGlyphWidths != null && structure.fontType.Value == "TrueType" && FontGlyphWidths.ContainsKey(intValue))
		{
			double advancedWidth = (float)FontGlyphWidths[intValue] * CharSizeMultiplier;
			glyph.AdvancedWidth = advancedWidth;
		}
		else if (FontSource != null)
		{
			FontSource.GetAdvancedWidth(glyph);
		}
		else
		{
			glyph.AdvancedWidth = 1.0;
		}
		return glyph.AdvancedWidth;
	}

	private double GetSystemFontGlyphWidth(Glyph glyph, GraphicsHelper g)
	{
		int intValue = glyph.CharId.IntValue;
		if (FontGlyphWidths != null && structure.fontType.Value == "TrueType" && FontGlyphWidths.ContainsKey(intValue))
		{
			double advancedWidth = (float)FontGlyphWidths[intValue] * CharSizeMultiplier;
			glyph.AdvancedWidth = advancedWidth;
		}
		else
		{
			if (FontSource == null)
			{
				return -1.0;
			}
			FontSource.GetAdvancedWidth(glyph);
		}
		return glyph.AdvancedWidth;
	}

	private string GetGlyphName(Glyph glyph)
	{
		if (FontSource != null)
		{
			FontSource.GetGlyphName(glyph);
		}
		return glyph.Name;
	}

	private void GlyphToSLCoordinates(Glyph glyph)
	{
		Matrix matrix = glyph.TransformMatrix;
		matrix.Translate(0.0, (0.0 - glyph.Ascent) / 1000.0);
		glyph.TransformMatrix = matrix;
	}

	private DocGen.Drawing.Matrix GetTransformationMatrix(Matrix transform)
	{
		return new DocGen.Drawing.Matrix((float)transform.M11, (float)transform.M12, (float)transform.M21, (float)transform.M22, (float)transform.OffsetX, (float)transform.OffsetY);
	}

	private void UpdateTextMatrix(double tj)
	{
		double x = 0.0 - tj * 0.001 * (double)FontSize * (double)TextHorizontalScaling / 100.0;
		Point point = textLineMatrix.Transform(new Point(0.0, 0.0));
		Point point2 = textLineMatrix.Transform(new Point(x, 0.0));
		if (point.X != point2.X)
		{
			textLineMatrix.OffsetX = point2.X;
		}
		else
		{
			textLineMatrix.OffsetY = point2.Y;
		}
	}

	private void UpdateTextMatrix(Glyph glyph)
	{
		textLineMatrix = CalculateTextMatrix(textLineMatrix, glyph);
	}

	private Matrix CalculateTextMatrix(Matrix m, Glyph glyph)
	{
		if (glyph.CharId.IntValue == 32)
		{
			glyph.WordSpacing = WordSpacing;
		}
		double offsetX = (glyph.Width * glyph.FontSize + glyph.CharSpacing + glyph.WordSpacing) * (glyph.HorizontalScaling / 100.0);
		return new Matrix(1.0, 0.0, 0.0, 1.0, offsetX, 0.0) * m;
	}

	internal float Render(GraphicsHelper g, PointF currentLocation, double textScaling, Dictionary<int, int> gWidths, double type1Height, Dictionary<int, string> differenceTable, Dictionary<string, string> differenceMappedTable, Dictionary<int, string> differenceEncoding, out Matrix txtMatrix)
	{
		Monitor.Enter(fontResourceLocker);
		try
		{
			m_isMpdfFont = isMpdfaaFonts();
			if (type3GlyphImage != null)
			{
				txtMatrix = default(Matrix);
				DrawType3Glyphs(type3GlyphImage, g);
				return 0f;
			}
			txtMatrix = Matrix.Identity;
			string fontName = FontName;
			renderedText = string.Empty;
			_ = FontName;
			int num = 0;
			string[] array = null;
			textFont = null;
			PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
			float x = currentLocation.X;
			if (Font != null && Isembeddedfont)
			{
				BackupEmbededFontName(fontName);
				FontName = CheckFontName(Font.Name);
				textFont = Font;
			}
			else
			{
				CheckFontStyle(FontName);
				FontName = CheckFontName(FontName);
				if (FontSize < 0f)
				{
					textFont = new Font(FontName, 0f - FontSize, FontStyle);
				}
				else
				{
					textFont = new Font(FontName, FontSize, FontStyle);
				}
			}
			string[] array2 = Text.Split(' ');
			PointF newLocation = currentLocation;
			if (IsTransparentText)
			{
				PathBrush = new PdfPen(Color.Transparent).Brush;
			}
			double num2 = FontSize;
			if (!IsType1Font && m_fontEncoding != "MacRomanEncoding" && m_fontEncoding != "WinAnsiEncoding" && !isNegativeFont)
			{
				string text = array2[0];
				for (int i = 1; i < array2.Length; i++)
				{
					text += " ";
					text += array2[i];
				}
				array2 = new string[1] { text };
			}
			fontName = ResolveFontName(fontName);
			if (StdFontsAssistant.IsStandardFontName(fontName) && !Isembeddedfont)
			{
				if (m_fontStyle == FontStyle.Bold && !fontName.Contains("Bold"))
				{
					fontName = ((!StdFontsAssistant.IsAlternativeStdFontAvailable(fontName)) ? (fontName + "-Bold") : (fontName + ",Bold"));
				}
				if (m_fontStyle == FontStyle.Italic && !fontName.Contains("Italic"))
				{
					fontName = (StdFontsAssistant.IsAlternativeStdFontAvailable(fontName) ? (fontName + ",Italic") : ((fontName.Contains("Courier") || fontName.Contains("Helvetica")) ? (fontName + "-Oblique") : (fontName + "-Italic")));
				}
				if (m_fontStyle == (FontStyle)3 && !fontName.Contains("Italic") && !fontName.Contains("Bold"))
				{
					if (StdFontsAssistant.IsAlternativeStdFontAvailable(fontName))
					{
						fontName += ",Bold";
						fontName += "Italic";
					}
					else if (!fontName.Contains("Courier") && !fontName.Contains("Helvetica"))
					{
						fontName += "-Bold";
						fontName += "Italic";
					}
					else
					{
						fontName += "-Bold";
						fontName += "Oblique";
					}
				}
				GraphicsUnit pageUnit = g.PageUnit;
				DocGen.Drawing.Matrix transform = g.Transform;
				g.PageUnit = GraphicsUnit.Pixel;
				if (!fontSourceCache.ContainsKey(FontID + structure.FontRefNumber))
				{
					FontSource = manager.GetStandardFontSource(fontName);
					fontSourceCache.Add(FontID + structure.FontRefNumber, FontSource);
				}
				else
				{
					FontSource = fontSourceCache[FontID + structure.FontRefNumber];
				}
				if (ZapfPostScript != null)
				{
					char[] separator = new char[1] { ' ' };
					array = ZapfPostScript.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				}
				string text2 = Text;
				for (int num3 = 0; num3 < text2.Length; num3++)
				{
					char c = text2[num3];
					g.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
					Glyph glyph = new Glyph();
					glyph.FontSize = FontSize;
					glyph.FontFamily = FontName;
					glyph.FontStyle = FontStyle;
					glyph.TransformMatrix = GetTextRenderingMatrix();
					glyph.Name = c.ToString();
					glyph.HorizontalScaling = TextHorizontalScaling;
					glyph.CharId = new CharCode((int)c);
					glyph.CharSpacing = CharacterSpacing;
					string[] standardFontEncodingNames = GetStandardFontEncodingNames();
					byte[] bytes = Encoding.UTF8.GetBytes(c.ToString());
					if (structure.ReverseDictMapping.ContainsKey(c.ToString()))
					{
						float num4 = structure.ReverseDictMapping[c.ToString()];
						if (structure.DifferencesDictionary.ContainsKey(num4.ToString()))
						{
							glyph.Name = FontStructure.GetCharCode(structure.DifferencesDictionary[num4.ToString()]);
						}
						else
						{
							bytes[0] = (byte)num4;
							glyph.Name = standardFontEncodingNames[bytes[0]];
						}
					}
					else if (OctDecMapTable != null && OctDecMapTable.ContainsKey(c) && structure.FontName != "Symbol")
					{
						char c2 = (char)OctDecMapTable[c];
						glyph.Name = standardFontEncodingNames[(uint)c2];
					}
					else
					{
						if (structure.DifferencesDictionary != null)
						{
							Dictionary<string, string> differencesDictionary = structure.DifferencesDictionary;
							int num5 = c;
							if (differencesDictionary.ContainsKey(num5.ToString()) && structure.BaseFontEncoding != "WinAnsiEncoding")
							{
								Dictionary<string, string> differencesDictionary2 = structure.DifferencesDictionary;
								num5 = c;
								glyph.Name = FontStructure.GetCharCode(differencesDictionary2[num5.ToString()]);
								goto IL_06d7;
							}
						}
						if (standardFontEncodingNames.Length > c && structure.FontName != "Symbol" && structure.FontName != "ZapfDingbats")
						{
							glyph.Name = standardFontEncodingNames[(uint)c];
						}
						else if (structure.FontName == "Symbol")
						{
							if (structure.FontEncoding == "Encoding")
							{
								glyph.Name = FontStructure.GetCharCode(c.ToString());
							}
							else
							{
								glyph.Name = GetGlyphName(glyph);
							}
						}
						else if (structure.FontName == "ZapfDingbats")
						{
							if (num < ZapfPostScript.Length)
							{
								glyph.Name = array[num].Trim();
								num++;
							}
						}
						else
						{
							glyph.Name = standardFontEncodingNames[bytes[0]];
						}
					}
					goto IL_06d7;
					IL_06d7:
					if (PdfDocument.EnableThreadSafe)
					{
						lock (m_locker)
						{
							glyph.Width = GetGlyphWidth(glyph);
						}
					}
					else
					{
						glyph.Width = GetGlyphWidth(glyph);
					}
					FontSource.GetGlyphOutlines(glyph, 100.0);
					new PdfElementsRendererNet().RenderGlyph(glyph);
					Matrix identity = Matrix.Identity;
					identity.Scale(0.01, 0.01, 0.0, 0.0);
					identity.Translate(0.0, 1.0);
					transformations.PushTransform(identity * glyph.TransformMatrix);
					DocGen.Drawing.Matrix matrix = (DocGen.Drawing.Matrix)g.Transform.Clone();
					matrix.Multiply(GetTransformationMatrix(transformations.CurrentTransform));
					g.Transform = matrix;
					g.SmoothingMode = SmoothingMode.AntiAlias;
					float num6 = 0f;
					if (glyph.TransformMatrix.M11 > 0.0)
					{
						num6 = (float)glyph.TransformMatrix.M11;
					}
					else if (glyph.TransformMatrix.M12 != 0.0 && glyph.TransformMatrix.M21 != 0.0)
					{
						num6 = ((!(glyph.TransformMatrix.M12 < 0.0)) ? ((float)glyph.TransformMatrix.M12) : ((float)(0.0 - glyph.TransformMatrix.M12)));
					}
					else if (glyph.TransformMatrix.M11 == 0.0)
					{
						num6 = ((!(glyph.FontSize > 0.0)) ? 0f : ((float)glyph.FontSize));
					}
					else if (glyph.TransformMatrix.M11 < 0.0)
					{
						num6 = 0f - (float)glyph.TransformMatrix.M11;
					}
					if ((int)num6 == 0)
					{
						num6 = (int)glyph.FontSize;
					}
					string text3 = c.ToString();
					if (!structure.IsMappingDone)
					{
						if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.ContainsKey(Convert.ToChar(text3)) && structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
						{
							text3 = CharacterMapTable[CidToGidReverseMapTable[Convert.ToChar(text3)]];
						}
						else if (structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
						{
							text3 = ((structure.tempStringList.Count <= 0) ? structure.MapCharactersFromTable(text3.ToString()) : structure.CharacterMapTable[(int)Convert.ToChar(text3)]);
						}
						else if (structure.DifferencesDictionary != null && structure.DifferencesDictionary.Count > 0)
						{
							text3 = structure.MapDifferences(text3.ToString());
						}
						else if (structure.CidToGidReverseMapTable != null && structure.CidToGidReverseMapTable.ContainsKey(Convert.ToChar(text3)))
						{
							text3 = ((char)structure.CidToGidReverseMapTable[Convert.ToChar(text3)]).ToString();
						}
						if (text3.Contains("\u0092"))
						{
							text3 = text3.Replace("\u0092", "’");
						}
					}
					glyph.ToUnicode = text3;
					if (pageRotation == 90f || pageRotation == 270f)
					{
						if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
						{
							glyph.IsRotated = false;
							glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num6 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num6, num6));
						}
						else
						{
							glyph.IsRotated = true;
							if (IsFindText && pageRotation == 90f)
							{
								glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels(matrix.OffsetX + (num6 + (float)(glyph.Ascent / 1000.0)) * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConvertor.ConvertFromPixels(matrix.OffsetY - num6 * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor), new Size(num6, glyph.Width * (double)num6));
							}
							else
							{
								glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels(matrix.OffsetX + (num6 + (float)(glyph.Ascent / 1000.0)) * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConvertor.ConvertFromPixels(matrix.OffsetY - num6 * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor), new Size(num6, glyph.Width * (double)num6));
							}
						}
					}
					else
					{
						if (pageRotation == 180f)
						{
							if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
							{
								glyph.IsRotated = true;
							}
							else
							{
								glyph.IsRotated = false;
							}
						}
						float num7 = 0f;
						if (glyph.TransformMatrix.M12 != 0.0 && glyph.TransformMatrix.M21 != 0.0)
						{
							num7 = ((!(glyph.TransformMatrix.M12 < 0.0)) ? ((float)glyph.TransformMatrix.M12) : ((float)(0.0 - glyph.TransformMatrix.M12)));
						}
						else if (glyph.TransformMatrix.M11 != 0.0 && glyph.FontSize <= 1.0)
						{
							num7 = (float)Math.Abs(glyph.TransformMatrix.M11);
						}
						if ((int)num7 == 0)
						{
							num7 = ((num6 == 0f) ? ((float)(int)glyph.FontSize) : ((float)(int)num6));
						}
						if (Math.Round(Math.Atan2(glyph.TransformMatrix.M21, glyph.TransformMatrix.M11) * 180.0 / Math.PI) == -90.0)
						{
							glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConvertor.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
						}
						else
						{
							glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConvertor.ConvertFromPixels(matrix.OffsetY - num6 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
						}
					}
					if (structure.CharacterMapTable != null && structure.CharacterMapTable.ContainsKey((int)c))
					{
						glyph.ToUnicode = structure.CharacterMapTable[(int)c];
					}
					if (glyph.ToUnicode.Length != 1)
					{
						textElementGlyphList.Add(glyph);
						for (int j = 0; j < glyph.ToUnicode.Length - 1; j++)
						{
							Glyph item = new Glyph();
							textElementGlyphList.Add(item);
						}
					}
					else
					{
						textElementGlyphList.Add(glyph);
					}
					if (m_isExtractTextData && glyph.FontSize != (double)num6)
					{
						glyph.MatrixFontSize = num6;
					}
					UpdateTextMatrix(glyph);
					transformations.PopTransform();
					if (structure.CharacterMapTable != null && structure.CharacterMapTable.ContainsKey((int)c))
					{
						renderedText += glyph.ToString();
					}
					else
					{
						renderedText += c;
					}
				}
				g.Transform = transform;
				g.PageUnit = pageUnit;
				txtMatrix = textLineMatrix;
			}
			else
			{
				bool flag = false;
				float num8 = 0f;
				int num9 = 0;
				string text4 = string.Empty;
				for (int k = 0; k < Text.Length; k++)
				{
					char c3 = Text[k];
					flag = false;
					if (IsType1Font && !structure.IsOpenTypeFont && differenceMappedTable.Count == 0)
					{
						text4 = Text[k].ToString();
						int num10 = 0;
						if (ReverseMapTable.ContainsKey(text4))
						{
							foreach (KeyValuePair<double, string> item2 in CharacterMapTable)
							{
								if (item2.Value.IndexOf(c3) > -1)
								{
									num10++;
								}
							}
						}
						if ((!ReverseMapTable.ContainsKey(text4) || num10 > 1) && (char.IsLetter(c3) || char.IsPunctuation(c3) || char.IsSymbol(c3)))
						{
							num10 = 0;
							if (k != Text.Length - 1)
							{
								text4 = Text.Substring(k);
								k += text4.Length - 1;
							}
						}
						if (text4 != c3.ToString())
						{
							for (int l = 0; l < text4.Length; l++)
							{
								if (!ReverseMapTable.ContainsKey(text4))
								{
									text4 = text4.Remove(text4.Length - 1);
									k--;
									l = 0;
									if (text4 == Text[k].ToString())
									{
										text4 = string.Empty;
									}
									if (ReverseMapTable.ContainsKey(text4))
									{
										break;
									}
								}
							}
						}
						else
						{
							text4 = string.Empty;
						}
					}
					new Glyph();
					StringFormat format = new StringFormat(StringFormat.GenericDefault);
					num9++;
					if (IsType1Font && !structure.IsOpenTypeFont)
					{
						string text5 = c3.ToString();
						int num11 = c3;
						if (structure.DifferencesDictionary.ContainsValue(Text))
						{
							text5 = Text;
							k = Text.Length - 1;
						}
						if (EncodedTextBytes != null && ReverseMapTable.Count == CharacterMapTable.Count && OctDecMapTable.Count == 0 && k < EncodedTextBytes.Count && differenceMappedTable != null && differenceMappedTable.ContainsKey(EncodedTextBytes[k].ToString()))
						{
							text5 = differenceMappedTable[EncodedTextBytes[k].ToString()];
						}
						if (differenceTable.ContainsValue(text5) && differenceMappedTable.ContainsValue(text5))
						{
							foreach (KeyValuePair<int, string> item3 in differenceTable)
							{
								if (item3.Value == text5)
								{
									num11 = item3.Key;
									break;
								}
							}
						}
						else if ((ReverseMapTable.ContainsKey(text5) || ReverseMapTable.ContainsKey(text4)) && (ReverseMapTable.Count == differenceTable.Count || ReverseMapTable.Count == CharacterMapTable.Count))
						{
							num11 = ((!(text4 == string.Empty)) ? ((int)ReverseMapTable[text4]) : ((int)ReverseMapTable[text5]));
							if (differenceTable.ContainsKey(num11))
							{
								text5 = differenceTable[num11];
							}
						}
						else if (CharacterMapTable.ContainsValue(text5) && CharacterMapTable.Count == differenceTable.Count)
						{
							foreach (KeyValuePair<double, string> item4 in CharacterMapTable)
							{
								if (item4.Value == text5)
								{
									num11 = (int)item4.Key;
									if (differenceTable.ContainsKey(num11))
									{
										text5 = differenceTable[num11];
									}
									break;
								}
							}
						}
						else if (differenceMappedTable.ContainsValue(text5))
						{
							foreach (KeyValuePair<string, string> item5 in differenceMappedTable)
							{
								if (item5.Value == text5)
								{
									num11 = int.Parse(item5.Key);
									if (differenceTable.ContainsKey(num11))
									{
										text5 = differenceTable[num11];
									}
									break;
								}
							}
						}
						else if (differenceMappedTable.ContainsValue(text5))
						{
							foreach (KeyValuePair<string, string> item6 in differenceMappedTable)
							{
								if (item6.Value == text5)
								{
									num11 = int.Parse(item6.Key);
									if (differenceTable.ContainsKey(num11))
									{
										text5 = differenceTable[num11];
									}
									break;
								}
							}
						}
						else if (differenceTable.ContainsKey(num11))
						{
							text5 = differenceTable[num11];
						}
						else if (m_cffGlyphs.DifferenceEncoding.ContainsKey(num11) && structure.FontEncoding != "MacRomanEncoding")
						{
							text5 = m_cffGlyphs.DifferenceEncoding[num11];
						}
						else if (structure.FontEncoding == "MacRomanEncoding")
						{
							if (structure.m_macRomanMapTable.ContainsKey(num11))
							{
								text5 = structure.m_macRomanMapTable[num11];
							}
						}
						else if (structure.FontEncoding == "WinAnsiEncoding" && structure.m_winansiMapTable.ContainsKey(num11))
						{
							text5 = structure.m_winansiMapTable[num11];
						}
						try
						{
							if (!Is1C)
							{
								_ = CharSizeMultiplier;
							}
							else
							{
								_ = FontSize;
							}
							GlyphWriter glyphWriter = new GlyphWriter(m_cffGlyphs.Glyphs, m_cffGlyphs.GlobalBias, Is1C);
							glyphWriter.is1C = Is1C;
							if (structure.BaseFontEncoding == "WinAnsiEncoding" && structure.FontEncoding != "Encoding")
							{
								glyphWriter.HasBaseEncoding = true;
							}
							if (structure.m_isContainFontfile && structure.FontFileType1Font.m_hasFontMatrix)
							{
								glyphWriter.FontMatrix = structure.FontFileType1Font.m_fontMatrix;
							}
							if (structure.IsContainFontfile3 && structure.fontFile3Type1Font.hasFontMatrix)
							{
								glyphWriter.FontMatrix = structure.fontFile3Type1Font.FontMatrix;
							}
							if (FontEncoding == "MacRomanEncoding" && c3 > '~')
							{
								GetMacEncodeTable();
								if (OctDecMapTable != null && OctDecMapTable.ContainsKey(c3))
								{
									num11 = OctDecMapTable[c3];
									c3 = (char)num11;
								}
								string text6 = m_macEncodeTable[c3];
								text5 = FontStructure.GetCharCode(text6);
								c3 = text6[0];
							}
							if (!IsCID)
							{
								if (ReverseMapTable.ContainsKey(text5) && (ReverseMapTable.Count == differenceTable.Count || ReverseMapTable.Count == CharacterMapTable.Count) && m_cffGlyphs.DifferenceEncoding.ContainsKey(num11) && structure.FontEncoding != "MacRomanEncoding")
								{
									text5 = m_cffGlyphs.DifferenceEncoding[num11];
								}
								if (glyphWriter.glyphs.ContainsKey(text5))
								{
									_ = g.Transform;
									new PdfUnitConverter();
									if (m_cffGlyphs.RenderedPath.ContainsKey(text5))
									{
										pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[text5];
									}
								}
								else
								{
									string charCode = FontStructure.GetCharCode(text5);
									if (glyphWriter.glyphs.ContainsKey(charCode))
									{
										_ = g.Transform;
										new PdfUnitConverter();
										if (m_cffGlyphs.RenderedPath.ContainsKey(charCode))
										{
											pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[charCode];
										}
									}
									else
									{
										if (m_cffGlyphs.DiffTable != null)
										{
											text5 = m_cffGlyphs.DiffTable[num11];
										}
										else if (UnicodeCharMapTable.ContainsKey(num11))
										{
											text5 = UnicodeCharMapTable[num11];
										}
										charCode = FontStructure.GetCharCode(text5);
										if (glyphWriter.glyphs.ContainsKey(charCode))
										{
											_ = g.Transform;
											new PdfUnitConverter();
											if (m_cffGlyphs.RenderedPath.ContainsKey(charCode))
											{
												pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[charCode];
											}
										}
									}
								}
							}
							else
							{
								string charCode2 = FontStructure.GetCharCode(text5);
								if (ReverseMapTable.ContainsKey(text5) && FontEncoding != "Identity-H")
								{
									charCode2 = ReverseMapTable[text5].ToString();
									if (glyphWriter.glyphs.ContainsKey(charCode2))
									{
										_ = g.Transform;
										new PdfUnitConverter();
										if (m_cffGlyphs.RenderedPath.ContainsKey(charCode2))
										{
											pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[charCode2];
										}
									}
								}
								else
								{
									string key = num11.ToString();
									if (glyphWriter.glyphs.ContainsKey(charCode2))
									{
										_ = g.Transform;
										new PdfUnitConverter();
										if (m_cffGlyphs.RenderedPath.ContainsKey(charCode2))
										{
											pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[charCode2];
										}
									}
									else if (glyphWriter.glyphs.ContainsKey(key))
									{
										_ = g.Transform;
										new PdfUnitConverter();
										if (m_cffGlyphs.RenderedPath.ContainsKey(key))
										{
											pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[key];
										}
									}
								}
							}
							if (gWidths != null)
							{
								if (!differenceTable.ContainsValue(text5) && num11 == 0)
								{
									foreach (KeyValuePair<double, string> item7 in CharacterMapTable)
									{
										if (item7.Value.Equals(text5))
										{
											num11 = (int)item7.Key;
										}
									}
								}
								if (gWidths.ContainsKey(num11))
								{
									currentGlyphWidth = gWidths[num11];
									currentGlyphWidth *= CharSizeMultiplier;
								}
								else if (OctDecMapTable != null && OctDecMapTable.Count != 0)
								{
									num11 = OctDecMapTable[num11];
									if (gWidths.ContainsKey(num11))
									{
										currentGlyphWidth = gWidths[num11];
										currentGlyphWidth *= CharSizeMultiplier;
									}
									else
									{
										currentGlyphWidth = DefaultGlyphWidth;
										currentGlyphWidth *= CharSizeMultiplier;
									}
								}
								else if (CharacterMapTable.Count != 0)
								{
									foreach (KeyValuePair<double, string> item8 in CharacterMapTable)
									{
										if (item8.Value.Equals(text5))
										{
											num11 = (int)item8.Key;
										}
									}
									if (gWidths.ContainsKey(num11))
									{
										currentGlyphWidth = gWidths[num11];
										currentGlyphWidth *= CharSizeMultiplier;
									}
									else
									{
										currentGlyphWidth = DefaultGlyphWidth;
										currentGlyphWidth *= CharSizeMultiplier;
									}
								}
								else
								{
									currentGlyphWidth = DefaultGlyphWidth;
									currentGlyphWidth *= CharSizeMultiplier;
								}
							}
							else
							{
								currentGlyphWidth = DefaultGlyphWidth;
								currentGlyphWidth *= CharSizeMultiplier;
							}
						}
						catch
						{
							SizeF sizeF = g.MeasureString(c3.ToString(), skpaint);
							num8 = sizeF.Width / 100f * TextScaling;
							if (FontGlyphWidths != null && FontGlyphWidths.ContainsKey(c3))
							{
								currentGlyphWidth = FontGlyphWidths[c3];
								currentGlyphWidth *= CharSizeMultiplier;
							}
							try
							{
								if ((byte)c3 > 126 && m_fontEncoding == "MacRomanEncoding")
								{
									_ = MacRomanToUnicode[(byte)c3 - 128];
									if (isNegativeFont)
									{
										DocGen.Pdf.Graphics.GraphicsState state = g.Save();
										g.MultiplyTransform(new DocGen.Drawing.Matrix(1f, 0f, 0f, -1f, 0f, 2f * newLocation.Y + 2f * sizeF.Height));
										flag = true;
										DrawSystemFontGlyphShape(c3, g, out txtMatrix);
										g.Restore(state);
									}
									else
									{
										flag = true;
										DrawSystemFontGlyphShape(c3, g, out txtMatrix);
									}
								}
								else
								{
									if (isNegativeFont)
									{
										DocGen.Pdf.Graphics.GraphicsState state = g.Save();
										g.MultiplyTransform(new DocGen.Drawing.Matrix(1f, 0f, 0f, -1f, 0f, 2f * newLocation.Y + 2f * sizeF.Height));
										flag = true;
										DrawSystemFontGlyphShape(c3, g, out txtMatrix);
										g.Restore(state);
									}
									else if (c3 > '\u007f' && c3 <= 'ÿ' && m_fontEncoding == "WinAnsiEncoding")
									{
										Encoding.Default.GetString(new byte[1] { (byte)c3 });
										flag = true;
										DrawSystemFontGlyphShape(c3, g, out txtMatrix);
									}
									else
									{
										flag = true;
										DrawSystemFontGlyphShape(c3, g, out txtMatrix);
									}
									renderedText += c3;
								}
							}
							catch (Exception ex)
							{
								exceptions.Exceptions.Append("\r\nCharacter not rendered " + c3 + "\r\n" + ex.StackTrace);
								continue;
							}
							if (num9 < text5.Length)
							{
								newLocation.X += num8 + CharacterSpacing;
							}
							else
							{
								newLocation.X += num8;
							}
						}
						if (!flag)
						{
							if (text4 == string.Empty)
							{
								DrawGlyphs(pathGeom, currentGlyphWidth, g, out txtMatrix, c3.ToString());
							}
							else
							{
								DrawGlyphs(pathGeom, currentGlyphWidth, g, out txtMatrix, text4);
							}
						}
						continue;
					}
					SizeF sizeF2 = g.MeasureString(c3.ToString(), skpaint);
					num8 = sizeF2.Width / 100f * TextScaling;
					if (num8 == 0f && c3 == ' ')
					{
						new StringFormat(StringFormat.GenericDefault).FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
						currentGlyphWidth = g.MeasureString(" ", skpaint).Width;
					}
					try
					{
						if ((byte)c3 > 126 && m_fontEncoding == "MacRomanEncoding" && !Isembeddedfont)
						{
							_ = MacRomanToUnicode[(byte)c3 - 128];
							if (isNegativeFont)
							{
								DocGen.Pdf.Graphics.GraphicsState state2 = g.Save();
								g.MultiplyTransform(new DocGen.Drawing.Matrix(1f, 0f, 0f, -1f, 0f, 2f * newLocation.Y + 2f * sizeF2.Height));
								flag = true;
								DrawSystemFontGlyphShape(c3, g, out txtMatrix);
								g.Restore(state2);
							}
							else
							{
								flag = true;
								DrawSystemFontGlyphShape(c3, g, out txtMatrix);
							}
						}
						else if (isNegativeFont)
						{
							DocGen.Pdf.Graphics.GraphicsState state2 = g.Save();
							g.MultiplyTransform(new DocGen.Drawing.Matrix(1f, 0f, 0f, -1f, 0f, 2f * newLocation.Y + 2f * sizeF2.Height));
							flag = true;
							DrawSystemFontGlyphShape(c3, g, out txtMatrix);
							g.Restore(state2);
						}
						else if (c3 > '\u007f' && c3 <= 'ÿ' && m_fontEncoding == "WinAnsiEncoding" && !Isembeddedfont && !m_isMpdfFont)
						{
							Encoding.Default.GetString(new byte[1] { (byte)c3 });
							flag = true;
							DrawSystemFontGlyphShape(c3, g, out txtMatrix);
						}
						else if (FontEncoding != "Identity-H" && structure.fontType.Value == "TrueType" && structure.GlyphFontFile2 != null)
						{
							if (OctDecMapTable != null && OctDecMapTable.ContainsKey(c3))
							{
								c3 = (char)OctDecMapTable[c3];
							}
							if (structure.FontEncoding == "WinAnsiEncoding" && structure.m_winansiMapTable.ContainsKey(c3))
							{
								c3 = structure.m_winansiMapTable[c3][0];
							}
						}
						else if (structure.GlyphFontFile2 == null)
						{
							flag = true;
							if (!DrawSystemFontGlyphShape(c3, g, out txtMatrix))
							{
								DrawSystemFontGlyph(c3.ToString(), skpaint, new PdfPen(PathBrush).Brush, newLocation, format, g, out newLocation);
							}
							txtMatrix = textLineMatrix;
						}
						if (CharacterMapTable.ContainsKey((int)c3) && CharacterMapTable.Count > 0)
						{
							char c4 = ' ';
							string text7 = CharacterMapTable[(int)c3];
							_ = new char[text7.Length];
							c4 = text7.ToCharArray()[0];
							if (FontGlyphWidths != null)
							{
								if (structure.fontType.Value == "Type0")
								{
									if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.ContainsKey(c3) && !structure.IsMappingDone)
									{
										if (FontGlyphWidths.ContainsKey(CidToGidReverseMapTable[c3]))
										{
											currentGlyphWidth = FontGlyphWidths[CidToGidReverseMapTable[c3]];
											currentGlyphWidth *= CharSizeMultiplier;
										}
										else
										{
											currentGlyphWidth = DefaultGlyphWidth;
											currentGlyphWidth *= CharSizeMultiplier;
										}
									}
									else if (FontGlyphWidths.ContainsKey(c3))
									{
										currentGlyphWidth = FontGlyphWidths[c3];
										currentGlyphWidth *= CharSizeMultiplier;
									}
									else if (ReverseMapTable.ContainsKey(c4.ToString()))
									{
										if (FontGlyphWidths.ContainsKey((int)ReverseMapTable[c4.ToString()]))
										{
											currentGlyphWidth = g.MeasureString(c4.ToString(), skpaint).Width / 100f * TextScaling / textFont.Size;
										}
										else
										{
											currentGlyphWidth = DefaultGlyphWidth;
											currentGlyphWidth *= CharSizeMultiplier;
										}
									}
									else
									{
										currentGlyphWidth = DefaultGlyphWidth;
										currentGlyphWidth *= CharSizeMultiplier;
									}
								}
								else if (structure.fontType.Value == "TrueType" && FontGlyphWidths.ContainsKey(c3))
								{
									currentGlyphWidth = FontGlyphWidths[c3];
									currentGlyphWidth *= CharSizeMultiplier;
								}
							}
							else if (FontGlyphWidths == null)
							{
								currentGlyphWidth = DefaultGlyphWidth;
								currentGlyphWidth *= CharSizeMultiplier;
							}
						}
						else if (structure.CidToGidReverseMapTable.ContainsKey(c3))
						{
							int key2 = structure.CidToGidReverseMapTable[c3];
							if (FontGlyphWidths.ContainsKey(key2))
							{
								currentGlyphWidth = FontGlyphWidths[key2];
								currentGlyphWidth *= CharSizeMultiplier;
							}
						}
						else if (FontGlyphWidths != null && FontGlyphWidths.Count > 0 && FontGlyphWidths.ContainsKey(c3))
						{
							currentGlyphWidth = FontGlyphWidths[c3];
							currentGlyphWidth *= CharSizeMultiplier;
						}
						else
						{
							currentGlyphWidth = DefaultGlyphWidth * CharSizeMultiplier;
						}
					}
					catch (Exception ex2)
					{
						exceptions.Exceptions.Append("\r\nCharacter not rendered " + c3 + "\r\n" + ex2.StackTrace);
						continue;
					}
					if (num9 < c3.ToString().Length)
					{
						newLocation.X += CharacterSpacing;
					}
					if (!flag)
					{
						DrawGlyphs(pathGeom, currentGlyphWidth, g, out txtMatrix, c3.ToString());
					}
				}
				txtMatrix = textLineMatrix;
			}
			return newLocation.X - x;
		}
		finally
		{
			Monitor.Exit(fontResourceLocker);
		}
	}

	private string ResolveFontName(string matrixImplFontName)
	{
		if (matrixImplFontName.Contains("times") || matrixImplFontName.Contains("Times"))
		{
			return "Times New Roman";
		}
		if (matrixImplFontName.Contains("Helvetica"))
		{
			return "Helvetica";
		}
		return matrixImplFontName;
	}

	private bool GetFlag(byte bit)
	{
		bit--;
		return GetBit(FontFlag, bit);
	}

	public bool GetBit(int n, byte bit)
	{
		return (n & (1 << (int)bit)) != 0;
	}

	private bool DrawSystemFontGlyphShape(char letter, GraphicsHelper g, out Matrix temptextmatrix)
	{
		GraphicsUnit pageUnit = g.PageUnit;
		DocGen.Drawing.Matrix transform = g.Transform;
		g.PageUnit = GraphicsUnit.Pixel;
		if (SubstitutedFontsList != null && SubstitutedFontsList.Count > 0)
		{
			string text = m_fontName;
			if (m_fontStyle.ToString() != "Regular")
			{
				text = text + " " + m_fontStyle;
			}
			if (SubstitutedFontsList.ContainsKey(text))
			{
				string text2 = SubstitutedFontsList[text];
				if (text2.Contains("Bold"))
				{
					text2 = text2.Replace("Bold", "");
					m_fontStyle = FontStyle.Bold;
				}
				else if (text2.Contains("Italic"))
				{
					text2 = text2.Replace("Italic", "");
					m_fontStyle = FontStyle.Italic;
				}
				else
				{
					m_fontStyle = FontStyle.Regular;
				}
				m_fontName = text2.Trim();
			}
		}
		SystemFontFontDescriptor systemFontFontDescriptor = new SystemFontFontDescriptor(m_fontName, m_fontStyle);
		if (!structure.IsOpenTypeFont)
		{
			if (testdict.ContainsKey(systemFontFontDescriptor))
			{
				openTypeFontSource = testdict[systemFontFontDescriptor];
			}
			else
			{
				openTypeFontSource = SystemFontsManager.GetFontSource(systemFontFontDescriptor) as SystemFontOpenTypeFontSource;
				testdict.Add(systemFontFontDescriptor, openTypeFontSource);
			}
		}
		else if (testdict.ContainsKey(systemFontFontDescriptor))
		{
			openTypeFontSource = testdict[systemFontFontDescriptor];
		}
		else
		{
			openTypeFontSource = new SystemFontOpenTypeFontSource(new SystemFontOpenTypeFontReader(structure.FontStream.ToArray()));
			testdict.Add(systemFontFontDescriptor, openTypeFontSource);
		}
		SystemFontGlyph systemFontGlyph = new SystemFontGlyph();
		g.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		systemFontGlyph.Name = letter.ToString();
		systemFontGlyph.FontSize = m_fontSize;
		if (OctDecMapTable != null && OctDecMapTable.ContainsKey(letter))
		{
			letter = (char)OctDecMapTable[letter];
		}
		else if (structure.FontEncoding == "WinAnsiEncoding" && structure.m_winansiMapTable != null && structure.m_winansiMapTable.ContainsKey(letter) && structure.CharacterMapTable.ContainsKey((int)letter))
		{
			letter = structure.m_winansiMapTable[letter].ToCharArray()[0];
		}
		else if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.Count > 0 && CidToGidReverseMapTable.ContainsKey(letter))
		{
			letter = (char)CidToGidReverseMapTable[letter];
		}
		byte b = (byte)letter;
		string text3 = letter.ToString();
		systemFontGlyph.CharId = new CharCode(b);
		systemFontGlyph.CharSpacing = m_characterSpacing;
		systemFontGlyph.FontStyle = m_fontStyle;
		systemFontGlyph.HorizontalScaling = TextHorizontalScaling;
		ushort num = 0;
		string text4 = letter.ToString();
		num = GetGlyphID(letter.ToString());
		if (!structure.IsMappingDone)
		{
			if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.ContainsKey(Convert.ToChar(text4)) && structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
			{
				text4 = CharacterMapTable[CidToGidReverseMapTable[Convert.ToChar(text4)]];
			}
			else if (structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
			{
				text4 = structure.MapCharactersFromTable(text4);
			}
			else if (structure.DifferencesDictionary != null && structure.DifferencesDictionary.Count > 0)
			{
				text4 = structure.MapDifferences(text4);
			}
		}
		systemFontGlyph.GlyphId = num;
		Glyph glyph = new Glyph();
		glyph.HorizontalScaling = TextHorizontalScaling;
		glyph.CharSpacing = CharacterSpacing;
		glyph.FontSize = systemFontGlyph.FontSize;
		glyph.Name = systemFontGlyph.Name;
		glyph.FontFamily = m_fontName;
		glyph.CharId = systemFontGlyph.CharId;
		glyph.TransformMatrix = GetTextRenderingMatrix();
		systemFontGlyph.TransformMatrix = new SystemFontMatrix(glyph.TransformMatrix.M11, glyph.TransformMatrix.M12, glyph.TransformMatrix.M21, glyph.TransformMatrix.M22, glyph.TransformMatrix.OffsetX, glyph.TransformMatrix.OffsetY);
		openTypeFontSource.GetGlyphOutlines(systemFontGlyph, 100.0);
		systemFontGlyph.Width = GetSystemFontGlyphWidth(glyph, g);
		if (systemFontGlyph.Width == -1.0)
		{
			openTypeFontSource.GetAdvancedWidth(systemFontGlyph);
			systemFontGlyph.Width = systemFontGlyph.AdvancedWidth;
		}
		if (FontGlyphWidths != null && FontGlyphWidths.Count > 0 && FontGlyphWidths.ContainsKey(letter))
		{
			if (ReverseMapTable != null && ReverseMapTable.ContainsKey(letter.ToString()))
			{
				double num2 = ReverseMapTable[letter.ToString()];
				systemFontGlyph.Width = FontGlyphWidths[(int)num2];
				systemFontGlyph.Width *= CharSizeMultiplier;
			}
			else
			{
				systemFontGlyph.Width = FontGlyphWidths[letter];
				systemFontGlyph.Width *= CharSizeMultiplier;
			}
		}
		if (systemFontGlyph.AdvancedWidth > 0.0 && IsCID && !IsType1Font && structure.IsAdobeIdentity)
		{
			systemFontGlyph.Width = systemFontGlyph.AdvancedWidth;
		}
		glyph.CharSpacing = CharacterSpacing;
		if (letter.ToString() == " " && ((CharID.BytesCount == 1 && CharID.Bytes[0] == 32) || CharID.IsEmpty))
		{
			glyph.WordSpacing = WordSpacing;
		}
		if (systemFontGlyph.Width != -1.0)
		{
			glyph.Width = systemFontGlyph.Width;
			Matrix identity = Matrix.Identity;
			identity.Scale(0.01, 0.01, 0.0, 0.0);
			identity.Translate(0.0, 1.0);
			transformations.PushTransform(identity * glyph.TransformMatrix);
			DocGen.Drawing.Matrix matrix = (DocGen.Drawing.Matrix)g.Transform.Clone();
			matrix.Multiply(GetTransformationMatrix(transformations.CurrentTransform));
			g.Transform = matrix;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			if (structure.ReverseDictMapping.ContainsKey(systemFontGlyph.Name))
			{
				byte b2 = (byte)(float)structure.ReverseDictMapping[systemFontGlyph.Name];
				string[] standardFontEncodingNames = GetStandardFontEncodingNames();
				systemFontGlyph.Name = standardFontEncodingNames[b2];
			}
			if (!m_isMpdfFont && !IsPdfium)
			{
				DrawPath(g, systemFontGlyph, systemFontGlyph.Name);
			}
			float num3 = 0f;
			num3 = ((glyph.TransformMatrix.M11 > 0.0) ? ((float)glyph.TransformMatrix.M11) : ((glyph.TransformMatrix.M12 == 0.0 || glyph.TransformMatrix.M21 == 0.0) ? ((float)glyph.FontSize) : ((!(glyph.TransformMatrix.M12 < 0.0)) ? ((float)glyph.TransformMatrix.M12) : ((float)(0.0 - glyph.TransformMatrix.M12)))));
			glyph.ToUnicode = text4;
			PdfUnitConverter pdfUnitConverter = new PdfUnitConverter();
			if (pageRotation == 90f || pageRotation == 270f)
			{
				if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
				{
					glyph.IsRotated = false;
					glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num3 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num3, num3));
				}
				else
				{
					glyph.IsRotated = true;
					if (IsFindText && pageRotation == 90f)
					{
						glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num3 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num3, num3));
					}
					else
					{
						glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX + (num3 + (float)(glyph.Ascent / 1000.0)) * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConverter.ConvertFromPixels(matrix.OffsetY + num3 * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor), new Size(num3, glyph.Width * (double)num3));
					}
				}
			}
			else
			{
				if (pageRotation == 180f)
				{
					if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
					{
						glyph.IsRotated = true;
					}
					else
					{
						glyph.IsRotated = false;
					}
				}
				glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConverter.ConvertFromPixels(matrix.OffsetY - num3 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num3, num3));
			}
			if (structure.IsAdobeJapanFont)
			{
				if (structure.AdobeJapanCidMapTable.ContainsKey(Convert.ToChar(text3)))
				{
					text3 = structure.AdobeJapanCidMapTableGlyphParser(text3);
				}
				glyph.ToUnicode = text3;
			}
			if (glyph.ToUnicode.Length != 1)
			{
				textElementGlyphList.Add(glyph);
				for (int i = 0; i < glyph.ToUnicode.Length - 1; i++)
				{
					Glyph item = new Glyph();
					textElementGlyphList.Add(item);
				}
			}
			else
			{
				textElementGlyphList.Add(glyph);
			}
			UpdateTextMatrix(glyph);
			transformations.PopTransform();
			renderedText += text4;
			g.Transform = transform;
			g.PageUnit = pageUnit;
			temptextmatrix = textLineMatrix;
			if (m_isMpdfFont)
			{
				return false;
			}
			return true;
		}
		temptextmatrix = textLineMatrix;
		return false;
	}

	private int GetInt(byte[] val)
	{
		int num = 0;
		int num2 = val.Length;
		for (int i = 0; i < num2; i++)
		{
			num |= ((num2 > i) ? (val[i] & 0xFF) : 0);
			if (i < num2 - 1)
			{
				num <<= 8;
			}
		}
		return num;
	}

	internal float RenderWithSpace(GraphicsHelper g, PointF currentLocation, List<string> decodedList, List<float> characterSpacings, double textScaling, Dictionary<int, int> gWidths, double type1Height, Dictionary<int, string> differenceTable, Dictionary<string, string> differenceMappedTable, Dictionary<int, string> differenceEncoding, out Matrix textmatrix)
	{
		textmatrix = Matrix.Identity;
		string fontName = FontName;
		renderedText = string.Empty;
		_ = FontName;
		textFont = null;
		float num = 0f;
		float x = currentLocation.X;
		int num2 = 0;
		string[] array = null;
		PdfUnitConverter pdfUnitConverter = new PdfUnitConverter();
		m_isMpdfFont = isMpdfaaFonts();
		if (Font != null && Isembeddedfont)
		{
			BackupEmbededFontName(fontName);
			FontName = CheckFontName(Font.Name);
			textFont = Font;
		}
		else
		{
			CheckFontStyle(FontName);
			FontName = CheckFontName(FontName);
			if (FontSize < 0f)
			{
				textFont = new Font(FontName, 0f - FontSize, FontStyle);
			}
			else
			{
				textFont = new Font(FontName, FontSize, FontStyle);
			}
		}
		if (ZapfPostScript != null)
		{
			char[] separator = new char[1] { ' ' };
			array = ZapfPostScript.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		}
		PointF newLocation = currentLocation;
		float fontSize = FontSize;
		_ = currentLocation.Y;
		bool flag = false;
		fontName = ResolveFontName(fontName);
		foreach (string decoded in decodedList)
		{
			flag = false;
			double result;
			if (StdFontsAssistant.IsStandardFontName(fontName) && !Isembeddedfont)
			{
				if (m_fontStyle == FontStyle.Bold && !fontName.Contains("Bold"))
				{
					fontName = ((!StdFontsAssistant.IsAlternativeStdFontAvailable(fontName)) ? (fontName + "-Bold") : (fontName + ",Bold"));
				}
				if (m_fontStyle == FontStyle.Italic && !fontName.Contains("Italic"))
				{
					fontName = (StdFontsAssistant.IsAlternativeStdFontAvailable(fontName) ? (fontName + ",Italic") : ((fontName.Contains("Courier") || fontName.Contains("Helvetica")) ? (fontName + "-Oblique") : (fontName + "-Italic")));
				}
				if (m_fontStyle == (FontStyle)3 && !fontName.Contains("Italic") && !fontName.Contains("Bold"))
				{
					if (StdFontsAssistant.IsAlternativeStdFontAvailable(fontName))
					{
						fontName += ",Bold";
						fontName += "Italic";
					}
					else if (!fontName.Contains("Courier") && !fontName.Contains("Helvetica"))
					{
						fontName += "-Bold";
						fontName += "Italic";
					}
					else
					{
						fontName += "-Bold";
						fontName += "Oblique";
					}
				}
				GraphicsUnit pageUnit = g.PageUnit;
				DocGen.Drawing.Matrix transform = g.Transform;
				g.PageUnit = GraphicsUnit.Pixel;
				if (double.TryParse(decoded, out result))
				{
					UpdateTextMatrix(result);
				}
				else
				{
					if (!fontSourceCache.ContainsKey(FontID + structure.FontRefNumber))
					{
						FontSource = manager.GetStandardFontSource(fontName);
						fontSourceCache.Add(FontID + structure.FontRefNumber, FontSource);
					}
					else
					{
						FontSource = fontSourceCache[FontID + structure.FontRefNumber];
					}
					string text = decoded.Remove(decoded.Length - 1, 1);
					flag = false;
					byte? b = null;
					if (structure.ReverseDictMapping.ContainsKey(text))
					{
						float num3 = structure.ReverseDictMapping[text];
						b = (byte)num3;
						flag = false;
						RenderReverseMapTableByte((char)b.Value, g, text);
						g.Transform = transform;
						g.PageUnit = pageUnit;
						textmatrix = textLineMatrix;
						if (!m_isRectation)
						{
							continue;
						}
					}
					string text2 = text;
					for (int num4 = 0; num4 < text2.Length; num4++)
					{
						char c = text2[num4];
						flag = false;
						g.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
						Glyph glyph = new Glyph();
						glyph.FontSize = FontSize;
						glyph.FontFamily = FontName;
						glyph.FontStyle = FontStyle;
						glyph.TransformMatrix = GetTextRenderingMatrix();
						glyph.Name = c.ToString();
						glyph.HorizontalScaling = TextHorizontalScaling;
						glyph.CharId = new CharCode((int)c);
						glyph.CharSpacing = CharacterSpacing;
						string[] standardFontEncodingNames = GetStandardFontEncodingNames();
						byte[] bytes = Encoding.UTF8.GetBytes(c.ToString());
						if (structure.ReverseDictMapping.ContainsKey(c.ToString()))
						{
							float num5 = structure.ReverseDictMapping[c.ToString()];
							if (structure.DifferencesDictionary.ContainsKey(num5.ToString()))
							{
								glyph.Name = FontStructure.GetCharCode(structure.DifferencesDictionary[num5.ToString()]);
							}
							else
							{
								bytes[0] = (byte)num5;
								glyph.Name = standardFontEncodingNames[bytes[0]];
							}
						}
						else if (OctDecMapTable != null && OctDecMapTable.ContainsKey(c))
						{
							char c2 = (char)OctDecMapTable[c];
							glyph.Name = standardFontEncodingNames[(uint)c2];
						}
						else
						{
							if (structure.DifferencesDictionary != null)
							{
								Dictionary<string, string> differencesDictionary = structure.DifferencesDictionary;
								int num6 = c;
								if (differencesDictionary.ContainsKey(num6.ToString()) && structure.BaseFontEncoding != "WinAnsiEncoding")
								{
									Dictionary<string, string> differencesDictionary2 = structure.DifferencesDictionary;
									num6 = c;
									glyph.Name = FontStructure.GetCharCode(differencesDictionary2[num6.ToString()]);
									goto IL_0675;
								}
							}
							if (standardFontEncodingNames.Length > c && structure.FontName != "ZapfDingbats")
							{
								if (FontName == "Symbol")
								{
									glyph.Name = FontStructure.GetCharCode(c.ToString());
								}
								else
								{
									glyph.Name = standardFontEncodingNames[(uint)c];
								}
							}
							else if (structure.FontName == "ZapfDingbats")
							{
								if (num2 < ZapfPostScript.Length)
								{
									glyph.Name = array[num2].Trim();
									num2++;
								}
							}
							else
							{
								glyph.Name = standardFontEncodingNames[bytes[0]];
							}
						}
						goto IL_0675;
						IL_0675:
						glyph.Width = GetGlyphWidth(glyph);
						FontSource.GetGlyphOutlines(glyph, 100.0);
						new PdfElementsRendererNet().RenderGlyph(glyph);
						Matrix identity = Matrix.Identity;
						identity.Scale(0.01, 0.01, 0.0, 0.0);
						identity.Translate(0.0, 1.0);
						transformations.PushTransform(identity * glyph.TransformMatrix);
						DocGen.Drawing.Matrix matrix = (DocGen.Drawing.Matrix)g.Transform.Clone();
						matrix.Multiply(GetTransformationMatrix(transformations.CurrentTransform));
						g.Transform = matrix;
						g.SmoothingMode = SmoothingMode.AntiAlias;
						glyph.ToUnicode = c.ToString();
						float num7 = 0f;
						num7 = ((glyph.TransformMatrix.M11 > 0.0) ? ((float)glyph.TransformMatrix.M11) : ((glyph.TransformMatrix.M12 == 0.0 || glyph.TransformMatrix.M21 == 0.0) ? ((float)glyph.FontSize) : ((!(glyph.TransformMatrix.M12 < 0.0)) ? ((float)glyph.TransformMatrix.M12) : ((float)(0.0 - glyph.TransformMatrix.M12)))));
						if (pageRotation == 90f || pageRotation == 270f)
						{
							if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
							{
								glyph.IsRotated = false;
								glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num7 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
							}
							else
							{
								glyph.IsRotated = true;
								if (IsFindText && pageRotation == 90f)
								{
									glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num7 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
								}
								else
								{
									glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX + (num7 + (float)(glyph.Ascent / 1000.0)) * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetY - num7 * matrix.Elements[2], PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num7 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size((double)num7 - glyph.Ascent / 1000.0, glyph.Width * (double)num7));
								}
							}
						}
						else
						{
							if (pageRotation == 180f)
							{
								if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
								{
									glyph.IsRotated = true;
								}
								else
								{
									glyph.IsRotated = false;
								}
							}
							glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConverter.ConvertFromPixels(matrix.OffsetY - num7 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
						}
						if (structure.CharacterMapTable != null && structure.CharacterMapTable.ContainsKey((int)c))
						{
							glyph.ToUnicode = structure.CharacterMapTable[(int)c];
						}
						if (glyph.ToUnicode.Length != 1)
						{
							textElementGlyphList.Add(glyph);
							for (int i = 0; i < glyph.ToUnicode.Length - 1; i++)
							{
								Glyph item = new Glyph();
								textElementGlyphList.Add(item);
							}
						}
						else
						{
							textElementGlyphList.Add(glyph);
						}
						GetFontSize(glyph, num7);
						UpdateTextMatrix(glyph);
						transformations.PopTransform();
						if (structure.CharacterMapTable != null && structure.CharacterMapTable.ContainsKey((int)c))
						{
							renderedText += glyph.ToString();
						}
						else
						{
							renderedText += c;
						}
					}
				}
				g.Transform = transform;
				g.PageUnit = pageUnit;
				textmatrix = textLineMatrix;
				continue;
			}
			StringFormat format = new StringFormat(StringFormat.GenericDefault);
			new StringFormat(StringFormat.GenericDefault).FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
			float width = g.MeasureString(" ", skpaint).Width;
			if (double.TryParse(decoded, out result))
			{
				UpdateTextMatrix(result);
				float sizeInPoints = textFont.SizeInPoints;
				float num8 = (float)result * (sizeInPoints / 1000f);
				num8 -= CharacterSpacing;
				newLocation.X -= num8;
				textmatrix = textLineMatrix;
				continue;
			}
			if (decoded[0] >= '\u0e00' && decoded[0] <= '\u0e7f')
			{
				string empty = string.Empty;
				new List<char>();
				empty = decoded.Remove(decoded.Length - 1, 1);
				flag = true;
				DrawSystemFontGlyph(empty, skpaint, new PdfPen(PathBrush).Brush, newLocation, format, g, out newLocation);
				continue;
			}
			string text3 = decoded.Remove(decoded.Length - 1, 1);
			int num9 = 0;
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			if (IsCID && !IsType1Font)
			{
				byte[] bytes2 = Encoding.Unicode.GetBytes(text3);
				int num10 = 0;
				for (int j = 0; j < bytes2.Length; j += 2)
				{
					dictionary.Add(num10, bytes2[j]);
					num10++;
				}
			}
			for (int k = 0; k < text3.Length; k++)
			{
				char c3 = text3[k];
				string text4 = string.Empty;
				if (IsType1Font && !structure.IsOpenTypeFont && differenceTable.Count == 0)
				{
					text4 = text3[k].ToString();
					int num11 = 0;
					if (ReverseMapTable.ContainsKey(text4))
					{
						foreach (KeyValuePair<double, string> item2 in CharacterMapTable)
						{
							if (item2.Value.IndexOf(c3) > -1)
							{
								num11++;
							}
						}
					}
					if ((!ReverseMapTable.ContainsKey(text4) || num11 > 1) && (char.IsLetter(c3) || char.IsPunctuation(c3) || char.IsSymbol(c3)))
					{
						num11 = 0;
						if (k != text3.Length - 1)
						{
							text4 = text3.Substring(k);
							k += text4.Length - 1;
						}
					}
					if (text4 != c3.ToString())
					{
						for (int l = 0; l < text4.Length; l++)
						{
							if (!ReverseMapTable.ContainsKey(text4))
							{
								text4 = text4.Remove(text4.Length - 1);
								k--;
								l = 0;
								if (text4 == text3[k].ToString())
								{
									text4 = string.Empty;
								}
								if (ReverseMapTable.ContainsKey(text4))
								{
									break;
								}
							}
						}
					}
					else
					{
						text4 = string.Empty;
					}
				}
				if (IsCID && !IsType1Font)
				{
					byte[] array2 = new byte[2];
					int num12 = text3[k];
					int num13 = -1;
					if (k + 1 < text3.Length)
					{
						num13 = text3[k + 1];
						if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.Count > 0 && CidToGidReverseMapTable.ContainsKey(text3[k + 1]))
						{
							num13 = CidToGidReverseMapTable[text3[k + 1]];
						}
					}
					if (text3.Length > k && CidToGidReverseMapTable != null && CidToGidReverseMapTable.Count > 0 && CidToGidReverseMapTable.ContainsKey(text3[k]))
					{
						num12 = CidToGidReverseMapTable[text3[k]];
					}
					if (CharacterMapTable.Count != 0 && k + 1 < text3.Length && (!CharacterMapTable.ContainsKey(num13) || (CharacterMapTable.ContainsKey(num12) && CharacterMapTable.ContainsKey(num13) && structure.CidToGidMap == null && !structure.IsHexaDecimalString && !structure.IsMappingDone)))
					{
						array2[0] = (byte)dictionary[k];
						array2[1] = (byte)dictionary[k + 1];
						c3 = (char)GetInt(array2);
						if (!CharacterMapTable.ContainsKey((int)c3))
						{
							c3 = text3[k];
						}
						else
						{
							k++;
						}
					}
					else if (CharacterMapTable.Count != 0 && !CharacterMapTable.ContainsKey(num12))
					{
						if (k + 1 < text3.Length)
						{
							array2[0] = (byte)dictionary[k];
							array2[1] = (byte)dictionary[k + 1];
							c3 = (char)GetInt(array2);
							if (!CharacterMapTable.ContainsKey((int)c3))
							{
								array2 = new byte[2];
								array2 = Encoding.Unicode.GetBytes(text3[k].ToString());
								c3 = (char)GetInt(array2);
							}
							else
							{
								k++;
							}
						}
						else if (!CharacterMapTable.ContainsKey((int)c3))
						{
							array2 = Encoding.Unicode.GetBytes(text3[k].ToString());
							c3 = (char)GetInt(array2);
							if (!CharacterMapTable.ContainsKey((int)c3))
							{
								c3 = text3[k];
							}
						}
					}
				}
				num9++;
				if (IsType1Font && !structure.IsOpenTypeFont)
				{
					GlyphWriter glyphWriter = new GlyphWriter(m_cffGlyphs.Glyphs, Is1C);
					string text5 = c3.ToString();
					int num14 = c3;
					text5 = c3.ToString();
					num14 = c3;
					if (structure.DifferencesDictionary.ContainsValue(text3))
					{
						text5 = text3;
						k = text3.Length - 1;
					}
					if (differenceTable.ContainsValue(text3) && differenceMappedTable.ContainsValue(text3))
					{
						text5 = text3;
						foreach (KeyValuePair<int, string> item3 in differenceTable)
						{
							if (item3.Value == text3)
							{
								num14 = item3.Key;
								k = text3.Length - 1;
								break;
							}
						}
					}
					else if ((ReverseMapTable.ContainsKey(text5) || ReverseMapTable.ContainsKey(text4)) && (ReverseMapTable.Count == differenceTable.Count || ReverseMapTable.Count == CharacterMapTable.Count))
					{
						num14 = ((!(text4 == string.Empty)) ? ((int)ReverseMapTable[text4]) : ((int)ReverseMapTable[text5]));
						if (differenceTable.ContainsKey(num14))
						{
							byte b2 = Convert.ToByte(num14);
							text5 = differenceTable[num14];
							CharID = new CharCode(b2);
						}
					}
					else if (differenceMappedTable.ContainsValue(text5))
					{
						foreach (KeyValuePair<string, string> item4 in differenceMappedTable)
						{
							if (item4.Value == text5)
							{
								num14 = int.Parse(item4.Key);
								if (differenceTable.ContainsKey(num14))
								{
									text5 = differenceTable[num14];
								}
								break;
							}
						}
					}
					else if (differenceMappedTable.ContainsKey(num14.ToString()))
					{
						text5 = differenceMappedTable[num14.ToString()];
					}
					else if (differenceTable.ContainsKey(c3))
					{
						text5 = differenceTable[c3];
						num14 = c3;
					}
					else if (differenceMappedTable.ContainsValue(text3))
					{
						using Dictionary<string, string>.Enumerator enumerator4 = differenceMappedTable.GetEnumerator();
						if (enumerator4.MoveNext())
						{
							KeyValuePair<string, string> current4 = enumerator4.Current;
							if (current4.Value == text3)
							{
								num14 = int.Parse(current4.Key);
							}
							if (differenceTable.ContainsKey(num14))
							{
								text5 = differenceTable[num14];
							}
						}
					}
					else if (CharacterMapTable.ContainsValue(text5) && CharacterMapTable.Count == differenceTable.Count)
					{
						foreach (KeyValuePair<double, string> item5 in CharacterMapTable)
						{
							if (item5.Value == text5)
							{
								num14 = (int)item5.Key;
								if (differenceTable.ContainsKey(num14))
								{
									text5 = differenceTable[num14];
								}
								break;
							}
						}
					}
					else if (differenceTable.ContainsKey(num14))
					{
						text5 = differenceTable[num14];
					}
					else if (m_cffGlyphs.DifferenceEncoding.ContainsKey(num14) && structure.FontEncoding != "MacRomanEncoding")
					{
						text5 = m_cffGlyphs.DifferenceEncoding[num14];
					}
					else if (structure.FontEncoding == "MacRomanEncoding")
					{
						if (structure.m_macRomanMapTable.ContainsKey(num14))
						{
							text5 = structure.m_macRomanMapTable[num14];
						}
					}
					else if (structure.FontEncoding == "WinAnsiEncoding" && structure.m_winansiMapTable.ContainsKey(num14))
					{
						text5 = structure.m_winansiMapTable[num14];
					}
					try
					{
						double num15 = 0.0;
						if (!Is1C)
						{
							_ = CharSizeMultiplier;
						}
						else
						{
							_ = FontSize;
						}
						glyphWriter = new GlyphWriter(m_cffGlyphs.Glyphs, m_cffGlyphs.GlobalBias, Is1C);
						glyphWriter.is1C = Is1C;
						if (structure.BaseFontEncoding == "WinAnsiEncoding" && structure.FontEncoding != "Encoding")
						{
							glyphWriter.HasBaseEncoding = true;
						}
						if (structure.m_isContainFontfile && structure.FontFileType1Font.m_hasFontMatrix)
						{
							glyphWriter.FontMatrix = structure.FontFileType1Font.m_fontMatrix;
						}
						if (structure.IsContainFontfile3 && structure.fontFile3Type1Font.hasFontMatrix)
						{
							glyphWriter.FontMatrix = structure.fontFile3Type1Font.FontMatrix;
						}
						if (FontEncoding == "MacRomanEncoding" && c3 > '~')
						{
							GetMacEncodeTable();
							if (OctDecMapTable != null && OctDecMapTable.ContainsKey(c3))
							{
								num14 = OctDecMapTable[c3];
								c3 = (char)num14;
							}
							string text6 = m_macEncodeTable[c3];
							text5 = FontStructure.GetCharCode(text6);
							c3 = text6[0];
						}
						if (!IsCID)
						{
							if (glyphWriter.glyphs.ContainsKey(text5))
							{
								_ = g.Transform;
								if (!m_cffGlyphs.RenderedPath.ContainsKey(text5))
								{
									m_spaceCheck = true;
								}
								else
								{
									pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[text5];
									m_spaceCheck = true;
								}
							}
							else
							{
								string charCode = FontStructure.GetCharCode(text5);
								if (glyphWriter.glyphs.ContainsKey(charCode))
								{
									_ = g.Transform;
									new PdfUnitConverter();
									if (m_cffGlyphs.RenderedPath.ContainsKey(charCode))
									{
										pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[charCode];
									}
								}
								if (m_spaceCheck)
								{
									m_spaceCheck = false;
									GraphicsPath graphicsPath = new GraphicsPath();
									pathGeom = graphicsPath;
								}
							}
						}
						else
						{
							string charCode2 = FontStructure.GetCharCode(text5);
							if (ReverseMapTable.ContainsKey(text5) && FontEncoding != "Identity-H")
							{
								charCode2 = ReverseMapTable[text5].ToString();
								if (glyphWriter.glyphs.ContainsKey(charCode2))
								{
									_ = g.Transform;
									new PdfUnitConverter();
									if (m_cffGlyphs.RenderedPath.ContainsKey(charCode2))
									{
										pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[charCode2];
									}
								}
							}
							else
							{
								charCode2 = num14.ToString();
								if (glyphWriter.glyphs.ContainsKey(charCode2))
								{
									_ = g.Transform;
									new PdfUnitConverter();
									if (m_cffGlyphs.RenderedPath.ContainsKey(charCode2))
									{
										pathGeom = (GraphicsPath)m_cffGlyphs.RenderedPath[charCode2];
									}
								}
							}
						}
						if (gWidths != null)
						{
							if (!differenceTable.ContainsValue(text5) && num14 == 0)
							{
								foreach (KeyValuePair<double, string> item6 in CharacterMapTable)
								{
									if (item6.Value.Equals(text5))
									{
										num14 = (int)item6.Key;
									}
								}
							}
							if (gWidths.ContainsKey(num14))
							{
								currentGlyphWidth = gWidths[num14];
								currentGlyphWidth *= CharSizeMultiplier;
							}
							else if (OctDecMapTable != null && OctDecMapTable.Count != 0)
							{
								num14 = OctDecMapTable[num14];
								if (gWidths.ContainsKey(num14))
								{
									currentGlyphWidth = gWidths[num14];
									currentGlyphWidth *= CharSizeMultiplier;
								}
								else
								{
									currentGlyphWidth = DefaultGlyphWidth;
									currentGlyphWidth *= CharSizeMultiplier;
								}
							}
							else if (CharacterMapTable.Count != 0)
							{
								foreach (KeyValuePair<double, string> item7 in CharacterMapTable)
								{
									if (item7.Value.Equals(text5))
									{
										num14 = (int)item7.Key;
									}
								}
								if (gWidths.ContainsKey(num14))
								{
									currentGlyphWidth = gWidths[num14];
									currentGlyphWidth *= CharSizeMultiplier;
								}
								else
								{
									currentGlyphWidth = DefaultGlyphWidth;
									currentGlyphWidth *= CharSizeMultiplier;
								}
							}
							else
							{
								currentGlyphWidth = DefaultGlyphWidth;
								currentGlyphWidth *= CharSizeMultiplier;
							}
						}
						else
						{
							currentGlyphWidth = DefaultGlyphWidth;
							currentGlyphWidth *= CharSizeMultiplier;
						}
						num = (float)(num15 / 100.0 * (double)(float)textScaling);
						if (num9 < text3.Length)
						{
							newLocation.X += num + CharacterSpacing;
						}
						else
						{
							newLocation.X += num;
						}
						if (num14 == 32 || c3 == ' ')
						{
							newLocation.X += WordSpacing;
						}
					}
					catch
					{
						if (c3 == ' ')
						{
							newLocation.X += width + WordSpacing;
							renderedText += " ";
							continue;
						}
						SizeF sizeF = g.MeasureString(c3.ToString(), skpaint);
						num = sizeF.Width / 100f * TextScaling;
						if (FontGlyphWidths != null && FontGlyphWidths.ContainsKey(c3))
						{
							num = (float)FontGlyphWidths[c3] * (CharSizeMultiplier * FontSize) / 100f * TextScaling;
						}
						try
						{
							if ((byte)c3 > 126 && m_fontEncoding == "MacRomanEncoding")
							{
								char letter = (char)MacRomanToUnicode[(byte)c3 - 128];
								if (isNegativeFont)
								{
									DocGen.Pdf.Graphics.GraphicsState state = g.Save();
									g.MultiplyTransform(new DocGen.Drawing.Matrix(1f, 0f, 0f, -1f, 0f, 2f * newLocation.Y + 2f * sizeF.Height));
									flag = true;
									DrawSystemFontGlyphShape(c3, g, out textmatrix);
									g.Restore(state);
								}
								else
								{
									flag = true;
									DrawSystemFontGlyphShape(letter, g, out textmatrix);
								}
							}
							else
							{
								if (isNegativeFont)
								{
									DocGen.Pdf.Graphics.GraphicsState state = g.Save();
									g.MultiplyTransform(new DocGen.Drawing.Matrix(1f, 0f, 0f, -1f, 0f, 2f * newLocation.Y + 2f * sizeF.Height));
									flag = true;
									DrawSystemFontGlyphShape(c3, g, out textmatrix);
									g.Restore(state);
								}
								else if (RenderingMode != 1)
								{
									flag = true;
									DrawSystemFontGlyphShape(c3, g, out textmatrix);
								}
								renderedText += c3;
							}
						}
						catch (Exception ex)
						{
							exceptions.Exceptions.Append("\r\nCharacter not rendered " + c3 + "\r\n" + ex.StackTrace);
							continue;
						}
						if (num9 < text3.Length)
						{
							newLocation.X += num + CharacterSpacing;
						}
						else
						{
							newLocation.X += num;
						}
					}
					if (!flag)
					{
						if (text4 == string.Empty)
						{
							DrawGlyphs(pathGeom, currentGlyphWidth, g, out textmatrix, c3.ToString());
						}
						else
						{
							DrawGlyphs(pathGeom, currentGlyphWidth, g, out textmatrix, text4);
						}
					}
					m_spaceCheck = false;
					continue;
				}
				SizeF sizeF2 = g.MeasureString(c3.ToString(), skpaint);
				num = sizeF2.Width / 100f * TextScaling;
				try
				{
					if ((byte)c3 > 126 && m_fontEncoding == "MacRomanEncoding" && !Isembeddedfont)
					{
						_ = MacRomanToUnicode[(byte)c3 - 128];
						if (isNegativeFont)
						{
							DocGen.Pdf.Graphics.GraphicsState state = g.Save();
							g.MultiplyTransform(new DocGen.Drawing.Matrix(1f, 0f, 0f, -1f, 0f, 2f * newLocation.Y + 2f * sizeF2.Height));
							flag = true;
							DrawSystemFontGlyphShape(c3, g, out textmatrix);
							g.Restore(state);
						}
						else
						{
							flag = true;
							DrawSystemFontGlyphShape(c3, g, out textmatrix);
						}
					}
					else
					{
						if (isNegativeFont && !Isembeddedfont)
						{
							DocGen.Pdf.Graphics.GraphicsState state = g.Save();
							g.MultiplyTransform(new DocGen.Drawing.Matrix(1f, 0f, 0f, -1f, 0f, 2f * newLocation.Y + 2f * sizeF2.Height));
							flag = true;
							DrawSystemFontGlyphShape(c3, g, out textmatrix);
							g.Restore(state);
						}
						else if (RenderingMode == 1)
						{
							flag = true;
							DrawSystemFontGlyphShape(c3, g, out textmatrix);
						}
						else if (FontEncoding != "Identity-H" && structure.fontType.Value == "TrueType" && structure.GlyphFontFile2 != null)
						{
							if (OctDecMapTable != null && OctDecMapTable.ContainsKey(c3))
							{
								c3 = (char)OctDecMapTable[c3];
							}
							if (pathGeom == null && CharacterMapTable.Count > 0 && CharacterMapTable.ContainsKey((int)c3))
							{
								char letter2 = CharacterMapTable[(int)c3].ToCharArray()[0];
								flag = true;
								bool flag2 = false;
								if (structure != null && !structure.IsMappingDone)
								{
									structure.IsMappingDone = true;
									flag2 = true;
								}
								DrawSystemFontGlyphShape(letter2, g, out textmatrix);
								if (flag2)
								{
									structure.IsMappingDone = false;
								}
							}
							else if (pathGeom != null && pathGeom.PathData.Points.Length == 0 && structure.m_winansiMapTable.ContainsKey(c3))
							{
								flag = true;
								DrawSystemFontGlyphShape(c3, g, out textmatrix);
							}
						}
						else if (structure.GlyphFontFile2 == null)
						{
							if (CharacterMapTable.Count > 0 && CharacterMapTable.ContainsKey((int)c3))
							{
								char letter3 = CharacterMapTable[(int)c3].ToCharArray()[0];
								flag = true;
								bool flag3 = false;
								if (structure != null && !structure.IsMappingDone)
								{
									structure.IsMappingDone = true;
									flag3 = true;
								}
								DrawSystemFontGlyphShape(letter3, g, out textmatrix);
								if (flag3)
								{
									structure.IsMappingDone = false;
								}
							}
							else
							{
								flag = true;
								if (!DrawSystemFontGlyphShape(c3, g, out textmatrix))
								{
									DrawSystemFontGlyph(c3.ToString(), skpaint, new PdfPen(PathBrush).Brush, newLocation, format, g, out newLocation);
								}
							}
						}
						if (CharacterMapTable.ContainsKey((int)c3) && CharacterMapTable.Count > 0)
						{
							char c4 = ' ';
							string text7 = CharacterMapTable[(int)c3];
							_ = new char[text7.Length];
							c4 = text7.ToCharArray()[0];
							if (FontGlyphWidths != null)
							{
								if (structure.fontType.Value == "Type0")
								{
									if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.ContainsKey(c3) && !structure.IsMappingDone)
									{
										currentGlyphWidth = FontGlyphWidths[CidToGidReverseMapTable[c3]];
										currentGlyphWidth *= CharSizeMultiplier;
									}
									else if (FontGlyphWidths.ContainsKey(c3))
									{
										currentGlyphWidth = FontGlyphWidths[c3];
										currentGlyphWidth *= CharSizeMultiplier;
									}
									else if (ReverseMapTable.ContainsKey(c4.ToString()))
									{
										if (FontGlyphWidths.ContainsKey((int)ReverseMapTable[c4.ToString()]))
										{
											currentGlyphWidth = g.MeasureString(c4.ToString(), skpaint).Width / 100f * TextScaling / textFont.Size;
										}
										else
										{
											currentGlyphWidth = DefaultGlyphWidth;
											currentGlyphWidth *= CharSizeMultiplier;
										}
									}
								}
								else if (structure.fontType.Value == "TrueType" && FontGlyphWidths.ContainsKey(c3))
								{
									currentGlyphWidth = FontGlyphWidths[c3];
									currentGlyphWidth *= CharSizeMultiplier;
								}
							}
							else if (FontGlyphWidths == null)
							{
								currentGlyphWidth = DefaultGlyphWidth;
								currentGlyphWidth *= CharSizeMultiplier;
							}
						}
						else if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.Count > 1)
						{
							if (CidToGidReverseMapTable.ContainsKey(c3))
							{
								int key = CidToGidReverseMapTable[c3];
								if (FontGlyphWidths != null && FontGlyphWidths.ContainsKey(key))
								{
									currentGlyphWidth = FontGlyphWidths[key];
									currentGlyphWidth *= CharSizeMultiplier;
								}
							}
						}
						else if (FontGlyphWidths != null)
						{
							if (FontGlyphWidths.ContainsKey(c3))
							{
								currentGlyphWidth = FontGlyphWidths[c3];
								currentGlyphWidth *= CharSizeMultiplier;
							}
							else
							{
								currentGlyphWidth = DefaultGlyphWidth * CharSizeMultiplier;
							}
						}
					}
				}
				catch (Exception ex2)
				{
					exceptions.Exceptions.Append("\r\nCharacter not rendered " + c3 + "\r\n" + ex2.StackTrace);
					continue;
				}
				if (num9 < text3.Length)
				{
					newLocation.X += CharacterSpacing;
				}
				if (!flag)
				{
					DrawGlyphs(pathGeom, currentGlyphWidth, g, out textmatrix, c3.ToString());
				}
				if (pathGeom != null && pathGeom.PathData.Points.Length == 0 && (structure.m_winansiMapTable.ContainsKey(c3) || (structure.CidToGidMap != null && CharacterMapTable != null && CidToGidReverseMapTable != null && CharacterMapTable.Count == structure.CidToGidMap.Count && CidToGidReverseMapTable.Count > 0 && CidToGidReverseMapTable.ContainsKey(c3))))
				{
					flag = false;
				}
			}
		}
		return newLocation.X - x;
	}

	private void GetMacEncodeTable()
	{
		m_macEncodeTable = new Dictionary<int, string>();
		m_macEncodeTable.Add(127, " ");
		m_macEncodeTable.Add(128, "Ä");
		m_macEncodeTable.Add(129, "Å");
		m_macEncodeTable.Add(130, "Ç");
		m_macEncodeTable.Add(131, "É");
		m_macEncodeTable.Add(132, "Ñ");
		m_macEncodeTable.Add(133, "Ö");
		m_macEncodeTable.Add(134, "Ü");
		m_macEncodeTable.Add(135, "á");
		m_macEncodeTable.Add(136, "à");
		m_macEncodeTable.Add(137, "â");
		m_macEncodeTable.Add(138, "ä");
		m_macEncodeTable.Add(139, "ã");
		m_macEncodeTable.Add(140, "å");
		m_macEncodeTable.Add(141, "ç");
		m_macEncodeTable.Add(142, "é");
		m_macEncodeTable.Add(143, "è");
		m_macEncodeTable.Add(144, "ê");
		m_macEncodeTable.Add(145, "ë");
		m_macEncodeTable.Add(146, "í");
		m_macEncodeTable.Add(147, "ì");
		m_macEncodeTable.Add(148, "î");
		m_macEncodeTable.Add(149, "ï");
		m_macEncodeTable.Add(150, "ñ");
		m_macEncodeTable.Add(151, "ó");
		m_macEncodeTable.Add(152, "ò");
		m_macEncodeTable.Add(153, "ô");
		m_macEncodeTable.Add(154, "ö");
		m_macEncodeTable.Add(155, "õ");
		m_macEncodeTable.Add(156, "ú");
		m_macEncodeTable.Add(157, "ù");
		m_macEncodeTable.Add(158, "û");
		m_macEncodeTable.Add(159, "ü");
		m_macEncodeTable.Add(160, "†");
		m_macEncodeTable.Add(161, "°");
		m_macEncodeTable.Add(162, "¢");
		m_macEncodeTable.Add(163, "£");
		m_macEncodeTable.Add(164, "§");
		m_macEncodeTable.Add(165, "•");
		m_macEncodeTable.Add(166, "¶");
		m_macEncodeTable.Add(167, "ß");
		m_macEncodeTable.Add(168, "®");
		m_macEncodeTable.Add(169, "©");
		m_macEncodeTable.Add(170, "™");
		m_macEncodeTable.Add(171, "\u00b4");
		m_macEncodeTable.Add(172, "\u00a8");
		m_macEncodeTable.Add(173, "≠");
		m_macEncodeTable.Add(174, "Æ");
		m_macEncodeTable.Add(175, "Ø");
		m_macEncodeTable.Add(176, "∞");
		m_macEncodeTable.Add(177, "±");
		m_macEncodeTable.Add(178, "≤");
		m_macEncodeTable.Add(179, "≥");
		m_macEncodeTable.Add(180, "¥");
		m_macEncodeTable.Add(181, "µ");
		m_macEncodeTable.Add(182, "∂");
		m_macEncodeTable.Add(183, "∑");
		m_macEncodeTable.Add(184, "∏");
		m_macEncodeTable.Add(185, "π");
		m_macEncodeTable.Add(186, "∫");
		m_macEncodeTable.Add(187, "ª");
		m_macEncodeTable.Add(188, "º");
		m_macEncodeTable.Add(189, "Ω");
		m_macEncodeTable.Add(190, "æ");
		m_macEncodeTable.Add(191, "ø");
		m_macEncodeTable.Add(192, "¿");
		m_macEncodeTable.Add(193, "¡");
		m_macEncodeTable.Add(194, "¬");
		m_macEncodeTable.Add(195, "√");
		m_macEncodeTable.Add(196, "ƒ");
		m_macEncodeTable.Add(197, "≈");
		m_macEncodeTable.Add(198, "∆");
		m_macEncodeTable.Add(199, "«");
		m_macEncodeTable.Add(200, "»");
		m_macEncodeTable.Add(201, "…");
		m_macEncodeTable.Add(202, " ");
		m_macEncodeTable.Add(203, "À");
		m_macEncodeTable.Add(204, "Ã");
		m_macEncodeTable.Add(205, "Õ");
		m_macEncodeTable.Add(206, "Œ");
		m_macEncodeTable.Add(207, "œ");
		m_macEncodeTable.Add(208, "–");
		m_macEncodeTable.Add(209, "—");
		m_macEncodeTable.Add(210, "“");
		m_macEncodeTable.Add(211, "”");
		m_macEncodeTable.Add(212, "‘");
		m_macEncodeTable.Add(213, "’");
		m_macEncodeTable.Add(214, "÷");
		m_macEncodeTable.Add(215, "◊");
		m_macEncodeTable.Add(216, "ÿ");
		m_macEncodeTable.Add(217, "Ÿ");
		m_macEncodeTable.Add(218, "⁄");
		m_macEncodeTable.Add(219, "€");
		m_macEncodeTable.Add(220, "‹");
		m_macEncodeTable.Add(221, "›");
		m_macEncodeTable.Add(222, "ﬁ");
		m_macEncodeTable.Add(223, "ﬂ");
		m_macEncodeTable.Add(224, "‡");
		m_macEncodeTable.Add(225, "·");
		m_macEncodeTable.Add(226, ",");
		m_macEncodeTable.Add(227, "„");
		m_macEncodeTable.Add(228, "‰");
		m_macEncodeTable.Add(229, "Â");
		m_macEncodeTable.Add(230, "Ê");
		m_macEncodeTable.Add(231, "Á");
		m_macEncodeTable.Add(232, "Ë");
		m_macEncodeTable.Add(233, "È");
		m_macEncodeTable.Add(234, "Í");
		m_macEncodeTable.Add(235, "Î");
		m_macEncodeTable.Add(236, "Ï");
		m_macEncodeTable.Add(237, "Ì");
		m_macEncodeTable.Add(238, "Ó");
		m_macEncodeTable.Add(239, "Ô");
		m_macEncodeTable.Add(240, "\uf8ff");
		m_macEncodeTable.Add(241, "Ò");
		m_macEncodeTable.Add(242, "Ú");
		m_macEncodeTable.Add(243, "Û");
		m_macEncodeTable.Add(244, "Ù");
		m_macEncodeTable.Add(245, "ı");
		m_macEncodeTable.Add(246, "ˆ");
		m_macEncodeTable.Add(247, "\u02dc");
		m_macEncodeTable.Add(248, "\u00af");
		m_macEncodeTable.Add(249, "\u02d8");
		m_macEncodeTable.Add(250, "\u02d9");
		m_macEncodeTable.Add(251, "\u02da");
		m_macEncodeTable.Add(252, "\u00b8");
		m_macEncodeTable.Add(253, "\u02dd");
		m_macEncodeTable.Add(254, "\u02db");
		m_macEncodeTable.Add(255, "ˇ");
	}

	private void RenderReverseMapTableByte(char character, GraphicsHelper g)
	{
		new PdfUnitConverter();
		g.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		Glyph glyph = new Glyph();
		glyph.FontSize = FontSize;
		glyph.FontFamily = FontName;
		glyph.FontStyle = FontStyle;
		glyph.TransformMatrix = GetTextRenderingMatrix();
		glyph.Name = character.ToString();
		glyph.HorizontalScaling = TextHorizontalScaling;
		glyph.CharId = new CharCode((int)character);
		glyph.CharSpacing = CharacterSpacing;
		string[] standardFontEncodingNames = GetStandardFontEncodingNames();
		byte[] bytes = Encoding.UTF8.GetBytes(character.ToString());
		if (structure.ReverseDictMapping.ContainsKey(character.ToString()))
		{
			float num = structure.ReverseDictMapping[character.ToString()];
			if (structure.DifferencesDictionary.ContainsKey(num.ToString()))
			{
				Dictionary<string, string> differencesDictionary = structure.DifferencesDictionary;
				int num2 = character;
				glyph.Name = FontStructure.GetCharCode(differencesDictionary[num2.ToString()]);
			}
			else
			{
				bytes[0] = (byte)num;
				glyph.Name = standardFontEncodingNames[bytes[0]];
			}
		}
		else if (OctDecMapTable != null && OctDecMapTable.ContainsKey(character))
		{
			char c = (char)OctDecMapTable[character];
			glyph.Name = standardFontEncodingNames[(uint)c];
		}
		else
		{
			Dictionary<string, string> differencesDictionary2 = structure.DifferencesDictionary;
			int num2 = character;
			if (differencesDictionary2.ContainsKey(num2.ToString()))
			{
				Dictionary<string, string> differencesDictionary3 = structure.DifferencesDictionary;
				num2 = character;
				glyph.Name = FontStructure.GetCharCode(differencesDictionary3[num2.ToString()]);
			}
			else if (standardFontEncodingNames.Length > character)
			{
				glyph.Name = standardFontEncodingNames[(uint)character];
			}
			else
			{
				glyph.Name = standardFontEncodingNames[bytes[0]];
			}
		}
		glyph.Width = GetGlyphWidth(glyph);
		FontSource.GetGlyphOutlines(glyph, 100.0);
		GlyphToSLCoordinates(glyph);
		new PdfElementsRendererNet().RenderGlyph(glyph);
		Matrix identity = Matrix.Identity;
		identity.Scale(0.01, 0.01, 0.0, 0.0);
		identity.Translate(0.0, 1.0);
		transformations.PushTransform(identity * glyph.TransformMatrix);
		DocGen.Drawing.Matrix matrix = (DocGen.Drawing.Matrix)g.Transform.Clone();
		matrix.Multiply(GetTransformationMatrix(transformations.CurrentTransform));
		g.Transform = matrix;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		UpdateTextMatrix(glyph);
		transformations.PopTransform();
	}

	private void RenderReverseMapTableByte(char character, GraphicsHelper g, string text)
	{
		PdfUnitConverter pdfUnitConverter = new PdfUnitConverter();
		g.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		Glyph glyph = new Glyph();
		glyph.FontSize = FontSize;
		glyph.FontFamily = FontName;
		glyph.FontStyle = FontStyle;
		glyph.TransformMatrix = GetTextRenderingMatrix();
		glyph.Name = character.ToString();
		glyph.HorizontalScaling = TextHorizontalScaling;
		glyph.CharId = new CharCode((int)character);
		glyph.CharSpacing = CharacterSpacing;
		string[] standardFontEncodingNames = GetStandardFontEncodingNames();
		byte[] bytes = Encoding.UTF8.GetBytes(character.ToString());
		if (structure.ReverseDictMapping.ContainsKey(character.ToString()))
		{
			float num = structure.ReverseDictMapping[character.ToString()];
			if (structure.DifferencesDictionary.ContainsKey(num.ToString()))
			{
				glyph.Name = FontStructure.GetCharCode(structure.DifferencesDictionary[num.ToString()]);
			}
			else
			{
				bytes[0] = (byte)num;
				glyph.Name = standardFontEncodingNames[bytes[0]];
			}
		}
		else if (OctDecMapTable != null && OctDecMapTable.ContainsKey(character))
		{
			char c = (char)OctDecMapTable[character];
			glyph.Name = standardFontEncodingNames[(uint)c];
		}
		else
		{
			Dictionary<string, string> differencesDictionary = structure.DifferencesDictionary;
			int num2 = character;
			if (differencesDictionary.ContainsKey(num2.ToString()))
			{
				Dictionary<string, string> differencesDictionary2 = structure.DifferencesDictionary;
				num2 = character;
				glyph.Name = FontStructure.GetCharCode(differencesDictionary2[num2.ToString()]);
			}
			else if (standardFontEncodingNames.Length > character)
			{
				glyph.Name = standardFontEncodingNames[(uint)character];
			}
			else
			{
				glyph.Name = standardFontEncodingNames[bytes[0]];
			}
		}
		glyph.Width = GetGlyphWidth(glyph);
		FontSource.GetGlyphOutlines(glyph, 100.0);
		new PdfElementsRendererNet().RenderGlyph(glyph);
		Matrix identity = Matrix.Identity;
		identity.Scale(0.01, 0.01, 0.0, 0.0);
		identity.Translate(0.0, 1.0);
		transformations.PushTransform(identity * glyph.TransformMatrix);
		DocGen.Drawing.Matrix matrix = g.Transform.Clone() as DocGen.Drawing.Matrix;
		matrix.Multiply(GetTransformationMatrix(transformations.CurrentTransform));
		g.Transform = matrix;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		glyph.ToUnicode = text;
		float num3 = 0f;
		num3 = ((glyph.TransformMatrix.M11 > 0.0) ? ((float)glyph.TransformMatrix.M11) : ((glyph.TransformMatrix.M12 == 0.0 || glyph.TransformMatrix.M21 == 0.0) ? ((float)glyph.FontSize) : ((!(glyph.TransformMatrix.M12 < 0.0)) ? ((float)glyph.TransformMatrix.M12) : ((float)(0.0 - glyph.TransformMatrix.M12)))));
		if (pageRotation == 90f || pageRotation == 270f)
		{
			if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
			{
				glyph.IsRotated = false;
				glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num3 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num3, num3));
			}
			else
			{
				glyph.IsRotated = true;
				if (IsFindText && pageRotation == 90f)
				{
					glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num3 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num3, num3));
				}
				else
				{
					glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX + (num3 + (float)(glyph.Ascent / 1000.0)) * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetY - num3 * matrix.Elements[2], PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num3 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size((double)num3 - glyph.Ascent / 1000.0, glyph.Width * (double)num3));
				}
			}
		}
		else
		{
			if (pageRotation == 180f)
			{
				if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
				{
					glyph.IsRotated = true;
				}
				else
				{
					glyph.IsRotated = false;
				}
			}
			glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConverter.ConvertFromPixels(matrix.OffsetY - num3 * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num3, num3));
		}
		if (structure.CharacterMapTable != null && structure.CharacterMapTable.ContainsKey((int)character))
		{
			glyph.ToUnicode = structure.CharacterMapTable[(int)character];
		}
		if (glyph.ToUnicode.Length != 1)
		{
			textElementGlyphList.Add(glyph);
			for (int i = 0; i < glyph.ToUnicode.Length - 1; i++)
			{
				Glyph item = new Glyph();
				textElementGlyphList.Add(item);
			}
		}
		else
		{
			textElementGlyphList.Add(glyph);
		}
		GetFontSize(glyph, num3);
		if (structure.CharacterMapTable != null && structure.CharacterMapTable.ContainsKey((int)character))
		{
			renderedText += glyph.ToString();
		}
		else
		{
			renderedText += character;
		}
		UpdateTextMatrix(glyph);
		transformations.PopTransform();
	}

	private void DrawPath(GraphicsHelper g, SystemFontGlyph glyph, string charString)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		foreach (SystemFontPathFigure outline in glyph.Outlines)
		{
			graphicsPath.StartFigure();
			PointF pointF = new PointF((float)outline.StartPoint.X, (float)outline.StartPoint.Y);
			DocGen.Drawing.Matrix transformationMatrix = GetTransformationMatrix(SystemFontMatrix.Identity);
			foreach (SystemFontPathSegment segment in outline.Segments)
			{
				if (segment is SystemFontLineSegment)
				{
					SystemFontLineSegment systemFontLineSegment = (SystemFontLineSegment)segment;
					PointF[] array = new PointF[2]
					{
						pointF,
						new PointF((float)systemFontLineSegment.Point.X, (float)systemFontLineSegment.Point.Y)
					};
					transformationMatrix.TransformPoints(array);
					graphicsPath.AddLine(array[0], array[1]);
					pointF = new PointF((float)systemFontLineSegment.Point.X, (float)systemFontLineSegment.Point.Y);
				}
				else if (segment is SystemFontBezierSegment)
				{
					SystemFontBezierSegment systemFontBezierSegment = segment as SystemFontBezierSegment;
					PointF[] array2 = new PointF[4]
					{
						pointF,
						new PointF((float)systemFontBezierSegment.Point1.X, (float)systemFontBezierSegment.Point1.Y),
						new PointF((float)systemFontBezierSegment.Point2.X, (float)systemFontBezierSegment.Point2.Y),
						new PointF((float)systemFontBezierSegment.Point3.X, (float)systemFontBezierSegment.Point3.Y)
					};
					transformationMatrix.TransformPoints(array2);
					graphicsPath.AddBezier(array2[0], array2[1], array2[2], array2[3]);
					pointF = new PointF((float)systemFontBezierSegment.Point3.X, (float)systemFontBezierSegment.Point3.Y);
				}
				else if (segment is SystemFontQuadraticBezierSegment)
				{
					SystemFontQuadraticBezierSegment systemFontQuadraticBezierSegment = segment as SystemFontQuadraticBezierSegment;
					PointF[] array3 = new PointF[3]
					{
						pointF,
						new PointF((float)systemFontQuadraticBezierSegment.Point1.X, (float)systemFontQuadraticBezierSegment.Point1.Y),
						new PointF((float)systemFontQuadraticBezierSegment.Point2.X, (float)systemFontQuadraticBezierSegment.Point2.Y)
					};
					transformationMatrix.TransformPoints(array3);
					graphicsPath.AddBezier(array3[0], array3[1], array3[2], array3[2]);
					pointF = new PointF((float)systemFontQuadraticBezierSegment.Point2.X, (float)systemFontQuadraticBezierSegment.Point2.Y);
				}
			}
			if (outline.IsClosed)
			{
				graphicsPath.CloseFigure();
			}
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.PageUnit = GraphicsUnit.Pixel;
		_ = PathBrush;
	}

	private DocGen.Drawing.Matrix GetTransformationMatrix(SystemFontMatrix transform)
	{
		return new DocGen.Drawing.Matrix((float)transform.M11, (float)transform.M12, (float)transform.M21, (float)transform.M22, (float)transform.OffsetX, (float)transform.OffsetY);
	}

	private ushort GetGlyphID(string glyphName)
	{
		if (IsNonsymbolic || structure.BaseFontEncoding == "MacRomanEncoding" || structure.BaseFontEncoding == "WinAnsiEncoding")
		{
			SystemFontCMapTable cMapTable = openTypeFontSource.CMap.GetCMapTable(3, 1);
			if (structure.CharacterMapTable != null && structure.CharacterMapTable.ContainsValue(glyphName) && structure.fontType.Value == "Type0" && structure.CidToGidReverseMapTable.Count == 0)
			{
				foreach (double key in structure.CharacterMapTable.Keys)
				{
					int num = (int)key;
					if (structure.CharacterMapTable[num] == glyphName)
					{
						return (ushort)num;
					}
				}
			}
			if (cMapTable != null)
			{
				if (FontEncoding == "Identity-H")
				{
					return cMapTable.GetGlyphId(glyphName[0]);
				}
				return GetGlyphsFromMicrosoftUnicodeWithEncoding(cMapTable, glyphName);
			}
			cMapTable = openTypeFontSource.CMap.GetCMapTable(1, 0);
			if (cMapTable != null)
			{
				if (FontEncoding == "Identity-H")
				{
					return cMapTable.GetGlyphId(glyphName[0]);
				}
				return GetGlyphsFromMacintoshRomanWithEncoding(cMapTable, glyphName);
			}
			return 0;
		}
		SystemFontCMapTable cMapTable2 = openTypeFontSource.CMap.GetCMapTable(3, 0);
		if (cMapTable2 != null)
		{
			return GetGlyphsFromMicrosoftSymbolWithoutEncoding(cMapTable2, glyphName);
		}
		cMapTable2 = openTypeFontSource.CMap.GetCMapTable(1, 0);
		if (cMapTable2 != null)
		{
			return GetGlyphsFromMacintoshRomanWithoutEncoding(cMapTable2, glyphName);
		}
		if (FontEncoding != "GBK-EUC-H")
		{
			cMapTable2 = openTypeFontSource.CMap.GetCMapTable(3, 1);
			if (cMapTable2 != null)
			{
				return GetGlyphsFromMacintoshRomanWithoutEncoding(cMapTable2, glyphName);
			}
		}
		return 0;
	}

	private ushort GetGlyphsFromMacintoshRomanWithoutEncoding(SystemFontCMapTable roman, string glyphName)
	{
		byte b = (byte)glyphName[0];
		return new Glyph
		{
			CharId = new CharCode(b),
			GlyphId = roman.GetGlyphId(b)
		}.GlyphId;
	}

	private ushort GetGlyphsFromMicrosoftSymbolWithoutEncoding(SystemFontCMapTable unicode, string glyphName)
	{
		CalculateByteToAppend(unicode);
		byte b = (byte)glyphName[0];
		if (TryAppendByte(b, out var res))
		{
			return new Glyph
			{
				CharId = new CharCode(b),
				GlyphId = unicode.GetGlyphId(res)
			}.GlyphId;
		}
		return 0;
	}

	private void CalculateByteToAppend(SystemFontCMapTable unicode)
	{
		if (firstCode.HasValue)
		{
			return;
		}
		try
		{
			firstCode = new CharCode(unicode.FirstCode);
		}
		catch (NotSupportedException)
		{
			firstCode = default(CharCode);
		}
	}

	private bool TryAppendByte(byte b, out ushort res)
	{
		res = 0;
		if (!firstCode.HasValue || firstCode.Value.IsEmpty)
		{
			return false;
		}
		try
		{
			CharCode charCode = new CharCode(new byte[2]
			{
				firstCode.Value.Bytes[0],
				b
			});
			res = (ushort)charCode.IntValue;
			return true;
		}
		catch
		{
			return false;
		}
	}

	private ushort GetGlyphsFromMacintoshRomanWithEncoding(SystemFontCMapTable cMapTable, string glyphName)
	{
		byte b = (byte)glyphName[0];
		Glyph obj = new Glyph
		{
			CharId = new CharCode(b)
		};
		byte charId = SystemFontPredefinedEncoding.StandardMacRomanEncoding.GetCharId(glyphName);
		obj.GlyphId = cMapTable.GetGlyphId(charId);
		return obj.GlyphId;
	}

	private ushort GetGlyphsFromMicrosoftUnicodeWithEncoding(SystemFontCMapTable unicode, string glyphName)
	{
		ushort result = 0;
		try
		{
			byte b = (byte)glyphName[0];
			string name = GetName(b);
			if (name != null)
			{
				string charCode = FontStructure.GetCharCode(name);
				result = (SystemFontAdobeGlyphList.IsSupportedPdfName(name) ? unicode.GetGlyphId(SystemFontAdobeGlyphList.GetUnicode(name)) : ((!SystemFontAdobeGlyphList.IsSupportedPdfName(charCode)) ? GetGlyphIdFromPostTable(GetName(b)) : unicode.GetGlyphId(SystemFontAdobeGlyphList.GetUnicode(charCode))));
			}
		}
		catch
		{
			result = 0;
		}
		return result;
	}

	private string GetName(byte b)
	{
		Initialize();
		return names[b];
	}

	private void Initialize()
	{
		if (m_fontEncoding != null && m_fontEncoding != string.Empty)
		{
			SystemFontPredefinedEncoding predefinedEncoding = SystemFontPredefinedEncoding.GetPredefinedEncoding(m_fontEncoding);
			if (predefinedEncoding != null)
			{
				names = predefinedEncoding.GetNames();
			}
			else
			{
				predefinedEncoding = SystemFontPredefinedEncoding.GetPredefinedEncoding(structure.BaseFontEncoding);
				if (predefinedEncoding != null)
				{
					names = predefinedEncoding.GetNames();
				}
			}
		}
		if (names == null)
		{
			names = SystemFontPredefinedEncoding.StandardEncoding.GetNames();
		}
		MapDifferenceElement();
	}

	private void MapDifferenceElement()
	{
		if (structure.DifferencesDictionary == null)
		{
			return;
		}
		int result = 0;
		List<string> list = new List<string>(structure.DifferencesDictionary.Keys);
		List<string> list2 = new List<string>(structure.DifferencesDictionary.Values);
		for (int i = 0; i < list.Count; i++)
		{
			int.TryParse(list[i], out result);
			if (result < 256)
			{
				names[result] = list2[i];
			}
		}
	}

	private ushort GetGlyphIdFromPostTable(string name)
	{
		if (openTypeFontSource.Post == null)
		{
			return 0;
		}
		return openTypeFontSource.Post.GetGlyphId(name);
	}

	private void DrawSystemFontGlyph(string str, SKPaint paint, PdfBrush brush, PointF currentLocation, StringFormat format, GraphicsHelper g, out PointF newLocation)
	{
		newLocation = currentLocation;
		if (str == " ")
		{
			new StringFormat(StringFormat.GenericDefault).FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
			float width = g.MeasureString(" ", paint).Width;
			newLocation.X += width;
		}
		else
		{
			g.PageUnit = GraphicsUnit.Point;
			g.DrawString(str, paint, currentLocation);
			float num = g.MeasureString(str, paint).Width;
			if (FontGlyphWidths != null && !m_isMpdfFont && FontGlyphWidths.ContainsKey(str[0]))
			{
				num = (float)FontGlyphWidths[str[0]] * (CharSizeMultiplier * FontSize);
			}
			newLocation.X += num;
		}
		renderedText += str;
	}

	public void DrawGlyphs(GraphicsPath path, float glyphwidth, GraphicsHelper g, out Matrix temptextmatrix, string glyphChar)
	{
		DocGen.Drawing.Matrix transform = g.Transform;
		g.PageUnit = GraphicsUnit.Pixel;
		g.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		GraphicsUnit pageUnit = g.PageUnit;
		Glyph glyph = new Glyph();
		glyph.FontSize = FontSize;
		glyph.FontFamily = FontName;
		SetEmbededFontName(glyph);
		glyph.FontStyle = FontStyle;
		glyph.TransformMatrix = GetTextRenderingMatrix();
		glyph.HorizontalScaling = TextHorizontalScaling;
		glyph.Width = glyphwidth;
		glyph.CharSpacing = CharacterSpacing;
		if (glyphChar == " " && ((CharID.BytesCount == 1 && CharID.Bytes[0] == 32) || CharID.IsEmpty))
		{
			glyph.WordSpacing = WordSpacing;
		}
		Matrix identity = Matrix.Identity;
		identity.Scale(0.01, 0.01, 0.0, 0.0);
		identity.Translate(0.0, 1.0);
		transformations.PushTransform(identity * glyph.TransformMatrix);
		DocGen.Drawing.Matrix matrix = (DocGen.Drawing.Matrix)g.Transform.Clone();
		matrix.Multiply(GetTransformationMatrix(transformations.CurrentTransform));
		g.Transform = matrix;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		if (!structure.IsMappingDone)
		{
			if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.ContainsKey(Convert.ToChar(glyphChar)) && structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
			{
				glyphChar = CharacterMapTable[CidToGidReverseMapTable[Convert.ToChar(glyphChar)]];
			}
			else if (structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
			{
				glyphChar = ((structure.tempStringList.Count <= 0 && (structure.CharacterMapTable.Count == structure.ReverseMapTable.Count || !(structure.FontName == "AllAndNone"))) ? structure.MapCharactersFromTable(glyphChar.ToString()) : structure.CharacterMapTable[(int)Convert.ToChar(glyphChar)]);
			}
			else if (structure.DifferencesDictionary != null && structure.DifferencesDictionary.Count > 0)
			{
				glyphChar = structure.MapDifferences(glyphChar.ToString());
			}
			else if (structure.CidToGidReverseMapTable != null && structure.CidToGidReverseMapTable.ContainsKey(Convert.ToChar(glyphChar)))
			{
				glyphChar = ((char)structure.CidToGidReverseMapTable[Convert.ToChar(glyphChar)]).ToString();
			}
			if (glyphChar.Contains("\u0092"))
			{
				glyphChar = glyphChar.Replace("\u0092", "’");
			}
		}
		float num = 0f;
		num = ((glyph.TransformMatrix.M11 > 0.0) ? ((float)glyph.TransformMatrix.M11) : ((glyph.TransformMatrix.M12 == 0.0 || glyph.TransformMatrix.M21 == 0.0) ? ((float)glyph.FontSize) : ((!(glyph.TransformMatrix.M12 < 0.0)) ? ((float)glyph.TransformMatrix.M12) : ((float)(0.0 - glyph.TransformMatrix.M12)))));
		glyph.ToUnicode = glyphChar;
		PdfUnitConverter pdfUnitConverter = new PdfUnitConverter();
		if (pageRotation == 90f || pageRotation == 270f)
		{
			if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
			{
				glyph.IsRotated = false;
				glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num, num));
			}
			else
			{
				glyph.IsRotated = true;
				if (IsFindText && pageRotation == 90f)
				{
					glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX + (num + (float)(glyph.Ascent / 1000.0)) * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConverter.ConvertFromPixels(matrix.OffsetY - num * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor), new Size(num, glyph.Width * (double)num));
				}
				else
				{
					glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX + (num + (float)(glyph.Ascent / 1000.0)) * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConverter.ConvertFromPixels(matrix.OffsetY + num * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor), new Size(num, glyph.Width * (double)num));
				}
			}
		}
		else if (matrix.Elements[1] != 0f && matrix.Elements[2] != 0f)
		{
			glyph.IsRotated = true;
			if (matrix.Elements[1] < 0f && matrix.Elements[2] > 0f)
			{
				glyph.RotationAngle = 270;
			}
			else if (matrix.Elements[1] > 0f && matrix.Elements[2] < 0f)
			{
				glyph.RotationAngle = 90;
			}
			else if (matrix.Elements[1] < 0f && matrix.Elements[2] < 0f)
			{
				glyph.RotationAngle = 180;
			}
			if (IsFindText && pageRotation == 90f)
			{
				glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetY, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num, num));
			}
			else
			{
				glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX + (num + (float)(glyph.Ascent / 1000.0)) * matrix.Elements[2], PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConverter.ConvertFromPixels(matrix.OffsetY - num * matrix.Elements[2], PdfGraphicsUnit.Point) - pdfUnitConverter.ConvertFromPixels(num * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num, num));
			}
		}
		else
		{
			if (pageRotation == 180f)
			{
				if (matrix.Elements[1] == 0f && matrix.Elements[2] == 0f)
				{
					glyph.IsRotated = false;
				}
				else
				{
					glyph.IsRotated = true;
				}
			}
			glyph.BoundingRect = new Rect(new Point(pdfUnitConverter.ConvertFromPixels(matrix.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConverter.ConvertFromPixels(matrix.OffsetY - num * zoomFactor * (g.DpiY / 96f), PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num, num));
		}
		if (structure.IsAdobeJapanFont)
		{
			if (structure.AdobeJapanCidMapTable.ContainsKey(Convert.ToChar(glyphChar)))
			{
				glyphChar = structure.AdobeJapanCidMapTableGlyphParser(glyphChar);
			}
			glyph.ToUnicode = glyphChar;
		}
		if (glyph.ToUnicode.Length != 1)
		{
			textElementGlyphList.Add(glyph);
			for (int i = 0; i < glyph.ToUnicode.Length - 1; i++)
			{
				Glyph item = new Glyph();
				textElementGlyphList.Add(item);
			}
		}
		else
		{
			if (glyphChar.ToCharArray()[0] == '\u00a0')
			{
				glyphChar = ' '.ToString();
				glyph.ToUnicode = glyphChar;
			}
			textElementGlyphList.Add(glyph);
		}
		GetFontSize(glyph, num);
		UpdateTextMatrix(glyph);
		transformations.PopTransform();
		g.Transform = transform;
		g.PageUnit = pageUnit;
		temptextmatrix = textLineMatrix;
		renderedText += glyphChar;
	}

	private void SetEmbededFontName(Glyph glyph)
	{
		if (glyph != null && m_isExtractTextData && !string.IsNullOrEmpty(m_embeddedFontFamily) && m_embeddedFontFamily != glyph.FontFamily)
		{
			glyph.EmbededFontFamily = m_embeddedFontFamily;
		}
	}

	private void BackupEmbededFontName(string matrixImplFontName)
	{
		if (m_isExtractTextData)
		{
			m_embeddedFontFamily = matrixImplFontName;
		}
	}

	private void GetFontSize(Glyph glyph, float tempFontSize)
	{
		if (glyph != null && m_isExtractTextData && tempFontSize > 0f && (double)tempFontSize != glyph.FontSize)
		{
			glyph.MatrixFontSize = tempFontSize;
		}
	}

	public void DrawType3Glyphs(ImageHelper image, GraphicsHelper g)
	{
		GraphicsUnit pageUnit = g.PageUnit;
		DocGen.Drawing.Matrix transform = g.Transform;
		g.PageUnit = GraphicsUnit.Pixel;
		g.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		Glyph glyph = new Glyph();
		glyph.FontSize = FontSize;
		glyph.TransformMatrix = GetTextRenderingMatrix();
		glyph.HorizontalScaling = TextHorizontalScaling;
		glyph.CharSpacing = CharacterSpacing;
		Matrix identity = Matrix.Identity;
		transformations.PushTransform(identity * glyph.TransformMatrix);
		DocGen.Drawing.Matrix matrix = (DocGen.Drawing.Matrix)g.Transform.Clone();
		matrix.Multiply(GetTransformationMatrix(transformations.CurrentTransform));
		g.Transform = matrix;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		new PdfUnitConverter();
		g.DrawImage(image, new RectangleF(0f, 0f, 1f, 1f));
		transformations.PopTransform();
		g.Transform = transform;
		g.PageUnit = pageUnit;
	}

	internal string MapEscapeSequence(char letter)
	{
		return letter switch
		{
			' ' => '\0'.ToString(), 
			'\r' => '\ufff9'.ToString(), 
			'\v' => '￼'.ToString(), 
			'\n' => '\ufffb'.ToString(), 
			'\t' => '\ufffa'.ToString(), 
			_ => letter.ToString(), 
		};
	}

	internal bool isMpdfaaFonts()
	{
		bool result = false;
		if (structure.FontDictionary.ContainsKey("BaseFont"))
		{
			PdfName pdfName = structure.FontDictionary["BaseFont"] as PdfName;
			if (pdfName != null)
			{
				string text = ((!pdfName.Value.Contains("+")) ? pdfName.Value : pdfName.Value.Split('+')[0]);
				if (text == "MPDFAA")
				{
					result = true;
				}
			}
		}
		return result;
	}

	private string SkipEscapeSequence(string text)
	{
		int num = -1;
		do
		{
			num = text.IndexOf("\\", num + 1);
			if (num < 0)
			{
				continue;
			}
			if (text.Length > num + 1)
			{
				string text2 = text[num + 1].ToString();
				if (num >= 0)
				{
					switch (text2)
					{
					case "\\":
					case "(":
					case ")":
					case "\n":
						text = text.Remove(num, 1);
						break;
					}
				}
			}
			else
			{
				text = text.Remove(num, 1);
				num = -1;
			}
		}
		while (num >= 0);
		if (text.Contains("\n"))
		{
			text = text.Replace("\n", "");
		}
		return text;
	}

	internal void CheckFontStyle(string fontName)
	{
		if (!string.IsNullOrEmpty(fontName))
		{
			if (fontName.Contains("Regular"))
			{
				FontStyle = FontStyle.Regular;
			}
			else if (fontName.Contains("Bold"))
			{
				FontStyle = FontStyle.Bold;
			}
			else if (fontName.Contains("Italic"))
			{
				FontStyle = FontStyle.Italic;
			}
		}
	}

	internal static string CheckFontName(string fontName)
	{
		string text = fontName;
		if (!string.IsNullOrEmpty(fontName))
		{
			if (text.Contains("#20"))
			{
				text = text.Replace("#20", " ");
			}
			string[] array = new string[1] { "" };
			int num = 0;
			for (int i = 0; i < text.Length; i++)
			{
				string text2 = text.Substring(i, 1);
				if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Contains(text2) && i > 0 && !"ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Contains(text[i - 1].ToString()))
				{
					num++;
					string[] array2 = new string[num + 1];
					System.Array.Copy(array, 0, array2, 0, num);
					array = array2;
				}
				array[num] += text2;
			}
			fontName = string.Empty;
			string[] array3 = array;
			for (int j = 0; j < array3.Length; j++)
			{
				string text3 = array3[j].Trim();
				fontName = fontName + text3 + " ";
			}
			if (fontName.Contains("Zapf"))
			{
				fontName = "MS Gothic";
			}
			if (fontName.Contains("Times"))
			{
				fontName = "Times New Roman";
			}
			if (fontName == "Bookshelf Symbol Seven")
			{
				fontName = "Bookshelf Symbol 7";
			}
			if (fontName.Contains("Courier"))
			{
				fontName = "Courier New";
			}
			if (fontName.Contains("Song Std"))
			{
				fontName = "Adobe Song Std L";
			}
			if (fontName.Contains("Free") && fontName.Contains("9"))
			{
				fontName = "Free 3 of 9";
			}
			if (fontName.Contains("Regular"))
			{
				fontName = fontName.Replace("Regular", "");
			}
			else if (fontName.Contains("Bold"))
			{
				fontName = fontName.Replace("Bold", "");
			}
			else if (fontName.Contains("Italic"))
			{
				fontName = fontName.Replace("Italic", "");
			}
			fontName = fontName.Trim();
		}
		return fontName;
	}
}
