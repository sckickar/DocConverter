using System.Collections.Generic;

namespace DocGen.Office;

public class FallbackFonts : List<FallbackFont>
{
	public void Add(ScriptType scriptType, string fontNames)
	{
		switch (scriptType)
		{
		case ScriptType.Arabic:
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Unicode, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Unicode, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Supplement, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Supplement, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Arabic_ExtendedA, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Arabic_ExtendedA, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsA, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsA, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsB, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsB, isStartRange: false), fontNames);
			break;
		case ScriptType.Hebrew:
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Hebrew_Unicode, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Hebrew_Unicode, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Hebrew_Alphabetic_Presentation_Forms, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Hebrew_Alphabetic_Presentation_Forms, isStartRange: false), fontNames);
			break;
		case ScriptType.Hindi:
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Devanagari, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Devanagari, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Devanagari_Extended, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Devanagari_Extended, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Vedic_Extensions, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Vedic_Extensions, isStartRange: false), fontNames);
			break;
		case ScriptType.Chinese:
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionA, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionA, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB1, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB1, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB2, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB2, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Compatibility_Ideographs, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Compatibility_Ideographs, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Chinese_HalfAndFull_width_Forms, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Chinese_HalfAndFull_width_Forms, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Symbols_And_Punctuation, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Symbols_And_Punctuation, isStartRange: false), fontNames);
			break;
		case ScriptType.Japanese:
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Japanese_Katakana, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Japanese_Katakana, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Japanese_Hiragana, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Japanese_Hiragana, isStartRange: false), fontNames);
			break;
		case ScriptType.Thai:
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Thai, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Thai, isStartRange: false), fontNames);
			break;
		case ScriptType.Korean:
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Compatibility_Jamo, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Compatibility_Jamo, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedA, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedA, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedB, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedB, isStartRange: false), fontNames);
			AddFallbackFont(TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Syllables, isStartRange: true), TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Syllables, isStartRange: false), fontNames);
			break;
		}
	}

	private void AddFallbackFont(uint startUnicodeRange, uint endUnicodeRange, string fontNames)
	{
		FallbackFont item = new FallbackFont(startUnicodeRange, endUnicodeRange, fontNames);
		Add(item);
	}

	public void InitializeDefault()
	{
		Add(ScriptType.Arabic, "Arial, Times New Roman, Microsoft Uighur");
		Add(ScriptType.Hebrew, "Arial, Times New Roman, David");
		Add(ScriptType.Hindi, "Mangal, Utsaah");
		Add(ScriptType.Chinese, "DengXian, MingLiU, MS Gothic");
		Add(ScriptType.Japanese, "Yu Mincho, MS Mincho");
		Add(ScriptType.Thai, "Tahoma, Microsoft Sans Serif");
		Add(ScriptType.Korean, "Malgun Gothic, Batang");
	}
}
