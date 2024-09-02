namespace DocGen.Pdf.Parsing;

internal static class SystemFontsTags
{
	internal static readonly uint DEFAULT_TABLE_SCRIPT_TAG = GetTagFromString("DFLT");

	internal static readonly uint CFF_TABLE = GetTagFromString("CFF ");

	internal static readonly uint CMAP_TABLE = GetTagFromString("cmap");

	internal static readonly uint GLYF_TABLE = GetTagFromString("glyf");

	internal static readonly uint MAXP_TABLE = GetTagFromString("maxp");

	internal static readonly uint LOCA_TABLE = GetTagFromString("loca");

	internal static readonly uint HEAD_TABLE = GetTagFromString("head");

	internal static readonly uint HHEA_TABLE = GetTagFromString("hhea");

	internal static readonly uint HMTX_TABLE = GetTagFromString("hmtx");

	internal static readonly uint KERN_TABLE = GetTagFromString("kern");

	internal static readonly uint GSUB_TABLE = GetTagFromString("GSUB");

	internal static readonly uint NAME_TABLE = GetTagFromString("name");

	internal static readonly uint OS_2_TABLE = GetTagFromString("OS/2");

	internal static readonly uint POST_TABLE = GetTagFromString("post");

	internal static readonly uint OTTO_TAG = GetTagFromString("OTTO");

	internal static readonly uint NULL_TAG = GetTagFromString("NULL");

	internal static readonly ushort NULL_TYPE = 255;

	internal static readonly uint TRUE_TYPE_COLLECTION = GetTagFromString("ttcf");

	internal static readonly uint FEATURE_ACCESS_ALL_ALTERNATES = GetTagFromString("aalt");

	internal static readonly uint FEATURE_ABOVE_BASE_FORMS = GetTagFromString("abvf");

	internal static readonly uint FEATURE_ABOVE_BASE_MARK_POSITIONING = GetTagFromString("abvm");

	internal static readonly uint FEATURE_ABOVE_BASE_SUBSTITUTIONS = GetTagFromString("abvs");

	internal static readonly uint FEATURE_ALTERNATIVE_FRACTIONS = GetTagFromString("afrc");

	internal static readonly uint FEATURE_AKHANDS = GetTagFromString("akhn");

	internal static readonly uint FEATURE_BELOW_BASE_FORMS = GetTagFromString("blwf");

	internal static readonly uint FEATURE_BELOW_BASE_MARK_POSITIONING = GetTagFromString("blwm");

	internal static readonly uint FEATURE_BELOW_BASE_SUBSTITUTIONS = GetTagFromString("blws");

	internal static readonly uint FEATURE_CONTEXTUAL_ALTERNATES = GetTagFromString("calt");

	internal static readonly uint FEATURE_CASE_SENSITIVE_FORMS = GetTagFromString("case");

	internal static readonly uint FEATURE_GLYPH_COMPOSITION_DECOMPOSITION = GetTagFromString("ccmp");

	internal static readonly uint FEATURE_CONJUNCT_FORM_AFTER_RO = GetTagFromString("cfar");

	internal static readonly uint FEATURE_CONJUNCT_FORMS = GetTagFromString("cjct");

	internal static readonly uint FEATURE_CONTEXTUAL_LIGATURES = GetTagFromString("clig");

	internal static readonly uint FEATURE_CENTERED_CJK_PUNCTUATION = GetTagFromString("cpct");

	internal static readonly uint FEATURE_CAPITAL_SPACING = GetTagFromString("cpsp");

	internal static readonly uint FEATURE_CONTEXTUAL_SWASH = GetTagFromString("cswh");

	internal static readonly uint FEATURE_CURSIVE_POSITIONING = GetTagFromString("curs");

	internal static readonly uint FEATURE_PETITE_CAPITALS_FROM_CAPITALS = GetTagFromString("c2pc");

	internal static readonly uint FEATURE_SMALL_CAPITALS_FROM_CAPITALS = GetTagFromString("c2sc");

	internal static readonly uint FEATURE_DISTANCES = GetTagFromString("dist");

