using System;
using System.Collections.Generic;
using System.Globalization;

namespace DocGen.Pdf.Graphics.Fonts;

internal class LanguageUtil
{
	internal Dictionary<int, UnicodeLanguageInfo> languageTags = new Dictionary<int, UnicodeLanguageInfo>();

	private static int[] dicardCharacters = new int[100]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 14,
		15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
		25, 26, 27, 127, 128, 129, 130, 131, 132, 133,
		134, 135, 136, 137, 138, 139, 140, 141, 142, 143,
		144, 145, 146, 147, 148, 149, 150, 151, 152, 153,
		154, 155, 156, 157, 158, 159, 173, 1536, 1537, 1538,
		1539, 1757, 1807, 6068, 6069, 8203, 8204, 8205, 8206, 8207,
		8234, 8235, 8236, 8237, 8238, 8288, 8289, 8290, 8291, 8292,
		8298, 8299, 8300, 8301, 8302, 8303, 65279, 65529, 65530, 65531,
		69821, 119155, 119156, 119157, 119158, 119159, 119160, 119161, 119162, 917505
	};

	private int[] tagStart = new int[317]
	{
		0, 65, 91, 97, 123, 170, 171, 186, 187, 192,
		215, 216, 247, 248, 697, 736, 741, 746, 748, 768,
		880, 884, 885, 894, 900, 901, 902, 903, 904, 994,
		1008, 1024, 1157, 1159, 1329, 1417, 1418, 1425, 1536, 1548,
		1549, 1563, 1566, 1567, 1568, 1600, 1601, 1611, 1622, 1632,
		1642, 1648, 1649, 1757, 1758, 1792, 1872, 1920, 1984, 2048,
		2112, 2208, 2304, 2385, 2387, 2404, 2406, 2433, 2561, 2689,
		2817, 2946, 3073, 3202, 3330, 3458, 3585, 3647, 3648, 3713,
		3840, 4053, 4057, 4096, 4256, 4347, 4348, 4352, 4608, 5024,
		5120, 5760, 5792, 5867, 5870, 5888, 5920, 5941, 5952, 5984,
		6016, 6144, 6146, 6148, 6149, 6150, 6320, 6400, 6480, 6528,
		6624, 6656, 6688, 6912, 7040, 7104, 7168, 7248, 7360, 7376,
		7379, 7380, 7393, 7394, 7401, 7405, 7406, 7412, 7413, 7424,
		7462, 7467, 7468, 7517, 7522, 7526, 7531, 7544, 7545, 7615,
		7616, 7680, 7936, 8192, 8204, 8206, 8305, 8308, 8319, 8320,
		8336, 8352, 8400, 8448, 8486, 8487, 8490, 8492, 8498, 8499,
		8526, 8527, 8544, 8585, 10240, 10496, 11264, 11360, 11392, 11520,
		11568, 11648, 11744, 11776, 11904, 12272, 12293, 12294, 12295, 12296,
		12321, 12330, 12334, 12336, 12344, 12348, 12353, 12441, 12443, 12445,
		12448, 12449, 12539, 12541, 12549, 12593, 12688, 12704, 12736, 12784,
		12800, 12832, 12896, 12927, 13008, 13144, 13312, 19904, 19968, 40960,
		42192, 42240, 42560, 42656, 42752, 42786, 42888, 42891, 43008, 43056,
		43072, 43136, 43232, 43264, 43312, 43360, 43392, 43520, 43616, 43648,
		43744, 43777, 43968, 44032, 55292, 63744, 64256, 64275, 64285, 64336,
		64830, 64848, 65021, 65024, 65040, 65056, 65072, 65136, 65279, 65313,
		65339, 65345, 65371, 65382, 65392, 65393, 65438, 65440, 65504, 65536,
		65792, 65856, 65936, 66045, 66176, 66208, 66304, 66352, 66432, 66464,
		66560, 66640, 66688, 67584, 67648, 67840, 67872, 67968, 68000, 68096,
		68192, 68352, 68416, 68448, 68608, 69216, 69632, 69760, 69840, 69888,
		70016, 71296, 73728, 77824, 92160, 93952, 110592, 110593, 118784, 119143,
		119146, 119163, 119171, 119173, 119180, 119210, 119214, 119296, 119552, 126464,
		126976, 127488, 127489, 131072, 917505, 917760, 918000
	};

	private int max_code = 1114111;

	private ScriptTags[] unicodeTags = new ScriptTags[317]
	{
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Bopomofo,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Greek,
		ScriptTags.Common,
		ScriptTags.Greek,
		ScriptTags.Common,
		ScriptTags.Greek,
		ScriptTags.Common,
		ScriptTags.Greek,
		ScriptTags.Common,
		ScriptTags.Greek,
		ScriptTags.Coptic,
		ScriptTags.Greek,
		ScriptTags.Cyrillic,
		ScriptTags.Inherited,
		ScriptTags.Cyrillic,
		ScriptTags.Armenian,
		ScriptTags.Common,
		ScriptTags.Armenian,
		ScriptTags.Hebrew,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Arabic,
		ScriptTags.Inherited,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Arabic,
		ScriptTags.Inherited,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Arabic,
		ScriptTags.Syriac,
		ScriptTags.Arabic,
		ScriptTags.Thaana,
		ScriptTags.Nko,
		ScriptTags.Samaritan,
		ScriptTags.Mandaic,
		ScriptTags.Arabic,
		ScriptTags.Devanagari,
		ScriptTags.Inherited,
		ScriptTags.Devanagari,
		ScriptTags.Common,
		ScriptTags.Devanagari,
		ScriptTags.Bengali,
		ScriptTags.Gurmukhi,
		ScriptTags.Gujarati,
		ScriptTags.Oriya,
		ScriptTags.Tamil,
		ScriptTags.Telugu,
		ScriptTags.Kannada,
		ScriptTags.Malayalam,
		ScriptTags.Sinhala,
		ScriptTags.Thai,
		ScriptTags.Common,
		ScriptTags.Thai,
		ScriptTags.Lao,
		ScriptTags.Tibetan,
		ScriptTags.Common,
		ScriptTags.Tibetan,
		ScriptTags.Myanmar,
		ScriptTags.Georgian,
		ScriptTags.Common,
		ScriptTags.Georgian,
		ScriptTags.Hangul,
		ScriptTags.Ethiopic,
		ScriptTags.Cherokee,
		ScriptTags.Canadian_Aboriginal,
		ScriptTags.Ogham,
		ScriptTags.Runic,
		ScriptTags.Common,
		ScriptTags.Runic,
		ScriptTags.Tagalog,
		ScriptTags.Hanunoo,
		ScriptTags.Common,
		ScriptTags.Buhid,
		ScriptTags.Tagbanwa,
		ScriptTags.Khmer,
		ScriptTags.Mongolian,
		ScriptTags.Common,
		ScriptTags.Mongolian,
		ScriptTags.Common,
		ScriptTags.Mongolian,
		ScriptTags.Canadian_Aboriginal,
		ScriptTags.Limbu,
		ScriptTags.TaiLe,
		ScriptTags.NewTaiLue,
		ScriptTags.Khmer,
		ScriptTags.Buginese,
		ScriptTags.Tai_Tham,
		ScriptTags.Balinese,
		ScriptTags.Sundanese,
		ScriptTags.Batak,
		ScriptTags.Lepcha,
		ScriptTags.Ol_Chiki,
		ScriptTags.Sundanese,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Greek,
		ScriptTags.Cyrillic,
		ScriptTags.Latin,
		ScriptTags.Greek,
		ScriptTags.Latin,
		ScriptTags.Greek,
		ScriptTags.Latin,
		ScriptTags.Cyrillic,
		ScriptTags.Latin,
		ScriptTags.Greek,
		ScriptTags.Inherited,
		ScriptTags.Latin,
		ScriptTags.Greek,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Greek,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Braille,
		ScriptTags.Common,
		ScriptTags.Glagolitic,
		ScriptTags.Latin,
		ScriptTags.Coptic,
		ScriptTags.Georgian,
		ScriptTags.Tifinagh,
		ScriptTags.Ethiopic,
		ScriptTags.Cyrillic,
		ScriptTags.Common,
		ScriptTags.Han,
		ScriptTags.Common,
		ScriptTags.Han,
		ScriptTags.Common,
		ScriptTags.Han,
		ScriptTags.Common,
		ScriptTags.Han,
		ScriptTags.Inherited,
		ScriptTags.Hangul,
		ScriptTags.Common,
		ScriptTags.Han,
		ScriptTags.Common,
		ScriptTags.Hiragana,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Hiragana,
		ScriptTags.Common,
		ScriptTags.Hiragana,
		ScriptTags.Common,
		ScriptTags.Hiragana,
		ScriptTags.Bopomofo,
		ScriptTags.Hangul,
		ScriptTags.Common,
		ScriptTags.Bopomofo,
		ScriptTags.Common,
		ScriptTags.Hiragana,
		ScriptTags.Hangul,
		ScriptTags.Common,
		ScriptTags.Hangul,
		ScriptTags.Common,
		ScriptTags.Hiragana,
		ScriptTags.Common,
		ScriptTags.Han,
		ScriptTags.Common,
		ScriptTags.Han,
		ScriptTags.Yi,
		ScriptTags.Lisu,
		ScriptTags.Vai,
		ScriptTags.Cyrillic,
		ScriptTags.Bamum,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.SylotiNagri,
		ScriptTags.Common,
		ScriptTags.Phagspa,
		ScriptTags.Saurashtra,
		ScriptTags.Devanagari,
		ScriptTags.Kayah_Li,
		ScriptTags.Rejang,
		ScriptTags.Hangul,
		ScriptTags.Javanese,
		ScriptTags.Cham,
		ScriptTags.Myanmar,
		ScriptTags.Tai_Viet,
		ScriptTags.Meetei_Mayek,
		ScriptTags.Ethiopic,
		ScriptTags.Meetei_Mayek,
		ScriptTags.Hangul,
		ScriptTags.Unknown,
		ScriptTags.Han,
		ScriptTags.Latin,
		ScriptTags.Armenian,
		ScriptTags.Hebrew,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Latin,
		ScriptTags.Common,
		ScriptTags.Hiragana,
		ScriptTags.Common,
		ScriptTags.Hiragana,
		ScriptTags.Common,
		ScriptTags.Hangul,
		ScriptTags.Common,
		ScriptTags.LinearB,
		ScriptTags.Common,
		ScriptTags.Greek,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Lycian,
		ScriptTags.Carian,
		ScriptTags.OldItalic,
		ScriptTags.Gothic,
		ScriptTags.UgariticCuneiform,
		ScriptTags.OldPersianCuneiform,
		ScriptTags.Deseret,
		ScriptTags.Shavian,
		ScriptTags.Osmanya,
		ScriptTags.CypriotSyllabary,
		ScriptTags.Imperial_Aramaic,
		ScriptTags.Phoenician,
		ScriptTags.Lydian,
		ScriptTags.Meroitic_Hieroglyphs,
		ScriptTags.Meroitic_Cursive,
		ScriptTags.Kharosthi,
		ScriptTags.Old_South_Arabian,
		ScriptTags.Avestan,
		ScriptTags.Inscriptional_Parthian,
		ScriptTags.Inscriptional_Pahlavi,
		ScriptTags.Old_Turkic,
		ScriptTags.Arabic,
		ScriptTags.Brahmi,
		ScriptTags.Kaithi,
		ScriptTags.Sora_Sompeng,
		ScriptTags.Chakma,
		ScriptTags.Sharada,
		ScriptTags.Takri,
		ScriptTags.Cuneiform,
		ScriptTags.Egyptian_Hieroglyphs,
		ScriptTags.Bamum,
		ScriptTags.Miao,
		ScriptTags.Hiragana,
		ScriptTags.Hiragana,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Common,
		ScriptTags.Greek,
		ScriptTags.Common,
		ScriptTags.Arabic,
		ScriptTags.Common,
		ScriptTags.Hiragana,
		ScriptTags.Common,
		ScriptTags.Han,
		ScriptTags.Common,
		ScriptTags.Inherited,
		ScriptTags.Unknown
	};

	internal LanguageUtil()
	{
		UpdateLanguages();
	}

	internal ScriptTags GetLanguage(char c)
	{
		int[] array = new int[languageTags.Keys.Count];
		languageTags.Keys.CopyTo(array, 0);
		int num = Array.BinarySearch(array, c);
		if (num < 0)
		{
			num = -num - 2;
		}
		languageTags.TryGetValue(array[num], out UnicodeLanguageInfo value);
		return GetScriptTag<ScriptTags>(value.LanguageName);
	}

	private T GetScriptTag<T>(string languageName)
	{
		T result = default(T);
		if (Enum.IsDefined(typeof(T), languageName))
		{
			result = (T)Enum.Parse(typeof(T), languageName, ignoreCase: true);
		}
		else
		{
			string[] names = Enum.GetNames(typeof(T));
			foreach (string text in names)
			{
				if (text.Equals(languageName, StringComparison.OrdinalIgnoreCase))
				{
					result = (T)Enum.Parse(typeof(T), text);
				}
			}
		}
		return result;
	}

	internal int FindIndex(int[] array, char key)
	{
		int num = 0;
		int num2 = array.Length - 1;
		int num3 = 0;
		do
		{
			num3 = num + (num2 - num) / 2;
			if (key > array[num3])
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
			if (array[num3] == key)
			{
				return num3;
			}
		}
		while (num <= num2);
		return num3 - 1;
	}

	internal ScriptTags GetGlyphTag(int code)
	{
		if (IsValidCode(code) && !char.IsHighSurrogate((char)code))
		{
			try
			{
				if (CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(code), 0) == UnicodeCategory.OtherNotAssigned)
				{
					return ScriptTags.Unknown;
				}
				int num = Array.BinarySearch(tagStart, code);
				if (num < 0)
				{
					num = -num - 2;
				}
				return unicodeTags[num];
			}
			catch
			{
				return ScriptTags.Unknown;
			}
		}
		return ScriptTags.Unknown;
	}

	private bool IsValidCode(int code)
	{
		return code >>> 16 < max_code + 1 >>> 16;
	}

	private void UpdateLanguages()
	{
		languageTags.Add(0, new UnicodeLanguageInfo("Latin", 0, 127));
		languageTags.Add(128, new UnicodeLanguageInfo("Latin", 128, 255));
		languageTags.Add(256, new UnicodeLanguageInfo("Latin", 256, 383));
		languageTags.Add(384, new UnicodeLanguageInfo("Latin", 384, 591));
		languageTags.Add(592, new UnicodeLanguageInfo("IPA Extensions", 592, 687));
		languageTags.Add(688, new UnicodeLanguageInfo("Spacing Modifier Letters", 688, 767));
		languageTags.Add(768, new UnicodeLanguageInfo("Combining Diacritical Marks", 768, 879));
		languageTags.Add(880, new UnicodeLanguageInfo("Greek and Coptic", 880, 1023));
		languageTags.Add(1024, new UnicodeLanguageInfo("Cyrillic", 1024, 1279));
		languageTags.Add(1280, new UnicodeLanguageInfo("Cyrillic", 1280, 1327));
		languageTags.Add(1328, new UnicodeLanguageInfo("Armenian", 1328, 1423));
		languageTags.Add(1424, new UnicodeLanguageInfo("Hebrew", 1424, 1535));
		languageTags.Add(1536, new UnicodeLanguageInfo("Arabic", 1536, 1791));
		languageTags.Add(1792, new UnicodeLanguageInfo("Syriac", 1792, 1871));
		languageTags.Add(1872, new UnicodeLanguageInfo("Arabic", 1872, 1919));
		languageTags.Add(1920, new UnicodeLanguageInfo("Thaana", 1920, 1983));
		languageTags.Add(1984, new UnicodeLanguageInfo("NKo", 1984, 2047));
		languageTags.Add(2048, new UnicodeLanguageInfo("Samaritan", 2048, 2111));
		languageTags.Add(2112, new UnicodeLanguageInfo("Mandaic", 2112, 2143));
		languageTags.Add(2144, new UnicodeLanguageInfo("Syriac", 2144, 2159));
		languageTags.Add(2208, new UnicodeLanguageInfo("Arabic", 2208, 2303));
		languageTags.Add(2304, new UnicodeLanguageInfo("Devanagari", 2304, 2431));
		languageTags.Add(2432, new UnicodeLanguageInfo("Bengali", 2432, 2559));
		languageTags.Add(2560, new UnicodeLanguageInfo("Gurmukhi", 2560, 2687));
		languageTags.Add(2688, new UnicodeLanguageInfo("Gujarati", 2688, 2815));
		languageTags.Add(2816, new UnicodeLanguageInfo("Oriya", 2816, 2943));
		languageTags.Add(2944, new UnicodeLanguageInfo("Tamil", 2944, 3071));
		languageTags.Add(3072, new UnicodeLanguageInfo("Telugu", 3072, 3199));
		languageTags.Add(3200, new UnicodeLanguageInfo("Kannada", 3200, 3327));
		languageTags.Add(3328, new UnicodeLanguageInfo("Malayalam", 3328, 3455));
		languageTags.Add(3456, new UnicodeLanguageInfo("Sinhala", 3456, 3583));
		languageTags.Add(3584, new UnicodeLanguageInfo("Thai", 3584, 3711));
		languageTags.Add(3712, new UnicodeLanguageInfo("Lao", 3712, 3839));
		languageTags.Add(3840, new UnicodeLanguageInfo("Tibetan", 3840, 4095));
		languageTags.Add(4096, new UnicodeLanguageInfo("Myanmar", 4096, 4255));
		languageTags.Add(4256, new UnicodeLanguageInfo("Georgian", 4256, 4351));
		languageTags.Add(4352, new UnicodeLanguageInfo("Hangul", 4352, 4607));
		languageTags.Add(4608, new UnicodeLanguageInfo("Ethiopic", 4608, 4991));
		languageTags.Add(4992, new UnicodeLanguageInfo("Ethiopic Supplement", 4992, 5023));
		languageTags.Add(5024, new UnicodeLanguageInfo("Cherokee", 5024, 5119));
		languageTags.Add(5120, new UnicodeLanguageInfo("Canadian", 5120, 5759));
		languageTags.Add(5760, new UnicodeLanguageInfo("Ogham", 5760, 5791));
		languageTags.Add(5792, new UnicodeLanguageInfo("Runic", 5792, 5887));
		languageTags.Add(5888, new UnicodeLanguageInfo("Tagalog", 5888, 5919));
		languageTags.Add(5920, new UnicodeLanguageInfo("Hanunoo", 5920, 5951));
		languageTags.Add(5952, new UnicodeLanguageInfo("Buhid", 5952, 5983));
		languageTags.Add(5984, new UnicodeLanguageInfo("Tagbanwa", 5984, 6015));
		languageTags.Add(6016, new UnicodeLanguageInfo("Khmer", 6016, 6143));
		languageTags.Add(6144, new UnicodeLanguageInfo("Mongolian", 6144, 6319));
		languageTags.Add(6320, new UnicodeLanguageInfo("Canadian", 6320, 6399));
		languageTags.Add(6400, new UnicodeLanguageInfo("Limbu", 6400, 6479));
		languageTags.Add(6480, new UnicodeLanguageInfo("Tai Le", 6480, 6527));
		languageTags.Add(6528, new UnicodeLanguageInfo("New Tai Lue", 6528, 6623));
		languageTags.Add(6624, new UnicodeLanguageInfo("Khmer Symbols", 6624, 6655));
		languageTags.Add(6656, new UnicodeLanguageInfo("Buginese", 6656, 6687));
		languageTags.Add(6688, new UnicodeLanguageInfo("Tai Tham", 6688, 6831));
		languageTags.Add(6832, new UnicodeLanguageInfo("Diacritical Marks", 6832, 6911));
		languageTags.Add(6912, new UnicodeLanguageInfo("Balinese", 6912, 7039));
		languageTags.Add(7040, new UnicodeLanguageInfo("Sundanese", 7040, 7103));
		languageTags.Add(7104, new UnicodeLanguageInfo("Batak", 7104, 7167));
		languageTags.Add(7168, new UnicodeLanguageInfo("Lepcha", 7168, 7247));
		languageTags.Add(7248, new UnicodeLanguageInfo("Ol Chiki", 7248, 7295));
		languageTags.Add(7296, new UnicodeLanguageInfo("Cyrillic", 7296, 7311));
		languageTags.Add(7312, new UnicodeLanguageInfo("Georgian", 7312, 7359));
		languageTags.Add(7360, new UnicodeLanguageInfo("Sundanese", 7360, 7375));
		languageTags.Add(7376, new UnicodeLanguageInfo("Vedic Extensions", 7376, 7423));
		languageTags.Add(7424, new UnicodeLanguageInfo("Phonetic", 7424, 7551));
		languageTags.Add(7552, new UnicodeLanguageInfo("Phonetic", 7552, 7615));
		languageTags.Add(7616, new UnicodeLanguageInfo("Diacritical Marks", 7616, 7679));
		languageTags.Add(7680, new UnicodeLanguageInfo("Latin", 7680, 7935));
		languageTags.Add(7936, new UnicodeLanguageInfo("Greek", 7936, 8191));
		languageTags.Add(8192, new UnicodeLanguageInfo("General Punctuation", 8192, 8303));
		languageTags.Add(8304, new UnicodeLanguageInfo("Superscripts and Subscripts", 8304, 8351));
		languageTags.Add(8352, new UnicodeLanguageInfo("Currency Symbols", 8352, 8399));
		languageTags.Add(8400, new UnicodeLanguageInfo("Diacritical Marks", 8400, 8447));
		languageTags.Add(8448, new UnicodeLanguageInfo("Letterlike Symbols", 8448, 8527));
		languageTags.Add(8528, new UnicodeLanguageInfo("Number Forms", 8528, 8591));
		languageTags.Add(8592, new UnicodeLanguageInfo("Arrows", 8592, 8703));
		languageTags.Add(8704, new UnicodeLanguageInfo("Operators", 8704, 8959));
		languageTags.Add(8960, new UnicodeLanguageInfo("Miscellaneous", 8960, 9215));
		languageTags.Add(9216, new UnicodeLanguageInfo("Control Pictures", 9216, 9279));
		languageTags.Add(9280, new UnicodeLanguageInfo("Optical Character Recognition", 9280, 9311));
		languageTags.Add(9312, new UnicodeLanguageInfo("Enclosed Alphanumerics", 9312, 9471));
		languageTags.Add(9472, new UnicodeLanguageInfo("Box Drawing", 9472, 9599));
		languageTags.Add(9600, new UnicodeLanguageInfo("Block Elements", 9600, 9631));
		languageTags.Add(9632, new UnicodeLanguageInfo("Geometric Shapes", 9632, 9727));
		languageTags.Add(9728, new UnicodeLanguageInfo("Miscellaneous", 9728, 9983));
		languageTags.Add(9984, new UnicodeLanguageInfo("Dingbats", 9984, 10175));
		languageTags.Add(10176, new UnicodeLanguageInfo("Miscellaneous", 10176, 10223));
		languageTags.Add(10224, new UnicodeLanguageInfo("Supplemental", 10224, 10239));
		languageTags.Add(10240, new UnicodeLanguageInfo("Braille Patterns", 10240, 10495));
		languageTags.Add(10496, new UnicodeLanguageInfo("Supplemental", 10496, 10623));
		languageTags.Add(10624, new UnicodeLanguageInfo("Miscellaneous", 10624, 10751));
		languageTags.Add(10752, new UnicodeLanguageInfo("Supplemental", 10752, 11007));
		languageTags.Add(11008, new UnicodeLanguageInfo("Miscellaneous", 11008, 11263));
		languageTags.Add(11264, new UnicodeLanguageInfo("Glagolitic", 11264, 11359));
		languageTags.Add(11360, new UnicodeLanguageInfo("Latin", 11360, 11391));
		languageTags.Add(11392, new UnicodeLanguageInfo("Coptic", 11392, 11519));
		languageTags.Add(11520, new UnicodeLanguageInfo("Georgian Supplement", 11520, 11567));
		languageTags.Add(11568, new UnicodeLanguageInfo("Tifinagh", 11568, 11647));
		languageTags.Add(11648, new UnicodeLanguageInfo("Ethiopic Extended", 11648, 11743));
		languageTags.Add(11744, new UnicodeLanguageInfo("Cyrillic", 11744, 11775));
		languageTags.Add(11776, new UnicodeLanguageInfo("Supplemental", 11776, 11903));
		languageTags.Add(11904, new UnicodeLanguageInfo("CJK", 11904, 12031));
		languageTags.Add(12032, new UnicodeLanguageInfo("Kangxi Radicals", 12032, 12255));
		languageTags.Add(12272, new UnicodeLanguageInfo("Ideographic", 12272, 12287));
		languageTags.Add(12288, new UnicodeLanguageInfo("CJK", 12288, 12351));
		languageTags.Add(12352, new UnicodeLanguageInfo("Hiragana", 12352, 12447));
		languageTags.Add(12448, new UnicodeLanguageInfo("Katakana", 12448, 12543));
		languageTags.Add(12544, new UnicodeLanguageInfo("Bopomofo", 12544, 12591));
		languageTags.Add(12592, new UnicodeLanguageInfo("Hangul", 12592, 12687));
		languageTags.Add(12688, new UnicodeLanguageInfo("Kanbun", 12688, 12703));
		languageTags.Add(12704, new UnicodeLanguageInfo("Bopomofo Extended", 12704, 12735));
		languageTags.Add(12736, new UnicodeLanguageInfo("CJK", 12736, 12783));
		languageTags.Add(12784, new UnicodeLanguageInfo("Katakana", 12784, 12799));
		languageTags.Add(12800, new UnicodeLanguageInfo("CJK", 12800, 13055));
		languageTags.Add(13056, new UnicodeLanguageInfo("CJK", 13056, 13311));
		languageTags.Add(13312, new UnicodeLanguageInfo("CJK", 13312, 19903));
		languageTags.Add(19904, new UnicodeLanguageInfo("Yijing", 19904, 19967));
		languageTags.Add(19968, new UnicodeLanguageInfo("CJK", 19968, 40959));
		languageTags.Add(40960, new UnicodeLanguageInfo("Yi", 40960, 42127));
		languageTags.Add(42128, new UnicodeLanguageInfo("Yi", 42128, 42191));
		languageTags.Add(42192, new UnicodeLanguageInfo("Lisu", 42192, 42239));
		languageTags.Add(42240, new UnicodeLanguageInfo("Vai", 42240, 42559));
		languageTags.Add(42560, new UnicodeLanguageInfo("Cyrillic", 42560, 42655));
		languageTags.Add(42656, new UnicodeLanguageInfo("Bamum", 42656, 42751));
		languageTags.Add(42752, new UnicodeLanguageInfo("Modifier Tone Letters", 42752, 42783));
		languageTags.Add(42784, new UnicodeLanguageInfo("Latin", 42784, 43007));
		languageTags.Add(43008, new UnicodeLanguageInfo("Syloti Nagri", 43008, 43055));
		languageTags.Add(43056, new UnicodeLanguageInfo("Indic Number", 43056, 43071));
		languageTags.Add(43072, new UnicodeLanguageInfo("Phags", 43072, 43135));
		languageTags.Add(43136, new UnicodeLanguageInfo("Saurashtra", 43136, 43231));
		languageTags.Add(43232, new UnicodeLanguageInfo("Devanagari", 43232, 43263));
		languageTags.Add(43264, new UnicodeLanguageInfo("Kayah Li", 43264, 43311));
		languageTags.Add(43312, new UnicodeLanguageInfo("Rejang", 43312, 43359));
		languageTags.Add(43360, new UnicodeLanguageInfo("Hangul", 43360, 43391));
		languageTags.Add(43392, new UnicodeLanguageInfo("Javanese", 43392, 43487));
		languageTags.Add(43488, new UnicodeLanguageInfo("Myanmar", 43488, 43519));
		languageTags.Add(43520, new UnicodeLanguageInfo("Cham", 43520, 43615));
		languageTags.Add(43616, new UnicodeLanguageInfo("Myanmar", 43616, 43647));
		languageTags.Add(43648, new UnicodeLanguageInfo("Tai Viet", 43648, 43743));
		languageTags.Add(43744, new UnicodeLanguageInfo("Meetei", 43744, 43775));
		languageTags.Add(43776, new UnicodeLanguageInfo("Ethiopic", 43776, 43823));
		languageTags.Add(43824, new UnicodeLanguageInfo("Latin", 43824, 43887));
		languageTags.Add(43888, new UnicodeLanguageInfo("Cherokee", 43888, 43967));
		languageTags.Add(43968, new UnicodeLanguageInfo("Meetei", 43968, 44031));
		languageTags.Add(44032, new UnicodeLanguageInfo("Hangul", 44032, 55215));
		languageTags.Add(55216, new UnicodeLanguageInfo("Hangul", 55216, 55295));
		languageTags.Add(55296, new UnicodeLanguageInfo("Surrogates", 55296, 56191));
		languageTags.Add(56192, new UnicodeLanguageInfo("Surrogates", 56192, 56319));
		languageTags.Add(56320, new UnicodeLanguageInfo("Surrogates", 56320, 57343));
		languageTags.Add(57344, new UnicodeLanguageInfo("Private Use Area", 57344, 63743));
		languageTags.Add(63744, new UnicodeLanguageInfo("CJK", 63744, 64255));
		languageTags.Add(64256, new UnicodeLanguageInfo("Alphabetic", 64256, 64335));
		languageTags.Add(64336, new UnicodeLanguageInfo("Arabic", 64336, 65023));
		languageTags.Add(65024, new UnicodeLanguageInfo("Variation Selectors", 65024, 65039));
		languageTags.Add(65040, new UnicodeLanguageInfo("Vertical Forms", 65040, 65055));
		languageTags.Add(65056, new UnicodeLanguageInfo("Combining Half Marks", 65056, 65071));
		languageTags.Add(65072, new UnicodeLanguageInfo("CJK", 65072, 65103));
		languageTags.Add(65104, new UnicodeLanguageInfo("Small Form Variants", 65104, 65135));
		languageTags.Add(65136, new UnicodeLanguageInfo("Arabic", 65136, 65279));
		languageTags.Add(65280, new UnicodeLanguageInfo("Halfwidth and Fullwidth Forms", 65280, 65519));
		languageTags.Add(65520, new UnicodeLanguageInfo("Specials", 65520, 65535));
		languageTags.Add(65536, new UnicodeLanguageInfo("Linear", 65536, 65663));
		languageTags.Add(65664, new UnicodeLanguageInfo("Linear", 65664, 65791));
		languageTags.Add(65792, new UnicodeLanguageInfo("Aegean", 65792, 65855));
		languageTags.Add(65856, new UnicodeLanguageInfo("Ancient", 65856, 65935));
		languageTags.Add(65936, new UnicodeLanguageInfo("Ancient", 65936, 65999));
		languageTags.Add(66000, new UnicodeLanguageInfo("Phaistos", 66000, 66047));
		languageTags.Add(66176, new UnicodeLanguageInfo("Lycian", 66176, 66207));
		languageTags.Add(66208, new UnicodeLanguageInfo("Carian", 66208, 66271));
		languageTags.Add(66272, new UnicodeLanguageInfo("Coptic", 66272, 66303));
		languageTags.Add(66304, new UnicodeLanguageInfo("Italic", 66304, 66351));
		languageTags.Add(66352, new UnicodeLanguageInfo("Gothic", 66352, 66383));
		languageTags.Add(66384, new UnicodeLanguageInfo("Permic", 66384, 66431));
		languageTags.Add(66432, new UnicodeLanguageInfo("Ugaritic", 66432, 66463));
		languageTags.Add(66464, new UnicodeLanguageInfo("Persian", 66464, 66527));
		languageTags.Add(66560, new UnicodeLanguageInfo("Deseret", 66560, 66639));
		languageTags.Add(66640, new UnicodeLanguageInfo("Shavian", 66640, 66687));
		languageTags.Add(66688, new UnicodeLanguageInfo("Osmanya", 66688, 66735));
		languageTags.Add(66736, new UnicodeLanguageInfo("Osage", 66736, 66815));
		languageTags.Add(66816, new UnicodeLanguageInfo("Elbasan", 66816, 66863));
		languageTags.Add(66864, new UnicodeLanguageInfo("Caucasian Albanian", 66864, 66927));
		languageTags.Add(67072, new UnicodeLanguageInfo("Linear A", 67072, 67455));
		languageTags.Add(67584, new UnicodeLanguageInfo("Cypriot Syllabary", 67584, 67647));
		languageTags.Add(67648, new UnicodeLanguageInfo("Imperial Aramaic", 67648, 67679));
		languageTags.Add(67680, new UnicodeLanguageInfo("Palmyrene", 67680, 67711));
		languageTags.Add(67712, new UnicodeLanguageInfo("Nabataean", 67712, 67759));
		languageTags.Add(67808, new UnicodeLanguageInfo("Hatran", 67808, 67839));
		languageTags.Add(67840, new UnicodeLanguageInfo("Phoenician", 67840, 67871));
		languageTags.Add(67872, new UnicodeLanguageInfo("Lydian", 67872, 67903));
		languageTags.Add(67968, new UnicodeLanguageInfo("Meroitic Hieroglyphs", 67968, 67999));
		languageTags.Add(68000, new UnicodeLanguageInfo("Meroitic Cursive", 68000, 68095));
		languageTags.Add(68096, new UnicodeLanguageInfo("Kharoshthi", 68096, 68191));
		languageTags.Add(68192, new UnicodeLanguageInfo("Old South Arabian", 68192, 68223));
		languageTags.Add(68224, new UnicodeLanguageInfo("Old North Arabian", 68224, 68255));
		languageTags.Add(68288, new UnicodeLanguageInfo("Manichaean", 68288, 68351));
		languageTags.Add(68352, new UnicodeLanguageInfo("Avestan", 68352, 68415));
		languageTags.Add(68416, new UnicodeLanguageInfo("Inscriptional Parthian", 68416, 68447));
		languageTags.Add(68448, new UnicodeLanguageInfo("Inscriptional Pahlavi", 68448, 68479));
		languageTags.Add(68480, new UnicodeLanguageInfo("Psalter Pahlavi", 68480, 68527));
		languageTags.Add(68608, new UnicodeLanguageInfo("Old Turkic", 68608, 68687));
		languageTags.Add(68736, new UnicodeLanguageInfo("Old Hungarian", 68736, 68863));
		languageTags.Add(68864, new UnicodeLanguageInfo("Hanifi Rohingya", 68864, 68927));
		languageTags.Add(69216, new UnicodeLanguageInfo("Rumi Numeral Symbols", 69216, 69247));
		languageTags.Add(69376, new UnicodeLanguageInfo("Old Sogdian", 69376, 69423));
		languageTags.Add(69424, new UnicodeLanguageInfo("Sogdian", 69424, 69487));
		languageTags.Add(69632, new UnicodeLanguageInfo("Brahmi", 69632, 69759));
		languageTags.Add(69760, new UnicodeLanguageInfo("Kaithi", 69760, 69839));
		languageTags.Add(69840, new UnicodeLanguageInfo("Sora Sompeng", 69840, 69887));
		languageTags.Add(69888, new UnicodeLanguageInfo("Chakma", 69888, 69967));
		languageTags.Add(69968, new UnicodeLanguageInfo("Mahajani", 69968, 70015));
		languageTags.Add(70016, new UnicodeLanguageInfo("Sharada", 70016, 70111));
		languageTags.Add(70112, new UnicodeLanguageInfo("Sinhala", 70112, 70143));
		languageTags.Add(70144, new UnicodeLanguageInfo("Khojki", 70144, 70223));
		languageTags.Add(70272, new UnicodeLanguageInfo("Multani", 70272, 70319));
		languageTags.Add(70320, new UnicodeLanguageInfo("Khudawadi", 70320, 70399));
		languageTags.Add(70400, new UnicodeLanguageInfo("Grantha", 70400, 70527));
		languageTags.Add(70656, new UnicodeLanguageInfo("Newa", 70656, 70783));
		languageTags.Add(70784, new UnicodeLanguageInfo("Tirhuta", 70784, 70879));
		languageTags.Add(71040, new UnicodeLanguageInfo("Siddham", 71040, 71167));
		languageTags.Add(71168, new UnicodeLanguageInfo("Modi", 71168, 71263));
		languageTags.Add(71264, new UnicodeLanguageInfo("Mongolian", 71264, 71295));
		languageTags.Add(71296, new UnicodeLanguageInfo("Takri", 71296, 71375));
		languageTags.Add(71424, new UnicodeLanguageInfo("Ahom", 71424, 71487));
		languageTags.Add(71680, new UnicodeLanguageInfo("Dogra", 71680, 71759));
		languageTags.Add(71840, new UnicodeLanguageInfo("Warang Citi", 71840, 71935));
		languageTags.Add(72192, new UnicodeLanguageInfo("Zanabazar Square", 72192, 72271));
		languageTags.Add(72272, new UnicodeLanguageInfo("Soyombo", 72272, 72367));
		languageTags.Add(72384, new UnicodeLanguageInfo("Pau Cin Hau", 72384, 72447));
		languageTags.Add(72704, new UnicodeLanguageInfo("Bhaiksuki", 72704, 72815));
		languageTags.Add(72816, new UnicodeLanguageInfo("Marchen", 72816, 72895));
		languageTags.Add(72960, new UnicodeLanguageInfo("Masaram Gondi", 72960, 73055));
		languageTags.Add(73056, new UnicodeLanguageInfo("Gunjala Gondi", 73056, 73135));
		languageTags.Add(73440, new UnicodeLanguageInfo("Makasar", 73440, 73471));
		languageTags.Add(73728, new UnicodeLanguageInfo("Cuneiform", 73728, 74751));
		languageTags.Add(74752, new UnicodeLanguageInfo("Cuneiform Numbers and Punctuation", 74752, 74879));
		languageTags.Add(74880, new UnicodeLanguageInfo("Early Dynastic Cuneiform", 74880, 75087));
		languageTags.Add(77824, new UnicodeLanguageInfo("Egyptian Hieroglyphs", 77824, 78895));
		languageTags.Add(82944, new UnicodeLanguageInfo("Anatolian Hieroglyphs", 82944, 83583));
		languageTags.Add(92160, new UnicodeLanguageInfo("Bamum Supplement", 92160, 92735));
		languageTags.Add(92736, new UnicodeLanguageInfo("Mro", 92736, 92783));
		languageTags.Add(92880, new UnicodeLanguageInfo("Bassa Vah", 92880, 92927));
		languageTags.Add(92928, new UnicodeLanguageInfo("Pahawh Hmong", 92928, 93071));
		languageTags.Add(93760, new UnicodeLanguageInfo("Medefaidrin", 93760, 93855));
		languageTags.Add(93952, new UnicodeLanguageInfo("Miao", 93952, 94111));
		languageTags.Add(94176, new UnicodeLanguageInfo("Ideographic", 94176, 94207));
		languageTags.Add(94208, new UnicodeLanguageInfo("Tangut", 94208, 100351));
		languageTags.Add(100352, new UnicodeLanguageInfo("Tangut", 100352, 101119));
		languageTags.Add(110592, new UnicodeLanguageInfo("Kana", 110592, 110847));
		languageTags.Add(110848, new UnicodeLanguageInfo("Kana", 110848, 110895));
		languageTags.Add(110960, new UnicodeLanguageInfo("Nushu", 110960, 111359));
		languageTags.Add(113664, new UnicodeLanguageInfo("Duployan", 113664, 113823));
		languageTags.Add(113824, new UnicodeLanguageInfo("Shorthand", 113824, 113839));
		languageTags.Add(118784, new UnicodeLanguageInfo("Byzantine Musical Symbols", 118784, 119039));
		languageTags.Add(119040, new UnicodeLanguageInfo("Musical Symbols", 119040, 119295));
		languageTags.Add(119296, new UnicodeLanguageInfo("Greek", 119296, 119375));
		languageTags.Add(119520, new UnicodeLanguageInfo("Mayan Numerals", 119520, 119551));
		languageTags.Add(119552, new UnicodeLanguageInfo("Tai Xuan Jing Symbols", 119552, 119647));
		languageTags.Add(119648, new UnicodeLanguageInfo("Counting Rod Numerals", 119648, 119679));
		languageTags.Add(119808, new UnicodeLanguageInfo("Alphanumeric", 119808, 120831));
		languageTags.Add(120832, new UnicodeLanguageInfo("Sutton SignWriting", 120832, 121519));
		languageTags.Add(122880, new UnicodeLanguageInfo("Glagolitic Supplement", 122880, 122927));
		languageTags.Add(124928, new UnicodeLanguageInfo("Mende Kikakui", 124928, 125151));
		languageTags.Add(125184, new UnicodeLanguageInfo("Adlam", 125184, 125279));
		languageTags.Add(126064, new UnicodeLanguageInfo("Indic Siyaq Numbers", 126064, 126143));
		languageTags.Add(126464, new UnicodeLanguageInfo("Arabic", 126464, 126719));
		languageTags.Add(126976, new UnicodeLanguageInfo("Mahjong Tiles", 126976, 127023));
		languageTags.Add(127024, new UnicodeLanguageInfo("Domino Tiles", 127024, 127135));
		languageTags.Add(127136, new UnicodeLanguageInfo("Playing Cards", 127136, 127231));
		languageTags.Add(127232, new UnicodeLanguageInfo("Alphanumeric", 127232, 127487));
		languageTags.Add(127488, new UnicodeLanguageInfo("Ideographic", 127488, 127743));
		languageTags.Add(127744, new UnicodeLanguageInfo("Miscellaneous Symbols and Pictographs", 127744, 128511));
		languageTags.Add(128512, new UnicodeLanguageInfo("Emoticons", 128512, 128591));
		languageTags.Add(128592, new UnicodeLanguageInfo("Ornamental Dingbats", 128592, 128639));
		languageTags.Add(128640, new UnicodeLanguageInfo("Transport and Map Symbols", 128640, 128767));
		languageTags.Add(128768, new UnicodeLanguageInfo("Alchemical Symbols", 128768, 128895));
		languageTags.Add(128896, new UnicodeLanguageInfo("Geometric Shapes Extended", 128896, 129023));
		languageTags.Add(129024, new UnicodeLanguageInfo("Supplemental", 129024, 129279));
		languageTags.Add(129280, new UnicodeLanguageInfo("Supplemental", 129280, 129535));
		languageTags.Add(129536, new UnicodeLanguageInfo("Chess Symbols", 129536, 129647));
		languageTags.Add(131072, new UnicodeLanguageInfo("CJK", 131072, 173791));
		languageTags.Add(173824, new UnicodeLanguageInfo("CJK", 173824, 177983));
		languageTags.Add(177984, new UnicodeLanguageInfo("CJK", 177984, 178207));
		languageTags.Add(178208, new UnicodeLanguageInfo("CJK", 178208, 183983));
		languageTags.Add(183984, new UnicodeLanguageInfo("CJK", 183984, 191471));
		languageTags.Add(194560, new UnicodeLanguageInfo("CJK", 194560, 195103));
		languageTags.Add(917504, new UnicodeLanguageInfo("Tags", 917504, 917631));
		languageTags.Add(917760, new UnicodeLanguageInfo("Variation Selectors Supplement", 917760, 917999));
		languageTags.Add(983040, new UnicodeLanguageInfo("Supplementary", 983040, 1048575));
		languageTags.Add(1048576, new UnicodeLanguageInfo("Supplementary", 1048576, 1114111));
	}

	internal static bool IsDiscardGlyph(int charCode)
	{
		bool flag = false;
		if (charCode > 917535)
		{
			if (charCode < 917632)
			{
				flag = true;
			}
		}
		else if (Array.BinarySearch(dicardCharacters, charCode) > -1)
		{
			flag = true;
		}
		if (!flag)
		{
			return charCode == 173;
		}
		return true;
	}
}
