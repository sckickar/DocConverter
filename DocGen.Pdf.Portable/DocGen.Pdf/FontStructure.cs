using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.Pdf.Exporting;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf;

internal class FontStructure
{
	private string m_fontName;

	private FontStyle m_fontStyle;

	private PdfDictionary m_fontDictionary;

	private Dictionary<double, string> m_characterMapTable;

	private Dictionary<string, double> m_reverseMapTable;

	private Dictionary<double, string> m_cidToGidTable;

	private Dictionary<string, string> m_differencesDictionary;

	private const string m_replacementCharacter = "\ufffd";

	private bool hasStemH;

	private float m_fontSize;

	private string m_fontEncoding;

	private bool isGetFontCalled;

	private Dictionary<int, int> m_fontGlyphWidth;

	private bool m_containsCmap = true;

	private bool m_fontFileContainsCmap;

	private Dictionary<double, string> tempMapTable = new Dictionary<double, string>();

	private bool m_isSameFont;

	private Dictionary<int, int> m_octDecMapTable;

	private Dictionary<int, int> m_cidToGidReverseMapTable;

	internal bool IsMappingDone;

	public bool IsSystemFontExist;

	private Dictionary<int, int> m_fontGlyphWidthMapping;

	private float m_type1GlyphHeight;

	internal Dictionary<string, byte[]> m_type1FontGlyphs = new Dictionary<string, byte[]>();

	internal bool IsType1Font;

	internal bool Is1C;

	private bool m_isCID;

	internal Dictionary<int, string> differenceTable = new Dictionary<int, string>();

	internal Dictionary<int, string> differenceEncoding = new Dictionary<int, string>();

	internal static Dictionary<int, string> unicodeCharMapTable;

	private Dictionary<int, string> m_macEncodeTable;

	public byte[] fontfilebytess;

	private FontFile2 m_fontfile2;

	internal bool m_isContainFontfile;

	internal bool IsContainFontfile2;

	internal bool IsContainFontfile3;

	internal bool m_isEmbedded;

	internal Dictionary<string, int> ReverseDictMapping = new Dictionary<string, int>();

	private string m_zapfPostScript;

	private string m_fontRefNumber = string.Empty;

	private string m_baseFontEncoding = string.Empty;

	internal PdfPageResources Type3FontGlyphImages;

	internal Dictionary<string, PdfStream> Type3FontCharProcsDict = new Dictionary<string, PdfStream>();

	private Dictionary<int, string> m_adobeJapanCidMapTable;

	private byte m_Flag;

	internal List<string> tempStringList = new List<string>();

	private bool m_isTextExtraction;

	private string[] m_differenceDictionaryValues;

	private Dictionary<int, char> m_cidByte2Unicode;

	private bool m_skipEscapeSequence;

	private bool m_mapOctalText;

	internal bool m_mapstringFromRender;

	private static readonly object m_locker = new object();

	internal bool isDecodingConformance;

