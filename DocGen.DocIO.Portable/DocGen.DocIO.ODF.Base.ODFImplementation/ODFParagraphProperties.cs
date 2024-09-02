using System;
using System.Collections.Generic;
using DocGen.DocIO.ODFConverter.Base.ODFImplementation;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class ODFParagraphProperties : CommonTableParaProperties
{
	private HyphenationKeep m_hyphenationKeep;

	private int m_hyphenationLadderCount;

	private KeepTogether m_keepTogether;

	private float m_lineHeight;

	private uint m_orphans;

	private float m_padding;

	private float m_paddingBottom;

	private float m_paddingTop;

	private float m_paddingLeft;

	private float m_paddingRight;

	private TextAlign m_textAlign;

	private TextAlignLast m_textAlignLast;

	private float m_textIndent;

	private uint m_windows;

	private uint m_backgroundTransparancy;

	private float m_borderLineWidth;

	private float m_borderLineWidthTop;

	private float m_borderLineWidthBottom;

	private float m_borderLineWidthLeft;

	private float m_borderLineWidthRight;

	private bool m_fontIndependentLineSpacing;

	private bool m_justifySingleWord;

	private double m_lineHeightAtLeast;

	private double m_lineSpacing;

	private bool m_registerTrue;

	private bool m_snapToLayoutGrid;

	private uint m_tapToDistance;

	private uint m_lineNumber;

	private bool m_numberLines;

	private bool m_lineBreak;

	private PunctuationWrap m_punctuationWrap;

	private TextAutoSpace m_textAutoSpace;

	private VerticalAlign? m_verticalAlign;

	private bool m_isTab;

	private double m_afterSpacing;

	private double m_beforeSpacing;

	private double m_leftIndent;

	private double m_rightIndent;

	private List<TabStops> m_tabStops;

	internal int m_styleFlag1;

	internal byte m_styleFlag2;

	private const int VerticalAlignKey = 0;

	private const int TextAutoSpaceKey = 1;

	private const int PunctuationWrapKey = 2;

	private const int LineBreakKey = 3;

	private const int NumberLinesKey = 4;

	private const int LineNumberKey = 5;

	private const int TapToDistanceKey = 6;

	private const int SnapToLayoutGridKey = 7;

	private const int RegisterTrueKey = 8;

	private const int LineSpacingKey = 9;

	private const int LineHeightAtLeastKey = 10;

	private const int JustifySingleWordKey = 11;

	private const int BorderLineWidthRightKey = 12;

	private const int BorderLineWidthLeftKey = 13;

	private const int BorderLineWidthBottomKey = 14;

	private const int BorderLineWidthTopKey = 15;

	private const int BorderLineWidthKey = 16;

	private const int BackgroundTransparancyKey = 17;

	private const int WindowsKey = 18;

	private const int TextIndentKey = 19;

	private const int TextAlignLastKey = 20;

	private const int TextAlignKey = 21;

	private const int PaddingRightKey = 22;

	private const int PaddingLeftKey = 23;

	private const int PaddingTopKey = 24;

	private const int PaddingBottomKey = 25;

	private const int PaddingKey = 26;

	private const int OrphansKey = 27;

	private const int LineHeightKey = 28;

	private const int KeepTogetherKey = 29;

	private const int HyphenationLadderCountKey = 30;

	private const int HyphenationKeepKey = 31;

	private const byte BeforeSpacingKey = 0;

	private const byte AfterSpacingKey = 1;

	private const byte LeftIndentKey = 2;

	private const byte RightIndentKey = 3;

	internal List<TabStops> TabStops
	{
		get
		{
			if (m_tabStops == null)
			{
				m_tabStops = new List<TabStops>();
			}
			return m_tabStops;
		}
		set
		{
			m_tabStops = value;
		}
	}

	internal VerticalAlign? VerticalAlign
	{
		get
		{
			return m_verticalAlign;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFFFEu) | 1);
			m_verticalAlign = value;
		}
	}

	internal TextAutoSpace TextAutoSpace
	{
		get
		{
			return m_textAutoSpace;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFFFDu) | 2);
			m_textAutoSpace = value;
		}
	}

	internal PunctuationWrap PunctuationWrap
	{
		get
		{
			return m_punctuationWrap;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFFFBu) | 4);
			m_punctuationWrap = value;
		}
	}

	internal bool LineBreak
	{
		get
		{
			return m_lineBreak;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFFF7u) | 8);
			m_lineBreak = value;
		}
	}

	internal bool NumberLines
	{
		get
		{
			return m_numberLines;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFFEFu) | 0x10);
			m_numberLines = value;
		}
	}

	internal uint LineNumber
	{
		get
		{
			return m_lineNumber;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFFDFu) | 0x20);
			m_lineNumber = value;
		}
	}

	internal uint TapToDistance
	{
		get
		{
			return m_tapToDistance;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFFBFu) | 0x40);
			m_tapToDistance = value;
		}
	}

	internal bool SnapToLayoutGrid
	{
		get
		{
			return m_snapToLayoutGrid;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFF7Fu) | 0x80);
			m_snapToLayoutGrid = value;
		}
	}

	internal bool RegisterTrue
	{
		get
		{
			return m_registerTrue;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFEFFu) | 0x100);
			m_registerTrue = value;
		}
	}

	internal double LineSpacing
	{
		get
		{
			return m_lineSpacing;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFDFFu) | 0x200);
			m_lineSpacing = value;
		}
	}

	internal double LineHeightAtLeast
	{
		get
		{
			return m_lineHeightAtLeast;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFFBFFu) | 0x400);
			m_lineHeightAtLeast = value;
		}
	}

	internal bool JustifySingleWord
	{
		get
		{
			return m_justifySingleWord;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFF7FFu) | 0x800);
			m_justifySingleWord = value;
		}
	}

	internal float BorderLineWidthRight
	{
		get
		{
			return m_borderLineWidthRight;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFEFFFu) | 0x1000);
			m_borderLineWidthRight = value;
		}
	}

	internal float BorderLineWidthLeft
	{
		get
		{
			return m_borderLineWidthLeft;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFDFFFu) | 0x2000);
			m_borderLineWidthLeft = value;
		}
	}

	internal float BorderLineWidthBottom
	{
		get
		{
			return m_borderLineWidthBottom;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFFBFFFu) | 0x4000);
			m_borderLineWidthBottom = value;
		}
	}

	internal float BorderLineWidthTop
	{
		get
		{
			return m_borderLineWidthTop;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFF7FFFu) | 0x8000);
			m_borderLineWidthTop = value;
		}
	}

	internal float BorderLineWidth
	{
		get
		{
			return m_borderLineWidth;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFEFFFFu) | 0x10000);
			m_borderLineWidth = value;
		}
	}

	internal uint BackgroundTransparancy
	{
		get
		{
			return m_backgroundTransparancy;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFDFFFFu) | 0x20000);
			m_backgroundTransparancy = value;
		}
	}

	internal uint Windows
	{
		get
		{
			return m_windows;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFFBFFFFu) | 0x40000);
			m_windows = value;
		}
	}

	internal float TextIndent
	{
		get
		{
			return m_textIndent;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFF7FFFFu) | 0x80000);
			m_textIndent = value;
		}
	}

	internal TextAlignLast TextAlignLast
	{
		get
		{
			return m_textAlignLast;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFEFFFFFu) | 0x100000);
			m_textAlignLast = value;
		}
	}

	internal TextAlign TextAlign
	{
		get
		{
			return m_textAlign;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFDFFFFFu) | 0x200000);
			m_textAlign = value;
		}
	}

	internal float PaddingRight
	{
		get
		{
			return m_paddingRight;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFFBFFFFFu) | 0x400000);
			m_paddingRight = value;
		}
	}

	internal float PaddingLeft
	{
		get
		{
			return m_paddingLeft;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFF7FFFFFu) | 0x800000);
			m_paddingLeft = value;
		}
	}

	internal float PaddingTop
	{
		get
		{
			return m_paddingTop;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFEFFFFFFu) | 0x1000000);
			m_paddingTop = value;
		}
	}

	internal float PaddingBottom
	{
		get
		{
			return m_paddingBottom;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFDFFFFFFu) | 0x2000000);
			m_paddingBottom = value;
		}
	}

	internal float Padding
	{
		get
		{
			return m_padding;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xFBFFFFFFu) | 0x4000000);
			m_padding = value;
		}
	}

	internal uint Orphans
	{
		get
		{
			return m_orphans;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xF7FFFFFFu) | 0x8000000);
			m_orphans = value;
		}
	}

	internal float LineHeight
	{
		get
		{
			return m_lineHeight;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xEFFFFFFFu) | 0x10000000);
			m_lineHeight = value;
		}
	}

	internal KeepTogether KeepTogether
	{
		get
		{
			return m_keepTogether;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xDFFFFFFFu) | 0x20000000);
			m_keepTogether = value;
		}
	}

	internal int HyphenationLadderCount
	{
		get
		{
			return m_hyphenationLadderCount;
		}
		set
		{
			m_styleFlag1 = (int)((m_styleFlag1 & 0xBFFFFFFFu) | 0x40000000);
			m_hyphenationLadderCount = value;
		}
	}

	internal HyphenationKeep HyphenationKeep
	{
		get
		{
			return m_hyphenationKeep;
		}
		set
		{
			m_styleFlag1 = (m_styleFlag1 & 0x7FFFFFFF) | int.MinValue;
			m_hyphenationKeep = value;
		}
	}

	internal bool FontIndependentLineSpacing
	{
		get
		{
			return m_fontIndependentLineSpacing;
		}
		set
		{
			m_fontIndependentLineSpacing = value;
		}
	}

	internal bool IsTab
	{
		get
		{
			return m_isTab;
		}
		set
		{
			m_isTab = value;
		}
	}

	internal double BeforeSpacing
	{
		get
		{
			return m_beforeSpacing;
		}
		set
		{
			m_styleFlag2 = (byte)((m_styleFlag2 & 0xFEu) | 1u);
			m_beforeSpacing = value;
		}
	}

	internal double AfterSpacing
	{
		get
		{
			return m_afterSpacing;
		}
		set
		{
			m_styleFlag2 = (byte)((m_styleFlag2 & 0xFDu) | 2u);
			m_afterSpacing = value;
		}
	}

	internal double LeftIndent
	{
		get
		{
			return m_leftIndent;
		}
		set
		{
			m_styleFlag2 = (byte)((m_styleFlag2 & 0xFBu) | 4u);
			m_leftIndent = value;
		}
	}

	internal double RightIndent
	{
		get
		{
			return m_rightIndent;
		}
		set
		{
			m_styleFlag2 = (byte)((m_styleFlag2 & 0xF7u) | 8u);
			m_rightIndent = value;
		}
	}

	internal bool HasKey(int propertyKey, int flagName)
	{
		return (flagName & (int)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ODFParagraphProperties oDFParagraphProperties))
		{
			return false;
		}
		bool result = false;
		if (HasKey(0, m_CommonstyleFlags) && oDFParagraphProperties.HasKey(0, m_CommonstyleFlags) && base.BackgroundColor != null)
		{
			result = base.BackgroundColor.Equals(oDFParagraphProperties.BackgroundColor);
		}
		if (HasKey(16, m_styleFlag1))
		{
			result = BorderLineWidth.Equals(oDFParagraphProperties.BorderLineWidth);
		}
		if (HasKey(14, m_styleFlag1))
		{
			result = BorderLineWidthBottom.Equals(oDFParagraphProperties.BorderLineWidthBottom);
		}
		if (HasKey(13, m_styleFlag1))
		{
			result = BorderLineWidthLeft.Equals(oDFParagraphProperties.BorderLineWidthLeft);
		}
		if (HasKey(12, m_styleFlag1))
		{
			result = BorderLineWidthRight.Equals(oDFParagraphProperties.BorderLineWidthRight);
		}
		if (HasKey(15, m_styleFlag1))
		{
			result = BorderLineWidthTop.Equals(oDFParagraphProperties.BorderLineWidthTop);
		}
		if (HasKey(2, m_CommonstyleFlags))
		{
			result = base.AfterBreak.Equals(oDFParagraphProperties.AfterBreak);
		}
		if (HasKey(17, m_styleFlag1))
		{
			result = BackgroundTransparancy.Equals(oDFParagraphProperties.BackgroundTransparancy);
		}
		if (HasKey(1, m_CommonstyleFlags))
		{
			result = base.BeforeBreak.Equals(oDFParagraphProperties.BeforeBreak);
		}
		if (FontIndependentLineSpacing)
		{
			result = FontIndependentLineSpacing.Equals(oDFParagraphProperties.FontIndependentLineSpacing);
		}
		if (HasKey(31, m_styleFlag1))
		{
			result = HyphenationKeep.Equals(oDFParagraphProperties.HyphenationKeep);
		}
		if (HasKey(30, m_styleFlag1))
		{
			result = HyphenationLadderCount.Equals(oDFParagraphProperties.HyphenationLadderCount);
		}
		if (HasKey(11, m_styleFlag1))
		{
			result = JustifySingleWord.Equals(oDFParagraphProperties.JustifySingleWord);
		}
		if (HasKey(29, m_styleFlag1))
		{
			result = KeepTogether.Equals(oDFParagraphProperties.KeepTogether);
		}
		if (HasKey(3, m_CommonstyleFlags))
		{
			result = base.KeepWithNext.Equals(oDFParagraphProperties.KeepWithNext);
		}
		if (HasKey(3, m_styleFlag1))
		{
			result = LineBreak.Equals(oDFParagraphProperties.LineBreak);
		}
		if (HasKey(28, m_styleFlag1))
		{
			result = LineHeight.Equals(oDFParagraphProperties.LineHeight);
		}
		if (HasKey(10, m_styleFlag1))
		{
			result = LineHeightAtLeast.Equals(oDFParagraphProperties.LineHeightAtLeast);
		}
		if (HasKey(5, m_styleFlag1))
		{
			result = LineNumber.Equals(oDFParagraphProperties.LineNumber);
		}
		if (HasKey(9, m_styleFlag1))
		{
			result = LineSpacing.Equals(oDFParagraphProperties.LineSpacing);
		}
		if (HasKey(3, m_marginFlag))
		{
			result = base.MarginBottom.Equals(oDFParagraphProperties.MarginBottom);
		}
		if (HasKey(0, m_marginFlag))
		{
			result = base.MarginLeft.Equals(oDFParagraphProperties.MarginLeft);
		}
		if (HasKey(1, m_marginFlag))
		{
			result = base.MarginRight.Equals(oDFParagraphProperties.MarginRight);
		}
		if (HasKey(2, m_marginFlag))
		{
			result = base.MarginTop.Equals(oDFParagraphProperties.MarginTop);
		}
		if (HasKey(4, m_styleFlag1))
		{
			result = NumberLines.Equals(oDFParagraphProperties.NumberLines);
		}
		if (HasKey(27, m_styleFlag1))
		{
			result = Orphans.Equals(oDFParagraphProperties.Orphans);
		}
		if (HasKey(26, m_styleFlag1))
		{
			result = Padding.Equals(oDFParagraphProperties.Padding);
		}
		if (HasKey(25, m_styleFlag1))
		{
			result = PaddingBottom.Equals(oDFParagraphProperties.PaddingBottom);
		}
		if (HasKey(23, m_styleFlag1))
		{
			result = PaddingLeft.Equals(oDFParagraphProperties.PaddingLeft);
		}
		if (HasKey(25, m_styleFlag1))
		{
			result = PaddingRight.Equals(oDFParagraphProperties.PaddingRight);
		}
		if (HasKey(24, m_styleFlag1))
		{
			result = PaddingTop.Equals(oDFParagraphProperties.PaddingTop);
		}
		if (HasKey(5, m_CommonstyleFlags))
		{
			result = base.PageNumber.Equals(oDFParagraphProperties.PageNumber);
		}
		if (HasKey(2, m_styleFlag1))
		{
			result = PunctuationWrap.Equals(oDFParagraphProperties.PunctuationWrap);
		}
		if (HasKey(8, m_styleFlag1))
		{
			result = RegisterTrue.Equals(oDFParagraphProperties.RegisterTrue);
		}
		if (HasKey(4, m_CommonstyleFlags))
		{
			result = base.ShadowType.Equals(oDFParagraphProperties.ShadowType);
		}
		if (HasKey(7, m_styleFlag1))
		{
			result = SnapToLayoutGrid.Equals(oDFParagraphProperties.SnapToLayoutGrid);
		}
		if (HasKey(6, m_styleFlag1))
		{
			result = TapToDistance.Equals(oDFParagraphProperties.TapToDistance);
		}
		if (HasKey(21, m_styleFlag1))
		{
			result = TextAlign.Equals(oDFParagraphProperties.TextAlign);
		}
		if (HasKey(20, m_styleFlag1))
		{
			result = TextAlignLast.Equals(oDFParagraphProperties.TextAlignLast);
		}
		if (HasKey(1, m_styleFlag1))
		{
			result = TextAutoSpace.Equals(oDFParagraphProperties.TextAutoSpace);
		}
		if (HasKey(19, m_styleFlag1))
		{
			result = TextIndent.Equals(oDFParagraphProperties.TextIndent);
		}
		if (HasKey(0, m_styleFlag1))
		{
			result = VerticalAlign.Equals(oDFParagraphProperties.VerticalAlign);
		}
		if (HasKey(18, m_styleFlag1))
		{
			result = Windows.Equals(oDFParagraphProperties.Windows);
		}
		if (HasKey(0, m_CommonstyleFlags))
		{
			result = base.WritingMode.Equals(oDFParagraphProperties.WritingMode);
		}
		if (HasKey(1, m_styleFlag2))
		{
			result = AfterSpacing.Equals(oDFParagraphProperties.AfterSpacing);
		}
		if (HasKey(0, m_styleFlag2))
		{
			result = BeforeSpacing.Equals(oDFParagraphProperties.BeforeSpacing);
		}
		if (HasKey(2, m_styleFlag2))
		{
			result = LeftIndent.Equals(oDFParagraphProperties.LeftIndent);
		}
		if (HasKey(3, m_styleFlag2))
		{
			result = RightIndent.Equals(oDFParagraphProperties.RightIndent);
		}
		if (HasKey(0, m_marginFlag))
		{
			result = base.MarginLeft.Equals(oDFParagraphProperties.MarginLeft);
		}
		if (HasKey(1, m_marginFlag))
		{
			result = base.MarginRight.Equals(oDFParagraphProperties.MarginRight);
		}
		if (HasKey(2, m_marginFlag))
		{
			result = base.MarginTop.Equals(oDFParagraphProperties.MarginTop);
		}
		if (HasKey(3, m_marginFlag))
		{
			result = base.MarginBottom.Equals(oDFParagraphProperties.MarginBottom);
		}
		return result;
	}

	internal void Close()
	{
		if (m_tabStops != null)
		{
			m_tabStops.Clear();
			m_tabStops = null;
		}
	}
}
