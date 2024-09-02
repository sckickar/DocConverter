using System;

namespace DocGen.Office;

public class FallbackFont
{
	private uint m_startUnicodeRange;

	private uint m_endUnicodeRange;

	private string m_fontNames;

	[CLSCompliant(false)]
	public uint StartUnicodeRange
	{
		get
		{
			return m_startUnicodeRange;
		}
		set
		{
			m_startUnicodeRange = value;
		}
	}

	[CLSCompliant(false)]
	public uint EndUnicodeRange
	{
		get
		{
			return m_endUnicodeRange;
		}
		set
		{
			m_endUnicodeRange = value;
		}
	}

	public ScriptType ScriptType => GetScriptType();

	public string FontNames
	{
		get
		{
			return m_fontNames;
		}
		set
		{
			m_fontNames = value;
		}
	}

	[CLSCompliant(false)]
	public FallbackFont(uint startUnicodeRange, uint endUnicodeRange, string fontNames)
	{
		m_startUnicodeRange = startUnicodeRange;
		m_endUnicodeRange = endUnicodeRange;
		m_fontNames = fontNames;
	}

	internal bool IsWithInRange(string inputText)
	{
		foreach (char c in inputText)
		{
			if (c != ' ' && c >= (ushort)StartUnicodeRange && c <= (ushort)EndUnicodeRange)
			{
				return true;
			}
		}
		return false;
	}

	private ScriptType GetScriptType()
	{
		if (IsHindiUnicodeRange())
		{
			return ScriptType.Hindi;
		}
		if (IsKoreanUnicodeRange())
		{
			return ScriptType.Korean;
		}
		if (IsJapaneseUnicodeRange())
		{
			return ScriptType.Japanese;
		}
		if (IsChineseUnicodeRange())
		{
			return ScriptType.Chinese;
		}
		if (IsThaiUnicodeRange())
		{
			return ScriptType.Thai;
		}
		if (IsArabicUnicodeRange())
		{
			return ScriptType.Arabic;
		}
		if (IsHebrewUnicodeRange())
		{
			return ScriptType.Hebrew;
		}
		return ScriptType.Unknown;
	}

	private bool IsArabicUnicodeRange()
	{
		if ((StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Unicode, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Unicode, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Supplement, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Supplement, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Arabic_ExtendedA, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Arabic_ExtendedA, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsA, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsA, isStartRange: false)))
		{
			if (StartUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsB, isStartRange: true))
			{
				return EndUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Arabic_Presentation_FormsB, isStartRange: false);
			}
			return false;
		}
		return true;
	}

	private bool IsHebrewUnicodeRange()
	{
		if (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Hebrew_Unicode, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Hebrew_Unicode, isStartRange: false))
		{
			if (StartUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Hebrew_Alphabetic_Presentation_Forms, isStartRange: true))
			{
				return EndUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Hebrew_Alphabetic_Presentation_Forms, isStartRange: false);
			}
			return false;
		}
		return true;
	}

	private bool IsHindiUnicodeRange()
	{
		if ((StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Devanagari, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Devanagari, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Devanagari_Extended, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Devanagari_Extended, isStartRange: false)))
		{
			if (StartUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Vedic_Extensions, isStartRange: true))
			{
				return EndUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Hindi_Vedic_Extensions, isStartRange: false);
			}
			return false;
		}
		return true;
	}

	private bool IsKoreanUnicodeRange()
	{
		if ((StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Compatibility_Jamo, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Compatibility_Jamo, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedA, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedA, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedB, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Jamo_ExtendedB, isStartRange: false)))
		{
			if (StartUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Syllables, isStartRange: true))
			{
				return EndUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Korean_Hangul_Syllables, isStartRange: false);
			}
			return false;
		}
		return true;
	}

	private bool IsJapaneseUnicodeRange()
	{
		if (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Japanese_Katakana, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Japanese_Katakana, isStartRange: false))
		{
			if (StartUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Japanese_Hiragana, isStartRange: true))
			{
				return EndUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Japanese_Hiragana, isStartRange: false);
			}
			return false;
		}
		return true;
	}

	private bool IsThaiUnicodeRange()
	{
		if (StartUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Thai, isStartRange: true))
		{
			return EndUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Thai, isStartRange: false);
		}
		return false;
	}

	private bool IsChineseUnicodeRange()
	{
		if ((StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionA, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionA, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB1, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB1, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB2, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Unified_Ideographs_ExtensionB2, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Compatibility_Ideographs, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Compatibility_Ideographs, isStartRange: false)) && (StartUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_HalfAndFull_width_Forms, isStartRange: true) || EndUnicodeRange != TextSplitter.GetUnicodeRange(FontScriptType.Chinese_HalfAndFull_width_Forms, isStartRange: false)))
		{
			if (StartUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Symbols_And_Punctuation, isStartRange: true))
			{
				return EndUnicodeRange == TextSplitter.GetUnicodeRange(FontScriptType.Chinese_Symbols_And_Punctuation, isStartRange: false);
			}
			return false;
		}
		return true;
	}
}