	internal static readonly uint FEATURE_DISCRETIONARY_LIGATURES = GetTagFromString("dlig");

	internal static readonly uint FEATURE_DENOMINATORS = GetTagFromString("dnom");

	internal static readonly uint FEATURE_EXPERT_FORMS = GetTagFromString("expt");

	internal static readonly uint FEATURE_FINAL_GLYPH_ON_LINE_ALTERNATES = GetTagFromString("falt");

	internal static readonly uint FEATURE_TERMINAL_FORMS_2 = GetTagFromString("fin2");

	internal static readonly uint FEATURE_TERMINAL_FORMS_3 = GetTagFromString("fin3");

	internal static readonly uint FEATURE_TERMINAL_FORMS = GetTagFromString("fina");

	internal static readonly uint FEATURE_FRACTIONS = GetTagFromString("frac");

	internal static readonly uint FEATURE_FULL_WIDTHS = GetTagFromString("fwid");

	internal static readonly uint FEATURE_HALF_FORMS = GetTagFromString("half");

	internal static readonly uint FEATURE_HALANT_FORMS = GetTagFromString("haln");

	internal static readonly uint FEATURE_ALTERNATE_HALF_WIDTHS = GetTagFromString("halt");

	internal static readonly uint FEATURE_HISTORICAL_FORMS = GetTagFromString("hist");

	internal static readonly uint FEATURE_HORIZONTAL_KANA_ALTERNATES = GetTagFromString("hkna");

	internal static readonly uint FEATURE_HISTORICAL_LIGATURES = GetTagFromString("hlig");

	internal static readonly uint FEATURE_HANGUL = GetTagFromString("hngl");

	internal static readonly uint FEATURE_HOJO_KANJI_FORMS = GetTagFromString("hojo");

	internal static readonly uint FEATURE_HALF_WIDTHS = GetTagFromString("hwid");

	internal static readonly uint FEATURE_INITIAL_FORMS = GetTagFromString("init");

	internal static readonly uint FEATURE_ISOLATED_FORMS = GetTagFromString("isol");

	internal static readonly uint FEATURE_ITALICS = GetTagFromString("ital");

	internal static readonly uint FEATURE_JUSTIFICATION_ALTERNATES = GetTagFromString("jalt");

	internal static readonly uint FEATURE_JIS78_FORMS = GetTagFromString("jp78");

	internal static readonly uint FEATURE_JIS83_FORMS = GetTagFromString("jp83");

	internal static readonly uint FEATURE_JIS90_FORMS = GetTagFromString("jp90");

	internal static readonly uint FEATURE_JIS2004_FORMS = GetTagFromString("jp04");

	internal static readonly uint FEATURE_KERNING = GetTagFromString("kern");

	internal static readonly uint FEATURE_LEFT_BOUNDS = GetTagFromString("lfbd");

	internal static readonly uint FEATURE_STANDARD_LIGATURES = GetTagFromString("liga");

	internal static readonly uint FEATURE_LEADING_JAMO_FORMS = GetTagFromString("ljmo");

	internal static readonly uint FEATURE_LINING_FIGURES = GetTagFromString("lnum");

	internal static readonly uint FEATURE_LOCALIZED_FORMS = GetTagFromString("locl");

	internal static readonly uint FEATURE_LEFT_TO_RIGHT_ALTERNATES = GetTagFromString("ltra");

	internal static readonly uint FEATURE_LEFT_TO_RIGHT_MIRRORED_FORMS = GetTagFromString("ltrm");

	internal static readonly uint FEATURE_MARK_POSITIONING = GetTagFromString("mark");

	internal static readonly uint FEATURE_MEDIAL_FORMS_2 = GetTagFromString("med2");

	internal static readonly uint FEATURE_MEDIAL_FORMS = GetTagFromString("medi");

	internal static readonly uint FEATURE_MATHEMATICAL_GREEK = GetTagFromString("mgrk");

	internal static readonly uint FEATURE_MARK_TO_MARK_POSITIONING = GetTagFromString("mkmk");

