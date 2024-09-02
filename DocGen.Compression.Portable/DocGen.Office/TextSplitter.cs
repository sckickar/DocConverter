using System.Collections.Generic;

namespace DocGen.Office;

internal class TextSplitter
{
	private static readonly char[] WordSplitCharacters = new char[42]
	{
		' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')',
		'*', '+', ',', '-', '.', '/', ':', ';', '<', '=',
		'>', '?', '@', '[', '\\', ']', '^', '_', '`', '{',
		'|', '}', '~', '،', '؛', '–', '—', '‘', '’', '”',
		'\u3000', '\u200f'
	};

	private static readonly char[] NumberNonReversingCharacters = new char[5] { ',', '.', '/', ':', '،' };

	internal static bool IsEastAsiaScript(FontScriptType scriptType)
	{
		if (scriptType != 0)
		{
			if (!FontScriptType.Japanese.HasFlag(scriptType) && !FontScriptType.Korean.HasFlag(scriptType))
			{
				return FontScriptType.Chinese.HasFlag(scriptType);
			}
			return true;
		}
		return false;
	}

	internal static bool IsComplexScript(FontScriptType scriptType)
	{
		if (scriptType != 0)
		{
			if (!FontScriptType.Arabic.HasFlag(scriptType))
			{
				return FontScriptType.Hebrew.HasFlag(scriptType);
			}
			return true;
		}
		return false;
	}

	private FontScriptType GetFontScriptType(char inputCharacter)
	{
		if (IsHindiChar(inputCharacter))
		{
			return FontScriptType.Hindi;
		}
		if (IsKoreanChar(inputCharacter))
		{
			return FontScriptType.Korean;
		}
		if (IsJapanese(inputCharacter))
		{
			return FontScriptType.Japanese;
		}
		if (IsChineseChar(inputCharacter))
		{
			return FontScriptType.Chinese;
		}
		if (IsThaiCharacter(inputCharacter))
		{
			return FontScriptType.Thai;
		}
		if (IsArabicChar(inputCharacter))
		{
			return FontScriptType.Arabic;
		}
		if (IsHebrewChar(inputCharacter))
		{
			return FontScriptType.Hebrew;
		}
		return FontScriptType.English;
	}

	private static FontScriptType GetFontScriptSubType(char inputCharacter)
	{
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Hindi_Devanagari, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Hindi_Devanagari, isStartRange: false))
		{
			return FontScriptType.Hindi_Devanagari;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Hindi_Devanagari_Extended, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Hindi_Devanagari_Extended, isStartRange: false))
		{
			return FontScriptType.Hindi_Devanagari_Extended;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Hindi_Vedic_Extensions, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Hindi_Vedic_Extensions, isStartRange: false))
		{
			return FontScriptType.Hindi_Vedic_Extensions;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Korean_Hangul, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Korean_Hangul, isStartRange: false))
		{
			return FontScriptType.Korean_Hangul;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo, isStartRange: false))
		{
			return FontScriptType.Korean_Hangul_Jamo;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Korean_Hangul_Compatibility_Jamo, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Korean_Hangul_Compatibility_Jamo, isStartRange: false))
		{
			return FontScriptType.Korean_Hangul_Compatibility_Jamo;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedA, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedA, isStartRange: false))
		{
			return FontScriptType.Korean_Hangul_Jamo_ExtendedA;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedB, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedB, isStartRange: false))
		{
			return FontScriptType.Korean_Hangul_Jamo_ExtendedB;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Korean_Hangul_Syllables, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Korean_Hangul_Syllables, isStartRange: false))
		{
			return FontScriptType.Korean_Hangul_Syllables;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Japanese_Katakana, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Japanese_Katakana, isStartRange: false))
		{
			return FontScriptType.Japanese_Katakana;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Japanese_Hiragana, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Japanese_Hiragana, isStartRange: false))
		{
			return FontScriptType.Japanese_Hiragana;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs, isStartRange: false))
		{
			return FontScriptType.Chinese_Unified_Ideographs;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionA, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionA, isStartRange: false))
		{
			return FontScriptType.Chinese_Unified_Ideographs_ExtensionA;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB1, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB1, isStartRange: false))
		{
			return FontScriptType.Chinese_Unified_Ideographs_ExtensionB1;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB2, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB2, isStartRange: false))
		{
			return FontScriptType.Chinese_Unified_Ideographs_ExtensionB2;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Chinese_Compatibility_Ideographs, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Chinese_Compatibility_Ideographs, isStartRange: false))
		{
			return FontScriptType.Chinese_Compatibility_Ideographs;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Chinese_HalfAndFull_width_Forms, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Chinese_HalfAndFull_width_Forms, isStartRange: false))
		{
			return FontScriptType.Chinese_HalfAndFull_width_Forms;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Chinese_Symbols_And_Punctuation, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Chinese_Symbols_And_Punctuation, isStartRange: false))
		{
			return FontScriptType.Chinese_Symbols_And_Punctuation;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Thai, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Thai, isStartRange: false))
		{
			return FontScriptType.Thai;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Arabic_Unicode, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Arabic_Unicode, isStartRange: false))
		{
			return FontScriptType.Arabic_Unicode;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Arabic_Supplement, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Arabic_Supplement, isStartRange: false))
		{
			return FontScriptType.Arabic_Supplement;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Arabic_ExtendedA, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Arabic_ExtendedA, isStartRange: false))
		{
			return FontScriptType.Arabic_ExtendedA;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsA, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsA, isStartRange: false))
		{
			return FontScriptType.Arabic_Presentation_FormsA;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsB, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsB, isStartRange: false))
		{
			return FontScriptType.Arabic_Presentation_FormsB;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Hebrew_Unicode, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Hebrew_Unicode, isStartRange: false))
		{
			return FontScriptType.Hebrew_Unicode;
		}
		if (inputCharacter >= GetUnicodeRange(FontScriptType.Hebrew_Alphabetic_Presentation_Forms, isStartRange: true) && inputCharacter <= GetUnicodeRange(FontScriptType.Hebrew_Alphabetic_Presentation_Forms, isStartRange: false))
		{
			return FontScriptType.Hebrew_Alphabetic_Presentation_Forms;
		}
		return FontScriptType.English;
	}

	internal static uint GetUnicodeRange(FontScriptType fontScriptType, bool isStartRange)
	{
		switch (fontScriptType)
		{
		case FontScriptType.Hindi_Devanagari:
			if (isStartRange)
			{
				return 2304u;
			}
			return 2431u;
		case FontScriptType.Hindi_Devanagari_Extended:
			if (isStartRange)
			{
				return 43232u;
			}
			return 43263u;
		case FontScriptType.Hindi_Vedic_Extensions:
			if (isStartRange)
			{
				return 7376u;
			}
			return 7423u;
		case FontScriptType.Korean_Hangul:
			if (isStartRange)
			{
				return 44032u;
			}
			return 55203u;
		case FontScriptType.Korean_Hangul_Jamo:
			if (isStartRange)
			{
				return 4352u;
			}
			return 4607u;
		case FontScriptType.Korean_Hangul_Compatibility_Jamo:
			if (isStartRange)
			{
				return 12592u;
			}
			return 12687u;
		case FontScriptType.Korean_Hangul_Jamo_ExtendedA:
			if (isStartRange)
			{
				return 43360u;
			}
			return 43391u;
		case FontScriptType.Korean_Hangul_Jamo_ExtendedB:
			if (isStartRange)
			{
				return 55216u;
			}
			return 55295u;
		case FontScriptType.Korean_Hangul_Syllables:
			if (isStartRange)
			{
				return 44032u;
			}
			return 55215u;
		case FontScriptType.Japanese_Katakana:
			if (isStartRange)
			{
				return 12448u;
			}
			return 12543u;
		case FontScriptType.Japanese_Hiragana:
			if (isStartRange)
			{
				return 12352u;
			}
			return 12447u;
		case FontScriptType.Chinese_Unified_Ideographs:
			if (isStartRange)
			{
				return 19968u;
			}
			return 40959u;
		case FontScriptType.Chinese_Unified_Ideographs_ExtensionA:
			if (isStartRange)
			{
				return 13312u;
			}
			return 19903u;
		case FontScriptType.Chinese_Unified_Ideographs_ExtensionB1:
			if (isStartRange)
			{
				return 55360u;
			}
			return 55401u;
		case FontScriptType.Chinese_Unified_Ideographs_ExtensionB2:
			if (isStartRange)
			{
				return 56320u;
			}
			return 57055u;
		case FontScriptType.Chinese_Compatibility_Ideographs:
			if (isStartRange)
			{
				return 43360u;
			}
			return 43391u;
		case FontScriptType.Chinese_HalfAndFull_width_Forms:
			if (isStartRange)
			{
				return 65280u;
			}
			return 65519u;
		case FontScriptType.Chinese_Symbols_And_Punctuation:
			if (isStartRange)
			{
				return 12288u;
			}
			return 12351u;
		case FontScriptType.Thai:
			if (isStartRange)
			{
				return 3584u;
			}
			return 3711u;
		case FontScriptType.Arabic_Unicode:
			if (isStartRange)
			{
				return 1536u;
			}
			return 1791u;
		case FontScriptType.Arabic_Supplement:
			if (isStartRange)
			{
				return 1872u;
			}
			return 1919u;
		case FontScriptType.Arabic_ExtendedA:
			if (isStartRange)
			{
				return 2208u;
			}
			return 2303u;
		case FontScriptType.Arabic_Presentation_FormsA:
			if (isStartRange)
			{
				return 64336u;
			}
			return 65023u;
		case FontScriptType.Arabic_Presentation_FormsB:
			if (isStartRange)
			{
				return 65136u;
			}
			return 65279u;
		case FontScriptType.Hebrew_Unicode:
			if (isStartRange)
			{
				return 1424u;
			}
			return 1535u;
		case FontScriptType.Hebrew_Alphabetic_Presentation_Forms:
			if (isStartRange)
			{
				return 64285u;
			}
			return 64335u;
		default:
			return 0u;
		}
	}

	internal string[] SplitTextByFontScriptType(string inputText, ref List<FontScriptType> fontScriptTypes)
	{
		return SplitTextByFontScriptType(inputText, ref fontScriptTypes, splitTextBasedOnSubType: false);
	}

	internal string[] SplitTextByFontScriptType(string inputText, ref List<FontScriptType> fontScriptTypes, bool splitTextBasedOnSubType)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(inputText))
		{
			return list.ToArray();
		}
		string text = string.Empty;
		FontScriptType fontScriptType = FontScriptType.English;
		FontScriptType fontScriptType2 = FontScriptType.English;
		for (int i = 0; i < inputText.Length; i++)
		{
			if (inputText[i] != ' ' && !char.IsHighSurrogate(inputText[i]) && !char.IsLowSurrogate(inputText[i]))
			{
				fontScriptType2 = ((!splitTextBasedOnSubType) ? GetFontScriptType(inputText[i]) : GetFontScriptSubType(inputText[i]));
			}
			if (text != string.Empty && fontScriptType2 != fontScriptType)
			{
				list.Add(text);
				fontScriptTypes.Add(fontScriptType);
				text = string.Empty;
			}
			text += inputText[i];
			fontScriptType = fontScriptType2;
		}
		if (text != string.Empty)
		{
			list.Add(text);
			fontScriptTypes.Add(fontScriptType2);
			text = string.Empty;
		}
		return list.ToArray();
	}

	internal string[] SplitTextByConsecutiveLtrAndRtl(string text, bool isTextBidi, bool isRTLLang, ref List<CharacterRangeType> characterRangeTypes, ref bool? isPrevLTRText, ref bool hasRTLCharacter)
	{
		int count = characterRangeTypes.Count;
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(text))
		{
			return list.ToArray();
		}
		int num = -1;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string text4 = string.Empty;
		string text5 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			int num2 = 0;
			bool flag = false;
			if ((isPrevLTRText.HasValue ? (!isPrevLTRText.Value) : isTextBidi) && char.IsNumber(text[i]))
			{
				text5 += text[i];
				num2 = 4;
			}
			else if (IsWordSplitChar(text[i]))
			{
				num2 = 2;
				text4 = ((!(flag = isTextBidi || ((byte)text[i] == 32 && text4 == string.Empty))) ? (text4 + text[i]) : (text4 + text[i]));
			}
			else if (IsRTLChar(text[i]) && !char.IsNumber(text[i]))
			{
				isPrevLTRText = false;
				hasRTLCharacter = true;
				text3 += text[i];
				num2 = 1;
			}
			else
			{
				isPrevLTRText = true;
				text2 += text[i];
			}
			if (text5 != string.Empty && num2 != 4)
			{
				list.Add(text5);
				characterRangeTypes.Add(CharacterRangeType.Number);
				text5 = string.Empty;
			}
			if (text3 != string.Empty && num2 != 1)
			{
				list.Add(text3);
				characterRangeTypes.Add(CharacterRangeType.RTL);
				text3 = string.Empty;
			}
			if (text2 != string.Empty && num2 != 0)
			{
				list.Add(text2);
				num = list.Count - 1;
				characterRangeTypes.Add(CharacterRangeType.LTR);
				text2 = string.Empty;
			}
			if (text4 != string.Empty && (num2 != 2 || flag))
			{
				list.Add(text4);
				characterRangeTypes.Add(CharacterRangeType.WordSplit);
				text4 = string.Empty;
			}
		}
		if (text5 != string.Empty)
		{
			list.Add(text5);
			characterRangeTypes.Add(CharacterRangeType.Number);
		}
		else if (text3 != string.Empty)
		{
			list.Add(text3);
			characterRangeTypes.Add(CharacterRangeType.RTL);
		}
		else if (text2 != string.Empty)
		{
			list.Add(text2);
			num = list.Count - 1;
			characterRangeTypes.Add(CharacterRangeType.LTR);
		}
		else if (text4 != string.Empty)
		{
			list.Add(text4);
			characterRangeTypes.Add(CharacterRangeType.WordSplit);
		}
		if (hasRTLCharacter || (isPrevLTRText.HasValue && !isPrevLTRText.Value))
		{
			for (int j = 1; j < list.Count; j++)
			{
				if (characterRangeTypes[j + count] == CharacterRangeType.WordSplit && list[j].Length == 1 && j + count + 1 < characterRangeTypes.Count && characterRangeTypes[j + count - 1] != CharacterRangeType.WordSplit && (characterRangeTypes[j + count - 1] != CharacterRangeType.Number || IsNumberNonReversingCharacter(list[j], isTextBidi)) && characterRangeTypes[j + count - 1] == characterRangeTypes[j + count + 1])
				{
					list[j - 1] = list[j - 1] + list[j] + list[j + 1];
					list.RemoveAt(j);
					list.RemoveAt(j);
					characterRangeTypes.RemoveAt(j + count);
					characterRangeTypes.RemoveAt(j + count);
					j--;
				}
			}
		}
		else if (num != -1)
		{
			if (isTextBidi)
			{
				for (int k = 1; k < num; k++)
				{
					if (characterRangeTypes[k + count] == CharacterRangeType.WordSplit && k < num && characterRangeTypes[k + count - 1] == CharacterRangeType.LTR)
					{
						text2 = string.Empty;
						int num3;
						for (num3 = k + 1; num3 <= num; num3++)
						{
							text2 += list[num3];
							list.RemoveAt(num3);
							characterRangeTypes.RemoveAt(num3 + count);
							num3--;
							num--;
						}
						list[k - 1] = list[k - 1] + list[k] + text2;
						list.RemoveAt(k);
						characterRangeTypes.RemoveAt(k + count);
						k--;
						num--;
					}
				}
			}
			else
			{
				list.Clear();
				list.Add(text);
			}
		}
		else if (!isTextBidi)
		{
			list.Clear();
			list.Add(text);
		}
		if (isTextBidi)
		{
			for (int l = 1; l < list.Count; l++)
			{
				CharacterRangeType characterRangeType = characterRangeTypes[l + count];
				if (characterRangeType == CharacterRangeType.WordSplit && list[l].Length == 1 && l + count + 1 < characterRangeTypes.Count && characterRangeTypes[l + count - 1] != CharacterRangeType.WordSplit && (characterRangeTypes[l + count - 1] != CharacterRangeType.Number || IsNumberNonReversingCharacter(list[l], isTextBidi) || !isRTLLang) && characterRangeTypes[l + count - 1] == characterRangeTypes[l + count + 1])
				{
					list[l - 1] = list[l - 1] + list[l] + list[l + 1];
					list.RemoveAt(l);
					list.RemoveAt(l);
					characterRangeTypes.RemoveAt(l + count);
					characterRangeTypes.RemoveAt(l + count);
					l--;
				}
				else if (characterRangeType == CharacterRangeType.WordSplit && characterRangeTypes[l + count - 1] == CharacterRangeType.Number && IsNonWordSplitCharacter(list[l]) && !isRTLLang)
				{
					list[l - 1] += list[l];
					list.RemoveAt(l);
					characterRangeTypes.RemoveAt(l + count);
					l--;
				}
				else if (characterRangeType == CharacterRangeType.LTR && (characterRangeTypes[l + count - 1] == CharacterRangeType.Number || characterRangeTypes[l + count - 1] == CharacterRangeType.LTR))
				{
					list[l - 1] += list[l];
					characterRangeTypes[l + count - 1] = CharacterRangeType.LTR;
					list.RemoveAt(l);
					characterRangeTypes.RemoveAt(l + count);
					l--;
				}
			}
		}
		return list.ToArray();
	}

	internal static bool IsRTLChar(char character)
	{
		if (!IsHebrewChar(character) && !IsArabicChar(character) && (character < '\ua980' || character > '꧟') && (character < '܀' || character > 'ݏ') && (character < 'ހ' || character > '\u07bf') && (character < 'ࡀ' || character > '\u085f') && (character < '߀' || character > '߿'))
		{
			if (character >= 'ࠀ')
			{
				return character <= '\u083f';
			}
			return false;
		}
		return true;
	}

	private bool IsNonWordSplitCharacter(string character)
	{
		bool result = false;
		foreach (char c in character)
		{
			if (c == '#' || c == '$' || c == '%')
			{
				result = true;
				continue;
			}
			result = false;
			break;
		}
		return result;
	}

	internal static bool IsNumberNonReversingCharacter(string character, bool isTextBidi)
	{
		char[] numberNonReversingCharacters = NumberNonReversingCharacters;
		foreach (char c in numberNonReversingCharacters)
		{
			if (character[0] == c && (c != '/' || !isTextBidi))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsWordSplitChar(char character)
	{
		char[] wordSplitCharacters = WordSplitCharacters;
		foreach (char c in wordSplitCharacters)
		{
			if (character == c)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsArabicChar(char character)
	{
		FontScriptType fontScriptSubType = GetFontScriptSubType(character);
		if (fontScriptSubType != 0)
		{
			return FontScriptType.Arabic.HasFlag(fontScriptSubType);
		}
		return false;
	}

	private static bool IsHebrewChar(char character)
	{
		FontScriptType fontScriptSubType = GetFontScriptSubType(character);
		if (fontScriptSubType != 0)
		{
			return FontScriptType.Hebrew.HasFlag(fontScriptSubType);
		}
		return false;
	}

	private static bool IsHindiChar(char character)
	{
		FontScriptType fontScriptSubType = GetFontScriptSubType(character);
		if (fontScriptSubType != 0)
		{
			return FontScriptType.Hindi.HasFlag(fontScriptSubType);
		}
		return false;
	}

	private static bool IsKoreanChar(char character)
	{
		FontScriptType fontScriptSubType = GetFontScriptSubType(character);
		if (fontScriptSubType != 0)
		{
			return FontScriptType.Korean.HasFlag(fontScriptSubType);
		}
		return false;
	}

	private static bool IsJapanese(char character)
	{
		FontScriptType fontScriptSubType = GetFontScriptSubType(character);
		if (fontScriptSubType != 0)
		{
			return FontScriptType.Japanese.HasFlag(fontScriptSubType);
		}
		return false;
	}

	private static bool IsThaiCharacter(char character)
	{
		return GetFontScriptSubType(character) == FontScriptType.Thai;
	}

	private static bool IsChineseChar(char character)
	{
		FontScriptType fontScriptSubType = GetFontScriptSubType(character);
		if (fontScriptSubType != 0)
		{
			return FontScriptType.Chinese.HasFlag(fontScriptSubType);
		}
		return false;
	}
}
