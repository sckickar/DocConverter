using System.Collections.Generic;
using DocGen.DocIO.DLS;

namespace DocGen.Layouting;

internal class ParagraphLayoutInfo : LayoutInfo, ILayoutSpacingsInfo
{
	private byte m_bFlags = 10;

	private float m_topMargin;

	private float m_bottomMargin;

	private float m_topPadding;

	private float m_bottomPadding;

	private int m_levelNumber = -1;

	private HAlignment m_justification;

	private float m_firstLineIndent;

	private float m_listTab;

	private float m_yPosition;

	private float m_pargaraphOriginalYPosition = float.MinValue;

	private float m_listTabWidth;

	private List<float> m_listYPositions;

	private string m_listValue = string.Empty;

	private WCharacterFormat m_characterFormat;

	private ListNumberAlignment m_listAlignment;

	private TabsLayoutInfo.LayoutTab m_listTabStop;

	private float m_xPosition;

	private ListType m_listType;

	private Spacings m_paddings;

	private Spacings m_margins;

	private SyncFont m_listfont;

	private bool m_skipTopBorder;

	private bool m_skipBottomBorder;

	private bool m_skipLeftBorder;

	private bool m_skipRightBorder;

	private bool m_skipHorizontalBorder;

	internal bool SkipTopBorder
	{
		get
		{
			return m_skipTopBorder;
		}
		set
		{
			m_skipTopBorder = value;
		}
	}

	internal bool SkipBottomBorder
	{
		get
		{
			return m_skipBottomBorder;
		}
		set
		{
			m_skipBottomBorder = value;
		}
	}

	internal bool SkipLeftBorder
	{
		get
		{
			return m_skipLeftBorder;
		}
		set
		{
			m_skipLeftBorder = value;
		}
	}

	internal bool SkipRightBorder
	{
		get
		{
			return m_skipRightBorder;
		}
		set
		{
			m_skipRightBorder = value;
		}
	}

	internal bool SkipHorizonatalBorder
	{
		get
		{
			return m_skipHorizontalBorder;
		}
		set
		{
			m_skipHorizontalBorder = value;
		}
	}

	public bool IsPageBreak
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

	public int LevelNumber
	{
		get
		{
			return m_levelNumber;
		}
		set
		{
			m_levelNumber = value;
		}
	}

	public HAlignment Justification
	{
		get
		{
			return m_justification;
		}
		set
		{
			m_justification = value;
		}
	}

	public float FirstLineIndent
	{
		get
		{
			return m_firstLineIndent;
		}
		set
		{
			m_firstLineIndent = value;
		}
	}

	public bool IsKeepTogether
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

	public float ListTab
	{
		get
		{
			return m_listTab;
		}
		set
		{
			m_listTab = value;
		}
	}

	internal float ListTabWidth
	{
		get
		{
			return m_listTabWidth;
		}
		set
		{
			m_listTabWidth = value;
		}
	}

	internal float YPosition
	{
		get
		{
			return m_yPosition;
		}
		set
		{
			m_yPosition = value;
		}
	}

	internal float PargaraphOriginalYPosition
	{
		get
		{
			return m_pargaraphOriginalYPosition;
		}
		set
		{
			m_pargaraphOriginalYPosition = value;
		}
	}

	internal List<float> ListYPositions => m_listYPositions ?? (m_listYPositions = new List<float>());

	public string ListValue
	{
		get
		{
			return m_listValue;
		}
		set
		{
			m_listValue = value;
		}
	}

	public ListType CurrentListType
	{
		get
		{
			return m_listType;
		}
		set
		{
			m_listType = value;
		}
	}

	public WCharacterFormat CharacterFormat
	{
		get
		{
			return m_characterFormat;
		}
		set
		{
			m_characterFormat = value;
		}
	}

	public ListNumberAlignment ListAlignment
	{
		get
		{
			return m_listAlignment;
		}
		set
		{
			m_listAlignment = value;
		}
	}

	public TabsLayoutInfo.LayoutTab ListTabStop
	{
		get
		{
			return m_listTabStop;
		}
		set
		{
			m_listTabStop = value;
		}
	}

	public bool IsFirstLine
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

	public float TopPadding
	{
		get
		{
			return m_topPadding;
		}
		set
		{
			m_topPadding = value;
		}
	}

	public float BottomPadding
	{
		get
		{
			return m_bottomPadding;
		}
		set
		{
			m_bottomPadding = value;
		}
	}

	public float TopMargin
	{
		get
		{
			return m_topMargin;
		}
		set
		{
			m_topMargin = value;
		}
	}

	public float BottomMargin
	{
		get
		{
			return m_bottomMargin;
		}
		set
		{
			m_bottomMargin = value;
		}
	}

	public bool IsNotFitted
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

	internal bool IsXPositionReUpdate
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

	internal float XPosition
	{
		get
		{
			return m_xPosition;
		}
		set
		{
			m_xPosition = value;
		}
	}

	internal SyncFont ListFont
	{
		get
		{
			return m_listfont;
		}
		set
		{
			m_listfont = value;
		}
	}

	internal bool IsSectionEndMark
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

	public Spacings Paddings
	{
		get
		{
			if (m_paddings == null)
			{
				m_paddings = new Spacings();
			}
			return m_paddings;
		}
	}

	public Spacings Margins
	{
		get
		{
			if (m_margins == null)
			{
				m_margins = new Spacings();
			}
			return m_margins;
		}
	}

	public ParagraphLayoutInfo(ChildrenLayoutDirection childLayoutDirection)
		: base(childLayoutDirection)
	{
	}

	public ParagraphLayoutInfo(ChildrenLayoutDirection childLayoutDirection, bool isPageBreak)
		: this(childLayoutDirection)
	{
		IsPageBreak = isPageBreak;
	}

	public ParagraphLayoutInfo()
	{
	}

	internal void InitLayoutInfo()
	{
		if (m_listYPositions != null)
		{
			m_listYPositions.Clear();
			m_listYPositions = null;
		}
	}
}