	internal static readonly uint FEATURE_MARK_POSITIONING_VIA_SUBSTITUTION = GetTagFromString("mset");

	internal static readonly uint FEATURE_ALTERNATE_ANNOTATION_FORMS = GetTagFromString("nalt");

	internal static readonly uint FEATURE_NLC_KANJI_FORMS = GetTagFromString("nlck");

	internal static readonly uint FEATURE_NUKTA_FORMS = GetTagFromString("nukt");

	internal static readonly uint FEATURE_NUMERATORS = GetTagFromString("numr");

	internal static readonly uint FEATURE_OLDSTYLE_FIGURES = GetTagFromString("onum");

	internal static readonly uint FEATURE_OPTICAL_BOUNDS = GetTagFromString("opbd");

	internal static readonly uint FEATURE_ORDINALS = GetTagFromString("ordn");

	internal static readonly uint FEATURE_ORNAMENTS = GetTagFromString("ornm");

	internal static readonly uint FEATURE_PROPORTIONAL_ALTERNATE_WIDTHS = GetTagFromString("palt");

	internal static readonly uint FEATURE_PETITE_CAPITALS = GetTagFromString("pcap");

	internal static readonly uint FEATURE_PROPORTIONAL_KANA = GetTagFromString("pkna");

	internal static readonly uint FEATURE_PROPORTIONAL_FIGURES = GetTagFromString("pnum");

	internal static readonly uint FEATURE_PRE_BASE_FORMS = GetTagFromString("pref");

	internal static readonly uint FEATURE_PRE_BASE_SUBSTITUTIONS = GetTagFromString("pres");

	internal static readonly uint FEATURE_POST_BASE_FORMS = GetTagFromString("pstf");

	internal static readonly uint FEATURE_POST_BASE_SUBSTITUTIONS = GetTagFromString("psts");

	internal static readonly uint FEATURE_PROPORTIONAL_WIDTHS = GetTagFromString("pwid");

	internal static readonly uint FEATURE_QUARTER_WIDTHS = GetTagFromString("qwid");

	internal static readonly uint FEATURE_RANDOMIZE = GetTagFromString("rand");

	internal static readonly uint FEATURE_RAKAR_FORMS = GetTagFromString("rkrf");

	internal static readonly uint FEATURE_REQUIRED_LIGATURES = GetTagFromString("rlig");

	internal static readonly uint FEATURE_REPH_FORMS = GetTagFromString("rphf");

	internal static readonly uint FEATURE_RIGHT_BOUNDS = GetTagFromString("rtbd");

	internal static readonly uint FEATURE_RIGHT_TO_LEFT_ALTERNATES = GetTagFromString("rtla");

	internal static readonly uint FEATURE_RIGHT_TO_LEFT_MIRRORED_FORMS = GetTagFromString("rtlm");

	internal static readonly uint FEATURE_RUBY_NOTATION_FORMS = GetTagFromString("ruby");

	internal static readonly uint FEATURE_STYLISTIC_ALTERNATES = GetTagFromString("salt");

	internal static readonly uint FEATURE_SCIENTIFIC_INFERIORS = GetTagFromString("sinf");

	internal static readonly uint FEATURE_OPTICAL_SIZE = GetTagFromString("size");

	internal static readonly uint FEATURE_SMALL_CAPITALS = GetTagFromString("smcp");

	internal static readonly uint FEATURE_SIMPLIFIED_FORMS = GetTagFromString("smpl");

	internal static readonly uint FEATURE_STYLISTIC_SET_1 = GetTagFromString("ss01");

	internal static readonly uint FEATURE_STYLISTIC_SET_2 = GetTagFromString("ss02");

	internal static readonly uint FEATURE_STYLISTIC_SET_3 = GetTagFromString("ss03");

	internal static readonly uint FEATURE_STYLISTIC_SET_4 = GetTagFromString("ss04");

	internal static readonly uint FEATURE_STYLISTIC_SET_5 = GetTagFromString("ss05");

	internal static readonly uint FEATURE_STYLISTIC_SET_6 = GetTagFromString("ss06");

