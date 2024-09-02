using System.Collections.Generic;

namespace DocGen.Pdf;

internal class PredefinedEncoding
{
	private string[] names;

	private string m_name;

	private static PredefinedEncoding m_pdfdocEncoding;

	private static PredefinedEncoding m_winAnsiEncoding;

	private static PredefinedEncoding m_macRomanEncoding;

	private static PredefinedEncoding m_standardMacRomanEncoding;

	private static PredefinedEncoding m_standardEncoding;

	private Dictionary<string, byte> mapping;

	public static PredefinedEncoding PdfDocEncoding
	{
		get
		{
			return m_pdfdocEncoding;
		}
		private set
		{
			m_pdfdocEncoding = value;
		}
	}

	public static PredefinedEncoding WinAnsiEncoding
	{
		get
		{
			return m_winAnsiEncoding;
		}
		private set
		{
			m_winAnsiEncoding = value;
		}
	}

	public static PredefinedEncoding MacRomanEncoding
	{
		get
		{
			return m_macRomanEncoding;
		}
		private set
		{
			m_macRomanEncoding = value;
		}
	}

	public static PredefinedEncoding StandardMacRomanEncoding
	{
		get
		{
			return m_standardMacRomanEncoding;
		}
		private set
		{
			m_standardMacRomanEncoding = value;
		}
	}

