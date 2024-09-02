using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Pdf;

internal class FontFile3
{
	private int glyphCount;

	private char[] nybChars = new char[15]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'.', 'e', 'e', ' ', '-'
	};

	private int ROS = -1;

	private int CIDFontVersion;

	private int CIDFontRevision;

	private int CIDFontType;

	private int CIDcount;

	private int UIDBase = -1;

	private int FDArray = -1;

	private int FDSelect = -1;

	private int top;

	private string copyright;

	private string embeddedFontName;

	private int charset;

	public double[] FontMatrix = new double[6] { 0.001, 0.0, 0.0, 0.001, 0.0, 0.0 };

	private int encodeValue;

	internal bool hasFontMatrix;

	private int italicAngle;

	private int charstrings;

	internal bool isCID;

	private int stringIndex;

	private bool trackIndices;

	private int stringStart;

	private Dictionary<string, byte[]> glyphs = new Dictionary<string, byte[]>();

	private int stringOffsetSize;

	public int GlobalBias;

	internal double[] m_fontMatrix = new double[6];

	internal CffGlyphs m_cffGlyphs = new CffGlyphs();

	private int privateDict = -1;

	private int privateDictOffset = -1;

	private int defaultWidthX;

	private int nominalWidthX;

	public float[] FontBBox = new float[4] { 0f, 0f, 1000f, 1000f };

	public string[] m_diffTable;

	private int maxCharCount = 256;

	private static int[] ExpertSubCharset = new int[87]
	{
		0, 1, 231, 232, 235, 236, 237, 238, 13, 14,
		15, 99, 239, 240, 241, 242, 243, 244, 245, 246,
		247, 248, 27, 28, 249, 250, 251, 253, 254, 255,
		256, 257, 258, 259, 260, 261, 262, 263, 264, 265,
		266, 109, 110, 267, 268, 269, 270, 272, 300, 301,
		302, 305, 314, 315, 158, 155, 163, 320, 321, 322,
		323, 324, 325, 326, 150, 164, 169, 327, 328, 329,
		330, 331, 332, 333, 334, 335, 336, 337, 338, 339,
		340, 341, 342, 343, 344, 345, 346
	};

	private static string[] type1CStdStrings = new string[391]
	{
		".notdef", "space", "exclam", "quotedbl", "numbersign", "dollar", "percent", "ampersand", "quoteright", "parenleft",
		"parenright", "asterisk", "plus", "comma", "hyphen", "period", "slash", "zero", "one", "two",
		"three", "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon", "less",
		"equal", "greater", "question", "at", "A", "B", "C", "D", "E", "F",
		"G", "H", "I", "J", "K", "L", "M", "N", "O", "P",
		"Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
		"bracketleft", "backslash", "bracketright", "asciicircum", "underscore", "quoteleft", "a", "b", "c", "d",
		"e", "f", "g", "h", "i", "j", "k", "l", "m", "n",
		"o", "p", "q", "r", "s", "t", "u", "v", "w", "x",
		"y", "z", "braceleft", "bar", "braceright", "asciitilde", "exclamdown", "cent", "sterling", "fraction",
		"yen", "florin", "section", "currency", "quotesingle", "quotedblleft", "guillemotleft", "guilsinglleft", "guilsinglright", "fi",
		"fl", "endash", "dagger", "daggerdbl", "periodcentered", "paragraph", "bullet", "quotesinglbase", "quotedblbase", "quotedblright",
		"guillemotright", "ellipsis", "perthousand", "questiondown", "grave", "acute", "circumflex", "tilde", "macron", "breve",
		"dotaccent", "dieresis", "ring", "cedilla", "hungarumlaut", "ogonek", "caron", "emdash", "AE", "ordfeminine",
		"Lslash", "Oslash", "OE", "ordmasculine", "ae", "dotlessi", "lslash", "oslash", "oe", "germandbls",
		"onesuperior", "logicalnot", "mu", "trademark", "Eth", "onehalf", "plusminus", "Thorn", "onequarter", "divide",
		"brokenbar", "degree", "thorn", "threequarters", "twosuperior", "registered", "minus", "eth", "multiply", "threesuperior",
		"copyright", "Aacute", "Acircumflex", "Adieresis", "Agrave", "Aring", "Atilde", "Ccedilla", "Eacute", "Ecircumflex",
		"Edieresis", "Egrave", "Iacute", "Icircumflex", "Idieresis", "Igrave", "Ntilde", "Oacute", "Ocircumflex", "Odieresis",
		"Ograve", "Otilde", "Scaron", "Uacute", "Ucircumflex", "Udieresis", "Ugrave", "Yacute", "Ydieresis", "Zcaron",
		"aacute", "acircumflex", "adieresis", "agrave", "aring", "atilde", "ccedilla", "eacute", "ecircumflex", "edieresis",
		"egrave", "iacute", "icircumflex", "idieresis", "igrave", "ntilde", "oacute", "ocircumflex", "odieresis", "ograve",
		"otilde", "scaron", "uacute", "ucircumflex", "udieresis", "ugrave", "yacute", "ydieresis", "zcaron", "exclamsmall",
		"Hungarumlautsmall", "dollaroldstyle", "dollarsuperior", "ampersandsmall", "Acutesmall", "parenleftsuperior", "parenrightsuperior", "twodotenleader", "onedotenleader", "zerooldstyle",
		"oneoldstyle", "twooldstyle", "threeoldstyle", "fouroldstyle", "fiveoldstyle", "sixoldstyle", "sevenoldstyle", "eightoldstyle", "nineoldstyle", "commasuperior",
		"threequartersemdash", "periodsuperior", "questionsmall", "asuperior", "bsuperior", "centsuperior", "dsuperior", "esuperior", "isuperior", "lsuperior",
		"msuperior", "nsuperior", "osuperior", "rsuperior", "ssuperior", "tsuperior", "ff", "ffi", "ffl", "parenleftinferior",
		"parenrightinferior", "Circumflexsmall", "hyphensuperior", "Gravesmall", "Asmall", "Bsmall", "Csmall", "Dsmall", "Esmall", "Fsmall",
		"Gsmall", "Hsmall", "Ismall", "Jsmall", "Ksmall", "Lsmall", "Msmall", "Nsmall", "Osmall", "Psmall",
		"Qsmall", "Rsmall", "Ssmall", "Tsmall", "Usmall", "Vsmall", "Wsmall", "Xsmall", "Ysmall", "Zsmall",
		"colonmonetary", "onefitted", "rupiah", "Tildesmall", "exclamdownsmall", "centoldstyle", "Lslashsmall", "Scaronsmall", "Zcaronsmall", "Dieresissmall",
		"Brevesmall", "Caronsmall", "Dotaccentsmall", "Macronsmall", "figuredash", "hypheninferior", "Ogoneksmall", "Ringsmall", "Cedillasmall", "questiondownsmall",
		"oneeighth", "threeeighths", "fiveeighths", "seveneighths", "onethird", "twothirds", "zerosuperior", "foursuperior", "fivesuperior", "sixsuperior",
		"sevensuperior", "eightsuperior", "ninesuperior", "zeroinferior", "oneinferior", "twoinferior", "threeinferior", "fourinferior", "fiveinferior", "sixinferior",
		"seveninferior", "eightinferior", "nineinferior", "centinferior", "dollarinferior", "periodinferior", "commainferior", "Agravesmall", "Aacutesmall", "Acircumflexsmall",
		"Atildesmall", "Adieresissmall", "Aringsmall", "AEsmall", "Ccedillasmall", "Egravesmall", "Eacutesmall", "Ecircumflexsmall", "Edieresissmall", "Igravesmall",
		"Iacutesmall", "Icircumflexsmall", "Idieresissmall", "Ethsmall", "Ntildesmall", "Ogravesmall", "Oacutesmall", "Ocircumflexsmall", "Otildesmall", "Odieresissmall",
		"OEsmall", "Oslashsmall", "Ugravesmall", "Uacutesmall", "Ucircumflexsmall", "Udieresissmall", "Yacutesmall", "Thornsmall", "Ydieresissmall", "001.000",
		"001.001", "001.002", "001.003", "Black", "Bold", "Book", "Light", "Medium", "Regular", "Roman",
		"Semibold"
	};

	private static int[] ISOAdobeCharset = new int[229]
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
		120, 121, 122, 123, 124, 125, 126, 127, 128, 129,
		130, 131, 132, 133, 134, 135, 136, 137, 138, 139,
		140, 141, 142, 143, 144, 145, 146, 147, 148, 149,
		150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
		160, 161, 162, 163, 164, 165, 166, 167, 168, 169,
		170, 171, 172, 173, 174, 175, 176, 177, 178, 179,
		180, 181, 182, 183, 184, 185, 186, 187, 188, 189,
		190, 191, 192, 193, 194, 195, 196, 197, 198, 199,
		200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
		210, 211, 212, 213, 214, 215, 216, 217, 218, 219,
		220, 221, 222, 223, 224, 225, 226, 227, 228
	};

	private static int[] ExpertCharset = new int[166]
	{
		0, 1, 229, 230, 231, 232, 233, 234, 235, 236,
		237, 238, 13, 14, 15, 99, 239, 240, 241, 242,
		243, 244, 245, 246, 247, 248, 27, 28, 249, 250,
		251, 252, 253, 254, 255, 256, 257, 258, 259, 260,
		261, 262, 263, 264, 265, 266, 109, 110, 267, 268,
		269, 270, 271, 272, 273, 274, 275, 276, 277, 278,
		279, 280, 281, 282, 283, 284, 285, 286, 287, 288,
		289, 290, 291, 292, 293, 294, 295, 296, 297, 298,
		299, 300, 301, 302, 303, 304, 305, 306, 307, 308,
		309, 310, 311, 312, 313, 314, 315, 316, 317, 318,
		158, 155, 163, 319, 320, 321, 322, 323, 324, 325,
		326, 150, 164, 169, 327, 328, 329, 330, 331, 332,
		333, 334, 335, 336, 337, 338, 339, 340, 341, 342,
		343, 344, 345, 346, 347, 348, 349, 350, 351, 352,
		353, 354, 355, 356, 357, 358, 359, 360, 361, 362,
		363, 364, 365, 366, 367, 368, 369, 370, 371, 372,
		373, 374, 375, 376, 377, 378
	};

	public int localBias;

	internal CffGlyphs readType1CFontFile(byte[] fontDataAsArray)
	{
		StreamReader streamReader = new StreamReader(new MemoryStream(fontDataAsArray));
		while (true)
		{
			string text = streamReader.ReadLine();
			if (text == null)
			{
				break;
			}
			if (text.IndexOf("/FontMatrix") == -1)
			{
				continue;
			}
			string text2 = "";
			int num = text.IndexOf('[');
			if (num != -1)
			{
				int num2 = text.IndexOf(']');
				text2 = text.Substring(num + 1, num2 - (num + 1));
			}
			else
			{
				num = text.IndexOf('{');
				if (num != -1)
				{
					int num2 = text.IndexOf('}');
					text2 = text.Substring(num + 1, num2 - (num + 1));
				}
			}
			string[] array = text2.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < 6; i++)
			{
				m_fontMatrix[i] = Convert.ToDouble(array[i]);
			}
		}
		int num3 = 2;
		_ = fontDataAsArray[0];
		_ = fontDataAsArray[1];
		top = fontDataAsArray[2];
		int num4 = 0;
		int num5 = 0;
		num4 = getWord(fontDataAsArray, top, num3);
		num5 = fontDataAsArray[top + num3];
		top += num3 + 1;
		int num6 = top + (num4 + 1) * num5 - 1;
		top = num6 + getWord(fontDataAsArray, top + num4 * num5, num5);
		num4 = getWord(fontDataAsArray, top, num3);
		num5 = fontDataAsArray[top + num3];
		top += num3 + 1;
		num6 = top + (num4 + 1) * num5 - 1;
		int num7 = 0;
		int num8 = 0;
		num7 = num6 + getWord(fontDataAsArray, top, num5);
		num8 = num6 + getWord(fontDataAsArray, top + num5, num5);
		string[] strings = readStringIndex(fontDataAsArray, num6, num5, num4);
		readGlobalSubRoutines(fontDataAsArray);
		decodeDictionary(fontDataAsArray, num7, num8, strings);
		if (FDSelect != -1)
		{
			int fDArray = FDArray;
			num4 = getWord(fontDataAsArray, fDArray, num3);
			num5 = fontDataAsArray[fDArray + num3];
			fDArray += num3 + 1;
			num6 = fDArray + (num4 + 1) * num5 - 1;
			num7 = num6 + getWord(fontDataAsArray, fDArray, num5);
			num8 = num6 + getWord(fontDataAsArray, fDArray + num5, num5);
			decodeDictionary(fontDataAsArray, num7, num8, strings);
		}
		top = charstrings;
		glyphCount = getWord(fontDataAsArray, top, num3);
		int[] names = readCharset(charset, glyphCount, charstrings, fontDataAsArray);
		setEncoding(fontDataAsArray, glyphCount, names);
		top = charstrings;
		readGlyphs(fontDataAsArray, glyphCount, names);
		if (privateDict != -1)
		{
			decodeDictionary(fontDataAsArray, privateDict, privateDictOffset + privateDict, strings);
			top = privateDict + privateDictOffset;
			int num9 = 0;
			int num10 = 0;
			num9 = fontDataAsArray.Length;
			if (top + 2 < num9)
			{
				num10 = getWord(fontDataAsArray, top, num3);
				if (num10 > 0)
				{
					readSubrs(fontDataAsArray, num10);
				}
			}
		}
		m_cffGlyphs.Glyphs = glyphs;
		m_cffGlyphs.FontMatrix = m_fontMatrix;
		m_cffGlyphs.DiffTable = m_diffTable;
		m_cffGlyphs.GlobalBias = GlobalBias;
		return m_cffGlyphs;
	}

	private void setEncoding(byte[] fontDataAsArray, int totalGlyphCount, int[] names)
	{
		bool flag = fontDataAsArray != null;
		top = encodeValue;
		int num = 0;
		int num2 = 0;
		if (flag)
		{
			num2 = fontDataAsArray[top++] & 0xFF;
		}
		string mappedChar = null;
		if ((num2 & 0x7F) == 0)
		{
			int num3 = 0;
			if (flag)
			{
				num3 = 1 + (fontDataAsArray[top++] & 0xFF);
			}
			if (num3 > totalGlyphCount)
			{
				num3 = totalGlyphCount;
			}
			for (int i = 1; i < num3; i++)
			{
				if (flag)
				{
					num = fontDataAsArray[top++] & 0xFF;
					mappedChar = getString(fontDataAsArray, names[i], stringIndex, stringStart, stringOffsetSize);
				}
				DifferenceTableMapping(num, mappedChar);
			}
		}
		else if ((num2 & 0x7F) == 1)
		{
			int num4 = 0;
			if (flag)
			{
				num4 = fontDataAsArray[top++] & 0xFF;
			}
			int num5 = 1;
			for (int j = 0; j < num4; j++)
			{
				if (flag)
				{
					num = fontDataAsArray[top++] & 0xFF;
				}
				for (int k = 0; k <= num; k++)
				{
					if (num5 >= totalGlyphCount)
					{
						break;
					}
					if (flag)
					{
						mappedChar = getString(fontDataAsArray, names[num5], stringIndex, stringStart, stringOffsetSize);
					}
					DifferenceTableMapping(num, mappedChar);
					num5++;
					num++;
				}
			}
		}
		if ((num2 & 0x80) == 0)
		{
			return;
		}
		int num6 = 0;
		if (flag)
		{
			num6 = fontDataAsArray[top++] & 0xFF;
		}
		for (int l = 0; l < num6; l++)
		{
			if (flag)
			{
				num = fontDataAsArray[top++] & 0xFF;
			}
			int stringId = 0;
			if (flag)
			{
				stringId = getWord(fontDataAsArray, top, 2);
			}
			top += 2;
			if (flag)
			{
				mappedChar = getString(fontDataAsArray, stringId, stringIndex, stringStart, stringOffsetSize);
			}
			DifferenceTableMapping(num, mappedChar);
		}
	}

	internal void DifferenceTableMapping(int charInt, string mappedChar)
	{
		if (m_diffTable == null)
		{
			m_diffTable = new string[maxCharCount];
		}
		if (m_diffTable.Length > charInt)
		{
			m_diffTable[charInt] = mappedChar;
		}
		if (!isCID)
		{
			InsertMappedChar(charInt, mappedChar);
		}
	}

	public void InsertMappedChar(int charInt, string mappedChar)
	{
		if (m_diffTable == null)
		{
			m_diffTable = new string[maxCharCount];
		}
		else if (m_diffTable.Length > charInt && m_diffTable[charInt] == null && mappedChar != null && !mappedChar.StartsWith("glyph"))
		{
			m_diffTable[charInt] = mappedChar;
		}
	}

	private int getWord(byte[] fontDataAsArray, int index, int size)
	{
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			if (index + i < fontDataAsArray.Length)
			{
				num = (num << 8) + (fontDataAsArray[index + i] & 0xFF);
			}
		}
		return num;
	}

	private string[] readStringIndex(byte[] fontDataAsArray, int start, int offsize, int count)
	{
		int num = 0;
		bool flag = fontDataAsArray != null;
		if (flag)
		{
			top = start + getWord(fontDataAsArray, top + count * offsize, offsize);
			num = getWord(fontDataAsArray, top, 2);
			stringOffsetSize = fontDataAsArray[top + 2];
		}
		top += 3;
		stringIndex = top;
		stringStart = top + (num + 1) * stringOffsetSize - 1;
		if (flag)
		{
			top = stringStart + getWord(fontDataAsArray, top + num * stringOffsetSize, stringOffsetSize);
		}
		int[] array = new int[num + 2];
		string[] array2 = new string[num + 2];
		int num2 = stringIndex;
		for (int i = 0; i < num + 1; i++)
		{
			if (flag)
			{
				array[i] = getWord(fontDataAsArray, num2, stringOffsetSize);
			}
			num2 += stringOffsetSize;
		}
		array[num + 1] = top - stringStart;
		int num3 = 0;
		for (int j = 0; j < num + 1; j++)
		{
			StringBuilder stringBuilder = new StringBuilder(array[j] - num3);
			for (int k = num3; k < array[j]; k++)
			{
				if (flag)
				{
					stringBuilder.Append((char)fontDataAsArray[stringStart + k]);
				}
			}
			array2[j] = stringBuilder.ToString();
			num3 = array[j];
		}
		return array2;
	}

	private void readGlobalSubRoutines(byte[] fontDataAsArray)
	{
		bool flag = fontDataAsArray != null;
		int num = 0;
		int num2 = 0;
		if (flag)
		{
			num = fontDataAsArray[top + 2] & 0xFF;
			num2 = getWord(fontDataAsArray, top, 2);
		}
		top += 3;
		if (num2 <= 0)
		{
			return;
		}
		int num3 = top;
		int num4 = top + (num2 + 1) * num - 1;
		if (flag)
		{
			top = num4 + getWord(fontDataAsArray, top + num2 * num, num);
		}
		int[] array = new int[num2 + 2];
		int num5 = num3;
		for (int i = 0; i < num2 + 1; i++)
		{
			if (flag)
			{
				array[i] = num4 + getWord(fontDataAsArray, num5, num);
			}
			num5 += num;
		}
		array[num2 + 1] = top;
		GlobalBias = calculateSubroutineBias(num2);
		int num6 = array[0];
		for (int j = 1; j < num2 + 1; j++)
		{
			MemoryStream memoryStream = new MemoryStream();
			for (int k = num6; k < array[j]; k++)
			{
				if (flag)
				{
					memoryStream.WriteByte(fontDataAsArray[k]);
				}
			}
			memoryStream.Close();
			glyphs.Add("global" + (j - 1), memoryStream.ToArray());
			num6 = array[j];
		}
	}

	private void decodeDictionary(byte[] fontDataAsArray, int dicStart, int dicEnd, string[] strings)
	{
		bool flag = false;
		bool flag2 = fontDataAsArray != null;
		int num = dicStart;
		int num2 = 0;
		int num3 = 0;
		double[] array = new double[48];
		while (num < dicEnd)
		{
			if (flag2)
			{
				num2 = fontDataAsArray[num] & 0xFF;
			}
			if (num2 <= 27 || num2 == 31)
			{
				int num4 = num2;
				num++;
				switch (num4)
				{
				case 12:
					if (flag2)
					{
						num4 = fontDataAsArray[num] & 0xFF;
					}
					num++;
					if (num4 != 36 && num4 != 37 && num4 != 7 && FDSelect != -1)
					{
						break;
					}
					switch (num4)
					{
					case 2:
						italicAngle = (int)array[0];
						break;
					case 7:
						if (!hasFontMatrix)
						{
							Array.Copy(array, 0, FontMatrix, 0, 6);
						}
						hasFontMatrix = true;
						break;
					case 30:
						ROS = (int)array[0];
						isCID = true;
						break;
					case 31:
						CIDFontVersion = (int)array[0];
						break;
					case 32:
						CIDFontRevision = (int)array[0];
						break;
					case 33:
						CIDFontType = (int)array[0];
						break;
					case 34:
						CIDcount = (int)array[0];
						break;
					case 35:
						UIDBase = (int)array[0];
						break;
					case 36:
						FDArray = (int)array[0];
						break;
					case 37:
						FDSelect = (int)array[0];
						flag = true;
						break;
					case 0:
					{
						int num5 = (int)array[0];
						if (num5 > 390)
						{
							num5 -= 390;
						}
						copyright = strings[num5];
						break;
					}
					}
					break;
				case 2:
				{
					int num6 = (int)array[0];
					if (num6 > 390)
					{
						num6 -= 390;
					}
					embeddedFontName = strings[num6];
					break;
				}
				case 5:
				{
					for (int i = 0; i < 4; i++)
					{
						FontBBox[i] = (float)array[i];
					}
					break;
				}
				case 15:
					charset = (int)array[0];
					break;
				case 16:
					encodeValue = (int)array[0];
					break;
				case 17:
					charstrings = (int)array[0];
					break;
				case 18:
					privateDict = (int)array[1];
					privateDictOffset = (int)array[0];
					break;
				case 20:
					defaultWidthX = (int)array[0];
					break;
				case 21:
					nominalWidthX = (int)array[0];
					break;
				}
				num3 = 0;
			}
			else
			{
				if (flag2)
				{
					num = getNumber(fontDataAsArray, num, array, num3, debug: false);
				}
				num3++;
			}
		}
		if (!flag)
		{
			FDSelect = -1;
		}
	}

	public int getNumber(byte[] fontDataAsArray, int pos, double[] values, int valuePointer, bool debug)
	{
		double num = 0.0;
		int num2 = fontDataAsArray[pos] & 0xFF;
		if (num2 < 28 || num2 == 31)
		{
			Console.Error.WriteLine("!!!!Incorrect type1C operand");
		}
		else if (num2 == 28)
		{
			num = (fontDataAsArray[pos + 1] << 8) + (fontDataAsArray[pos + 2] & 0xFF);
			pos += 3;
		}
		else if (num2 == 255)
		{
			int num3 = ((fontDataAsArray[pos + 1] & 0xFF) << 8) + (fontDataAsArray[pos + 2] & 0xFF);
			if (num3 > 32768)
			{
				num3 = 65536 - num3;
			}
			double num4 = num3;
			double num5 = ((fontDataAsArray[pos + 3] & 0xFF) << 8) + (fontDataAsArray[pos + 4] & 0xFF);
			num = num4 + num5 / 65536.0;
			if (fontDataAsArray[pos + 1] < 0)
			{
				if (debug)
				{
					Console.WriteLine("Negative " + num);
				}
				num = 0.0 - num;
			}
			if (debug)
			{
				Console.WriteLine("x=" + num);
				for (int i = 0; i < 5; i++)
				{
					Console.WriteLine(i + " " + fontDataAsArray[pos + i] + " " + (fontDataAsArray[pos + i] & 0xFF) + " " + (fontDataAsArray[pos + i] & 0x7F));
				}
			}
			pos += 5;
		}
		else if (num2 == 29)
		{
			num = ((fontDataAsArray[pos + 1] & 0xFF) << 24) + ((fontDataAsArray[pos + 2] & 0xFF) << 16) + ((fontDataAsArray[pos + 3] & 0xFF) << 8) + (fontDataAsArray[pos + 4] & 0xFF);
			pos += 5;
		}
		else if (num2 == 30)
		{
			char[] array = new char[65];
			pos++;
			int num6 = 0;
			while (num6 < 64)
			{
				int num7 = fontDataAsArray[pos++] & 0xFF;
				int num8 = (num7 >> 4) & 0xF;
				int num9 = num7 & 0xF;
				if (num8 == 15)
				{
					break;
				}
				array[num6++] = nybChars[num8];
				if (num6 == 64)
				{
					break;
				}
				if (num8 == 12)
				{
					array[num6++] = '-';
				}
				if (num6 == 64 || num9 == 15)
				{
					break;
				}
				array[num6++] = nybChars[num9];
				if (num6 == 64)
				{
					break;
				}
				if (num9 == 12)
				{
					array[num6++] = '-';
				}
			}
			num = Convert.ToDouble(new string(array, 0, num6));
		}
		else if (num2 < 247)
		{
			num = num2 - 139;
			pos++;
		}
		else if (num2 < 251)
		{
			num = (num2 - 247 << 8) + (fontDataAsArray[pos + 1] & 0xFF) + 108;
			pos += 2;
		}
		else
		{
			num = -(num2 - 251 << 8) - (fontDataAsArray[pos + 1] & 0xFF) - 108;
			pos += 2;
		}
		values[valuePointer] = num;
		return pos;
	}

	private int[] readCharset(int charset, int nGlyphs, int top, byte[] fontDataAsArray)
	{
		bool flag = fontDataAsArray != null;
		int[] array;
		switch (charset)
		{
		case 0:
			array = ISOAdobeCharset;
			break;
		case 1:
			array = ExpertCharset;
			break;
		case 2:
			array = ExpertSubCharset;
			break;
		default:
		{
			array = new int[nGlyphs + 1];
			array[0] = 0;
			top = charset;
			int num = 0;
			if (flag)
			{
				num = fontDataAsArray[top++] & 0xFF;
			}
			switch (num)
			{
			case 0:
			{
				for (int num2 = 1; num2 < nGlyphs; num2++)
				{
					if (flag)
					{
						array[num2] = getWord(fontDataAsArray, top, 2);
					}
					top += 2;
				}
				break;
			}
			case 1:
			{
				int num2 = 1;
				int num5 = 0;
				int num6 = 0;
				while (num2 < nGlyphs)
				{
					if (flag)
					{
						num5 = getWord(fontDataAsArray, top, 2);
					}
					top += 2;
					if (flag)
					{
						num6 = fontDataAsArray[top++] & 0xFF;
					}
					for (int i = 0; i <= num6; i++)
					{
						array[num2++] = num5++;
					}
				}
				break;
			}
			case 2:
			{
				int num2 = 1;
				int num3 = 0;
				int num4 = 0;
				while (num2 < nGlyphs)
				{
					if (flag)
					{
						num3 = getWord(fontDataAsArray, top, 2);
					}
					top += 2;
					if (flag)
					{
						num4 = getWord(fontDataAsArray, top, 2);
					}
					top += 2;
					for (int i = 0; i <= num4; i++)
					{
						array[num2++] = num3++;
					}
				}
				break;
			}
			}
			break;
		}
		}
		return array;
	}

	private int calculateSubroutineBias(int subroutineCount)
	{
		if (subroutineCount < 1240)
		{
			return 107;
		}
		if (subroutineCount < 33900)
		{
			return 1131;
		}
		return 32768;
	}

	internal virtual void readGlyphs(byte[] fontDataAsArray, int nGlyphs, int[] names)
	{
		bool flag = fontDataAsArray != null;
		int num = 0;
		if (flag)
		{
			num = fontDataAsArray[top + 2];
		}
		top += 3;
		int num2 = top;
		int num3 = top + (nGlyphs + 1) * num - 1;
		if (flag)
		{
			top = num3 + getWord(fontDataAsArray, top + nGlyphs * num, num);
		}
		int[] array = new int[nGlyphs + 2];
		int num4 = num2;
		for (int i = 0; i < nGlyphs + 1; i++)
		{
			if (flag)
			{
				array[i] = num3 + getWord(fontDataAsArray, num4, num);
			}
			num4 += num;
		}
		array[nGlyphs + 1] = top;
		int num5 = array[0];
		string key = "";
		for (int j = 1; j < nGlyphs + 1; j++)
		{
			byte[] array2 = new byte[array[j] - num5];
			for (int k = num5; k < array[j]; k++)
			{
				if (flag)
				{
					array2[k - num5] = fontDataAsArray[k];
				}
			}
			if (isCID)
			{
				key = Convert.ToString(names[j - 1]);
			}
			else if (flag)
			{
				key = getString(fontDataAsArray, names[j - 1], stringIndex, stringStart, stringOffsetSize);
			}
			glyphs.Add(key, array2);
			num5 = array[j];
			_ = trackIndices;
		}
	}

	private string getString(byte[] fontDataAsArray, int stringId, int idx, int start, int offsize)
	{
		if (stringId < 391)
		{
			return type1CStdStrings[stringId];
		}
		stringId -= 391;
		int num = start + getWord(fontDataAsArray, idx + stringId * offsize, offsize);
		int num2 = start + getWord(fontDataAsArray, idx + (stringId + 1) * offsize, offsize);
		int num3;
		if ((num3 = num2 - num) > 255)
		{
			num3 = 255;
		}
		else if ((num3 = num2 - num) < 0)
		{
			num3 = 0;
		}
		return Encoding.UTF8.GetString(fontDataAsArray, num, num3);
	}

	internal virtual void readSubrs(byte[] fontDataAsArray, int nSubrs)
	{
		bool flag = fontDataAsArray != null;
		int num = 0;
		if (flag)
		{
			num = fontDataAsArray[top + 2];
		}
		top += 3;
		int num2 = top;
		int num3 = top + (nSubrs + 1) * num - 1;
		int num4 = top + nSubrs * num;
		if (flag)
		{
			if (num4 < fontDataAsArray.Length)
			{
				top = num3 + getWord(fontDataAsArray, num4, num);
			}
			else
			{
				top = fontDataAsArray.Length - 1;
			}
		}
		int[] array = new int[nSubrs + 2];
		int num5 = num2;
		for (int i = 0; i < nSubrs + 1; i++)
		{
			if (flag && num5 + num < fontDataAsArray.Length)
			{
				array[i] = num3 + getWord(fontDataAsArray, num5, num);
			}
			num5 += num;
		}
		array[nSubrs + 1] = top;
		localBias = calculateSubroutineBias(nSubrs);
		int num6 = array[0];
		for (int j = 1; j < nSubrs + 1; j++)
		{
			if (num6 == 0 || array[j] > fontDataAsArray.Length || array[j] < 0 || array[j] == 0)
			{
				continue;
			}
			if (flag)
			{
				int num7 = array[j] - num6;
				if (num7 > 0)
				{
					byte[] array2 = new byte[num7];
					Array.Copy(fontDataAsArray, num6, array2, 0, num7);
					glyphs.Add("subrs" + (j - 1), array2);
				}
			}
			num6 = array[j];
		}
	}
}
