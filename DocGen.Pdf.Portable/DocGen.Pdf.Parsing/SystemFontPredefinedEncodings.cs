namespace DocGen.Pdf.Parsing;

internal static class SystemFontPredefinedEncodings
{
	public static string[] StandardEncoding { get; private set; }

	private static void InitializeStandardEncoding()
	{
		StandardEncoding = new string[256];
		StandardEncoding[179] = "adblgrave";
		StandardEncoding[100] = "d";
		StandardEncoding[101] = "e";
		StandardEncoding[102] = "f";
		StandardEncoding[103] = "g";
		StandardEncoding[104] = "h";
		StandardEncoding[105] = "i";
		StandardEncoding[106] = "j";
		StandardEncoding[107] = "k";
		StandardEncoding[108] = "l";
		StandardEncoding[109] = "m";
		StandardEncoding[110] = "n";
		StandardEncoding[111] = "o";
		StandardEncoding[112] = "p";
		StandardEncoding[113] = "q";
		StandardEncoding[114] = "r";
		StandardEncoding[115] = "s";
		StandardEncoding[116] = "t";
		StandardEncoding[117] = "u";
		StandardEncoding[118] = "v";
		StandardEncoding[119] = "w";
		StandardEncoding[120] = "x";
		StandardEncoding[121] = "y";
		StandardEncoding[122] = "z";
		StandardEncoding[123] = "braceleft";
		StandardEncoding[124] = "bar";
		StandardEncoding[125] = "braceright";
		StandardEncoding[126] = "asciitilde";
		StandardEncoding[161] = "exclamdown";
		StandardEncoding[162] = "cent";
		StandardEncoding[163] = "sterling";
		StandardEncoding[164] = "fraction";
		StandardEncoding[165] = "yen";
		StandardEncoding[166] = "florin";
		StandardEncoding[167] = "section";
		StandardEncoding[168] = "currency";
		StandardEncoding[169] = "quotesingle";
		StandardEncoding[170] = "quotedblleft";
		StandardEncoding[171] = "guillemotleft";
		StandardEncoding[172] = "guilsinglleft";
		StandardEncoding[173] = "guilsinglright";
		StandardEncoding[174] = "fi";
		StandardEncoding[175] = "fl";
		StandardEncoding[177] = "endash";
		StandardEncoding[178] = "dagger";
		StandardEncoding[180] = "middot";
		StandardEncoding[182] = "paragraph";
		StandardEncoding[183] = "bullet";
		StandardEncoding[184] = "quotesinglbase";
		StandardEncoding[185] = "quotedblbase";
		StandardEncoding[186] = "quotedblright";
		StandardEncoding[187] = "guillemotright";
		StandardEncoding[188] = "ellipsis";
		StandardEncoding[189] = "perthousand";
		StandardEncoding[191] = "questiondown";
		StandardEncoding[193] = "grave";
		StandardEncoding[194] = "acute";
		StandardEncoding[195] = "circumflex";
		StandardEncoding[196] = "ilde";
		StandardEncoding[197] = "macron";
		StandardEncoding[198] = "breve";
		StandardEncoding[199] = "dotaccent";
		StandardEncoding[200] = "dieresis";
		StandardEncoding[202] = "ring";
		StandardEncoding[203] = "cedilla";
		StandardEncoding[205] = "hungarumlaut";
		StandardEncoding[206] = "ogonek";
		StandardEncoding[207] = "caron";
		StandardEncoding[208] = "emdash";
		StandardEncoding[225] = "AE";
		StandardEncoding[227] = "ordfeminine";
		StandardEncoding[232] = "Lslash";
		StandardEncoding[233] = "Oslash";
		StandardEncoding[234] = "OE";
		StandardEncoding[235] = "ordmasculine";
		StandardEncoding[241] = "ae";
		StandardEncoding[245] = "dotlessi";
		StandardEncoding[248] = "lslash";
		StandardEncoding[249] = "oslash";
		StandardEncoding[250] = "oe";
		StandardEncoding[251] = "germandbls";
		StandardEncoding[32] = "space";
		StandardEncoding[33] = "exclam";
		StandardEncoding[34] = "quotedbl";
		StandardEncoding[35] = "numbersign";
		StandardEncoding[36] = "dollar";
		StandardEncoding[37] = "percent";
		StandardEncoding[38] = "ampersand";
		StandardEncoding[39] = "quoteright";
		StandardEncoding[40] = "parenleft";
		StandardEncoding[41] = "parenright";
		StandardEncoding[42] = "asterisk";
		StandardEncoding[43] = "plus";
		StandardEncoding[44] = "comma";
		StandardEncoding[45] = "hyphen";
		StandardEncoding[46] = "period";
		StandardEncoding[47] = "slash";
		StandardEncoding[48] = "zero";
		StandardEncoding[49] = "one";
		StandardEncoding[50] = "two";
		StandardEncoding[51] = "three";
		StandardEncoding[52] = "four";
		StandardEncoding[53] = "five";
		StandardEncoding[54] = "six";
		StandardEncoding[55] = "seven";
		StandardEncoding[56] = "eight";
		StandardEncoding[57] = "nine";
		StandardEncoding[58] = "colon";
		StandardEncoding[59] = "semicolon";
		StandardEncoding[60] = "less";
		StandardEncoding[61] = "equal";
		StandardEncoding[62] = "greater";
		StandardEncoding[63] = "question";
		StandardEncoding[64] = "at";
		StandardEncoding[65] = "A";
		StandardEncoding[66] = "B";
		StandardEncoding[67] = "C";
		StandardEncoding[68] = "D";
		StandardEncoding[69] = "E";
		StandardEncoding[70] = "F";
		StandardEncoding[71] = "G";
		StandardEncoding[72] = "H";
		StandardEncoding[73] = "I";
		StandardEncoding[74] = "J";
		StandardEncoding[75] = "K";
		StandardEncoding[76] = "L";
		StandardEncoding[77] = "M";
		StandardEncoding[78] = "N";
		StandardEncoding[79] = "O";
		StandardEncoding[80] = "P";
		StandardEncoding[81] = "Q";
		StandardEncoding[82] = "R";
		StandardEncoding[83] = "S";
		StandardEncoding[84] = "T";
		StandardEncoding[85] = "U";
		StandardEncoding[86] = "V";
		StandardEncoding[87] = "W";
		StandardEncoding[88] = "X";
		StandardEncoding[89] = "Y";
		StandardEncoding[90] = "Z";
		StandardEncoding[91] = "bracketleft";
		StandardEncoding[92] = "backslash";
		StandardEncoding[93] = "bracketright";
		StandardEncoding[94] = "asciicircum";
		StandardEncoding[95] = "underscore";
		StandardEncoding[96] = "quoteleft";
		StandardEncoding[97] = "a";
		StandardEncoding[98] = "b";
		StandardEncoding[99] = "c";
	}

	static SystemFontPredefinedEncodings()
	{
		InitializeStandardEncoding();
	}

	public static SystemFontPostScriptArray CreateEncoding(string predefinedEncoding)
	{
		string[] array = null;
		if (predefinedEncoding != null && predefinedEncoding == "StandardEncoding")
		{
			array = StandardEncoding;
		}
		if (array == null)
		{
			return null;
		}
		SystemFontPostScriptArray systemFontPostScriptArray = new SystemFontPostScriptArray(array.Length);
		string[] array2 = array;
		foreach (string item in array2)
		{
			systemFontPostScriptArray.Add(item);
		}
		return systemFontPostScriptArray;
	}
}
