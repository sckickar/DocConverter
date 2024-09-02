using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.PdfViewer.Base;

internal class TextElement
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

	internal Color m_textcolor;

	internal string m_fontEncoding;

	internal string m_text;

	internal RenderedString renderedString;

	internal int m_finalAngle;

	private bool m_spaceCheck;

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

	internal FontStructure structure;

	internal bool IsContainFontfile2;

	public float currentGlyphWidth;

	internal bool Isembeddedfont;

	internal int FontFlag;

	internal float LineWidth;

	internal Dictionary<SystemFontFontDescriptor, SystemFontOpenTypeFontSource> testdict;

	internal Image type3GlyphImage;

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

	private bool m_isUpdated;

	private bool m_isFound;

	internal bool isExtractText;

	internal bool isRedaction;

	internal int m_mcid = -1;

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

	private Dictionary<int, string> m_macEncodeTable;

	internal int RotateAngle
	{
		get
		{
			return m_finalAngle;
		}
		set
		{
			m_finalAngle = value;
		}
	}

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

	internal Color TextColor
	{
		get
		{
			return m_textcolor;
		}
		set
		{
			m_textcolor = value;
		}
	}

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

	internal TextElement(string text)
	{
		m_text = text;
	}

	internal TextElement(string text, Matrix transformMatrix)
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

	internal TextElement(Image img, Matrix transformMatrix)
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

	private double GetSystemFontGlyphWidth(Glyph glyph)
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

	private Matrix GetTransformationMatrix(Matrix transform)
	{
		return new Matrix((float)transform.M11, (float)transform.M12, (float)transform.M21, (float)transform.M22, (float)transform.OffsetX, (float)transform.OffsetY);
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

	internal float Render(GraphicsObject g, PointF currentLocation, double textScaling, Dictionary<int, int> gWidths, double type1Height, Dictionary<int, string> differenceTable, Dictionary<string, string> differenceMappedTable, Dictionary<int, string> differenceEncoding, out Matrix txtMatrix)
	{
		txtMatrix = default(Matrix);
		string fontName = FontName;
		renderedText = string.Empty;
		string empty = string.Empty;
		bool flag = false;
		_ = FontName;
		int num = 0;
		string[] array = null;
		textFont = null;
		PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
		float x = currentLocation.X;
		if (Font != null)
		{
			FontName = CheckFontName(Font.Name);
			textFont = Font;
		}
		else
		{
			CheckFontStyle(FontName);
			FontName = CheckFontName(FontName);
			if (FontSize < 0f)
			{
				textFont = new Font(FontName, 0f - FontSize);
			}
			else
			{
				textFont = new Font(FontName, FontSize);
			}
		}
		string[] array2 = Text.Split(' ');
		PointF pointF = currentLocation;
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
			Matrix matrix = g.TransformMatrix;
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
			int num3 = 0;
			string text2 = Text;
			for (int num4 = 0; num4 < text2.Length; num4++)
			{
				char c = text2[num4];
				num3++;
				g.TransformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
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
						int num6 = c;
						if (differencesDictionary.ContainsKey(num6.ToString()) && structure.BaseFontEncoding != "WinAnsiEncoding")
						{
							Dictionary<string, string> differencesDictionary2 = structure.DifferencesDictionary;
							num6 = c;
							glyph.Name = FontStructure.GetCharCode(differencesDictionary2[num6.ToString()]);
							goto IL_0662;
						}
					}
					if (standardFontEncodingNames.Length > c && structure.FontName != "Symbol")
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
				goto IL_0662;
				IL_0662:
				glyph.Width = GetGlyphWidth(glyph);
				if (renderedString != null && renderedString.IndexAndWidths != null && renderedString.IndexAndWidths.ContainsKey(num3 - 1))
				{
					glyph.Width = renderedString.IndexAndWidths[num3 - 1];
					glyph.Width *= CharSizeMultiplier;
				}
				FontSource.GetGlyphOutlines(glyph, 100.0);
				Matrix identity = Matrix.Identity;
				identity.Scale(0.01, 0.01, 0.0, 0.0);
				identity.Translate(0.0, 1.0);
				transformations.PushTransform(identity * glyph.TransformMatrix);
				Matrix matrix2 = g.TransformMatrix.Clone();
				matrix2 *= GetTransformationMatrix(transformations.CurrentTransform);
				g.TransformMatrix = matrix2;
				float num7 = 0f;
				num7 = ((glyph.TransformMatrix.M11 > 0.0) ? ((float)glyph.TransformMatrix.M11) : ((glyph.TransformMatrix.M12 == 0.0 || glyph.TransformMatrix.M21 == 0.0) ? ((float)glyph.FontSize) : ((!(glyph.TransformMatrix.M12 < 0.0)) ? ((float)glyph.TransformMatrix.M12) : ((float)(0.0 - glyph.TransformMatrix.M12)))));
				glyph.ToUnicode = c.ToString();
				if (pageRotation == 90f || pageRotation == 270f)
				{
					if (matrix2.M12 == 0.0 && matrix2.M21 == 0.0)
					{
						glyph.IsRotated = false;
						glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num7 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
					}
					else
					{
						glyph.IsRotated = true;
						if (IsFindText && pageRotation == 90f)
						{
							glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num7 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
						}
						else
						{
							glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)(matrix2.OffsetX + (double)(num7 + (float)(glyph.Ascent / 1000.0)) * matrix2.M21), PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConvertor.ConvertFromPixels((float)(matrix2.OffsetY - (double)num7 * matrix2.M21), PdfGraphicsUnit.Point) / zoomFactor), new Size(num7, glyph.Width * (double)num7));
						}
					}
				}
				else
				{
					glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY - num7 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
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
				UpdateTextMatrix(glyph);
				transformations.PopTransform();
				if (structure.FontDictionary != null && structure.FontDictionary.ContainsKey("FontDescriptor") && PdfCrossTable.Dereference(structure.FontDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("CharSet") && pdfDictionary.ContainsKey("FontStretch"))
				{
					PdfString pdfString = PdfCrossTable.Dereference(pdfDictionary["CharSet"]) as PdfString;
					PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["FontStretch"]) as PdfName;
					if (pdfString != null && pdfString.Value != string.Empty && pdfName != null && pdfName.Value == "Condensed")
					{
						empty = pdfString.Value.Replace("/", "");
						int num8 = empty.IndexOf("Y");
						empty = empty.Substring(0, num8 + 1);
						for (int k = 0; k < empty.Length; k++)
						{
							if (!char.IsUpper(empty[k]))
							{
								flag = false;
								break;
							}
							flag = true;
						}
					}
				}
				if (FontEncoding == "WinAnsiEncoding" && flag)
				{
					renderedText += glyph.ToUnicode.ToUpper();
					for (int l = 0; l < textElementGlyphList.Count; l++)
					{
						textElementGlyphList[l].ToUnicode = textElementGlyphList[l].ToUnicode.ToUpper();
					}
				}
				else
				{
					renderedText += glyph.ToUnicode;
				}
			}
			g.TransformMatrix = matrix;
			txtMatrix = textLineMatrix;
		}
		else
		{
			bool flag2 = false;
			int num9 = 0;
			for (int m = 0; m < Text.Length; m++)
			{
				char c3 = Text[m];
				flag2 = false;
				new Glyph();
				num9++;
				if (IsType1Font && !structure.IsOpenTypeFont)
				{
					string text3 = c3.ToString();
					int num10 = c3;
					if (structure.DifferencesDictionary.ContainsValue(Text))
					{
						text3 = Text;
						m = Text.Length - 1;
					}
					if (EncodedTextBytes != null && ReverseMapTable.Count == 0 && OctDecMapTable.Count == 0 && differenceMappedTable.ContainsKey(EncodedTextBytes[m].ToString()))
					{
						text3 = differenceMappedTable[EncodedTextBytes[m].ToString()];
					}
					if (differenceTable.ContainsValue(text3) && differenceMappedTable.ContainsValue(text3))
					{
						foreach (KeyValuePair<int, string> item2 in differenceTable)
						{
							if (item2.Value == text3)
							{
								num10 = item2.Key;
								break;
							}
						}
					}
					else if (ReverseMapTable.ContainsKey(text3) && (ReverseMapTable.Count == differenceTable.Count || ReverseMapTable.Count == CharacterMapTable.Count))
					{
						num10 = (int)ReverseMapTable[text3];
						if (differenceTable.ContainsKey(num10))
						{
							text3 = differenceTable[num10];
						}
					}
					else if (CharacterMapTable.ContainsValue(text3) && CharacterMapTable.Count == differenceTable.Count)
					{
						foreach (KeyValuePair<double, string> item3 in CharacterMapTable)
						{
							if (item3.Value == text3)
							{
								num10 = (int)item3.Key;
								if (differenceTable.ContainsKey(num10))
								{
									text3 = differenceTable[num10];
								}
								break;
							}
						}
					}
					else if (differenceMappedTable.ContainsValue(text3))
					{
						foreach (KeyValuePair<string, string> item4 in differenceMappedTable)
						{
							if (item4.Value == text3)
							{
								num10 = int.Parse(item4.Key);
								if (differenceTable.ContainsKey(num10))
								{
									text3 = differenceTable[num10];
								}
								break;
							}
						}
					}
					else if (differenceMappedTable.ContainsValue(text3))
					{
						foreach (KeyValuePair<string, string> item5 in differenceMappedTable)
						{
							if (item5.Value == text3)
							{
								num10 = int.Parse(item5.Key);
								if (differenceTable.ContainsKey(num10))
								{
									text3 = differenceTable[num10];
								}
								break;
							}
						}
					}
					else if (differenceTable.ContainsKey(num10))
					{
						text3 = differenceTable[num10];
					}
					else if (m_cffGlyphs.DifferenceEncoding.ContainsKey(num10) && structure.FontEncoding != "MacRomanEncoding")
					{
						text3 = m_cffGlyphs.DifferenceEncoding[num10];
					}
					else if (structure.FontEncoding == "MacRomanEncoding")
					{
						if (structure.m_macRomanMapTable.ContainsKey(num10))
						{
							text3 = structure.m_macRomanMapTable[num10];
						}
					}
					else if (structure.FontEncoding == "WinAnsiEncoding" && structure.m_winansiMapTable.ContainsKey(num10))
					{
						text3 = structure.m_winansiMapTable[num10];
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
						if (FontEncoding == "MacRomanEncoding" && c3 > '~')
						{
							GetMacEncodeTable();
							if (OctDecMapTable != null && OctDecMapTable.ContainsKey(c3))
							{
								num10 = OctDecMapTable[c3];
								c3 = (char)num10;
							}
							text3 = FontStructure.GetCharCode(m_macEncodeTable[c3]);
						}
						if (!IsCID)
						{
							if (ReverseMapTable.ContainsKey(text3) && (ReverseMapTable.Count == differenceTable.Count || ReverseMapTable.Count == CharacterMapTable.Count) && m_cffGlyphs.DifferenceEncoding.ContainsKey(num10) && structure.FontEncoding != "MacRomanEncoding")
							{
								text3 = m_cffGlyphs.DifferenceEncoding[num10];
							}
							if (glyphWriter.glyphs.ContainsKey(text3))
							{
								_ = g.TransformMatrix;
								new PdfUnitConvertor();
							}
							else
							{
								string charCode = FontStructure.GetCharCode(text3);
								if (glyphWriter.glyphs.ContainsKey(charCode))
								{
									_ = g.TransformMatrix;
									new PdfUnitConvertor();
								}
								else
								{
									if (m_cffGlyphs.DiffTable != null)
									{
										text3 = m_cffGlyphs.DiffTable[num10];
									}
									charCode = FontStructure.GetCharCode(text3);
									if (glyphWriter.glyphs.ContainsKey(charCode))
									{
										_ = g.TransformMatrix;
										new PdfUnitConvertor();
									}
								}
							}
						}
						else
						{
							string charCode2 = FontStructure.GetCharCode(text3);
							if (ReverseMapTable.ContainsKey(text3) && FontEncoding != "Identity-H")
							{
								charCode2 = ReverseMapTable[text3].ToString();
								if (glyphWriter.glyphs.ContainsKey(charCode2))
								{
									_ = g.TransformMatrix;
									new PdfUnitConvertor();
								}
							}
							else
							{
								string key = num10.ToString();
								if (glyphWriter.glyphs.ContainsKey(charCode2))
								{
									_ = g.TransformMatrix;
									new PdfUnitConvertor();
								}
								else if (glyphWriter.glyphs.ContainsKey(key))
								{
									_ = g.TransformMatrix;
									new PdfUnitConvertor();
								}
							}
						}
						if (gWidths != null)
						{
							if (!differenceTable.ContainsValue(text3) && num10 == 0)
							{
								foreach (KeyValuePair<double, string> item6 in CharacterMapTable)
								{
									if (item6.Value.Equals(text3))
									{
										num10 = (int)item6.Key;
									}
								}
							}
							if (gWidths.ContainsKey(num10))
							{
								currentGlyphWidth = gWidths[num10];
								currentGlyphWidth *= CharSizeMultiplier;
							}
							else if (OctDecMapTable != null && OctDecMapTable.Count != 0)
							{
								num10 = OctDecMapTable[num10];
								if (gWidths.ContainsKey(num10))
								{
									currentGlyphWidth = gWidths[num10];
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
									if (item7.Value.Equals(text3))
									{
										num10 = (int)item7.Key;
									}
								}
								if (gWidths.ContainsKey(num10))
								{
									currentGlyphWidth = gWidths[num10];
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
									GraphicsState graphicState = g.Save();
									flag2 = true;
									DrawSystemFontGlyphShape(c3, g, out txtMatrix);
									g.Restore(graphicState);
								}
								else
								{
									flag2 = true;
									DrawSystemFontGlyphShape(c3, g, out txtMatrix);
								}
							}
							else
							{
								if (isNegativeFont)
								{
									GraphicsState graphicState = g.Save();
									flag2 = true;
									DrawSystemFontGlyphShape(c3, g, out txtMatrix);
									g.Restore(graphicState);
								}
								else if (c3 > '\u007f' && c3 <= 'ÿ' && m_fontEncoding == "WinAnsiEncoding")
								{
									flag2 = true;
									DrawSystemFontGlyphShape(c3, g, out txtMatrix);
								}
								else
								{
									flag2 = true;
									DrawSystemFontGlyphShape(c3, g, out txtMatrix);
								}
								renderedText += c3;
							}
							goto end_IL_15ca;
						}
						catch (Exception ex)
						{
							exceptions.Exceptions.Append("\r\nCharacter not rendered " + c3 + "\r\n" + ex.StackTrace);
						}
						continue;
						end_IL_15ca:;
					}
					if (!flag2)
					{
						DrawGlyphs(currentGlyphWidth, g, out txtMatrix, c3.ToString());
					}
					continue;
				}
				try
				{
					if ((byte)c3 > 126 && m_fontEncoding == "MacRomanEncoding" && !Isembeddedfont)
					{
						_ = MacRomanToUnicode[(byte)c3 - 128];
						if (isNegativeFont)
						{
							GraphicsState graphicState2 = g.Save();
							flag2 = true;
							DrawSystemFontGlyphShape(c3, g, out txtMatrix);
							g.Restore(graphicState2);
						}
						else
						{
							flag2 = true;
							DrawSystemFontGlyphShape(c3, g, out txtMatrix);
						}
					}
					else if (isNegativeFont)
					{
						GraphicsState graphicState2 = g.Save();
						flag2 = true;
						DrawSystemFontGlyphShape(c3, g, out txtMatrix);
						g.Restore(graphicState2);
					}
					else if (c3 > '\u007f' && c3 <= 'ÿ' && m_fontEncoding == "WinAnsiEncoding" && !Isembeddedfont)
					{
						flag2 = true;
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
					else if (structure.GlyphFontFile2 != null)
					{
						flag2 = true;
						txtMatrix = textLineMatrix;
					}
					else if (structure.GlyphFontFile2 == null && !structure.IsEmbedded)
					{
						flag2 = true;
						if (!DrawSystemFontGlyphShape(c3, g, out txtMatrix))
						{
							if (renderedString != null && renderedString.IndexAndWidths != null)
							{
								if (renderedString.IndexAndWidths.ContainsKey(num9 - 1))
								{
									currentGlyphWidth = renderedString.IndexAndWidths[num9 - 1];
									currentGlyphWidth *= CharSizeMultiplier;
								}
								else if (renderedString.IndexAndWidths.Count >= num9 - 1)
								{
									currentGlyphWidth = 0f;
								}
							}
							DrawGlyphs(currentGlyphWidth, g, out txtMatrix, c3.ToString());
						}
						txtMatrix = textLineMatrix;
					}
					if (renderedString != null && renderedString.IndexAndWidths != null)
					{
						if (renderedString.IndexAndWidths.ContainsKey(num9 - 1))
						{
							currentGlyphWidth = renderedString.IndexAndWidths[num9 - 1];
							currentGlyphWidth *= CharSizeMultiplier;
						}
						else if (renderedString.IndexAndWidths.Count >= num9 - 1)
						{
							currentGlyphWidth = 0f;
						}
					}
					else if (CharacterMapTable.ContainsKey((int)c3) && CharacterMapTable.Count > 0)
					{
						char c4 = ' ';
						string text4 = CharacterMapTable[(int)c3];
						_ = new char[text4.Length];
						c4 = text4.ToCharArray()[0];
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
									if (!FontGlyphWidths.ContainsKey((int)ReverseMapTable[c4.ToString()]))
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
						if (FontGlyphWidths != null && FontGlyphWidths.ContainsKey(key2))
						{
							currentGlyphWidth = FontGlyphWidths[key2];
							currentGlyphWidth *= CharSizeMultiplier;
						}
					}
					else if (FontGlyphWidths != null && FontGlyphWidths.Count > 0)
					{
						if (FontGlyphWidths.ContainsKey(c3))
						{
							currentGlyphWidth = FontGlyphWidths[c3];
							currentGlyphWidth *= CharSizeMultiplier;
						}
						else if (structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
						{
							string text5 = structure.MapCharactersFromTable(c3.ToString());
							if (text5.Length == 0)
							{
								flag2 = true;
								txtMatrix = textLineMatrix;
							}
							else if (structure.ReverseMapTable != null && structure.ReverseMapTable.ContainsKey(text5))
							{
								c3 = (char)structure.ReverseMapTable[text5];
								if (FontGlyphWidths.ContainsKey(c3))
								{
									currentGlyphWidth = FontGlyphWidths[c3];
									currentGlyphWidth *= CharSizeMultiplier;
								}
							}
						}
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
					pointF.X += CharacterSpacing;
				}
				if (!flag2)
				{
					DrawGlyphs(currentGlyphWidth, g, out txtMatrix, c3.ToString());
				}
			}
			txtMatrix = textLineMatrix;
		}
		return pointF.X - x;
	}

	private bool DrawSystemFontGlyphShape(char letter, GraphicsObject g, out Matrix temptextmatrix)
	{
		Matrix matrix = g.TransformMatrix;
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
		g.TransformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
		systemFontGlyph.Name = letter.ToString();
		systemFontGlyph.FontSize = m_fontSize;
		if (OctDecMapTable != null && OctDecMapTable.ContainsKey(letter))
		{
			letter = (char)OctDecMapTable[letter];
		}
		byte b = (byte)letter;
		string text = letter.ToString();
		systemFontGlyph.CharId = new CharCode(b);
		systemFontGlyph.CharSpacing = m_characterSpacing;
		systemFontGlyph.FontStyle = m_fontStyle;
		systemFontGlyph.HorizontalScaling = TextHorizontalScaling;
		ushort num = 0;
		string text2 = letter.ToString();
		num = GetGlyphID(letter.ToString());
		if (!structure.IsMappingDone)
		{
			if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.ContainsKey(Convert.ToChar(text2)) && structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
			{
				text2 = CharacterMapTable[CidToGidReverseMapTable[Convert.ToChar(text2)]];
			}
			else if (structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
			{
				structure.m_mapstringFromRender = true;
				text2 = structure.MapCharactersFromTable(text2);
				structure.m_mapstringFromRender = false;
			}
			else if (structure.DifferencesDictionary != null && structure.DifferencesDictionary.Count > 0)
			{
				text2 = structure.MapDifferences(text2);
			}
		}
		systemFontGlyph.GlyphId = num;
		Glyph glyph = new Glyph();
		glyph.HorizontalScaling = TextHorizontalScaling;
		glyph.CharSpacing = CharacterSpacing;
		glyph.FontSize = systemFontGlyph.FontSize;
		glyph.Name = systemFontGlyph.Name;
		glyph.CharId = systemFontGlyph.CharId;
		glyph.TransformMatrix = GetTextRenderingMatrix();
		systemFontGlyph.TransformMatrix = new SystemFontMatrix(glyph.TransformMatrix.M11, glyph.TransformMatrix.M12, glyph.TransformMatrix.M21, glyph.TransformMatrix.M22, glyph.TransformMatrix.OffsetX, glyph.TransformMatrix.OffsetY);
		openTypeFontSource.GetGlyphOutlines(systemFontGlyph, 100.0);
		systemFontGlyph.Width = GetSystemFontGlyphWidth(glyph);
		if (systemFontGlyph.Width == -1.0 && double.IsNaN(systemFontGlyph.Width))
		{
			openTypeFontSource.GetAdvancedWidth(systemFontGlyph);
			systemFontGlyph.Width = systemFontGlyph.AdvancedWidth;
		}
		if (systemFontGlyph.Width <= 0.0 && FontGlyphWidths != null && FontGlyphWidths.Count > 0 && FontGlyphWidths.ContainsKey(letter))
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
			Matrix matrix2 = g.TransformMatrix.Clone();
			matrix2 *= GetTransformationMatrix(transformations.CurrentTransform);
			g.TransformMatrix = matrix2;
			if (structure.ReverseDictMapping.ContainsKey(systemFontGlyph.Name))
			{
				byte b2 = (byte)(float)structure.ReverseDictMapping[systemFontGlyph.Name];
				string[] standardFontEncodingNames = GetStandardFontEncodingNames();
				systemFontGlyph.Name = standardFontEncodingNames[b2];
			}
			if (structure != null && structure.fontType != null && structure.fontType.Value == "Type3")
			{
				Matrix matrix3 = default(Matrix);
				RectangleF fontBBox = structure.FontBBox;
				int num3 = 0;
				if (ReverseMapTable != null && ReverseMapTable.ContainsKey(letter.ToString()))
				{
					double num4 = ReverseMapTable[letter.ToString()];
					num3 = FontGlyphWidths[(int)num4];
				}
				else if (FontGlyphWidths != null && FontGlyphWidths.ContainsKey(letter))
				{
					num3 = FontGlyphWidths[letter];
				}
				if (structure.FontMatrix != matrix3)
				{
					Rect rect = new Rect(0.0, 0.0, num3, fontBBox.Height);
					structure.FontMatrix.Transform(rect);
					float num5 = (float)structure.FontMatrix.TransformX(num3);
					glyph.Width = num5;
				}
			}
			float num6 = 0f;
			num6 = ((glyph.TransformMatrix.M11 > 0.0) ? ((float)glyph.TransformMatrix.M11) : ((glyph.TransformMatrix.M12 == 0.0 || glyph.TransformMatrix.M21 == 0.0) ? ((float)glyph.FontSize) : ((!(glyph.TransformMatrix.M12 < 0.0)) ? ((float)glyph.TransformMatrix.M12) : ((float)(0.0 - glyph.TransformMatrix.M12)))));
			glyph.ToUnicode = text2;
			PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
			if (pageRotation == 90f || pageRotation == 270f)
			{
				if (matrix2.M12 == 0.0 && matrix2.M21 == 0.0)
				{
					glyph.IsRotated = false;
					glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num6 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num6, num6));
				}
				else
				{
					glyph.IsRotated = true;
					if (IsFindText && pageRotation == 90f)
					{
						glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num6 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num6, num6));
					}
					else
					{
						glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX + (num6 + (float)(glyph.Ascent / 1000.0)) * (float)matrix2.M21, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY - num6 * (float)matrix2.M21, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num6 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(num6, glyph.Width * (double)num6));
					}
				}
			}
			else
			{
				glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY - num6 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num6, num6));
			}
			if (structure.IsAdobeJapanFont)
			{
				if (structure.AdobeJapanCidMapTable.ContainsKey(Convert.ToChar(text)))
				{
					text = structure.AdobeJapanCidMapTableGlyphParser(text);
				}
				glyph.ToUnicode = text;
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
			renderedText += text2;
			g.TransformMatrix = matrix;
			temptextmatrix = textLineMatrix;
			return true;
		}
		temptextmatrix = textLineMatrix;
		return false;
	}

	internal float RenderWithSpace(GraphicsObject g, PointF currentLocation, List<RenderedString> decodedList, List<float> characterSpacings, double textScaling, Dictionary<int, int> gWidths, double type1Height, Dictionary<int, string> differenceTable, Dictionary<string, string> differenceMappedTable, Dictionary<int, string> differenceEncoding, out Matrix textmatrix)
	{
		bool flag = false;
		textmatrix = default(Matrix);
		string fontName = FontName;
		renderedText = string.Empty;
		_ = FontName;
		textFont = null;
		float num = 0f;
		float x = currentLocation.X;
		int num2 = 0;
		string[] array = null;
		PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
		if (Font != null && Isembeddedfont)
		{
			FontName = CheckFontName(Font.Name);
			textFont = Font;
		}
		else
		{
			CheckFontStyle(FontName);
			FontName = CheckFontName(FontName);
			if (FontSize < 0f)
			{
				textFont = new Font(FontName, 0f - FontSize);
			}
			else
			{
				textFont = new Font(FontName, FontSize);
			}
		}
		if (ZapfPostScript != null)
		{
			char[] separator = new char[1] { ' ' };
			array = ZapfPostScript.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		}
		PointF pointF = currentLocation;
		float fontSize = FontSize;
		_ = currentLocation.Y;
		bool flag2 = false;
		fontName = ResolveFontName(fontName);
		foreach (RenderedString decoded in decodedList)
		{
			flag2 = false;
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
				Matrix matrix = g.TransformMatrix;
				if (double.TryParse(decoded.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
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
					string text = decoded.Text.Remove(decoded.Text.Length - 1, 1);
					flag2 = false;
					int num3 = 0;
					string text2 = text;
					for (int num4 = 0; num4 < text2.Length; num4++)
					{
						char c = text2[num4];
						num3++;
						flag2 = false;
						g.TransformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
						Glyph glyph = new Glyph();
						glyph.FontSize = FontSize;
						glyph.FontFamily = FontName;
						glyph.TextColor = TextColor;
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
									goto IL_05fb;
								}
							}
							if (standardFontEncodingNames.Length > c)
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
						goto IL_05fb;
						IL_05fb:
						glyph.Width = GetGlyphWidth(glyph);
						if (decoded != null && decoded.IndexAndWidths != null && decoded.IndexAndWidths.ContainsKey(num3 - 1))
						{
							glyph.Width = decoded.IndexAndWidths[num3 - 1];
							glyph.Width *= CharSizeMultiplier;
						}
						FontSource.GetGlyphOutlines(glyph, 100.0);
						Matrix identity = Matrix.Identity;
						identity.Scale(0.01, 0.01, 0.0, 0.0);
						identity.Translate(0.0, 1.0);
						transformations.PushTransform(identity * glyph.TransformMatrix);
						Matrix matrix2 = g.TransformMatrix.Clone();
						matrix2 *= GetTransformationMatrix(transformations.CurrentTransform);
						g.TransformMatrix = matrix2;
						glyph.ToUnicode = c.ToString();
						float num7 = 0f;
						num7 = ((glyph.TransformMatrix.M11 > 0.0) ? ((float)glyph.TransformMatrix.M11) : ((glyph.TransformMatrix.M12 == 0.0 || glyph.TransformMatrix.M21 == 0.0) ? ((float)glyph.FontSize) : ((!(glyph.TransformMatrix.M12 < 0.0)) ? ((float)glyph.TransformMatrix.M12) : ((float)(0.0 - glyph.TransformMatrix.M12)))));
						if (pageRotation == 90f || pageRotation == 270f)
						{
							if (matrix2.M12 == 0.0 && matrix2.M21 == 0.0)
							{
								glyph.IsRotated = false;
								glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num7 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
							}
							else
							{
								glyph.IsRotated = true;
								if (IsFindText && pageRotation == 90f)
								{
									glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num7 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
								}
								else
								{
									glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX + (num7 + (float)(glyph.Ascent / 1000.0)) * (float)matrix2.M21, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY - num7 * (float)matrix2.M21, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num7 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size((double)num7 - glyph.Ascent / 1000.0, glyph.Width * (double)num7));
								}
							}
						}
						else
						{
							glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY - num7 * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num7, num7));
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
						UpdateTextMatrix(glyph);
						transformations.PopTransform();
						renderedText += glyph.ToUnicode;
					}
				}
				g.TransformMatrix = matrix;
				textmatrix = textLineMatrix;
				continue;
			}
			if (double.TryParse(decoded.Text, out result))
			{
				UpdateTextMatrix(result);
				float sizeInPoints = textFont.SizeInPoints;
				float num8 = (float)result * (sizeInPoints / 1000f);
				num8 -= CharacterSpacing;
				pointF.X -= num8;
				textmatrix = textLineMatrix;
				continue;
			}
			if (decoded.Text[0] >= '\u0e00' && decoded.Text[0] <= '\u0e7f')
			{
				_ = string.Empty;
				new List<char>();
				decoded.Text.Remove(decoded.Text.Length - 1, 1);
				flag2 = true;
				continue;
			}
			string text3 = decoded.Text.Remove(decoded.Text.Length - 1, 1);
			int num9 = 0;
			for (int j = 0; j < text3.Length; j++)
			{
				char c3 = text3[j];
				num9++;
				if (IsType1Font && !structure.IsOpenTypeFont)
				{
					GlyphWriter glyphWriter = new GlyphWriter(m_cffGlyphs.Glyphs, Is1C);
					string text4 = c3.ToString();
					int num10 = c3;
					text4 = c3.ToString();
					num10 = c3;
					if (structure.DifferencesDictionary.ContainsValue(text3))
					{
						text4 = text3;
						j = text3.Length - 1;
					}
					if (differenceTable.ContainsValue(text3) && differenceMappedTable.ContainsValue(text3))
					{
						text4 = text3;
						foreach (KeyValuePair<int, string> item2 in differenceTable)
						{
							if (item2.Value == text3)
							{
								num10 = item2.Key;
								j = text3.Length - 1;
								break;
							}
						}
					}
					else if (ReverseMapTable.ContainsKey(text4) && (ReverseMapTable.Count == differenceTable.Count || ReverseMapTable.Count == CharacterMapTable.Count))
					{
						num10 = (int)ReverseMapTable[text4];
						if (differenceTable.ContainsKey(num10))
						{
							byte b = Convert.ToByte(num10);
							text4 = differenceTable[num10];
							CharID = new CharCode(b);
						}
					}
					else if (differenceMappedTable.ContainsValue(text4))
					{
						foreach (KeyValuePair<string, string> item3 in differenceMappedTable)
						{
							if (item3.Value == text4)
							{
								num10 = int.Parse(item3.Key);
								if (differenceTable.ContainsKey(num10))
								{
									text4 = differenceTable[num10];
								}
								break;
							}
						}
					}
					else if (differenceMappedTable.ContainsKey(num10.ToString()))
					{
						text4 = differenceMappedTable[num10.ToString()];
					}
					else if (differenceTable.ContainsKey(c3))
					{
						text4 = differenceTable[c3];
						num10 = c3;
					}
					else if (differenceMappedTable.ContainsValue(text3))
					{
						using Dictionary<string, string>.Enumerator enumerator3 = differenceMappedTable.GetEnumerator();
						if (enumerator3.MoveNext())
						{
							KeyValuePair<string, string> current4 = enumerator3.Current;
							if (current4.Value == text3)
							{
								num10 = int.Parse(current4.Key);
							}
							if (differenceTable.ContainsKey(num10))
							{
								text4 = differenceTable[num10];
							}
						}
					}
					else if (CharacterMapTable.ContainsValue(text4) && CharacterMapTable.Count == differenceTable.Count)
					{
						foreach (KeyValuePair<double, string> item4 in CharacterMapTable)
						{
							if (item4.Value == text4)
							{
								num10 = (int)item4.Key;
								if (differenceTable.ContainsKey(num10))
								{
									text4 = differenceTable[num10];
								}
								break;
							}
						}
					}
					else if (differenceTable.ContainsKey(num10))
					{
						text4 = differenceTable[num10];
					}
					else if (m_cffGlyphs.DifferenceEncoding.ContainsKey(num10) && structure.FontEncoding != "MacRomanEncoding")
					{
						text4 = m_cffGlyphs.DifferenceEncoding[num10];
					}
					else if (structure.FontEncoding == "MacRomanEncoding")
					{
						if (structure.m_macRomanMapTable.ContainsKey(num10))
						{
							text4 = structure.m_macRomanMapTable[num10];
						}
					}
					else if (structure.FontEncoding == "WinAnsiEncoding" && structure.m_winansiMapTable.ContainsKey(num10))
					{
						text4 = structure.m_winansiMapTable[num10];
					}
					try
					{
						double num11 = 0.0;
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
						if (FontEncoding == "MacRomanEncoding" && c3 > '~')
						{
							GetMacEncodeTable();
							if (OctDecMapTable != null && OctDecMapTable.ContainsKey(c3))
							{
								num10 = OctDecMapTable[c3];
								c3 = (char)num10;
							}
							text4 = FontStructure.GetCharCode(m_macEncodeTable[c3]);
						}
						if (!IsCID)
						{
							if (glyphWriter.glyphs.ContainsKey(text4))
							{
								_ = g.TransformMatrix;
								if (!m_cffGlyphs.RenderedPath.ContainsKey(text4))
								{
									new Matrix(0.1, 0.0, 0.0, -0.1, 0.0, 0.0);
								}
							}
							else
							{
								string charCode = FontStructure.GetCharCode(text4);
								if (glyphWriter.glyphs.ContainsKey(charCode))
								{
									_ = g.TransformMatrix;
									new PdfUnitConvertor();
								}
							}
						}
						else
						{
							string charCode2 = FontStructure.GetCharCode(text4);
							if (ReverseMapTable.ContainsKey(text4) && FontEncoding != "Identity-H")
							{
								charCode2 = ReverseMapTable[text4].ToString();
								if (glyphWriter.glyphs.ContainsKey(charCode2))
								{
									_ = g.TransformMatrix;
									new PdfUnitConvertor();
								}
							}
							else
							{
								charCode2 = num10.ToString();
								if (glyphWriter.glyphs.ContainsKey(charCode2))
								{
									_ = g.TransformMatrix;
									new PdfUnitConvertor();
								}
							}
						}
						if (gWidths != null)
						{
							if (!differenceTable.ContainsValue(text4) && num10 == 0)
							{
								foreach (KeyValuePair<double, string> item5 in CharacterMapTable)
								{
									if (item5.Value.Equals(text4))
									{
										num10 = (int)item5.Key;
									}
								}
							}
							if (gWidths.ContainsKey(num10))
							{
								currentGlyphWidth = gWidths[num10];
								currentGlyphWidth *= CharSizeMultiplier;
							}
							else if (OctDecMapTable != null && OctDecMapTable.Count != 0)
							{
								num10 = OctDecMapTable[num10];
								if (gWidths.ContainsKey(num10))
								{
									currentGlyphWidth = gWidths[num10];
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
								foreach (KeyValuePair<double, string> item6 in CharacterMapTable)
								{
									if (item6.Value.Equals(text4))
									{
										num10 = (int)item6.Key;
									}
								}
								if (gWidths.ContainsKey(num10))
								{
									currentGlyphWidth = gWidths[num10];
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
						num = (float)(num11 / 100.0 * (double)(float)textScaling);
						if (num9 < text3.Length)
						{
							pointF.X += num + CharacterSpacing;
						}
						else
						{
							pointF.X += num;
						}
						if (num10 == 32 || c3 == ' ')
						{
							pointF.X += WordSpacing;
						}
					}
					catch
					{
						if (c3 == ' ')
						{
							renderedText += " ";
							continue;
						}
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
									GraphicsState graphicState = g.Save();
									flag2 = true;
									DrawSystemFontGlyphShape(c3, g, out textmatrix);
									g.Restore(graphicState);
								}
								else
								{
									flag2 = true;
									DrawSystemFontGlyphShape(letter, g, out textmatrix);
								}
							}
							else
							{
								if (isNegativeFont)
								{
									GraphicsState graphicState = g.Save();
									flag2 = true;
									DrawSystemFontGlyphShape(c3, g, out textmatrix);
									g.Restore(graphicState);
								}
								else if (RenderingMode != 1)
								{
									flag2 = true;
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
							pointF.X += num + CharacterSpacing;
						}
						else
						{
							pointF.X += num;
						}
					}
					if (!flag2)
					{
						DrawGlyphs(currentGlyphWidth, g, out textmatrix, c3.ToString());
					}
				}
				else
				{
					IsTextGlyphAdded = false;
					try
					{
						if ((byte)c3 > 126 && m_fontEncoding == "MacRomanEncoding" && !Isembeddedfont)
						{
							_ = MacRomanToUnicode[(byte)c3 - 128];
							if (isNegativeFont)
							{
								GraphicsState graphicState = g.Save();
								flag2 = true;
								DrawSystemFontGlyphShape(c3, g, out textmatrix);
								g.Restore(graphicState);
							}
							else
							{
								flag2 = true;
								DrawSystemFontGlyphShape(c3, g, out textmatrix);
							}
						}
						else
						{
							if (isNegativeFont && !Isembeddedfont)
							{
								GraphicsState graphicState = g.Save();
								flag2 = true;
								DrawSystemFontGlyphShape(c3, g, out textmatrix);
								g.Restore(graphicState);
							}
							else if (RenderingMode == 1)
							{
								flag2 = true;
								DrawSystemFontGlyphShape(c3, g, out textmatrix);
							}
							else if ((FontEncoding != "Identity-H" && structure.fontType.Value == "TrueType") || structure.GlyphFontFile2 != null)
							{
								if (OctDecMapTable != null && OctDecMapTable.ContainsKey(c3))
								{
									c3 = (char)OctDecMapTable[c3];
								}
							}
							else if (structure.GlyphFontFile2 == null && !structure.IsEmbedded && !structure.IsMappingDone)
							{
								if (CharacterMapTable.Count > 0 && CharacterMapTable.ContainsKey((int)c3))
								{
									char letter2 = CharacterMapTable[(int)c3].ToCharArray()[0];
									flag2 = true;
									IsTextGlyphAdded = true;
									DrawSystemFontGlyphShape(letter2, g, out textmatrix);
								}
								else
								{
									flag2 = true;
								}
							}
							if (structure.FontDictionary.ContainsKey("DescendantFonts") && PdfCrossTable.Dereference(structure.FontDictionary["DescendantFonts"]) is PdfArray { Count: >0 } pdfArray && pdfArray[0] as PdfReferenceHolder != null)
							{
								PdfDictionary pdfDictionary = PdfCrossTable.Dereference(pdfArray[0]) as PdfDictionary;
								if ((pdfDictionary["FontDescriptor"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2 && pdfDictionary.ContainsKey("Subtype") && !pdfDictionary2.ContainsKey("FontFile2") && !pdfDictionary2.ContainsKey("CIDSet"))
								{
									flag = true;
								}
							}
							if (structure.FontEncoding == "Identity-H" && structure.fontType != null && structure.fontType.Value == "Type0" && flag && ReverseMapTable.ContainsKey(c3.ToString()))
							{
								double num12 = ReverseMapTable[c3.ToString()];
								if (FontGlyphWidths != null && FontGlyphWidths.ContainsKey((int)num12))
								{
									currentGlyphWidth = FontGlyphWidths[(int)num12];
									currentGlyphWidth *= CharSizeMultiplier;
									m_isFound = true;
								}
								else
								{
									currentGlyphWidth = DefaultGlyphWidth;
									currentGlyphWidth *= CharSizeMultiplier;
									m_isFound = true;
								}
							}
							if (decoded.IndexAndWidths != null && !m_isFound)
							{
								if (decoded.IndexAndWidths.ContainsKey(num9 - 1))
								{
									currentGlyphWidth = decoded.IndexAndWidths[num9 - 1];
									currentGlyphWidth *= CharSizeMultiplier;
								}
								else if (decoded.IndexAndWidths.Count >= num9 - 1)
								{
									currentGlyphWidth = 0f;
								}
							}
							else if (CharacterMapTable.ContainsKey((int)c3) && CharacterMapTable.Count > 0 && !m_isFound)
							{
								char c4 = ' ';
								string text5 = CharacterMapTable[(int)c3];
								_ = new char[text5.Length];
								c4 = text5.ToCharArray()[0];
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
										else if (ReverseMapTable.ContainsKey(c4.ToString()) && !FontGlyphWidths.ContainsKey((int)ReverseMapTable[c4.ToString()]))
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
							else if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.Count > 1 && !m_isFound)
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
							else
							{
								if (FontGlyphWidths != null && !m_isFound)
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
								if (structure.GetType1Font() && !structure.IsOpenTypeFont)
								{
									string text6 = c3.ToString();
									int num13 = -1;
									if (differenceMappedTable.ContainsValue(text6))
									{
										foreach (KeyValuePair<string, string> item7 in differenceMappedTable)
										{
											if (item7.Value == text6)
											{
												num13 = int.Parse(item7.Key);
												if (gWidths.ContainsKey(num13))
												{
													currentGlyphWidth = gWidths[num13];
													currentGlyphWidth *= CharSizeMultiplier;
													break;
												}
											}
										}
									}
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
						pointF.X += CharacterSpacing;
					}
					if (!IsTextGlyphAdded)
					{
						DrawGlyphs(currentGlyphWidth, g, out textmatrix, c3.ToString());
					}
				}
				m_isFound = false;
			}
		}
		return pointF.X - x;
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

	private void RenderReverseMapTableByte(char character, GraphicsObject g)
	{
		g.TransformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
		Glyph glyph = new Glyph();
		glyph.FontSize = FontSize;
		glyph.FontFamily = FontName;
		glyph.TextColor = TextColor;
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
			bytes[0] = (byte)num;
			glyph.Name = standardFontEncodingNames[bytes[0]];
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
		GlyphToSLCoordinates(glyph);
		Matrix identity = Matrix.Identity;
		identity.Scale(0.01, 0.01, 0.0, 0.0);
		identity.Translate(0.0, 1.0);
		transformations.PushTransform(identity * glyph.TransformMatrix);
		Matrix matrix = g.TransformMatrix.Clone();
		matrix *= GetTransformationMatrix(transformations.CurrentTransform);
		g.TransformMatrix = matrix;
		UpdateTextMatrix(glyph);
		transformations.PopTransform();
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

	public void DrawGlyphs(float glyphwidth, GraphicsObject g, out Matrix temptextmatrix, string glyphChar)
	{
		Matrix matrix = g.TransformMatrix;
		g.TransformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
		Glyph glyph = new Glyph();
		glyph.FontSize = FontSize;
		glyph.FontFamily = FontName;
		glyph.TextColor = TextColor;
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
		Matrix matrix2 = g.TransformMatrix.Clone();
		matrix2 *= GetTransformationMatrix(transformations.CurrentTransform);
		g.TransformMatrix = matrix2;
		if (!structure.IsMappingDone)
		{
			if (CidToGidReverseMapTable != null && CidToGidReverseMapTable.ContainsKey(Convert.ToChar(glyphChar)) && structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
			{
				glyphChar = CharacterMapTable[CidToGidReverseMapTable[Convert.ToChar(glyphChar)]];
			}
			else if (structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0)
			{
				glyphChar = structure.MapCharactersFromTable(glyphChar.ToString());
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
		PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
		if (pageRotation == 90f || pageRotation == 270f)
		{
			if (matrix2.M12 == 0.0 && matrix2.M21 == 0.0)
			{
				glyph.IsRotated = false;
				glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num, num));
			}
			else
			{
				glyph.IsRotated = true;
				if (IsFindText && pageRotation == 90f)
				{
					glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num, num));
				}
				else
				{
					glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX + (num + (float)(glyph.Ascent / 1000.0)) * (float)matrix2.M21, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY - num * (float)matrix2.M21, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(num, glyph.Width * (double)num));
				}
			}
		}
		else if (matrix2.M12 != 0.0 && matrix2.M21 != 0.0)
		{
			glyph.IsRotated = true;
			if (matrix2.M12 < 0.0 && matrix2.M21 > 0.0)
			{
				glyph.RotationAngle = 270;
			}
			else if (matrix2.M12 > 0.0 && matrix2.M21 < 0.0)
			{
				glyph.RotationAngle = 90;
			}
			else if (matrix2.M12 < 0.0 && matrix2.M21 < 0.0)
			{
				glyph.RotationAngle = 180;
			}
			if (IsFindText && pageRotation == 90f)
			{
				glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num, num));
			}
			else
			{
				glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX + (num + (float)(glyph.Ascent / 1000.0)) * (float)matrix2.M21, PdfGraphicsUnit.Point) / zoomFactor, (pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY - num * (float)matrix2.M21, PdfGraphicsUnit.Point) - pdfUnitConvertor.ConvertFromPixels(num * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point)) / zoomFactor), new Size(glyph.Width * (double)num, num));
			}
		}
		else
		{
			glyph.BoundingRect = new Rect(new Point(pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetX, PdfGraphicsUnit.Point) / zoomFactor, pdfUnitConvertor.ConvertFromPixels((float)matrix2.OffsetY - num * zoomFactor * (float)(dpiY / 96), PdfGraphicsUnit.Point) / zoomFactor), new Size(glyph.Width * (double)num, num));
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
			textElementGlyphList.Add(glyph);
		}
		UpdateTextMatrix(glyph);
		transformations.PopTransform();
		g.TransformMatrix = matrix;
		temptextmatrix = textLineMatrix;
		renderedText += glyphChar;
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

	internal static string CheckFontName(string fontName)
	{
		string text = fontName;
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
		return fontName;
	}
}
