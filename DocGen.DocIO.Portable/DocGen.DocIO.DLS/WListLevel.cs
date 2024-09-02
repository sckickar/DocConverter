using System;
using System.Text;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class WListLevel : XDLSSerializableBase
{
	internal int restartLevel = -1;

	private readonly string[] DEF_NUMBER_WORDS = new string[19]
	{
		"one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
		"eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"
	};

	private readonly string[] DEF_TENS_WORDS = new string[9] { "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

	internal const string Level1Str = "\0";

	internal const string Level2Str = "\u0001";

	internal const string Level3Str = "\u0002";

	internal const string Level4Str = "\u0003";

	internal const string Level5Str = "\u0004";

	internal const string Level6Str = "\u0005";

	internal const string Level7Str = "\u0006";

	internal const string Level8Str = "\a";

	internal const string Level9Str = "\b";

	private WCharacterFormat m_chFormat;

	private WParagraphFormat m_prFormat;

	private string m_numberPrefix;

	private string m_numberSufix;

	private string m_layoutNumPref = string.Empty;

	private string m_bulletChar;

	private int m_startAt;

	private ListNumberAlignment m_alignment;

	private ListPatternType m_patternType;

	private FollowCharacterType m_followChar;

	private byte[] m_charOffset = new byte[9];

	private int m_legacySpace;

	private int m_legacyIndent;

	private string m_pStyle;

	private WPicture m_picBullet;

	private short m_picButtetId;

	private byte m_bFlags;

	private string m_levelText;

	public ListNumberAlignment NumberAlignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
		}
	}

	public int StartAt
	{
		get
		{
			return m_startAt;
		}
		set
		{
			m_startAt = value;
		}
	}

	public float TabSpaceAfter
	{
		get
		{
			if (m_prFormat.Tabs.Count > 0)
			{
				return m_prFormat.Tabs[0].Position;
			}
			return 0f;
		}
		set
		{
			m_prFormat.Tabs.AddTab(value);
		}
	}

	public float TextPosition
	{
		get
		{
			return m_prFormat.LeftIndent;
		}
		set
		{
			m_prFormat.SetPropertyValue(2, value);
		}
	}

	public string NumberPrefix
	{
		get
		{
			return m_numberPrefix;
		}
		set
		{
			m_numberPrefix = value;
		}
	}

	[Obsolete("This property has been deprecated. Use the NumberSuffix property of WListLevel class to set/get the suffix after the number for the specified list level.")]
	public string NumberSufix
	{
		get
		{
			return m_numberSufix;
		}
		set
		{
			m_numberSufix = value;
		}
	}

	public string NumberSuffix
	{
		get
		{
			return m_numberSufix;
		}
		set
		{
			m_numberSufix = value;
		}
	}

	public string BulletCharacter
	{
		get
		{
			return m_bulletChar;
		}
		set
		{
			m_bulletChar = value;
		}
	}

	public ListPatternType PatternType
	{
		get
		{
			return m_patternType;
		}
		set
		{
			m_patternType = value;
		}
	}

	public bool NoRestartByHigher
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public WCharacterFormat CharacterFormat => m_chFormat;

	public WParagraphFormat ParagraphFormat => m_prFormat;

	protected ListStyle OwnerListStyle => base.OwnerBase as ListStyle;

	protected WListLevel PreviousLevel
	{
		get
		{
			ListStyle ownerListStyle = OwnerListStyle;
			if (ownerListStyle != null)
			{
				int num = ownerListStyle.Levels.IndexOf(this);
				if (num > 0)
				{
					return ownerListStyle.Levels[num - 1];
				}
			}
			return null;
		}
	}

	public FollowCharacterType FollowCharacter
	{
		get
		{
			return m_followChar;
		}
		set
		{
			m_followChar = value;
		}
	}

	public bool IsLegalStyleNumbering
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public float NumberPosition
	{
		get
		{
			return m_prFormat.FirstLineIndent;
		}
		set
		{
			m_prFormat.SetPropertyValue(5, value);
		}
	}

	public bool UsePrevLevelPattern
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool Word6Legacy
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal int LegacySpace
	{
		get
		{
			return m_legacySpace;
		}
		set
		{
			m_legacySpace = value;
		}
	}

	internal int LegacyIndent
	{
		get
		{
			return m_legacyIndent;
		}
		set
		{
			m_legacyIndent = value;
		}
	}

	internal string ParaStyleName
	{
		get
		{
			return m_pStyle;
		}
		set
		{
			m_pStyle = value;
		}
	}

	internal bool NoLevelText
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal int LevelNumber
	{
		get
		{
			if (OwnerListStyle == null)
			{
				if (base.OwnerBase is OverrideLevelFormat)
				{
					OverrideLevelFormat overrideLevelFormat = base.OwnerBase as OverrideLevelFormat;
					return (overrideLevelFormat.OwnerBase as ListOverrideStyle).OverrideLevels.GetLevelNumber(overrideLevelFormat);
				}
				return -1;
			}
			return OwnerListStyle.Levels.IndexOf(this);
		}
	}

	internal WPicture PicBullet
	{
		get
		{
			return m_picBullet;
		}
		set
		{
			m_picBullet = value;
			m_picBullet.SetOwner(this);
		}
	}

	internal short PicBulletId
	{
		get
		{
			return m_picButtetId;
		}
		set
		{
			m_picButtetId = value;
		}
	}

	internal int PicIndex => CharacterFormat.ListPictureIndex;

	internal bool IsEmptyPicture
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal string LevelText
	{
		get
		{
			return m_levelText;
		}
		set
		{
			m_levelText = value;
		}
	}

	public WListLevel(ListStyle listStyle)
		: this(listStyle.Document)
	{
		SetOwner(listStyle);
	}

	internal WListLevel(WordDocument doc)
		: base(doc, null)
	{
		m_chFormat = m_doc.CreateCharacterFormatImpl();
		m_chFormat.SetOwner(this);
		m_prFormat = m_doc.CreateParagraphFormatImpl();
		m_prFormat.SetOwner(this);
	}

	public void CreateLayoutData(string numStr, byte[] characterOffsets, int levelNumber)
	{
		int num = 0;
		int num2 = 0;
		char[] separator = new char[2]
		{
			'\\',
			Convert.ToChar(levelNumber)
		};
		string[] array = numStr.Split(separator);
		int num3 = array[0].Length + 1;
		for (int i = 0; i < 9; i++)
		{
			if (characterOffsets[i] == num3)
			{
				if (i == 0)
				{
					m_numberPrefix = numStr.Substring(0, num3 - 1);
				}
				else
				{
					num = characterOffsets[i - 1];
					num2 = num3 - 1 - characterOffsets[i - 1];
					m_numberPrefix = numStr.Substring(num, num2);
				}
				if (i == 8 || characterOffsets[i + 1] == 0)
				{
					m_numberSufix = array[1];
					break;
				}
				num2 = characterOffsets[i + 1] - (num3 + 1);
				num = num3 + 1;
				m_numberSufix = numStr.Substring(num, num2);
				break;
			}
		}
	}

	public string GetListItemText(int listItemIndex, ListType listType)
	{
		return GetListItemText(listItemIndex, listType, new WParagraph(base.Document));
	}

	internal string GetListItemText(int listItemIndex, ListType listType, WParagraph paragraph)
	{
		string result = string.Empty;
		if (listType == ListType.Bulleted && PatternType != ListPatternType.Bullet)
		{
			listType = ListType.Numbered;
		}
		switch (listType)
		{
		case ListType.Numbered:
			if (m_numberPrefix != null && m_numberSufix != null)
			{
				result = GetNumberedItemText(listItemIndex, paragraph);
			}
			break;
		case ListType.Bulleted:
			result = m_bulletChar;
			break;
		default:
			result = "";
			break;
		}
		return result;
	}

	public WListLevel Clone()
	{
		return (WListLevel)CloneImpl();
	}

	internal static WListLevel CreateDefBulletLvl(float dxLeft, string str, ListStyle listStyle)
	{
		WListLevel wListLevel = listStyle.Document.CreateListLevelImpl(listStyle);
		wListLevel.m_startAt = 1;
		wListLevel.m_patternType = ListPatternType.Bullet;
		string fontName = "Times New Roman";
		switch (str)
		{
		case "\uf0b7":
			fontName = "Symbol";
			break;
		case "o":
			fontName = "Courier New";
			break;
		case "\uf0a7":
			fontName = "Wingdings";
			break;
		}
		wListLevel.m_chFormat.FontName = fontName;
		wListLevel.m_prFormat.SetPropertyValue(2, dxLeft);
		wListLevel.m_bulletChar = str;
		return wListLevel;
	}

	internal static WListLevel CreateDefNumberLvl(int dxLeft, int levelNumber, ListPatternType patType, ListNumberAlignment align, ListStyle listStyle)
	{
		WListLevel wListLevel = listStyle.Document.CreateListLevelImpl(listStyle);
		wListLevel.m_startAt = 1;
		wListLevel.m_patternType = patType;
		wListLevel.m_alignment = align;
		wListLevel.NumberPrefix = string.Empty;
		wListLevel.NumberSuffix = ".";
		wListLevel.m_prFormat.SetPropertyValue(2, (float)dxLeft);
		return wListLevel;
	}

	protected override object CloneImpl()
	{
		WListLevel wListLevel = (WListLevel)base.CloneImpl();
		wListLevel.m_chFormat = new WCharacterFormat(base.Document);
		wListLevel.m_chFormat.ImportContainer(CharacterFormat);
		wListLevel.m_chFormat.CopyProperties(CharacterFormat);
		wListLevel.m_chFormat.SetOwner(wListLevel);
		wListLevel.m_prFormat = new WParagraphFormat(base.Document);
		wListLevel.m_prFormat.ImportContainer(ParagraphFormat);
		wListLevel.m_prFormat.CopyProperties(ParagraphFormat);
		wListLevel.m_prFormat.SetOwner(wListLevel);
		if (PicBullet != null)
		{
			wListLevel.PicBullet = m_picBullet.Clone() as WPicture;
		}
		wListLevel.m_charOffset = new byte[m_charOffset.Length];
		m_charOffset.CopyTo(wListLevel.m_charOffset, 0);
		return wListLevel;
	}

	private string GetNumberedItemText(int listItemIndex, WParagraph paragraph)
	{
		(new char[1])[0] = '.';
		switch (m_patternType)
		{
		case ListPatternType.UpRoman:
			return m_numberPrefix + base.Document.GetAsRoman(listItemIndex + 1).ToUpper() + m_numberSufix;
		case ListPatternType.LowRoman:
			return m_numberPrefix + base.Document.GetAsRoman(listItemIndex + 1).ToLower() + m_numberSufix;
		case ListPatternType.UpLetter:
			return m_numberPrefix + base.Document.GetAsLetter(listItemIndex + 1).ToUpper() + m_numberSufix;
		case ListPatternType.LowLetter:
			return m_numberPrefix + base.Document.GetAsLetter(listItemIndex + 1).ToLower() + m_numberSufix;
		case ListPatternType.Ordinal:
			return m_numberPrefix + base.Document.GetOrdinal(listItemIndex + 1, CharacterFormat) + m_numberSufix;
		case ListPatternType.KanjiDigit:
		case ListPatternType.ChineseCountingThousand:
			return m_numberPrefix + base.Document.GetChineseExpression(listItemIndex + 1, m_patternType) + m_numberSufix;
		case ListPatternType.Arabic:
			return m_numberPrefix + (listItemIndex + 1) + m_numberSufix;
		case ListPatternType.LeadingZero:
			if (listItemIndex < 9)
			{
				return m_numberPrefix + "0" + (listItemIndex + 1) + m_numberSufix;
			}
			return m_numberPrefix + (listItemIndex + 1) + m_numberSufix;
		case ListPatternType.Number:
		{
			if ((paragraph.BreakCharacterFormat != null && paragraph.BreakCharacterFormat.HasValue(73) && paragraph.BreakCharacterFormat.LocaleIdASCII == 3082) || (paragraph.ParaStyle is WParagraphStyle && (paragraph.ParaStyle as WParagraphStyle).CharacterFormat.HasValue(73) && (paragraph.ParaStyle as WParagraphStyle).CharacterFormat.LocaleIdASCII == 3082) || (paragraph.ParaStyle is WParagraphStyle && (paragraph.ParaStyle as WParagraphStyle).BaseStyle != null && (paragraph.ParaStyle as WParagraphStyle).BaseStyle.CharacterFormat.HasValue(73) && (paragraph.ParaStyle as WParagraphStyle).BaseStyle.CharacterFormat.LocaleIdASCII == 3082))
			{
				string spanishCardinalTextString = base.Document.GetSpanishCardinalTextString(cardinalString: true, (listItemIndex + 1).ToString());
				spanishCardinalTextString = spanishCardinalTextString[0].ToString().ToUpper() + spanishCardinalTextString.Substring(1);
				return m_numberPrefix + spanishCardinalTextString + m_numberSufix;
			}
			string cardTextString = base.Document.GetCardTextString(cardinalString: true, (listItemIndex + 1).ToString());
			cardTextString = cardTextString[0].ToString().ToUpper() + cardTextString.Substring(1);
			return m_numberPrefix + cardTextString + m_numberSufix;
		}
		case ListPatternType.OrdinalText:
		{
			if ((paragraph.BreakCharacterFormat != null && paragraph.BreakCharacterFormat.HasValue(73) && paragraph.BreakCharacterFormat.LocaleIdASCII == 3082) || (paragraph.ParaStyle is WParagraphStyle && (paragraph.ParaStyle as WParagraphStyle).CharacterFormat.HasValue(73) && (paragraph.ParaStyle as WParagraphStyle).CharacterFormat.LocaleIdASCII == 3082) || (paragraph.ParaStyle is WParagraphStyle && (paragraph.ParaStyle as WParagraphStyle).BaseStyle != null && (paragraph.ParaStyle as WParagraphStyle).BaseStyle.CharacterFormat.HasValue(73) && (paragraph.ParaStyle as WParagraphStyle).BaseStyle.CharacterFormat.LocaleIdASCII == 3082))
			{
				string spanishOrdinalTextString = base.Document.GetSpanishOrdinalTextString(ordinalString: true, (listItemIndex + 1).ToString());
				spanishOrdinalTextString = spanishOrdinalTextString[0].ToString().ToUpper() + spanishOrdinalTextString.Substring(1);
				return m_numberPrefix + spanishOrdinalTextString + m_numberSufix;
			}
			string ordTextString = base.Document.GetOrdTextString(ordinalString: true, (listItemIndex + 1).ToString());
			ordTextString = ordTextString[0].ToString().ToUpper() + ordTextString.Substring(1);
			return m_numberPrefix + ordTextString + m_numberSufix;
		}
		case ListPatternType.None:
			return "";
		default:
			return m_numberPrefix + (listItemIndex + 1) + m_numberSufix;
		}
	}

	private string GetAsWord(int number, bool isOrdinal)
	{
		string text = "";
		if (isOrdinal)
		{
			throw new NotImplementedException("style list not implemented now");
		}
		if (number > 99)
		{
			throw new ArgumentOutOfRangeException("Cannot support number greater than 99");
		}
		if (number < 20)
		{
			return DEF_NUMBER_WORDS[number];
		}
		int num = (int)Math.Floor((double)number / 10.0);
		return DEF_TENS_WORDS[num] + "-" + DEF_NUMBER_WORDS[number - num * 10];
	}

	private string GenerateNumber(ref int value, int magnitude, string letter)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (value >= magnitude)
		{
			value -= magnitude;
			stringBuilder.Append(letter);
		}
		return stringBuilder.ToString();
	}

	internal new void Close()
	{
		m_charOffset = null;
		if (m_chFormat != null)
		{
			m_chFormat.Close();
			m_chFormat = null;
		}
		if (m_prFormat != null)
		{
			m_prFormat.Close();
			m_chFormat = null;
		}
	}

	internal bool Compare(WListLevel listLevel)
	{
		if (BulletCharacter != listLevel.BulletCharacter)
		{
			return false;
		}
		if (FollowCharacter != listLevel.FollowCharacter)
		{
			return false;
		}
		if (IsLegalStyleNumbering != listLevel.IsLegalStyleNumbering)
		{
			return false;
		}
		if (LegacyIndent != listLevel.LegacyIndent)
		{
			return false;
		}
		if (LegacySpace != listLevel.LegacySpace)
		{
			return false;
		}
		if (LevelNumber != listLevel.LevelNumber)
		{
			return false;
		}
		if (NoLevelText != listLevel.NoLevelText)
		{
			return false;
		}
		if (NoRestartByHigher != listLevel.NoRestartByHigher)
		{
			return false;
		}
		if (NumberAlignment != listLevel.NumberAlignment)
		{
			return false;
		}
		if (NumberPosition != listLevel.NumberPosition)
		{
			return false;
		}
		if (NumberPrefix != listLevel.NumberPrefix)
		{
			return false;
		}
		if (NumberSuffix != listLevel.NumberSuffix)
		{
			return false;
		}
		if (ParaStyleName != null && listLevel.ParaStyleName != null && !ParaStyleName.StartsWith(RemoveGUID(listLevel.ParaStyleName + "_")) && !listLevel.ParaStyleName.StartsWith(RemoveGUID(ParaStyleName + "_")) && !ParaStyleName.Equals(listLevel.ParaStyleName))
		{
			return false;
		}
		if (PatternType != listLevel.PatternType)
		{
			return false;
		}
		if (PicBullet != listLevel.PicBullet)
		{
			return false;
		}
		if (PicBulletId != listLevel.PicBulletId)
		{
			return false;
		}
		if (PicIndex != listLevel.PicIndex)
		{
			return false;
		}
		if (StartAt != listLevel.StartAt)
		{
			return false;
		}
		if (TabSpaceAfter != listLevel.TabSpaceAfter)
		{
			return false;
		}
		if (TextPosition != listLevel.TextPosition)
		{
			return false;
		}
		if (UsePrevLevelPattern != listLevel.UsePrevLevelPattern)
		{
			return false;
		}
		if (Word6Legacy != listLevel.Word6Legacy)
		{
			return false;
		}
		if (ParagraphFormat != null && listLevel.ParagraphFormat != null)
		{
			if (!ParagraphFormat.Compare(listLevel.ParagraphFormat))
			{
				return false;
			}
		}
		else if ((ParagraphFormat != null && listLevel.ParagraphFormat == null) || (ParagraphFormat == null && listLevel.ParagraphFormat != null))
		{
			return false;
		}
		if (CharacterFormat != null && listLevel.CharacterFormat != null)
		{
			if (!CharacterFormat.Compare(listLevel.CharacterFormat))
			{
				return false;
			}
		}
		else if ((CharacterFormat != null && listLevel.CharacterFormat == null) || (CharacterFormat == null && listLevel.CharacterFormat != null))
		{
			return false;
		}
		return true;
	}

	private string RemoveGUID(string styleName)
	{
		if (Style.HasGuid(styleName, out var guid))
		{
			styleName = styleName.Substring(0, styleName.IndexOf(guid));
		}
		return styleName;
	}

	internal string CheckNumberPrefOrSuf(string text)
	{
		if (text == null || text == string.Empty)
		{
			return text;
		}
		return text.Replace("%1", "\0").Replace("%2", "\u0001").Replace("%3", "\u0002")
			.Replace("%4", "\u0003")
			.Replace("%5", "\u0004")
			.Replace("%6", "\u0005")
			.Replace("%7", "\u0006")
			.Replace("%8", "\a")
			.Replace("%9", "\b");
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("Indent"))
		{
			TextPosition = reader.ReadFloat("Indent");
		}
		if (reader.HasAttribute("PrefPattern"))
		{
			NumberPrefix = reader.ReadString("PrefPattern");
		}
		else
		{
			NumberPrefix = null;
		}
		if (reader.HasAttribute("SufPattern"))
		{
			NumberSuffix = reader.ReadString("SufPattern");
		}
		else
		{
			NumberSuffix = null;
		}
		if (reader.HasAttribute("BulletPattern"))
		{
			BulletCharacter = reader.ReadString("BulletPattern");
		}
		if (reader.HasAttribute("PatternType"))
		{
			PatternType = (ListPatternType)(object)reader.ReadEnum("PatternType", typeof(ListPatternType));
		}
		if (reader.HasAttribute("PrevPattern"))
		{
			UsePrevLevelPattern = reader.ReadBoolean("PrevPattern");
		}
		if (reader.HasAttribute("StartAt"))
		{
			StartAt = reader.ReadInt("StartAt");
		}
		if (reader.HasAttribute("NumberAlign"))
		{
			NumberAlignment = (ListNumberAlignment)(object)reader.ReadEnum("NumberAlign", typeof(ListNumberAlignment));
		}
		if (reader.HasAttribute("FollowCharacter"))
		{
			FollowCharacter = (FollowCharacterType)(object)reader.ReadEnum("FollowCharacter", typeof(FollowCharacterType));
		}
		if (reader.HasAttribute("IsLegal"))
		{
			IsLegalStyleNumbering = reader.ReadBoolean("IsLegal");
		}
		if (reader.HasAttribute("NoRestart"))
		{
			NoRestartByHigher = reader.ReadBoolean("NoRestart");
		}
		if (reader.HasAttribute("Legacy"))
		{
			Word6Legacy = reader.ReadBoolean("Legacy");
		}
		if (reader.HasAttribute("LegacyIndent"))
		{
			m_legacyIndent = reader.ReadInt("LegacyIndent");
		}
		if (reader.HasAttribute("LegacySpace"))
		{
			m_legacySpace = reader.ReadInt("LegacySpace");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("Indent", TextPosition);
		writer.WriteValue("PrefPattern", NumberPrefix);
		writer.WriteValue("SufPattern", NumberSuffix);
		writer.WriteValue("BulletPattern", BulletCharacter);
		writer.WriteValue("PatternType", PatternType);
		writer.WriteValue("PrevPattern", UsePrevLevelPattern);
		writer.WriteValue("StartAt", StartAt);
		ListStyle ownerListStyle = OwnerListStyle;
		if (ownerListStyle != null && ownerListStyle.ListType == ListType.Numbered)
		{
			writer.WriteValue("NumberAlign", NumberAlignment);
		}
		writer.WriteValue("IsLegal", IsLegalStyleNumbering);
		writer.WriteValue("FollowCharacter", FollowCharacter);
		writer.WriteValue("NoRestart", NoRestartByHigher);
		if (Word6Legacy)
		{
			writer.WriteValue("Legacy", Word6Legacy);
			writer.WriteValue("LegacyIndent", m_legacyIndent);
			writer.WriteValue("LegacySpace", m_legacySpace);
		}
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("paragraph-format", m_prFormat);
		base.XDLSHolder.AddElement("character-format", m_chFormat);
	}
}