	public static PredefinedEncoding StandardEncoding
	{
		get
		{
			return m_standardEncoding;
		}
		private set
		{
			m_standardEncoding = value;
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		private set
		{
			m_name = value;
		}
	}

	private static void InitializePdfEncoding()
	{
		m_pdfdocEncoding = new PredefinedEncoding();
		m_pdfdocEncoding.Init();
		m_pdfdocEncoding.names[130] = "adblgrave";
		m_pdfdocEncoding.names[100] = "d";
		m_pdfdocEncoding.names[101] = "e";
		m_pdfdocEncoding.names[102] = "f";
		m_pdfdocEncoding.names[103] = "g";
		m_pdfdocEncoding.names[104] = "h";
		m_pdfdocEncoding.names[105] = "i";
		m_pdfdocEncoding.names[106] = "j";
		m_pdfdocEncoding.names[107] = "k";
		m_pdfdocEncoding.names[108] = "l";
		m_pdfdocEncoding.names[109] = "m";
		m_pdfdocEncoding.names[110] = "n";
		m_pdfdocEncoding.names[111] = "o";
		m_pdfdocEncoding.names[112] = "p";
		m_pdfdocEncoding.names[113] = "q";
		m_pdfdocEncoding.names[114] = "r";
		m_pdfdocEncoding.names[115] = "s";
		m_pdfdocEncoding.names[116] = "t";
		m_pdfdocEncoding.names[117] = "u";
		m_pdfdocEncoding.names[118] = "v";
		m_pdfdocEncoding.names[119] = "w";
		m_pdfdocEncoding.names[120] = "x";
		m_pdfdocEncoding.names[121] = "y";
		m_pdfdocEncoding.names[122] = "z";
		m_pdfdocEncoding.names[123] = "braceleft";
		m_pdfdocEncoding.names[124] = "bar";
		m_pdfdocEncoding.names[125] = "braceright";
		m_pdfdocEncoding.names[126] = "asciitilde";
		m_pdfdocEncoding.names[128] = "bullet";
		m_pdfdocEncoding.names[129] = "dagger";
		m_pdfdocEncoding.names[131] = "ellipsis";
		m_pdfdocEncoding.names[132] = "emdash";
		m_pdfdocEncoding.names[133] = "endash";
		m_pdfdocEncoding.names[134] = "florin";
		m_pdfdocEncoding.names[135] = "fraction";
		m_pdfdocEncoding.names[136] = "guilsinglleft";
		m_pdfdocEncoding.names[137] = "guilsinglright";
		m_pdfdocEncoding.names[138] = "minus";
		m_pdfdocEncoding.names[139] = "perthousand";
		m_pdfdocEncoding.names[140] = "quotedblbase";
		m_pdfdocEncoding.names[141] = "quotedblleft";
		m_pdfdocEncoding.names[142] = "quotedblright";
		m_pdfdocEncoding.names[143] = "quoteleft";
		m_pdfdocEncoding.names[144] = "quoteright";
		m_pdfdocEncoding.names[145] = "quotesinglbase";
		m_pdfdocEncoding.names[146] = "trademark";
		m_pdfdocEncoding.names[147] = "fi";
		m_pdfdocEncoding.names[148] = "fl";
		m_pdfdocEncoding.names[149] = "Lslash";
		m_pdfdocEncoding.names[150] = "OE";
		m_pdfdocEncoding.names[151] = "Scaron";
		m_pdfdocEncoding.names[152] = "Ydieresis";
		m_pdfdocEncoding.names[153] = "Zcaron";
		m_pdfdocEncoding.names[154] = "dotlessi";
		m_pdfdocEncoding.names[155] = "lslash";
		m_pdfdocEncoding.names[156] = "oe";
		m_pdfdocEncoding.names[157] = "scaron";
		m_pdfdocEncoding.names[158] = "zcaron";
		m_pdfdocEncoding.names[160] = "Euro";
		m_pdfdocEncoding.names[161] = "exclamdown";
		m_pdfdocEncoding.names[162] = "cent";
		m_pdfdocEncoding.names[163] = "sterling";
		m_pdfdocEncoding.names[164] = "currency";
		m_pdfdocEncoding.names[165] = "yen";
		m_pdfdocEncoding.names[166] = "brokenbar";
		m_pdfdocEncoding.names[167] = "section";
		m_pdfdocEncoding.names[168] = "dieresis";
		m_pdfdocEncoding.names[169] = "copyright";
		m_pdfdocEncoding.names[170] = "ordfeminine";
		m_pdfdocEncoding.names[171] = "guillemotleft";
		m_pdfdocEncoding.names[172] = "logicalnot";
		m_pdfdocEncoding.names[174] = "registered";
		m_pdfdocEncoding.names[175] = "macron";
		m_pdfdocEncoding.names[176] = "degree";
		m_pdfdocEncoding.names[177] = "plusminus";
		m_pdfdocEncoding.names[178] = "twosuperior";
		m_pdfdocEncoding.names[179] = "threesuperior";
		m_pdfdocEncoding.names[180] = "acute";
		m_pdfdocEncoding.names[181] = "mu";
		m_pdfdocEncoding.names[182] = "paragraph";
		m_pdfdocEncoding.names[183] = "middot";
		m_pdfdocEncoding.names[184] = "cedilla";
		m_pdfdocEncoding.names[185] = "onesuperior";
		m_pdfdocEncoding.names[186] = "ordmasculine";
		m_pdfdocEncoding.names[187] = "guillemotright";
		m_pdfdocEncoding.names[188] = "onequarter";
		m_pdfdocEncoding.names[189] = "onehalf";
		m_pdfdocEncoding.names[190] = "threequarters";
		m_pdfdocEncoding.names[191] = "questiondown";
		m_pdfdocEncoding.names[192] = "Agrave";
		m_pdfdocEncoding.names[193] = "Aacute";
		m_pdfdocEncoding.names[194] = "Acircumflex";
		m_pdfdocEncoding.names[195] = "Atilde";
		m_pdfdocEncoding.names[196] = "Adieresis";
		m_pdfdocEncoding.names[197] = "Aring";
		m_pdfdocEncoding.names[198] = "AE";
		m_pdfdocEncoding.names[199] = "Ccedilla";
		m_pdfdocEncoding.names[200] = "Egrave";
		m_pdfdocEncoding.names[201] = "Eacute";
		m_pdfdocEncoding.names[202] = "Ecircumflex";
		m_pdfdocEncoding.names[203] = "Edieresis";
		m_pdfdocEncoding.names[204] = "Igrave";
		m_pdfdocEncoding.names[205] = "Iacute";
		m_pdfdocEncoding.names[206] = "Icircumflex";
		m_pdfdocEncoding.names[207] = "Idieresis";
		m_pdfdocEncoding.names[208] = "Eth";
		m_pdfdocEncoding.names[210] = "Ntilde";
		m_pdfdocEncoding.names[211] = "Oacute";
		m_pdfdocEncoding.names[212] = "Ocircumflex";
		m_pdfdocEncoding.names[213] = "Otilde";
		m_pdfdocEncoding.names[214] = "Odieresis";
		m_pdfdocEncoding.names[215] = "multiply";
		m_pdfdocEncoding.names[216] = "Oslash";
		m_pdfdocEncoding.names[217] = "Ugrave";
		m_pdfdocEncoding.names[218] = "Uacute";
		m_pdfdocEncoding.names[219] = "Ucircumflex";
		m_pdfdocEncoding.names[220] = "Udieresis";
		m_pdfdocEncoding.names[221] = "Yacute";
		m_pdfdocEncoding.names[222] = "Thorn";
		m_pdfdocEncoding.names[223] = "germandbls";
		m_pdfdocEncoding.names[224] = "agrave";
		m_pdfdocEncoding.names[225] = "aacute";
		m_pdfdocEncoding.names[226] = "acircumflex";
		m_pdfdocEncoding.names[227] = "atilde";
		m_pdfdocEncoding.names[228] = "adieresis";
		m_pdfdocEncoding.names[229] = "aring";
		m_pdfdocEncoding.names[230] = "ae";
		m_pdfdocEncoding.names[231] = "ccedilla";
		m_pdfdocEncoding.names[232] = "egrave";
		m_pdfdocEncoding.names[233] = "eacute";
		m_pdfdocEncoding.names[234] = "ecircumflex";
		m_pdfdocEncoding.names[235] = "edieresis";
		m_pdfdocEncoding.names[236] = "igrave";
		m_pdfdocEncoding.names[237] = "iacute";
		m_pdfdocEncoding.names[238] = "icircumflex";
		m_pdfdocEncoding.names[239] = "idieresis";
		m_pdfdocEncoding.names[24] = "breve";
		m_pdfdocEncoding.names[240] = "eth";
		m_pdfdocEncoding.names[241] = "ntilde";
		m_pdfdocEncoding.names[242] = "ograve";
		m_pdfdocEncoding.names[243] = "oacute";
		m_pdfdocEncoding.names[244] = "ocircumflex";
		m_pdfdocEncoding.names[245] = "otilde";
		m_pdfdocEncoding.names[246] = "odieresis";
		m_pdfdocEncoding.names[247] = "divide";
		m_pdfdocEncoding.names[248] = "oslash";
		m_pdfdocEncoding.names[249] = "ugrave";
		m_pdfdocEncoding.names[25] = "caron";
		m_pdfdocEncoding.names[250] = "uacute";
		m_pdfdocEncoding.names[251] = "ucircumflex";
		m_pdfdocEncoding.names[252] = "udieresis";
		m_pdfdocEncoding.names[253] = "yacute";
		m_pdfdocEncoding.names[254] = "thorn";
		m_pdfdocEncoding.names[255] = "ydieresis";
		m_pdfdocEncoding.names[26] = "circumflex";
		m_pdfdocEncoding.names[27] = "dotaccent";
		m_pdfdocEncoding.names[28] = "hungarumlaut";
		m_pdfdocEncoding.names[29] = "ogonek";
		m_pdfdocEncoding.names[30] = "ring";
		m_pdfdocEncoding.names[31] = "ilde";
		m_pdfdocEncoding.names[32] = "space";
		m_pdfdocEncoding.names[33] = "exclam";
		m_pdfdocEncoding.names[34] = "quotedbl";
		m_pdfdocEncoding.names[35] = "numbersign";
		m_pdfdocEncoding.names[36] = "dollar";
		m_pdfdocEncoding.names[37] = "percent";
		m_pdfdocEncoding.names[38] = "ampersand";
		m_pdfdocEncoding.names[39] = "quotesingle";
		m_pdfdocEncoding.names[40] = "parenleft";
		m_pdfdocEncoding.names[41] = "parenright";
		m_pdfdocEncoding.names[42] = "asterisk";
		m_pdfdocEncoding.names[43] = "plus";
		m_pdfdocEncoding.names[44] = "comma";
		m_pdfdocEncoding.names[45] = "hyphen";
		m_pdfdocEncoding.names[46] = "period";
		m_pdfdocEncoding.names[47] = "slash";
		m_pdfdocEncoding.names[48] = "zero";
		m_pdfdocEncoding.names[49] = "one";
		m_pdfdocEncoding.names[50] = "two";
		m_pdfdocEncoding.names[51] = "three";
		m_pdfdocEncoding.names[52] = "four";
		m_pdfdocEncoding.names[53] = "five";
		m_pdfdocEncoding.names[54] = "six";
		m_pdfdocEncoding.names[55] = "seven";
		m_pdfdocEncoding.names[56] = "eight";
		m_pdfdocEncoding.names[57] = "nine";
		m_pdfdocEncoding.names[58] = "colon";
		m_pdfdocEncoding.names[59] = "semicolon";
		m_pdfdocEncoding.names[60] = "less";
		m_pdfdocEncoding.names[61] = "equal";
		m_pdfdocEncoding.names[62] = "greater";
		m_pdfdocEncoding.names[63] = "question";
		m_pdfdocEncoding.names[64] = "at";
		m_pdfdocEncoding.names[65] = "A";
		m_pdfdocEncoding.names[66] = "B";
		m_pdfdocEncoding.names[67] = "C";
		m_pdfdocEncoding.names[68] = "D";
		m_pdfdocEncoding.names[69] = "E";
		m_pdfdocEncoding.names[70] = "F";
		m_pdfdocEncoding.names[71] = "G";
		m_pdfdocEncoding.names[72] = "H";
		m_pdfdocEncoding.names[73] = "I";
		m_pdfdocEncoding.names[74] = "J";
		m_pdfdocEncoding.names[75] = "K";
		m_pdfdocEncoding.names[76] = "L";
		m_pdfdocEncoding.names[77] = "M";
		m_pdfdocEncoding.names[78] = "N";
		m_pdfdocEncoding.names[79] = "O";
		m_pdfdocEncoding.names[80] = "P";
		m_pdfdocEncoding.names[81] = "Q";
		m_pdfdocEncoding.names[82] = "R";
		m_pdfdocEncoding.names[83] = "S";
		m_pdfdocEncoding.names[84] = "T";
		m_pdfdocEncoding.names[85] = "U";
		m_pdfdocEncoding.names[86] = "V";
		m_pdfdocEncoding.names[87] = "W";
		m_pdfdocEncoding.names[88] = "X";
		m_pdfdocEncoding.names[89] = "Y";
		m_pdfdocEncoding.names[90] = "Z";
		m_pdfdocEncoding.names[91] = "bracketleft";
		m_pdfdocEncoding.names[92] = "backslash";
		m_pdfdocEncoding.names[93] = "bracketright";
		m_pdfdocEncoding.names[94] = "asciicircum";
		m_pdfdocEncoding.names[95] = "underscore";
		m_pdfdocEncoding.names[96] = "grave";
		m_pdfdocEncoding.names[97] = "a";
		m_pdfdocEncoding.names[98] = "b";
		m_pdfdocEncoding.names[99] = "c";
		m_pdfdocEncoding.InitMapping();
	}

	private static void InitializeWinAnsiEncoding()
	{
		m_winAnsiEncoding = new PredefinedEncoding();
		m_winAnsiEncoding.Init();
		m_winAnsiEncoding.m_name = "WinAnsiEncoding";
		m_winAnsiEncoding.names[173] = "hyphen";
		m_winAnsiEncoding.names[160] = "space";
		m_winAnsiEncoding.names[135] = "adblgrave";
		m_winAnsiEncoding.names[100] = "d";
		m_winAnsiEncoding.names[101] = "e";
		m_winAnsiEncoding.names[102] = "f";
		m_winAnsiEncoding.names[103] = "g";
		m_winAnsiEncoding.names[104] = "h";
		m_winAnsiEncoding.names[105] = "i";
		m_winAnsiEncoding.names[106] = "j";
		m_winAnsiEncoding.names[107] = "k";
		m_winAnsiEncoding.names[108] = "l";
		m_winAnsiEncoding.names[109] = "m";
		m_winAnsiEncoding.names[110] = "n";
		m_winAnsiEncoding.names[111] = "o";
		m_winAnsiEncoding.names[112] = "p";
		m_winAnsiEncoding.names[113] = "q";
		m_winAnsiEncoding.names[114] = "r";
		m_winAnsiEncoding.names[115] = "s";
		m_winAnsiEncoding.names[116] = "t";
		m_winAnsiEncoding.names[117] = "u";
		m_winAnsiEncoding.names[118] = "v";
		m_winAnsiEncoding.names[119] = "w";
		m_winAnsiEncoding.names[120] = "x";
		m_winAnsiEncoding.names[121] = "y";
		m_winAnsiEncoding.names[122] = "z";
		m_winAnsiEncoding.names[123] = "braceleft";
		m_winAnsiEncoding.names[124] = "bar";
		m_winAnsiEncoding.names[125] = "braceright";
		m_winAnsiEncoding.names[126] = "asciitilde";
		m_winAnsiEncoding.names[128] = "Euro";
		m_winAnsiEncoding.names[130] = "quotesinglbase";
		m_winAnsiEncoding.names[131] = "florin";
		m_winAnsiEncoding.names[132] = "quotedblbase";
		m_winAnsiEncoding.names[133] = "ellipsis";
		m_winAnsiEncoding.names[134] = "dagger";
		m_winAnsiEncoding.names[136] = "circumflex";
		m_winAnsiEncoding.names[137] = "perthousand";
		m_winAnsiEncoding.names[138] = "Scaron";
		m_winAnsiEncoding.names[139] = "guilsinglleft";
		m_winAnsiEncoding.names[140] = "OE";
		m_winAnsiEncoding.names[142] = "Zcaron";
		m_winAnsiEncoding.names[145] = "quoteleft";
		m_winAnsiEncoding.names[146] = "quoteright";
		m_winAnsiEncoding.names[147] = "quotedblleft";
		m_winAnsiEncoding.names[148] = "quotedblright";
		m_winAnsiEncoding.names[149] = "bullet";
		m_winAnsiEncoding.names[150] = "endash";
		m_winAnsiEncoding.names[151] = "emdash";
		m_winAnsiEncoding.names[152] = "ilde";
		m_winAnsiEncoding.names[153] = "trademark";
		m_winAnsiEncoding.names[154] = "scaron";
		m_winAnsiEncoding.names[155] = "guilsinglright";
		m_winAnsiEncoding.names[156] = "oe";
		m_winAnsiEncoding.names[158] = "zcaron";
		m_winAnsiEncoding.names[159] = "Ydieresis";
		m_winAnsiEncoding.names[161] = "exclamdown";
		m_winAnsiEncoding.names[162] = "cent";
		m_winAnsiEncoding.names[163] = "sterling";
		m_winAnsiEncoding.names[164] = "currency";
		m_winAnsiEncoding.names[165] = "yen";
		m_winAnsiEncoding.names[166] = "brokenbar";
		m_winAnsiEncoding.names[167] = "section";
		m_winAnsiEncoding.names[168] = "dieresis";
		m_winAnsiEncoding.names[169] = "copyright";
		m_winAnsiEncoding.names[170] = "ordfeminine";
		m_winAnsiEncoding.names[171] = "guillemotleft";
		m_winAnsiEncoding.names[172] = "logicalnot";
		m_winAnsiEncoding.names[174] = "registered";
		m_winAnsiEncoding.names[175] = "macron";
		m_winAnsiEncoding.names[176] = "degree";
		m_winAnsiEncoding.names[177] = "plusminus";
		m_winAnsiEncoding.names[178] = "twosuperior";
		m_winAnsiEncoding.names[179] = "threesuperior";
		m_winAnsiEncoding.names[180] = "acute";
		m_winAnsiEncoding.names[181] = "mu";
		m_winAnsiEncoding.names[182] = "paragraph";
		m_winAnsiEncoding.names[183] = "middot";
		m_winAnsiEncoding.names[184] = "cedilla";
		m_winAnsiEncoding.names[185] = "onesuperior";
		m_winAnsiEncoding.names[186] = "ordmasculine";
		m_winAnsiEncoding.names[187] = "guillemotright";
		m_winAnsiEncoding.names[188] = "onequarter";
		m_winAnsiEncoding.names[189] = "onehalf";
		m_winAnsiEncoding.names[190] = "threequarters";
		m_winAnsiEncoding.names[191] = "questiondown";
		m_winAnsiEncoding.names[192] = "Agrave";
		m_winAnsiEncoding.names[193] = "Aacute";
		m_winAnsiEncoding.names[194] = "Acircumflex";
		m_winAnsiEncoding.names[195] = "Atilde";
		m_winAnsiEncoding.names[196] = "Adieresis";
		m_winAnsiEncoding.names[197] = "Aring";
		m_winAnsiEncoding.names[198] = "AE";
		m_winAnsiEncoding.names[199] = "Ccedilla";
		m_winAnsiEncoding.names[200] = "Egrave";
		m_winAnsiEncoding.names[201] = "Eacute";
		m_winAnsiEncoding.names[202] = "Ecircumflex";
		m_winAnsiEncoding.names[203] = "Edieresis";
		m_winAnsiEncoding.names[204] = "Igrave";
		m_winAnsiEncoding.names[205] = "Iacute";
		m_winAnsiEncoding.names[206] = "Icircumflex";
		m_winAnsiEncoding.names[207] = "Idieresis";
		m_winAnsiEncoding.names[208] = "Eth";
		m_winAnsiEncoding.names[210] = "Ntilde";
		m_winAnsiEncoding.names[211] = "Oacute";
		m_winAnsiEncoding.names[212] = "Ocircumflex";
		m_winAnsiEncoding.names[213] = "Otilde";
		m_winAnsiEncoding.names[214] = "Odieresis";
		m_winAnsiEncoding.names[215] = "multiply";
		m_winAnsiEncoding.names[216] = "Oslash";
		m_winAnsiEncoding.names[217] = "Ugrave";
		m_winAnsiEncoding.names[218] = "Uacute";
		m_winAnsiEncoding.names[219] = "Ucircumflex";
		m_winAnsiEncoding.names[220] = "Udieresis";
		m_winAnsiEncoding.names[221] = "Yacute";
		m_winAnsiEncoding.names[222] = "Thorn";
		m_winAnsiEncoding.names[223] = "germandbls";
		m_winAnsiEncoding.names[224] = "agrave";
		m_winAnsiEncoding.names[225] = "aacute";
		m_winAnsiEncoding.names[226] = "acircumflex";
		m_winAnsiEncoding.names[227] = "atilde";
		m_winAnsiEncoding.names[228] = "adieresis";
		m_winAnsiEncoding.names[229] = "aring";
		m_winAnsiEncoding.names[230] = "ae";
		m_winAnsiEncoding.names[231] = "ccedilla";
		m_winAnsiEncoding.names[232] = "egrave";
		m_winAnsiEncoding.names[233] = "eacute";
		m_winAnsiEncoding.names[234] = "ecircumflex";
		m_winAnsiEncoding.names[235] = "edieresis";
		m_winAnsiEncoding.names[236] = "igrave";
		m_winAnsiEncoding.names[237] = "iacute";
		m_winAnsiEncoding.names[238] = "icircumflex";
		m_winAnsiEncoding.names[239] = "idieresis";
		m_winAnsiEncoding.names[240] = "eth";
		m_winAnsiEncoding.names[241] = "ntilde";
		m_winAnsiEncoding.names[242] = "ograve";
		m_winAnsiEncoding.names[243] = "oacute";
		m_winAnsiEncoding.names[244] = "ocircumflex";
		m_winAnsiEncoding.names[245] = "otilde";
		m_winAnsiEncoding.names[246] = "odieresis";
		m_winAnsiEncoding.names[247] = "divide";
		m_winAnsiEncoding.names[248] = "oslash";
		m_winAnsiEncoding.names[249] = "ugrave";
		m_winAnsiEncoding.names[250] = "uacute";
		m_winAnsiEncoding.names[251] = "ucircumflex";
		m_winAnsiEncoding.names[252] = "udieresis";
		m_winAnsiEncoding.names[253] = "yacute";
		m_winAnsiEncoding.names[254] = "thorn";
		m_winAnsiEncoding.names[255] = "ydieresis";
		m_winAnsiEncoding.names[32] = "space";
		m_winAnsiEncoding.names[33] = "exclam";
		m_winAnsiEncoding.names[34] = "quotedbl";
		m_winAnsiEncoding.names[35] = "numbersign";
		m_winAnsiEncoding.names[36] = "dollar";
		m_winAnsiEncoding.names[37] = "percent";
		m_winAnsiEncoding.names[38] = "ampersand";
		m_winAnsiEncoding.names[39] = "quotesingle";
		m_winAnsiEncoding.names[40] = "parenleft";
		m_winAnsiEncoding.names[41] = "parenright";
		m_winAnsiEncoding.names[42] = "asterisk";
		m_winAnsiEncoding.names[43] = "plus";
		m_winAnsiEncoding.names[44] = "comma";
		m_winAnsiEncoding.names[45] = "hyphen";
		m_winAnsiEncoding.names[46] = "period";
		m_winAnsiEncoding.names[47] = "slash";
		m_winAnsiEncoding.names[48] = "zero";
		m_winAnsiEncoding.names[49] = "one";
		m_winAnsiEncoding.names[50] = "two";
		m_winAnsiEncoding.names[51] = "three";
		m_winAnsiEncoding.names[52] = "four";
		m_winAnsiEncoding.names[53] = "five";
		m_winAnsiEncoding.names[54] = "six";
		m_winAnsiEncoding.names[55] = "seven";
		m_winAnsiEncoding.names[56] = "eight";
		m_winAnsiEncoding.names[57] = "nine";
		m_winAnsiEncoding.names[58] = "colon";
		m_winAnsiEncoding.names[59] = "semicolon";
		m_winAnsiEncoding.names[60] = "less";
		m_winAnsiEncoding.names[61] = "equal";
		m_winAnsiEncoding.names[62] = "greater";
		m_winAnsiEncoding.names[63] = "question";
		m_winAnsiEncoding.names[64] = "at";
		m_winAnsiEncoding.names[65] = "A";
		m_winAnsiEncoding.names[66] = "B";
		m_winAnsiEncoding.names[67] = "C";
		m_winAnsiEncoding.names[68] = "D";
		m_winAnsiEncoding.names[69] = "E";
		m_winAnsiEncoding.names[70] = "F";
		m_winAnsiEncoding.names[71] = "G";
		m_winAnsiEncoding.names[72] = "H";
		m_winAnsiEncoding.names[73] = "I";
		m_winAnsiEncoding.names[74] = "J";
		m_winAnsiEncoding.names[75] = "K";
		m_winAnsiEncoding.names[76] = "L";
		m_winAnsiEncoding.names[77] = "M";
		m_winAnsiEncoding.names[78] = "N";
		m_winAnsiEncoding.names[79] = "O";
		m_winAnsiEncoding.names[80] = "P";
		m_winAnsiEncoding.names[81] = "Q";
		m_winAnsiEncoding.names[82] = "R";
		m_winAnsiEncoding.names[83] = "S";
		m_winAnsiEncoding.names[84] = "T";
		m_winAnsiEncoding.names[85] = "U";
		m_winAnsiEncoding.names[86] = "V";
		m_winAnsiEncoding.names[87] = "W";
		m_winAnsiEncoding.names[88] = "X";
		m_winAnsiEncoding.names[89] = "Y";
		m_winAnsiEncoding.names[90] = "Z";
		m_winAnsiEncoding.names[91] = "bracketleft";
		m_winAnsiEncoding.names[92] = "backslash";
		m_winAnsiEncoding.names[93] = "bracketright";
		m_winAnsiEncoding.names[94] = "asciicircum";
		m_winAnsiEncoding.names[95] = "underscore";
		m_winAnsiEncoding.names[96] = "grave";
		m_winAnsiEncoding.names[97] = "a";
		m_winAnsiEncoding.names[98] = "b";
		m_winAnsiEncoding.names[99] = "c";
		m_winAnsiEncoding.InitMapping();
	}

	private static void InitializeMacEncoding()
	{
		m_macRomanEncoding = new PredefinedEncoding();
		m_macRomanEncoding.Init();
		m_macRomanEncoding.m_name = "MacRomanEncoding";
		m_macRomanEncoding.names[202] = "space";
		m_macRomanEncoding.names[224] = "adblgrave";
		m_macRomanEncoding.names[100] = "d";
		m_macRomanEncoding.names[101] = "e";
		m_macRomanEncoding.names[102] = "f";
		m_macRomanEncoding.names[103] = "g";
		m_macRomanEncoding.names[104] = "h";
		m_macRomanEncoding.names[105] = "i";
		m_macRomanEncoding.names[106] = "j";
		m_macRomanEncoding.names[107] = "k";
		m_macRomanEncoding.names[108] = "l";
		m_macRomanEncoding.names[109] = "m";
		m_macRomanEncoding.names[110] = "n";
		m_macRomanEncoding.names[111] = "o";
		m_macRomanEncoding.names[112] = "p";
		m_macRomanEncoding.names[113] = "q";
		m_macRomanEncoding.names[114] = "r";
		m_macRomanEncoding.names[115] = "s";
		m_macRomanEncoding.names[116] = "t";
		m_macRomanEncoding.names[117] = "u";
		m_macRomanEncoding.names[118] = "v";
		m_macRomanEncoding.names[119] = "w";
		m_macRomanEncoding.names[120] = "x";
		m_macRomanEncoding.names[121] = "y";
		m_macRomanEncoding.names[122] = "z";
		m_macRomanEncoding.names[123] = "braceleft";
		m_macRomanEncoding.names[124] = "bar";
		m_macRomanEncoding.names[125] = "braceright";
		m_macRomanEncoding.names[126] = "asciitilde";
		m_macRomanEncoding.names[128] = "Adieresis";
		m_macRomanEncoding.names[129] = "Aring";
		m_macRomanEncoding.names[130] = "Ccedilla";
		m_macRomanEncoding.names[131] = "Eacute";
		m_macRomanEncoding.names[132] = "Ograve";
		m_macRomanEncoding.names[133] = "Odieresis";
		m_macRomanEncoding.names[134] = "Udieresis";
		m_macRomanEncoding.names[135] = "aacute";
		m_macRomanEncoding.names[137] = "acircumflex";
		m_macRomanEncoding.names[137] = "agrave";
		m_macRomanEncoding.names[138] = "adieresis";
		m_macRomanEncoding.names[139] = "atilde";
		m_macRomanEncoding.names[140] = "aring";
		m_macRomanEncoding.names[141] = "ccedilla";
		m_macRomanEncoding.names[142] = "eacute";
		m_macRomanEncoding.names[143] = "egrave";
		m_macRomanEncoding.names[144] = "ecircumflex";
		m_macRomanEncoding.names[145] = "edieresis";
		m_macRomanEncoding.names[146] = "iacute";
		m_macRomanEncoding.names[147] = "igrave";
		m_macRomanEncoding.names[148] = "icircumflex";
		m_macRomanEncoding.names[149] = "idieresis";
		m_macRomanEncoding.names[150] = "ntilde";
		m_macRomanEncoding.names[151] = "oacute";
		m_macRomanEncoding.names[152] = "ograve";
		m_macRomanEncoding.names[153] = "ocircumflex";
		m_macRomanEncoding.names[154] = "odieresis";
		m_macRomanEncoding.names[155] = "otilde";
		m_macRomanEncoding.names[156] = "uacute";
		m_macRomanEncoding.names[157] = "ugrave";
		m_macRomanEncoding.names[158] = "ucircumflex";
		m_macRomanEncoding.names[159] = "udieresis";
		m_macRomanEncoding.names[160] = "dagger";
		m_macRomanEncoding.names[161] = "degree";
		m_macRomanEncoding.names[162] = "cent";
		m_macRomanEncoding.names[163] = "sterling";
		m_macRomanEncoding.names[164] = "section";
		m_macRomanEncoding.names[165] = "bullet";
		m_macRomanEncoding.names[166] = "paragraph";
		m_macRomanEncoding.names[167] = "germandbls";
		m_macRomanEncoding.names[168] = "registered";
		m_macRomanEncoding.names[169] = "copyright";
		m_macRomanEncoding.names[170] = "trademark";
		m_macRomanEncoding.names[171] = "acute";
		m_macRomanEncoding.names[172] = "dieresis";
		m_macRomanEncoding.names[174] = "AE";
		m_macRomanEncoding.names[175] = "Oslash";
		m_macRomanEncoding.names[177] = "plusminus";
		m_macRomanEncoding.names[180] = "yen";
		m_macRomanEncoding.names[181] = "mu";
		m_macRomanEncoding.names[187] = "ordfeminine";
		m_macRomanEncoding.names[188] = "ordmasculine";
		m_macRomanEncoding.names[190] = "ae";
		m_macRomanEncoding.names[191] = "oslash";
		m_macRomanEncoding.names[192] = "questiondown";
		m_macRomanEncoding.names[193] = "exclamdown";
		m_macRomanEncoding.names[194] = "logicalnot";
		m_macRomanEncoding.names[196] = "florin";
		m_macRomanEncoding.names[199] = "guillemotleft";
		m_macRomanEncoding.names[200] = "guillemotright";
		m_macRomanEncoding.names[201] = "ellipsis";
		m_macRomanEncoding.names[203] = "Agrave";
		m_macRomanEncoding.names[204] = "Atilde";
		m_macRomanEncoding.names[205] = "Otilde";
		m_macRomanEncoding.names[206] = "OE";
		m_macRomanEncoding.names[207] = "oe";
		m_macRomanEncoding.names[208] = "endash";
		m_macRomanEncoding.names[209] = "emdash";
		m_macRomanEncoding.names[210] = "quotedblleft";
		m_macRomanEncoding.names[211] = "quotedblright";
		m_macRomanEncoding.names[212] = "quoteleft";
		m_macRomanEncoding.names[213] = "quoteright";
		m_macRomanEncoding.names[214] = "divide";
		m_macRomanEncoding.names[216] = "ydieresis";
		m_macRomanEncoding.names[217] = "Ydieresis";
		m_macRomanEncoding.names[218] = "fraction";
		m_macRomanEncoding.names[219] = "currency";
		m_macRomanEncoding.names[220] = "guilsinglleft";
		m_macRomanEncoding.names[221] = "guilsinglright";
		m_macRomanEncoding.names[222] = "fi";
		m_macRomanEncoding.names[223] = "fl";
		m_macRomanEncoding.names[225] = "middot";
		m_macRomanEncoding.names[226] = "quotesinglbase";
		m_macRomanEncoding.names[227] = "quotedblbase";
		m_macRomanEncoding.names[228] = "perthousand";
		m_macRomanEncoding.names[229] = "Acircumflex";
		m_macRomanEncoding.names[230] = "Ecircumflex";
		m_macRomanEncoding.names[231] = "Aacute";
		m_macRomanEncoding.names[232] = "Edieresis";
		m_macRomanEncoding.names[233] = "Egrave";
		m_macRomanEncoding.names[234] = "Iacute";
		m_macRomanEncoding.names[235] = "Icircumflex";
		m_macRomanEncoding.names[236] = "Idieresis";
		m_macRomanEncoding.names[237] = "Igrave";
		m_macRomanEncoding.names[238] = "Oacute";
		m_macRomanEncoding.names[239] = "Ocircumflex";
		m_macRomanEncoding.names[241] = "Ntilde";
		m_macRomanEncoding.names[242] = "Uacute";
		m_macRomanEncoding.names[243] = "Ucircumflex";
		m_macRomanEncoding.names[244] = "Ugrave";
		m_macRomanEncoding.names[245] = "dotlessi";
		m_macRomanEncoding.names[246] = "circumflex";
		m_macRomanEncoding.names[247] = "ilde";
		m_macRomanEncoding.names[248] = "macron";
		m_macRomanEncoding.names[249] = "breve";
		m_macRomanEncoding.names[250] = "dotaccent";
		m_macRomanEncoding.names[251] = "ring";
		m_macRomanEncoding.names[252] = "cedilla";
		m_macRomanEncoding.names[253] = "hungarumlaut";
		m_macRomanEncoding.names[254] = "ogonek";
		m_macRomanEncoding.names[255] = "caron";
		m_macRomanEncoding.names[32] = "space";
		m_macRomanEncoding.names[33] = "exclam";
		m_macRomanEncoding.names[34] = "quotedbl";
		m_macRomanEncoding.names[35] = "numbersign";
		m_macRomanEncoding.names[36] = "dollar";
		m_macRomanEncoding.names[37] = "percent";
		m_macRomanEncoding.names[38] = "ampersand";
		m_macRomanEncoding.names[39] = "quotesingle";
		m_macRomanEncoding.names[40] = "parenleft";
		m_macRomanEncoding.names[41] = "parenright";
		m_macRomanEncoding.names[42] = "asterisk";
		m_macRomanEncoding.names[43] = "plus";
		m_macRomanEncoding.names[44] = "comma";
		m_macRomanEncoding.names[45] = "hyphen";
		m_macRomanEncoding.names[46] = "period";
		m_macRomanEncoding.names[47] = "slash";
		m_macRomanEncoding.names[48] = "zero";
		m_macRomanEncoding.names[49] = "one";
		m_macRomanEncoding.names[50] = "two";
		m_macRomanEncoding.names[51] = "three";
		m_macRomanEncoding.names[52] = "four";
		m_macRomanEncoding.names[53] = "five";
		m_macRomanEncoding.names[54] = "six";
		m_macRomanEncoding.names[55] = "seven";
		m_macRomanEncoding.names[56] = "eight";
		m_macRomanEncoding.names[57] = "nine";
		m_macRomanEncoding.names[58] = "colon";
		m_macRomanEncoding.names[59] = "semicolon";
		m_macRomanEncoding.names[60] = "less";
		m_macRomanEncoding.names[61] = "equal";
		m_macRomanEncoding.names[62] = "greater";
		m_macRomanEncoding.names[63] = "question";
		m_macRomanEncoding.names[64] = "at";
		m_macRomanEncoding.names[65] = "A";
		m_macRomanEncoding.names[66] = "B";
		m_macRomanEncoding.names[67] = "C";
		m_macRomanEncoding.names[68] = "D";
		m_macRomanEncoding.names[69] = "E";
		m_macRomanEncoding.names[70] = "F";
		m_macRomanEncoding.names[71] = "G";
		m_macRomanEncoding.names[72] = "H";
		m_macRomanEncoding.names[73] = "I";
		m_macRomanEncoding.names[74] = "J";
		m_macRomanEncoding.names[75] = "K";
		m_macRomanEncoding.names[76] = "L";
		m_macRomanEncoding.names[77] = "M";
		m_macRomanEncoding.names[78] = "N";
		m_macRomanEncoding.names[79] = "O";
		m_macRomanEncoding.names[80] = "P";
		m_macRomanEncoding.names[81] = "Q";
		m_macRomanEncoding.names[82] = "R";
		m_macRomanEncoding.names[83] = "S";
		m_macRomanEncoding.names[84] = "T";
		m_macRomanEncoding.names[85] = "U";
		m_macRomanEncoding.names[86] = "V";
		m_macRomanEncoding.names[87] = "W";
		m_macRomanEncoding.names[88] = "X";
		m_macRomanEncoding.names[89] = "Y";
		m_macRomanEncoding.names[90] = "Z";
		m_macRomanEncoding.names[91] = "bracketleft";
		m_macRomanEncoding.names[92] = "backslash";
		m_macRomanEncoding.names[93] = "bracketright";
		m_macRomanEncoding.names[94] = "asciicircum";
		m_macRomanEncoding.names[95] = "underscore";
		m_macRomanEncoding.names[96] = "grave";
		m_macRomanEncoding.names[97] = "a";
		m_macRomanEncoding.names[98] = "b";
		m_macRomanEncoding.names[99] = "c";
		m_macRomanEncoding.InitMapping();
	}

	private static void InitializeStandardMacEncoding()
	{
		StandardMacRomanEncoding = new PredefinedEncoding();
		m_standardMacRomanEncoding.Init();
		m_standardMacRomanEncoding.m_name = "MacRomanEncoding";
		StandardMacRomanEncoding.names[224] = "adblgrave";
		m_standardMacRomanEncoding.names[100] = "d";
		m_standardMacRomanEncoding.names[101] = "e";
		m_standardMacRomanEncoding.names[102] = "f";
		m_standardMacRomanEncoding.names[103] = "g";
		m_standardMacRomanEncoding.names[104] = "h";
		m_standardMacRomanEncoding.names[105] = "i";
		m_standardMacRomanEncoding.names[106] = "j";
		m_standardMacRomanEncoding.names[107] = "k";
		m_standardMacRomanEncoding.names[108] = "l";
		m_standardMacRomanEncoding.names[109] = "m";
		m_standardMacRomanEncoding.names[110] = "n";
		m_standardMacRomanEncoding.names[111] = "o";
		m_standardMacRomanEncoding.names[112] = "p";
		m_standardMacRomanEncoding.names[113] = "q";
		m_standardMacRomanEncoding.names[114] = "r";
		m_standardMacRomanEncoding.names[115] = "s";
		m_standardMacRomanEncoding.names[116] = "t";
		m_standardMacRomanEncoding.names[117] = "u";
		m_standardMacRomanEncoding.names[118] = "v";
		m_standardMacRomanEncoding.names[119] = "w";
		m_standardMacRomanEncoding.names[120] = "x";
		m_standardMacRomanEncoding.names[121] = "y";
		m_standardMacRomanEncoding.names[122] = "z";
		m_standardMacRomanEncoding.names[123] = "braceleft";
		m_standardMacRomanEncoding.names[124] = "bar";
		m_standardMacRomanEncoding.names[125] = "braceright";
		m_standardMacRomanEncoding.names[126] = "asciitilde";
		m_standardMacRomanEncoding.names[128] = "Adieresis";
		m_standardMacRomanEncoding.names[129] = "Aring";
		m_standardMacRomanEncoding.names[130] = "Ccedilla";
		m_standardMacRomanEncoding.names[131] = "Eacute";
		m_standardMacRomanEncoding.names[132] = "Ograve";
		m_standardMacRomanEncoding.names[133] = "Odieresis";
		m_standardMacRomanEncoding.names[134] = "Udieresis";
		m_standardMacRomanEncoding.names[135] = "aacute";
		m_standardMacRomanEncoding.names[137] = "acircumflex";
		m_standardMacRomanEncoding.names[137] = "agrave";
		m_standardMacRomanEncoding.names[138] = "adieresis";
		m_standardMacRomanEncoding.names[139] = "atilde";
		m_standardMacRomanEncoding.names[140] = "aring";
		m_standardMacRomanEncoding.names[141] = "ccedilla";
		m_standardMacRomanEncoding.names[142] = "eacute";
		m_standardMacRomanEncoding.names[143] = "egrave";
		m_standardMacRomanEncoding.names[144] = "ecircumflex";
		m_standardMacRomanEncoding.names[145] = "edieresis";
		m_standardMacRomanEncoding.names[146] = "iacute";
		m_standardMacRomanEncoding.names[147] = "igrave";
		m_standardMacRomanEncoding.names[148] = "icircumflex";
		m_standardMacRomanEncoding.names[149] = "idieresis";
		m_standardMacRomanEncoding.names[150] = "ntilde";
		m_standardMacRomanEncoding.names[151] = "oacute";
		m_standardMacRomanEncoding.names[152] = "ograve";
		m_standardMacRomanEncoding.names[153] = "ocircumflex";
		m_standardMacRomanEncoding.names[154] = "odieresis";
		m_standardMacRomanEncoding.names[155] = "otilde";
		m_standardMacRomanEncoding.names[156] = "uacute";
		m_standardMacRomanEncoding.names[157] = "ugrave";
		m_standardMacRomanEncoding.names[158] = "ucircumflex";
		m_standardMacRomanEncoding.names[159] = "udieresis";
		m_standardMacRomanEncoding.names[160] = "dagger";
		m_standardMacRomanEncoding.names[161] = "degree";
		m_standardMacRomanEncoding.names[162] = "cent";
		m_standardMacRomanEncoding.names[163] = "sterling";
		m_standardMacRomanEncoding.names[164] = "section";
		m_standardMacRomanEncoding.names[165] = "bullet";
		m_standardMacRomanEncoding.names[166] = "paragraph";
		m_standardMacRomanEncoding.names[167] = "germandbls";
		m_standardMacRomanEncoding.names[168] = "registered";
		m_standardMacRomanEncoding.names[169] = "copyright";
		m_standardMacRomanEncoding.names[170] = "trademark";
		m_standardMacRomanEncoding.names[171] = "acute";
		m_standardMacRomanEncoding.names[172] = "dieresis";
		m_standardMacRomanEncoding.names[174] = "AE";
		m_standardMacRomanEncoding.names[175] = "Oslash";
		m_standardMacRomanEncoding.names[177] = "plusminus";
		m_standardMacRomanEncoding.names[180] = "yen";
		m_standardMacRomanEncoding.names[181] = "mu";
		m_standardMacRomanEncoding.names[187] = "ordfeminine";
		m_standardMacRomanEncoding.names[188] = "ordmasculine";
		m_standardMacRomanEncoding.names[190] = "ae";
		m_standardMacRomanEncoding.names[191] = "oslash";
		m_standardMacRomanEncoding.names[192] = "questiondown";
		m_standardMacRomanEncoding.names[193] = "exclamdown";
		m_standardMacRomanEncoding.names[194] = "logicalnot";
		m_standardMacRomanEncoding.names[196] = "florin";
		m_standardMacRomanEncoding.names[199] = "guillemotleft";
		m_standardMacRomanEncoding.names[200] = "guillemotright";
		m_standardMacRomanEncoding.names[201] = "ellipsis";
		m_standardMacRomanEncoding.names[203] = "Agrave";
		m_standardMacRomanEncoding.names[204] = "Atilde";
		m_standardMacRomanEncoding.names[205] = "Otilde";
		m_standardMacRomanEncoding.names[206] = "OE";
		m_standardMacRomanEncoding.names[207] = "oe";
		m_standardMacRomanEncoding.names[208] = "endash";
		m_standardMacRomanEncoding.names[209] = "emdash";
		m_standardMacRomanEncoding.names[210] = "quotedblleft";
		m_standardMacRomanEncoding.names[211] = "quotedblright";
		m_standardMacRomanEncoding.names[212] = "quoteleft";
		m_standardMacRomanEncoding.names[213] = "quoteright";
		m_standardMacRomanEncoding.names[214] = "divide";
		m_standardMacRomanEncoding.names[216] = "ydieresis";
		m_standardMacRomanEncoding.names[217] = "Ydieresis";
		m_standardMacRomanEncoding.names[218] = "fraction";
		m_standardMacRomanEncoding.names[220] = "guilsinglleft";
		m_standardMacRomanEncoding.names[221] = "guilsinglright";
		m_standardMacRomanEncoding.names[222] = "fi";
		m_standardMacRomanEncoding.names[223] = "fl";
		m_standardMacRomanEncoding.names[225] = "middot";
		m_standardMacRomanEncoding.names[226] = "quotesinglbase";
		m_standardMacRomanEncoding.names[227] = "quotedblbase";
		m_standardMacRomanEncoding.names[228] = "perthousand";
		m_standardMacRomanEncoding.names[229] = "Acircumflex";
		m_standardMacRomanEncoding.names[230] = "Ecircumflex";
		m_standardMacRomanEncoding.names[231] = "Aacute";
		m_standardMacRomanEncoding.names[232] = "Edieresis";
		m_standardMacRomanEncoding.names[233] = "Egrave";
		m_standardMacRomanEncoding.names[234] = "Iacute";
		m_standardMacRomanEncoding.names[235] = "Icircumflex";
		m_standardMacRomanEncoding.names[236] = "Idieresis";
		m_standardMacRomanEncoding.names[237] = "Igrave";
		m_standardMacRomanEncoding.names[238] = "Oacute";
		m_standardMacRomanEncoding.names[239] = "Ocircumflex";
		m_standardMacRomanEncoding.names[241] = "Ntilde";
		m_standardMacRomanEncoding.names[242] = "Uacute";
		m_standardMacRomanEncoding.names[243] = "Ucircumflex";
		m_standardMacRomanEncoding.names[244] = "Ugrave";
		m_standardMacRomanEncoding.names[245] = "dotlessi";
		m_standardMacRomanEncoding.names[246] = "circumflex";
		m_standardMacRomanEncoding.names[247] = "ilde";
		m_standardMacRomanEncoding.names[248] = "macron";
		m_standardMacRomanEncoding.names[249] = "breve";
		m_standardMacRomanEncoding.names[250] = "dotaccent";
		m_standardMacRomanEncoding.names[251] = "ring";
		m_standardMacRomanEncoding.names[252] = "cedilla";
		m_standardMacRomanEncoding.names[253] = "hungarumlaut";
		m_standardMacRomanEncoding.names[254] = "ogonek";
		m_standardMacRomanEncoding.names[255] = "caron";
		m_standardMacRomanEncoding.names[32] = "space";
		m_standardMacRomanEncoding.names[33] = "exclam";
		m_standardMacRomanEncoding.names[34] = "quotedbl";
		m_standardMacRomanEncoding.names[35] = "numbersign";
		m_standardMacRomanEncoding.names[36] = "dollar";
		m_standardMacRomanEncoding.names[37] = "percent";
		m_standardMacRomanEncoding.names[38] = "ampersand";
		m_standardMacRomanEncoding.names[39] = "quotesingle";
		m_standardMacRomanEncoding.names[40] = "parenleft";
		m_standardMacRomanEncoding.names[41] = "parenright";
		m_standardMacRomanEncoding.names[42] = "asterisk";
		m_standardMacRomanEncoding.names[43] = "plus";
		m_standardMacRomanEncoding.names[44] = "comma";
		m_standardMacRomanEncoding.names[45] = "hyphen";
		m_standardMacRomanEncoding.names[46] = "period";
		m_standardMacRomanEncoding.names[47] = "slash";
		m_standardMacRomanEncoding.names[48] = "zero";
		m_standardMacRomanEncoding.names[49] = "one";
		m_standardMacRomanEncoding.names[50] = "two";
		m_standardMacRomanEncoding.names[51] = "three";
		m_standardMacRomanEncoding.names[52] = "four";
		m_standardMacRomanEncoding.names[53] = "five";
		m_standardMacRomanEncoding.names[54] = "six";
		m_standardMacRomanEncoding.names[55] = "seven";
		m_standardMacRomanEncoding.names[56] = "eight";
		m_standardMacRomanEncoding.names[57] = "nine";
		m_standardMacRomanEncoding.names[58] = "colon";
		m_standardMacRomanEncoding.names[59] = "semicolon";
		m_standardMacRomanEncoding.names[60] = "less";
		m_standardMacRomanEncoding.names[61] = "equal";
		m_standardMacRomanEncoding.names[62] = "greater";
		m_standardMacRomanEncoding.names[63] = "question";
		m_standardMacRomanEncoding.names[64] = "at";
		m_standardMacRomanEncoding.names[65] = "A";
		m_standardMacRomanEncoding.names[66] = "B";
		m_standardMacRomanEncoding.names[67] = "C";
		m_standardMacRomanEncoding.names[68] = "D";
		m_standardMacRomanEncoding.names[69] = "E";
		m_standardMacRomanEncoding.names[70] = "F";
		m_standardMacRomanEncoding.names[71] = "G";
		m_standardMacRomanEncoding.names[72] = "H";
		m_standardMacRomanEncoding.names[73] = "I";
		m_standardMacRomanEncoding.names[74] = "J";
		m_standardMacRomanEncoding.names[75] = "K";
		m_standardMacRomanEncoding.names[76] = "L";
		m_standardMacRomanEncoding.names[77] = "M";
		m_standardMacRomanEncoding.names[78] = "N";
		m_standardMacRomanEncoding.names[79] = "O";
		m_standardMacRomanEncoding.names[80] = "P";
		m_standardMacRomanEncoding.names[81] = "Q";
		m_standardMacRomanEncoding.names[82] = "R";
		m_standardMacRomanEncoding.names[83] = "S";
		m_standardMacRomanEncoding.names[84] = "T";
		m_standardMacRomanEncoding.names[85] = "U";
		m_standardMacRomanEncoding.names[86] = "V";
		m_standardMacRomanEncoding.names[87] = "W";
		m_standardMacRomanEncoding.names[88] = "X";
		m_standardMacRomanEncoding.names[89] = "Y";
		m_standardMacRomanEncoding.names[90] = "Z";
		m_standardMacRomanEncoding.names[91] = "bracketleft";
		m_standardMacRomanEncoding.names[92] = "backslash";
		m_standardMacRomanEncoding.names[93] = "bracketright";
		m_standardMacRomanEncoding.names[94] = "asciicircum";
		m_standardMacRomanEncoding.names[95] = "underscore";
		m_standardMacRomanEncoding.names[96] = "grave";
		m_standardMacRomanEncoding.names[97] = "a";
		m_standardMacRomanEncoding.names[98] = "b";
		m_standardMacRomanEncoding.names[99] = "c";
		m_standardMacRomanEncoding.names[173] = "notequal";
		m_standardMacRomanEncoding.names[176] = "infinity";
		m_standardMacRomanEncoding.names[178] = "lessequal";
		m_standardMacRomanEncoding.names[179] = "greaterequal";
		m_standardMacRomanEncoding.names[182] = "partialdiff";
		m_standardMacRomanEncoding.names[183] = "summation";
		m_standardMacRomanEncoding.names[184] = "product";
		m_standardMacRomanEncoding.names[185] = "pi";
		m_standardMacRomanEncoding.names[186] = "integral";
		m_standardMacRomanEncoding.names[189] = "Omega";
		m_standardMacRomanEncoding.names[195] = "radical";
		m_standardMacRomanEncoding.names[197] = "approxequal";
		m_standardMacRomanEncoding.names[198] = "Delta";
		StandardMacRomanEncoding.names[215] = "lozenge";
		StandardMacRomanEncoding.names[219] = "Euro";
		m_standardMacRomanEncoding.names[240] = "apple";
		m_standardMacRomanEncoding.InitMapping();
	}

	public static PredefinedEncoding GetPredefinedEncoding(string encoding)
	{
		return encoding switch
		{
			"m_pdfdocEncoding" => m_pdfdocEncoding, 
			"MacRomanEncoding" => MacRomanEncoding, 
			"WinAnsiEncoding" => WinAnsiEncoding, 
			"StandardEncoding" => StandardEncoding, 
			_ => null, 
		};
	}

	static PredefinedEncoding()
	{
		InitializePdfEncoding();
		InitializeMacEncoding();
		InitializeStandardMacEncoding();
		InitializeWinAnsiEncoding();
	}

	private void Init()
	{
		names = new string[256];
		for (int i = 0; i < 256; i++)
		{
			names[i] = ".notdef";
		}
	}

	private void InitMapping()
	{
		if (mapping != null)
		{
			return;
		}
		mapping = new Dictionary<string, byte>(names.Length);
		for (int i = 0; i < names.Length; i++)
		{
			if (names[i] != null)
			{
				mapping[names[i]] = (byte)i;
			}
		}
	}

	public string[] GetNames()
	{
		return (string[])names.Clone();
	}

	public byte GetCharId(string name)
	{
		return mapping[name];
	}
}
