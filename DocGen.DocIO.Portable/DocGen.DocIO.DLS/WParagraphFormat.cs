using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class WParagraphFormat : FormatBase
{
	internal const short HrAlignmentKey = 0;

	internal const short LeftIndentKey = 2;

	internal const short RightIndentKey = 3;

	internal const short FirstLineIndentKey = 5;

	internal const short KeepKey = 6;

	internal const short BeforeSpacingKey = 8;

	internal const short AfterSpacingKey = 9;

	internal const short KeepFollowKey = 10;

	internal const short WidowControlKey = 11;

	internal const short BeforeLinesKey = 90;

	internal const short AfterLinesKey = 91;

	internal const short PageBreakBeforeKey = 12;

	internal const short PageBreakAfterKey = 13;

	internal const short BordersKey = 20;

	internal const short BackColorKey = 21;

	internal const short BackGroundColorKey = 23;

	internal const short ColumnBreakAfterKey = 22;

	internal const short TabsKey = 30;

	internal const short BidiKey = 31;

	internal const short ForeColorKey = 32;

	internal const short TextureStyleKey = 33;

	internal const short DataKey = 50;

	internal const short AdjustRightIndentKey = 80;

	internal const short AutoSpaceDEKey = 81;

	internal const short AutoSpaceDNKey = 82;

	internal const short LineSpacingKey = 52;

	internal const short LineSpacingRuleKey = 53;

	internal const short SpacingBeforeAutoKey = 54;

	internal const short SpacingAfterAutoKey = 55;

	internal const short OutlineLevelKey = 56;

	internal const short LeftBorderKey = 57;

	internal const short RightBorderKey = 58;

	internal const short TopBorderKey = 59;

	internal const short BottomBorderKey = 60;

	internal const short LeftBorderNewKey = 61;

	internal const short RightBorderNewKey = 62;

	internal const short TopBorderNewKey = 63;

	internal const short BottomBorderNewKey = 64;

	internal const short BetweenBorderKey = 66;

	internal const short BarBorderKey = 67;

	internal const short BetweenBorderNewKey = 93;

	internal const short BarBorderNewKey = 94;

	internal const short ChangedFormatKey = 65;

	internal const short ContextualSpacingKey = 92;

	internal const short FrameHorizontalPositionKey = 71;

	internal const short FrameVerticalPositionKey = 72;

	internal const short FrameXKey = 73;

	internal const short FrameYKey = 74;

	internal const short FrameWidthKey = 76;

	internal const short FrameHeightKey = 77;

	internal const short FrameHorizontalDistanceFromTextKey = 83;

	internal const short FrameVerticalDistanceFromTextKey = 84;

	internal const short WrapFrameAroundKey = 88;

	internal const short SuppressAutoHyphensKey = 78;

	internal const short MirrorIndentsKey = 75;

	internal const short LeftIndentCharsKey = 85;

	internal const short FirstLineIndentCharsKey = 86;

	internal const short RightIndentCharsKey = 87;

	internal const short WordWrapKey = 89;

	internal const short BaseLineAlignmentKey = 34;

	internal const short SnapToGridKey = 35;

	internal const short SuppressOverlapKey = 36;

	internal const short TextBoxTightWrapKey = 37;

	internal const short SuppressLineNumbersKey = 38;

	internal const short LockFrameAnchorKey = 39;

	internal const short KinsokuKey = 40;

	internal const short OverflowPunctuationKey = 41;

	internal const short TopLinePunctuationKey = 42;

	internal const short DropCapKey = 43;

	internal const short DropCapLinesKey = 44;

	internal const short FrameTextDirectionKey = 48;

	internal const short FormatChangeDateTimeKey = 45;

	internal const short FormatChangeAuthorNameKey = 46;

	internal const short ParagraphStyleNameKey = 47;

	private WParagraphFormat m_tableStyleParagraphFormat;

	private byte m_bFlags;

	internal WAbsoluteTab m_absoluteTab;

	private Dictionary<string, Stream> m_xmlProps;

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

	internal bool WordWrap
	{
		get
		{
			return (bool)GetPropertyValue(89);
		}
		set
		{
			SetPropertyValue(89, value);
		}
	}

	internal WAbsoluteTab AbsoluteTab
	{
		get
		{
			if (m_absoluteTab != null && m_absoluteTab.Owner == null)
			{
				m_absoluteTab.SetOwner(this);
			}
			return m_absoluteTab;
		}
		set
		{
			m_absoluteTab = value;
		}
	}

	internal float FirstLineIndentChars
	{
		get
		{
			return (float)GetPropertyValue(86);
		}
		set
		{
			SetPropertyValue(86, value);
		}
	}

	internal float LeftIndentChars
	{
		get
		{
			return (float)GetPropertyValue(85);
		}
		set
		{
			SetPropertyValue(85, value);
		}
	}

	internal float RightIndentChars
	{
		get
		{
			return (float)GetPropertyValue(87);
		}
		set
		{
			SetPropertyValue(87, value);
		}
	}

	public bool Bidi
	{
		get
		{
			return (bool)GetPropertyValue(31);
		}
		set
		{
			SetPropertyValue(31, value);
		}
	}

	public TabCollection Tabs
	{
		get
		{
			if (!HasValue(30) || (base.IsFormattingChange && base.OldPropertiesHash != null && !base.OldPropertiesHash.ContainsKey(30)))
			{
				CreateTabsCol();
			}
			if (base.IsFormattingChange && base.OldPropertiesHash.ContainsKey(30))
			{
				return (TabCollection)base.OldPropertiesHash[30];
			}
			return (TabCollection)GetPropertyValue(30);
		}
	}

	public bool Keep
	{
		get
		{
			return (bool)GetPropertyValue(6);
		}
		set
		{
			SetPropertyValue(6, value);
		}
	}

	public bool KeepFollow
	{
		get
		{
			return (bool)GetPropertyValue(10);
		}
		set
		{
			SetPropertyValue(10, value);
		}
	}

	public bool PageBreakBefore
	{
		get
		{
			return (bool)GetPropertyValue(12);
		}
		set
		{
			SetPropertyValue(12, value);
		}
	}

	public bool PageBreakAfter
	{
		get
		{
			if (base[13] == null)
			{
				return false;
			}
			return (bool)base[13];
		}
		set
		{
			base[13] = value;
		}
	}

	public bool WidowControl
	{
		get
		{
			return (bool)GetPropertyValue(11);
		}
		set
		{
			SetPropertyValue(11, value);
		}
	}

	internal bool AutoSpaceDN
	{
		get
		{
			return (bool)GetPropertyValue(82);
		}
		set
		{
			SetPropertyValue(82, value);
		}
	}

	internal bool AutoSpaceDE
	{
		get
		{
			return (bool)GetPropertyValue(81);
		}
		set
		{
			SetPropertyValue(81, value);
		}
	}

	internal bool AdjustRightIndent
	{
		get
		{
			return (bool)GetPropertyValue(80);
		}
		set
		{
			SetPropertyValue(80, value);
		}
	}

	public HorizontalAlignment HorizontalAlignment
	{
		get
		{
			HorizontalAlignment logicalJustification = LogicalJustification;
			if (Bidi)
			{
				switch (logicalJustification)
				{
				case HorizontalAlignment.Left:
					return HorizontalAlignment.Right;
				case HorizontalAlignment.Right:
					return HorizontalAlignment.Left;
				}
			}
			return logicalJustification;
		}
		set
		{
			HorizontalAlignment horizontalAlignment = value;
			if (Bidi)
			{
				switch (horizontalAlignment)
				{
				case HorizontalAlignment.Left:
					horizontalAlignment = HorizontalAlignment.Right;
					break;
				case HorizontalAlignment.Right:
					horizontalAlignment = HorizontalAlignment.Left;
					break;
				}
			}
			LogicalJustification = horizontalAlignment;
		}
	}

	internal HorizontalAlignment LogicalJustification
	{
		get
		{
			return (HorizontalAlignment)GetPropertyValue(0);
		}
		set
		{
			SetPropertyValue(0, value);
		}
	}

	public float LeftIndent
	{
		get
		{
			return (float)GetPropertyValue(2);
		}
		set
		{
			if (value < -1584f || value > 1584f)
			{
				throw new ArgumentException("LeftIndent must be between -1584 pt and 1584 pt.");
			}
			SetPropertyValue(2, value);
		}
	}

	public float RightIndent
	{
		get
		{
			return (float)GetPropertyValue(3);
		}
		set
		{
			if (value < -1584f || value > 1584f)
			{
				throw new ArgumentException("RightIndent must be between -1584 pt and 1584 pt.");
			}
			SetPropertyValue(3, value);
		}
	}

	public float FirstLineIndent
	{
		get
		{
			return (float)GetPropertyValue(5);
		}
		set
		{
			if (value < -1584f || value > 1584f)
			{
				throw new ArgumentException("FirstLineIndent must be between -1584 pt and 1584 pt.");
			}
			SetPropertyValue(5, value);
		}
	}

	public float BeforeSpacing
	{
		get
		{
			return (float)GetPropertyValue(8);
		}
		set
		{
			if (value < 0f || value > 1584f)
			{
				throw new ArgumentException("BeforeSpacing must be between 0 pt and 1584 pt.");
			}
			if (SpaceBeforeAuto && !base.Document.IsOpening)
			{
				SpaceBeforeAuto = false;
			}
			SetPropertyValue(8, value);
		}
	}

	internal float BeforeLines
	{
		get
		{
			return (float)GetPropertyValue(90);
		}
		set
		{
			SetPropertyValue(90, value);
		}
	}

	internal float AfterLines
	{
		get
		{
			return (float)GetPropertyValue(91);
		}
		set
		{
			SetPropertyValue(91, value);
		}
	}

	public float AfterSpacing
	{
		get
		{
			return (float)GetPropertyValue(9);
		}
		set
		{
			if (value < 0f || value > 1584f)
			{
				throw new ArgumentException("AfterSpacing must be between 0 pt and 1584 pt.");
			}
			if (SpaceAfterAuto && !base.Document.IsOpening)
			{
				SpaceAfterAuto = false;
			}
			SetPropertyValue(9, value);
		}
	}

	public Borders Borders => GetPropertyValue(20) as Borders;

	public Color BackColor
	{
		get
		{
			return (Color)GetPropertyValue(21);
		}
		set
		{
			SetPropertyValue(21, value);
		}
	}

	internal Color BackGroundColor
	{
		get
		{
			return (Color)GetPropertyValue(23);
		}
		set
		{
			SetPropertyValue(23, value);
		}
	}

	public bool ColumnBreakAfter
	{
		get
		{
			if (base[22] == null)
			{
				return false;
			}
			return (bool)base[22];
		}
		set
		{
			base[22] = value;
		}
	}

	public float LineSpacing
	{
		get
		{
			return (float)GetPropertyValue(52);
		}
		set
		{
			if ((double)value < 0.7 || value > 1584f)
			{
				throw new ArgumentException("LineSpacing must be between 0.7 pt and 1584 pt.");
			}
			SetPropertyValue(52, value);
		}
	}

	public LineSpacingRule LineSpacingRule
	{
		get
		{
			return (LineSpacingRule)GetPropertyValue(53);
		}
		set
		{
			SetPropertyValue(53, value);
		}
	}

	internal Color ForeColor
	{
		get
		{
			return (Color)GetPropertyValue(32);
		}
		set
		{
			SetPropertyValue(32, value);
		}
	}

	internal TextureStyle TextureStyle
	{
		get
		{
			return (TextureStyle)GetPropertyValue(33);
		}
		set
		{
			SetPropertyValue(33, value);
		}
	}

	public bool SpaceBeforeAuto
	{
		get
		{
			return (bool)GetPropertyValue(54);
		}
		set
		{
			SetPropertyValue(54, value);
		}
	}

	public bool SpaceAfterAuto
	{
		get
		{
			return (bool)GetPropertyValue(55);
		}
		set
		{
			SetPropertyValue(55, value);
		}
	}

	public OutlineLevel OutlineLevel
	{
		get
		{
			WParagraphStyle wParagraphStyle = null;
			if (base.OwnerBase is WParagraph)
			{
				wParagraphStyle = (base.OwnerBase as WParagraph).ParaStyle as WParagraphStyle;
			}
			else if (base.OwnerBase is WParagraphStyle)
			{
				wParagraphStyle = base.OwnerBase as WParagraphStyle;
			}
			if (wParagraphStyle != null && wParagraphStyle.BuiltInStyleIdentifier != BuiltinStyle.User && IsBuiltInHeadingStyle(wParagraphStyle.Name))
			{
				return GetOutLineLevelForHeadingStyle(wParagraphStyle.Name);
			}
			byte b = (byte)GetPropertyValue(56);
			if (b <= 9)
			{
				return (OutlineLevel)Enum.ToObject(typeof(OutlineLevel), b);
			}
			return OutlineLevel.BodyText;
		}
		set
		{
			SetPropertyValue(56, (byte)value);
		}
	}

	internal bool IsFrame
	{
		get
		{
			if (base.OwnerBase is WParagraph && ((base.OwnerBase as WParagraph).GetOwnerBaseEntity(base.OwnerBase as Entity) is WTextBox || (base.OwnerBase as WParagraph).GetOwnerBaseEntity(base.OwnerBase as Entity) is Shape))
			{
				return false;
			}
			if (base.OwnerBase is WParagraph && (base.OwnerBase as WParagraph).OwnerTextBody != null && (base.OwnerBase as WParagraph).OwnerTextBody is WTableCell && ((base.OwnerBase as WParagraph).OwnerTextBody as WTableCell).OwnerRow != null)
			{
				WTable ownerTable = ((base.OwnerBase as WParagraph).OwnerTextBody as WTableCell).OwnerRow.OwnerTable;
				if ((ownerTable != null && ownerTable.TableFormat.PropertiesHash.ContainsKey(63)) || ownerTable.TableFormat.PropertiesHash.ContainsKey(62) || ownerTable.TableFormat.PropertiesHash.ContainsKey(64) || (ownerTable.TableFormat.PropertiesHash.ContainsKey(65) && ownerTable.TableFormat.Positioning.VertRelationTo != VerticalRelation.Page))
				{
					return false;
				}
			}
			if (FrameWidth != 0f || FrameHeight != 0f || FrameX != 0f || FrameY != 0f || FrameHorizontalPos != 0 || WrapFrameAround != 0)
			{
				return true;
			}
			if (HasValue(72) && FrameVerticalPos == 2)
			{
				return true;
			}
			return false;
		}
	}

	internal byte FrameVerticalPos
	{
		get
		{
			return (byte)GetPropertyValue(72);
		}
		set
		{
			SetPropertyValue(72, value);
		}
	}

	internal byte FrameVerticalAnchor
	{
		get
		{
			if ((base.Document == null || base.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013) && !HasValue(74))
			{
				if (base.OwnerBase is WParagraph && (base.OwnerBase as WParagraph).ParaStyle != null && (base.OwnerBase as WParagraph).ParaStyle.ParagraphFormat.HasValue(74))
				{
					return (base.OwnerBase as WParagraph).ParaStyle.ParagraphFormat.FrameVerticalPos;
				}
				return 2;
			}
			return FrameVerticalPos;
		}
	}

	internal byte FrameHorizontalPos
	{
		get
		{
			return (byte)GetPropertyValue(71);
		}
		set
		{
			SetPropertyValue(71, value);
		}
	}

	internal float FrameX
	{
		get
		{
			return (float)GetPropertyValue(73);
		}
		set
		{
			SetPropertyValue(73, value);
		}
	}

	internal float FrameY
	{
		get
		{
			return (float)GetPropertyValue(74);
		}
		set
		{
			SetPropertyValue(74, value);
		}
	}

	internal float FrameWidth
	{
		get
		{
			return (float)GetPropertyValue(76);
		}
		set
		{
			SetPropertyValue(76, value);
		}
	}

	internal float FrameHeight
	{
		get
		{
			return (float)GetPropertyValue(77);
		}
		set
		{
			SetPropertyValue(77, value);
		}
	}

	internal float FrameHorizontalDistanceFromText
	{
		get
		{
			return (float)GetPropertyValue(83);
		}
		set
		{
			SetPropertyValue(83, value);
		}
	}

	internal float FrameVerticalDistanceFromText
	{
		get
		{
			return (float)GetPropertyValue(84);
		}
		set
		{
			SetPropertyValue(84, value);
		}
	}

	internal FrameWrapMode WrapFrameAround
	{
		get
		{
			return (FrameWrapMode)GetPropertyValue(88);
		}
		set
		{
			SetPropertyValue(88, value);
		}
	}

	internal bool IsChangedFormat
	{
		get
		{
			return (bool)GetPropertyValue(65);
		}
		set
		{
			if (value)
			{
				SetPropertyValue(65, true);
			}
		}
	}

	public bool ContextualSpacing
	{
		get
		{
			return (bool)GetPropertyValue(92);
		}
		set
		{
			SetPropertyValue(92, value);
		}
	}

	internal WParagraphFormat TableStyleParagraphFormat
	{
		get
		{
			return m_tableStyleParagraphFormat;
		}
		set
		{
			m_tableStyleParagraphFormat = value;
		}
	}

	public bool MirrorIndents
	{
		get
		{
			return (bool)GetPropertyValue(75);
		}
		set
		{
			SetPropertyValue(75, value);
		}
	}

	public bool SuppressAutoHyphens
	{
		get
		{
			return (bool)GetPropertyValue(78);
		}
		set
		{
			SetPropertyValue(78, value);
		}
	}

	internal BaseLineAlignment BaseLineAlignment
	{
		get
		{
			return (BaseLineAlignment)GetPropertyValue(34);
		}
		set
		{
			SetPropertyValue(34, value);
		}
	}

	internal bool SnapToGrid
	{
		get
		{
			return (bool)GetPropertyValue(35);
		}
		set
		{
			SetPropertyValue(35, value);
		}
	}

	internal bool SuppressOverlap
	{
		get
		{
			return (bool)GetPropertyValue(36);
		}
		set
		{
			SetPropertyValue(36, value);
		}
	}

	internal TextboxTightWrapOptions TextboxTightWrap
	{
		get
		{
			return (TextboxTightWrapOptions)GetPropertyValue(37);
		}
		set
		{
			SetPropertyValue(37, value);
		}
	}

	internal bool SuppressLineNumbers
	{
		get
		{
			return (bool)GetPropertyValue(38);
		}
		set
		{
			SetPropertyValue(38, value);
		}
	}

	internal bool LockFrameAnchor
	{
		get
		{
			return (bool)GetPropertyValue(39);
		}
		set
		{
			SetPropertyValue(39, value);
		}
	}

	internal bool Kinsoku
	{
		get
		{
			return (bool)GetPropertyValue(40);
		}
		set
		{
			SetPropertyValue(40, value);
		}
	}

	internal bool OverflowPunctuation
	{
		get
		{
			return (bool)GetPropertyValue(41);
		}
		set
		{
			SetPropertyValue(41, value);
		}
	}

	internal bool TopLinePunctuation
	{
		get
		{
			return (bool)GetPropertyValue(41);
		}
		set
		{
			SetPropertyValue(41, value);
		}
	}

	internal DropCapType DropCap
	{
		get
		{
			return (DropCapType)GetPropertyValue(43);
		}
		set
		{
			SetPropertyValue(43, value);
		}
	}

	internal int DropCapLines
	{
		get
		{
			return (int)GetPropertyValue(44);
		}
		set
		{
			SetPropertyValue(44, value);
		}
	}

	internal byte TextDirection
	{
		get
		{
			return (byte)GetPropertyValue(48);
		}
		set
		{
			SetPropertyValue(48, value);
		}
	}

	internal Dictionary<string, Stream> XmlProps
	{
		get
		{
			if (m_xmlProps == null)
			{
				m_xmlProps = new Dictionary<string, Stream>();
			}
			return m_xmlProps;
		}
	}

	internal DateTime FormatChangeDateTime
	{
		get
		{
			return (DateTime)GetPropertyValue(45);
		}
		set
		{
			SetPropertyValue(45, value);
		}
	}

	internal string FormatChangeAuthorName
	{
		get
		{
			return (string)GetPropertyValue(46);
		}
		set
		{
			SetPropertyValue(46, value);
		}
	}

	internal string ParagraphStyleName
	{
		get
		{
			return (string)GetPropertyValue(47);
		}
		set
		{
			SetPropertyValue(47, value);
		}
	}

	public WParagraphFormat()
	{
	}

	public WParagraphFormat(IWordDocument document)
		: base((WordDocument)document)
	{
	}

	internal bool IsContainFrameKey()
	{
		if (base.PropertiesHash.Count > 0)
		{
			if ((!base.PropertiesHash.ContainsKey(72) || !base.PropertiesHash.ContainsKey(71)) && !base.PropertiesHash.ContainsKey(88) && !base.PropertiesHash.ContainsKey(73) && !base.PropertiesHash.ContainsKey(74) && !base.PropertiesHash.ContainsKey(77) && !base.PropertiesHash.ContainsKey(76) && !base.PropertiesHash.ContainsKey(83))
			{
				return base.PropertiesHash.ContainsKey(84);
			}
			return true;
		}
		return false;
	}

	internal bool IsInFrame()
	{
		if (base.PropertiesHash.Count > 0)
		{
			if (base.OwnerBase is WParagraph && (base.OwnerBase as WParagraph).OwnerTextBody != null && ((base.OwnerBase as WParagraph).OwnerTextBody.Owner is WTextBox || (base.OwnerBase as WParagraph).OwnerTextBody.Owner is Shape))
			{
				return false;
			}
			if ((!base.PropertiesHash.ContainsKey(71) || FrameHorizontalPos == 0) && (!base.PropertiesHash.ContainsKey(72) || FrameVerticalPos == 0) && (!base.PropertiesHash.ContainsKey(88) || WrapFrameAround == FrameWrapMode.Auto) && (!base.PropertiesHash.ContainsKey(74) || FrameY == 0f) && (!base.PropertiesHash.ContainsKey(73) || FrameX == 0f))
			{
				if (base.PropertiesHash.ContainsKey(76))
				{
					return FrameWidth != 0f;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	internal bool IsFrameXAlign(float xPosition)
	{
		if (xPosition != 0f && xPosition != -4f && xPosition != -8f && xPosition != -12f)
		{
			return xPosition == -16f;
		}
		return true;
	}

	internal bool IsFrameYAlign(float yPosition)
	{
		if (yPosition != 0f && yPosition != -4f && yPosition != -8f && yPosition != -12f && yPosition != -16f)
		{
			return yPosition == -20f;
		}
		return true;
	}

	private object GetPropertyValue(int propKey)
	{
		return base[propKey];
	}

	private bool ContainsBordersSprm()
	{
		if (m_sprms != null)
		{
			int[] array = new int[12]
			{
				25636, 50766, 25638, 50768, 25639, 50769, 25637, 50767, 25640, 50770,
				26153, 50771
			};
			for (int i = 0; i < m_sprms.Modifiers.Count; i++)
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord = m_sprms.Modifiers[i];
				if (singlePropertyModifierRecord != null && Array.IndexOf(array, singlePropertyModifierRecord.TypedOptions) != -1)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal object GetSpacingValue(int key)
	{
		int fullKey = GetFullKey(key);
		if (PropertiesHash.ContainsKey(fullKey))
		{
			return PropertiesHash[fullKey];
		}
		FormatBase baseFormat = BaseFormat;
		while (baseFormat != null && baseFormat.PropertiesHash != null && baseFormat != m_doc.DefParaFormat)
		{
			if (baseFormat.PropertiesHash.ContainsKey(fullKey))
			{
				return baseFormat.PropertiesHash[fullKey];
			}
			if (baseFormat.BaseFormat == null && TableStyleParagraphFormat != null)
			{
				if (TableStyleParagraphFormat.PropertiesHash.ContainsKey(fullKey))
				{
					return TableStyleParagraphFormat.PropertiesHash[key];
				}
				baseFormat = TableStyleParagraphFormat.BaseFormat;
			}
			else
			{
				baseFormat = baseFormat.BaseFormat;
			}
		}
		return 0f;
	}

	internal object GetParagraphFormat(int key)
	{
		object result = base[key];
		WParagraphFormat wParagraphFormat = this;
		int fullKey = GetFullKey(key);
		while (!wParagraphFormat.PropertiesHash.ContainsKey(fullKey))
		{
			WListFormat wListFormat = null;
			if (wParagraphFormat.OwnerBase is WParagraph)
			{
				wListFormat = (wParagraphFormat.OwnerBase as WParagraph).ListFormat;
			}
			else if (wParagraphFormat.OwnerBase is WParagraphStyle)
			{
				wListFormat = (wParagraphFormat.OwnerBase as WParagraphStyle).ListFormat;
			}
			else if (wParagraphFormat.OwnerBase is WTableStyle)
			{
				wListFormat = (wParagraphFormat.OwnerBase as WTableStyle).ListFormat;
			}
			else if (wParagraphFormat.OwnerBase is WNumberingStyle)
			{
				wListFormat = (wParagraphFormat.OwnerBase as WNumberingStyle).ListFormat;
			}
			if (wListFormat != null && wListFormat.CurrentListLevel != null && wListFormat.CurrentListLevel.ParagraphFormat.HasValue(key))
			{
				return wListFormat.CurrentListLevel.ParagraphFormat.PropertiesHash[key];
			}
			if (wParagraphFormat.BaseFormat == null)
			{
				if (TableStyleParagraphFormat != null)
				{
					return TableStyleParagraphFormat[key];
				}
				return result;
			}
			wParagraphFormat = wParagraphFormat.BaseFormat as WParagraphFormat;
		}
		return result;
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		base[propKey] = value;
	}

	internal void ChangeTabs(TabCollection tabs)
	{
		base[30] = tabs;
	}

	internal void CreateTabsCol()
	{
		base[30] = new TabCollection(m_doc, this);
	}

	internal bool ContainsValue(int key)
	{
		if (base.PropertiesHash.ContainsKey(key) || (base.BaseFormat is WParagraphFormat && (base.BaseFormat as WParagraphFormat).ContainsValue(key)))
		{
			return true;
		}
		if (base.OwnerBase is WParagraph && (base.OwnerBase as WParagraph).IsInCell)
		{
			WTableCell wTableCell = (base.OwnerBase as WParagraph).GetOwnerEntity() as WTableCell;
			if (wTableCell.Owner is WTableRow && wTableCell.Owner.Owner is WTable && (wTableCell.Owner.Owner as WTable).GetStyle() is WTableStyle wTableStyle && wTableStyle.ParagraphFormat.PropertiesHash.ContainsKey(key))
			{
				return true;
			}
		}
		return false;
	}

	internal bool IsPreviousParagraphInSameFrame()
	{
		if (!(base.OwnerBase is WParagraph))
		{
			return false;
		}
		WParagraph wParagraph = base.OwnerBase as WParagraph;
		IEntity previousSibling = wParagraph.PreviousSibling;
		if (wParagraph.IsInCell && !(previousSibling is WParagraph))
		{
			previousSibling = wParagraph.GetOwnerTableCell(wParagraph.OwnerTextBody).OwnerRow.OwnerTable.PreviousSibling;
			if (previousSibling is WParagraph)
			{
				return IsInSameFrame((previousSibling as WParagraph).ParagraphFormat);
			}
			return false;
		}
		if (previousSibling is WParagraph)
		{
			WParagraphFormat paragraphFormat = (previousSibling as WParagraph).ParagraphFormat;
			return IsInSameFrame(paragraphFormat);
		}
		if (previousSibling is WTable && (previousSibling as WTable).IsFrame)
		{
			WParagraphFormat paragraphFormat2 = (previousSibling as WTable).Rows[0].Cells[0].Paragraphs[0].ParagraphFormat;
			return IsInSameFrame(paragraphFormat2);
		}
		return false;
	}

	internal bool IsNextParagraphInSameFrame()
	{
		if (base.OwnerBase is WParagraph wParagraph && wParagraph.NextSibling is WParagraph)
		{
			WParagraphFormat paragraphFormat = ((WParagraph)wParagraph.NextSibling).ParagraphFormat;
			return IsInSameFrame(paragraphFormat);
		}
		return false;
	}

	internal bool IsInSameFrame(WParagraphFormat paraFormat)
	{
		float num = (ushort)Math.Round(paraFormat.FrameHeight * 20f) & 0x7FFF;
		float num2 = (ushort)Math.Round(FrameHeight * 20f) & 0x7FFF;
		if (paraFormat.IsFrame && paraFormat.FrameX == FrameX && paraFormat.FrameWidth == FrameWidth && num == num2 && paraFormat.FrameHorizontalPos == FrameHorizontalPos && paraFormat.FrameVerticalPos == FrameVerticalPos && paraFormat.FrameY == FrameY)
		{
			return paraFormat.WrapFrameAround == WrapFrameAround;
		}
		return false;
	}

	internal void SetDefaultProperties()
	{
		base.PropertiesHash.Add(21, Color.Empty);
		base.PropertiesHash.Add(8, 0f);
		base.PropertiesHash.Add(9, 0f);
		base.PropertiesHash.Add(31, false);
		base.PropertiesHash.Add(22, false);
		base.PropertiesHash.Add(92, false);
		base.PropertiesHash.Add(5, 0f);
		base.PropertiesHash.Add(32, Color.Empty);
		base.PropertiesHash.Add(0, HorizontalAlignment.Left);
		base.PropertiesHash.Add(6, false);
		base.PropertiesHash.Add(10, false);
		base.PropertiesHash.Add(2, 0f);
		base.PropertiesHash.Add(52, 12f);
		base.PropertiesHash.Add(53, LineSpacingRule.Multiple);
		base.PropertiesHash.Add(56, (byte)9);
		base.PropertiesHash.Add(13, false);
		base.PropertiesHash.Add(12, false);
		base.PropertiesHash.Add(3, 0f);
		base.PropertiesHash.Add(55, false);
		base.PropertiesHash.Add(54, false);
		base.PropertiesHash.Add(33, TextureStyle.TextureNone);
		base.PropertiesHash.Add(11, false);
		base.PropertiesHash.Add(89, true);
		Borders.SetDefaultProperties();
	}

	internal bool IsBuiltInHeadingStyle(string styleName)
	{
		switch (styleName)
		{
		case "Heading 1":
		case "Heading 2":
		case "Heading 3":
		case "Heading 4":
		case "Heading 5":
		case "Heading 6":
		case "Heading 7":
		case "Heading 8":
		case "Heading 9":
			return true;
		default:
			return false;
		}
	}

	private OutlineLevel GetOutLineLevelForHeadingStyle(string styleName)
	{
		return styleName switch
		{
			"Heading 1" => OutlineLevel.Level1, 
			"Heading 2" => OutlineLevel.Level2, 
			"Heading 3" => OutlineLevel.Level3, 
			"Heading 4" => OutlineLevel.Level4, 
			"Heading 5" => OutlineLevel.Level5, 
			"Heading 6" => OutlineLevel.Level6, 
			"Heading 7" => OutlineLevel.Level7, 
			"Heading 8" => OutlineLevel.Level8, 
			"Heading 9" => OutlineLevel.Level9, 
			_ => OutlineLevel.BodyText, 
		};
	}

	protected internal override void EnsureComposites()
	{
		EnsureComposites(20);
	}

	protected override object GetDefValue(int key)
	{
		if (base.Document != null && base.Document.m_defParaFormat != null && base.Document.m_defParaFormat != this)
		{
			return base.Document.m_defParaFormat[key];
		}
		switch (key)
		{
		case 0:
			return HorizontalAlignment.Left;
		case 2:
		case 3:
		case 5:
		case 85:
		case 86:
		case 87:
			return 0f;
		case 6:
		case 10:
		case 12:
		case 13:
		case 22:
		case 31:
		case 65:
			return false;
		case 8:
		case 9:
		case 90:
		case 91:
			return 0f;
		case 21:
		case 23:
		case 32:
			return Color.Empty;
		case 50:
			return null;
		case 52:
			return 12f;
		case 53:
			return LineSpacingRule.Multiple;
		case 30:
			return new TabCollection(base.Document, this);
		case 33:
			return TextureStyle.TextureNone;
		case 54:
		case 55:
		case 75:
		case 78:
		case 80:
		case 81:
		case 82:
		case 92:
			return false;
		case 56:
			return byte.MaxValue;
		case 11:
		case 89:
			return true;
		case 71:
		case 72:
			return (byte)0;
		case 73:
		case 74:
		case 76:
		case 77:
		case 83:
		case 84:
			return 0f;
		case 88:
			return FrameWrapMode.Auto;
		case 34:
			return BaseLineAlignment.Auto;
		case 35:
		case 41:
			return true;
		case 36:
		case 38:
		case 39:
		case 40:
		case 42:
			return false;
		case 37:
			return TextboxTightWrapOptions.None;
		case 43:
			return DropCapType.None;
		case 44:
			return 1;
		case 46:
			return string.Empty;
		case 45:
			return DateTime.MinValue;
		case 47:
			return string.Empty;
		case 48:
			return 0;
		default:
			throw new ArgumentException("key has invalid value");
		}
	}

	protected override FormatBase GetDefComposite(int key)
	{
		if (key == 20)
		{
			return GetDefComposite(20, new Borders(this, 20));
		}
		return null;
	}

	protected internal new void ImportContainer(FormatBase format)
	{
		base.ImportContainer(format);
	}

	private void ImportXmlProps(WParagraphFormat format)
	{
		if (format.m_xmlProps != null && format.m_xmlProps.Count > 0)
		{
			format.Document.CloneProperties(format.XmlProps, ref m_xmlProps);
		}
	}

	protected override void ImportMembers(FormatBase format)
	{
		base.ImportMembers(format);
		if (format is WParagraphFormat wParagraphFormat)
		{
			CopyFormat(format);
			if (wParagraphFormat.HasValue(13))
			{
				base[13] = wParagraphFormat.PageBreakAfter;
			}
			if (wParagraphFormat.HasValue(22))
			{
				base[22] = wParagraphFormat.ColumnBreakAfter;
			}
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("Bidi"))
		{
			Bidi = reader.ReadBoolean("Bidi");
		}
		if (reader.HasAttribute("HrAlignment"))
		{
			HorizontalAlignment = (HorizontalAlignment)(object)reader.ReadEnum("HrAlignment", typeof(HorizontalAlignment));
		}
		if (reader.HasAttribute("LeftIndent"))
		{
			SetPropertyValue(2, reader.ReadFloat("LeftIndent"));
		}
		if (reader.HasAttribute("RightIndent"))
		{
			SetPropertyValue(3, reader.ReadFloat("RightIndent"));
		}
		if (reader.HasAttribute("FirstLineIndent"))
		{
			SetPropertyValue(5, reader.ReadFloat("FirstLineIndent"));
		}
		if (reader.HasAttribute("Keep"))
		{
			Keep = reader.ReadBoolean("Keep");
		}
		if (reader.HasAttribute("BeforeSpacing"))
		{
			SetPropertyValue(8, reader.ReadFloat("BeforeSpacing"));
		}
		if (reader.HasAttribute("AfterSpacing"))
		{
			SetPropertyValue(9, reader.ReadFloat("AfterSpacing"));
		}
		if (reader.HasAttribute("KeepFollow"))
		{
			KeepFollow = reader.ReadBoolean("KeepFollow");
		}
		if (reader.HasAttribute("WidowControl"))
		{
			WidowControl = reader.ReadBoolean("WidowControl");
		}
		if (reader.HasAttribute("PageBreakBefore"))
		{
			PageBreakBefore = reader.ReadBoolean("PageBreakBefore");
		}
		if (reader.HasAttribute("PageBreakAfter"))
		{
			PageBreakAfter = reader.ReadBoolean("PageBreakAfter");
		}
		if (reader.HasAttribute("BackColor"))
		{
			BackColor = reader.ReadColor("BackColor");
		}
		if (reader.HasAttribute("ColumnBreakAfter"))
		{
			ColumnBreakAfter = reader.ReadBoolean("ColumnBreakAfter");
		}
		if (reader.HasAttribute("LineSpacing"))
		{
			SetPropertyValue(52, reader.ReadFloat("LineSpacing"));
		}
		if (reader.HasAttribute("LineSpacingRule"))
		{
			LineSpacingRule = (LineSpacingRule)(object)reader.ReadEnum("LineSpacingRule", typeof(LineSpacingRule));
		}
		if (reader.HasAttribute("ForeColor"))
		{
			ForeColor = reader.ReadColor("ForeColor");
		}
		if (reader.HasAttribute("Texture"))
		{
			TextureStyle = (TextureStyle)(object)reader.ReadEnum("Texture", typeof(TextureStyle));
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (HasKey(13))
		{
			writer.WriteValue("PageBreakAfter", PageBreakAfter);
		}
		if (HasKey(22))
		{
			writer.WriteValue("ColumnBreakAfter", ColumnBreakAfter);
		}
		if (m_sprms == null)
		{
			if (HasValue(31))
			{
				writer.WriteValue("Bidi", Bidi);
			}
			if (HasValue(0))
			{
				writer.WriteValue("HrAlignment", HorizontalAlignment);
			}
			if (HasValue(2))
			{
				writer.WriteValue("LeftIndent", LeftIndent);
			}
			if (HasValue(3))
			{
				writer.WriteValue("RightIndent", RightIndent);
			}
			if (HasValue(5))
			{
				writer.WriteValue("FirstLineIndent", FirstLineIndent);
			}
			if (HasValue(6))
			{
				writer.WriteValue("Keep", Keep);
			}
			if (HasValue(8))
			{
				writer.WriteValue("BeforeSpacing", BeforeSpacing);
			}
			if (HasValue(9))
			{
				writer.WriteValue("AfterSpacing", AfterSpacing);
			}
			if (HasValue(10))
			{
				writer.WriteValue("KeepFollow", KeepFollow);
			}
			if (HasValue(11))
			{
				writer.WriteValue("WidowControl", WidowControl);
			}
			if (HasValue(12))
			{
				writer.WriteValue("PageBreakBefore", PageBreakBefore);
			}
			if (!BackColor.IsEmpty)
			{
				writer.WriteValue("BackColor", BackColor);
			}
			if (HasValue(52))
			{
				writer.WriteValue("LineSpacing", LineSpacing);
			}
			if (HasValue(53))
			{
				writer.WriteValue("LineSpacingRule", LineSpacingRule);
			}
			if (ForeColor != Color.Empty)
			{
				writer.WriteValue("ForeColor", ForeColor);
			}
			if (TextureStyle != 0)
			{
				writer.WriteValue("Texture", TextureStyle);
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
			ParagraphPropertiesConverter.SprmsToFormat(singlePropertyModifierArray, this, null, null);
			singlePropertyModifierArray.Clear();
		}
		return result;
	}

	protected override void InitXDLSHolder()
	{
		if (m_sprms == null)
		{
			base.XDLSHolder.AddElement("borders", Borders);
			base.XDLSHolder.AddElement("Tabs", Tabs);
		}
	}

	internal override void AcceptChanges()
	{
		base[65] = false;
		if (base.OldPropertiesHash != null && base.OldPropertiesHash.Count > 0)
		{
			base.OldPropertiesHash.Clear();
			base.AcceptChanges();
		}
	}

	internal override void RemovePositioning()
	{
		if (m_sprms != null && m_sprms.Count > 0)
		{
			m_sprms.RemoveValue(9755);
			m_sprms.RemoveValue(9251);
			m_sprms.RemoveValue(33816);
			m_sprms.RemoveValue(33817);
			m_sprms.RemoveValue(33839);
			m_sprms.RemoveValue(17954);
			m_sprms.RemoveValue(33838);
			m_sprms.RemoveValue(33818);
			m_sprms.RemoveValue(17451);
		}
	}

	internal override void ApplyBase(FormatBase baseFormat)
	{
		base.ApplyBase(baseFormat);
		if (baseFormat == null)
		{
			Borders.ApplyBase(null);
		}
		else
		{
			Borders.ApplyBase((baseFormat as WParagraphFormat).Borders);
		}
	}

	internal override void Close()
	{
		base.Close();
		if (m_tableStyleParagraphFormat != null)
		{
			m_tableStyleParagraphFormat.Close();
			m_tableStyleParagraphFormat = null;
		}
		if (m_xmlProps != null)
		{
			m_xmlProps.Clear();
			m_xmlProps = null;
		}
		if (Borders != null)
		{
			Borders.Close();
		}
	}

	internal override bool HasValue(int propertyKey)
	{
		if (HasKey(propertyKey))
		{
			return true;
		}
		if (propertyKey != 0)
		{
			_ = 31;
		}
		return false;
	}

	internal bool HasValueWithParent(int propertyKey)
	{
		bool flag = HasValue(propertyKey);
		if (!flag && base.BaseFormat is WParagraphFormat)
		{
			return (base.BaseFormat as WParagraphFormat).HasValueWithParent(propertyKey);
		}
		return flag;
	}

	internal bool HasBorder()
	{
		if (!Borders.Horizontal.IsBorderDefined && !Borders.Left.IsBorderDefined && !Borders.Right.IsBorderDefined && !Borders.Top.IsBorderDefined)
		{
			return Borders.Bottom.IsBorderDefined;
		}
		return true;
	}

	internal HorizontalAlignment GetAlignmentToRender()
	{
		HorizontalAlignment horizontalAlignment = HorizontalAlignment;
		switch ((byte)horizontalAlignment)
		{
		case 6:
			horizontalAlignment = (HorizontalAlignment)(Bidi ? 2u : 0u);
			break;
		case 5:
		case 7:
		case 8:
		case 9:
			horizontalAlignment = HorizontalAlignment.Justify;
			break;
		}
		return horizontalAlignment;
	}

	internal bool HasShading()
	{
		if (base.PropertiesHash.ContainsKey(21) || base.PropertiesHash.ContainsKey(33) || base.PropertiesHash.ContainsKey(32))
		{
			return true;
		}
		return false;
	}

	internal override int GetSprmOption(int propertyKey)
	{
		switch (propertyKey)
		{
		case 31:
			return 9281;
		case 6:
			return 9221;
		case 10:
			return 9222;
		case 12:
			return 9223;
		case 75:
			return 9328;
		case 11:
			return 9265;
		case 2:
			return 33807;
		case 3:
			return 33806;
		case 5:
			return 33809;
		case 90:
			return 17496;
		case 91:
			return 17497;
		case 8:
			return 42003;
		case 9:
			return 42004;
		case 52:
		case 53:
			return 25618;
		case 0:
			return 9219;
		case 55:
			return 9308;
		case 54:
			return 9307;
		case 56:
			return 9792;
		case 57:
			return 25637;
		case 61:
			return 50767;
		case 58:
			return 25639;
		case 62:
			return 50769;
		case 59:
			return 25636;
		case 63:
			return 50766;
		case 60:
			return 25638;
		case 64:
			return 50768;
		case 66:
			return 25640;
		case 93:
			return 50770;
		case 94:
			return 50771;
		case 67:
			return 26153;
		case 71:
		case 72:
			return 9755;
		case 73:
			return 33816;
		case 74:
			return 33817;
		case 76:
			return 33818;
		case 77:
			return 17451;
		case 83:
			return 33839;
		case 84:
			return 33838;
		case 88:
			return 9251;
		case 92:
			return 9325;
		case 78:
			return 9258;
		case 81:
			return 9271;
		case 82:
			return 9272;
		case 80:
			return 9288;
		case 85:
			return 17494;
		case 87:
			return 17493;
		case 86:
			return 17495;
		case 89:
			return 9268;
		case 34:
			return 17465;
		case 35:
			return 9287;
		case 36:
			return 9314;
		case 38:
			return 9228;
		case 37:
			return 9329;
		case 39:
			return 9264;
		case 40:
			return 9267;
		case 41:
			return 9269;
		case 42:
			return 9270;
		case 43:
		case 44:
			return 17452;
		case 65:
			return 9828;
		case 48:
			return 17466;
		default:
			return int.MaxValue;
		}
	}

	internal void UpdateJustification(SinglePropertyModifierArray sprms, SinglePropertyModifierRecord sprmPJc)
	{
		if (base.Document == null || base.Document.WordVersion == 0 || base.Document.WordVersion > 217)
		{
			return;
		}
		if (sprmPJc != null && (base.Document.WordVersion == 193 || !Bidi))
		{
			base[0] = sprmPJc.ByteValue;
		}
		else
		{
			if (base.Document.WordVersion == 193 || !Bidi)
			{
				return;
			}
			byte byteValue = sprmPJc.ByteValue;
			if (byteValue == 5)
			{
				sprms.SetByteValue(9313, 5);
				sprms.SetIntValue(9219, 4);
			}
			if (base.Document.WordVersion == 217)
			{
				if ((!(base.OwnerBase is WParagraph) || sprmPJc != null) && byteValue == 0)
				{
					base[0] = HorizontalAlignment.Right;
				}
				else
				{
					base[0] = (HorizontalAlignment)((byteValue != 2) ? byteValue : 0);
				}
			}
			else if (base.Document.WordVersion == 192)
			{
				base[0] = (HorizontalAlignment)byteValue;
			}
			else if (Bidi)
			{
				base[0] = byteValue switch
				{
					0 => HorizontalAlignment.Right, 
					2 => HorizontalAlignment.Left, 
					_ => (HorizontalAlignment)byteValue, 
				};
			}
			else
			{
				base[0] = (HorizontalAlignment)byteValue;
			}
		}
	}

	internal void UpdateBiDi(bool value)
	{
		if (base.Document == null || base.Document.WordVersion != 193)
		{
			base[31] = value;
		}
	}

	internal void UpdateTabs(SinglePropertyModifierArray sprms)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = (base.IsFormattingChange ? sprms.GetOldSprm(50701, 9828) : sprms.GetNewSprm(50701, 9828));
		if (singlePropertyModifierRecord == null)
		{
			singlePropertyModifierRecord = (base.IsFormattingChange ? sprms.GetOldSprm(50709, 9828) : sprms.GetNewSprm(50709, 9828));
		}
		if (singlePropertyModifierRecord != null)
		{
			TabsInfo info = new TabsInfo(singlePropertyModifierRecord);
			TabCollection tabCollection = new TabCollection(base.Document, this)
			{
				CancelOnChangeEvent = true
			};
			ParagraphPropertiesConverter.ExportTabs(info, tabCollection);
			base[30] = tabCollection;
			tabCollection.CancelOnChangeEvent = false;
		}
	}

	internal void UpdateOldFormatBorders(ref Borders borders)
	{
		if (base.IsFormattingChange && borders == null)
		{
			borders = new Borders();
			SetPropertyValue(20, borders);
		}
	}

	internal BorderCode GetBorder(SinglePropertyModifierRecord record)
	{
		byte[] byteArray = record.ByteArray;
		BorderCode borderCode;
		if (byteArray.Length == 4)
		{
			borderCode = new BorderCode(record.ByteArray, 0);
		}
		else
		{
			borderCode = new BorderCode();
			borderCode.ParseNewBrc(byteArray, 0);
		}
		return borderCode;
	}

	internal void UpdateSourceFormat(WParagraphFormat destBaseFormat)
	{
		WParagraphFormat wParagraphFormat = new WParagraphFormat(destBaseFormat.Document);
		wParagraphFormat.ImportContainer(this);
		wParagraphFormat.CopyProperties(this);
		wParagraphFormat.ApplyBase(destBaseFormat);
		UpdateSourceFormatting(wParagraphFormat);
		ImportContainer(wParagraphFormat);
		CopyProperties(wParagraphFormat);
		wParagraphFormat.Close();
		wParagraphFormat = null;
	}

	internal void UpdateSourceFormatting(WParagraphFormat format)
	{
		if (format.AdjustRightIndent != AdjustRightIndent)
		{
			format.AdjustRightIndent = AdjustRightIndent;
		}
		if (format.AfterSpacing != AfterSpacing)
		{
			format.SetPropertyValue(9, AfterSpacing);
		}
		if (format.AutoSpaceDE != AutoSpaceDE)
		{
			format.AutoSpaceDE = AutoSpaceDE;
		}
		if (format.AutoSpaceDN != AutoSpaceDN)
		{
			format.AutoSpaceDN = AutoSpaceDN;
		}
		if (format.BackColor != BackColor)
		{
			format.BackColor = BackColor;
		}
		if (format.BeforeSpacing != BeforeSpacing)
		{
			format.SetPropertyValue(8, BeforeSpacing);
		}
		if (format.Bidi != Bidi)
		{
			format.Bidi = Bidi;
		}
		if (format.ColumnBreakAfter != ColumnBreakAfter)
		{
			format.ColumnBreakAfter = ColumnBreakAfter;
		}
		if (format.ContextualSpacing != ContextualSpacing)
		{
			format.ContextualSpacing = ContextualSpacing;
		}
		if (format.FirstLineIndent != FirstLineIndent)
		{
			format.SetPropertyValue(5, FirstLineIndent);
		}
		if (format.ForeColor != ForeColor)
		{
			format.ForeColor = ForeColor;
		}
		if (format.FrameHeight != FrameHeight)
		{
			format.FrameHeight = FrameHeight;
		}
		if (format.FrameHorizontalDistanceFromText != FrameHorizontalDistanceFromText)
		{
			format.FrameHorizontalDistanceFromText = FrameHorizontalDistanceFromText;
		}
		if (format.FrameHorizontalPos != FrameHorizontalPos)
		{
			format.FrameHorizontalPos = FrameHorizontalPos;
		}
		if (format.FrameVerticalDistanceFromText != FrameVerticalDistanceFromText)
		{
			format.FrameVerticalDistanceFromText = FrameVerticalDistanceFromText;
		}
		if (format.FrameVerticalPos != FrameVerticalPos)
		{
			format.FrameVerticalPos = FrameVerticalPos;
		}
		if (format.FrameWidth != FrameWidth)
		{
			format.FrameWidth = FrameWidth;
		}
		if (format.FrameX != FrameX)
		{
			format.FrameX = FrameX;
		}
		if (format.FrameY != FrameY)
		{
			format.FrameY = FrameY;
		}
		if (format.HorizontalAlignment != HorizontalAlignment)
		{
			format.HorizontalAlignment = HorizontalAlignment;
		}
		if (format.Keep != Keep)
		{
			format.Keep = Keep;
		}
		if (format.KeepFollow != KeepFollow)
		{
			format.KeepFollow = KeepFollow;
		}
		if (format.LeftIndent != LeftIndent)
		{
			format.SetPropertyValue(2, LeftIndent);
		}
		if (format.LineSpacing != LineSpacing)
		{
			format.SetPropertyValue(52, LineSpacing);
		}
		if (format.LineSpacingRule != LineSpacingRule)
		{
			format.LineSpacingRule = LineSpacingRule;
			if (!format.HasValue(52))
			{
				format.SetPropertyValue(52, LineSpacing);
			}
		}
		if (format.MirrorIndents != MirrorIndents)
		{
			format.MirrorIndents = MirrorIndents;
		}
		if (format.OutlineLevel != OutlineLevel)
		{
			format.OutlineLevel = OutlineLevel;
		}
		if (format.PageBreakAfter != PageBreakAfter)
		{
			format.PageBreakAfter = PageBreakAfter;
		}
		if (format.PageBreakBefore != PageBreakBefore)
		{
			format.PageBreakBefore = PageBreakBefore;
		}
		if (format.RightIndent != RightIndent)
		{
			format.SetPropertyValue(3, RightIndent);
		}
		if (format.SpaceAfterAuto != SpaceAfterAuto)
		{
			format.SpaceAfterAuto = SpaceAfterAuto;
		}
		if (format.SpaceBeforeAuto != SpaceBeforeAuto)
		{
			format.SpaceBeforeAuto = SpaceBeforeAuto;
		}
		if (format.SuppressAutoHyphens != SuppressAutoHyphens)
		{
			format.SuppressAutoHyphens = SuppressAutoHyphens;
		}
		if (format.TextureStyle != TextureStyle)
		{
			format.TextureStyle = TextureStyle;
		}
		if (format.WidowControl != WidowControl)
		{
			format.WidowControl = WidowControl;
		}
		if (format.WrapFrameAround != WrapFrameAround)
		{
			format.WrapFrameAround = WrapFrameAround;
		}
		if (format.LeftIndentChars != LeftIndentChars)
		{
			format.LeftIndentChars = LeftIndentChars;
		}
		if (format.FirstLineIndentChars != FirstLineIndentChars)
		{
			format.FirstLineIndentChars = FirstLineIndentChars;
		}
		if (format.RightIndentChars != RightIndentChars)
		{
			format.RightIndentChars = RightIndentChars;
		}
		Borders.UpdateSourceFormatting(format.Borders);
		CompareListFormat(format);
	}

	private void CompareListFormat(WParagraphFormat format)
	{
		if (!(base.OwnerBase is WParagraph) || (base.OwnerBase as WParagraph).ListFormat.ListType == ListType.NoList || (base.OwnerBase as WParagraph).ListFormat.CurrentListLevel == null)
		{
			return;
		}
		int[] array = new int[format.PropertiesHash.Count];
		format.PropertiesHash.Keys.CopyTo(array, 0);
		int[] array2 = array;
		foreach (int key in array2)
		{
			if (!base.PropertiesHash.ContainsKey(key) && (base.OwnerBase as WParagraph).ListFormat.CurrentListLevel.ParagraphFormat.IsValueDefined(key))
			{
				format.RemoveValue(key);
			}
		}
	}

	internal void NestedParaFormatting(WParagraphFormat format)
	{
		if (!format.HasKey(80) && HasKey(80))
		{
			format.AdjustRightIndent = AdjustRightIndent;
		}
		if (!format.HasKey(9) && HasKey(9))
		{
			format.SetPropertyValue(9, AfterSpacing);
		}
		if (!format.HasKey(81) && HasKey(81))
		{
			format.AutoSpaceDE = AutoSpaceDE;
		}
		if (!format.HasKey(82) && HasKey(82))
		{
			format.AutoSpaceDN = AutoSpaceDN;
		}
		if (!format.HasKey(21) && HasKey(21))
		{
			format.BackColor = BackColor;
		}
		if (!format.HasKey(8) && HasKey(8))
		{
			format.SetPropertyValue(8, BeforeSpacing);
		}
		if (!format.HasKey(31) && HasKey(31))
		{
			format.Bidi = Bidi;
		}
		if (!format.HasKey(22) && HasKey(22))
		{
			format.ColumnBreakAfter = ColumnBreakAfter;
		}
		if (!format.HasKey(92) && HasKey(92))
		{
			format.ContextualSpacing = ContextualSpacing;
		}
		if (!format.HasKey(5) && HasKey(5))
		{
			format.SetPropertyValue(5, FirstLineIndent);
		}
		if (!format.HasKey(32) && HasKey(32))
		{
			format.ForeColor = ForeColor;
		}
		if (!format.HasKey(77) && HasKey(77))
		{
			format.FrameHeight = FrameHeight;
		}
		if (!format.HasKey(83) && HasKey(83))
		{
			format.FrameHorizontalDistanceFromText = FrameHorizontalDistanceFromText;
		}
		if (!format.HasKey(71) && HasKey(71))
		{
			format.FrameHorizontalPos = FrameHorizontalPos;
		}
		if (!format.HasKey(84) && HasKey(84))
		{
			format.FrameVerticalDistanceFromText = FrameVerticalDistanceFromText;
		}
		if (!format.HasKey(72) && HasKey(72))
		{
			format.FrameVerticalPos = FrameVerticalPos;
		}
		if (!format.HasKey(76) && HasKey(76))
		{
			format.FrameWidth = FrameWidth;
		}
		if (!format.HasKey(73) && HasKey(73))
		{
			format.FrameX = FrameX;
		}
		if (!format.HasKey(74) && HasKey(74))
		{
			format.FrameY = FrameY;
		}
		if (!format.HasKey(0) && HasKey(0))
		{
			format.HorizontalAlignment = HorizontalAlignment;
		}
		if (!format.HasKey(6) && HasKey(6))
		{
			format.Keep = Keep;
		}
		if (!format.HasKey(10) && HasKey(10))
		{
			format.KeepFollow = KeepFollow;
		}
		if (!format.HasKey(2) && HasKey(2))
		{
			format.SetPropertyValue(2, LeftIndent);
		}
		if (!format.HasKey(52) && HasKey(52))
		{
			format.SetPropertyValue(52, LineSpacing);
		}
		if (!format.HasKey(53) && HasKey(53))
		{
			format.LineSpacingRule = LineSpacingRule;
			if (!format.HasValue(52) && HasKey(52))
			{
				format.SetPropertyValue(52, LineSpacing);
			}
		}
		if (!format.HasKey(75) && HasKey(75))
		{
			format.MirrorIndents = MirrorIndents;
		}
		if (!format.HasKey(56) && HasKey(56))
		{
			format.OutlineLevel = OutlineLevel;
		}
		if (!format.HasKey(13) && HasKey(13))
		{
			format.PageBreakAfter = PageBreakAfter;
		}
		if (!format.HasKey(12) && HasKey(12))
		{
			format.PageBreakBefore = PageBreakBefore;
		}
		if (!format.HasKey(3) && HasKey(3))
		{
			format.SetPropertyValue(3, RightIndent);
		}
		if (!format.HasKey(55) && HasKey(55))
		{
			format.SpaceAfterAuto = SpaceAfterAuto;
		}
		if (!format.HasKey(54) && HasKey(54))
		{
			format.SpaceBeforeAuto = SpaceBeforeAuto;
		}
		if (!format.HasKey(78) && HasKey(78))
		{
			format.SuppressAutoHyphens = SuppressAutoHyphens;
		}
		if (!format.HasKey(33) && HasKey(33))
		{
			format.TextureStyle = TextureStyle;
		}
		if (!format.HasKey(11) && HasKey(11))
		{
			format.WidowControl = WidowControl;
		}
		if (!format.HasKey(88) && HasKey(88))
		{
			format.WrapFrameAround = WrapFrameAround;
		}
		if (!format.HasKey(85) && HasKey(85))
		{
			format.LeftIndentChars = LeftIndentChars;
		}
		if (!format.HasKey(86) && HasKey(86))
		{
			format.FirstLineIndentChars = FirstLineIndentChars;
		}
		if (!format.HasKey(87) && HasKey(87))
		{
			format.RightIndentChars = RightIndentChars;
		}
		if (format.Borders.NoBorder && !Borders.NoBorder)
		{
			Borders.UpdateSourceFormatting(format.Borders);
		}
	}

	private bool IsValueDefined(int key)
	{
		while (key > 255)
		{
			key >>= 8;
		}
		return HasValue(key);
	}

	private void RemoveValue(int key)
	{
		base.PropertiesHash.Remove(key);
	}

	public override void ClearFormatting()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
		}
		if (m_sprms != null)
		{
			m_sprms.Clear();
		}
		if (m_xmlProps != null)
		{
			m_xmlProps.Clear();
		}
	}

	internal void SetFrameHorizontalDistanceFromTextValue(float value)
	{
		if (value < 0f || value > 1638f)
		{
			value = 0f;
		}
		FrameHorizontalDistanceFromText = value;
	}

	internal void SetFrameVerticalDistanceFromTextValue(float value)
	{
		if (value < 0f || value > 1638f)
		{
			value = 0f;
		}
		FrameVerticalDistanceFromText = value;
	}

	internal void SetFrameYValue(float value)
	{
		if (value < -1584f && value > 1584f)
		{
			value = 0f;
		}
		FrameY = value;
	}

	internal void SetFrameXValue(float value)
	{
		if (value < -1584f && value > 1584f)
		{
			value = 0f;
		}
		FrameX = value;
	}

	internal void SetFrameWidthValue(float value)
	{
		if ((double)value < 0.05 || value > 1584f)
		{
			value = 0f;
		}
		FrameWidth = value;
	}

	internal bool Compare(WParagraphFormat paragraphFormat)
	{
		if (!Compare(91, paragraphFormat))
		{
			return false;
		}
		if (!Compare(9, paragraphFormat))
		{
			return false;
		}
		if (!Compare(81, paragraphFormat))
		{
			return false;
		}
		if (!Compare(82, paragraphFormat))
		{
			return false;
		}
		if (!Compare(21, paragraphFormat))
		{
			return false;
		}
		if (!Compare(90, paragraphFormat))
		{
			return false;
		}
		if (!Compare(8, paragraphFormat))
		{
			return false;
		}
		if (!Compare(31, paragraphFormat))
		{
			return false;
		}
		if (!Compare(22, paragraphFormat))
		{
			return false;
		}
		if (!Compare(92, paragraphFormat))
		{
			return false;
		}
		if (!Compare(5, paragraphFormat))
		{
			return false;
		}
		if (!Compare(86, paragraphFormat))
		{
			return false;
		}
		if (!Compare(32, paragraphFormat))
		{
			return false;
		}
		if (!Compare(77, paragraphFormat))
		{
			return false;
		}
		if (!Compare(83, paragraphFormat))
		{
			return false;
		}
		if (!Compare(71, paragraphFormat))
		{
			return false;
		}
		if (!Compare(84, paragraphFormat))
		{
			return false;
		}
		if (!Compare(72, paragraphFormat))
		{
			return false;
		}
		if (!Compare(76, paragraphFormat))
		{
			return false;
		}
		if (!Compare(73, paragraphFormat))
		{
			return false;
		}
		if (!Compare(74, paragraphFormat))
		{
			return false;
		}
		if (!Compare(32, paragraphFormat))
		{
			return false;
		}
		if (!Compare(77, paragraphFormat))
		{
			return false;
		}
		if (!Compare(6, paragraphFormat))
		{
			return false;
		}
		if (!Compare(10, paragraphFormat))
		{
			return false;
		}
		if (IsFrame != paragraphFormat.IsFrame)
		{
			return false;
		}
		if (!Compare(2, paragraphFormat))
		{
			return false;
		}
		if (!Compare(85, paragraphFormat))
		{
			return false;
		}
		if (!Compare(52, paragraphFormat))
		{
			return false;
		}
		if (!Compare(53, paragraphFormat))
		{
			return false;
		}
		if (!Compare(0, paragraphFormat))
		{
			return false;
		}
		if (!Compare(75, paragraphFormat))
		{
			return false;
		}
		if (!Compare(56, paragraphFormat))
		{
			return false;
		}
		if (!Compare(13, paragraphFormat))
		{
			return false;
		}
		if (!Compare(12, paragraphFormat))
		{
			return false;
		}
		if (!Compare(3, paragraphFormat))
		{
			return false;
		}
		if (!Compare(87, paragraphFormat))
		{
			return false;
		}
		if (!Compare(55, paragraphFormat))
		{
			return false;
		}
		if (!Compare(54, paragraphFormat))
		{
			return false;
		}
		if (!Compare(78, paragraphFormat))
		{
			return false;
		}
		if (!Compare(33, paragraphFormat))
		{
			return false;
		}
		if (!Compare(11, paragraphFormat))
		{
			return false;
		}
		if (!Compare(89, paragraphFormat))
		{
			return false;
		}
		if (!Compare(88, paragraphFormat))
		{
			return false;
		}
		if (!Tabs.Compare(paragraphFormat.Tabs))
		{
			return false;
		}
		if (Borders != null && paragraphFormat.Borders != null && !Borders.Compare(paragraphFormat.Borders))
		{
			return false;
		}
		if (paragraphFormat.Document.IsComparing)
		{
			if (!Compare(80, paragraphFormat))
			{
				return false;
			}
			if (!Compare(23, paragraphFormat))
			{
				return false;
			}
			if (!Compare(67, paragraphFormat))
			{
				return false;
			}
			if (!Compare(94, paragraphFormat))
			{
				return false;
			}
			if (!Compare(34, paragraphFormat))
			{
				return false;
			}
			if (!Compare(66, paragraphFormat))
			{
				return false;
			}
			if (!Compare(93, paragraphFormat))
			{
				return false;
			}
			if (!Compare(60, paragraphFormat))
			{
				return false;
			}
			if (!Compare(64, paragraphFormat))
			{
				return false;
			}
			if (!Compare(50, paragraphFormat))
			{
				return false;
			}
			if (!Compare(43, paragraphFormat))
			{
				return false;
			}
			if (!Compare(44, paragraphFormat))
			{
				return false;
			}
			if (!Compare(48, paragraphFormat))
			{
				return false;
			}
			if (!Compare(40, paragraphFormat))
			{
				return false;
			}
			if (!Compare(57, paragraphFormat))
			{
				return false;
			}
			if (!Compare(61, paragraphFormat))
			{
				return false;
			}
			if (!Compare(39, paragraphFormat))
			{
				return false;
			}
			if (!Compare(41, paragraphFormat))
			{
				return false;
			}
			if (!Compare(47, paragraphFormat))
			{
				return false;
			}
			if (!Compare(58, paragraphFormat))
			{
				return false;
			}
			if (!Compare(62, paragraphFormat))
			{
				return false;
			}
			if (!Compare(35, paragraphFormat))
			{
				return false;
			}
			if (!Compare(38, paragraphFormat))
			{
				return false;
			}
			if (!Compare(36, paragraphFormat))
			{
				return false;
			}
			if (!Compare(37, paragraphFormat))
			{
				return false;
			}
			if (!Compare(59, paragraphFormat))
			{
				return false;
			}
			if (!Compare(63, paragraphFormat))
			{
				return false;
			}
			if (!Compare(42, paragraphFormat))
			{
				return false;
			}
		}
		return true;
	}
}