	internal static readonly uint FEATURE_STYLISTIC_SET_7 = GetTagFromString("ss07");

	internal static readonly uint FEATURE_STYLISTIC_SET_8 = GetTagFromString("ss08");

	internal static readonly uint FEATURE_STYLISTIC_SET_9 = GetTagFromString("ss09");

	internal static readonly uint FEATURE_STYLISTIC_SET_10 = GetTagFromString("ss10");

	internal static readonly uint FEATURE_STYLISTIC_SET_11 = GetTagFromString("ss11");

	internal static readonly uint FEATURE_STYLISTIC_SET_12 = GetTagFromString("ss12");

	internal static readonly uint FEATURE_STYLISTIC_SET_13 = GetTagFromString("ss13");

	internal static readonly uint FEATURE_STYLISTIC_SET_14 = GetTagFromString("ss14");

	internal static readonly uint FEATURE_STYLISTIC_SET_15 = GetTagFromString("ss15");

	internal static readonly uint FEATURE_STYLISTIC_SET_16 = GetTagFromString("ss16");

	internal static readonly uint FEATURE_STYLISTIC_SET_17 = GetTagFromString("ss17");

	internal static readonly uint FEATURE_STYLISTIC_SET_18 = GetTagFromString("ss18");

	internal static readonly uint FEATURE_STYLISTIC_SET_19 = GetTagFromString("ss19");

	internal static readonly uint FEATURE_STYLISTIC_SET_20 = GetTagFromString("ss20");

	internal static readonly uint FEATURE_SUBSCRIPT = GetTagFromString("subs");

	internal static readonly uint FEATURE_SUPERSCRIPT = GetTagFromString("sups");

	internal static readonly uint FEATURE_SWASH = GetTagFromString("swsh");

	internal static readonly uint FEATURE_TITLING = GetTagFromString("titl");

	internal static readonly uint FEATURE_TRAILING_JAMO_FORMS = GetTagFromString("tjmo");

	internal static readonly uint FEATURE_TRADITIONAL_NAME_FORMS = GetTagFromString("tnam");

	internal static readonly uint FEATURE_TABULAR_FIGURES = GetTagFromString("tnum");

	internal static readonly uint FEATURE_TRADITIONAL_FORMS = GetTagFromString("trad");

	internal static readonly uint FEATURE_THIRD_WIDTHS = GetTagFromString("twid");

	internal static readonly uint FEATURE_UNICASE = GetTagFromString("unic");

	internal static readonly uint FEATURE_ALTERNATE_VERTICAL_METRICS = GetTagFromString("valt");

	internal static readonly uint FEATURE_VATTU_VARIANTS = GetTagFromString("vatu");

	internal static readonly uint FEATURE_VERTICAL_WRITING = GetTagFromString("vert");

	internal static readonly uint FEATURE_ALTERNATE_VERTICAL_HALF_METRICS = GetTagFromString("vhal");

	internal static readonly uint FEATURE_VOWEL_JAMO_FORMS = GetTagFromString("vjmo");

	internal static readonly uint FEATURE_VERTICAL_KANA_ALTERNATES = GetTagFromString("vkna");

	internal static readonly uint FEATURE_VERTICAL_KERNING = GetTagFromString("vkrn");

	internal static readonly uint FEATURE_PROPORTIONAL_ALTERNATE_VERTICAL_METRICS = GetTagFromString("vpal");

	internal static readonly uint FEATURE_VERTICAL_ALTERNATES_AND_ROTATION = GetTagFromString("vrt2");

	internal static readonly uint FEATURE_SLASHED_ZERO = GetTagFromString("zero");

	internal static string GetStringFromTag(uint tag)
	{
		return new string(new char[4]
		{
			(char)(0xFFu & (tag >> 24)),
			(char)(0xFFu & (tag >> 16)),
			(char)(0xFFu & (tag >> 8)),
			(char)(0xFFu & tag)
		});
	}

	internal static uint GetTagFromString(string str)
	{
		return ((uint)str[0] << 24) | ((uint)str[1] << 16) | ((uint)str[2] << 8) | str[3];
	}
}