	private int[] m_charCodeTable = new int[256]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
		20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
		30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
		40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
		50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
		60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
		70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
		80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
		90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
		100, 101, 102, 103, 104, 105, 106, 107, 108, 109,
		110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
		120, 121, 122, 123, 124, 125, 126, 127, 8364, 65533,
		8218, 402, 8222, 8230, 8224, 8225, 710, 8240, 352, 8249,
		338, 65533, 381, 65533, 65533, 8216, 8217, 8220, 8221, 8226,
		8211, 8212, 732, 8482, 353, 8250, 339, 65533, 382, 376,
		160, 161, 162, 163, 164, 165, 166, 167, 168, 169,
		170, 171, 172, 173, 174, 175, 176, 177, 178, 179,
		180, 181, 182, 183, 184, 185, 186, 187, 188, 189,
		190, 191, 192, 193, 194, 195, 196, 197, 198, 199,
		200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
		210, 211, 212, 213, 214, 215, 216, 217, 218, 219,
		220, 221, 222, 223, 224, 225, 226, 227, 228, 229,
		230, 231, 232, 233, 234, 235, 236, 237, 238, 239,
		240, 241, 242, 243, 244, 245, 246, 247, 248, 249,
		250, 251, 252, 253, 254, 255
	};

	public MemoryStream FontStream = new MemoryStream();

	internal float DefaultGlyphWidth;

	internal Dictionary<int, string> m_macRomanMapTable = new Dictionary<int, string>();

	internal Dictionary<int, string> m_winansiMapTable = new Dictionary<int, string>();

	private PdfDictionary m_cidSystemInfoDictionary;

	internal bool IsAdobeJapanFont;

	internal bool IsAdobeIdentity;

	private string m_subType;

	public PdfName fontType;

	internal bool IsOpenTypeFont;

	public bool IsEmbedded
	{
		get
		{
			return m_isEmbedded;
		}
		set
		{
			m_isEmbedded = value;
		}
	}

	internal bool IsTextExtraction
	{
		get
		{
			return m_isTextExtraction;
		}
		set
		{
			m_isTextExtraction = value;
		}
	}

	public PdfDictionary FontDictionary
	{
		get
		{
			return m_fontDictionary;
		}
		set
		{
			if (value != null)
			{
				m_fontDictionary = value;
			}
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

	public bool Issymbol => GetFlag(3);

	public bool IsNonSymbol => GetFlag(6);

	public PdfNumber Flags => GetFlagValue();

	internal bool IsCID
	{
		get
		{
			m_isCID = IsCIDFontType();
			return m_isCID;
		}
		set
		{
			m_isCID = value;
		}
	}

	internal FontFile2 GlyphFontFile2
	{
		get
		{
			return m_fontfile2;
		}
		set
		{
			m_fontfile2 = value;
		}
	}

	internal string FontRefNumber
	{
		get
		{
			return m_fontRefNumber;
		}
		set
		{
			m_fontRefNumber = value;
		}
	}

	internal Dictionary<int, string> MacEncodeTable
	{
		get
		{
			if (m_macEncodeTable == null)
			{
				GetMacEncodeTable();
			}
			return m_macEncodeTable;
		}
		set
		{
			m_macEncodeTable = value;
		}
	}

	internal Dictionary<int, string> UnicodeCharMapTable
	{
		get
		{
			if (unicodeCharMapTable == null)
			{
				unicodeCharMapTable = GetUnicodeCharMapTable();
			}
			return unicodeCharMapTable;
		}
		set
		{
			unicodeCharMapTable = value;
		}
	}

	public Dictionary<string, double> ReverseMapTable
	{
		get
		{
			if (m_reverseMapTable == null)
			{
				m_reverseMapTable = GetReverseMapTable();
			}
			return m_reverseMapTable;
		}
		set
		{
			m_reverseMapTable = value;
		}
	}

	public Dictionary<double, string> CharacterMapTable
	{
		get
		{
			if (m_characterMapTable == null)
			{
				m_characterMapTable = GetCharacterMapTable();
			}
			return m_characterMapTable;
		}
		set
		{
			m_characterMapTable = value;
		}
	}

	internal string[] DifferencesDictionaryValues
	{
		get
		{
			if (m_differenceDictionaryValues == null)
			{
				m_differenceDictionaryValues = getMapDifference();
			}
			return m_differenceDictionaryValues;
		}
		set
		{
			m_differenceDictionaryValues = value;
		}
	}

	public Dictionary<string, string> DifferencesDictionary
	{
		get
		{
			if (m_differencesDictionary == null)
			{
				m_differencesDictionary = GetDifferencesDictionary();
			}
			return m_differencesDictionary;
		}
		set
		{
			m_differencesDictionary = value;
		}
	}

	internal Dictionary<int, int> OctDecMapTable
	{
		get
		{
			if (m_octDecMapTable == null)
			{
				m_octDecMapTable = new Dictionary<int, int>();
			}
			return m_octDecMapTable;
		}
		set
		{
			m_octDecMapTable = value;
		}
	}

	internal Dictionary<int, int> CidToGidReverseMapTable
	{
		get
		{
			if (m_cidToGidReverseMapTable == null)
			{
				m_cidToGidReverseMapTable = new Dictionary<int, int>();
			}
			return m_cidToGidReverseMapTable;
		}
		set
		{
			m_cidToGidReverseMapTable = value;
		}
	}

	internal Dictionary<int, string> AdobeJapanCidMapTable => m_adobeJapanCidMapTable;

	internal bool IsHexaDecimalString
	{
		get
		{
			return (m_Flag & 2) >> 1 != 0;
		}
		set
		{
			m_Flag = (byte)((m_Flag & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal Dictionary<int, char> CIDByte2Unicode
	{
		get
		{
			if (m_cidByte2Unicode == null)
			{
				GetCIDByte2Unicode();
			}
			return m_cidByte2Unicode;
		}
		set
		{
			m_cidByte2Unicode = value;
		}
	}

	public DocGen.PdfViewer.Base.Matrix FontMatrix
	{
		get
		{
			DocGen.PdfViewer.Base.Matrix result = default(DocGen.PdfViewer.Base.Matrix);
			if (m_fontDictionary != null && m_fontDictionary.ContainsKey("FontMatrix") && PdfCrossTable.Dereference(m_fontDictionary["FontMatrix"]) is PdfArray { Count: >5 } pdfArray)
			{
				float floatValue = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue2 = (pdfArray[1] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[2] as PdfNumber).FloatValue;
				float floatValue4 = (pdfArray[3] as PdfNumber).FloatValue;
				float floatValue5 = (pdfArray[4] as PdfNumber).FloatValue;
				float floatValue6 = (pdfArray[5] as PdfNumber).FloatValue;
				result = new DocGen.PdfViewer.Base.Matrix(floatValue, floatValue2, floatValue3, floatValue4, floatValue5, floatValue6);
			}
			return result;
		}
	}

	public RectangleF FontBBox
	{
		get
		{
			RectangleF result = default(RectangleF);
			if (m_fontDictionary != null && m_fontDictionary.ContainsKey("FontBBox") && PdfCrossTable.Dereference(m_fontDictionary["FontBBox"]) is PdfArray { Count: >3 } pdfArray)
			{
				return pdfArray.ToRectangle();
			}
			return result;
		}
	}

	public string FontName
	{
		get
		{
			if (m_fontName == null)
			{
				m_fontName = GetFontName();
			}
			return m_fontName;
		}
	}

	public FontStyle FontStyle
	{
		get
		{
			if (m_fontStyle == FontStyle.Regular)
			{
				m_fontStyle = GetFontStyle();
			}
			return m_fontStyle;
		}
	}

	public bool IsSameFont
	{
		get
		{
			return m_isSameFont;
		}
		set
		{
			m_isSameFont = value;
		}
	}

	internal Dictionary<double, string> CidToGidMap => m_cidToGidTable;

	public Dictionary<int, int> FontGlyphWidths
	{
		get
		{
			if (FontEncoding == "Identity-H" || FontEncoding == "Identity#2DH")
			{
				GetGlyphWidths();
			}
			else
			{
				GetGlyphWidthsNonIdH();
			}
			return m_fontGlyphWidth;
		}
		set
		{
			m_fontGlyphWidth = value;
		}
	}

	internal float Type1GlyphHeight => m_type1GlyphHeight;

	public float FontSize
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

	public string FontEncoding
	{
		get
		{
			if (m_fontEncoding == null)
			{
				m_fontEncoding = GetFontEncoding();
			}
			return m_fontEncoding;
		}
	}

	public string BaseFontEncoding => m_baseFontEncoding;

	internal bool ContainsCmap => m_fontFileContainsCmap;

	public FontStructure()
	{
	}

	~FontStructure()
	{
		tempMapTable = new Dictionary<double, string>();
	}

	public FontStructure(IPdfPrimitive fontDictionary)
	{
		m_fontDictionary = fontDictionary as PdfDictionary;
		if (m_fontDictionary != null && m_fontDictionary.ContainsKey(new PdfName("Subtype")))
		{
			fontType = m_fontDictionary.Items[new PdfName("Subtype")] as PdfName;
		}
		if (m_fontDictionary != null && IsCID)
		{
			DefaultGlyphWidth = 1000f;
		}
		IsEmbedded = IsEmbbedFont();
	}

	public FontStructure(IPdfPrimitive fontDictionary, string fontRefNum)
	{
		m_fontDictionary = fontDictionary as PdfDictionary;
		if (m_fontDictionary != null && m_fontDictionary.ContainsKey(new PdfName("Subtype")))
		{
			fontType = m_fontDictionary.Items[new PdfName("Subtype")] as PdfName;
		}
		m_fontRefNumber = fontRefNum;
		if (m_fontDictionary != null && IsCID)
		{
			DefaultGlyphWidth = 1000f;
		}
		IsEmbedded = IsEmbbedFont();
		if (!(fontType != null) || !(fontType.Value == "Type3"))
		{
			return;
		}
		Type3FontGlyphImages = GetImageResources(m_fontDictionary);
		if (!m_fontDictionary.Items.ContainsKey(new PdfName("CharProcs")))
		{
			return;
		}
		PdfDictionary pdfDictionary = m_fontDictionary["CharProcs"] as PdfDictionary;
		if (pdfDictionary == null)
		{
			pdfDictionary = (m_fontDictionary["CharProcs"] as PdfReferenceHolder).Object as PdfDictionary;
		}
		PdfReferenceHolder[] array = new PdfReferenceHolder[pdfDictionary.Count];
		PdfName[] array2 = new PdfName[pdfDictionary.Count];
		Dictionary<PdfName, IPdfPrimitive>.ValueCollection values = pdfDictionary.Items.Values;
		IPdfPrimitive[] array3 = array;
		values.CopyTo(array3, 0);
		pdfDictionary.Items.Keys.CopyTo(array2, 0);
		int num = 0;
		foreach (PdfReferenceHolder value in pdfDictionary.Items.Values)
		{
			Type3FontCharProcsDict.Add(array2[num].Value, value.Object as PdfStream);
			num++;
		}
	}

	internal PdfPageResources GetImageResources(PdfDictionary resourceDictionary)
	{
		PdfPageResources pdfPageResources = new PdfPageResources();
		if (resourceDictionary.ContainsKey("Resources"))
		{
			resourceDictionary = resourceDictionary["Resources"] as PdfDictionary;
		}
		if (resourceDictionary != null && resourceDictionary.ContainsKey("XObject"))
		{
			IPdfPrimitive pdfPrimitive = ((!(resourceDictionary["XObject"] is PdfDictionary)) ? (resourceDictionary["XObject"] as PdfReferenceHolder).Object : resourceDictionary["XObject"]);
			if (pdfPrimitive is PdfDictionary)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in ((PdfDictionary)pdfPrimitive).Items)
				{
					if (item.Value is PdfReferenceHolder)
					{
						PdfDictionary pdfDictionary = (item.Value as PdfReferenceHolder).Object as PdfDictionary;
						if (!pdfDictionary.ContainsKey("Subtype"))
						{
							continue;
						}
						if (!((pdfDictionary["Subtype"] as PdfName).Value == "Image") && (pdfDictionary["Subtype"] as PdfName).Value == "Form")
						{
							if (pdfDictionary.ContainsKey("Resources") && pdfDictionary["Resources"] is PdfDictionary)
							{
								foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in (pdfDictionary["Resources"] as PdfDictionary).Items)
								{
									if (!(item2.Key.Value == "XObject") || !(item2.Value is PdfDictionary))
									{
										continue;
									}
									foreach (KeyValuePair<PdfName, IPdfPrimitive> item3 in (item2.Value as PdfDictionary).Items)
									{
										_ = item3;
										MemoryStream memoryStream = new MemoryStream((pdfDictionary as PdfStream).GetDecompressedData());
										memoryStream.Position = 0L;
										new PdfReader(memoryStream).Position = 0L;
									}
								}
							}
							pdfPageResources.Add(item.Key.Value, new XObjectElement(pdfDictionary, item.Key.Value));
						}
						if (!pdfPageResources.ContainsKey(item.Key.Value))
						{
							pdfPageResources.Add(item.Key.Value, new XObjectElement(pdfDictionary, item.Key.Value));
						}
					}
					else
					{
						PdfDictionary pdfDictionary = item.Value as PdfDictionary;
						pdfPageResources.Add(item.Key.Value, new XObjectElement(pdfDictionary, item.Key.Value));
					}
				}
			}
		}
		return pdfPageResources;
	}

	public RenderedString Decode(string textToDecode, bool isSameFont)
	{
		RenderedString renderedString = new RenderedString();
		string text = textToDecode;
		m_isSameFont = isSameFont;
		bool flag = false;
		switch (text[0])
		{
		case '(':
		{
			if (text.Contains("\\\n"))
			{
				StringBuilder stringBuilder = new StringBuilder(text);
				stringBuilder.Replace("\\\n", "");
				text = stringBuilder.ToString();
			}
			text = text.Substring(1, text.Length - 2);
			renderedString.Text = GetLiteralString(text);
			if (renderedString.Text.Contains("\\\\") && FontEncoding == "Identity-H")
			{
				flag = true;
				renderedString.Text = SkipEscapeSequence(renderedString.Text);
			}
			if (!m_fontDictionary.ContainsKey("Encoding") || !(m_fontDictionary["Encoding"] is PdfName))
			{
				break;
			}
			string value = (m_fontDictionary["Encoding"] as PdfName).Value;
			if (value == "Identity-H")
			{
				string text2 = renderedString.Text;
				if (!flag)
				{
					if (text2.Contains("\\a") || text2.Contains("\\") || text2.Contains("\\b") || text2.Contains("\\f") || text2.Contains("\\r") || text2.Contains("\\t") || text2.Contains("\\n") || text2.Contains("\\v") || text2.Contains("\\'"))
					{
						while (text2.Contains("\\a") || text2.Contains("\\") || text2.Contains("\\b") || text2.Contains("\\f") || text2.Contains("\\r") || text2.Contains("\\t") || text2.Contains("\\n") || text2.Contains("\\v") || text2.Contains("\\'"))
						{
							text2 = SkipEscapeSequence(text2);
						}
					}
					else
					{
						text2 = SkipEscapeSequence(text2);
					}
				}
				List<byte> list = new List<byte>();
				string text3 = text2;
				foreach (char c in text3)
				{
					list.Add((byte)c);
				}
				if (value == "Identity-H")
				{
					renderedString.Text = Encoding.BigEndianUnicode.GetString(list.ToArray(), 0, list.Count);
				}
				if (renderedString.Text.Contains("\\"))
				{
					renderedString.Text = renderedString.Text.Replace("\\", "\\\\");
				}
				break;
			}
			PdfName pdfName = PdfCrossTable.Dereference(m_fontDictionary["Encoding"]) as PdfName;
			PdfName pdfName2 = null;
			if (m_fontDictionary.ContainsKey("Subtype"))
			{
				pdfName2 = PdfCrossTable.Dereference(m_fontDictionary["Subtype"]) as PdfName;
			}
			if (!(pdfName != null) || !(pdfName2 != null) || !(pdfName2.Value == "TrueType") || !(pdfName.Value == "WinAnsiEncoding") || DifferencesDictionary.Count != 0)
			{
				break;
			}
			char[] array = renderedString.Text.ToCharArray();
			string text4 = string.Empty;
			char[] array2 = array;
			foreach (char key in array2)
			{
				if (CIDByte2Unicode.ContainsKey(key))
				{
					text4 += CIDByte2Unicode[key];
				}
			}
			renderedString.Text = text4;
			break;
		}
		case '[':
			if (text.Contains("\\\n"))
			{
				StringBuilder stringBuilder2 = new StringBuilder(text);
				stringBuilder2.Replace("\\\n", "");
				text = stringBuilder2.ToString();
			}
			text = text.Substring(1, text.Length - 2);
			while (text.Length > 0)
			{
				bool flag2 = false;
				int num = text.IndexOf('(');
				int num2 = text.IndexOf(')');
				int num3 = text.IndexOf('<');
				int num4 = text.IndexOf('>');
				if (num3 < num && num3 > -1)
				{
					num = num3;
					num2 = num4;
					flag2 = true;
				}
				if (num < 0)
				{
					num = text.IndexOf('<');
					num2 = text.IndexOf('>');
					if (num < 0)
					{
						break;
					}
					flag2 = true;
				}
				else if (num2 > 0)
				{
					while (text[num2 - 1] == '\\' && text.IndexOf(')', num2 + 1) >= 0)
					{
						num2 = text.IndexOf(')', num2 + 1);
					}
				}
				string text5 = text.Substring(num + 1, num2 - num - 1);
				if (flag2)
				{
					renderedString.Text += GetHexaDecimalString(text5);
				}
				else if (!flag2 && FontEncoding == "Identity-H")
				{
					renderedString.Text += GetRawString(text5);
				}
				else
				{
					renderedString.Text += GetLiteralString(text5);
				}
				text = text.Substring(num2 + 1, text.Length - num2 - 1);
			}
			break;
		case '<':
		{
			string hexEncodedText = text.Substring(1, text.Length - 2);
			renderedString.Text = GetHexaDecimalString(hexEncodedText);
			if (renderedString.Text.Contains("\\"))
			{
				renderedString.Text = renderedString.Text.Replace("\\", "\\\\");
			}
			break;
		}
		}
		if (renderedString.Text.Contains("\0") && !CharacterMapTable.ContainsKey(0.0) && CharacterMapTable.Count > 0)
		{
			renderedString.Text = renderedString.Text.Replace("\0", "");
		}
		if (!IsTextExtraction)
		{
			renderedString.Text = SkipEscapeSequence(renderedString.Text);
		}
		if (((FontEncoding != "Identity-H" && fontType.Value != "TrueType") || (FontEncoding == "Identity-H" && IsType1Font) || (FontEncoding == "Identity-H" && !IsEmbedded)) && fontType.Value != "Type3" && (!(FontName == "MinionPro") || !(FontEncoding == "Encoding")))
		{
			IsMappingDone = true;
			string text6 = renderedString.Text;
			if (DifferencesDictionary.Count == CharacterMapTable.Count && FontEncoding != "" && FontName == "Univers")
			{
				if (DifferencesDictionary != null && DifferencesDictionary.Count > 0)
				{
					renderedString.Text = MapDifferences(renderedString.Text);
				}
				else if (CharacterMapTable != null && CharacterMapTable.Count > 0)
				{
					renderedString.Text = MapCharactersFromTable(renderedString.Text, renderedString);
				}
				else if (FontEncoding != "")
				{
					renderedString.Text = SkipEscapeSequence(renderedString.Text);
				}
			}
			else if (CharacterMapTable != null && CharacterMapTable.Count > 0)
			{
				renderedString.Text = MapCharactersFromTable(renderedString.Text, renderedString);
			}
			else if (DifferencesDictionary != null && DifferencesDictionary.Count > 0)
			{
				renderedString.Text = MapDifferences(renderedString.Text);
			}
			else if (FontEncoding != "")
			{
				renderedString.Text = SkipEscapeSequence(renderedString.Text);
			}
			if (renderedString.Text == text6 && FontName == "AllAndNone")
			{
				IsMappingDone = false;
			}
		}
		if (m_cidToGidTable != null)
		{
			renderedString.Text = MapCidToGid(renderedString.Text);
		}
		if (FontName == "ZapfDingbats" && !IsEmbedded)
		{
			renderedString.Text = MapZapf(renderedString.Text);
		}
		return renderedString;
	}

	internal string ToGetEncodedText(string textElement, bool isSameFont)
	{
		string text = string.Empty;
		string text2 = textElement;
		m_isSameFont = isSameFont;
		bool flag = false;
		switch (text2[0])
		{
		case '(':
		{
			if (text2.Contains("\\\n"))
			{
				StringBuilder stringBuilder = new StringBuilder(text2);
				stringBuilder.Replace("\\\n", "");
				text2 = stringBuilder.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			text = GetLiteralString(text2);
			if (text.Contains("\\\\") && FontEncoding == "Identity-H")
			{
				flag = true;
				text = SkipEscapeSequence(text);
			}
			if (!m_fontDictionary.ContainsKey("Encoding") || !(m_fontDictionary["Encoding"] is PdfName))
			{
				break;
			}
			string value = (m_fontDictionary["Encoding"] as PdfName).Value;
			if (!(value == "Identity-H"))
			{
				break;
			}
			string text3 = text;
			if (!flag)
			{
				if (text3.Contains("\\a") || text3.Contains("\\") || text3.Contains("\\b") || text3.Contains("\\f") || text3.Contains("\\r") || text3.Contains("\\t") || text3.Contains("\\n") || text3.Contains("\\v") || text3.Contains("\\'"))
				{
					while (text3.Contains("\\a") || text3.Contains("\\") || text3.Contains("\\b") || text3.Contains("\\f") || text3.Contains("\\r") || text3.Contains("\\t") || text3.Contains("\\n") || text3.Contains("\\v") || text3.Contains("\\'"))
					{
						text3 = SkipEscapeSequence(text3);
					}
				}
				else
				{
					text3 = SkipEscapeSequence(text3);
				}
			}
			List<byte> list = new List<byte>();
			string text4 = text3;
			foreach (char c in text4)
			{
				list.Add((byte)c);
			}
			if (value == "Identity-H")
			{
				text = Encoding.BigEndianUnicode.GetString(list.ToArray(), 0, list.Count);
			}
			if (text.Contains("\\"))
			{
				text = text.Replace("\\", "\\\\");
			}
			break;
		}
		case '[':
			if (text2.Contains("\\\n"))
			{
				StringBuilder stringBuilder2 = new StringBuilder(text2);
				stringBuilder2.Replace("\\\n", "");
				text2 = stringBuilder2.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			while (text2.Length > 0)
			{
				bool flag2 = false;
				int num = text2.IndexOf('(');
				int num2 = text2.IndexOf(')');
				int num3 = text2.IndexOf('<');
				int num4 = text2.IndexOf('>');
				for (int j = num2 + 1; j < text2.Length && text2[j] != '('; j++)
				{
					if (text2[j] == ')')
					{
						num2 = j;
						break;
					}
				}
				if (num3 < num && num3 > -1)
				{
					num = num3;
					num2 = num4;
					flag2 = true;
				}
				if (num < 0)
				{
					num = text2.IndexOf('<');
					num2 = text2.IndexOf('>');
					if (num < 0)
					{
						break;
					}
					flag2 = true;
				}
				else if (num2 > 0)
				{
					while (text2[num2 - 1] == '\\' && text2.IndexOf(')', num2 + 1) >= 0)
					{
						num2 = text2.IndexOf(')', num2 + 1);
					}
				}
				string text5 = text2.Substring(num + 1, num2 - num - 1);
				text = ((!flag2) ? ((flag2 || !(FontEncoding == "Identity-H")) ? (text + GetLiteralString(text5)) : (text + GetRawString(text5))) : (text + GetHexaDecimalString(text5)));
				text2 = text2.Substring(num2 + 1, text2.Length - num2 - 1);
			}
			break;
		case '<':
		{
			string hexEncodedText = text2.Substring(1, text2.Length - 2);
			text = GetHexaDecimalString(hexEncodedText);
			if (text.Contains("\\"))
			{
				text = text.Replace("\\", "\\\\");
			}
			break;
		}
		}
		if (text.Contains("\0") && !CharacterMapTable.ContainsKey(0.0) && CharacterMapTable.Count > 0)
		{
			text = text.Replace("\0", "");
		}
		if (!IsTextExtraction)
		{
			text = SkipEscapeSequence(text);
		}
		return text;
	}

	public string DecodeType3FontData(string textToDecode)
	{
		string text = textToDecode;
		if (DifferencesDictionary != null && DifferencesDictionary.Count > 0)
		{
			text = MapDifferences(text);
		}
		else if (CharacterMapTable != null && CharacterMapTable.Count > 0)
		{
			text = MapCharactersFromTable(text);
		}
		else if (FontEncoding != "")
		{
			text = SkipEscapeSequence(text);
		}
		return text;
	}

	public List<RenderedString> DecodeTextTJ(string textToDecode, bool isSameFont)
	{
		string text = string.Empty;
		string text2 = textToDecode;
		RenderedString renderedString = new RenderedString();
		m_isSameFont = isSameFont;
		List<RenderedString> list = new List<RenderedString>();
		IsHexaDecimalString = false;
		bool flag = false;
		switch (text2[0])
		{
		case '(':
			if (text2.Contains("\\\n"))
			{
				StringBuilder stringBuilder2 = new StringBuilder(text2);
				stringBuilder2.Replace("\\\n", "");
				text2 = stringBuilder2.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			text = GetLiteralString(text2);
			text = SkipEscapeSequence(text);
			if (m_fontDictionary.ContainsKey("Encoding") && m_fontDictionary["Encoding"] is PdfName && (m_fontDictionary["Encoding"] as PdfName).Value == "Identity-H")
			{
				List<byte> list5 = new List<byte>();
				string text8 = text;
				foreach (char c5 in text8)
				{
					list5.Add((byte)c5);
				}
				text = Encoding.BigEndianUnicode.GetString(list5.ToArray(), 0, list5.Count);
			}
			break;
		case '[':
			if (text2.Contains("\\\n"))
			{
				StringBuilder stringBuilder = new StringBuilder(text2);
				stringBuilder.Replace("\\\n", "");
				text2 = stringBuilder.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			while (text2.Length > 0)
			{
				bool flag2 = false;
				flag = false;
				int num = text2.IndexOf('(');
				int num2 = text2.IndexOf(')');
				for (int i = num2 + 1; i < text2.Length && text2[i] != '('; i++)
				{
					if (text2[i] == ')')
					{
						num2 = i;
						break;
					}
				}
				int num3 = text2.IndexOf('<');
				int num4 = text2.IndexOf('>');
				if (num3 < num && num3 > -1)
				{
					num = num3;
					num2 = num4;
					flag2 = true;
				}
				if (num < 0)
				{
					num = text2.IndexOf('<');
					num2 = text2.IndexOf('>');
					if (num < 0)
					{
						if (text2.Trim().Contains(" "))
						{
							string[] array = text2.Split(' ');
							foreach (string text3 in array)
							{
								if (!string.IsNullOrEmpty(text3))
								{
									renderedString.Text = text3;
									list.Add(renderedString);
									renderedString = new RenderedString();
								}
							}
						}
						else
						{
							renderedString.Text = text2;
							list.Add(renderedString);
							renderedString = new RenderedString();
						}
						break;
					}
					flag2 = true;
				}
				if (num2 < 0 && text2.Length > 0)
				{
					renderedString.Text = text2;
					list.Add(renderedString);
					renderedString = new RenderedString();
					break;
				}
				if (num2 > 0)
				{
					while (text2[num2 - 1] == '\\' && (num2 - 1 <= 0 || text2[num2 - 2] != '\\') && text2.IndexOf(')', num2 + 1) >= 0)
					{
						num2 = text2.IndexOf(')', num2 + 1);
					}
				}
				if (num != 0)
				{
					string text4 = text2.Substring(0, num);
					if (text4.Trim().Contains(" "))
					{
						string[] array = text4.Split(' ');
						foreach (string text5 in array)
						{
							if (!string.IsNullOrEmpty(text5))
							{
								renderedString.Text = text5;
								list.Add(renderedString);
								renderedString = new RenderedString();
							}
						}
					}
					else
					{
						renderedString.Text = text4;
						list.Add(renderedString);
						renderedString = new RenderedString();
					}
				}
				string text6 = text2.Substring(num + 1, num2 - num - 1);
				if (flag2)
				{
					renderedString.Text = GetHexaDecimalString(text6);
					if (renderedString.Text.Contains("\\"))
					{
						renderedString.Text = renderedString.Text.Replace("\\", "\\\\");
					}
					text += renderedString;
				}
				else if (!flag2 && FontEncoding == "Identity-H" && ReverseMapTable.Count != 0 && FontName != "MinionPro")
				{
					renderedString.Text = GetRawString(text6);
					text += renderedString;
				}
				else
				{
					renderedString.Text = GetLiteralString(text6);
					if (renderedString.Text.Contains("\\\\") && FontEncoding == "Encoding")
					{
						flag = true;
					}
					text += renderedString.Text;
				}
				if (renderedString.Text.Contains("\\000"))
				{
					renderedString.Text = renderedString.Text.Replace("\\000", "");
				}
				bool flag3 = false;
				if (renderedString.Text.Contains("\0") && (!CharacterMapTable.ContainsKey(0.0) || CharacterMapTable.ContainsValue("\0")) && (CharacterMapTable.Count > 0 || (IsCID && !FontDictionary.ContainsKey("ToUnicode"))))
				{
					if (FontEncoding == "Identity-H" && IsType1Font && CharacterMapTable.Count == 0)
					{
						int num5 = 0;
						bool flag4 = false;
						string text7 = text;
						text7 = SkipEscapeSequence(text7);
						string text8 = text7;
						foreach (char c in text8)
						{
							if (num5 % 2 == 0 && c != 0 && c != '(' && c != ')')
							{
								flag4 = true;
								break;
							}
							num5 = ((num5 % 2 != 0 || (c != '(' && c != ')')) ? (num5 + 1) : 0);
						}
						if (flag4)
						{
							flag3 = true;
							List<byte> list2 = new List<byte>();
							text8 = renderedString.Text;
							foreach (char c2 in text8)
							{
								list2.Add((byte)c2);
							}
							char[] chars = Encoding.BigEndianUnicode.GetChars(list2.ToArray());
							renderedString.Text = chars.ToString();
							list2.Clear();
							list2 = null;
							flag4 = false;
						}
						else
						{
							renderedString.Text = renderedString.Text.Replace("\0", "");
						}
					}
					else if (IsCID && CharacterMapTable.Count > 0 && !flag3 && !flag2 && FontEncoding == "Identity-H" && ReverseMapTable.Count != 0 && FontName != "MinionPro" && GlyphFontFile2 != null && CidToGidMap != null && CidToGidMap.Count > 0 && CharacterMapTable.Count == CidToGidMap.Count)
					{
						List<byte> list3 = new List<byte>();
						flag3 = true;
						string text8 = renderedString.Text;
						foreach (char c3 in text8)
						{
							list3.Add((byte)c3);
						}
						if (renderedString.Text.Contains("\\"))
						{
							renderedString.Text = renderedString.Text.Replace("\0", "");
						}
						else
						{
							renderedString.Text = Encoding.BigEndianUnicode.GetString(list3.ToArray(), 0, list3.Count);
							list3.Clear();
							list3 = null;
							flag3 = true;
						}
					}
					else
					{
						renderedString.Text = renderedString.Text.Replace("\0", "");
					}
				}
				if (IsCID && CharacterMapTable.Count > 0 && !flag3 && !flag2 && FontEncoding == "Identity-H" && ReverseMapTable.Count != 0 && FontName != "MinionPro" && GlyphFontFile2 != null && CidToGidMap != null && CidToGidMap.Count > 0 && CharacterMapTable.Count == CidToGidMap.Count && isMpdfaaFonts())
				{
					List<byte> list4 = new List<byte>();
					string text8 = renderedString.Text;
					foreach (char c4 in text8)
					{
						list4.Add((byte)c4);
					}
					renderedString.Text = Encoding.BigEndianUnicode.GetString(list4.ToArray(), 0, list4.Count);
					list4.Clear();
					list4 = null;
					flag3 = false;
				}
				if (!IsTextExtraction)
				{
					renderedString.Text = SkipEscapeSequence(renderedString.Text);
				}
				PdfName pdfName = null;
				if (m_fontDictionary.ContainsKey("DescendantFonts") && PdfCrossTable.Dereference(m_fontDictionary["DescendantFonts"]) is PdfArray { Count: >0 } pdfArray && pdfArray[0] as PdfReferenceHolder != null)
				{
					PdfDictionary pdfDictionary = PdfCrossTable.Dereference(pdfArray[0]) as PdfDictionary;
					if (PdfCrossTable.Dereference(pdfDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary2 && pdfDictionary.ContainsKey("Subtype") && !pdfDictionary2.ContainsKey("FontFile2") && !pdfDictionary2.ContainsKey("CIDSet"))
					{
						pdfName = pdfDictionary["Subtype"] as PdfName;
					}
				}
				if ((((FontEncoding != "Identity-H" && fontType != null && fontType.Value != "TrueType") || (FontEncoding == "Identity-H" && IsType1Font) || (FontEncoding == "Identity-H" && !IsEmbedded)) && (FontName != "MinionPro" || (FontName == "MinionPro" && IsCID)) && fontType != null && fontType.Value != "Type3" && !flag) || (FontEncoding == "Identity-H" && m_containsCmap && fontType != null && fontType.Value == "Type0" && pdfName != null && pdfName.Value == "CIDFontType0"))
				{
					IsMappingDone = true;
					string text9 = renderedString.Text;
					if (CharacterMapTable != null && CharacterMapTable.Count > 0)
					{
						renderedString.Text = MapCharactersFromTable(renderedString.Text, renderedString);
					}
					else if (DifferencesDictionary != null && DifferencesDictionary.Count > 0)
					{
						renderedString.Text = MapDifferences(renderedString.Text);
					}
					else if (FontEncoding != "")
					{
						renderedString.Text = SkipEscapeSequence(renderedString.Text);
					}
					byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text9);
					if ((FontEncoding == "Identity-H" && m_containsCmap && fontType != null && fontType.Value == "Type0" && pdfName != null && pdfName.Value == "CIDFontType0") || bytes.Length >= 2)
					{
						if (bytes.Length > 2)
						{
							for (int k = 0; k < bytes.Length; k += 2)
							{
								byte[] array2 = new byte[2];
								if (k + 1 < bytes.Length)
								{
									array2[0] = bytes[k];
									array2[1] = bytes[k + 1];
								}
								else
								{
									array2[0] = bytes[k];
									array2[1] = 0;
								}
								int num6 = ByteArrayToInt(array2, 2);
								if (CharacterMapTable != null && CharacterMapTable.ContainsKey(num6))
								{
									if (k == 0)
									{
										renderedString.Text = string.Empty;
										renderedString.Text += CharacterMapTable[num6];
									}
									else
									{
										renderedString.Text += CharacterMapTable[num6];
									}
								}
							}
						}
						else
						{
							int num7 = ByteArrayToInt(bytes, bytes.Length);
							if (CharacterMapTable != null && CharacterMapTable.ContainsKey(num7))
							{
								renderedString.Text = CharacterMapTable[num7];
							}
						}
					}
				}
				if (m_cidToGidTable != null)
				{
					renderedString.Text = MapCidToGid(renderedString.Text);
				}
				if (renderedString.Text.Length > 0)
				{
					if (renderedString.Text[0] >= '\u0e00' && renderedString.Text[0] <= '\u0e7f' && list.Count > 0)
					{
						RenderedString renderedString2 = list[0];
						renderedString2.Text = renderedString2.Text.Remove(renderedString2.Text.Length - 1);
						renderedString2.Text += renderedString.Text;
						renderedString = renderedString2;
						list[0].Text = renderedString.Text + "s";
					}
					else if ((renderedString.Text[0] == ' ' || renderedString.Text[0] == '/') && renderedString.Text.Length > 1)
					{
						if (renderedString.Text[1] >= '\u0e00' && renderedString.Text[1] <= '\u0e7f' && list.Count > 0)
						{
							RenderedString renderedString3 = list[0];
							renderedString3.Text = renderedString3.Text.Remove(renderedString3.Text.Length - 1);
							renderedString3.Text += renderedString.Text;
							renderedString.Text = renderedString3.Text;
							list[0].Text = renderedString.Text + "s";
						}
						else
						{
							renderedString.Text += "s";
							list.Add(renderedString);
							renderedString = new RenderedString();
						}
					}
					else
					{
						renderedString.Text += "s";
						list.Add(renderedString);
						renderedString = new RenderedString();
					}
				}
				else
				{
					renderedString.Text += "s";
					list.Add(renderedString);
					renderedString = new RenderedString();
				}
				flag = false;
				text2 = text2.Substring(num2 + 1, text2.Length - num2 - 1);
			}
			break;
		case '<':
		{
			string hexEncodedText = text2.Substring(1, text2.Length - 2);
			text = GetHexaDecimalString(hexEncodedText);
			break;
		}
		}
		text = SkipEscapeSequence(text);
		return list;
	}

	private int ByteArrayToInt(byte[] data, int dataLen)
	{
		int num = 0;
		for (int i = 0; i < dataLen; i++)
		{
			num <<= 8;
			num |= data[i] & 0xFF;
		}
		return num;
	}

	internal List<string> DecodeTextExtractionTJ(string textToDecode, bool isSameFont)
	{
		string text = string.Empty;
		string text2 = textToDecode;
		m_isSameFont = isSameFont;
		List<string> list = new List<string>();
		switch (text2[0])
		{
		case '(':
			if (text2.Contains("\\\n"))
			{
				StringBuilder stringBuilder2 = new StringBuilder(text2);
				stringBuilder2.Replace("\\\n", "");
				text2 = stringBuilder2.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			text = GetLiteralString(text2);
			if (m_fontDictionary != null && m_fontDictionary.ContainsKey("Encoding") && m_fontDictionary["Encoding"] is PdfName && (m_fontDictionary["Encoding"] as PdfName).Value == "Identity-H")
			{
				string text6 = SkipEscapeSequence(text);
				List<byte> list2 = new List<byte>();
				string text7 = text6;
				foreach (char c in text7)
				{
					list2.Add((byte)c);
				}
				text = Encoding.BigEndianUnicode.GetString(list2.ToArray(), 0, list2.Count);
			}
			break;
		case '[':
			if (text2.Contains("\\\n"))
			{
				StringBuilder stringBuilder = new StringBuilder(text2);
				stringBuilder.Replace("\\\n", "");
				text2 = stringBuilder.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			while (text2.Length > 0)
			{
				bool flag = false;
				int num = text2.IndexOf('(');
				int num2 = text2.IndexOf(')');
				for (int i = num2 + 1; i < text2.Length && text2[i] != '('; i++)
				{
					if (text2[i] == ')')
					{
						num2 = i;
						break;
					}
				}
				int num3 = text2.IndexOf('<');
				int num4 = text2.IndexOf('>');
				if (num3 < num && num3 > -1)
				{
					num = num3;
					num2 = num4;
					flag = true;
				}
				string item;
				if (num < 0)
				{
					num = text2.IndexOf('<');
					num2 = text2.IndexOf('>');
					if (num < 0)
					{
						item = text2;
						list.Add(item);
						break;
					}
					flag = true;
				}
				if (num2 < 0 && text2.Length > 0)
				{
					item = text2;
					list.Add(item);
					break;
				}
				if (num2 > 0)
				{
					while (text2[num2 - 1] == '\\' && (num2 - 1 <= 0 || text2[num2 - 2] != '\\') && text2.IndexOf(')', num2 + 1) >= 0)
					{
						num2 = text2.IndexOf(')', num2 + 1);
					}
				}
				if (num != 0)
				{
					item = text2.Substring(0, num);
					list.Add(item);
				}
				string text3 = text2.Substring(num + 1, num2 - num - 1);
				if (flag)
				{
					item = GetHexaDecimalString(text3);
					if (item.Contains("\\"))
					{
						item = item.Replace("\\", "\\\\");
					}
					text += item;
				}
				else
				{
					m_mapOctalText = true;
					item = GetLiteralString(text3);
					if (item.Contains("\\") && (FontEncoding == "Identity-H" || (FontEncoding == string.Empty && fontType != null && fontType.Value == "TrueType")))
					{
						item = SkipEscapeSequence(item);
					}
					text += item;
					m_mapOctalText = false;
				}
				if (item.Contains("\0") && !CharacterMapTable.ContainsKey(0.0))
				{
					item = item.Replace("\0", "");
				}
				if (FontEncoding == "Identity-H" && !CharacterMapTable.ContainsKey(92.0))
				{
					do
					{
						item = SkipEscapeSequence(item);
					}
					while (!m_skipEscapeSequence && HasEscapeCharacter(item));
					m_skipEscapeSequence = false;
				}
				if (FontEncoding != "Identity-H" || FontEncoding == "Identity-H" || (FontEncoding == "Identity-H" && m_containsCmap))
				{
					IsMappingDone = true;
					if (CharacterMapTable != null && CharacterMapTable.Count > 0)
					{
						item = MapCharactersFromTable(item);
					}
					else if (DifferencesDictionary != null && DifferencesDictionary.Count > 0)
					{
						item = MapDifferences(item);
					}
					else if (FontEncoding != "")
					{
						item = SkipEscapeSequence(item);
					}
				}
				if (m_cidToGidTable != null && !m_isTextExtraction)
				{
					item = MapCidToGid(item);
				}
				if (item.Length > 0)
				{
					if (item[0] >= '\u0e00' && item[0] <= '\u0e7f' && list.Count > 0)
					{
						string text4 = list[0];
						item = text4.Remove(text4.Length - 1) + item;
						list[0] = item + "s";
					}
					else if ((item[0] == ' ' || item[0] == '/') && item.Length > 1)
					{
						if (item[1] >= '\u0e00' && item[1] <= '\u0e7f' && list.Count > 0)
						{
							string text5 = list[0];
							item = text5.Remove(text5.Length - 1) + item;
							list[0] = item + "s";
						}
						else
						{
							item += "s";
							list.Add(item);
						}
					}
					else
					{
						item += "s";
						list.Add(item);
					}
				}
				else
				{
					item += "s";
					list.Add(item);
				}
				text2 = text2.Substring(num2 + 1, text2.Length - num2 - 1);
			}
			break;
		case '<':
		{
			string hexEncodedText = text2.Substring(1, text2.Length - 2);
			text = GetHexaDecimalString(hexEncodedText);
			break;
		}
		}
		text = SkipEscapeSequence(text);
		return list;
	}

	private bool HasEscapeCharacter(string text)
	{
		if (!text.Contains("\\a") && !text.Contains("\\") && !text.Contains("\\b") && !text.Contains("\\f") && !text.Contains("\\r") && !text.Contains("\\t") && !text.Contains("\\n") && !text.Contains("\\v") && !text.Contains("\\'"))
		{
			return text.Contains("\\0");
		}
		return true;
	}

	public string DecodeTextExtraction(string textToDecode, bool isSameFont)
	{
		string text = string.Empty;
		string text2 = textToDecode;
		m_isSameFont = isSameFont;
		bool flag = false;
		switch (text2[0])
		{
		case '(':
		{
			if (text2.Contains("\\\n") || text2.Contains("\\("))
			{
				StringBuilder stringBuilder2 = new StringBuilder(text2);
				if (text2.Contains("\\\n"))
				{
					stringBuilder2.Replace("\\\n", "");
				}
				else
				{
					stringBuilder2.Replace("\\(", "(");
				}
				text2 = stringBuilder2.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			text = GetLiteralString(text2);
			if ((text.Contains("\\\\") && FontEncoding == "Identity-H") || (text.Contains("\\") && FontEncoding == string.Empty))
			{
				flag = true;
				text = SkipEscapeSequence(text);
			}
			if (m_fontDictionary == null || !m_fontDictionary.ContainsKey("Encoding") || !(m_fontDictionary["Encoding"] is PdfName))
			{
				break;
			}
			string value = (m_fontDictionary["Encoding"] as PdfName).Value;
			if (value == "Identity-H" || value == "UniGB-UCS2-H")
			{
				string text4 = text;
				if (!flag)
				{
					do
					{
						text4 = SkipEscapeSequence(text4);
					}
					while (!m_skipEscapeSequence && HasEscapeCharacter(text4));
					m_skipEscapeSequence = false;
				}
				List<byte> list = new List<byte>();
				string text5 = text4;
				foreach (char c in text5)
				{
					list.Add((byte)c);
				}
				text = Encoding.BigEndianUnicode.GetString(list.ToArray(), 0, list.Count);
				break;
			}
			PdfName pdfName = PdfCrossTable.Dereference(m_fontDictionary["Encoding"]) as PdfName;
			PdfName pdfName2 = null;
			if (m_fontDictionary.ContainsKey("Subtype"))
			{
				pdfName2 = PdfCrossTable.Dereference(m_fontDictionary["Subtype"]) as PdfName;
			}
			if (pdfName != null && pdfName2 != null && pdfName2.Value == "TrueType" && pdfName.Value == "WinAnsiEncoding" && DifferencesDictionary.Count == 0)
			{
				char[] array = text.ToCharArray();
				string text6 = string.Empty;
				char[] array2 = array;
				foreach (char key in array2)
				{
					text6 += CIDByte2Unicode[key];
				}
				text = text6;
			}
			break;
		}
		case '[':
			if (text2.Contains("\\\n"))
			{
				StringBuilder stringBuilder = new StringBuilder(text2);
				stringBuilder.Replace("\\\n", "");
				text2 = stringBuilder.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			while (text2.Length > 0)
			{
				bool flag2 = false;
				int num = text2.IndexOf('(');
				int num2 = text2.IndexOf(')');
				int num3 = text2.IndexOf('<');
				int num4 = text2.IndexOf('>');
				if (num3 < num && num3 > -1)
				{
					num = num3;
					num2 = num4;
					flag2 = true;
				}
				if (num < 0)
				{
					num = text2.IndexOf('<');
					num2 = text2.IndexOf('>');
					if (num < 0)
					{
						break;
					}
					flag2 = true;
				}
				else if (num2 > 0)
				{
					while (text2[num2 - 1] == '\\' && text2.IndexOf(')', num2 + 1) >= 0)
					{
						num2 = text2.IndexOf(')', num2 + 1);
					}
				}
				m_mapOctalText = true;
				string text3 = text2.Substring(num + 1, num2 - num - 1);
				if (flag2)
				{
					text += GetHexaDecimalString(text3);
				}
				else
				{
					text += GetLiteralString(text3);
					if (text.Contains("\\") && (FontEncoding == "Identity-H" || (FontEncoding == string.Empty && fontType != null && fontType.Value == "TrueType")))
					{
						flag = true;
						text = SkipEscapeSequence(text);
					}
				}
				m_mapOctalText = false;
				text2 = text2.Substring(num2 + 1, text2.Length - num2 - 1);
			}
			break;
		case '<':
		{
			string hexEncodedText = text2.Substring(1, text2.Length - 2);
			text = GetHexaDecimalString(hexEncodedText);
			break;
		}
		}
		if (FontEncoding == "Identity-H" && !CharacterMapTable.ContainsKey(92.0))
		{
			do
			{
				text = SkipEscapeSequence(text);
			}
			while (!m_skipEscapeSequence && HasEscapeCharacter(text));
			m_skipEscapeSequence = false;
		}
		PdfName pdfName3 = null;
		if (m_fontDictionary != null && m_fontDictionary.ContainsKey("Subtype"))
		{
			pdfName3 = PdfCrossTable.Dereference(m_fontDictionary["Subtype"]) as PdfName;
		}
		if (pdfName3 != null && pdfName3.Value == "TrueType" && m_fontDictionary != null && m_fontDictionary.ContainsKey("FontDescriptor") && PdfCrossTable.Dereference(m_fontDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("StemH"))
		{
			hasStemH = true;
		}
		if ((FontEncoding != "Identity-H" && (pdfName3.Value != "TrueType" || !hasStemH || FontEncoding != "WinAnsiEncoding")) || FontEncoding == "Identity-H" || (FontEncoding == "Identity-H" && m_containsCmap))
		{
			IsMappingDone = true;
			if (CharacterMapTable != null && CharacterMapTable.Count > 0)
			{
				text = MapCharactersFromTable(text);
			}
			else if (DifferencesDictionary != null && DifferencesDictionary.Count > 0)
			{
				text = MapDifferences(text);
			}
			else if (FontEncoding != "")
			{
				text = SkipEscapeSequence(text);
			}
		}
		if (m_cidToGidTable != null && !IsTextExtraction)
		{
			text = MapCidToGid(text);
		}
		if (FontName == "ZapfDingbats" && !IsEmbedded)
		{
			text = MapZapf(text);
		}
		if (FontEncoding == "MacRomanEncoding")
		{
			string text7 = string.Empty;
			string text5 = text;
			for (int i = 0; i < text5.Length; i++)
			{
				char c2 = text5[i];
				if ((byte)c2 > 126)
				{
					string text8 = MacEncodeTable[(byte)c2];
					text7 += text8;
				}
				else
				{
					text7 += c2;
				}
			}
			if (text7 != string.Empty)
			{
				text = text7;
			}
		}
		text2 = null;
		if (text != null && text.Contains("\u0001"))
		{
			text = text.Replace("\u0001", "");
		}
		if (text != null && text.Contains("\u0092"))
		{
			text = text.Replace("\u0092", "’");
		}
		return text;
	}

	internal string GetLiteralString(string encodedText)
	{
		string text = encodedText;
		int num = -1;
		int num2 = 3;
		bool flag = false;
		bool flag2 = false;
		while ((text.Contains("\\") && !text.Contains("\\\\")) || text.Contains("\0") || (text.Contains("\\\\") && FontEncoding == "Encoding"))
		{
			string text2 = string.Empty;
			if (text.IndexOf('\\', num + 1) >= 0)
			{
				int num3 = text.IndexOf('\0', num + 1);
				num = text.IndexOf('\\', num + 1);
				if (num3 > -1 && num > num3)
				{
					num = num3;
					num2 = 2;
				}
			}
			else
			{
				num = text.IndexOf('\0', num + 1);
				if (num < 0)
				{
					break;
				}
				num2 = 2;
			}
			for (int i = num + 1; i <= num + num2; i++)
			{
				if (i < text.Length)
				{
					int result = 0;
					if (!int.TryParse(text[i].ToString(), out result))
					{
						text2 = string.Empty;
						break;
					}
					if (result <= 8)
					{
						text2 += text[i];
					}
				}
				else
				{
					text2 = string.Empty;
				}
			}
			if (text2 != string.Empty)
			{
				int num4 = (int)Convert.ToUInt64(text2, 8);
				string value = string.Empty;
				char c = (char)num4;
				if (CharacterMapTable != null && CharacterMapTable.Count > 0)
				{
					value = c.ToString();
				}
				else if (DifferencesDictionary != null && DifferencesDictionary.Count > 0 && DifferencesDictionary.ContainsKey(num4.ToString()))
				{
					value = c.ToString();
				}
				else if (FontEncoding != "MacRomanEncoding")
				{
					Encoding encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252);
					if (encoding == null)
					{
						encoding = Encoding.UTF8;
					}
					if (encoding == null)
					{
						encoding = Encoding.UTF8;
					}
					char[] chars = encoding.GetChars(new byte[1] { Convert.ToByte(num4) });
					int key = 0;
					char[] array = chars;
					for (int j = 0; j < array.Length; j++)
					{
						char c2 = array[j];
						key = c2;
						value = c2.ToString();
					}
					if (!OctDecMapTable.ContainsKey(key))
					{
						OctDecMapTable.Add(key, num4);
					}
					flag2 = true;
				}
				else
				{
					Encoding encoding2 = CodePagesEncodingProvider.Instance.GetEncoding(10000);
					if (encoding2 == null)
					{
						encoding2 = Encoding.UTF8;
					}
					if (encoding2 == null)
					{
						encoding2 = Encoding.UTF8;
					}
					char[] chars2 = encoding2.GetChars(new byte[1] { Convert.ToByte(num4) });
					int key2 = 0;
					char[] array = chars2;
					for (int j = 0; j < array.Length; j++)
					{
						char c3 = array[j];
						key2 = c3;
						value = c3.ToString();
					}
					if (!OctDecMapTable.ContainsKey(key2))
					{
						OctDecMapTable.Add(key2, num4);
					}
					flag = true;
				}
				text = text.Remove(num, num2 + 1);
				text = text.Insert(num, value);
			}
			else if (m_mapOctalText && num2 == 2)
			{
				char c4 = (char)(((text[num] & 0xFF) << 8) + (text[num + 1] & 0xFF));
				string empty = string.Empty;
				if (CharacterMapTable != null && CharacterMapTable.Count > 0)
				{
					empty = c4.ToString();
					text = text.Remove(num, num2);
					text = text.Insert(num, empty);
				}
			}
		}
		if (text.Contains("\\") && m_fontEncoding != "Identity-H" && text.Length > 1)
		{
			int num5 = text.IndexOf("\\");
			char c5 = text[num5 + 1];
			if (c5 == '(' || c5 == ')')
			{
				Regex.Unescape(text);
			}
			else if (!text.Contains("\\\\"))
			{
				int num6 = 0;
				while (text.Contains("\\") && text.Length != num6)
				{
					num6 = text.Length;
					text = SkipEscapeSequence(text);
				}
			}
		}
		if (FontEncoding == "MacRomanEncoding" && !flag)
		{
			GetMacEncodeTable();
			string text3 = text;
			for (int j = 0; j < text3.Length; j++)
			{
				char c6 = text3[j];
				int num7 = c6;
				Encoding encoding3 = CodePagesEncodingProvider.Instance.GetEncoding(10000);
				if (encoding3 == null)
				{
					encoding3 = Encoding.UTF8;
				}
				if (m_macEncodeTable.ContainsValue(c6.ToString()) && !m_macRomanMapTable.ContainsKey(num7))
				{
					byte[] array2 = new byte[1] { Convert.ToByte(num7) };
					m_macRomanMapTable.Add(num7, encoding3.GetString(array2, 0, array2.Length));
				}
			}
		}
		if (FontEncoding == "WinAnsiEncoding" && !flag2)
		{
			_ = string.Empty;
			string text3 = encodedText;
			foreach (char c7 in text3)
			{
				int num8 = c7;
				if (num8 == 127 || num8 == 129 || num8 == 131 || num8 == 136 || num8 == 141 || num8 == 143 || num8 == 144 || num8 == 152 || num8 == 157 || num8 == 173 || num8 == 209)
				{
					char c8 = '\u0095';
					if (!m_winansiMapTable.ContainsKey(num8))
					{
						m_winansiMapTable.Add(c7, c8.ToString());
					}
				}
			}
		}
		return text;
	}

	internal string GetHexaDecimalString(string hexEncodedText)
	{
		string text = string.Empty;
		IsHexaDecimalString = true;
		if (!string.IsNullOrEmpty(hexEncodedText) && m_fontDictionary != null)
		{
			PdfName pdfName = m_fontDictionary.Items[new PdfName("Subtype")] as PdfName;
			int num = 2;
			if (pdfName.Value != "Type1" && pdfName.Value != "TrueType" && pdfName.Value != "Type3")
			{
				num = 4;
			}
			hexEncodedText = EscapeSymbols(hexEncodedText);
			string text2 = hexEncodedText;
			string text3 = text;
			string text4 = null;
			while (hexEncodedText.Length > 0)
			{
				if (hexEncodedText.Length % 4 != 0)
				{
					num = 2;
				}
				string text5 = hexEncodedText.Substring(0, num);
				if (m_fontDictionary.ContainsKey("DescendantFonts") && !m_fontDictionary.ContainsKey("ToUnicode"))
				{
					if (m_fontDictionary["DescendantFonts"] is PdfArray pdfArray)
					{
						if (pdfArray[0] as PdfReferenceHolder != null)
						{
							PdfDictionary pdfDictionary = (pdfArray[0] as PdfReferenceHolder).Object as PdfDictionary;
							if ((pdfDictionary["FontDescriptor"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2 && pdfDictionary.ContainsKey("Subtype") && !pdfDictionary2.ContainsKey("FontFile2") && (pdfDictionary["Subtype"] as PdfName).Value == "CIDFontType2")
							{
								text5 = MapHebrewCharacters(text5);
							}
						}
					}
					else if (m_fontDictionary.Items.ContainsKey(new PdfName("DescendantFonts")))
					{
						PdfReferenceHolder pdfReferenceHolder = m_fontDictionary.Items[new PdfName("DescendantFonts")] as PdfReferenceHolder;
						if (pdfReferenceHolder != null)
						{
							PdfName pdfName2 = null;
							PdfArray pdfArray2 = pdfReferenceHolder.Object as PdfArray;
							if (pdfArray2[0] as PdfReferenceHolder != null && (pdfArray2[0] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("CIDSystemInfo") && pdfDictionary3.ContainsKey("Subtype"))
							{
								pdfName2 = pdfDictionary3["Subtype"] as PdfName;
								if ((pdfDictionary3["CIDSystemInfo"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("Registry") && pdfDictionary4.ContainsKey("Ordering") && pdfDictionary4.ContainsKey("Supplement"))
								{
									PdfString pdfString = pdfDictionary4["Registry"] as PdfString;
									PdfNumber pdfNumber = pdfDictionary4["Supplement"] as PdfNumber;
									PdfString pdfString2 = pdfDictionary4["Ordering"] as PdfString;
									if (pdfString.Value != null)
									{
										_ = pdfNumber.IntValue;
										if (pdfString2.Value != null && pdfString.Value == "Adobe" && pdfString2.Value == "Identity" && pdfNumber.IntValue == 0 && pdfName2.Value == "CIDFontType2" && m_cidSystemInfoDictionary == null && !IsContainFontfile2)
										{
											IsAdobeIdentity = true;
											text5 = MapIdentityCharacters(text5);
										}
									}
								}
							}
						}
					}
				}
				text += (char)long.Parse(text5, NumberStyles.HexNumber);
				hexEncodedText = hexEncodedText.Substring(num, hexEncodedText.Length - num);
				text4 = text.ToString();
			}
			if ((text4.Contains("\u0093") || text4.Contains("\u0094") || text4.Contains("\u0092")) && text2.Length < num)
			{
				text = text3;
				byte[] bytes = BitConverter.GetBytes(int.Parse(text2, NumberStyles.HexNumber));
				hexEncodedText = Encoding.GetEncoding("windows-1251").GetString(bytes, 0, bytes.Length);
				hexEncodedText = hexEncodedText.Remove(1);
				text += hexEncodedText;
				bytes = null;
			}
		}
		return text;
	}

	internal string MapIdentityCharacters(string hexChar)
	{
		if (hexChar.Substring(0, 2) == "00")
		{
			if (hexChar.Substring(2, 1) != "0" || hexChar.Substring(2, 1) != "1")
			{
				int num = int.Parse(hexChar, NumberStyles.HexNumber);
				hexChar = (num + 29).ToString("X");
			}
			else
			{
				int num2 = int.Parse(hexChar, NumberStyles.HexNumber);
				hexChar = (num2 + 1335).ToString("X");
			}
		}
		return hexChar;
	}

	internal string GetRawString(string decodedText)
	{
		string text = string.Empty;
		int num = 0;
		bool flag = false;
		if (FontEncoding == "Identity-H" && IsCID)
		{
			string text2 = decodedText;
			for (int i = 0; i < text2.Length; i++)
			{
				char c = text2[i];
				switch (c)
				{
				case '\u0001':
					num = c;
					text += c;
					continue;
				case '\\':
					flag = true;
					text += "\\";
					continue;
				}
				if (flag)
				{
					char c2 = c;
					bool flag2 = false;
					switch (c2)
					{
					case 'n':
						c2 = '\n';
						flag2 = true;
						break;
					case 'b':
						c2 = '\b';
						flag2 = true;
						break;
					case 't':
						c2 = '\t';
						flag2 = true;
						break;
					case 'r':
						c2 = '\r';
						flag2 = true;
						break;
					case 'f':
						c2 = '\f';
						flag2 = true;
						break;
					default:
						num = num * 256 + c;
						text += Convert.ToString(Convert.ToChar(num));
						flag = false;
						num = 0;
						break;
					}
					if (flag2)
					{
						num = c2;
						text += Convert.ToString(Convert.ToChar(num));
						flag = false;
						num = 0;
					}
				}
				else
				{
					num = num * 256 + c;
					text += Convert.ToString(Convert.ToChar(num));
					num = 0;
				}
			}
			decodedText = text;
		}
		return decodedText;
	}

	internal string MapHebrewCharacters(string hexChar)
	{
		if (hexChar.Substring(0, 2) == "02")
		{
			int num = int.Parse(hexChar, NumberStyles.HexNumber);
			hexChar = (num + 816).ToString("X");
		}
		else if (hexChar.Substring(0, 2) == "00")
		{
			if (hexChar.Substring(2, 1) == "0" || hexChar.Substring(2, 1) == "1")
			{
				int num2 = int.Parse(hexChar, NumberStyles.HexNumber);
				hexChar = (num2 + 29).ToString("X");
			}
			else
			{
				int num3 = int.Parse(hexChar, NumberStyles.HexNumber);
				hexChar = (num3 + 1335).ToString("X");
			}
		}
		return hexChar;
	}

	private string GetFontName()
	{
		string text = string.Empty;
		IsSystemFontExist = false;
		new List<string>();
		if (m_fontDictionary != null && m_fontDictionary.ContainsKey("BaseFont"))
		{
			PdfName pdfName = m_fontDictionary["BaseFont"] as PdfName;
			if (pdfName == null)
			{
				pdfName = (m_fontDictionary["BaseFont"] as PdfReferenceHolder).Object as PdfName;
			}
			string text2 = pdfName.Value;
			if (text2.Contains("#20") && !text2.Contains("+"))
			{
				text2 = text2[..text2.LastIndexOf("#20")];
				text2 += "+";
			}
			text2.Contains("+");
			if (!IsSystemFontExist)
			{
				text = ((!pdfName.Value.Contains("+")) ? pdfName.Value : pdfName.Value.Split('+')[1]);
				if (text.Contains("-"))
				{
					text = text.Split('-')[0];
				}
				else if (text.Contains(","))
				{
					text = text.Split(',')[0];
				}
				if (text.Contains("MT"))
				{
					text = text.Replace("MT", "");
				}
				if (text.Contains("#20"))
				{
					text = text.Replace("#20", " ");
				}
				if (text.Contains("#"))
				{
					text = DecodeHexFontName(text);
				}
			}
		}
		return text;
	}

	private string DecodeHexFontName(string fontName)
	{
		StringBuilder stringBuilder = new StringBuilder(fontName);
		for (int i = 0; i < fontName.Length; i++)
		{
			if (fontName[i] == '#')
			{
				string text = fontName[i + 1].ToString() + fontName[i + 2];
				int num = int.Parse(text, NumberStyles.HexNumber);
				if (num != 0)
				{
					stringBuilder.Replace("#" + text.ToString(), ((char)num).ToString());
					i += 2;
				}
				if (!stringBuilder.ToString().Contains("#"))
				{
					break;
				}
			}
		}
		return stringBuilder.ToString();
	}

	private FontStyle GetFontStyle()
	{
		FontStyle result = FontStyle.Regular;
		if (m_fontDictionary != null && m_fontDictionary.ContainsKey("BaseFont"))
		{
			PdfName pdfName = m_fontDictionary["BaseFont"] as PdfName;
			if (pdfName == null)
			{
				pdfName = (m_fontDictionary["BaseFont"] as PdfReferenceHolder).Object as PdfName;
			}
			if (pdfName.Value.Contains("-") || pdfName.Value.Contains(","))
			{
				string text = string.Empty;
				if (pdfName.Value.Contains("-"))
				{
					text = pdfName.Value.Split('-')[1];
				}
				else if (pdfName.Value.Contains(","))
				{
					text = pdfName.Value.Split(',')[1];
				}
				switch (text.Replace("MT", ""))
				{
				case "Italic":
				case "Oblique":
					result = FontStyle.Italic;
					break;
				case "Bold":
				case "BoldMT":
					result = FontStyle.Bold;
					break;
				case "BoldItalic":
				case "BoldOblique":
					result = (FontStyle)3;
					break;
				}
			}
			else
			{
				if (pdfName.Value.Contains("Bold"))
				{
					result = FontStyle.Bold;
				}
				if (pdfName.Value.Contains("BoldItalic") || pdfName.Value.Contains("BoldOblique"))
				{
					result = (FontStyle)3;
				}
				if (pdfName.Value.Contains("Italic") || pdfName.Value.Contains("Oblique"))
				{
					result = FontStyle.Italic;
				}
			}
		}
		return result;
	}

	private string GetFontEncoding()
	{
		PdfName pdfName = new PdfName();
		string text = string.Empty;
		if (m_fontDictionary != null && m_fontDictionary.ContainsKey("Encoding"))
		{
			pdfName = m_fontDictionary["Encoding"] as PdfName;
			if (pdfName == null)
			{
				PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_fontDictionary["Encoding"]) as PdfDictionary;
				if (pdfDictionary != null && pdfDictionary.ContainsKey("Type"))
				{
					text = (PdfCrossTable.Dereference(pdfDictionary["Type"]) as PdfName).Value;
				}
				if (pdfDictionary != null && pdfDictionary.ContainsKey("BaseEncoding"))
				{
					m_baseFontEncoding = (PdfCrossTable.Dereference(pdfDictionary["BaseEncoding"]) as PdfName).Value;
				}
			}
			else
			{
				text = pdfName.Value;
			}
		}
		if (text == "CMap")
		{
			text = "Identity-H";
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = PdfName.DecodeName(text);
		}
		return text;
	}

	internal bool GetType1Font()
	{
		PdfDictionary fontDictionary = m_fontDictionary;
		PdfName pdfName = null;
		PdfName pdfName2 = null;
		if (fontDictionary != null && fontDictionary.ContainsKey("Subtype"))
		{
			pdfName2 = PdfCrossTable.Dereference(fontDictionary["Subtype"]) as PdfName;
			if (fontDictionary.ContainsKey("Encoding") && PdfCrossTable.Dereference(fontDictionary["Encoding"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("BaseEncoding"))
			{
				pdfName = PdfCrossTable.Dereference(pdfDictionary["BaseEncoding"]) as PdfName;
			}
		}
		if (pdfName2 != null && pdfName2.Value == "Type1" && pdfName != null && pdfName.Value == "WinAnsiEncoding" && !fontDictionary.ContainsKey("ToUnicode") && fontDictionary.ContainsKey("FontDescriptor") && PdfCrossTable.Dereference(fontDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("FontFile3"))
		{
			return IsType1Font = true;
		}
		return false;
	}

	private bool IsFontEmbedded()
	{
		bool result = false;
		if (FontDictionary != null)
		{
			PdfDictionary fontDictionary = FontDictionary;
			if (fontDictionary.ContainsKey("Encoding"))
			{
				PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_fontDictionary["Encoding"]) as PdfDictionary;
				string text = string.Empty;
				if (pdfDictionary != null && pdfDictionary.ContainsKey("Type"))
				{
					text = (PdfCrossTable.Dereference(pdfDictionary["Type"]) as PdfName).Value;
				}
				if (text != null && text == "Encoding" && fontDictionary.ContainsKey("FontDescriptor") && PdfCrossTable.Dereference(fontDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("FontFile3"))
				{
					result = true;
				}
			}
		}
		return result;
	}

	private Dictionary<int, string> AdobeJapanCidMap(StreamReader reader)
	{
		string[] separator = new string[8] { "..", "/uni", ".vert", ".hw", ".dup1", "/Japan1.", ".dup2", ".italic" };
		string[] array = (string.Empty + reader.ReadToEnd()).Split('\n');
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(' ');
			if (!int.TryParse(array2[0], out var result))
			{
				if (array2[0].Contains(".."))
				{
					string[] array3 = array2[0].Split(separator, StringSplitOptions.None);
					int num = int.Parse(array3[0]);
					int num2 = int.Parse(array3[1]);
					int num3 = 0;
					for (int j = num; j <= num2; j++)
					{
						string value = char.ConvertFromUtf32(int.Parse(array2[1], NumberStyles.HexNumber) + num3);
						dictionary.Add(j, value);
						num3++;
					}
				}
				continue;
			}
			if (array2[1].Contains("\r"))
			{
				array2[1] = array2[1].Replace("\r", "");
			}
			if (array2[1].Contains("/Japan1"))
			{
				array2[1] = "JPN" + array2[1].Split(separator, StringSplitOptions.RemoveEmptyEntries)[0];
				dictionary.Add(result, array2[1]);
				continue;
			}
			if (array2[1] == "/.notdef")
			{
				array2[1] = "0000";
			}
			else if (array2[1].Contains(","))
			{
				array2[1] = array2[1].Split(',')[0];
			}
			else if (array2[1].Contains("/uni"))
			{
				array2[1] = array2[1].Split(separator, StringSplitOptions.RemoveEmptyEntries)[0];
			}
			string value2 = char.ConvertFromUtf32(int.Parse(array2[1], NumberStyles.HexNumber));
			dictionary.Add(result, value2);
		}
		return dictionary;
	}

	internal string AdobeJapanCidMapTableGlyphParser(string mapChar)
	{
		mapChar = AdobeJapanCidMapTable[Convert.ToChar(mapChar)];
		if (mapChar.Contains("JPN"))
		{
			mapChar = AdobeJapanCidMapReference(mapChar);
		}
		return mapChar;
	}

	private string AdobeJapanCidMapReference(string mapValue)
	{
		int key = int.Parse(mapValue.Split(new string[1] { "JPN" }, StringSplitOptions.None)[1]);
		if (AdobeJapanCidMapTable.ContainsKey(key))
		{
			mapValue = AdobeJapanCidMapTable[key];
			if (mapValue.Contains("JPN"))
			{
				mapValue = AdobeJapanCidMapReference(mapValue);
			}
		}
		return mapValue;
	}

	private void GetGlyphWidthsType1()
	{
		int num = 0;
		PdfDictionary fontDictionary = m_fontDictionary;
		if (fontDictionary.ContainsKey("DW"))
		{
			DefaultGlyphWidth = (fontDictionary["DW"] as PdfNumber).FloatValue;
		}
		if (fontDictionary.ContainsKey("FirstChar"))
		{
			num = (fontDictionary["FirstChar"] as PdfNumber).IntValue;
		}
		if (fontDictionary.ContainsKey("LastChar"))
		{
			_ = (fontDictionary["LastChar"] as PdfNumber).IntValue;
		}
		m_fontGlyphWidth = new Dictionary<int, int>();
		m_fontGlyphWidthMapping = new Dictionary<int, int>();
		PdfArray pdfArray = null;
		int num2 = 0;
		if (fontDictionary["Widths"] is PdfArray)
		{
			pdfArray = fontDictionary["Widths"] as PdfArray;
		}
		if (fontDictionary["Widths"] is PdfReferenceHolder)
		{
			pdfArray = (fontDictionary["Widths"] as PdfReferenceHolder).Object as PdfArray;
		}
		if (pdfArray == null)
		{
			return;
		}
		try
		{
			for (int i = 0; i < pdfArray.Count; i++)
			{
				num2 = num + i;
				if (CharacterMapTable.ContainsKey(num2))
				{
					int key = CharacterMapTable[num2].ToCharArray()[0];
					if (!m_fontGlyphWidthMapping.ContainsKey(key))
					{
						m_fontGlyphWidthMapping.Add(key, (pdfArray[i] as PdfNumber).IntValue);
					}
				}
				else if (DifferencesDictionary.ContainsKey(num2.ToString()))
				{
					string text = DifferencesDictionary[num2.ToString()];
					int key2 = num2;
					if (text.Length == 1)
					{
						key2 = text.ToCharArray()[0];
					}
					if (!m_fontGlyphWidthMapping.ContainsKey(key2))
					{
						m_fontGlyphWidthMapping.Add(key2, (pdfArray[i] as PdfNumber).IntValue);
					}
				}
				else if (!m_fontGlyphWidthMapping.ContainsKey(num2))
				{
					m_fontGlyphWidthMapping.Add(num2, (pdfArray[i] as PdfNumber).IntValue);
				}
				m_fontGlyphWidth.Add(num2, (pdfArray[i] as PdfNumber).IntValue);
			}
		}
		catch
		{
			m_fontGlyphWidth = null;
		}
	}

	private void GetGlyphWidthsNonIdH()
	{
		int num = 0;
		PdfDictionary fontDictionary = m_fontDictionary;
		if (fontDictionary != null && fontDictionary.ContainsKey("DW"))
		{
			DefaultGlyphWidth = (fontDictionary["DW"] as PdfNumber).FloatValue;
		}
		if (fontDictionary != null && fontDictionary.ContainsKey("FirstChar"))
		{
			num = (fontDictionary["FirstChar"] as PdfNumber).IntValue;
		}
		if (fontDictionary != null && fontDictionary.ContainsKey("LastChar"))
		{
			_ = (fontDictionary["LastChar"] as PdfNumber).IntValue;
		}
		m_fontGlyphWidth = new Dictionary<int, int>();
		PdfArray pdfArray = null;
		int num2 = 0;
		if (fontDictionary != null && fontDictionary["Widths"] is PdfArray)
		{
			pdfArray = fontDictionary["Widths"] as PdfArray;
		}
		if (fontDictionary != null && fontDictionary["Widths"] is PdfReferenceHolder)
		{
			pdfArray = (fontDictionary["Widths"] as PdfReferenceHolder).Object as PdfArray;
		}
		if (fontDictionary != null && fontDictionary.Items.ContainsKey(new PdfName("DescendantFonts")) && fontDictionary["DescendantFonts"] is PdfArray pdfArray2 && pdfArray2[0] is PdfReferenceHolder)
		{
			PdfDictionary pdfDictionary = (pdfArray2[0] as PdfReferenceHolder).Object as PdfDictionary;
			if (pdfDictionary.ContainsKey("W"))
			{
				pdfArray = pdfDictionary["W"] as PdfArray;
			}
		}
		if (pdfArray != null)
		{
			try
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					num2 = num + i;
					if (CharacterMapTable.Count > 0 || DifferencesDictionary.Count > 0)
					{
						if (CharacterMapTable.ContainsKey(num2))
						{
							_ = CharacterMapTable[num2];
							int key = num2;
							if (!m_fontGlyphWidth.ContainsKey(key))
							{
								if (pdfArray[i] is PdfNumber pdfNumber)
								{
									m_fontGlyphWidth.Add(key, pdfNumber.IntValue);
								}
								else if (pdfArray[i] is PdfArray pdfArray3 && pdfArray3[0] is PdfNumber pdfNumber2)
								{
									m_fontGlyphWidth.Add(key, pdfNumber2.IntValue);
								}
							}
						}
						else if (DifferencesDictionary.ContainsKey(num2.ToString()))
						{
							_ = DifferencesDictionary[num2.ToString()];
							int key2 = num2;
							if (!m_fontGlyphWidth.ContainsKey(key2))
							{
								m_fontGlyphWidth.Add(key2, (pdfArray[i] as PdfNumber).IntValue);
							}
						}
						else if (!m_fontGlyphWidth.ContainsKey(num2))
						{
							m_fontGlyphWidth.Add(num2, (pdfArray[i] as PdfNumber).IntValue);
						}
					}
					else if (pdfArray[i] is PdfArray)
					{
						PdfArray pdfArray4 = pdfArray[i] as PdfArray;
						for (int j = i; j < pdfArray4.Count; j++)
						{
							num2 = num + j;
							if (CharacterMapTable.Count > 0 || DifferencesDictionary.Count > 0)
							{
								if (CharacterMapTable.ContainsKey(num2))
								{
									int key3 = CharacterMapTable[num2].ToCharArray()[0];
									if (!m_fontGlyphWidth.ContainsKey(key3))
									{
										m_fontGlyphWidth.Add(key3, (pdfArray4[j] as PdfNumber).IntValue);
									}
								}
								else if (DifferencesDictionary.ContainsKey(num2.ToString()))
								{
									_ = DifferencesDictionary[num2.ToString()];
									int key4 = num2;
									if (!m_fontGlyphWidth.ContainsKey(key4))
									{
										m_fontGlyphWidth.Add(key4, (pdfArray4[j] as PdfNumber).IntValue);
									}
								}
								else if (!m_fontGlyphWidth.ContainsKey(num2))
								{
									m_fontGlyphWidth.Add(num2, (pdfArray4[j] as PdfNumber).IntValue);
								}
							}
							else
							{
								m_fontGlyphWidth.Add(num2, (pdfArray4[j] as PdfNumber).IntValue);
							}
						}
					}
					else
					{
						int intValue = (pdfArray[i] as PdfNumber).IntValue;
						if (!m_fontGlyphWidth.ContainsKey(num2))
						{
							m_fontGlyphWidth.Add(num2, intValue);
						}
					}
				}
			}
			catch
			{
				m_fontGlyphWidth = null;
			}
		}
		if (m_fontGlyphWidth == null || !(fontType != null) || m_fontGlyphWidth.Count != 0 || !(FontEncoding == "WinAnsiEncoding") || !(fontType.Value == "TrueType"))
		{
			return;
		}
		PdfName pdfName = PdfCrossTable.Dereference(fontDictionary["BaseFont"]) as PdfName;
		if (!(pdfName != null))
		{
			return;
		}
		PdfFontStyle fontStyle = PdfFontStyle.Regular;
		if (FontStyle == FontStyle.Bold)
		{
			fontStyle = PdfFontStyle.Bold;
		}
		else if (FontStyle == FontStyle.Italic)
		{
			fontStyle = PdfFontStyle.Italic;
		}
		PdfFontMetrics pdfFontMetrics = null;
		switch (pdfName.Value)
		{
		case "Arial":
		case "ArialMT":
			pdfFontMetrics = PdfStandardFontMetricsFactory.GetMetrics(PdfFontFamily.Helvetica, fontStyle, FontSize);
			break;
		case "CourierCourierNew":
		case "CourierNew":
			pdfFontMetrics = PdfStandardFontMetricsFactory.GetMetrics(PdfFontFamily.Courier, fontStyle, FontSize);
			break;
		case "Times":
		case "TimesNewRomanPSMT":
		case "TimesNewRoman":
			pdfFontMetrics = PdfStandardFontMetricsFactory.GetMetrics(PdfFontFamily.TimesRoman, fontStyle, FontSize);
			break;
		case "Symbol":
			pdfFontMetrics = PdfStandardFontMetricsFactory.GetMetrics(PdfFontFamily.Symbol, fontStyle, FontSize);
			break;
		}
		if (pdfFontMetrics == null || pdfFontMetrics.WidthTable == null)
		{
			return;
		}
		PdfArray pdfArray5 = pdfFontMetrics.WidthTable.ToArray();
		num2 = 32;
		foreach (PdfNumber item in pdfArray5)
		{
			m_fontGlyphWidth.Add(num2, item.IntValue);
			num2++;
		}
	}

	private bool isMpdfaaFonts()
	{
		bool result = false;
		if (m_fontDictionary.ContainsKey("BaseFont"))
		{
			PdfName pdfName = m_fontDictionary["BaseFont"] as PdfName;
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

	private void GetGlyphWidths()
	{
		if (FontEncoding != "Identity-H")
		{
			return;
		}
		PdfDictionary pdfDictionary = m_fontDictionary;
		if (pdfDictionary.ContainsKey("DescendantFonts"))
		{
			PdfArray pdfArray = null;
			if (pdfDictionary["DescendantFonts"] is PdfArray)
			{
				pdfArray = pdfDictionary["DescendantFonts"] as PdfArray;
			}
			if (pdfDictionary["DescendantFonts"] is PdfReferenceHolder)
			{
				pdfArray = (pdfDictionary["DescendantFonts"] as PdfReferenceHolder).Object as PdfArray;
			}
			pdfDictionary = pdfArray[0] as PdfDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = (pdfArray[0] as PdfReferenceHolder).Object as PdfDictionary;
			}
			pdfArray = null;
		}
		m_fontGlyphWidth = new Dictionary<int, int>();
		PdfArray pdfArray2 = null;
		int i = 0;
		int num = 0;
		PdfArray pdfArray3 = null;
		if (pdfDictionary["W"] is PdfArray)
		{
			pdfArray2 = pdfDictionary["W"] as PdfArray;
		}
		if (pdfDictionary["W"] is PdfReferenceHolder)
		{
			pdfArray2 = (pdfDictionary["W"] as PdfReferenceHolder).Object as PdfArray;
		}
		if (pdfDictionary.ContainsKey("DW"))
		{
			DefaultGlyphWidth = (pdfDictionary["DW"] as PdfNumber).FloatValue;
		}
		try
		{
			if (pdfArray2 == null)
			{
				m_fontGlyphWidth = null;
				return;
			}
			int num2;
			for (num2 = 0; num2 < pdfArray2.Count; num2++)
			{
				if (pdfArray2[num2] is PdfNumber)
				{
					i = (pdfArray2[num2] as PdfNumber).IntValue;
				}
				num2++;
				if (pdfArray2[num2] is PdfArray)
				{
					pdfArray3 = pdfArray2[num2] as PdfArray;
					for (int j = 0; j < pdfArray3.Count; j++)
					{
						if (!m_containsCmap)
						{
							m_fontGlyphWidth.Add(i, (pdfArray3[j] as PdfNumber).IntValue);
						}
						else if (!m_fontGlyphWidth.ContainsKey(i))
						{
							m_fontGlyphWidth.Add(i, (pdfArray3[j] as PdfNumber).IntValue);
						}
						i++;
					}
				}
				else if (pdfArray2[num2] is PdfNumber)
				{
					num = (pdfArray2[num2] as PdfNumber).IntValue;
					num2++;
					for (; i <= num; i++)
					{
						if (!m_fontGlyphWidth.ContainsKey(i))
						{
							m_fontGlyphWidth.Add(i, (pdfArray2[num2] as PdfNumber).IntValue);
						}
					}
				}
				else if (pdfArray2[num2] is PdfReferenceHolder)
				{
					pdfArray3 = (pdfArray2[num2] as PdfReferenceHolder).Object as PdfArray;
					for (int k = 0; k < pdfArray3.Count; k++)
					{
						if (!m_containsCmap)
						{
							m_fontGlyphWidth.Add(i, (pdfArray3[k] as PdfNumber).IntValue);
						}
						else if (!m_fontGlyphWidth.ContainsKey(i))
						{
							m_fontGlyphWidth.Add(i, (pdfArray3[k] as PdfNumber).IntValue);
						}
						i++;
					}
				}
			}
		}
		catch
		{
			m_fontGlyphWidth = null;
		}
		pdfArray2 = null;
		pdfArray3 = null;
		pdfDictionary = null;
	}

	private string[] GetFontFilter(PdfDictionary streamDictionary)
	{
		string[] array = null;
		if (streamDictionary != null && streamDictionary.ContainsKey("Filter"))
		{
			if (streamDictionary["Filter"] is PdfName)
			{
				array = new string[1] { (streamDictionary["Filter"] as PdfName).Value };
			}
			else if (streamDictionary["Filter"] is PdfArray)
			{
				PdfArray pdfArray = streamDictionary["Filter"] as PdfArray;
				array = new string[pdfArray.Count];
				for (int i = 0; i < pdfArray.Count; i++)
				{
					array[i] = (pdfArray[i] as PdfName).Value;
				}
			}
			else if (streamDictionary["Filter"] is PdfReferenceHolder)
			{
				PdfArray pdfArray2 = (streamDictionary["Filter"] as PdfReferenceHolder).Object as PdfArray;
				array = new string[pdfArray2.Count];
				for (int j = 0; j < pdfArray2.Count; j++)
				{
					array[j] = (pdfArray2[j] as PdfName).Value;
				}
			}
		}
		return array;
	}

	private Dictionary<double, string> GetCidToGidTable(byte[] cidTOGidmap)
	{
		Dictionary<double, string> dictionary = new Dictionary<double, string>();
		byte[] array = new byte[2];
		int num = 0;
		int num2;
		for (num2 = 0; num2 < cidTOGidmap.Length; num2++)
		{
			array[0] = cidTOGidmap[num2];
			array[1] = cidTOGidmap[++num2];
			string text = ((!(FontEncoding == "Identity-H")) ? Encoding.UTF8.GetString(array, 0, array.Length) : Encoding.BigEndianUnicode.GetString(array, 0, array.Length));
			text = text.Replace("\0", "");
			dictionary.Add(num, text);
			num++;
		}
		return dictionary;
	}

	private Dictionary<string, double> GetReverseMapTable()
	{
		m_reverseMapTable = new Dictionary<string, double>();
		foreach (KeyValuePair<double, string> item in CharacterMapTable)
		{
			if (!m_reverseMapTable.ContainsKey(item.Value))
			{
				m_reverseMapTable.Add(item.Value, item.Key);
			}
		}
		return m_reverseMapTable;
	}

	private Dictionary<double, string> GetCharacterMapTable()
	{
		int num = 0;
		Dictionary<double, string> dictionary = new Dictionary<double, string>();
		if (m_fontDictionary != null && m_fontDictionary.ContainsKey("ToUnicode"))
		{
			IPdfPrimitive pdfPrimitive = m_fontDictionary["ToUnicode"];
			PdfStream pdfStream = ((!(pdfPrimitive is PdfReferenceHolder)) ? (pdfPrimitive as PdfStream) : ((pdfPrimitive as PdfReferenceHolder).Object as PdfStream));
			pdfPrimitive = null;
			if (pdfStream != null)
			{
				byte[] decompressedData = pdfStream.GetDecompressedData();
				string @string = Encoding.UTF8.GetString(decompressedData, 0, decompressedData.Length);
				bool flag = false;
				bool flag2 = false;
				int num2 = @string.IndexOf("begincmap");
				int num3 = @string.IndexOf("endcmap");
				int num4 = num2;
				int num5 = num2;
				int num6 = num3;
				if (num4 == -1)
				{
					return dictionary;
				}
				while (true)
				{
					if (!flag)
					{
						num5 = @string.IndexOf("beginbfchar", num4);
						if (num5 < 0)
						{
							flag2 = false;
							num5 = num2;
							num4 = num2;
							num6 = num3;
						}
						else
						{
							num6 = @string.IndexOf("endbfchar", num5);
							num4 = num6;
							flag2 = true;
						}
					}
					if (!flag2)
					{
						int num7 = @string.IndexOf("beginbfrange", num4);
						if (num7 < 0)
						{
							flag = false;
						}
						else
						{
							int num8 = @string.IndexOf("endbfrange", num4 + 5);
							num5 = num7;
							num6 = num8;
							num4 = num6;
							flag = true;
						}
					}
					if (!(flag2 || flag))
					{
						break;
					}
					string text = @string.Substring(num5, num6 - num5);
					if (flag2)
					{
						char[] separator = new char[2] { '\n', '\r' };
						string[] array = text.Split(separator);
						if (!array[0].Contains("\n") && !array[0].Contains("\r"))
						{
							List<string> list = new List<string>();
							for (int i = 0; i < array.Length; i++)
							{
								list = GetHexCode(array[i]);
								int count = list.Count;
								for (int j = 0; j < count / 2; j++)
								{
									if (list.Count < 2)
									{
										continue;
									}
									List<string> list2 = new List<string>();
									list2.Add(list[0]);
									list2.Add(list[1]);
									list.Remove(list[0]);
									list.Remove(list[0]);
									if (list2.Count <= 1)
									{
										continue;
									}
									if (list2[1].Length > 4)
									{
										string text2 = list2[1];
										text2 = text2.Replace(" ", "");
										string text3 = "";
										int num9 = text2.Length / 4;
										for (int k = 0; k < num9; k++)
										{
											char c = (char)long.Parse(text2.Substring(0, 4), NumberStyles.HexNumber);
											text2 = text2.Substring(4);
											text3 += c;
										}
										text3 = CheckContainInvalidChar(text3);
										if (!dictionary.ContainsKey(long.Parse(list2[0], NumberStyles.HexNumber)))
										{
											dictionary.Add(long.Parse(list2[0], NumberStyles.HexNumber), text3.ToString());
										}
									}
									else if (!dictionary.ContainsKey(long.Parse(list2[0], NumberStyles.HexNumber)))
									{
										char c2 = (char)long.Parse(list2[1], NumberStyles.HexNumber);
										dictionary.Add(long.Parse(list2[0], NumberStyles.HexNumber), c2.ToString());
									}
								}
							}
							continue;
						}
						for (int l = 0; l < array.Length; l++)
						{
							tempStringList = GetHexCode(array[l]);
							if (tempStringList.Count <= 1)
							{
								continue;
							}
							if (tempStringList[1].Length > 4)
							{
								string text4 = tempStringList[1];
								text4 = text4.Replace(" ", "");
								string text5 = "";
								int num10 = text4.Length / 4;
								for (int m = 0; m < num10; m++)
								{
									char c3 = (char)long.Parse(text4.Substring(0, 4), NumberStyles.HexNumber);
									text4 = text4.Substring(4);
									text5 += c3;
								}
								text5 = CheckContainInvalidChar(text5);
								if (!dictionary.ContainsKey(long.Parse(tempStringList[0], NumberStyles.HexNumber)))
								{
									dictionary.Add(long.Parse(tempStringList[0], NumberStyles.HexNumber), text5.ToString());
								}
							}
							else if (!dictionary.ContainsKey(long.Parse(tempStringList[0], NumberStyles.HexNumber)))
							{
								char c4 = (char)long.Parse(tempStringList[1], NumberStyles.HexNumber);
								dictionary.Add(long.Parse(tempStringList[0], NumberStyles.HexNumber), c4.ToString());
							}
						}
					}
					else
					{
						if (!flag)
						{
							continue;
						}
						char[] separator2 = new char[2] { '\n', '\r' };
						string[] array2 = text.Split(separator2);
						string text6 = " ";
						for (int n = 0; n < array2.Length; n++)
						{
							if (array2[n].Contains("["))
							{
								int num11 = array2[n].IndexOf("[");
								int num12 = array2[n].IndexOf("]");
								if (num12 == -1)
								{
									text6 = array2[n].Substring(num11, array2[n].Length - num11);
									for (n++; !array2[n].Contains("]"); n++)
									{
										text6 += array2[n];
									}
									text6 += array2[n].Substring(0, array2[n].IndexOf("]"));
								}
								else
								{
									text6 = array2[n].Substring(num11, num12 - num11);
								}
								List<string> list3 = new List<string>();
								list3 = GetHexCode(text6);
								string text7 = " ";
								if (num12 == -1)
								{
									for (int num13 = num + 1; num13 <= n; num13++)
									{
										text7 += array2[num13];
									}
									tempStringList = GetHexCode(text7);
								}
								else
								{
									tempStringList = GetHexCode(array2[n]);
								}
								num = n;
								if (tempStringList.Count <= 1)
								{
									continue;
								}
								double num14 = long.Parse(tempStringList[0], NumberStyles.HexNumber);
								double num15 = long.Parse(tempStringList[1], NumberStyles.HexNumber);
								int num16 = 0;
								double num17 = num14;
								double num18 = 0.0;
								while (num17 <= num15)
								{
									string text8 = string.Empty;
									if (num16 < list3.Count)
									{
										string[] array3 = list3[num16].Split(' ');
										foreach (string text9 in array3)
										{
											long result = -1L;
											if (long.TryParse(text9, out result) || text9.Length <= 18)
											{
												text8 += (char)(double)long.Parse(((int)Convert.ToInt64(text9, 16)).ToString("x"), NumberStyles.HexNumber);
											}
										}
									}
									if (!dictionary.ContainsKey(num17))
									{
										dictionary.Add(num17, text8);
									}
									num17 += 1.0;
									num18 += 1.0;
									num16++;
								}
								continue;
							}
							tempStringList = GetHexCode(array2[n]);
							if (tempStringList.Count == 3)
							{
								double num20 = long.Parse(tempStringList[0], NumberStyles.HexNumber);
								double num15 = long.Parse(tempStringList[1], NumberStyles.HexNumber);
								string text10 = tempStringList[2];
								if (tempStringList[2].Length > 4)
								{
									text10 = text10.Substring(1, 4);
								}
								text10 = text10.Trim();
								double num21 = Convert.ToInt64(text10, 16);
								double num22 = num20;
								double num23 = 0.0;
								while (num22 <= num15)
								{
									char c5 = (char)(double)long.Parse(((int)(num21 + num23)).ToString("x"), NumberStyles.HexNumber);
									if (!dictionary.ContainsKey(num22))
									{
										dictionary.Add(num22, c5.ToString());
									}
									num22 += 1.0;
									num23 += 1.0;
								}
							}
							else if (tempStringList.Count > 1)
							{
								int count2 = tempStringList.Count;
								for (int num24 = 0; num24 < count2; num24 += 3)
								{
									char c6 = (char)long.Parse(tempStringList[num24 + 2], NumberStyles.HexNumber);
									dictionary.Add(long.Parse(tempStringList[num24], NumberStyles.HexNumber), c6.ToString());
								}
							}
						}
					}
				}
			}
			pdfStream = null;
		}
		if (m_isSameFont)
		{
			foreach (KeyValuePair<double, string> item in dictionary)
			{
				if (!tempMapTable.ContainsKey(item.Key))
				{
					tempMapTable.Add(item.Key, item.Value);
					continue;
				}
				tempMapTable.Remove(item.Key);
				tempMapTable.Add(item.Key, item.Value);
			}
		}
		return dictionary;
	}

	private Dictionary<string, string> GetDifferencesDictionary()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		PdfDictionary pdfDictionary = null;
		if (m_fontDictionary != null && m_fontDictionary.ContainsKey("Encoding"))
		{
			if (m_fontDictionary["Encoding"] is PdfReferenceHolder)
			{
				pdfDictionary = (m_fontDictionary["Encoding"] as PdfReferenceHolder).Object as PdfDictionary;
			}
			else if (m_fontDictionary["Encoding"] is PdfDictionary)
			{
				pdfDictionary = m_fontDictionary["Encoding"] as PdfDictionary;
			}
			if (pdfDictionary != null && pdfDictionary.ContainsKey("Differences"))
			{
				int num = 0;
				PdfArray pdfArray = pdfDictionary["Differences"] as PdfArray;
				if (pdfArray == null)
				{
					pdfArray = (pdfDictionary["Differences"] as PdfReferenceHolder).Object as PdfArray;
				}
				for (int i = 0; i < pdfArray.Count; i++)
				{
					string empty = string.Empty;
					if (pdfArray[i] is PdfNumber)
					{
						empty = (pdfArray[i] as PdfNumber).FloatValue.ToString();
						num = int.Parse(empty);
					}
					else
					{
						if (!(pdfArray[i] is PdfName))
						{
							continue;
						}
						empty = (pdfArray[i] as PdfName).Value;
						if (fontType.Value == "Type1" && empty == ".notdef")
						{
							empty = " ";
							dictionary.Add(num.ToString(), GetLatinCharacter(empty));
							num++;
							continue;
						}
						empty = GetLatinCharacter(empty);
						empty = GetSpecialCharacter(empty);
						if (!dictionary.ContainsKey(num.ToString()))
						{
							dictionary.Add(num.ToString(), GetLatinCharacter(empty));
						}
						num++;
					}
				}
				pdfArray = null;
			}
		}
		pdfDictionary = null;
		return dictionary;
	}

	internal static string GetLatinCharacter(string decodedCharacter)
	{
		return decodedCharacter switch
		{
			"zero" => "0", 
			"one" => "1", 
			"two" => "2", 
			"three" => "3", 
			"four" => "4", 
			"five" => "5", 
			"six" => "6", 
			"seven" => "7", 
			"eight" => "8", 
			"nine" => "9", 
			"aacute" => "á", 
			"asciicircum" => "^", 
			"asciitilde" => "~", 
			"asterisk" => "*", 
			"at" => "@", 
			"atilde" => "ã", 
			"backslash" => "\\", 
			"bar" => "|", 
			"braceleft" => "{", 
			"braceright" => "}", 
			"bracketleft" => "[", 
			"bracketright" => "]", 
			"breve" => "\u02d8", 
			"brokenbar" => "|", 
			"bullet3" => "•", 
			"bullet" => "•", 
			"caron" => "ˇ", 
			"ccedilla" => "ç", 
			"cedilla" => "\u00b8", 
			"cent" => "¢", 
			"circumflex" => "ˆ", 
			"colon" => ":", 
			"comma" => ",", 
			"copyright" => "©", 
			"currency1" => "¤", 
			"dagger" => "†", 
			"daggerdbl" => "‡", 
			"degree" => "°", 
			"dieresis" => "\u00a8", 
			"divide" => "÷", 
			"dollar" => "$", 
			"dotaccent" => "\u02d9", 
			"dotlessi" => "ı", 
			"eacute" => "é", 
			"middot" => "\u02d9", 
			"edieresis" => "ë", 
			"egrave" => "è", 
			"ellipsis" => "...", 
			"emdash" => "—", 
			"endash" => "–", 
			"equal" => "=", 
			"eth" => "ð", 
			"exclam" => "!", 
			"exclamdown" => "¡", 
			"florin" => "ƒ", 
			"fraction" => "⁄", 
			"germandbls" => "ß", 
			"grave" => "`", 
			"greater" => ">", 
			"guillemotleft4" => "«", 
			"guillemotright4" => "»", 
			"guilsinglleft" => "‹", 
			"guilsinglright" => "›", 
			"hungarumlaut" => "\u02dd", 
			"hyphen5" => "-", 
			"iacute" => "í", 
			"icircumflex" => "î", 
			"idieresis" => "ï", 
			"igrave" => "ì", 
			"less" => "<", 
			"logicalnot" => "¬", 
			"lslash" => "ł", 
			"Lslash" => "Ł", 
			"macron" => "\u00af", 
			"minus" => "−", 
			"mu" => "μ", 
			"multiply" => "×", 
			"ntilde" => "ñ", 
			"numbersign" => "#", 
			"oacute" => "ó", 
			"ocircumflex" => "ô", 
			"odieresis" => "ö", 
			"oe" => "oe", 
			"ogonek" => "\u02db", 
			"ograve" => "ò", 
			"onehalf" => "1/2", 
			"onequarter" => "1/4", 
			"onesuperior" => "¹", 
			"ordfeminine" => "ª", 
			"ordmasculine" => "º", 
			"otilde" => "õ", 
			"paragraph" => "¶", 
			"parenleft" => "(", 
			"parenright" => ")", 
			"percent" => "%", 
			"period" => ".", 
			"periodcentered" => "·", 
			"perthousand" => "‰", 
			"plus" => "+", 
			"plusminus" => "±", 
			"question" => "?", 
			"questiondown" => "¿", 
			"quotedbl" => "\"", 
			"quotedblbase" => "„", 
			"quotedblleft" => "“", 
			"quotedblright" => "”", 
			"quoteleft" => "‘", 
			"quoteright" => "’", 
			"quotesinglbase" => "‚", 
			"quotesingle" => "'", 
			"registered" => "®", 
			"ring" => "\u02da", 
			"scaron" => "š", 
			"section" => "§", 
			"semicolon" => ";", 
			"slash" => "/", 
			"space6" => " ", 
			"space" => " ", 
			"udieresis" => "ü", 
			"uacute" => "ú", 
			"Ecircumflex" => "Ê", 
			"hyphen" => "-", 
			"underscore" => "_", 
			"adieresis" => "ä", 
			"ampersand" => "&", 
			"Adieresis" => "Ä", 
			"Udieresis" => "Ü", 
			"ccaron" => "č", 
			"Scaron" => "Š", 
			"zcaron" => "ž", 
			"sterling" => "£", 
			"agrave" => "à", 
			"ecircumflex" => "ê", 
			"acircumflex" => "â", 
			"Oacute" => "Ó", 
			_ => decodedCharacter, 
		};
	}

	internal static string GetSpecialCharacter(string decodedCharacter)
	{
		switch (decodedCharacter)
		{
		case "head2right":
			return "➢";
		case "aacute":
			return "a\u0301";
		case "eacute":
			return "e\u0301";
		case "iacute":
			return "i\u0301";
		case "oacute":
			return "o\u0301";
		case "uacute":
			return "u\u0301";
		case "circleright":
			return "➲";
		case "bleft":
			return "⇦";
		case "bright":
			return "⇨";
		case "bup":
			return "⇧";
		case "bdown":
			return "⇩";
		case "barb4right":
			return "➔";
		case "bleftright":
			return "⬄";
		case "bupdown":
			return "⇳";
		case "bnw":
			return "⬀";
		case "bne":
			return "⬁";
		case "bsw":
			return "⬃";
		case "bse":
			return "⬂";
		case "bdash1":
			return "▭";
		case "bdash2":
			return "▫";
		case "xmarkbld":
			return "✗";
		case "checkbld":
			return "✓";
		case "boxxmarkbld":
			return "☒";
		case "boxcheckbld":
			return "☑";
		case "space":
			return " ";
		case "pencil":
			return "✏";
		case "scissors":
			return "✂";
		case "scissorscutting":
			return "✁";
		case "readingglasses":
			return "✁";
		case "bell":
			return "✁";
		case "book":
			return "✁";
		case "telephonesolid":
			return "✁";
		case "telhandsetcirc":
			return "✁";
		case "envelopeback":
			return "✁";
		case "hourglass":
			return "⌛";
		case "keyboard":
			return "⌨";
		case "tapereel":
			return "✇";
		case "handwrite":
			return "✍";
		case "handv":
			return "✌";
		case "handptleft":
			return "☜";
		case "handptright":
			return "☞";
		case "handptup":
			return "☝";
		case "handptdown":
			return "☟";
		case "smileface":
			return "☺";
		case "frownface":
			return "☹";
		case "skullcrossbones":
			return "☠";
		case "flag":
			return "⚐";
		case "pennant":
			return "Ὢ9";
		case "airplane":
			return "✈";
		case "sunshine":
			return "☼";
		case "droplet":
			return "Ὂ7";
		case "snowflake":
			return "❄";
		case "crossshadow":
			return "✞";
		case "crossmaltese":
			return "✠";
		case "starofdavid":
			return "✡";
		case "crescentstar":
			return "☪";
		case "yinyang":
			return "☯";
		case "om":
			return "ॐ";
		case "wheel":
			return "☸";
		case "aries":
			return "♈";
		case "taurus":
			return "♉";
		case "gemini":
			return "♊";
		case "cancer":
			return "♋";
		case "leo":
			return "♌";
		case "virgo":
			return "♍";
		case "libra":
			return "♎";
		case "scorpio":
			return "♏";
		case "saggitarius":
			return "♐";
		case "capricorn":
			return "♑";
		case "aquarius":
			return "♒";
		case "pisces":
			return "♓";
		case "ampersanditlc":
			return "&";
		case "ampersandit":
			return "&";
		case "circle6":
			return "●";
		case "circleshadowdwn":
			return "❍";
		case "square6":
			return "■";
		case "box3":
			return "□";
		case "boxshadowdwn":
			return "❑";
		case "boxshadowup":
			return "❒";
		case "lozenge4":
			return "⬧";
		case "lozenge6":
			return "⧫";
		case "rhombus6":
			return "◆";
		case "xrhombus":
			return "❖";
		case "rhombus4":
			return "⬥";
		case "clear":
			return "⌧";
		case "escape":
			return "⍓";
		case "command":
			return "⌘";
		case "rosette":
			return "❀";
		case "rosettesolid":
			return "✿";
		case "quotedbllftbld":
			return "❝";
		case "quotedblrtbld":
			return "❞";
		case ".notdef":
			return "▯";
		case "zerosans":
			return "⓪";
		case "onesans":
			return "①";
		case "twosans":
			return "②";
		case "threesans":
			return "③";
		case "foursans":
			return "④";
		case "fivesans":
			return "⑤";
		case "sixsans":
			return "⑥";
		case "sevensans":
			return "⑦";
		case "eightsans":
			return "⑧";
		case "ninesans":
			return "⑨";
		case "tensans":
			return "⑩";
		case "zerosansinv":
			return "⓿";
		case "onesansinv":
			return "❶";
		case "twosansinv":
			return "❷";
		case "threesansinv":
			return "❸";
		case "foursansinv":
			return "❹";
		case "circle2":
			return "·";
		case "circle4":
			return "•";
		case "square2":
			return "▪";
		case "ring2":
			return "○";
		case "ringbutton2":
			return "◉";
		case "target":
			return "◎";
		case "square4":
			return "▪";
		case "box2":
			return "◻";
		case "crosstar2":
			return "✦";
		case "pentastar2":
			return "★";
		case "hexstar2":
			return "✶";
		case "octastar2":
			return "✴";
		case "dodecastar3":
			return "✹";
		case "octastar4":
			return "✵";
		case "registercircle":
			return "⌖";
		case "cuspopen":
			return "⟡";
		case "cuspopen1":
			return "⌑";
		case "circlestar":
			return "★";
		case "starshadow":
			return "✰";
		case "deleteleft":
			return "⌫";
		case "deleteright":
			return "⌦";
		case "scissorsoutline":
			return "✄";
		case "telephone":
			return "☏";
		case "telhandset":
			return "ὍE";
		case "handptlft1":
			return "☜";
		case "handptrt1":
			return "☞";
		case "handptlftsld1":
			return "☚";
		case "handptrtsld1":
			return "☛";
		case "handptup1":
			return "☝";
		case "handptdwn1":
			return "☟";
		case "xmark":
			return "✗";
		case "check":
			return "✓";
		case "boxcheck":
			return "☑";
		case "boxx":
			return "☒";
		case "boxxbld":
			return "☒";
		case "circlex":
			return "=⌔";
		case "circlexbld":
			return "⌔";
		case "prohibitbld":
		case "prohibit":
			return "⦸";
		case "ampersanditaldm":
		case "ampersandsandm":
		case "ampersandbld":
		case "ampersandsans":
			return "&";
		case "interrobang":
		case "interrobangsans":
		case "interrobngsandm":
		case "interrobangdm":
			return "‽";
		case "sacute":
			return "ś";
		case "Sacute":
			return "Ś";
		case "eogonek":
			return "ę";
		case "cacute":
			return "ć";
		case "aogonek":
			return "ą";
		default:
			return decodedCharacter;
		}
	}

	internal string MapCharactersFromTable(string decodedText)
	{
		return MapCharactersFromTable(decodedText, null);
	}

	internal string MapCharactersFromTable(string decodedText, RenderedString renderedString)
	{
		string text = string.Empty;
		string empty = string.Empty;
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		if (m_fontDictionary != null && m_fontDictionary.ContainsKey("FontDescriptor") && PdfCrossTable.Dereference(m_fontDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("CharSet") && pdfDictionary.ContainsKey("FontStretch"))
		{
			PdfString pdfString = PdfCrossTable.Dereference(pdfDictionary["CharSet"]) as PdfString;
			PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["FontStretch"]) as PdfName;
			if (pdfString != null && pdfString.Value != string.Empty && pdfName != null && pdfName.Value == "Condensed")
			{
				empty = pdfString.Value.Replace("/", "");
				int num2 = empty.IndexOf("Y");
				empty = empty.Substring(0, num2 + 1);
				for (int i = 0; i < empty.Length; i++)
				{
					if (!char.IsUpper(empty[i]))
					{
						flag = false;
						break;
					}
					flag = true;
				}
			}
		}
		for (int j = 0; j < decodedText.Length; j++)
		{
			char c = decodedText[j];
			if (CharacterMapTable.ContainsKey((int)c) && !flag2)
			{
				string text2 = CharacterMapTable[(int)c];
				if (text2 == "\t")
				{
					text2 = " ";
				}
				if (FontEncoding == "WinAnsiEncoding" && flag)
				{
					text2 = text2.ToUpper();
				}
				if (text2.Contains("\ufffd"))
				{
					int startIndex = text2.IndexOf("\ufffd");
					text2 = text2.Remove(startIndex, 1);
					if (FontName.Contains("ZapfDingbats"))
					{
						text2 = c.ToString();
					}
				}
				if (FontEncoding != "Identity-H" && !IsTextExtraction && CharacterMapTable.Count != ReverseMapTable.Count)
				{
					if (IsCancel(text2) || IsNonPrintableCharacter(c))
					{
						text2 = c.ToString();
					}
				}
				else if (!IsTextExtraction && CharacterMapTable.Count != ReverseMapTable.Count)
				{
					Convert.ToString((int)c);
				}
				text += text2;
				flag2 = false;
				if (renderedString != null && FontGlyphWidths != null && FontGlyphWidths.ContainsKey(c))
				{
					if (renderedString.IndexAndWidths == null)
					{
						renderedString.IndexAndWidths = new Dictionary<int, int>();
					}
					renderedString.IndexAndWidths[num] = FontGlyphWidths[c];
					num += text2.Length;
				}
			}
			else if (!CharacterMapTable.ContainsKey((int)c) && !flag2)
			{
				byte[] bytes = Encoding.BigEndianUnicode.GetBytes(c.ToString());
				if (bytes[0] == 92 && (bytes[0] != 92 || bytes[1] != 92))
				{
					continue;
				}
				if (CharacterMapTable.ContainsKey((int)bytes[0]))
				{
					text += CharacterMapTable[(int)bytes[0]];
					flag2 = false;
				}
				else if (fontType != null && fontType.Value == "TrueType")
				{
					if (char.IsDigit(c))
					{
						text += c;
						flag2 = false;
					}
					else if (char.IsUpper(c) && !char.IsWhiteSpace(c) && (decodedText.Length > 1 || m_mapstringFromRender))
					{
						text += c;
						flag2 = false;
					}
					else if (decodedText.Length == 1 && c != '\u0001' && c != '\u0092')
					{
						text += c;
					}
				}
			}
			else if (tempMapTable.ContainsKey((int)c) && !flag2)
			{
				string text3 = tempMapTable[(int)c];
				if (c == '\\' && IsTextExtraction)
				{
					text3 = "";
				}
				if (text3.Contains("\ufffd"))
				{
					int startIndex2 = text3.IndexOf("\ufffd");
					text3 = text3.Remove(startIndex2, 1);
				}
				text += text3;
				flag2 = false;
			}
			else if (flag2)
			{
				switch (c.ToString())
				{
				case "n":
					if (CharacterMapTable.ContainsKey(10.0))
					{
						text += CharacterMapTable[10.0];
					}
					break;
				case "r":
					if (CharacterMapTable.ContainsKey(13.0))
					{
						text += CharacterMapTable[13.0];
					}
					break;
				case "b":
					if (CharacterMapTable.ContainsKey(8.0))
					{
						text += CharacterMapTable[8.0];
					}
					break;
				case "a":
					if (CharacterMapTable.ContainsKey(7.0))
					{
						text += CharacterMapTable[7.0];
					}
					break;
				case "f":
					if (CharacterMapTable.ContainsKey(12.0))
					{
						text += CharacterMapTable[12.0];
					}
					break;
				case "t":
					if (CharacterMapTable.ContainsKey(9.0))
					{
						text += CharacterMapTable[9.0];
					}
					break;
				case "v":
					if (CharacterMapTable.ContainsKey(11.0))
					{
						text += CharacterMapTable[11.0];
					}
					break;
				case "'":
					if (CharacterMapTable.ContainsKey(39.0))
					{
						text += CharacterMapTable[39.0];
					}
					break;
				default:
					if (CharacterMapTable.ContainsKey((int)c))
					{
						text += CharacterMapTable[(int)c];
					}
					break;
				}
				flag2 = false;
			}
			else if (c == '\\')
			{
				flag2 = true;
			}
			else
			{
				text += c;
			}
		}
		return text;
	}

	internal string MapCidToGid(string decodedText)
	{
		string text = string.Empty;
		bool flag = false;
		for (int i = 0; i < decodedText.Length; i++)
		{
			char c = decodedText[i];
			if (m_cidToGidTable.ContainsKey((int)c) && !flag && m_cidToGidTable.Count != tempMapTable.Count)
			{
				string text2 = m_cidToGidTable[(int)c];
				if (text2.Contains("\ufffd"))
				{
					int startIndex = text2.IndexOf("\ufffd");
					text2 = text2.Remove(startIndex, 1);
				}
				if (text2.Length > 0 && !CidToGidReverseMapTable.ContainsKey(text2[0]))
				{
					CidToGidReverseMapTable.Add(text2[0], c);
				}
				text += text2;
				flag = false;
			}
			else if (tempMapTable.ContainsKey((int)c) && !flag)
			{
				string text3 = tempMapTable[(int)c];
				if (text3.Contains("\ufffd"))
				{
					int startIndex2 = text3.IndexOf("\ufffd");
					text3 = text3.Remove(startIndex2, 1);
				}
				text += text3;
				flag = false;
			}
			else if (flag)
			{
				switch (c.ToString())
				{
				case "n":
					if (m_cidToGidTable.ContainsKey(10.0))
					{
						text += CharacterMapTable[10.0];
					}
					break;
				case "r":
					if (m_cidToGidTable.ContainsKey(13.0))
					{
						text += CharacterMapTable[13.0];
					}
					break;
				case "b":
					if (m_cidToGidTable.ContainsKey(8.0))
					{
						text += CharacterMapTable[8.0];
					}
					break;
				case "a":
					if (m_cidToGidTable.ContainsKey(7.0))
					{
						text += CharacterMapTable[7.0];
					}
					break;
				case "f":
					if (m_cidToGidTable.ContainsKey(12.0))
					{
						text += CharacterMapTable[12.0];
					}
					break;
				case "t":
					if (m_cidToGidTable.ContainsKey(9.0))
					{
						text += CharacterMapTable[9.0];
					}
					break;
				case "v":
					if (m_cidToGidTable.ContainsKey(11.0))
					{
						text += CharacterMapTable[11.0];
					}
					break;
				case "'":
					if (m_cidToGidTable.ContainsKey(39.0))
					{
						text += CharacterMapTable[39.0];
					}
					break;
				default:
					if (m_cidToGidTable.ContainsKey((int)c))
					{
						text += CharacterMapTable[(int)c];
					}
					break;
				}
				flag = false;
			}
			else if (c == '\\')
			{
				flag = true;
			}
		}
		return text;
	}

	internal string MapDifferences(string encodedText)
	{
		string text = string.Empty;
		bool flag = false;
		if (IsTextExtraction)
		{
			try
			{
				encodedText = Regex.Unescape(encodedText);
			}
			catch (ArgumentException ex)
			{
				if (string.IsNullOrEmpty(encodedText))
				{
					throw ex;
				}
				encodedText = Regex.Unescape(Regex.Escape(encodedText));
			}
		}
		else
		{
			SkipEscapeSequence(encodedText);
		}
		string text2 = encodedText;
		for (int i = 0; i < text2.Length; i++)
		{
			char c = text2[i];
			Dictionary<string, string> differencesDictionary = DifferencesDictionary;
			int num = c;
			string text3;
			if (differencesDictionary.ContainsKey(num.ToString()))
			{
				Dictionary<string, string> differencesDictionary2 = DifferencesDictionary;
				num = c;
				text3 = differencesDictionary2[num.ToString()];
				if (text3.Length > 1 && fontType.Value != "Type3" && !IsTextExtraction)
				{
					text = ((!isDecodingConformance) ? (text + c) : (text + AdobeGlyphList.GetUnicode(text3)));
				}
				else if (!IsTextExtraction)
				{
					Dictionary<string, string> differencesDictionary3 = DifferencesDictionary;
					num = c;
					string text4 = differencesDictionary3[num.ToString()];
					if (text4.Length == 7 && text4.ToLowerInvariant().StartsWith("uni"))
					{
						text4 = DecodeToUnicode(text4);
					}
					text = ((!isDecodingConformance || text4.Length <= 1 || c == ' ') ? (text + text4) : (text + AdobeGlyphList.GetUnicode(text4)));
				}
				else
				{
					if (!char.IsLetter(c))
					{
						Dictionary<string, string> differencesDictionary4 = DifferencesDictionary;
						num = c;
						if (!differencesDictionary4.ContainsKey(num.ToString()))
						{
							text += AdobeGlyphList.GetUnicode(text3);
						}
					}
					if (!IsTextExtraction)
					{
						if (isDecodingConformance)
						{
							Dictionary<string, string> differencesDictionary5 = DifferencesDictionary;
							num = c;
							text += AdobeGlyphList.GetUnicode(differencesDictionary5[num.ToString()]);
						}
					}
					else if (!isDecodingConformance)
					{
						Dictionary<string, string> differencesDictionary6 = DifferencesDictionary;
						num = c;
						string text5 = differencesDictionary6[num.ToString()];
						if (!char.IsLetter(c))
						{
							Dictionary<string, string> differencesDictionary7 = DifferencesDictionary;
							num = c;
							if (!differencesDictionary7.ContainsKey(num.ToString()))
							{
								goto IL_024d;
							}
						}
						if (text5.Length == 7 && text5.ToLowerInvariant().StartsWith("uni"))
						{
							goto IL_024d;
						}
						if (FontEncoding != "Encoding" || BaseFontEncoding != "WinAnsiEncoding")
						{
							Dictionary<string, string> differencesDictionary8 = DifferencesDictionary;
							num = c;
							string text6 = differencesDictionary8[num.ToString()];
							text += text6;
						}
						else
						{
							string text7 = c.ToString();
							text += text7;
						}
					}
				}
				goto IL_02bf;
			}
			if (flag)
			{
				switch (c)
				{
				case 'n':
					if (DifferencesDictionary.ContainsKey(10.ToString()))
					{
						text += DifferencesDictionary[10.ToString()];
					}
					break;
				case 'r':
					if (DifferencesDictionary.ContainsKey(13.ToString()))
					{
						text += DifferencesDictionary[13.ToString()];
					}
					break;
				case ')':
					text += c;
					break;
				}
				flag = false;
			}
			else if (c == '\\')
			{
				flag = true;
			}
			else
			{
				text += c;
			}
			continue;
			IL_024d:
			text += AdobeGlyphList.GetUnicode(text3);
			goto IL_02bf;
			IL_02bf:
			Dictionary<string, int> reverseDictMapping = ReverseDictMapping;
			Dictionary<string, string> differencesDictionary9 = DifferencesDictionary;
			num = c;
			if (!reverseDictMapping.ContainsKey(differencesDictionary9[num.ToString()]))
			{
				Dictionary<string, int> reverseDictMapping2 = ReverseDictMapping;
				Dictionary<string, string> differencesDictionary10 = DifferencesDictionary;
				num = c;
				reverseDictMapping2.Add(differencesDictionary10[num.ToString()], c);
			}
			if (FontName == "Wingdings")
			{
				text = MapDifferenceOfWingDings(text);
			}
			string specialCharacter = PdfTextExtractor.GetSpecialCharacter(text);
			if (text != specialCharacter)
			{
				IsEmbedded = IsFontEmbedded();
			}
			if (text != specialCharacter && !IsEmbedded)
			{
				text = text.Replace(text, specialCharacter);
			}
			flag = false;
		}
		return text;
	}

	internal string DecodeToUnicode(string textDecoded)
	{
		return ((char)(new int[1] { int.Parse(textDecoded.Substring(3), NumberStyles.HexNumber) })[0]).ToString();
	}

	private string MapDifferenceOfWingDings(string decodedText)
	{
		if (decodedText.Length > 1 && decodedText.Contains("c") && decodedText.IndexOf("c") == 0)
		{
			decodedText = decodedText.Remove(0, 1);
			int result = 0;
			int.TryParse(decodedText, out result);
			decodedText = ((char)result).ToString();
		}
		return decodedText;
	}

	internal string SkipEscapeSequence(string text)
	{
		if (text.Contains("\\"))
		{
			int num = text.IndexOf('\\');
			if (num + 1 != text.Length)
			{
				string text2 = text.Substring(num + 1, 1);
				switch (text2)
				{
				case "a":
					text = text.Replace("\\a", "\a");
					break;
				case "b":
					text = text.Replace("\\b", "\b");
					break;
				case "e":
					text = text.Replace("\\e", "\\e");
					break;
				case "f":
					text = text.Replace("\\f", "\f");
					break;
				case "n":
					text = text.Replace("\\n", "\n");
					break;
				case "r":
					text = text.Replace("\\r", "\r");
					break;
				case "t":
					text = text.Replace("\\t", "\t");
					break;
				case "v":
					text = text.Replace("\\v", "\v");
					break;
				case "'":
					text = text.Replace("\\'", "'");
					break;
				default:
					m_skipEscapeSequence = true;
					if (text2.ToCharArray()[0] == '\u0003')
					{
						text = text.Replace("\\", "\\");
						break;
					}
					if (text2.ToCharArray()[0] >= '\u007f')
					{
						text = text.Replace("\\", "");
						break;
					}
					try
					{
						bool flag = false;
						if (!string.IsNullOrEmpty(text) && text.Contains("\\"))
						{
							for (int i = 0; i < text.Length - 1; i++)
							{
								string input = text[i + 1].ToString();
								if (text[i] == '\\' && text[i + 1] == '\\')
								{
									i++;
								}
								else if (text[i] == '\\' && Regex.IsMatch(input, "[A-Z|d|g|h|i|j|k|l|m|o|p|q|s|w|y|z]"))
								{
									flag = true;
									break;
								}
							}
						}
						text = ((!flag) ? Regex.Unescape(text) : Regex.Unescape(Regex.Escape(text)));
					}
					catch (ArgumentException ex)
					{
						if (!string.IsNullOrEmpty(text))
						{
							text = Regex.Unescape(Regex.Escape(text));
							break;
						}
						throw ex;
					}
					break;
				}
			}
			else if (num + 1 == text.Length && (text.Equals("\0\\") || text.Contains("\\")))
			{
				text = text.Replace("\\", "");
			}
		}
		return text;
	}

	private string EscapeSymbols(string text)
	{
		while (text.Contains("\n"))
		{
			text = text.Replace("\n", "");
		}
		return text;
	}

	internal static List<string> GetHexCode(string hexCode)
	{
		List<string> list = new List<string>();
		string text = hexCode;
		int num = 0;
		int num2 = 0;
		string text2 = null;
		int num3 = 0;
		while (num >= 0)
		{
			num = text.IndexOf('<');
			num2 = text.IndexOf('>');
			if (num >= 0 && num2 >= 0)
			{
				text2 = text.Substring(num + 1, num2 - 1 - num);
				list.Add(text2);
				text = text.Substring(num2 + 1, text.Length - 1 - num2);
			}
			num3++;
		}
		return list;
	}

	internal static string GetCharCode(string decodedCharacter)
	{
		char c = decodedCharacter.ToCharArray()[0];
		if (decodedCharacter != null)
		{
			switch (decodedCharacter.Length)
			{
			case 1:
				switch (decodedCharacter[0])
				{
				case '0':
					return "zero";
				case '1':
					return "one";
				case '2':
					return "two";
				case '3':
					return "three";
				case '4':
					return "four";
				case '5':
					return "five";
				case '6':
					return "six";
				case '7':
					return "seven";
				case '8':
					return "eight";
				case '9':
					return "nine";
				case '\u00b4':
					return "acute";
				case 'å':
					return "aring";
				case '^':
					return "asciicircum";
				case '~':
					return "asciitilde";
				case '*':
					return "asterisk";
				case '@':
					return "at";
				case 'ã':
					return "atilde";
				case '\\':
					return "backslash";
				case '|':
					return "bar";
				case '{':
					return "braceleft";
				case '}':
					return "braceright";
				case '[':
					return "bracketleft";
				case ']':
					return "bracketright";
				case '\u02d8':
					return "breve";
				case '·':
				case '•':
					return "bullet";
				case 'ˇ':
					return "caron";
				case 'ç':
					return "ccedilla";
				case '\u00b8':
					return "cedilla";
				case '¢':
					return "cent";
				case 'ˆ':
					return "circumflex";
				case ':':
					return "colon";
				case ',':
					return "comma";
				case '©':
					return "copyright";
				case '\uf6d9':
					return "copyrightserif";
				case '¤':
					return "currency1";
				case '†':
					return "dagger";
				case '‡':
					return "daggerdbl";
				case '°':
					return "degree";
				case '\u00a8':
					return "dieresis";
				case '÷':
					return "divide";
				case '$':
					return "dollar";
				case '\u02d9':
					return "dotaccent";
				case 'ı':
					return "dotlessi";
				case 'é':
					return "eacute";
				case 'ë':
					return "edieresis";
				case 'è':
					return "egrave";
				case '…':
					break;
				case '—':
					return "emdash";
				case '–':
					return "endash";
				case '=':
					return "equal";
				case 'ð':
					return "eth";
				case '€':
					return "Euro";
				case '!':
					return "exclam";
				case '¡':
					return "exclamdown";
				case 'ƒ':
					return "florin";
				case '⁄':
					return "fraction";
				case 'ß':
					return "germandbls";
				case '`':
					return "grave";
				case '>':
					return "greater";
				case '«':
					return "guillemotleft4";
				case '»':
					return "guillemotright";
				case '‹':
					return "guilsinglleft";
				case '›':
					return "guilsinglright";
				case '\u02dd':
					return "hungarumlaut";
				case 'í':
					return "iacute";
				case 'î':
					return "icircumflex";
				case 'ï':
					return "idieresis";
				case 'ì':
					return "igrave";
				case '<':
					return "less";
				case '¬':
					return "logicalnot";
				case 'ł':
					return "lslash";
				case 'Ł':
					return "Lslash";
				case '\u00af':
					return "macron";
				case '−':
					return "minus";
				case 'µ':
				case 'μ':
					return "mu";
				case '×':
					return "multiply";
				case 'ñ':
					return "ntilde";
				case '#':
					return "numbersign";
				case 'ó':
					return "oacute";
				case 'ô':
					return "ocircumflex";
				case 'ö':
					return "odieresis";
				case '\u02db':
					return "ogonek";
				case 'ò':
					return "ograve";
				case '½':
					goto IL_0811;
				case '¹':
					return "onesuperior";
				case 'ª':
					return "ordfeminine";
				case 'º':
					return "ordmasculine";
				case 'ø':
					return "oslash";
				case 'õ':
					return "otilde";
				case '¶':
					return "paragraph";
				case '(':
					return "parenleft";
				case ')':
					return "parenright";
				case '%':
					return "percent";
				case '.':
					return "period";
				case '‰':
					return "perthousand";
				case '+':
					return "plus";
				case '±':
					return "plusminus";
				case '?':
					return "question";
				case '¿':
					return "questiondown";
				case '"':
					return "quotedbl";
				case '„':
					return "quotedblbase";
				case '“':
					return "quotedblleft";
				case '”':
					return "quotedblright";
				case '‘':
					return "quoteleft";
				case '’':
					return "quoteright";
				case '‚':
					return "quotesinglbase";
				case '\'':
					return "quotesingle";
				case '®':
					return "registered";
				case '\u02da':
					return "ring";
				case 'š':
					return "scaron";
				case '§':
					return "section";
				case ';':
					return "semicolon";
				case '/':
					return "slash";
				case ' ':
					return "space";
				case '£':
					return "sterling";
				case '™':
					return "trademark";
				case 'ü':
					return "udieresis";
				case '-':
					return "hyphen";
				case '_':
					return "underscore";
				case 'ä':
					return "adieresis";
				case '&':
					return "ampersand";
				case 'Ä':
					return "Adieresis";
				case 'Ü':
					return "Udieresis";
				case 'č':
					return "ccaron";
				case 'Š':
					return "Scaron";
				case 'ž':
					return "zcaron";
				case 'à':
					return "agrave";
				case 'ê':
					return "ecircumflex";
				case 'ﬁ':
					return "fi";
				case 'ﬂ':
					return "fl";
				case 'á':
					return "aacute";
				case 'Á':
					return "Aacute";
				case 'â':
					return "acircumflex";
				case 'Â':
					return "Acircumflex";
				case 'Ã':
					return "Atilde";
				case 'æ':
					return "ae";
				case 'Ç':
					return "Ccedilla";
				case 'É':
					return "Eacute";
				case 'Í':
					return "Iacute";
				case 'Ó':
					return "Oacute";
				case 'Õ':
					return "Otilde";
				case 'ú':
					return "uacute";
				case 'Ú':
					return "Uacute";
				case '¥':
					return "yen";
				case 'ś':
					return "sacute";
				case 'Ś':
					return "Sacute";
				case 'ę':
					return "eogonek";
				case 'ć':
					return "cacute";
				case 'ą':
					return "aogonek";
				default:
					goto end_IL_0019;
				}
				goto IL_072d;
			case 3:
			{
				char c2 = decodedCharacter[2];
				if (c2 != '.')
				{
					if (c2 != '2')
					{
						if (c2 != '4' || !(decodedCharacter == "1/4"))
						{
							break;
						}
						return "onequarter";
					}
					if (!(decodedCharacter == "1/2"))
					{
						break;
					}
					goto IL_0811;
				}
				if (!(decodedCharacter == "..."))
				{
					break;
				}
				goto IL_072d;
			}
			case 2:
				{
					if (!(decodedCharacter == "oe"))
					{
						break;
					}
					return "oe";
				}
				IL_0811:
				return "onehalf";
				IL_072d:
				return "ellipsis";
				end_IL_0019:
				break;
			}
		}
		if (c == '\u0081')
		{
			decodedCharacter = "bullet";
		}
		if (c == '\u0085')
		{
			decodedCharacter = "ellipsis";
		}
		if (c == '\u0091')
		{
			decodedCharacter = "quoteleft";
		}
		if (c == '\u0092')
		{
			decodedCharacter = "quoteright";
		}
		if (c == '\u0093')
		{
			decodedCharacter = "quotedblleft";
		}
		if (c == '\u0094')
		{
			decodedCharacter = "quotedblright";
		}
		if (c == '\u0095')
		{
			decodedCharacter = "bullet";
		}
		if (c == '\u0096')
		{
			decodedCharacter = "endash";
		}
		if (c == '\u0097')
		{
			decodedCharacter = "emdash";
		}
		if (c == '\u0099')
		{
			decodedCharacter = "trademark";
		}
		if (c >= '\b' && c <= '\r')
		{
			decodedCharacter = "space";
		}
		return decodedCharacter;
	}

	private int CalculateCheckSum(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int i = 0;
		for (int num6 = bytes.Length / 4; i < num6; i++)
		{
			_ = 195;
			num5 += bytes[num++] & 0xFF;
			num4 += bytes[num++] & 0xFF;
			num3 += bytes[num++] & 0xFF;
			num2 += bytes[num++] & 0xFF;
		}
		return num2 + (num3 << 8) + (num4 << 16) + (num5 << 24);
	}

	internal bool IsCancel(string mappingString)
	{
		bool result = false;
		if (mappingString.Equals("\u0018"))
		{
			result = true;
		}
		return result;
	}

	private bool IsNonPrintableCharacter(char character)
	{
		bool result = false;
		if (!IsTextExtraction && fontType.Value == "Type1" && FontEncoding == "Encoding" && FontName != "ZapfDingbats" && CharacterMapTable.Count == DifferencesDictionary.Count && ((character >= '\0' && character <= '\u001f') || character == '\u007f'))
		{
			result = true;
		}
		return result;
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
		foreach (string text3 in array3)
		{
			text3.Trim();
			fontName = fontName + text3 + " ";
		}
		if (fontName.Contains("Times"))
		{
			fontName = "Times New Roman";
		}
		if (fontName == "Bookshelf Symbol Seven")
		{
			fontName = "Bookshelf Symbol 7";
		}
		if (fontName.Contains("Song Std"))
		{
			fontName = "Adobe Song Std L";
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

	internal string MapZapf(string encodedText)
	{
		string text = null;
		for (int i = 0; i < encodedText.Length; i++)
		{
			switch (((int)encodedText[i]).ToString("X"))
			{
			case "20":
				text += " ";
				ZapfPostScript += "space ";
				continue;
			case "21":
				text += "✁";
				ZapfPostScript += "a1 ";
				continue;
			case "22":
				text += "✂";
				ZapfPostScript += "a2 ";
				continue;
			case "23":
				text += "✃";
				ZapfPostScript += "a202 ";
				continue;
			case "24":
				text += "✄";
				ZapfPostScript += "a3 ";
				continue;
			case "25":
				text += "☎";
				ZapfPostScript += "a4 ";
				continue;
			case "26":
				text += "✆";
				ZapfPostScript += "a5 ";
				continue;
			case "27":
				text += "✇";
				ZapfPostScript += "a119 ";
				continue;
			case "28":
				text += "✈";
				ZapfPostScript += "a118 ";
				continue;
			case "29":
				text += "✉";
				ZapfPostScript += "a117 ";
				continue;
			case "2A":
				text += "☛";
				ZapfPostScript += "a11 ";
				continue;
			case "2B":
				text += "☞";
				ZapfPostScript += "a12 ";
				continue;
			case "2C":
				text += "✌";
				ZapfPostScript += "a13 ";
				continue;
			case "2D":
				text += "✍";
				ZapfPostScript += "a14 ";
				continue;
			case "2E":
				text += "✎";
				ZapfPostScript += "a15 ";
				continue;
			case "2F":
				text += "✏";
				ZapfPostScript += "a16 ";
				continue;
			case "30":
				text += "✐";
				ZapfPostScript += "a105 ";
				continue;
			case "31":
				text += "✑";
				ZapfPostScript += "a17 ";
				continue;
			case "32":
				text += "✒";
				ZapfPostScript += "a18 ";
				continue;
			case "33":
				text += "✓";
				ZapfPostScript += "a19 ";
				continue;
			case "34":
				text += "✔";
				ZapfPostScript += "a20 ";
				continue;
			case "35":
				text += "✕";
				ZapfPostScript += "a21 ";
				continue;
			case "36":
				text += "✖";
				ZapfPostScript += "a22 ";
				continue;
			case "37":
				text += "✗";
				ZapfPostScript += "a23 ";
				continue;
			case "38":
				text += "✘";
				ZapfPostScript += "a24 ";
				continue;
			case "39":
				text += "✙";
				ZapfPostScript += "a25 ";
				continue;
			case "3A":
				text += "✚";
				ZapfPostScript += "a26 ";
				continue;
			case "3B":
				text += "✛";
				ZapfPostScript += "a27 ";
				continue;
			case "3C":
				text += "✜";
				ZapfPostScript += "a28 ";
				continue;
			case "3D":
				text += "✝";
				ZapfPostScript += "a6 ";
				continue;
			case "3E":
				text += "✞";
				ZapfPostScript += "a7 ";
				continue;
			case "3F":
				text += "✟";
				ZapfPostScript += "a8 ";
				continue;
			case "40":
				text += "✠";
				ZapfPostScript += "a9 ";
				continue;
			case "41":
				text += "✡";
				ZapfPostScript += "a10 ";
				continue;
			case "42":
				text += "✢";
				ZapfPostScript += "a29 ";
				continue;
			case "43":
				text += "✣";
				ZapfPostScript += "a30 ";
				continue;
			case "44":
				text += "✤";
				ZapfPostScript += "a31 ";
				continue;
			case "45":
				text += "✥";
				ZapfPostScript += "a32 ";
				continue;
			case "46":
				text += "✦";
				ZapfPostScript += "a33 ";
				continue;
			case "47":
				text += "✧";
				ZapfPostScript += "a34 ";
				continue;
			case "48":
				text += "★";
				ZapfPostScript += "a35 ";
				continue;
			case "49":
				text += "✩";
				ZapfPostScript += "a36 ";
				continue;
			case "4A":
				text += "✪";
				ZapfPostScript += "a37 ";
				continue;
			case "4B":
				text += "✫";
				ZapfPostScript += "a38 ";
				continue;
			case "4C":
				text += "✬";
				ZapfPostScript += "a39 ";
				continue;
			case "4D":
				text += "✭";
				ZapfPostScript += "a40 ";
				continue;
			case "4E":
				text += "✮";
				ZapfPostScript += "a41 ";
				continue;
			case "4F":
				text += "✯";
				ZapfPostScript += "a42 ";
				continue;
			case "50":
				text += "✰";
				ZapfPostScript += "a43 ";
				continue;
			case "51":
				text += "✱";
				ZapfPostScript += "a44 ";
				continue;
			case "52":
				text += "✲";
				ZapfPostScript += "a45 ";
				continue;
			case "53":
				text += "✳";
				ZapfPostScript += "a46 ";
				continue;
			case "54":
				text += "✴";
				ZapfPostScript += "a47 ";
				continue;
			case "55":
				text += "✵";
				ZapfPostScript += "a48 ";
				continue;
			case "56":
				text += "✶";
				ZapfPostScript += "a49 ";
				continue;
			case "57":
				text += "✷";
				ZapfPostScript += "a50 ";
				continue;
			case "58":
				text += "✸";
				ZapfPostScript += "a51 ";
				continue;
			case "59":
				text += "✹";
				ZapfPostScript += "a52 ";
				continue;
			case "5A":
				text += "✺";
				ZapfPostScript += "a53 ";
				continue;
			case "5B":
				text += "✻";
				ZapfPostScript += "a54 ";
				continue;
			case "5C":
				text += "✼";
				ZapfPostScript += "a55 ";
				continue;
			case "5D":
				text += "✽";
				ZapfPostScript += "a56 ";
				continue;
			case "5E":
				text += "✾";
				ZapfPostScript += "a57 ";
				continue;
			case "5F":
				text += "✿";
				ZapfPostScript += "a58 ";
				continue;
			case "60":
				text += "❀";
				ZapfPostScript += "a59 ";
				continue;
			case "61":
				text += "❁";
				ZapfPostScript += "a60 ";
				continue;
			case "62":
				text += "❂";
				ZapfPostScript += "a61 ";
				continue;
			case "63":
				text += "❃";
				ZapfPostScript += "a62 ";
				continue;
			case "64":
				text += "❄";
				ZapfPostScript += "a63 ";
				continue;
			case "65":
				text += "❅";
				ZapfPostScript += "a64 ";
				continue;
			case "66":
				text += "❆";
				ZapfPostScript += "a65 ";
				continue;
			case "67":
				text += "❇";
				ZapfPostScript += "a66 ";
				continue;
			case "68":
				text += "❈";
				ZapfPostScript += "a67 ";
				continue;
			case "69":
				text += "❉";
				ZapfPostScript += "a68 ";
				continue;
			case "6A":
				text += "❊";
				ZapfPostScript += "a69 ";
				continue;
			case "6B":
				text += "❋";
				ZapfPostScript += "a70 ";
				continue;
			case "6C":
				text += "●";
				ZapfPostScript += "a71 ";
				continue;
			case "6D":
				text += "╍";
				ZapfPostScript += "a72 ";
				continue;
			case "6E":
				text += "■";
				ZapfPostScript += "a73 ";
				continue;
			case "6F":
				text += "❏";
				ZapfPostScript += "a74 ";
				continue;
			case "70":
				text += "❐";
				ZapfPostScript += "a203 ";
				continue;
			case "71":
				text += "❑";
				ZapfPostScript += "a75 ";
				continue;
			case "72":
				text += "❒";
				ZapfPostScript += "a204 ";
				continue;
			case "73":
				text += "▲";
				ZapfPostScript += "a76 ";
				continue;
			case "74":
				text += "▼";
				ZapfPostScript += "a77 ";
				continue;
			case "75":
				text += "⟆";
				ZapfPostScript += "a78 ";
				continue;
			case "76":
				text += "❖";
				ZapfPostScript += "a79 ";
				continue;
			case "77":
				text += "◗";
				ZapfPostScript += "a81 ";
				continue;
			case "78":
				text += "❘";
				ZapfPostScript += "a82 ";
				continue;
			case "79":
				text += "❙";
				ZapfPostScript += "a83 ";
				continue;
			case "7A":
				text += "❚";
				ZapfPostScript += "a84 ";
				continue;
			case "7B":
				text += "❛";
				ZapfPostScript += "a97 ";
				continue;
			case "7C":
				text += "❜";
				ZapfPostScript += "a98 ";
				continue;
			case "7D":
				text += "❝";
				ZapfPostScript += "a99 ";
				continue;
			case "7E":
				text += "❞";
				ZapfPostScript += "a100 ";
				continue;
			case "80":
				text += "\uf8d7";
				ZapfPostScript += "a89 ";
				continue;
			case "81":
				text += "\uf8d8";
				ZapfPostScript += "a90 ";
				continue;
			case "82":
				text += "\uf8d9";
				ZapfPostScript += "a93 ";
				continue;
			case "83":
				text += "\uf8da";
				ZapfPostScript += "a94 ";
				continue;
			case "84":
				text += "\uf8db";
				ZapfPostScript += "a91 ";
				continue;
			case "85":
				text += "\uf8dc";
				ZapfPostScript += "a92 ";
				continue;
			case "86":
				text += "\uf8dd";
				ZapfPostScript += "a205 ";
				continue;
			case "87":
				text += "\uf8de";
				ZapfPostScript += "a85 ";
				continue;
			case "88":
				text += "\uf8df";
				ZapfPostScript += "a206 ";
				continue;
			case "89":
				text += "\uf8e0";
				ZapfPostScript += "a86 ";
				continue;
			case "8A":
				text += "\uf8e1";
				ZapfPostScript += "a87 ";
				continue;
			case "8B":
				text += "\uf8e2";
				ZapfPostScript += "a88 ";
				continue;
			case "8C":
				text += "\uf8e3";
				ZapfPostScript += "a95 ";
				continue;
			case "8D":
				text += "\uf8e4";
				ZapfPostScript += "a96 ";
				continue;
			case "A1":
				text += "❡";
				ZapfPostScript += "a101 ";
				continue;
			case "A2":
				text += "❢";
				ZapfPostScript += "a102 ";
				continue;
			case "A3":
				text += "❣";
				ZapfPostScript += "a103 ";
				continue;
			case "A4":
				text += "❤";
				ZapfPostScript += "a104 ";
				continue;
			case "A5":
				text += "❥";
				ZapfPostScript += "a106 ";
				continue;
			case "A6":
				text += "❦";
				ZapfPostScript += "a107 ";
				continue;
			case "A7":
				text += "❧";
				ZapfPostScript += "a108 ";
				continue;
			case "A8":
				text += "♣";
				ZapfPostScript += "a112 ";
				continue;
			case "A9":
				text += "♦";
				ZapfPostScript += "a111 ";
				continue;
			case "AA":
				text += "♥";
				ZapfPostScript += "a110 ";
				continue;
			case "AB":
				text += "♠";
				ZapfPostScript += "a109 ";
				continue;
			case "AC":
				text += "①";
				ZapfPostScript += "a120 ";
				continue;
			case "AD":
				text += "②";
				ZapfPostScript += "a121 ";
				continue;
			case "AE":
				text += "③";
				ZapfPostScript += "a122 ";
				continue;
			case "AF":
				text += "④";
				ZapfPostScript += "a123 ";
				continue;
			case "B0":
				text += "⑤";
				ZapfPostScript += "a124 ";
				continue;
			case "B1":
				text += "⑥";
				ZapfPostScript += "a125 ";
				continue;
			case "B2":
				text += "⑦";
				ZapfPostScript += "a126 ";
				continue;
			case "B3":
				text += "⑧";
				ZapfPostScript += "a127 ";
				continue;
			case "B4":
				text += "⑨";
				ZapfPostScript += "a128 ";
				continue;
			case "B5":
				text += "⑩";
				ZapfPostScript += "a129 ";
				continue;
			case "B6":
				text += "❶";
				ZapfPostScript += "a130 ";
				continue;
			case "B7":
				text += "❷";
				ZapfPostScript += "a131 ";
				continue;
			case "B8":
				text += "❸";
				ZapfPostScript += "a132 ";
				continue;
			case "B9":
				text += "❹";
				ZapfPostScript += "a133 ";
				continue;
			case "BA":
				text += "❺";
				ZapfPostScript += "a134 ";
				continue;
			case "BB":
				text += "❻";
				ZapfPostScript += "a135 ";
				continue;
			case "BC":
				text += "❼";
				ZapfPostScript += "a136 ";
				continue;
			case "BD":
				text += "❽";
				ZapfPostScript += "a137 ";
				continue;
			case "BE":
				text += "❾";
				ZapfPostScript += "a138 ";
				continue;
			case "BF":
				text += "❿";
				ZapfPostScript += "a139 ";
				continue;
			case "C0":
				text += "➀";
				ZapfPostScript += "a140 ";
				continue;
			case "C1":
				text += "➁";
				ZapfPostScript += "a141 ";
				continue;
			case "C2":
				text += "➂";
				ZapfPostScript += "a142 ";
				continue;
			case "C3":
				text += "➃";
				ZapfPostScript += "a143 ";
				continue;
			case "C4":
				text += "➄";
				ZapfPostScript += "a144 ";
				continue;
			case "C5":
				text += "➅";
				ZapfPostScript += "a145 ";
				continue;
			case "C6":
				text += "➆";
				ZapfPostScript += "a146 ";
				continue;
			case "C7":
				text += "➇";
				ZapfPostScript += "a147 ";
				continue;
			case "C8":
				text += "➈";
				ZapfPostScript += "a148 ";
				continue;
			case "C9":
				text += "➉";
				ZapfPostScript += "a149 ";
				continue;
			case "CA":
				text += "➊";
				ZapfPostScript += "150 ";
				continue;
			case "CB":
				text += "➋";
				ZapfPostScript += "a151 ";
				continue;
			case "CC":
				text += "➌";
				ZapfPostScript += "a152 ";
				continue;
			case "CD":
				text += "➍";
				ZapfPostScript += "a153 ";
				continue;
			case "CE":
				text += "➎";
				ZapfPostScript += "a154 ";
				continue;
			case "CF":
				text += "➏";
				ZapfPostScript += "a155 ";
				continue;
			case "D0":
				text += "➐";
				ZapfPostScript += "a156 ";
				continue;
			case "D1":
				text += "➑";
				ZapfPostScript += "a157 ";
				continue;
			case "D2":
				text += "➒";
				ZapfPostScript += "a158 ";
				continue;
			case "D3":
				text += "➓";
				ZapfPostScript += "a159 ";
				continue;
			case "D4":
				text += "➔";
				ZapfPostScript += "a160 ";
				continue;
			case "D5":
				text += "→";
				ZapfPostScript += "a161 ";
				continue;
			case "D6":
				text += "↔";
				ZapfPostScript += "a163 ";
				continue;
			case "D7":
				text += "↕";
				ZapfPostScript += "a164 ";
				continue;
			case "D8":
				text += "➘";
				ZapfPostScript += "a196 ";
				continue;
			case "D9":
				text += "➙";
				ZapfPostScript += "a165 ";
				continue;
			case "DA":
				text += "➚";
				ZapfPostScript += "a192 ";
				continue;
			case "DB":
				text += "➛";
				ZapfPostScript += "a166 ";
				continue;
			case "DC":
				text += "➜";
				ZapfPostScript += "a167 ";
				continue;
			case "DD":
				text += "➝";
				ZapfPostScript += "a168 ";
				continue;
			case "DE":
				text += "➞";
				ZapfPostScript += "a169 ";
				continue;
			case "DF":
				text += "➟";
				ZapfPostScript += "a170 ";
				continue;
			case "E0":
				text += "➠";
				ZapfPostScript += "a171 ";
				continue;
			case "E1":
				text += "➡";
				ZapfPostScript += "a172 ";
				continue;
			case "E2":
				text += "➢";
				ZapfPostScript += "a173 ";
				continue;
			case "E3":
				text += "➣";
				ZapfPostScript += "a162 ";
				continue;
			case "E4":
				text += "➤";
				ZapfPostScript += "a174 ";
				continue;
			case "E5":
				text += "➥";
				ZapfPostScript += "a175 ";
				continue;
			case "E6":
				text += "➦";
				ZapfPostScript += "a176 ";
				continue;
			case "E7":
				text += "➧";
				ZapfPostScript += "a177 ";
				continue;
			case "E8":
				text += "➨";
				ZapfPostScript += "a178 ";
				continue;
			case "E9":
				text += "➩";
				ZapfPostScript += "a179 ";
				continue;
			case "EA":
				text += "➪";
				ZapfPostScript += "a193 ";
				continue;
			case "EB":
				text += "➫";
				ZapfPostScript += "a180 ";
				continue;
			case "EC":
				text += "➬";
				ZapfPostScript += "a199 ";
				continue;
			case "ED":
				text += "➭";
				ZapfPostScript += "a181 ";
				continue;
			case "EE":
				text += "➮";
				ZapfPostScript += "a200 ";
				continue;
			case "EF":
				text += "➯";
				ZapfPostScript += "a182 ";
				continue;
			case "F1":
				text += "➱";
				ZapfPostScript += "a201 ";
				continue;
			case "F2":
				text += "➲";
				ZapfPostScript += "a183 ";
				continue;
			case "F3":
				text += "➳";
				ZapfPostScript += "a184 ";
				continue;
			case "F4":
				text += "➴";
				ZapfPostScript += "a197 ";
				continue;
			case "F5":
				text += "➵";
				ZapfPostScript += "a185 ";
				continue;
			case "F6":
				text += "➶";
				ZapfPostScript += "a194 ";
				continue;
			case "F7":
				text += "➷";
				ZapfPostScript += "a198 ";
				continue;
			case "F8":
				text += "➸";
				ZapfPostScript += "a186 ";
				continue;
			case "F9":
				text += "➹";
				ZapfPostScript += "a195 ";
				continue;
			case "FA":
				text += "➺";
				ZapfPostScript += "a187 ";
				continue;
			case "FB":
				text += "➻";
				ZapfPostScript += "a188 ";
				continue;
			case "FC":
				text += "➼";
				ZapfPostScript += "a189 ";
				continue;
			case "FD":
				text += "➽";
				ZapfPostScript += "a190 ";
				continue;
			case "FE":
				text += "➾";
				ZapfPostScript += "a191 ";
				continue;
			}
			if (differenceTable != null && differenceTable.Count > 0 && ReverseMapTable.ContainsKey(encodedText))
			{
				text = encodedText;
				int key = (int)ReverseMapTable[text];
				ZapfPostScript = differenceTable[key];
			}
			else
			{
				text = "✈";
				ZapfPostScript = "a118";
			}
		}
		return text;
	}

	internal Dictionary<int, string> GetUnicodeCharMapTable()
	{
		unicodeCharMapTable = new Dictionary<int, string>();
		return unicodeCharMapTable;
	}

	internal bool IsCIDFontType()
	{
		bool result = false;
		if (m_fontDictionary.Items.ContainsKey(new PdfName("DescendantFonts")))
		{
			PdfReferenceHolder pdfReferenceHolder = m_fontDictionary.Items[new PdfName("DescendantFonts")] as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				PdfArray pdfArray = pdfReferenceHolder.Object as PdfArray;
				if (pdfArray[0] as PdfReferenceHolder != null)
				{
					PdfName pdfName = ((pdfArray[0] as PdfReferenceHolder).Object as PdfDictionary)["Subtype"] as PdfName;
					pdfArray = null;
					if (pdfName.Value == "CIDFontType2" || pdfName.Value == "CIDFontType0")
					{
						result = true;
					}
				}
			}
			if (pdfReferenceHolder == null && m_fontDictionary.Items[new PdfName("DescendantFonts")] is PdfArray)
			{
				PdfArray pdfArray2 = m_fontDictionary.Items[new PdfName("DescendantFonts")] as PdfArray;
				if (pdfArray2[0] as PdfReferenceHolder != null)
				{
					PdfName pdfName2 = ((pdfArray2[0] as PdfReferenceHolder).Object as PdfDictionary)["Subtype"] as PdfName;
					pdfArray2 = null;
					if (pdfName2.Value == "CIDFontType2" || pdfName2.Value == "CIDFontType0")
					{
						result = true;
					}
				}
				else if (pdfArray2[0] is PdfDictionary)
				{
					PdfName pdfName3 = (pdfArray2[0] as PdfDictionary)["Subtype"] as PdfName;
					pdfArray2 = null;
					if (pdfName3.Value == "CIDFontType2" || pdfName3.Value == "CIDFontType0")
					{
						result = true;
					}
				}
			}
			pdfReferenceHolder = null;
		}
		return result;
	}

	private bool IsEmbbedFont()
	{
		bool result = false;
		if (FontDictionary != null)
		{
			PdfDictionary pdfDictionary = FontDictionary;
			if (pdfDictionary.ContainsKey("DescendantFonts"))
			{
				if (PdfCrossTable.Dereference(pdfDictionary["DescendantFonts"]) is PdfArray { Count: >0 } pdfArray)
				{
					pdfDictionary = PdfCrossTable.Dereference(pdfArray[0]) as PdfDictionary;
					if (pdfDictionary.ContainsKey("FontDescriptor"))
					{
						pdfDictionary = PdfCrossTable.Dereference(pdfDictionary["FontDescriptor"]) as PdfDictionary;
					}
				}
				if (pdfDictionary != null && (pdfDictionary.ContainsKey("FontFile") || pdfDictionary.ContainsKey("FontFile2") || pdfDictionary.ContainsKey("FontFile3")))
				{
					result = true;
				}
			}
		}
		return result;
	}

	private string CheckContainInvalidChar(string charvalue)
	{
		char[] array = charvalue.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			switch (array[i])
			{
			case '\u00a0':
				charvalue = " ";
				break;
			case '\uf076':
				charvalue = "";
				break;
			}
		}
		return charvalue;
	}

	public static bool GetBit(int n, byte bit)
	{
		return (n & (1 << (int)bit)) != 0;
	}

	private bool GetFlag(byte bit)
	{
		bit--;
		return GetBit(Flags.IntValue, bit);
	}

	private string[] getMapDifference()
	{
		if (DifferencesDictionary != null && DifferencesDictionary.Count > 0)
		{
			m_differenceDictionaryValues = new string[256];
			int result = 0;
			List<string> list = new List<string>(DifferencesDictionary.Keys);
			List<string> list2 = new List<string>(DifferencesDictionary.Values);
			for (int i = 0; i < list.Count; i++)
			{
				int.TryParse(list[i], out result);
				if (result < 256)
				{
					m_differenceDictionaryValues[result] = list2[i];
				}
			}
		}
		return m_differenceDictionaryValues;
	}

	private PdfNumber GetFlagValue()
	{
		if (FontEncoding != "Identity-H")
		{
			if (m_fontDictionary.Items.ContainsKey(new PdfName("FontDescriptor")))
			{
				PdfReferenceHolder pdfReferenceHolder = m_fontDictionary.Items[new PdfName("FontDescriptor")] as PdfReferenceHolder;
				if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary)
				{
					PdfDictionary pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
					pdfReferenceHolder = null;
					if (pdfDictionary != null && pdfDictionary.Items.ContainsKey(new PdfName("Flags")))
					{
						PdfNumber result = pdfDictionary.Items[new PdfName("Flags")] as PdfNumber;
						pdfDictionary = null;
						return result;
					}
				}
			}
		}
		else if (m_fontDictionary.Items.ContainsKey(new PdfName("DescendantFonts")))
		{
			if (m_fontDictionary.Items[new PdfName("DescendantFonts")] is PdfArray)
			{
				if (m_fontDictionary.Items[new PdfName("DescendantFonts")] is PdfArray pdfArray && pdfArray[0] is PdfReferenceHolder)
				{
					PdfReferenceHolder pdfReferenceHolder2 = pdfArray[0] as PdfReferenceHolder;
					PdfArray pdfArray2 = null;
					if (pdfReferenceHolder2 != null && pdfReferenceHolder2.Object is PdfDictionary)
					{
						PdfDictionary pdfDictionary2 = pdfReferenceHolder2.Object as PdfDictionary;
						pdfReferenceHolder2 = null;
						if (pdfDictionary2 != null && pdfDictionary2.Items.ContainsKey(new PdfName("FontDescriptor")))
						{
							if (pdfDictionary2.Items[new PdfName("FontDescriptor")] is PdfDictionary)
							{
								PdfDictionary pdfDictionary3 = pdfDictionary2.Items[new PdfName("FontDescriptor")] as PdfDictionary;
							}
							if (pdfDictionary2.Items[new PdfName("FontDescriptor")] is PdfReferenceHolder)
							{
								PdfDictionary pdfDictionary3 = (pdfDictionary2.Items[new PdfName("FontDescriptor")] as PdfReferenceHolder).Object as PdfDictionary;
								pdfDictionary2 = null;
								if (pdfDictionary3 != null && pdfDictionary3.Items.ContainsKey(new PdfName("Flags")))
								{
									PdfNumber result2 = pdfDictionary3.Items[new PdfName("Flags")] as PdfNumber;
									pdfDictionary3 = null;
									return result2;
								}
							}
						}
					}
				}
			}
			else
			{
				PdfReferenceHolder pdfReferenceHolder3 = m_fontDictionary.Items[new PdfName("DescendantFonts")] as PdfReferenceHolder;
				if (pdfReferenceHolder3 != null)
				{
					PdfArray pdfArray3 = pdfReferenceHolder3.Object as PdfArray;
					pdfReferenceHolder3 = null;
					if (pdfArray3[0] as PdfReferenceHolder != null)
					{
						PdfDictionary obj = (pdfArray3[0] as PdfReferenceHolder).Object as PdfDictionary;
						pdfArray3 = null;
						PdfName pdfName = obj["Subtype"] as PdfName;
						if (!(pdfName.Value == "CIDFontType2"))
						{
							_ = pdfName.Value == "CIDFontType0";
						}
					}
				}
			}
		}
		return null;
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

	private void GetCIDByte2Unicode()
	{
		int num = 256;
		m_cidByte2Unicode = new Dictionary<int, char>();
		for (int i = 0; i < num; i++)
		{
			char value = (char)m_charCodeTable[i];
			m_cidByte2Unicode[i] = value;
		}
	}

	internal void Dispose()
	{
		m_differenceDictionaryValues = null;
		fontfilebytess = null;
		if (m_characterMapTable != null && m_characterMapTable.Count > 0)
		{
			m_characterMapTable.Clear();
			m_characterMapTable = null;
		}
		if (ReverseDictMapping != null && ReverseDictMapping.Count > 0)
		{
			ReverseDictMapping.Clear();
			ReverseDictMapping = null;
		}
		if (m_reverseMapTable != null && m_reverseMapTable.Count > 0)
		{
			m_reverseMapTable.Clear();
			m_reverseMapTable = null;
		}
		if (m_cidToGidTable != null && m_cidToGidTable.Count > 0)
		{
			m_cidToGidTable.Clear();
			m_cidToGidTable = null;
		}
		if (m_differencesDictionary != null && m_differencesDictionary.Count > 0)
		{
			m_differencesDictionary.Clear();
			m_differencesDictionary = null;
		}
		if (m_fontGlyphWidth != null && m_fontGlyphWidth.Count > 0)
		{
			m_fontGlyphWidth.Clear();
			m_fontGlyphWidth = null;
		}
		if (tempMapTable != null && tempMapTable.Count > 0)
		{
			tempMapTable.Clear();
			tempMapTable = null;
		}
		if (m_octDecMapTable != null && m_octDecMapTable.Count > 0)
		{
			m_octDecMapTable.Clear();
			m_octDecMapTable = null;
		}
		if (m_cidToGidReverseMapTable != null && m_cidToGidReverseMapTable.Count > 0)
		{
			m_cidToGidReverseMapTable.Clear();
			m_cidToGidReverseMapTable = null;
		}
		if (m_fontGlyphWidthMapping != null && m_fontGlyphWidthMapping.Count > 0)
		{
			m_fontGlyphWidthMapping.Clear();
			m_fontGlyphWidthMapping = null;
		}
		if (m_type1FontGlyphs != null && m_type1FontGlyphs.Count > 0)
		{
			m_type1FontGlyphs.Clear();
			m_type1FontGlyphs = null;
		}
		if (differenceTable != null && differenceTable.Count > 0)
		{
			differenceTable.Clear();
			differenceTable = null;
		}
		if (differenceEncoding != null && differenceEncoding.Count > 0)
		{
			differenceEncoding.Clear();
			differenceEncoding = null;
		}
		if (Type3FontCharProcsDict != null && Type3FontCharProcsDict.Count > 0)
		{
			Type3FontCharProcsDict.Clear();
			Type3FontCharProcsDict = null;
		}
		if (m_adobeJapanCidMapTable != null && m_adobeJapanCidMapTable.Count > 0)
		{
			m_adobeJapanCidMapTable.Clear();
			m_adobeJapanCidMapTable = null;
		}
		if (tempStringList != null && tempStringList.Count > 0)
		{
			tempStringList.Clear();
			tempStringList = null;
		}
		if (unicodeCharMapTable != null && unicodeCharMapTable.Count > 0)
		{
			unicodeCharMapTable.Clear();
			unicodeCharMapTable = null;
		}
		if (m_macEncodeTable != null && m_macEncodeTable.Count > 0)
		{
			m_macEncodeTable.Clear();
			m_macEncodeTable = null;
		}
	}
}
