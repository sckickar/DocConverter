using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class WCharacterFormat : FormatBase, IOfficeRunFormat
{
	internal const string DEF_FONTFAMILY = "Times New Roman";

	internal const float DEF_FONTSIZE = 10f;

	internal const float DEF_SCALINGSIZE = 100f;

	internal const short FontKey = 0;

	internal const short TextColorKey = 1;

	internal const short FontNameKey = 2;

	internal const short FontSizeKey = 3;

	internal const short BoldKey = 4;

	internal const short ItalicKey = 5;

	internal const short StrikeKey = 6;

	internal const short UnderlineKey = 7;

	internal const short TextBkgColorKey = 9;

	internal const short SubSuperScriptKey = 10;

	internal const short DoubleStrikeKey = 14;

	internal const short AllCapsKey = 54;

	internal const short SpacingKey = 18;

	internal const short HiddenKey = 53;

	internal const short PositionKey = 17;

	internal const short LineBreakKey = 20;

	internal const short ShadowKey = 50;

	internal const short EmbossKey = 51;

	internal const short EngraveKey = 52;

	internal const short SmallCapsKey = 55;

	internal const short SpecVanishKey = 24;

	internal const short BidiKey = 58;

	internal const short BoldBidiKey = 59;

	internal const short ItalicBidiKey = 60;

	internal const short FontNameBidiKey = 61;

	internal const short FontSizeBidiKey = 62;

	internal const short HighlightColorKey = 63;

	internal const short LocaleIdASCIIKey = 73;

	internal const short LocaleIdFarEastKey = 74;

	internal const short LidBiKey = 75;

	internal const short FontNameAsciiKey = 68;

	internal const short FontNameFarEastKey = 69;

	internal const short FontNameNonFarEastKey = 70;

	internal const short UnderlineColorKey = 90;

	internal const short BorderKey = 67;

	internal const short OutlineKey = 71;

	internal const short IdctHintKey = 72;

	internal const short NoProofKey = 76;

	internal const short ForeColorKey = 77;

	internal const short TextureStyleKey = 78;

	internal const short FieldVanishKey = 109;

	internal const short EmphasisKey = 79;

	internal const short TextEffectkey = 80;

	internal const short SnapToGridKey = 81;

	internal const short BreakClearKey = 126;

	internal const short CharStyleNameKey = 91;

	internal const short ScalingKey = 127;

	internal const short ComplexScriptKey = 99;

	internal const short WebHiddenKey = 92;

	internal const short InserteRevisionKey = 103;

	internal const short DeleteRevisionKey = 104;

	internal const short ChangedFormatKey = 105;

	internal const short SpecialKey = 106;

	internal const short ListPicIndexKey = 107;

	internal const short ListHasPicKey = 108;

	internal const short ContextualAlternatesKey = 120;

	internal const short LigaturesKey = 121;

	internal const short NumberFormKey = 122;

	internal const short NumberSpacingKey = 123;

	internal const short StylisticSetKey = 124;

	internal const short KernKey = 125;

	internal const short AuthorNameKey = 8;

	internal const short FormatChangeAuthorNameKey = 12;

	internal const short DateTimeKey = 11;

	internal const short FormatChangeDateTimeKey = 15;

	internal const short RevisionNameKey = 128;

	internal const short CFELayoutKey = 13;

	internal const short FitTextKey = 16;

	internal const short FitTextIDKey = 19;

	protected string m_charStyleName;

	protected string m_symExFontName;

	private WCharacterFormat m_tableStyleCharacterFormat;

	private byte m_bFlags;

	private float m_reducedFontSize;

	private List<Stream> m_xmlProps;

	private BiDirectionalOverride m_biDirectionalOverride;

	private List<int> revisionKeys;

	private bool CancelOnChange
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

	internal bool CrossRefChecked
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

	internal float ReducedFontSize
	{
		get
		{
			return m_reducedFontSize;
		}
		set
		{
			m_reducedFontSize = value;
		}
	}

	internal string SymExFontName
	{
		get
		{
			return m_symExFontName;
		}
		set
		{
			m_symExFontName = value;
		}
	}

	internal bool IsDocReading
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

	public DocGen.Drawing.Font Font
	{
		get
		{
			DocGen.Drawing.Font font = base.Document.FontSettings.GetFont((SymExFontName != null) ? SymExFontName : FontName, FontSize, GetFontStyle(), FontScriptType.English);
			font.Italic = Italic;
			font.Bold = Bold;
			font.UnderlineStyle = UnderlineStyle;
			font.Strikeout = Strikeout;
			return font;
		}
		set
		{
			FontName = value.FontFamilyName;
			FontSize = value.SizeInPoints;
			Bold = value.Bold;
			Italic = value.Italic;
			Strikeout = value.Strikeout;
			UnderlineStyle = (value.Underline ? UnderlineStyle.Single : UnderlineStyle.None);
		}
	}

	public string FontName
	{
		get
		{
			return GetFontName(2);
		}
		set
		{
			base[2] = value;
			if (!base.Document.IsOpening || !HasValue(68))
			{
				FontNameAscii = value;
			}
			if (!base.Document.IsOpening && !HasValue(61))
			{
				FontNameBidi = value;
			}
			if (!base.Document.IsOpening)
			{
				FontNameFarEast = value;
			}
			if (!base.Document.IsOpening || !HasValue(70))
			{
				FontNameNonFarEast = value;
			}
			CheckCrossRef();
			if (!m_doc.UsedFontNames.Contains(value))
			{
				m_doc.UsedFontNames.Add(value);
			}
		}
	}

	internal BreakClearType BreakClear
	{
		get
		{
			return (BreakClearType)GetPropertyValue(126);
		}
		set
		{
			SetPropertyValue(126, value);
		}
	}

	public float FontSize
	{
		get
		{
			return (float)GetPropertyValue(3);
		}
		set
		{
			if (value < 0f || value > 1638f)
			{
				throw new ArgumentException("FontSize must be between 0 and 1638");
			}
			SetPropertyValue(3, value);
		}
	}

	internal float Scaling
	{
		get
		{
			return (float)GetPropertyValue(127);
		}
		set
		{
			SetPropertyValue(127, value);
		}
	}

	internal float Kern
	{
		get
		{
			return (float)GetPropertyValue(125);
		}
		set
		{
			SetPropertyValue(125, value);
		}
	}

	internal bool IsKernFont
	{
		get
		{
			float kern = Kern;
			if (kern > 0f && kern <= FontSize)
			{
				return true;
			}
			return false;
		}
	}

	public bool ComplexScript
	{
		get
		{
			return GetBoolPropertyValue(99);
		}
		set
		{
			SetPropertyValue(99, value);
		}
	}

	public bool Bold
	{
		get
		{
			return GetBoolPropertyValue(4);
		}
		set
		{
			SetPropertyValue(4, value);
		}
	}

	public bool Italic
	{
		get
		{
			return GetBoolPropertyValue(5);
		}
		set
		{
			SetPropertyValue(5, value);
		}
	}

	public bool Strikeout
	{
		get
		{
			return GetBoolPropertyValue(6);
		}
		set
		{
			if (value && DoubleStrike)
			{
				DoubleStrike = false;
			}
			SetPropertyValue(6, value);
		}
	}

	public bool DoubleStrike
	{
		get
		{
			return GetBoolPropertyValue(14);
		}
		set
		{
			if (value && Strikeout)
			{
				Strikeout = false;
			}
			SetPropertyValue(14, value);
		}
	}

	public UnderlineStyle UnderlineStyle
	{
		get
		{
			return (UnderlineStyle)GetPropertyValue(7);
		}
		set
		{
			if (value == UnderlineStyle.DotDot)
			{
				value = UnderlineStyle.None;
			}
			if (value.ToString().Length > 3)
			{
				SetPropertyValue(7, value);
			}
		}
	}

	internal Color UnderlineColor
	{
		get
		{
			return (Color)GetPropertyValue(90);
		}
		set
		{
			SetPropertyValue(90, value);
		}
	}

	public Color TextColor
	{
		get
		{
			return (Color)GetPropertyValue(1);
		}
		set
		{
			SetPropertyValue(1, value);
		}
	}

	public Color TextBackgroundColor
	{
		get
		{
			return (Color)GetPropertyValue(9);
		}
		set
		{
			SetPropertyValue(9, value);
		}
	}

	public SubSuperScript SubSuperScript
	{
		get
		{
			return (SubSuperScript)GetPropertyValue(10);
		}
		set
		{
			SetPropertyValue(10, value);
		}
	}

	public float CharacterSpacing
	{
		get
		{
			return (float)GetPropertyValue(18);
		}
		set
		{
			if (value < -1584f || value > 1584f)
			{
				throw new ArgumentException("CharacterSpacing must be between -1584 and 1584");
			}
			SetPropertyValue(18, value);
		}
	}

	public float Position
	{
		get
		{
			return (float)GetPropertyValue(17);
		}
		set
		{
			if (value < -1584f || value > 1584f)
			{
				throw new ArgumentException("Position must be between -1584 and 1584");
			}
			SetPropertyValue(17, value);
		}
	}

	internal bool LineBreak
	{
		get
		{
			return IsLineBreakNext();
		}
		set
		{
			SetLineBreakNext();
		}
	}

	public bool Shadow
	{
		get
		{
			return GetBoolPropertyValue(50);
		}
		set
		{
			if (value && ((HasValue(51) && Emboss) || (HasValue(52) && Engrave)))
			{
				Emboss = false;
				Engrave = false;
			}
			SetPropertyValue(50, value);
		}
	}

	public bool Emboss
	{
		get
		{
			return GetBoolPropertyValue(51);
		}
		set
		{
			if (value && ((HasValue(50) && Shadow) || (HasValue(71) && OutLine) || (HasValue(52) && Engrave)))
			{
				Shadow = false;
				OutLine = false;
				Engrave = false;
			}
			SetPropertyValue(51, value);
		}
	}

	public bool Engrave
	{
		get
		{
			return GetBoolPropertyValue(52);
		}
		set
		{
			if (value && ((HasValue(50) && Shadow) || (HasValue(71) && OutLine) || (HasValue(51) && Emboss)))
			{
				Shadow = false;
				OutLine = false;
				Emboss = false;
			}
			SetPropertyValue(52, value);
		}
	}

	public bool Hidden
	{
		get
		{
			return GetBoolPropertyValue(53);
		}
		set
		{
			SetPropertyValue(53, value);
		}
	}

	public bool AllCaps
	{
		get
		{
			return GetBoolPropertyValue(54);
		}
		set
		{
			if (value && SmallCaps && !base.Document.IsOpening)
			{
				SmallCaps = false;
			}
			SetPropertyValue(54, value);
		}
	}

	public bool SmallCaps
	{
		get
		{
			return GetBoolPropertyValue(55);
		}
		set
		{
			if (value && AllCaps && !base.Document.IsOpening)
			{
				AllCaps = false;
			}
			SetPropertyValue(55, value);
		}
	}

	internal bool SpecVanish
	{
		get
		{
			return GetBoolPropertyValue(24);
		}
		set
		{
			SetPropertyValue(24, value);
		}
	}

	internal BiDirectionalOverride BiDirectionalOverride
	{
		get
		{
			return m_biDirectionalOverride;
		}
		set
		{
			m_biDirectionalOverride = value;
		}
	}

	public bool Bidi
	{
		get
		{
			return GetBoolPropertyValue(58);
		}
		set
		{
			SetPropertyValue(58, value);
		}
	}

	public bool BoldBidi
	{
		get
		{
			return GetBoolPropertyValue(59);
		}
		set
		{
			SetPropertyValue(59, value);
		}
	}

	public bool ItalicBidi
	{
		get
		{
			return GetBoolPropertyValue(60);
		}
		set
		{
			SetPropertyValue(60, value);
		}
	}

	public float FontSizeBidi
	{
		get
		{
			return (float)GetPropertyValue(62);
		}
		set
		{
			if (value < 0f || value > 1638f)
			{
				throw new ArgumentException("FontSizeBi must be between 0 and 1638");
			}
			SetPropertyValue(62, value);
		}
	}

	public string FontNameBidi
	{
		get
		{
			return (string)base[61];
		}
		set
		{
			base[61] = value;
			CheckCrossRef();
		}
	}

	public Color HighlightColor
	{
		get
		{
			return (Color)GetPropertyValue(63);
		}
		set
		{
			SetPropertyValue(63, value);
		}
	}

	public Border Border => GetPropertyValue(67) as Border;

	internal EmphasisType EmphasisType
	{
		get
		{
			return (EmphasisType)GetPropertyValue(79);
		}
		set
		{
			SetPropertyValue(79, value);
		}
	}

	internal TextEffect TextEffect
	{
		get
		{
			return (TextEffect)GetPropertyValue(80);
		}
		set
		{
			SetPropertyValue(80, value);
		}
	}

	internal bool SnapToGrid
	{
		get
		{
			return GetBoolPropertyValue(81);
		}
		set
		{
			SetPropertyValue(81, value);
		}
	}

	internal bool WebHidden
	{
		get
		{
			return GetBoolPropertyValue(92);
		}
		set
		{
			SetPropertyValue(92, value);
		}
	}

	internal string FontNameAscii
	{
		get
		{
			return (string)base[68];
		}
		set
		{
			base[68] = value;
		}
	}

	internal string FontNameFarEast
	{
		get
		{
			return (string)base[69];
		}
		set
		{
			base[69] = value;
		}
	}

	internal string FontNameNonFarEast
	{
		get
		{
			return (string)base[70];
		}
		set
		{
			base[70] = value;
		}
	}

	internal FontHintType IdctHint
	{
		get
		{
			return (FontHintType)Convert.ToInt16(GetPropertyValue(72));
		}
		set
		{
			SetPropertyValue(72, value);
		}
	}

	public short LocaleIdASCII
	{
		get
		{
			return (short)GetPropertyValue(73);
		}
		set
		{
			SetPropertyValue(73, value);
		}
	}

	public short LocaleIdFarEast
	{
		get
		{
			return (short)GetPropertyValue(74);
		}
		set
		{
			SetPropertyValue(74, value);
		}
	}

	public short LocaleIdBidi
	{
		get
		{
			return (short)GetPropertyValue(75);
		}
		set
		{
			SetPropertyValue(75, value);
		}
	}

	internal bool NoProof
	{
		get
		{
			return GetBoolPropertyValue(76);
		}
		set
		{
			SetPropertyValue(76, value);
		}
	}

	internal Color ForeColor
	{
		get
		{
			return (Color)GetPropertyValue(77);
		}
		set
		{
			SetPropertyValue(77, value);
		}
	}

	internal TextureStyle TextureStyle
	{
		get
		{
			return (TextureStyle)GetPropertyValue(78);
		}
		set
		{
			SetPropertyValue(78, value);
		}
	}

	public bool OutLine
	{
		get
		{
			return GetBoolPropertyValue(71);
		}
		set
		{
			if (value && (Emboss || Engrave))
			{
				Emboss = false;
				Engrave = false;
			}
			SetPropertyValue(71, value);
		}
	}

	internal bool Special
	{
		get
		{
			return GetBoolPropertyValue(106);
		}
		set
		{
			SetPropertyValue(106, value);
		}
	}

	public string CharStyleName
	{
		get
		{
			return (string)GetPropertyValue(91);
		}
		internal set
		{
			if (base.OwnerBase != null)
			{
				if (base.OwnerBase.Document.Styles.FindByName(CharStyleName) is WCharacterStyle { IsRemoving: false, IsCustom: not false } wCharacterStyle && wCharacterStyle.RangeCollection.Contains(base.OwnerBase as Entity))
				{
					wCharacterStyle.RangeCollection.Remove(base.OwnerBase as Entity);
				}
				if (base.OwnerBase.Document.Styles.FindByName(value) is WCharacterStyle { IsCustom: not false } wCharacterStyle2)
				{
					wCharacterStyle2.RangeCollection.Add(base.OwnerBase as Entity);
				}
			}
			SetPropertyValue(91, value);
		}
	}

	internal string CharStyleId
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

	internal bool IsInsertRevision
	{
		get
		{
			return GetBoolPropertyValue(103);
		}
		set
		{
			SetPropertyValue(103, value);
		}
	}

	internal bool IsDeleteRevision
	{
		get
		{
			return GetBoolPropertyValue(104);
		}
		set
		{
			SetPropertyValue(104, value);
		}
	}

	internal bool IsChangedFormat
	{
		get
		{
			return (bool)GetPropertyValue(105);
		}
		set
		{
			if (value)
			{
				SetPropertyValue(105, true);
			}
		}
	}

	internal int ListPictureIndex
	{
		get
		{
			return (int)GetPropertyValue(107);
		}
		set
		{
			SetPropertyValue(107, value);
		}
	}

	internal bool ListHasPicture
	{
		get
		{
			return (bool)GetPropertyValue(108);
		}
		set
		{
			SetPropertyValue(108, value);
		}
	}

	internal WCharacterStyle CharStyle => GetCharStyleValue();

	internal bool FieldVanish
	{
		get
		{
			return GetBoolPropertyValue(109);
		}
		set
		{
			SetPropertyValue(109, value);
		}
	}

	internal WCharacterFormat TableStyleCharacterFormat
	{
		get
		{
			return m_tableStyleCharacterFormat;
		}
		set
		{
			m_tableStyleCharacterFormat = value;
		}
	}

	internal string AuthorName
	{
		get
		{
			return (string)GetPropertyValue(8);
		}
		set
		{
			SetPropertyValue(8, value);
		}
	}

	internal string FormatChangeAuthorName
	{
		get
		{
			return (string)GetPropertyValue(12);
		}
		set
		{
			SetPropertyValue(12, value);
		}
	}

	internal DateTime RevDateTime
	{
		get
		{
			return (DateTime)GetPropertyValue(11);
		}
		set
		{
			SetPropertyValue(11, value);
		}
	}

	internal string RevisionName
	{
		get
		{
			return (string)GetPropertyValue(128);
		}
		set
		{
			SetPropertyValue(128, value);
		}
	}

	internal DateTime FormatChangeDateTime
	{
		get
		{
			return (DateTime)GetPropertyValue(15);
		}
		set
		{
			SetPropertyValue(15, value);
		}
	}

	internal CFELayout CFELayout
	{
		get
		{
			return (CFELayout)GetPropertyValue(13);
		}
		set
		{
			SetPropertyValue(13, value);
		}
	}

	internal int FitTextWidth
	{
		get
		{
			return (int)GetPropertyValue(16);
		}
		set
		{
			SetPropertyValue(16, value);
		}
	}

	internal int FitTextID
	{
		get
		{
			return (int)GetPropertyValue(19);
		}
		set
		{
			SetPropertyValue(19, value);
		}
	}

	public bool UseContextualAlternates
	{
		get
		{
			return (bool)GetPropertyValue(120);
		}
		set
		{
			SetPropertyValue(120, value);
		}
	}

	public LigatureType Ligatures
	{
		get
		{
			return (LigatureType)GetPropertyValue(121);
		}
		set
		{
			SetPropertyValue(121, value);
		}
	}

	public NumberFormType NumberForm
	{
		get
		{
			return (NumberFormType)GetPropertyValue(122);
		}
		set
		{
			SetPropertyValue(122, value);
		}
	}

	public NumberSpacingType NumberSpacing
	{
		get
		{
			return (NumberSpacingType)GetPropertyValue(123);
		}
		set
		{
			SetPropertyValue(123, value);
		}
	}

	public StylisticSetType StylisticSet
	{
		get
		{
			return (StylisticSetType)GetPropertyValue(124);
		}
		set
		{
			SetPropertyValue(124, value);
		}
	}

	internal List<Stream> XmlProps
	{
		get
		{
			if (m_xmlProps == null)
			{
				m_xmlProps = new List<Stream>();
			}
			return m_xmlProps;
		}
	}

	internal List<int> RevisionKeys
	{
		get
		{
			if (revisionKeys == null)
			{
				revisionKeys = new List<int>(new int[8] { 103, 104, 105, 8, 12, 11, 15, 128 });
			}
			return revisionKeys;
		}
	}

	private WCharacterFormat()
	{
	}

	public WCharacterFormat(IWordDocument doc)
		: base(doc)
	{
	}

	internal WCharacterFormat(IWordDocument doc, Entity owner)
		: base(doc, owner)
	{
	}

	internal void RemoveFontNames()
	{
		base.PropertiesHash.Remove(2);
		base.PropertiesHash.Remove(68);
		base.PropertiesHash.Remove(61);
		base.PropertiesHash.Remove(69);
		base.PropertiesHash.Remove(70);
	}

	internal bool HasValueWithParent(int propertyKey)
	{
		bool flag = HasValue(propertyKey);
		if (!flag && base.BaseFormat != null && base.BaseFormat is WCharacterFormat)
		{
			flag = (base.BaseFormat as WCharacterFormat).HasValueWithParent(propertyKey);
		}
		if (!flag && CharStyle != null)
		{
			flag = CharStyle.CharacterFormat.HasValue(propertyKey);
		}
		if (!flag && base.Document != null && base.Document.DefCharFormat != null && base.Document.DefCharFormat != this)
		{
			flag = base.Document.DefCharFormat.HasValue(propertyKey);
		}
		return flag;
	}

	internal bool IsInheritedFromTableStyle(int propertyKey)
	{
		bool flag = HasValue(propertyKey);
		if (!flag && base.BaseFormat != null && base.BaseFormat is WCharacterFormat)
		{
			flag = (base.BaseFormat as WCharacterFormat).HasValueInBaseFormat(propertyKey);
		}
		if (!flag && CharStyle != null)
		{
			flag = CharStyle.CharacterFormat.HasValue(propertyKey);
		}
		if (!flag && TableStyleCharacterFormat != null && TableStyleCharacterFormat.HasValue(propertyKey))
		{
			return true;
		}
		return false;
	}

	internal bool HasValueInBaseFormat(int propertyKey)
	{
		bool flag = HasValue(propertyKey);
		if (!flag && base.BaseFormat != null && base.BaseFormat is WCharacterFormat)
		{
			flag = (base.BaseFormat as WCharacterFormat).HasValueInBaseFormat(propertyKey);
		}
		if (!flag && CharStyle != null)
		{
			flag = CharStyle.CharacterFormat.HasValue(propertyKey);
		}
		return flag;
	}

	internal DocGen.Drawing.Font GetFontToRender(FontScriptType scriptType)
	{
		DocGen.Drawing.Font font = base.Document.FontSettings.GetFont(GetFontNameToRender(scriptType), GetFontSizeToRender(), GetFontStyle(), scriptType);
		font.Italic = GetItalicToRender();
		font.Bold = GetBoldToRender();
		font.UnderlineStyle = UnderlineStyle;
		font.Strikeout = Strikeout;
		return font;
	}

	internal float GetFontSizeToRender()
	{
		if (ReducedFontSize != 0f)
		{
			return ReducedFontSize;
		}
		if (Bidi || ComplexScript)
		{
			return FontSizeBidi;
		}
		return FontSize;
	}

	internal bool GetBoldToRender()
	{
		if (Bidi || ComplexScript)
		{
			return BoldBidi;
		}
		return Bold;
	}

	internal bool GetItalicToRender()
	{
		if (Bidi || ComplexScript)
		{
			return ItalicBidi;
		}
		return Italic;
	}

	internal FontStyle GetFontStyle()
	{
		FontStyle fontStyle = FontStyle.Regular;
		if (GetBoldToRender())
		{
			fontStyle |= FontStyle.Bold;
		}
		if (GetItalicToRender())
		{
			fontStyle |= FontStyle.Italic;
		}
		if (base.Document.RevisionOptions.ShowRevisionMarks)
		{
			if (IsInsertRevision && IsNeedToShowInsertionMarkups())
			{
				fontStyle |= GetTextEffect(base.Document.RevisionOptions.InsertedTextEffect);
			}
			else if (IsDeleteRevision && IsNeedToShowDeletionMarkups())
			{
				fontStyle |= GetTextEffect(base.Document.RevisionOptions.DeletedTextEffect);
			}
		}
		return fontStyle;
	}

	private FontStyle GetTextEffect(RevisedTextEffect effect)
	{
		FontStyle result = FontStyle.Regular;
		switch (effect)
		{
		case RevisedTextEffect.Bold:
			result = FontStyle.Bold;
			break;
		case RevisedTextEffect.Italic:
			result = FontStyle.Italic;
			break;
		case RevisedTextEffect.StrikeThrough:
			result = FontStyle.Strikeout;
			break;
		case RevisedTextEffect.Underline:
			result = FontStyle.Underline;
			break;
		}
		return result;
	}

	internal bool IsNeedToShowInsertionMarkups()
	{
		if (base.Document != null)
		{
			return (base.Document.RevisionOptions.ShowMarkup & RevisionType.Insertions) == RevisionType.Insertions;
		}
		return false;
	}

	internal bool IsNeedToShowDeletionMarkups()
	{
		if (base.Document != null && (base.Document.RevisionOptions.ShowMarkup & RevisionType.Deletions) == RevisionType.Deletions)
		{
			return base.Document.RevisionOptions.ShowDeletedText;
		}
		return false;
	}

	private bool IsLineBreakNext()
	{
		bool result = false;
		OwnerHolder ownerBase = base.OwnerBase;
		if (ownerBase != null && ownerBase.OwnerBase is WParagraph)
		{
			WParagraph wParagraph = ownerBase.OwnerBase as WParagraph;
			int num = wParagraph.Items.IndexOf(ownerBase as IEntity);
			if (num < wParagraph.Items.Count - 1 && wParagraph.Items[num + 1] is Break)
			{
				Break @break = wParagraph.Items[num + 1] as Break;
				result = ((@break.BreakType == BreakType.LineBreak || @break.BreakType == BreakType.TextWrappingBreak) ? true : false);
			}
		}
		return result;
	}

	private void SetLineBreakNext()
	{
		OwnerHolder ownerBase = base.OwnerBase;
		if (ownerBase != null && ownerBase.OwnerBase is WParagraph)
		{
			WParagraph wParagraph = ownerBase.OwnerBase as WParagraph;
			int index = wParagraph.Items.IndexOf(ownerBase as IEntity) + 1;
			wParagraph.Items.Insert(index, new Break(wParagraph.Document, BreakType.LineBreak));
		}
	}

	internal object GetPropertyValue(int propKey)
	{
		return base[propKey];
	}

	internal bool GetBoolPropertyValue(short propKey)
	{
		bool flag = HasKey(propKey);
		bool complexBoolValue = GetComplexBoolValue(propKey);
		bool flag2 = false;
		bool flag3 = false;
		if (!flag)
		{
			FormatBase formatBase = null;
			if (CharStyle != null)
			{
				formatBase = CharStyle.CharacterFormat;
			}
			while (formatBase != null && formatBase is WCharacterFormat)
			{
				formatBase.HasKey(propKey);
				if (flag2)
				{
					flag2 = base.OwnerBase is WTextRange && formatBase.OwnerBase is WCharacterStyle;
					break;
				}
				formatBase = formatBase.BaseFormat;
			}
		}
		if (!flag && !flag2)
		{
			FormatBase baseFormat = base.BaseFormat;
			while (baseFormat != null && baseFormat is WCharacterFormat)
			{
				if (baseFormat.HasKey(propKey))
				{
					flag3 = base.OwnerBase is WTextRange && baseFormat.OwnerBase is WParagraphStyle;
					break;
				}
				baseFormat = baseFormat.BaseFormat;
			}
		}
		if (!flag && m_tableStyleCharacterFormat != null && (!flag3 || !flag2))
		{
			if (base.Document != null && base.Document.DefCharFormat != null && base.Document.DefCharFormat.GetComplexBoolValue(propKey))
			{
				if (!m_tableStyleCharacterFormat.GetBoolPropertyValue(propKey))
				{
					return !complexBoolValue;
				}
			}
			else if (m_tableStyleCharacterFormat.GetBoolPropertyValue(propKey))
			{
				return !complexBoolValue;
			}
		}
		return complexBoolValue;
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		if (IsBooleanProperty(propKey))
		{
			value = GetComplexBoolValue(value);
		}
		base[propKey] = value;
		OnStateChange(this);
	}

	private bool IsBooleanProperty(int key)
	{
		switch (key)
		{
		case 4:
		case 5:
		case 6:
		case 14:
		case 24:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 58:
		case 59:
		case 60:
		case 71:
		case 76:
		case 81:
		case 92:
		case 99:
		case 103:
		case 104:
		case 106:
		case 109:
			return true;
		default:
			return false;
		}
	}

	private bool SerializeAllData()
	{
		if (base.Document == null)
		{
			return false;
		}
		if (m_sprms != null)
		{
			return false;
		}
		return true;
	}

	internal bool GetComplexBoolValue(int optionKey)
	{
		byte b = 0;
		object obj = null;
		if (HasKey(optionKey))
		{
			obj = base.PropertiesHash[GetFullKey(optionKey)];
		}
		else
		{
			b = byte.MaxValue;
		}
		if (obj is ToggleOperand)
		{
			b = (byte)obj;
		}
		switch (b)
		{
		case 1:
			return true;
		case 0:
			return false;
		default:
		{
			bool flag = false;
			if (base.BaseFormat is WCharacterFormat)
			{
				flag = (base.BaseFormat as WCharacterFormat).GetComplexBoolValue(optionKey);
			}
			if (base.BaseFormat == null && base.Document != null && this != base.Document.DefCharFormat && base.Document.DefCharFormat != null)
			{
				flag = base.Document.DefCharFormat.GetComplexBoolValue(optionKey);
			}
			if (base.Document != null && base.Document.Styles != null)
			{
				Style style = null;
				style = ((CharStyleId != null) ? (base.Document.Styles as StyleCollection).FindStyleById(CharStyleId) : (base.Document.Styles.FindByName(CharStyleName, StyleType.CharacterStyle) as Style));
				if (style != null)
				{
					try
					{
						if (IsDocReading && style.CharacterFormat.PropertiesHash.ContainsKey(optionKey) && (ToggleOperand)style.CharacterFormat.PropertiesHash[optionKey] == ToggleOperand.True)
						{
							flag = true;
						}
						else
						{
							bool complexBoolValue = style.CharacterFormat.GetComplexBoolValue(optionKey);
							flag = flag != complexBoolValue;
						}
					}
					catch
					{
						bool complexBoolValue2 = style.CharacterFormat.GetComplexBoolValue(optionKey);
						flag = flag != complexBoolValue2;
					}
				}
			}
			if (b == 129)
			{
				return !flag;
			}
			return flag;
		}
		}
	}

	private WCharacterFormat GetBaseFormat(WCharacterFormat format)
	{
		if (format == null)
		{
			return null;
		}
		if (format.CharStyleName != null && base.Document.Styles.FindByName(format.CharStyleName, StyleType.CharacterStyle) is WCharacterStyle wCharacterStyle)
		{
			return wCharacterStyle.CharacterFormat;
		}
		return format.BaseFormat as WCharacterFormat;
	}

	internal override void RemoveChanges()
	{
		CheckCrossRef();
		base.RemoveChanges();
	}

	internal override void AcceptChanges()
	{
		base[104] = false;
		base[103] = false;
		base[105] = false;
		if (base.OldPropertiesHash.Count > 0)
		{
			base.OldPropertiesHash.Clear();
		}
	}

	internal void CheckCrossRef()
	{
		if (!base.Document.IsOpening && m_sprms != null && !CrossRefChecked)
		{
			CrossRefChecked = true;
		}
	}

	private WCharacterStyle GetCharStyleValue()
	{
		WCharacterStyle result = null;
		string text = (base.PropertiesHash.ContainsKey(91) ? (base.PropertiesHash[91] as string) : null);
		if (!string.IsNullOrEmpty(text) && base.Document != null)
		{
			result = ((CharStyleId != null) ? ((base.Document.Styles as StyleCollection).FindStyleById(CharStyleId) as WCharacterStyle) : (base.Document.Styles.FindByName(text, StyleType.CharacterStyle) as WCharacterStyle));
		}
		return result;
	}

	internal string GetFontName(short fontKey)
	{
		return (string)base[fontKey];
	}

	internal string GetFontNameToRender(FontScriptType scriptType)
	{
		if (Bidi || ComplexScript)
		{
			return GetFontNameBidiToRender(scriptType);
		}
		if (TextSplitter.IsComplexScript(scriptType))
		{
			return GetFontNameCSToRender(scriptType);
		}
		if (TextSplitter.IsEastAsiaScript(scriptType))
		{
			return GetFontNameEAToRender(scriptType);
		}
		if (SymExFontName == null)
		{
			return FontName;
		}
		return SymExFontName;
	}

	internal string GetFontNameBidiToRender(FontScriptType scriptType)
	{
		string fontNameBidi = FontNameBidi;
		if (string.IsNullOrEmpty(fontNameBidi) || IsThemeFont(fontNameBidi))
		{
			return GetFontNameFromTheme(fontNameBidi, scriptType, FontHintType.CS);
		}
		return fontNameBidi;
	}

	private string GetFontNameEAToRender(FontScriptType scriptType)
	{
		string fontNameFarEast = FontNameFarEast;
		if (string.IsNullOrEmpty(fontNameFarEast) || IsThemeFont(fontNameFarEast))
		{
			return GetFontNameFromTheme(fontNameFarEast, scriptType, FontHintType.EastAsia);
		}
		return fontNameFarEast;
	}

	private string GetFontNameCSToRender(FontScriptType scriptType)
	{
		string fontNameAscii = FontNameAscii;
		if (string.IsNullOrEmpty(fontNameAscii) || IsThemeFont(fontNameAscii))
		{
			return GetFontNameFromTheme(fontNameAscii, scriptType, FontHintType.CS);
		}
		return fontNameAscii;
	}

	private string GetFontNameFromTheme(string fontName, FontScriptType scriptType, FontHintType hintType)
	{
		FontScheme fontScheme = null;
		if (base.Document != null && base.Document.DocHasThemes && base.Document.Themes != null && base.Document.Themes.FontScheme != null)
		{
			fontScheme = base.Document.Themes.FontScheme;
		}
		switch (fontName)
		{
		case "majorAscii":
		case "majorBidi":
		case "majorEastAsia":
		case "majorHAnsi":
		{
			MajorMinorFontScheme majorMinorFontScheme2 = null;
			if (fontScheme != null && fontScheme.MajorFontScheme != null)
			{
				majorMinorFontScheme2 = fontScheme.MajorFontScheme;
			}
			UpdateFontNameFromTheme(majorMinorFontScheme2, scriptType, ref fontName, hintType);
			break;
		}
		case "minorAscii":
		case "minorBidi":
		case "minorEastAsia":
		case "minorHAnsi":
		{
			MajorMinorFontScheme majorMinorFontScheme = null;
			if (fontScheme != null && fontScheme.MajorFontScheme != null)
			{
				majorMinorFontScheme = fontScheme.MinorFontScheme;
			}
			UpdateFontNameFromTheme(majorMinorFontScheme, scriptType, ref fontName, hintType);
			break;
		}
		}
		if (string.IsNullOrEmpty(fontName) || IsThemeFont(fontName))
		{
			fontName = "Times New Roman";
		}
		return fontName;
	}

	private void UpdateFontNameFromTheme(MajorMinorFontScheme majorMinorFontScheme, FontScriptType scriptType, ref string fontName, FontHintType hintType)
	{
		string text = "";
		if (majorMinorFontScheme != null && majorMinorFontScheme.FontSchemeList != null && majorMinorFontScheme.FontSchemeList.Count > 0)
		{
			foreach (FontSchemeStruct fontScheme in majorMinorFontScheme.FontSchemeList)
			{
				if (fontScheme.Name == "cs" && (fontName == "majorBidi" || fontName == "minorBidi"))
				{
					text = fontScheme.Typeface;
				}
				else if (fontScheme.Name == "ea" && (fontName == "majorEastAsia" || fontName == "minorEastAsia"))
				{
					text = fontScheme.Typeface;
				}
				else if (fontScheme.Name == "latin" && (fontName == "majorAscii" || fontName == "majorHAnsi" || fontName == "minorAscii" || fontName == "minorHAnsi"))
				{
					text = fontScheme.Typeface;
				}
			}
		}
		if (majorMinorFontScheme != null && majorMinorFontScheme.FontTypeface != null)
		{
			switch (hintType)
			{
			case FontHintType.CS:
				if (ComplexScript && base.Document != null && base.Document.Settings.ThemeFontLanguages != null && base.Document.Settings.ThemeFontLanguages.HasValue(75))
				{
					fontName = GetFontNameWithFontScript(majorMinorFontScheme, base.Document.Settings.ThemeFontLanguages.LocaleIdBidi, hintType);
					if (fontName != null)
					{
						text = fontName;
					}
				}
				else if (Bidi && (fontName == "majorAscii" || fontName == "majorEastAsia" || fontName == "minorEastAsia" || fontName == "minorAscii") && base.Document != null && base.Document.Settings.ThemeFontLanguages != null && base.Document.Settings.ThemeFontLanguages.HasValue(75))
				{
					fontName = GetFontNameWithFontScript(majorMinorFontScheme, base.Document.Settings.ThemeFontLanguages.LocaleIdBidi, hintType);
					if (fontName != null)
					{
						text = fontName;
					}
				}
				else if (!ComplexScript && (fontName == "majorBidi" || fontName == "minorBidi") && base.Document != null && base.Document.Settings.ThemeFontLanguages != null && base.Document.Settings.ThemeFontLanguages.HasValue(75))
				{
					fontName = GetFontNameWithFontScript(majorMinorFontScheme, base.Document.Settings.ThemeFontLanguages.LocaleIdBidi, hintType);
					if (fontName != null)
					{
						text = fontName;
					}
				}
				else if ((!ComplexScript && !Bidi && (fontName == "majorAscii" || fontName == "majorHAnsi" || fontName == "minorAscii" || fontName == "minorHAnsi")) || (Bidi && (fontName == "majorHAnsi" || fontName == "minorHAnsi")))
				{
					fontName = text;
				}
				else if (majorMinorFontScheme.FontTypeface.ContainsKey("Arab"))
				{
					text = majorMinorFontScheme.FontTypeface["Arab"];
				}
				break;
			case FontHintType.EastAsia:
				if (base.Document == null || base.Document.Settings.ThemeFontLanguages == null || !base.Document.Settings.ThemeFontLanguages.HasValue(74))
				{
					break;
				}
				fontName = GetFontNameWithFontScript(majorMinorFontScheme, base.Document.Settings.ThemeFontLanguages.LocaleIdFarEast, hintType);
				if (fontName != null)
				{
					text = fontName;
				}
				else if (string.IsNullOrEmpty(text))
				{
					switch ((LocaleIDs)base.Document.Settings.ThemeFontLanguages.LocaleIdFarEast)
					{
					case LocaleIDs.zh_CN:
						text = "SimSun";
						break;
					case LocaleIDs.ko_KR:
						text = "Batang";
						break;
					case LocaleIDs.ja_JP:
						text = "MS Mincho";
						break;
					}
				}
				break;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "Times New Roman";
		}
		fontName = text;
	}

	private string GetFontNameWithFontScript(MajorMinorFontScheme majorMinorFontScheme, short localeID, FontHintType hintType)
	{
		string result = null;
		if ((localeID == 1095 || localeID == 71) && majorMinorFontScheme.FontTypeface.ContainsKey("Gujr"))
		{
			result = majorMinorFontScheme.FontTypeface["Gujr"];
		}
		else if ((localeID == 1081 || localeID == 1102 || localeID == 57 || localeID == 78) && majorMinorFontScheme.FontTypeface.ContainsKey("Deva"))
		{
			result = majorMinorFontScheme.FontTypeface["Deva"];
		}
		else if ((localeID == 1042 || localeID == 18) && majorMinorFontScheme.FontTypeface.ContainsKey("Hang"))
		{
			result = majorMinorFontScheme.FontTypeface["Hang"];
		}
		else if ((localeID == 2052 || localeID == 4100 || localeID == 4) && majorMinorFontScheme.FontTypeface.ContainsKey("Hans"))
		{
			result = majorMinorFontScheme.FontTypeface["Hans"];
		}
		else if ((localeID == 1028 || localeID == 3076 || localeID == 5124) && majorMinorFontScheme.FontTypeface.ContainsKey("Hant"))
		{
			result = majorMinorFontScheme.FontTypeface["Hant"];
		}
		else if ((localeID == 1041 || localeID == 17) && majorMinorFontScheme.FontTypeface.ContainsKey("Jpan"))
		{
			result = majorMinorFontScheme.FontTypeface["Jpan"];
		}
		else if ((localeID == 1097 || localeID == 73) && majorMinorFontScheme.FontTypeface.ContainsKey("Taml"))
		{
			result = majorMinorFontScheme.FontTypeface["Taml"];
		}
		else if ((localeID == 1098 || localeID == 74) && majorMinorFontScheme.FontTypeface.ContainsKey("Telu"))
		{
			result = majorMinorFontScheme.FontTypeface["Telu"];
		}
		else if ((localeID == 1037 || localeID == 1085 || localeID == 13) && majorMinorFontScheme.FontTypeface.ContainsKey("Hebr"))
		{
			result = majorMinorFontScheme.FontTypeface["Hebr"];
		}
		else if ((localeID == 1054 || localeID == 30) && majorMinorFontScheme.FontTypeface.ContainsKey("Thai"))
		{
			result = majorMinorFontScheme.FontTypeface["Thai"];
		}
		else if (hintType == FontHintType.CS && majorMinorFontScheme.FontTypeface.ContainsKey("Arab"))
		{
			result = majorMinorFontScheme.FontTypeface["Arab"];
		}
		return result;
	}

	internal bool ContainsValue(int key)
	{
		if (!base.PropertiesHash.ContainsKey(key) && (CharStyle == null || !CharStyle.CharacterFormat.HasValue(key)))
		{
			if (base.BaseFormat != null)
			{
				return (base.BaseFormat as WCharacterFormat).ContainsValue(key);
			}
			return false;
		}
		return true;
	}

	internal void SetDefaultProperties()
	{
		base.PropertiesHash.Add(3, 10f);
		base.PropertiesHash.Add(1, Color.Empty);
		base.PropertiesHash.Add(2, "Times New Roman");
		base.PropertiesHash.Add(4, false);
		base.PropertiesHash.Add(5, false);
		base.PropertiesHash.Add(7, UnderlineStyle.None);
		base.PropertiesHash.Add(63, Color.Empty);
		base.PropertiesHash.Add(50, false);
		base.PropertiesHash.Add(14, false);
		base.PropertiesHash.Add(51, false);
		base.PropertiesHash.Add(52, false);
		base.PropertiesHash.Add(10, SubSuperScript.None);
		base.PropertiesHash.Add(9, Color.Empty);
		base.PropertiesHash.Add(54, false);
		base.PropertiesHash.Add(59, false);
		base.PropertiesHash.Add(53, false);
		base.PropertiesHash.Add(24, false);
		base.PropertiesHash.Add(55, false);
		base.PropertiesHash.Add(18, 0f);
	}

	protected override void InitXDLSHolder()
	{
		if (m_sprms == null)
		{
			base.XDLSHolder.AddElement("text-border", Border);
		}
	}

	protected override object GetDefValue(int key)
	{
		if (base.Document == null)
		{
			return null;
		}
		if (base.Document != null && base.Document.DefCharFormat != null && base.Document.DefCharFormat != this)
		{
			return base.Document.DefCharFormat[key];
		}
		switch (key)
		{
		case 0:
			return base.Document.FontSettings.GetFont("Times New Roman", 10f, FontStyle.Regular, FontScriptType.English);
		case 1:
		case 9:
		case 63:
		case 77:
		case 90:
			return Color.Empty;
		case 3:
		case 62:
			return 10f;
		case 125:
			return 0f;
		case 127:
			return 100f;
		case 7:
			return UnderlineStyle.None;
		case 79:
			return EmphasisType.NoEmphasis;
		case 80:
			return TextEffect.None;
		case 81:
			return true;
		case 10:
			return SubSuperScript.None;
		case 126:
			return BreakClearType.None;
		case 17:
		case 18:
			return 0f;
		case 4:
		case 5:
		case 6:
		case 14:
		case 20:
		case 24:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 58:
		case 59:
		case 60:
		case 71:
		case 76:
		case 92:
		case 99:
		case 103:
		case 104:
		case 105:
		case 106:
		case 108:
		case 109:
		case 120:
			return false;
		case 2:
		case 68:
			if (!string.IsNullOrEmpty(m_doc.StandardAsciiFont))
			{
				return m_doc.StandardAsciiFont;
			}
			return "Times New Roman";
		case 69:
			if (!string.IsNullOrEmpty(m_doc.StandardFarEastFont))
			{
				return m_doc.StandardFarEastFont;
			}
			return "Times New Roman";
		case 61:
			if (!string.IsNullOrEmpty(m_doc.StandardBidiFont))
			{
				return m_doc.StandardBidiFont;
			}
			return "Times New Roman";
		case 70:
			if (!string.IsNullOrEmpty(m_doc.StandardNonFarEastFont))
			{
				return m_doc.StandardNonFarEastFont;
			}
			return "Times New Roman";
		case 73:
		case 74:
			return (short)1033;
		case 75:
			return (short)1025;
		case 78:
			return TextureStyle.TextureNone;
		case 107:
			return int.MaxValue;
		case 121:
			return LigatureType.None;
		case 122:
			return NumberFormType.Default;
		case 123:
			return NumberSpacingType.Default;
		case 124:
			return StylisticSetType.StylisticSetDefault;
		case 72:
			return FontHintType.Default;
		case 91:
			return null;
		case 8:
		case 12:
			return string.Empty;
		case 11:
		case 15:
			return DateTime.MinValue;
		case 13:
			return null;
		case 128:
			return string.Empty;
		case 16:
		case 19:
			return 0;
		default:
			throw new ArgumentException("key has invalid value");
		}
	}

	protected override FormatBase GetDefComposite(int key)
	{
		if (key == 67)
		{
			return GetDefComposite(67, new Border(this, 67));
		}
		return null;
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("FontName"))
		{
			base[2] = reader.ReadString("FontName");
		}
		if (reader.HasAttribute("FontNameBidi"))
		{
			base[61] = reader.ReadString("FontNameBidi");
		}
		if (reader.HasAttribute("FontNameAscii"))
		{
			base[68] = reader.ReadString("FontNameAscii");
		}
		if (reader.HasAttribute("FontNameFarEast"))
		{
			base[69] = reader.ReadString("FontNameFarEast");
		}
		if (reader.HasAttribute("FontNameNonFarEast"))
		{
			base[70] = reader.ReadString("FontNameNonFarEast");
		}
		if (reader.HasAttribute("CharStyleName"))
		{
			CharStyleName = reader.ReadString("CharStyleName");
		}
		if (reader.HasAttribute("Underline"))
		{
			UnderlineStyle = (UnderlineStyle)(object)reader.ReadEnum("Underline", typeof(UnderlineStyle));
		}
		if (reader.HasAttribute("TextColor"))
		{
			TextColor = reader.ReadColor("TextColor");
		}
		if (reader.HasAttribute("FontSize"))
		{
			SetPropertyValue(3, reader.ReadFloat("FontSize"));
		}
		if (reader.HasAttribute("Bold"))
		{
			Bold = reader.ReadBoolean("Bold");
		}
		if (reader.HasAttribute("Italic"))
		{
			Italic = reader.ReadBoolean("Italic");
		}
		if (reader.HasAttribute("Strike"))
		{
			Strikeout = reader.ReadBoolean("Strike");
		}
		if (reader.HasAttribute("DoubleStrike"))
		{
			DoubleStrike = reader.ReadBoolean("DoubleStrike");
		}
		if (reader.HasAttribute("LineSpacing"))
		{
			SetPropertyValue(18, reader.ReadFloat("LineSpacing"));
		}
		if (reader.HasAttribute("Position"))
		{
			SetPropertyValue(17, reader.ReadFloat("Position"));
		}
		if (reader.HasAttribute("SubSuperScript"))
		{
			SubSuperScript = (SubSuperScript)(object)reader.ReadEnum("SubSuperScript", typeof(SubSuperScript));
		}
		if (reader.HasAttribute("TextBackgroundColor"))
		{
			TextBackgroundColor = reader.ReadColor("TextBackgroundColor");
		}
		if (reader.HasAttribute("LineBreak"))
		{
			LineBreak = reader.ReadBoolean("LineBreak");
		}
		if (reader.HasAttribute("Shadow"))
		{
			Shadow = reader.ReadBoolean("Shadow");
		}
		if (reader.HasAttribute("Emboss"))
		{
			Emboss = reader.ReadBoolean("Emboss");
		}
		if (reader.HasAttribute("Engrave"))
		{
			Engrave = reader.ReadBoolean("Engrave");
		}
		if (reader.HasAttribute("Hidden"))
		{
			Hidden = reader.ReadBoolean("Hidden");
		}
		if (reader.HasAttribute("AllCaps"))
		{
			AllCaps = reader.ReadBoolean("AllCaps");
		}
		if (reader.HasAttribute("SmallCaps"))
		{
			SmallCaps = reader.ReadBoolean("SmallCaps");
		}
		if (reader.HasAttribute("Bidi"))
		{
			Bidi = reader.ReadBoolean("Bidi");
		}
		if (reader.HasAttribute("BoldBidi"))
		{
			BoldBidi = reader.ReadBoolean("BoldBidi");
		}
		if (reader.HasAttribute("ItalicBidi"))
		{
			ItalicBidi = reader.ReadBoolean("ItalicBidi");
		}
		if (reader.HasAttribute("FontSizeBidi"))
		{
			SetPropertyValue(62, reader.ReadFloat("FontSizeBidi"));
		}
		if (reader.HasAttribute("HighlightColor"))
		{
			HighlightColor = reader.ReadColor("HighlightColor");
		}
		if (reader.HasAttribute("hint"))
		{
			string text = reader.ReadString("hint");
			if (!(text == "cs"))
			{
				if (text == "eastAsia")
				{
					IdctHint = FontHintType.EastAsia;
				}
				else
				{
					IdctHint = FontHintType.Default;
				}
			}
			else
			{
				IdctHint = FontHintType.CS;
			}
		}
		if (reader.HasAttribute("RgLid0"))
		{
			LocaleIdASCII = reader.ReadShort("RgLid0");
		}
		if (reader.HasAttribute("RgLid1"))
		{
			LocaleIdFarEast = reader.ReadShort("RgLid1");
		}
		if (reader.HasAttribute("LidBi"))
		{
			LocaleIdBidi = reader.ReadShort("LidBi");
		}
		if (reader.HasAttribute("NoProof"))
		{
			NoProof = reader.ReadBoolean("NoProof");
		}
		if (reader.HasAttribute("ForeColor"))
		{
			ForeColor = reader.ReadColor("ForeColor");
		}
		if (reader.HasAttribute("Texture"))
		{
			TextureStyle = (TextureStyle)(object)reader.ReadEnum("Texture", typeof(TextureStyle));
		}
		if (reader.HasAttribute("Outline"))
		{
			OutLine = reader.ReadBoolean("Outline");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (HasKey(2))
		{
			writer.WriteValue("FontName", FontName);
		}
		if (HasKey(61))
		{
			writer.WriteValue("FontNameBidi", FontNameBidi);
		}
		if (HasKey(69))
		{
			writer.WriteValue("FontNameFarEast", FontNameFarEast);
		}
		if (HasKey(70))
		{
			writer.WriteValue("FontNameNonFarEast", FontNameNonFarEast);
		}
		if (HasKey(68))
		{
			writer.WriteValue("FontNameAscii", FontNameAscii);
		}
		if (CharStyleName != null)
		{
			writer.WriteValue("CharStyleName", CharStyleName);
		}
		if (SerializeAllData())
		{
			if (LineBreak)
			{
				writer.WriteValue("LineBreak", LineBreak);
			}
			if (!TextColor.IsEmpty)
			{
				writer.WriteValue("TextColor", TextColor);
			}
			if (HasValue(3))
			{
				writer.WriteValue("FontSize", FontSize);
			}
			if (HasValue(4))
			{
				writer.WriteValue("Bold", Bold);
			}
			if (HasValue(5))
			{
				writer.WriteValue("Italic", Italic);
			}
			if (HasValue(6))
			{
				writer.WriteValue("Strike", Strikeout);
			}
			if (HasValue(14))
			{
				writer.WriteValue("DoubleStrike", DoubleStrike);
			}
			if (HasValue(7))
			{
				writer.WriteValue("Underline", (int)UnderlineStyle);
			}
			if (HasValue(10))
			{
				writer.WriteValue("SubSuperScript", SubSuperScript);
			}
			if (HasValue(18))
			{
				writer.WriteValue("LineSpacing", CharacterSpacing);
			}
			if (HasValue(17))
			{
				writer.WriteValue("Position", Position);
			}
			if (HasValue(9))
			{
				writer.WriteValue("TextBackgroundColor", TextBackgroundColor);
			}
			if (HasValue(50))
			{
				writer.WriteValue("Shadow", Shadow);
			}
			if (HasValue(51))
			{
				writer.WriteValue("Emboss", Emboss);
			}
			if (HasValue(52))
			{
				writer.WriteValue("Engrave", Engrave);
			}
			if (HasValue(53))
			{
				writer.WriteValue("Hidden", Hidden);
			}
			if (HasValue(24))
			{
				writer.WriteValue("SpecVanish", SpecVanish);
			}
			if (HasValue(54))
			{
				writer.WriteValue("AllCaps", AllCaps);
			}
			if (HasValue(55))
			{
				writer.WriteValue("SmallCaps", SmallCaps);
			}
			if (HasValue(58))
			{
				writer.WriteValue("Bidi", Bidi);
			}
			if (HasValue(59))
			{
				writer.WriteValue("BoldBidi", BoldBidi);
			}
			if (HasValue(60))
			{
				writer.WriteValue("ItalicBidi", ItalicBidi);
			}
			if (HasValue(62))
			{
				writer.WriteValue("FontSizeBidi", FontSizeBidi);
			}
			if (HasValue(63))
			{
				writer.WriteValue("HighlightColor", HighlightColor);
			}
			if (HasValue(72))
			{
				writer.WriteValue("hint", IdctHint);
			}
			if (HasValue(73))
			{
				writer.WriteValue("RgLid0", LocaleIdASCII);
			}
			if (HasValue(74))
			{
				writer.WriteValue("RgLid1", LocaleIdFarEast);
			}
			if (HasValue(75))
			{
				writer.WriteValue("LidBi", LocaleIdBidi);
			}
			if (HasValue(76))
			{
				writer.WriteValue("NoProof", NoProof);
			}
			if (HasValue(77))
			{
				writer.WriteValue("ForeColor", ForeColor);
			}
			if (HasValue(78))
			{
				writer.WriteValue("Texture", TextureStyle);
			}
			if (HasValue(71))
			{
				writer.WriteValue("Outline", OutLine);
			}
		}
	}

	protected override void WriteXmlContent(IXDLSContentWriter writer)
	{
		base.WriteXmlContent(writer);
		if (m_sprms != null)
		{
			byte[] array = new byte[m_sprms.Length];
			m_sprms.Save(array, 0);
			writer.WriteChildBinaryElement("internal-data", array);
		}
	}

	protected override bool ReadXmlContent(IXDLSContentReader reader)
	{
		bool result = base.ReadXmlContent(reader);
		if (reader.TagName == "internal-data")
		{
			SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray(reader.ReadChildBinaryElement());
			result = true;
			CharacterPropertiesConverter.SprmsToFormat(singlePropertyModifierArray, this, null, null, isNewPropertyHash: false);
			singlePropertyModifierArray.Clear();
		}
		return result;
	}

	protected internal new void ImportContainer(FormatBase format)
	{
		base.ImportContainer(format);
		if (format is WCharacterFormat format2)
		{
			ImportXmlProps(format2);
		}
	}

	private void ImportXmlProps(WCharacterFormat format)
	{
		if (format.m_xmlProps == null || format.m_xmlProps.Count <= 0)
		{
			return;
		}
		foreach (Stream xmlProp in format.XmlProps)
		{
			XmlProps.Add(CloneStream(xmlProp));
		}
	}

	protected override void ImportMembers(FormatBase format)
	{
		base.ImportMembers(format);
		if (!(format is WCharacterFormat wCharacterFormat))
		{
			return;
		}
		if (wCharacterFormat.PropertiesHash.Count > 0)
		{
			foreach (KeyValuePair<int, object> item in wCharacterFormat.PropertiesHash)
			{
				if (!item.Key.Equals(91))
				{
					base.PropertiesHash[item.Key] = item.Value;
					base.IsDefault = false;
				}
			}
		}
		string charStyleName = wCharacterFormat.CharStyleName;
		if (charStyleName == null)
		{
			return;
		}
		WordDocument document = base.Document;
		if (((wCharacterFormat.CharStyleId != null) ? ((document.Styles as StyleCollection).FindStyleById(wCharacterFormat.CharStyleId) as WCharacterStyle) : (document.Styles.FindByName(charStyleName) as WCharacterStyle)) == null)
		{
			IStyle style;
			if (wCharacterFormat.CharStyleId == null)
			{
				style = wCharacterFormat.Document.Styles.FindByName(charStyleName);
			}
			else
			{
				IStyle style2 = (wCharacterFormat.Document.Styles as StyleCollection).FindStyleById(wCharacterFormat.CharStyleId);
				style = style2;
			}
			IStyle style3 = style;
			if (style3 != null)
			{
				document.Styles.Add(style3.Clone());
			}
		}
		CharStyleName = charStyleName;
		CharStyleId = wCharacterFormat.CharStyleId;
	}

	public override void ClearFormatting()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
		}
		if (m_xmlProps != null)
		{
			m_xmlProps.Clear();
		}
	}

	protected override void OnChange(FormatBase format, int propKey)
	{
	}

	internal override void ApplyBase(FormatBase baseFormat)
	{
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
		if (base.Document.IsCloning && base.Document != baseFormat.Document && !baseFormat.Document.ImportStyles)
		{
			List<int> list = new List<int>(new int[18]
			{
				109, 99, 4, 5, 6, 14, 50, 51, 52, 53,
				54, 55, 58, 59, 60, 72, 71, 106
			});
			foreach (int item in list)
			{
				if (HasValue(item))
				{
					dictionary.Add(item, GetComplexBoolValue(item));
				}
			}
			list.Clear();
		}
		base.ApplyBase(baseFormat);
		foreach (KeyValuePair<int, bool> item2 in dictionary)
		{
			if (item2.Value != GetComplexBoolValue(item2.Key) && HasValue(item2.Key))
			{
				if (GetBoolComplexValue(item2.Key, item2.Value) == 129)
				{
					SetPropertyValue(item2.Key, ToggleOperand.PositiveComplexValue);
				}
				else
				{
					SetPropertyValue(item2.Key, ToggleOperand.NegativeComplexValue);
				}
			}
		}
		dictionary.Clear();
		string text = (HasValue(68) ? GetFontName(68) : null);
		if (text != null && !m_doc.UsedFontNames.Contains(text))
		{
			m_doc.UsedFontNames.Add(text);
		}
	}

	public void Dispose()
	{
		Close();
	}

	internal override void Close()
	{
		base.Close();
		if (m_tableStyleCharacterFormat != null)
		{
			m_tableStyleCharacterFormat.Close();
			m_tableStyleCharacterFormat = null;
		}
		if (m_xmlProps != null)
		{
			m_xmlProps.Clear();
			m_xmlProps = null;
		}
	}

	private void UpdateUsedFontsCollection()
	{
		string text = (HasValue(68) ? GetFontName(68) : null);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		FontStyle fontStyle = FontStyle.Regular;
		if (HasValue(4) && Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (HasValue(5) && Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (HasValue(7) && UnderlineStyle != 0)
		{
			fontStyle |= FontStyle.Underline;
		}
		if (HasValue(6) && Strikeout)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		DocGen.Drawing.Font font = null;
		try
		{
			font = base.Document.FontSettings.GetFont(text, 11f, fontStyle, FontScriptType.English);
		}
		catch (Exception)
		{
			IFontFamily fontFamily = WordDocument.RenderHelper.GetFontFamily(text);
			if (fontFamily.IsStyleAvailable(FontStyle.Bold))
			{
				fontStyle |= FontStyle.Bold;
			}
			if (fontFamily.IsStyleAvailable(FontStyle.Italic))
			{
				fontStyle |= FontStyle.Italic;
			}
			if (fontFamily.IsStyleAvailable(FontStyle.Underline))
			{
				fontStyle |= FontStyle.Underline;
			}
			if (fontFamily.IsStyleAvailable(FontStyle.Strikeout))
			{
				fontStyle |= FontStyle.Strikeout;
			}
			font = base.Document.FontSettings.GetFont(text, 11f, fontStyle, FontScriptType.English);
		}
		if (!m_doc.UsedFonts.Contains(font))
		{
			m_doc.UsedFonts.Add(font);
		}
	}

	internal byte GetBoolComplexValue(int propertyKey, bool value)
	{
		byte b = 0;
		if (!(base.OwnerBase is WListLevel))
		{
			bool flag = false;
			if (base.BaseFormat is WCharacterFormat)
			{
				flag = (base.BaseFormat as WCharacterFormat).GetComplexBoolValue(propertyKey);
			}
			if (base.Document.Styles.FindByName(CharStyleName, StyleType.CharacterStyle) is Style style)
			{
				bool complexBoolValue = style.CharacterFormat.GetComplexBoolValue(propertyKey);
				flag = flag != complexBoolValue;
			}
			return (byte)((value == flag) ? 128 : 129);
		}
		return value ? ((byte)1) : ((byte)0);
	}

	internal object GetComplexBoolValue(object value)
	{
		if (value is bool)
		{
			return ((bool)value) ? ToggleOperand.True : ToggleOperand.False;
		}
		if (value is byte)
		{
			return (ToggleOperand)(byte)value;
		}
		return value;
	}

	internal override bool HasValue(int propertyKey)
	{
		if (HasKey(propertyKey))
		{
			return true;
		}
		return false;
	}

	internal override int GetSprmOption(int propertyKey)
	{
		switch (propertyKey)
		{
		case 2:
		case 68:
			return 19023;
		case 69:
			return 19024;
		case 70:
			return 19025;
		case 61:
			return 19038;
		case 4:
			return 2101;
		case 5:
			return 2102;
		case 6:
			return 2103;
		case 7:
			return 10814;
		case 9:
		case 77:
		case 78:
			return 18534;
		case 10:
			return 10824;
		case 126:
			return 10361;
		case 14:
			return 10835;
		case 17:
			return 18501;
		case 18:
			return 34880;
		case 127:
			return 18514;
		case 50:
			return 2105;
		case 51:
			return 2136;
		case 52:
			return 2132;
		case 53:
			return 2108;
		case 24:
			return 2072;
		case 54:
			return 2107;
		case 55:
			return 2106;
		case 58:
			return 2138;
		case 59:
			return 2140;
		case 60:
			return 2141;
		case 62:
			return 19041;
		case 63:
			return 10764;
		case 71:
			return 2104;
		case 72:
			return 10351;
		case 73:
			return 18547;
		case 74:
			return 18548;
		case 75:
			return 18527;
		case 3:
			return 19011;
		case 1:
			return 10818;
		case 107:
			return 26759;
		case 108:
			return 18568;
		case 99:
			return 2178;
		case 109:
			return 2050;
		case 105:
			return 51799;
		case 103:
			return 2049;
		case 104:
			return 2048;
		case 67:
			return 26725;
		case 76:
			return 2165;
		case 106:
			return 2133;
		case 125:
			return 18507;
		case 13:
			return 51832;
		case 81:
			return 2152;
		case 79:
			return 10804;
		case 80:
			return 10329;
		case 90:
			return 26743;
		case 91:
			return 18992;
		case 92:
			return 2065;
		default:
			return int.MaxValue;
		}
	}

	private void WriteComplexAttr(IXDLSAttributeWriter writer, int propKey, string xdlsConstant)
	{
		int sprmOption = GetSprmOption(propKey);
		SinglePropertyModifierRecord singlePropertyModifierRecord = m_sprms[sprmOption];
		writer.WriteValue(xdlsConstant, singlePropertyModifierRecord.ByteValue);
	}

	internal string GetFontHint()
	{
		string empty = string.Empty;
		return IdctHint switch
		{
			FontHintType.CS => "cs", 
			FontHintType.EastAsia => "eastAsia", 
			_ => "default", 
		};
	}

	internal string GetFontNameFromHint(FontScriptType scriptType)
	{
		string result = GetFontNameToRender(scriptType);
		if (Bidi || ComplexScript || TextSplitter.IsEastAsiaScript(scriptType))
		{
			return result;
		}
		switch (IdctHint)
		{
		case FontHintType.CS:
			if (!IsThemeFont(FontNameBidi))
			{
				result = FontNameBidi;
			}
			break;
		case FontHintType.EastAsia:
			return result;
		case FontHintType.Default:
			if (!IsThemeFont(FontNameNonFarEast))
			{
				result = FontNameNonFarEast;
			}
			break;
		}
		return result;
	}

	internal void MergeFormat(WCharacterFormat destinationFormat)
	{
		Dictionary<int, object> dictionary = new Dictionary<int, object>();
		if (Bidi)
		{
			dictionary.Add(58, true);
		}
		if (Bold)
		{
			dictionary.Add(4, true);
		}
		if (BoldBidi)
		{
			dictionary.Add(59, true);
		}
		if (ComplexScript)
		{
			dictionary.Add(99, true);
		}
		if (Hidden)
		{
			dictionary.Add(53, true);
		}
		if (Italic)
		{
			dictionary.Add(5, true);
		}
		if (ItalicBidi)
		{
			dictionary.Add(60, true);
		}
		if (SubSuperScript != 0)
		{
			dictionary.Add(10, SubSuperScript);
		}
		if (UnderlineStyle != 0)
		{
			dictionary.Add(7, UnderlineStyle);
		}
		dictionary.Add(73, LocaleIdASCII);
		dictionary.Add(74, LocaleIdFarEast);
		dictionary.Add(75, LocaleIdBidi);
		CharStyleName = null;
		ImportContainer(destinationFormat);
		CopyProperties(destinationFormat);
		ApplyBase(destinationFormat.BaseFormat);
		UpdateFormattings(dictionary);
		dictionary.Clear();
	}

	private void UpdateFormattings(Dictionary<int, object> properties)
	{
		foreach (KeyValuePair<int, object> property in properties)
		{
			switch (property.Key)
			{
			case 58:
				if (Bidi != (bool)property.Value)
				{
					Bidi = (bool)property.Value;
				}
				break;
			case 4:
				if (Bold != (bool)property.Value)
				{
					Bold = (bool)property.Value;
				}
				break;
			case 59:
				if (BoldBidi != (bool)property.Value)
				{
					BoldBidi = (bool)property.Value;
				}
				break;
			case 99:
				if (ComplexScript != (bool)property.Value)
				{
					ComplexScript = (bool)property.Value;
				}
				break;
			case 53:
				if (Hidden != (bool)property.Value)
				{
					Hidden = (bool)property.Value;
				}
				break;
			case 24:
				if (SpecVanish != (bool)property.Value)
				{
					SpecVanish = (bool)property.Value;
				}
				break;
			case 5:
				if (Italic != (bool)property.Value)
				{
					Italic = (bool)property.Value;
				}
				break;
			case 60:
				if (ItalicBidi != (bool)property.Value)
				{
					ItalicBidi = (bool)property.Value;
				}
				break;
			case 50:
				if (Shadow != (bool)property.Value)
				{
					Shadow = (bool)property.Value;
				}
				break;
			case 10:
				if (SubSuperScript != (SubSuperScript)property.Value)
				{
					SubSuperScript = (SubSuperScript)property.Value;
				}
				break;
			case 7:
				if (UnderlineStyle != (UnderlineStyle)property.Value)
				{
					UnderlineStyle = (UnderlineStyle)property.Value;
				}
				break;
			case 73:
				if (LocaleIdASCII != (short)property.Value)
				{
					LocaleIdASCII = (short)property.Value;
				}
				break;
			case 74:
				if (LocaleIdFarEast != (short)property.Value)
				{
					LocaleIdFarEast = (short)property.Value;
				}
				break;
			case 75:
				if (LocaleIdBidi != (short)property.Value)
				{
					LocaleIdBidi = (short)property.Value;
				}
				break;
			}
		}
	}

	internal new void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if ((doc.ImportOptions & ImportOptions.UseDestinationStyles) == 0)
		{
			UpdateFormatting(doc);
		}
		CloneRelationsTo(doc);
	}

	public IOfficeRunFormat Clone()
	{
		WCharacterFormat wCharacterFormat = new WCharacterFormat(m_doc);
		wCharacterFormat.ImportContainer(this);
		wCharacterFormat.CopyProperties(this);
		return wCharacterFormat;
	}

	internal void CloneRelationsTo(WordDocument doc)
	{
		if (!string.IsNullOrEmpty(CharStyleName) && (doc.ImportOptions & ImportOptions.UseDestinationStyles) != 0 && base.Document.Styles.FindByName(CharStyleName, StyleType.CharacterStyle) is WCharacterStyle wCharacterStyle && wCharacterStyle.ImportStyleTo(doc, isParagraphStyle: false) is WCharacterStyle wCharacterStyle2)
		{
			CharStyleName = wCharacterStyle2.Name;
		}
	}

	private void UpdateFormatting(WordDocument doc)
	{
		if ((doc.ImportOptions & ImportOptions.MergeFormatting) != 0)
		{
			WParagraph wParagraph = doc.LastParagraph;
			if (wParagraph == null)
			{
				wParagraph = new WParagraph(doc);
			}
			Dictionary<int, object> dictionary = new Dictionary<int, object>();
			if (OwnerBase is WTextRange && (OwnerBase as WTextRange).m_revisions != null && (OwnerBase as WTextRange).m_revisions.Count != 0)
			{
				foreach (KeyValuePair<int, object> item in base.PropertiesHash)
				{
					if ((item.Key == 8 && item.Value is string) || (item.Key == 11 && item.Value is DateTime) || ((item.Key == 103 || item.Key == 104) && item.Value is ToggleOperand))
					{
						dictionary.Add(item.Key, item.Value);
					}
				}
			}
			MergeFormat(wParagraph.BreakCharacterFormat);
			if (dictionary.Count == 0)
			{
				return;
			}
			{
				foreach (KeyValuePair<int, object> item2 in dictionary)
				{
					base.PropertiesHash.Add(item2.Key, item2.Value);
				}
				return;
			}
		}
		WParagraphStyle wParagraphStyle = doc.Styles.FindByName("Normal", StyleType.ParagraphStyle) as WParagraphStyle;
		if ((doc.ImportOptions & ImportOptions.KeepSourceFormatting) != 0)
		{
			UpdateSourceFormat(wParagraphStyle.CharacterFormat);
		}
	}

	internal void UpdateSourceFormat(WCharacterFormat destBaseFormat)
	{
		WCharacterFormat wCharacterFormat = new WCharacterFormat(destBaseFormat.Document);
		wCharacterFormat.ImportContainer(this);
		wCharacterFormat.CopyProperties(this);
		wCharacterFormat.ApplyBase(destBaseFormat);
		wCharacterFormat.CharStyleName = null;
		UpdateSourceFormatting(wCharacterFormat);
		if (IsThemeFont(wCharacterFormat.FontNameAscii))
		{
			wCharacterFormat.FontNameAscii = FontName;
		}
		if (IsThemeFont(wCharacterFormat.FontNameBidi))
		{
			wCharacterFormat.FontNameBidi = FontName;
		}
		if (IsThemeFont(wCharacterFormat.FontNameFarEast))
		{
			wCharacterFormat.FontNameFarEast = FontName;
		}
		if (IsThemeFont(wCharacterFormat.FontNameNonFarEast))
		{
			wCharacterFormat.FontNameNonFarEast = FontName;
		}
		ImportContainer(wCharacterFormat);
		CopyProperties(wCharacterFormat);
		wCharacterFormat.Close();
	}

	internal void UpdateSourceFormatting(WCharacterFormat format)
	{
		if (format.AllCaps != AllCaps)
		{
			format.AllCaps = AllCaps;
		}
		if (format.Bidi != Bidi)
		{
			format.Bidi = Bidi;
		}
		if (format.Bold != Bold)
		{
			format.Bold = Bold;
		}
		if (format.BoldBidi != BoldBidi)
		{
			format.BoldBidi = BoldBidi;
		}
		if (format.CharacterSpacing != CharacterSpacing)
		{
			format.SetPropertyValue(18, CharacterSpacing);
		}
		if (format.ComplexScript != ComplexScript)
		{
			format.ComplexScript = ComplexScript;
		}
		if (format.DoubleStrike != DoubleStrike)
		{
			format.DoubleStrike = DoubleStrike;
		}
		if (format.Emboss != Emboss)
		{
			format.Emboss = Emboss;
		}
		if (format.Engrave != Engrave)
		{
			format.Engrave = Engrave;
		}
		if (format.FieldVanish != FieldVanish)
		{
			format.FieldVanish = FieldVanish;
		}
		if (format.FontName != FontName)
		{
			format.FontName = FontName;
		}
		if (format.FontNameAscii != FontNameAscii)
		{
			format.FontNameAscii = FontNameAscii;
		}
		if (format.FontNameBidi != FontNameBidi)
		{
			format.FontNameBidi = FontNameBidi;
		}
		if (format.FontNameFarEast != FontNameFarEast)
		{
			format.FontNameFarEast = FontNameFarEast;
		}
		if (format.FontNameNonFarEast != FontNameNonFarEast)
		{
			format.FontNameNonFarEast = FontNameNonFarEast;
		}
		if (format.FontSize != FontSize)
		{
			format.SetPropertyValue(3, FontSize);
		}
		if (format.FontSizeBidi != FontSizeBidi)
		{
			format.SetPropertyValue(62, FontSizeBidi);
		}
		if (format.ForeColor != ForeColor)
		{
			format.ForeColor = ForeColor;
		}
		if (format.Hidden != Hidden)
		{
			format.Hidden = Hidden;
		}
		if (format.HighlightColor != HighlightColor)
		{
			format.HighlightColor = HighlightColor;
		}
		if (format.IdctHint != IdctHint)
		{
			format.IdctHint = IdctHint;
		}
		if (format.Italic != Italic)
		{
			format.Italic = Italic;
		}
		if (format.ItalicBidi != ItalicBidi)
		{
			format.ItalicBidi = ItalicBidi;
		}
		if (format.LocaleIdBidi != LocaleIdBidi)
		{
			format.LocaleIdBidi = LocaleIdBidi;
		}
		if (format.Ligatures != Ligatures)
		{
			format.Ligatures = Ligatures;
		}
		if (format.LocaleIdASCII != LocaleIdASCII)
		{
			format.LocaleIdASCII = LocaleIdASCII;
		}
		if (format.LocaleIdFarEast != LocaleIdFarEast)
		{
			format.LocaleIdFarEast = LocaleIdFarEast;
		}
		if (format.NoProof != NoProof)
		{
			format.NoProof = NoProof;
		}
		if (format.NumberForm != NumberForm)
		{
			format.NumberForm = NumberForm;
		}
		if (format.NumberSpacing != NumberSpacing)
		{
			format.NumberSpacing = NumberSpacing;
		}
		if (format.OutLine != OutLine)
		{
			format.OutLine = OutLine;
		}
		if (format.Position != Position)
		{
			format.SetPropertyValue(17, Position);
		}
		if (format.Shadow != Shadow)
		{
			format.Shadow = Shadow;
		}
		if (format.SmallCaps != SmallCaps)
		{
			format.SmallCaps = SmallCaps;
		}
		if (format.Special != Special)
		{
			format.Special = Special;
		}
		if (format.SpecVanish != SpecVanish)
		{
			format.SpecVanish = SpecVanish;
		}
		if (format.Kern != Kern)
		{
			format.Kern = Kern;
		}
		if (format.Scaling != Scaling)
		{
			format.Scaling = Scaling;
		}
		if (format.Strikeout != Strikeout)
		{
			format.Strikeout = Strikeout;
		}
		if (format.StylisticSet != StylisticSet)
		{
			format.StylisticSet = StylisticSet;
		}
		if (format.SubSuperScript != SubSuperScript)
		{
			format.SubSuperScript = SubSuperScript;
		}
		if (format.TextBackgroundColor != TextBackgroundColor)
		{
			format.TextBackgroundColor = TextBackgroundColor;
		}
		if (format.TextColor != TextColor)
		{
			format.TextColor = TextColor;
		}
		if (format.TextureStyle != TextureStyle)
		{
			format.TextureStyle = TextureStyle;
		}
		if (format.UnderlineStyle != UnderlineStyle)
		{
			format.UnderlineStyle = UnderlineStyle;
		}
		if (format.UseContextualAlternates != UseContextualAlternates)
		{
			format.UseContextualAlternates = UseContextualAlternates;
		}
		Border.UpdateSourceFormatting(format.Border);
	}

	internal bool IsThemeFont(string fontName)
	{
		switch (fontName)
		{
		default:
			return fontName == "minorHAnsi";
		case "majorAscii":
		case "majorBidi":
		case "majorEastAsia":
		case "majorHAnsi":
		case "minorAscii":
		case "minorBidi":
		case "minorEastAsia":
			return true;
		}
	}

	internal bool UpdateDocDefaults(int propertyKey)
	{
		if (!base.PropertiesHash.ContainsKey(propertyKey) && !BaseFormatHasFontInfoKey(this, propertyKey))
		{
			if (propertyKey == 3)
			{
				base.PropertiesHash.Add(3, 10f);
			}
			return true;
		}
		return false;
	}

	internal bool BaseFormatHasFontInfoKey(WCharacterFormat characterFormat, int propertyKey)
	{
		while (characterFormat != null)
		{
			if (characterFormat.PropertiesHash.ContainsKey(propertyKey))
			{
				return true;
			}
			characterFormat = ((characterFormat.BaseFormat != null && characterFormat.BaseFormat is WCharacterFormat) ? (characterFormat.BaseFormat as WCharacterFormat) : ((characterFormat == base.Document.DefCharFormat || base.Document.DefCharFormat == null) ? null : base.Document.DefCharFormat));
		}
		return false;
	}

	internal override bool Compare(FormatBase formatBase)
	{
		WCharacterFormat wCharacterFormat = formatBase as WCharacterFormat;
		bool flag = false;
		if (this != base.Document.DefCharFormat && base.OwnerBase is WParagraphStyle && (base.OwnerBase as WParagraphStyle).Name == "Normal" && !base.PropertiesHash.ContainsKey(3) && wCharacterFormat.PropertiesHash.ContainsKey(3) && wCharacterFormat.FontSize == 10f)
		{
			flag = UpdateDocDefaults(3);
		}
		if (!Compare(2, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(3, wCharacterFormat))
		{
			if (flag)
			{
				base.PropertiesHash.Remove(3);
			}
			return false;
		}
		if (!Compare(99, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(4, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(5, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(6, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(54, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(1, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(14, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(69, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(62, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(9, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(51, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(52, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(63, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(68, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(61, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(71, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(17, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(50, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(10, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(55, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(106, wCharacterFormat))
		{
			return false;
		}
		if (CharStyleName != wCharacterFormat.CharStyleName)
		{
			return false;
		}
		if (!Compare(18, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(109, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(70, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(53, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(60, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(75, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(123, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(7, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(59, wCharacterFormat))
		{
			return false;
		}
		if (!Compare(78, wCharacterFormat))
		{
			return false;
		}
		if (!Border.Compare(wCharacterFormat.Border))
		{
			return false;
		}
		if (flag)
		{
			base.PropertiesHash.Remove(3);
		}
		if (wCharacterFormat.Document.IsComparing)
		{
			if (!Compare(58, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(126, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(13, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(120, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(79, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(16, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(0, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(77, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(72, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(125, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(121, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(20, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(108, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(108, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(LocaleIdASCII, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(74, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(76, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(122, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(127, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(81, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(24, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(124, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(80, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(90, wCharacterFormat))
			{
				return false;
			}
			if (!Compare(92, wCharacterFormat))
			{
				return false;
			}
		}
		return true;
	}

	internal void ApplyCharFormatChange(WCharacterFormat charFormat)
	{
		charFormat.CompareProperties(this);
		charFormat.CharStyleName = CharStyleName;
		charFormat.CharStyleId = CharStyleId;
		if (base.Document.ComparisonOptions != null && base.Document.ComparisonOptions.DetectFormatChanges)
		{
			charFormat.IsChangedFormat = true;
			charFormat.FormatChangeAuthorName = base.Document.m_authorName;
			charFormat.FormatChangeDateTime = base.Document.m_dateTime;
			charFormat.Document.CharacterFormatChange(charFormat, null, null);
		}
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((int)BreakClear + ";");
		string text = (Shadow ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Hidden ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (SpecVanish ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append((int)BiDirectionalOverride + ";");
		text = (Bidi ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append((int)EmphasisType + ";");
		text = (SnapToGrid ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (WebHidden ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(LocaleIdBidi + ";");
		stringBuilder.Append((int)TextureStyle + ";");
		text = (OutLine ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(FitTextWidth + ";");
		stringBuilder.Append(FitTextID + ";");
		stringBuilder.Append(HighlightColor.ToArgb() + ";");
		stringBuilder.Append(ForeColor.ToArgb() + ";");
		stringBuilder.Append(TextColor.ToArgb() + ";");
		stringBuilder.Append(UnderlineColor.ToArgb() + ";");
		if (Border != null)
		{
			stringBuilder.Append(Border.GetAsString()?.ToString() + ";");
		}
		return stringBuilder;
	}

	internal void SetKernSize(float value)
	{
		if (value < 0f || value > 1638f)
		{
			value = FontSize;
		}
		Kern = value;
	}

	internal void SetPositionValue(float value)
	{
		if (Math.Abs(value) > 1584f)
		{
			value = 0f;
		}
		SetPropertyValue(17, value);
	}

	internal void SetScalingValue(float value)
	{
		if (value < 1f || value > 600f)
		{
			value = 100f;
		}
		Scaling = value;
	}

	internal void SetCharacterSpacingValue(float value)
	{
		if (Math.Abs(value) > 1584f)
		{
			value = 0f;
		}
		SetPropertyValue(18, value);
	}
}
