using System;
using DocGen.Drawing;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class TextProperties
{
	private string m_fontFamily;

	private string m_fontName;

	private double m_fontSize;

	private FontWeight m_fontWeight;

	private int m_textRotationAngle;

	private float m_textScale;

	private Color m_backgroundColor;

	private Color m_color;

	private string m_countryCode;

	private string m_textPosition;

	private ODFFontStyle m_fontStyle;

	private FontVariant m_fontVariant;

	private bool m_hyphenate;

	private int m_hyphenation_push_char_count;

	private int m_hyphenation_remain_char_count;

	private string m_language;

	private float m_letterSpacing;

	private bool m_shadow;

	private Transform m_textTransform;

	private string m_countryAsian;

	private string m_countryComplex;

	private string m_font_charset;

	private string m_font_charset_asian;

	private string m_font_charset_complex;

	private string m_fontFamilyAsian;

	private string m_fontFamliyComplex;

	private FontFamilyGeneric m_fontFamilyGeneric;

	private FontFamilyGeneric m_fontFamilyGenericComplex;

	private FontFamilyGeneric m_fontFamilyGenericAsian;

	private string m_fontNameAsian;

	private string m_fontNameComplex;

	private FontPitch m_fontPitch;

	private FontPitch m_fontPitchAsian;

	private FontPitch m_fontPitchComplex;

	private int m_fontSizeRel;

	private int m_fontSizeRelAsian;

	private int m_fontSizeRelComplex;

	private bool m_fontStyleAsian;

	private bool m_fontStyleComplex;

	private string m_fontStyleName;

	private string m_fontStyleNameComplex;

	private string m_fontStyleNameAsian;

	private FontWeight m_fontWeightComplex;

	private FontWeight m_fontWeightAsian;

	private string m_languageAsian;

	private string m_languageComplex;

	private bool m_letterKerning;

	private string m_rfcLanguageTag;

	private string m_rfcLanguageTagAsian;

	private string m_rfcLanguageTagComplex;

	private bool m_textBlinking;

	private Combine m_textCombine;

	private char m_textCombineEndChar;

	private char m_textCombineStartChar;

	private Emphasize m_textEmphasize;

	private string m_linethroughColor;

	private LineMode m_linethroughMode;

	private BorderLineStyle m_linethroughStyle;

	private string m_linethroughText;

	private LineType m_linethroughTextStyle;

	private LineType m_linethroughType;

	private LineWidth m_linethroughWidth;

	private bool m_textOutline;

	private string m_textOverlineColor;

	private LineMode m_textOverlineMode;

	private BorderLineStyle m_textOverlineStyle;

	private LineType m_textOverlineType;

	private LineWidth m_textOverlineWidth;

	private TextRotationScale m_textRotationScale;

	private int m_textScaling;

	private string m_textUnderlineColor;

	private LineMode m_textUnderlineMode;

	private BorderLineStyle m_textUnderlineStyle;

	private LineType m_textUnderlineType;

	private LineWidth m_textUnderlineWidth;

	private bool m_useWindowFontColor;

	private string m_textCondition;

	private TextDisplay m_textDisplay;

	private bool m_isTextDisplay;

	private FontRelief m_fontRelief;

	private string m_charStyleName;

	internal int m_textFlag1;

	internal int m_textFlag2;

	internal int m_textFlag3;

	private const int FontReliefKey = 0;

	private const int TextDisplayKey = 1;

	private const int TextConditionKey = 2;

	private const int UseWindowFontColorKey = 3;

	private const int TextUnderlineWidthKey = 4;

	private const int TextUnderlineTypeKey = 5;

	private const int TextUnderlineStyleKey = 6;

	private const int TextUnderlineModeKey = 7;

	private const int TextUnderlineColorKey = 8;

	private const int TextScalingKey = 9;

	private const int TextRotationScaleKey = 10;

	private const int TextOverlineWidthKey = 11;

	private const int TextOverlineTypeKey = 12;

	private const int TextOverlineStyleKey = 13;

	private const int TextOverlineModeKey = 14;

	private const int TextOverlineColorKey = 15;

	private const int FontNameKey = 16;

	private const int FontSizeKey = 17;

	private const int TextRotationAngleKey = 18;

	private const int TextScaleKey = 19;

	private const int BackgroundColorKey = 20;

	private const int TextPositionKey = 21;

	private const int FontWeightKey = 22;

	private const int ColorKey = 23;

	private const int CountryCodeKey = 24;

	private const int FontFamilyKey = 25;

	private const int FontStyleKey = 26;

	private const int IsTextDisplayKey = 27;

	private const int FontVariantKey = 28;

	private const int HyphenateKey = 29;

	private const int HyphenationPushCharCountKey = 30;

	private const int HyphenationRemainCharCountKey = 31;

	private const int LanguageKey = 0;

	private const int LetterSpacingKey = 1;

	private const int ShadowKey = 2;

	private const int TextTransformKey = 3;

	private const int CountryAsianKey = 4;

	private const int CountryComplexKey = 5;

	private const int FontCharsetKey = 6;

	private const int FontCharsetAsianKey = 7;

	private const int FontCharsetComplexKey = 8;

	private const int FontFamliyComplexKey = 9;

	private const int FontFamilyAsianKey = 10;

	private const int FontFamilyGenericKey = 11;

	private const int FontFamilyGenericAsianKey = 12;

	private const int FontFamilyGenericComplexKey = 13;

	private const int FontNameComplexKey = 14;

	private const int FontNameAsianKey = 15;

	private const int FontPitchKey = 16;

	private const int FontPitchComplexKey = 17;

	private const int FontPitchAsianKey = 18;

	private const int FontSizeRelComplexKey = 19;

	private const int FontSizeRelAsianKey = 20;

	private const int FontSizeRelKey = 21;

	private const int FontStyleComplexKey = 22;

	private const int FontStyleAsianKey = 23;

	private const int FontStyleNameKey = 24;

	private const int FontStyleNameAsianKey = 25;

	private const int FontStyleNameComplexKey = 26;

	private const int FontWeightAsianKey = 27;

	private const int FontWeightComplexKey = 28;

	private const int RfcLanguageTagComplexKey = 29;

	private const int RfcLanguageTagAsianKey = 30;

	private const int RfcLanguageTagKey = 31;

	private const int LetterKerningKey = 0;

	private const int LanguageComplexKey = 1;

	private const int LanguageAsianKey = 2;

	private const int TextCombineStartCharKey = 3;

	private const int TextCombineEndCharKey = 4;

	private const int TextCombineKey = 5;

	private const int TextBlinkingKey = 6;

	private const int TextOutlineKey = 7;

	private const int LinethroughWidthKey = 8;

	private const int LinethroughTypeKey = 9;

	private const int LinethroughTextStyleKey = 10;

	private const int LinethroughTextKey = 11;

	private const byte LinethroughStyleKey = 12;

	private const int LinethroughModeKey = 13;

	private const int LinethroughColorKey = 14;

	private const int TextEmphasizeKey = 15;

	internal string CharStyleName
	{
		get
		{
			return m_charStyleName;
		}
		set
		{
			m_charStyleName = value;
		}
	}

	internal FontRelief FontRelief
	{
		get
		{
			return m_fontRelief;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFFFEu) | 1);
			m_fontRelief = value;
		}
	}

	public TextDisplay TextDisplay
	{
		get
		{
			return m_textDisplay;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFFFDu) | 2);
			m_textDisplay = value;
		}
	}

	internal string TextCondition
	{
		get
		{
			return m_textCondition;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFFFBu) | 4);
			m_textCondition = value;
		}
	}

	internal bool UseWindowFontColor
	{
		get
		{
			return m_useWindowFontColor;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFFF7u) | 8);
			m_useWindowFontColor = value;
		}
	}

	internal LineWidth TextUnderlineWidth
	{
		get
		{
			return m_textUnderlineWidth;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFFEFu) | 0x10);
			m_textUnderlineWidth = value;
		}
	}

	internal LineType TextUnderlineType
	{
		get
		{
			return m_textUnderlineType;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFFDFu) | 0x20);
			m_textUnderlineType = value;
		}
	}

	internal BorderLineStyle TextUnderlineStyle
	{
		get
		{
			return m_textUnderlineStyle;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFFBFu) | 0x40);
			m_textUnderlineStyle = value;
		}
	}

	internal LineMode TextUnderlineMode
	{
		get
		{
			return m_textUnderlineMode;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFF7Fu) | 0x80);
			m_textUnderlineMode = value;
		}
	}

	internal string TextUnderlineColor
	{
		get
		{
			return m_textUnderlineColor;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFEFFu) | 0x100);
			m_textUnderlineColor = value;
		}
	}

	internal int TextScaling
	{
		get
		{
			return m_textScaling;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFDFFu) | 0x200);
			m_textScaling = value;
		}
	}

	internal TextRotationScale TextRotationScale
	{
		get
		{
			return m_textRotationScale;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFFBFFu) | 0x400);
			m_textRotationScale = value;
		}
	}

	internal LineWidth TextOverlineWidth
	{
		get
		{
			return m_textOverlineWidth;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFF7FFu) | 0x800);
			m_textOverlineWidth = value;
		}
	}

	internal LineType TextOverlineType
	{
		get
		{
			return m_textOverlineType;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFEFFFu) | 0x1000);
			m_textOverlineType = value;
		}
	}

	internal BorderLineStyle TextOverlineStyle
	{
		get
		{
			return m_textOverlineStyle;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFDFFFu) | 0x2000);
			m_textOverlineStyle = value;
		}
	}

	internal LineMode TextOverlineMode
	{
		get
		{
			return m_textOverlineMode;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFFBFFFu) | 0x4000);
			m_textOverlineMode = value;
		}
	}

	public string TextOverlineColor
	{
		get
		{
			return m_textOverlineColor;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFF7FFFu) | 0x8000);
			m_textOverlineColor = value;
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
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFEFFFFu) | 0x10000);
			m_fontName = value;
		}
	}

	internal double FontSize
	{
		get
		{
			return m_fontSize;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFDFFFFu) | 0x20000);
			m_fontSize = value;
		}
	}

	internal int TextRotationAngle
	{
		get
		{
			return m_textRotationAngle;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFFBFFFFu) | 0x40000);
			m_textRotationAngle = value;
		}
	}

	internal float TextScale
	{
		get
		{
			return m_textScale;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFF7FFFFu) | 0x80000);
			m_textScale = value;
		}
	}

	internal Color BackgroundColor
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFEFFFFFu) | 0x100000);
			m_backgroundColor = value;
		}
	}

	internal string TextPosition
	{
		get
		{
			return m_textPosition;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFDFFFFFu) | 0x200000);
			m_textPosition = value;
		}
	}

	internal FontWeight FontWeight
	{
		get
		{
			return m_fontWeight;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFFBFFFFFu) | 0x400000);
			m_fontWeight = value;
		}
	}

	internal Color Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFF7FFFFFu) | 0x800000);
			m_color = value;
		}
	}

	internal string CountryCode
	{
		get
		{
			return m_countryCode;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFEFFFFFFu) | 0x1000000);
			m_countryCode = value;
		}
	}

	internal string FontFamily
	{
		get
		{
			return m_fontFamily;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFDFFFFFFu) | 0x2000000);
			m_fontFamily = value;
		}
	}

	internal ODFFontStyle FontStyle
	{
		get
		{
			return m_fontStyle;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xFBFFFFFFu) | 0x4000000);
			m_fontStyle = value;
		}
	}

	internal bool IsTextDisplay
	{
		get
		{
			return m_isTextDisplay;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xF7FFFFFFu) | 0x8000000);
			m_isTextDisplay = value;
		}
	}

	internal FontVariant FontVariant
	{
		get
		{
			return m_fontVariant;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xEFFFFFFFu) | 0x10000000);
			m_fontVariant = value;
		}
	}

	internal bool Hyphenate
	{
		get
		{
			return m_hyphenate;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xDFFFFFFFu) | 0x20000000);
			m_hyphenate = value;
		}
	}

	internal int HyphenationPushCharCount
	{
		get
		{
			return m_hyphenation_push_char_count;
		}
		set
		{
			m_textFlag1 = (int)((m_textFlag1 & 0xBFFFFFFFu) | 0x40000000);
			m_hyphenation_push_char_count = value;
		}
	}

	internal int HyphenationRemainCharCount
	{
		get
		{
			return m_hyphenation_remain_char_count;
		}
		set
		{
			m_textFlag1 = (m_textFlag1 & 0x7FFFFFFF) | int.MinValue;
			m_hyphenation_remain_char_count = value;
		}
	}

	internal string Language
	{
		get
		{
			return m_language;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFFFEu) | 1);
			m_language = value;
		}
	}

	internal float LetterSpacing
	{
		get
		{
			return m_letterSpacing;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFFFDu) | 2);
			m_letterSpacing = value;
		}
	}

	internal bool Shadow
	{
		get
		{
			return m_shadow;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFFFBu) | 4);
			m_shadow = value;
		}
	}

	internal Transform TextTransform
	{
		get
		{
			return m_textTransform;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFFF7u) | 8);
			m_textTransform = value;
		}
	}

	internal string CountryAsian
	{
		get
		{
			return m_countryAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFFEFu) | 0x10);
			m_countryAsian = value;
		}
	}

	internal string CountryComplex
	{
		get
		{
			return m_countryComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFFDFu) | 0x20);
			m_countryComplex = value;
		}
	}

	internal string FontCharset
	{
		get
		{
			return m_font_charset;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFFBFu) | 0x40);
			m_font_charset = value;
		}
	}

	internal string FontCharsetAsian
	{
		get
		{
			return m_font_charset_asian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFF7Fu) | 0x80);
			m_font_charset_asian = value;
		}
	}

	internal string FontCharsetComplex
	{
		get
		{
			return m_font_charset_complex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFEFFu) | 0x100);
			m_font_charset_complex = value;
		}
	}

	internal string FontFamliyComplex
	{
		get
		{
			return m_fontFamliyComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFDFFu) | 0x200);
			m_fontFamliyComplex = value;
		}
	}

	internal string FontFamilyAsian
	{
		get
		{
			return m_fontFamilyAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFFBFFu) | 0x400);
			m_fontFamilyAsian = value;
		}
	}

	internal FontFamilyGeneric FontFamilyGeneric
	{
		get
		{
			return m_fontFamilyGeneric;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFF7FFu) | 0x800);
			m_fontFamilyGeneric = value;
		}
	}

	internal FontFamilyGeneric FontFamilyGenericAsian
	{
		get
		{
			return m_fontFamilyGenericAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFEFFFu) | 0x1000);
			m_fontFamilyGenericAsian = value;
		}
	}

	internal FontFamilyGeneric FontFamilyGenericComplex
	{
		get
		{
			return m_fontFamilyGenericComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFDFFFu) | 0x2000);
			m_fontFamilyGenericComplex = value;
		}
	}

	internal string FontNameComplex
	{
		get
		{
			return m_fontNameComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFFBFFFu) | 0x4000);
			m_fontNameComplex = value;
		}
	}

	internal string FontNameAsian
	{
		get
		{
			return m_fontNameAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFF7FFFu) | 0x8000);
			m_fontNameAsian = value;
		}
	}

	internal FontPitch FontPitch
	{
		get
		{
			return m_fontPitch;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFEFFFFu) | 0x10000);
			m_fontPitch = value;
		}
	}

	internal FontPitch FontPitchComplex
	{
		get
		{
			return m_fontPitchComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFDFFFFu) | 0x20000);
			m_fontPitchComplex = value;
		}
	}

	internal FontPitch FontPitchAsian
	{
		get
		{
			return m_fontPitchAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFFBFFFFu) | 0x40000);
			m_fontPitchAsian = value;
		}
	}

	internal int FontSizeRelComplex
	{
		get
		{
			return m_fontSizeRelComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFF7FFFFu) | 0x80000);
			m_fontSizeRelComplex = value;
		}
	}

	internal int FontSizeRelAsian
	{
		get
		{
			return m_fontSizeRelAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFEFFFFFu) | 0x100000);
			m_fontSizeRelAsian = value;
		}
	}

	internal int FontSizeRel
	{
		get
		{
			return m_fontSizeRel;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFDFFFFFu) | 0x200000);
			m_fontSizeRel = value;
		}
	}

	internal bool FontStyleComplex
	{
		get
		{
			return m_fontStyleComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFFBFFFFFu) | 0x400000);
			m_fontStyleComplex = value;
		}
	}

	internal bool FontStyleAsian
	{
		get
		{
			return m_fontStyleAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFF7FFFFFu) | 0x800000);
			m_fontStyleAsian = value;
		}
	}

	internal string FontStyleName
	{
		get
		{
			return m_fontStyleName;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFEFFFFFFu) | 0x1000000);
			m_fontStyleName = value;
		}
	}

	internal string FontStyleNameAsian
	{
		get
		{
			return m_fontStyleNameAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFDFFFFFFu) | 0x2000000);
			m_fontStyleNameAsian = value;
		}
	}

	internal string FontStyleNameComplex
	{
		get
		{
			return m_fontStyleNameComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xFBFFFFFFu) | 0x4000000);
			m_fontStyleNameComplex = value;
		}
	}

	internal FontWeight FontWeightAsian
	{
		get
		{
			return m_fontWeightAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xF7FFFFFFu) | 0x8000000);
			m_fontWeightAsian = value;
		}
	}

	internal FontWeight FontWeightComplex
	{
		get
		{
			return m_fontWeightComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xEFFFFFFFu) | 0x10000000);
			m_fontWeightComplex = value;
		}
	}

	internal string RfcLanguageTagComplex
	{
		get
		{
			return m_rfcLanguageTagComplex;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xDFFFFFFFu) | 0x20000000);
			m_rfcLanguageTagComplex = value;
		}
	}

	internal string RfcLanguageTagAsian
	{
		get
		{
			return m_rfcLanguageTagAsian;
		}
		set
		{
			m_textFlag2 = (int)((m_textFlag2 & 0xBFFFFFFFu) | 0x40000000);
			m_rfcLanguageTagAsian = value;
		}
	}

	internal string RfcLanguageTag
	{
		get
		{
			return m_rfcLanguageTag;
		}
		set
		{
			m_textFlag2 = (m_textFlag2 & 0x7FFFFFFF) | int.MinValue;
			m_rfcLanguageTag = value;
		}
	}

	internal bool LetterKerning
	{
		get
		{
			return m_letterKerning;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFFFE) | 1;
			m_letterKerning = value;
		}
	}

	internal string LanguageComplex
	{
		get
		{
			return m_languageComplex;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFFFD) | 2;
			m_languageComplex = value;
		}
	}

	internal string LanguageAsian
	{
		get
		{
			return m_languageAsian;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFFFB) | 4;
			m_languageAsian = value;
		}
	}

	internal char TextCombineStartChar
	{
		get
		{
			return m_textCombineStartChar;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFFF7) | 8;
			m_textCombineStartChar = value;
		}
	}

	internal char TextCombineEndChar
	{
		get
		{
			return m_textCombineEndChar;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFFEF) | 0x10;
			m_textCombineEndChar = value;
		}
	}

	internal Combine TextCombine
	{
		get
		{
			return m_textCombine;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFFDF) | 0x20;
			m_textCombine = value;
		}
	}

	internal bool TextBlinking
	{
		get
		{
			return m_textBlinking;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFFBF) | 0x40;
			m_textBlinking = value;
		}
	}

	internal bool TextOutline
	{
		get
		{
			return m_textOutline;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFF7F) | 0x80;
			m_textOutline = value;
		}
	}

	internal LineWidth LinethroughWidth
	{
		get
		{
			return m_linethroughWidth;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFEFF) | 0x100;
			m_linethroughWidth = value;
		}
	}

	internal LineType LinethroughType
	{
		get
		{
			return m_linethroughType;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFDFF) | 0x200;
			m_linethroughType = value;
		}
	}

	internal LineType LinethroughTextStyle
	{
		get
		{
			return m_linethroughTextStyle;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xFBFF) | 0x200;
			m_linethroughTextStyle = value;
		}
	}

	internal string LinethroughText
	{
		get
		{
			return m_linethroughText;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xF7FF) | 0x400;
			m_linethroughText = value;
		}
	}

	internal BorderLineStyle LinethroughStyle
	{
		get
		{
			return m_linethroughStyle;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xEFFF) | 0x800;
			m_linethroughStyle = value;
		}
	}

	internal LineMode LinethroughMode
	{
		get
		{
			return m_linethroughMode;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xDFFF) | 0x1000;
			m_linethroughMode = value;
		}
	}

	internal string LinethroughColor
	{
		get
		{
			return m_linethroughColor;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0xBFFF) | 0x2000;
			m_linethroughColor = value;
		}
	}

	internal Emphasize TextEmphasize
	{
		get
		{
			return m_textEmphasize;
		}
		set
		{
			m_textFlag3 = (m_textFlag3 & 0x7FFF) | 0x4000;
			m_textEmphasize = value;
		}
	}

	internal bool HasKey(int propertyKey, int flagname)
	{
		return (flagname & (int)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	public override bool Equals(object obj)
	{
		TextProperties textProperties = obj as TextProperties;
		bool result = false;
		if (textProperties == null)
		{
			return false;
		}
		if (HasKey(20, m_textFlag1) && textProperties.HasKey(20, m_textFlag1))
		{
			result = BackgroundColor.Equals(textProperties.BackgroundColor);
		}
		if (HasKey(23, m_textFlag1) && textProperties.HasKey(23, m_textFlag1))
		{
			result = Color.Equals(textProperties.Color);
		}
		if (HasKey(4, m_textFlag2) && textProperties.HasKey(4, m_textFlag2))
		{
			result = CountryAsian.Equals(textProperties.CountryAsian);
		}
		if (HasKey(24, m_textFlag2) && textProperties.HasKey(24, m_textFlag2))
		{
			result = CountryCode.Equals(textProperties.CountryCode);
		}
		if (HasKey(5, m_textFlag2) && textProperties.HasKey(5, m_textFlag2))
		{
			result = CountryComplex.Equals(textProperties.CountryComplex);
		}
		if (HasKey(6, m_textFlag2) && textProperties.HasKey(6, m_textFlag2))
		{
			result = FontCharset.Equals(textProperties.FontCharset);
		}
		if (HasKey(7, m_textFlag2) && textProperties.HasKey(7, m_textFlag2))
		{
			result = FontCharsetAsian.Equals(textProperties.FontCharsetAsian);
		}
		if (HasKey(8, m_textFlag2) && textProperties.HasKey(8, m_textFlag2))
		{
			result = FontCharsetComplex.Equals(textProperties.FontCharsetComplex);
		}
		if (HasKey(16, m_textFlag1) && textProperties.HasKey(16, m_textFlag1))
		{
			result = FontName.Equals(textProperties.FontName);
		}
		if (HasKey(15, m_textFlag2) && textProperties.HasKey(15, m_textFlag2))
		{
			result = FontNameAsian.Equals(textProperties.FontNameAsian);
		}
		if (HasKey(14, m_textFlag2) && textProperties.HasKey(14, m_textFlag2))
		{
			result = FontNameComplex.Equals(textProperties.FontNameComplex);
		}
		if (HasKey(16, m_textFlag2) && textProperties.HasKey(16, m_textFlag2))
		{
			result = FontPitch.Equals(textProperties.FontPitch);
		}
		if (HasKey(0, m_textFlag1) && textProperties.HasKey(0, m_textFlag1))
		{
			result = FontRelief.Equals(textProperties.FontRelief);
		}
		if (HasKey(17, m_textFlag1) && textProperties.HasKey(17, m_textFlag1))
		{
			result = FontSize.Equals(textProperties.FontSize);
		}
		if (HasKey(21, m_textFlag2) && textProperties.HasKey(21, m_textFlag2))
		{
			result = FontSizeRel.Equals(textProperties.FontSizeRel);
		}
		if (HasKey(26, m_textFlag2) && textProperties.HasKey(26, m_textFlag2))
		{
			result = FontStyle.Equals(textProperties.FontStyle);
		}
		if (HasKey(24, m_textFlag2) && textProperties.HasKey(24, m_textFlag2))
		{
			result = FontStyleName.Equals(textProperties.FontStyleName);
		}
		if (HasKey(28, m_textFlag1) && textProperties.HasKey(28, m_textFlag1))
		{
			result = FontVariant.Equals(textProperties.FontVariant);
		}
		if (HasKey(22, m_textFlag1) && textProperties.HasKey(22, m_textFlag1))
		{
			result = FontWeight.Equals(textProperties.FontWeight);
		}
		if (HasKey(29, m_textFlag1) && textProperties.HasKey(29, m_textFlag1))
		{
			result = Hyphenate.Equals(textProperties.Hyphenate);
		}
		if (HasKey(30, m_textFlag1) && textProperties.HasKey(30, m_textFlag1))
		{
			result = HyphenationPushCharCount.Equals(textProperties.HyphenationPushCharCount);
		}
		if (HasKey(31, m_textFlag1) && textProperties.HasKey(31, m_textFlag1))
		{
			result = HyphenationRemainCharCount.Equals(textProperties.HyphenationRemainCharCount);
		}
		if (HasKey(27, m_textFlag1) && textProperties.HasKey(27, m_textFlag1))
		{
			result = IsTextDisplay.Equals(textProperties.IsTextDisplay);
		}
		if (HasKey(0, m_textFlag2) && textProperties.HasKey(0, m_textFlag2))
		{
			result = Language.Equals(textProperties.Language);
		}
		if (HasKey(0, m_textFlag3) && textProperties.HasKey(0, m_textFlag3))
		{
			result = LetterKerning.Equals(textProperties.LetterKerning);
		}
		if (HasKey(1, m_textFlag3) && textProperties.HasKey(1, m_textFlag3))
		{
			result = LanguageComplex.Equals(textProperties.LanguageComplex);
		}
		if (HasKey(1, m_textFlag2) && textProperties.HasKey(1, m_textFlag2))
		{
			result = LetterSpacing.Equals(textProperties.LetterSpacing);
		}
		if (HasKey(14, m_textFlag3) && textProperties.HasKey(14, m_textFlag3))
		{
			result = LinethroughColor.Equals(textProperties.LinethroughColor);
		}
		if (HasKey(13, m_textFlag3) && textProperties.HasKey(13, m_textFlag3))
		{
			result = LinethroughMode.Equals(textProperties.LinethroughMode);
		}
		if (HasKey(12, m_textFlag3) && textProperties.HasKey(12, m_textFlag3))
		{
			result = LinethroughStyle.Equals(textProperties.LinethroughStyle);
		}
		if (HasKey(10, m_textFlag3) && textProperties.HasKey(10, m_textFlag3))
		{
			result = LinethroughTextStyle.Equals(textProperties.LinethroughTextStyle);
		}
		if (HasKey(9, m_textFlag3) && textProperties.HasKey(9, m_textFlag3))
		{
			result = LinethroughType.Equals(textProperties.LinethroughType);
		}
		if (HasKey(8, m_textFlag3) && textProperties.HasKey(8, m_textFlag3))
		{
			result = LinethroughWidth.Equals(textProperties.LinethroughWidth);
		}
		if (HasKey(31, m_textFlag2) && textProperties.HasKey(31, m_textFlag2))
		{
			result = RfcLanguageTag.Equals(textProperties.RfcLanguageTag);
		}
		if (HasKey(30, m_textFlag2) && textProperties.HasKey(30, m_textFlag2))
		{
			result = RfcLanguageTagAsian.Equals(textProperties.RfcLanguageTagAsian);
		}
		if (HasKey(29, m_textFlag2) && textProperties.HasKey(29, m_textFlag2))
		{
			result = RfcLanguageTagComplex.Equals(textProperties.RfcLanguageTagComplex);
		}
		if (HasKey(2, m_textFlag2) && textProperties.HasKey(2, m_textFlag2))
		{
			result = Shadow.Equals(textProperties.Shadow);
		}
		if (HasKey(6, m_textFlag3) && textProperties.HasKey(6, m_textFlag3))
		{
			result = TextBlinking.Equals(textProperties.TextBlinking);
		}
		if (HasKey(5, m_textFlag3) && textProperties.HasKey(5, m_textFlag3))
		{
			result = TextCombine.Equals(textProperties.TextCombine);
		}
		if (HasKey(4, m_textFlag3) && textProperties.HasKey(4, m_textFlag3))
		{
			result = TextCombineEndChar.Equals(textProperties.TextCombineEndChar);
		}
		if (HasKey(3, m_textFlag3) && textProperties.HasKey(3, m_textFlag3))
		{
			result = TextCombineStartChar.Equals(textProperties.TextCombineStartChar);
		}
		if (HasKey(2, m_textFlag1) && textProperties.HasKey(2, m_textFlag1))
		{
			result = TextCondition.Equals(textProperties.TextCondition);
		}
		if (HasKey(1, m_textFlag1) && textProperties.HasKey(1, m_textFlag1))
		{
			result = TextDisplay.Equals(textProperties.TextDisplay);
		}
		if (HasKey(15, m_textFlag3) && textProperties.HasKey(15, m_textFlag3))
		{
			result = TextEmphasize.Equals(textProperties.TextEmphasize);
		}
		if (HasKey(7, m_textFlag3) && textProperties.HasKey(7, m_textFlag3))
		{
			result = TextOutline.Equals(textProperties.TextOutline);
		}
		if (HasKey(15, m_textFlag1) && textProperties.HasKey(15, m_textFlag1))
		{
			result = TextOverlineColor.Equals(textProperties.TextOverlineColor);
		}
		if (HasKey(14, m_textFlag1) && textProperties.HasKey(14, m_textFlag1))
		{
			result = TextOverlineMode.Equals(textProperties.TextOverlineMode);
		}
		if (HasKey(13, m_textFlag1) && textProperties.HasKey(13, m_textFlag1))
		{
			result = TextOverlineStyle.Equals(textProperties.TextOverlineStyle);
		}
		if (HasKey(12, m_textFlag1) && textProperties.HasKey(12, m_textFlag1))
		{
			result = TextOverlineType.Equals(textProperties.TextOverlineType);
		}
		if (HasKey(11, m_textFlag1) && textProperties.HasKey(11, m_textFlag1))
		{
			result = TextOverlineWidth.Equals(textProperties.TextOverlineWidth);
		}
		if (HasKey(21, m_textFlag1) && textProperties.HasKey(21, m_textFlag1))
		{
			result = TextPosition.Equals(textProperties.TextPosition);
		}
		if (HasKey(18, m_textFlag1) && textProperties.HasKey(18, m_textFlag1))
		{
			result = TextRotationAngle.Equals(textProperties.TextRotationAngle);
		}
		if (HasKey(10, m_textFlag1) && textProperties.HasKey(10, m_textFlag1))
		{
			result = TextRotationScale.Equals(textProperties.TextRotationScale);
		}
		if (HasKey(19, m_textFlag1) && textProperties.HasKey(19, m_textFlag1))
		{
			result = TextScale.Equals(textProperties.TextScale);
		}
		if (HasKey(9, m_textFlag1) && textProperties.HasKey(9, m_textFlag1))
		{
			result = TextScaling.Equals(textProperties.TextScaling);
		}
		if (HasKey(3, m_textFlag2) && textProperties.HasKey(3, m_textFlag2))
		{
			result = TextTransform.Equals(textProperties.TextTransform);
		}
		if (HasKey(8, m_textFlag1) && textProperties.HasKey(8, m_textFlag1))
		{
			result = TextUnderlineColor.Equals(textProperties.TextUnderlineColor);
		}
		if (HasKey(7, m_textFlag1) && textProperties.HasKey(7, m_textFlag1))
		{
			result = TextUnderlineMode.Equals(textProperties.TextUnderlineMode);
		}
		if (HasKey(6, m_textFlag1) && textProperties.HasKey(6, m_textFlag1))
		{
			result = TextUnderlineStyle.Equals(textProperties.TextUnderlineStyle);
		}
		if (HasKey(5, m_textFlag1) && textProperties.HasKey(5, m_textFlag1))
		{
			result = TextUnderlineType.Equals(textProperties.TextUnderlineType);
		}
		if (HasKey(4, m_textFlag1) && textProperties.HasKey(4, m_textFlag1))
		{
			result = TextUnderlineWidth.Equals(textProperties.TextUnderlineWidth);
		}
		if (HasKey(3, m_textFlag1) && textProperties.HasKey(3, m_textFlag1))
		{
			result = UseWindowFontColor.Equals(textProperties.UseWindowFontColor);
		}
		return result;
	}
}
