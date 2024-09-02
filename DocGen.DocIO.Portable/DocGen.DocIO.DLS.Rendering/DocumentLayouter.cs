using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Drawing.DocIOHelper;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS.Rendering;

internal class DocumentLayouter : ILayoutProcessHandler
{
	internal class HeaderFooterLPHandler : ILayoutProcessHandler
	{
		private DocumentLayouter m_dl;

		private bool m_bFooter;

		public HeaderFooterLPHandler(DocumentLayouter dl, bool bFooter)
		{
			m_dl = dl;
			m_bFooter = bFooter;
		}

		public bool GetNextArea(out RectangleF area, ref int columnIndex, ref bool isContinuousSection, bool isSplittedWidget, ref float topMargin, bool isFromDynmicLayout, ref IWidgetContainer curWidget)
		{
			if (!m_bFooter)
			{
				m_dl.HeaderGetNextArea(out area);
				area = ModifyHeaderFooterArea(curWidget, area);
				return !area.Equals(RectangleF.Empty);
			}
			m_dl.FooterGetNextArea(out area);
			area = ModifyHeaderFooterArea(curWidget, area);
			return !area.Equals(RectangleF.Empty);
		}

		private RectangleF ModifyHeaderFooterArea(IWidgetContainer curWidget, RectangleF area)
		{
			if (!area.Equals(RectangleF.Empty) && m_dl.CurrentSection.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
			{
				string headerFooterType = m_dl.GetHeaderFooterType(curWidget);
				if (m_dl.HeaderFooterNeedsRelayout.ContainsKey(headerFooterType) && m_dl.HeaderFooterNeedsRelayout[headerFooterType])
				{
					area.Height = m_dl.GetWord2013HeaderFooterHeight() / 2f;
				}
				else if (!m_dl.HeaderFooterNeedsRelayout.ContainsKey(headerFooterType))
				{
					m_dl.HeaderFooterNeedsRelayout.Add(headerFooterType, value: false);
				}
			}
			return area;
		}

		public void PushLayoutedWidget(LayoutedWidget ltWidget, RectangleF layoutArea, bool isNeedToRestartFootnote, bool isNeedToRestartEndnote, LayoutState state, bool isNeedToFindInterSectingPoint, bool isContinuousSection)
		{
			if (!m_bFooter)
			{
				m_dl.HeaderPushLayoutedWidget(ltWidget);
			}
			else
			{
				m_dl.FooterPushLayoutedWidget(ltWidget);
			}
		}

		public bool HandleSplittedWidget(SplitWidgetContainer stWidgetContainer, LayoutState state, LayoutedWidget ltWidget, ref bool isLayoutedWidgetNeedToPushed)
		{
			return false;
		}

		public void HandleLayoutedWidget(LayoutedWidget ltWidget)
		{
		}
	}

	internal class BookmarkHyperlink
	{
		private RectangleF m_sourceBounds;

		private RectangleF m_targetBounds;

		private int m_targetPageNumber;

		private int m_sourcePageNumber;

		private string m_hyperlinkValue;

		private int m_tocLevel;

		private string m_tocText;

		private bool m_isTargetNull;

		private Hyperlink m_hyperlink;

		public RectangleF SourceBounds
		{
			get
			{
				return m_sourceBounds;
			}
			set
			{
				m_sourceBounds = value;
			}
		}

		public RectangleF TargetBounds
		{
			get
			{
				return m_targetBounds;
			}
			set
			{
				m_targetBounds = value;
			}
		}

		public int TargetPageNumber
		{
			get
			{
				return m_targetPageNumber;
			}
			set
			{
				m_targetPageNumber = value;
			}
		}

		public int SourcePageNumber
		{
			get
			{
				return m_sourcePageNumber;
			}
			set
			{
				m_sourcePageNumber = value;
			}
		}

		public string HyperlinkValue
		{
			get
			{
				return m_hyperlinkValue;
			}
			set
			{
				m_hyperlinkValue = value;
			}
		}

		public Hyperlink Hyperlink
		{
			get
			{
				return m_hyperlink;
			}
			set
			{
				m_hyperlink = value;
			}
		}

		public int TOCLevel
		{
			get
			{
				return m_tocLevel;
			}
			set
			{
				m_tocLevel = value;
			}
		}

		public string TOCText
		{
			get
			{
				return m_tocText;
			}
			set
			{
				m_tocText = value;
			}
		}

		public bool IsTargetNull
		{
			get
			{
				return m_isTargetNull;
			}
			set
			{
				m_isTargetNull = value;
			}
		}

		public BookmarkHyperlink()
		{
			SourceBounds = default(RectangleF);
			TargetBounds = default(RectangleF);
			TargetPageNumber = 0;
			SourcePageNumber = 0;
			TOCLevel = 0;
			TOCText = string.Empty;
			HyperlinkValue = string.Empty;
		}
	}

	private PageCollection m_pages = new PageCollection();

	private Page m_currPage;

	private IWidgetContainer m_docWidget;

	private IWSection m_currSection;

	private IWSection m_prevSection;

	private HeaderFooterLPHandler m_headerLPHandler;

	private HeaderFooterLPHandler m_footerLPHandler;

	private int m_columnIndex;

	private float m_columnsWidth;

	private int m_nextPageIndex;

	private bool m_bFirstPageForSection = true;

	private byte m_bFlag;

	private byte m_bitFlag;

	private bool m_preserveFormFields;

	private bool m_bisNeedToRestartFootnote = true;

	private bool m_bisNeedToRestartEndnote = true;

	private bool m_bDirty;

	private float m_totalColumnWidth;

	private WordToPDFResult m_pageResult;

	internal int m_footnoteCount;

	private int m_endnoteCount;

	private float m_usedHeight;

	private float m_clientHeight;

	private float m_totalHeight;

	private float m_sectionFixedHeight;

	private float m_pageTop;

	private int m_sectionIndex = -1;

	private int m_sectionPagesCount;

	private int lineHeigthCount;

	private float m_firstParaHeight;

	private float m_footnoteHeight;

	private bool m_isFirstPage = true;

	private bool m_sectionNewPage;

	private bool m_createNewPage;

	private bool m_isContinuousSectionLayouted;

	private List<float> m_lineHeights = new List<float>();

	private List<float> m_columnHeight = new List<float>();

	private List<bool> m_columnHasBreakItem = new List<bool>();

	private List<float> m_prevColumnsWidth = new List<float>();

	private List<FloatingItem> m_prevFloatingItems;

	private List<float> m_absolutePositionedTableHeights;

	private Dictionary<int, List<string>> m_tocLevels;

	private List<ParagraphItem> m_tocParaItems;

	private Dictionary<Entity, int> m_tocEntryPageNumbers;

	private Dictionary<Entity, int> m_bkStartPageNumbers;

	private Dictionary<string, bool> m_headerFooterNeedsRelayout;

	private bool m_useTCFields;

	private Entity m_lastTocEntity;

	[ThreadStatic]
	internal static bool m_UpdatingPageFields;

	internal bool m_isNeedtoAdjustFooter;

	internal List<FloatingItem> m_FloatingItems;

	private List<FloatingItem> m_WrapFloatingItems = new List<FloatingItem>();

	internal WParagraph m_dynamicParagraph;

	internal WTable m_dynamicTable;

	internal List<Entity> m_notFittedfloatingItems = new List<Entity>();

	internal LayoutedWidget m_maintainltWidget;

	internal int[] m_interSectingPoint;

	internal IWidgetContainer m_pageEndWidget;

	internal ExportBookmarkType m_exportBookmarkType;

	private List<LayoutedWidget> m_editableFormFieldinEMF = new List<LayoutedWidget>();

	private int[] m_sectionNumPages;

	private float m_removedWidgetsHeight;

	internal List<WParagraph> m_hiddenParaCollection = new List<WParagraph>();

	[ThreadStatic]
	private static byte m_bFlags;

	[ThreadStatic]
	internal static DrawingContext m_dc;

	[ThreadStatic]
	internal static int PageNumber;

	[ThreadStatic]
	private static List<Dictionary<string, BookmarkHyperlink>> m_bookmarkHyperlinks = null;

	[ThreadStatic]
	private static List<BookmarkPosition> m_bookmarks = null;

	[ThreadStatic]
	private static List<EquationField> m_equationFields = null;

	[ThreadStatic]
	internal static int m_footnoteIDRestartEachPage = 1;

	[ThreadStatic]
	internal static int m_endnoteIDRestartEachPage = 1;

	[ThreadStatic]
	private static bool m_isPageEnd;

	[ThreadStatic]
	private static bool m_isEndUpdateTOC;

	[ThreadStatic]
	internal static int m_footnoteIDRestartEachSection = 1;

	[ThreadStatic]
	internal static int m_footnoteId;

	[ThreadStatic]
	internal static int m_endnoteId;

	internal static DrawingContext DrawingContext
	{
		get
		{
			if (m_dc == null)
			{
				m_dc = new DrawingContext();
			}
			return m_dc;
		}
	}

	internal static bool IsFirstLayouting
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

	internal static bool IsUpdatingTOC
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

	internal static bool IsLayoutingHeaderFooter
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

	internal static bool IsEndPage
	{
		get
		{
			return m_isPageEnd;
		}
		set
		{
			m_isPageEnd = value;
		}
	}

	internal static List<Dictionary<string, BookmarkHyperlink>> BookmarkHyperlinks
	{
		get
		{
			if (m_bookmarkHyperlinks == null)
			{
				m_bookmarkHyperlinks = new List<Dictionary<string, BookmarkHyperlink>>();
			}
			return m_bookmarkHyperlinks;
		}
	}

	internal static List<BookmarkPosition> Bookmarks
	{
		get
		{
			if (m_bookmarks == null)
			{
				m_bookmarks = new List<BookmarkPosition>();
			}
			return m_bookmarks;
		}
	}

	internal bool IsForceFitLayout
	{
		get
		{
			return (m_bitFlag & 1) != 0;
		}
		set
		{
			m_bitFlag = (byte)((m_bitFlag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public bool PreserveFormField
	{
		get
		{
			return m_preserveFormFields;
		}
		set
		{
			m_preserveFormFields = value;
		}
	}

	internal bool PreserveOleEquationAsBitmap
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

	internal List<LayoutedWidget> EditableFormFieldinEMF
	{
		get
		{
			return m_editableFormFieldinEMF;
		}
		set
		{
			m_editableFormFieldinEMF = value;
		}
	}

	public PageCollection Pages => m_pages;

	internal Entity LastTocEntity
	{
		get
		{
			return m_lastTocEntity;
		}
		set
		{
			m_lastTocEntity = value;
		}
	}

	internal static bool IsEndUpdateTOC
	{
		get
		{
			return m_isEndUpdateTOC;
		}
		set
		{
			m_isEndUpdateTOC = value;
		}
	}

	internal ExportBookmarkType ExportBookmarks
	{
		get
		{
			return m_exportBookmarkType;
		}
		set
		{
			m_exportBookmarkType = value;
		}
	}

	internal Page CurrentPage => m_currPage;

	internal IWSection CurrentSection => m_currSection;

	protected Column CurrentColumn
	{
		get
		{
			if (CurrentSection.Columns.Count != 0)
			{
				return CurrentSection.Columns[m_columnIndex];
			}
			return null;
		}
	}

	public WordToPDFResult PageResult => m_pageResult;

	protected bool IsEvenPage => m_nextPageIndex % 2 == 0;

	internal bool EnablePdfConformanceLevel
	{
		get
		{
			return (m_bFlag & 1) != 0;
		}
		set
		{
			m_bFlag = (byte)((m_bFlag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal float RemovedWidgetsHeight
	{
		get
		{
			if (!m_isContinuousSectionLayouted)
			{
				return 0f;
			}
			return m_removedWidgetsHeight;
		}
		set
		{
			m_removedWidgetsHeight = value;
		}
	}

	internal List<ParagraphItem> tocParaItems
	{
		get
		{
			if (m_tocParaItems == null)
			{
				m_tocParaItems = new List<ParagraphItem>();
			}
			return m_tocParaItems;
		}
		set
		{
			m_tocParaItems = value;
		}
	}

	internal List<FloatingItem> FloatingItems
	{
		get
		{
			if (m_FloatingItems == null)
			{
				m_FloatingItems = new List<FloatingItem>();
			}
			return m_FloatingItems;
		}
		set
		{
			m_FloatingItems = value;
		}
	}

	internal List<FloatingItem> WrapFloatingItems => m_WrapFloatingItems;

	internal static List<EquationField> EquationFields
	{
		get
		{
			if (m_equationFields == null)
			{
				m_equationFields = new List<EquationField>();
			}
			return m_equationFields;
		}
	}

	internal LayoutedWidget MaintainltWidget
	{
		get
		{
			if (m_maintainltWidget == null)
			{
				m_maintainltWidget = new LayoutedWidget();
			}
			return m_maintainltWidget;
		}
		set
		{
			m_maintainltWidget = value;
		}
	}

	internal int[] InterSectingPoint
	{
		get
		{
			if (m_interSectingPoint == null)
			{
				m_interSectingPoint = new int[4] { -2147483648, -2147483648, -2147483648, -2147483648 };
			}
			return m_interSectingPoint;
		}
		set
		{
			m_interSectingPoint = value;
		}
	}

	internal bool IsCreateNewPage
	{
		set
		{
			m_createNewPage = value;
		}
	}

	internal Dictionary<Entity, int> TOCEntryPageNumbers
	{
		get
		{
			if (m_tocEntryPageNumbers == null)
			{
				m_tocEntryPageNumbers = new Dictionary<Entity, int>();
			}
			return m_tocEntryPageNumbers;
		}
	}

	internal Dictionary<Entity, int> BookmarkStartPageNumbers
	{
		get
		{
			if (m_bkStartPageNumbers == null)
			{
				m_bkStartPageNumbers = new Dictionary<Entity, int>();
			}
			return m_bkStartPageNumbers;
		}
	}

	internal Dictionary<string, bool> HeaderFooterNeedsRelayout
	{
		get
		{
			if (m_prevSection != m_currSection)
			{
				m_headerFooterNeedsRelayout = new Dictionary<string, bool>();
				m_prevSection = m_currSection;
			}
			return m_headerFooterNeedsRelayout;
		}
	}

	internal int[] SectionNumPages
	{
		get
		{
			if (m_sectionNumPages == null)
			{
				if (m_docWidget is WordDocument)
				{
					m_sectionNumPages = new int[(m_docWidget as WordDocument).Sections.Count];
				}
				else
				{
					m_sectionNumPages = new int[1];
				}
			}
			return m_sectionNumPages;
		}
	}

	internal bool UseTCFields
	{
		get
		{
			return m_useTCFields;
		}
		set
		{
			m_useTCFields = value;
		}
	}

	public DocumentLayouter()
	{
		m_pageResult = new WordToPDFResult();
		m_headerLPHandler = new HeaderFooterLPHandler(this, bFooter: false);
		m_footerLPHandler = new HeaderFooterLPHandler(this, bFooter: true);
	}

	public PageCollection Layout(IWordDocument doc)
	{
		if (doc.Sections.Count < 1)
		{
			return null;
		}
		m_docWidget = doc as IWidgetContainer;
		if (m_docWidget == null)
		{
			throw new DLSException("Document can't support IWidgetContainer interface");
		}
		m_currSection = doc.Sections[0];
		IsFirstLayouting = true;
		if (!LayoutPages())
		{
			m_bFirstPageForSection = true;
			IsFirstLayouting = false;
			MaintainltWidget.ChildWidgets.Clear();
			int i = 0;
			for (int count = m_pages.Count; i < count; i++)
			{
				Page page = m_pages[i];
				m_currSection.Document.PageCount = count;
				page.UpdateFieldsNumPages(count);
				if (m_currSection.Document.HasCoverPage && m_currSection.Document.Sections[0].PageSetup.HasKey(19) && m_currSection.Document.Sections[0].PageSetup.PageStartingNumber == 0)
				{
					int numPages = count - 1;
					page.UpdateFieldsNumPages(numPages);
				}
				ResetTheLayoutedFootnotes(page.FootnoteWidgets, page.EndnoteWidgets);
			}
			m_currSection = doc.Sections[0];
			m_isFirstPage = true;
			LayoutPages();
			IsFirstLayouting = true;
		}
		if (doc is WordDocument && (doc as WordDocument).IsNeedToAddLineNumbers())
		{
			AddLineNumbers(doc);
		}
		return m_pages;
	}

	internal void ResetTheLayoutedFootnotes(LayoutedWidgetList footnoteWidgets, LayoutedWidgetList endnoteWidgets)
	{
		for (int i = 0; i < footnoteWidgets.Count; i++)
		{
			WTextBody wTextBody = ((footnoteWidgets[i].Widget is WTextBody) ? (footnoteWidgets[i].Widget as WTextBody) : ((footnoteWidgets[i].Widget is SplitWidgetContainer) ? ((footnoteWidgets[i].Widget as SplitWidgetContainer).RealWidgetContainer as WTextBody) : null));
			if (wTextBody != null && wTextBody.Owner is WFootnote)
			{
				(wTextBody.Owner as WFootnote).IsLayouted = false;
				(wTextBody.Owner as IWidget).LayoutInfo.IsSkip = false;
			}
		}
		for (int j = 0; j < endnoteWidgets.Count; j++)
		{
			WTextBody wTextBody2 = ((endnoteWidgets[j].Widget is WTextBody) ? (endnoteWidgets[j].Widget as WTextBody) : ((endnoteWidgets[j].Widget is SplitWidgetContainer) ? ((endnoteWidgets[j].Widget as SplitWidgetContainer).RealWidgetContainer as WTextBody) : null));
			if (wTextBody2 != null && wTextBody2.Owner is WFootnote)
			{
				(wTextBody2.Owner as WFootnote).IsLayouted = false;
			}
		}
	}

	private void AddLineNumbers(IWordDocument doc)
	{
		WPageSetup pageSetup = doc.Sections[0].PageSetup;
		bool flag = true;
		int num = ((pageSetup.LineNumberingStartValue <= 0) ? (pageSetup.LineNumberingStartValue + 1) : pageSetup.LineNumberingStartValue);
		IntializeGraphics();
		for (int i = 0; i < m_pages.Count; i++)
		{
			for (int j = 0; j < m_pages[i].PageWidgets.Count; j++)
			{
				LayoutedWidget layoutedWidget = m_pages[i].PageWidgets[j];
				if (layoutedWidget.Widget is HeaderFooter)
				{
					continue;
				}
				for (int k = 0; k < layoutedWidget.ChildWidgets.Count; k++)
				{
					LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[k];
					pageSetup = UpdateSectionPageSetup(layoutedWidget2.Widget);
					if (pageSetup.OwnerBase is WSection && !(pageSetup.OwnerBase as WSection).LineNumbersEnabled())
					{
						continue;
					}
					if (pageSetup.LineNumberingMode == LineNumberingMode.RestartSection || (flag && pageSetup.LineNumberingMode == LineNumberingMode.RestartPage))
					{
						num = ((pageSetup.LineNumberingStartValue <= 0) ? (pageSetup.LineNumberingStartValue + 1) : pageSetup.LineNumberingStartValue);
					}
					flag = false;
					float x = layoutedWidget2.Bounds.X;
					for (int l = 0; l < layoutedWidget2.ChildWidgets.Count; l++)
					{
						LayoutedWidget layoutedWidget3 = layoutedWidget2.ChildWidgets[l];
						WParagraph wParagraph = layoutedWidget3.Widget as WParagraph;
						if (wParagraph == null && layoutedWidget3.Widget is SplitWidgetContainer)
						{
							wParagraph = (layoutedWidget3.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
						}
						if (wParagraph == null)
						{
							continue;
						}
						for (int m = 0; m < layoutedWidget3.ChildWidgets.Count; m++)
						{
							if (layoutedWidget3.ChildWidgets[m].ChildWidgets.Count == 0)
							{
								continue;
							}
							if (num % pageSetup.LineNumberingStep == 0)
							{
								if ((!wParagraph.SectionEndMark || wParagraph.ChildEntities.Count != 0) && !wParagraph.ParagraphFormat.SuppressLineNumbers)
								{
									LayoutLineNumber(layoutedWidget3.ChildWidgets[m], pageSetup, num, x);
									num++;
								}
							}
							else
							{
								num++;
							}
						}
					}
				}
			}
			flag = true;
		}
		DrawingContext.DisposeGraphics();
	}

	private void LayoutLineNumber(LayoutedWidget ltLineWidget, WPageSetup pageSetup, int lineNumber, float xPosition)
	{
		WTextRange wTextRange = new WTextRange(pageSetup.Document);
		wTextRange.Text = lineNumber.ToString();
		WCharacterFormat numberingFormat = GetNumberingFormat(pageSetup.Document);
		if (numberingFormat != null)
		{
			wTextRange.ApplyCharacterFormat(numberingFormat);
		}
		double topLineSpace = 0.0;
		float ascentOfText = GetAscentOfText(ltLineWidget, ref topLineSpace);
		float ascent = DrawingContext.GetAscent(wTextRange.CharacterFormat.GetFontToRender(wTextRange.ScriptType), wTextRange.ScriptType);
		LayoutedWidget layoutedWidget = new LayoutedWidget(wTextRange);
		wTextRange.m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		(wTextRange.m_layoutInfo as LayoutInfo).IsLineNumberItem = true;
		wTextRange.m_layoutInfo.Font = new SyncFont(wTextRange.CharacterFormat.GetFontToRender(wTextRange.ScriptType));
		SizeF sizeF2 = (wTextRange.m_layoutInfo.Size = DrawingContext.MeasureTextRange(wTextRange, wTextRange.Text));
		SizeF sizeF3 = sizeF2;
		float y = ltLineWidget.Bounds.Y + ascentOfText - ascent + (float)topLineSpace;
		UpdateXPositionBasedOnFloatingItem(ltLineWidget, ref xPosition);
		layoutedWidget.Bounds = new RectangleF(xPosition - pageSetup.LineNumberingDistanceFromText - sizeF3.Width, y, sizeF3.Width, sizeF3.Height);
		layoutedWidget.Owner = ltLineWidget;
		ltLineWidget.ChildWidgets.Insert(0, layoutedWidget);
	}

	private void UpdateXPositionBasedOnFloatingItem(LayoutedWidget ltLineWidget, ref float xPosition)
	{
		if (ltLineWidget.IntersectingBounds.Count <= 0)
		{
			return;
		}
		float num = ((ltLineWidget.ChildWidgets.Count > 0) ? GetFirstInlineItemXPosition(ltLineWidget) : ltLineWidget.Bounds.X);
		foreach (RectangleF intersectingBound in ltLineWidget.IntersectingBounds)
		{
			if (Math.Round(intersectingBound.Right, 1) <= Math.Round(num, 2))
			{
				xPosition = intersectingBound.Right;
				break;
			}
		}
	}

	private float GetFirstInlineItemXPosition(LayoutedWidget lineWidget)
	{
		foreach (LayoutedWidget childWidget in lineWidget.ChildWidgets)
		{
			IWidget widget = childWidget.Widget;
			if (!(widget is BookmarkStart) && !(widget is BookmarkEnd) && !(widget is WFieldMark))
			{
				if (widget is SplitStringWidget)
				{
					return childWidget.Bounds.X;
				}
				if (widget is ParagraphItem && !(widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
				{
					return childWidget.Bounds.X;
				}
			}
		}
		return lineWidget.Bounds.X;
	}

	private float GetAscentOfText(LayoutedWidget paraLtWidget, ref double topLineSpace)
	{
		WParagraph wParagraph = paraLtWidget.Owner.Widget as WParagraph;
		if (wParagraph == null && paraLtWidget.Owner.Widget is SplitWidgetContainer)
		{
			wParagraph = (paraLtWidget.Owner.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
		}
		bool isFirstLineOfParagraph = false;
		bool isLastLineOfParagraph = false;
		if (paraLtWidget.ChildWidgets.Count > 0)
		{
			isFirstLineOfParagraph = wParagraph.IsFirstLine(paraLtWidget.ChildWidgets[0]);
			isLastLineOfParagraph = wParagraph.IsLastLine(paraLtWidget.ChildWidgets[paraLtWidget.ChildWidgets.Count - 1]);
		}
		IStringWidget lastTextWidget = null;
		paraLtWidget.CalculateMaxChildWidget(DrawingContext, wParagraph, isFirstLineOfParagraph, isLastLineOfParagraph, out var maxHeight, out var maxAscent, out var maxTextHeight, out var _, out var maxTextDescent, out var _, out var _, out lastTextWidget, out var _, out var isTextInLine, out var containsInlinePicture, out var _);
		float num = (float)((maxHeight > maxTextHeight && isTextInLine && containsInlinePicture) ? maxTextDescent : 0.0);
		float lineSpacing = wParagraph.ParagraphFormat.LineSpacing;
		if (maxHeight != 0.0 || maxTextHeight != 0.0)
		{
			switch (wParagraph.ParagraphFormat.LineSpacingRule)
			{
			case LineSpacingRule.Exactly:
			{
				float num2 = Math.Abs(lineSpacing) - (float)maxTextHeight;
				topLineSpace = num2 * 80f / 100f;
				break;
			}
			case LineSpacingRule.AtLeast:
				if (maxHeight < (double)lineSpacing)
				{
					topLineSpace = (double)lineSpacing - (maxHeight + (double)num);
				}
				break;
			case LineSpacingRule.Multiple:
				if (lineSpacing != 12f && lineSpacing < 12f)
				{
					topLineSpace = maxTextHeight * (double)(lineSpacing / 12f) - maxTextHeight;
				}
				break;
			}
		}
		return (float)maxAscent;
	}

	private WPageSetup UpdateSectionPageSetup(IWidget widget)
	{
		if (widget is WSection)
		{
			return (widget as WSection).PageSetup;
		}
		if (widget is SplitWidgetContainer && (widget as SplitWidgetContainer).RealWidgetContainer is WSection)
		{
			return ((widget as SplitWidgetContainer).RealWidgetContainer as WSection).PageSetup;
		}
		return null;
	}

	private void IntializeGraphics()
	{
		DrawingContext.IntializeGraphics(Pages[0].Setup);
	}

	private WCharacterFormat GetNumberingFormat(WordDocument doc)
	{
		foreach (IStyle style in doc.Styles)
		{
			if (style.Name == "Line Number" && style.StyleType == StyleType.CharacterStyle && style is WCharacterStyle)
			{
				return (style as WCharacterStyle).CharacterFormat;
			}
		}
		return null;
	}

	public void InitLayoutInfo()
	{
		for (int i = 0; i < m_pages.Count; i++)
		{
			m_pages[i].InitLayoutInfo();
		}
		if (m_hiddenParaCollection.Count <= 0)
		{
			return;
		}
		for (int j = 0; j < m_hiddenParaCollection.Count; j++)
		{
			if (m_hiddenParaCollection[j].m_layoutInfo != null)
			{
				m_hiddenParaCollection[j].m_layoutInfo = null;
			}
		}
		m_hiddenParaCollection.Clear();
	}

	public void Close()
	{
		if (m_dc != null)
		{
			m_dc.Close();
			m_dc = null;
		}
		if (m_bookmarkHyperlinks != null)
		{
			m_bookmarkHyperlinks.Clear();
			m_bookmarkHyperlinks = null;
		}
		if (m_bookmarks != null)
		{
			m_bookmarks.Clear();
			m_bookmarks = null;
		}
		if (m_equationFields != null)
		{
			m_equationFields.Clear();
			m_equationFields = null;
		}
		if (FloatingItems != null)
		{
			ResetFloatingItemsProperties();
			FloatingItems.Clear();
			FloatingItems = null;
		}
		if (m_WrapFloatingItems != null)
		{
			m_WrapFloatingItems.Clear();
			m_WrapFloatingItems = null;
		}
		if (MaintainltWidget != null)
		{
			MaintainltWidget.ChildWidgets.Clear();
			MaintainltWidget = null;
		}
		if (m_tocEntryPageNumbers != null)
		{
			m_tocEntryPageNumbers.Clear();
			m_tocEntryPageNumbers = null;
		}
		if (m_tocParaItems != null)
		{
			m_tocParaItems.Clear();
			m_tocParaItems = null;
		}
		if (m_tocLevels != null)
		{
			m_tocLevels.Clear();
			m_tocLevels = null;
		}
		if (m_absolutePositionedTableHeights != null)
		{
			m_absolutePositionedTableHeights.Clear();
			m_absolutePositionedTableHeights = null;
		}
		if (m_prevFloatingItems != null)
		{
			m_prevFloatingItems.Clear();
			m_prevFloatingItems = null;
		}
		if (m_prevColumnsWidth != null)
		{
			m_prevColumnsWidth.Clear();
			m_prevColumnsWidth = null;
		}
		if (m_columnHasBreakItem != null)
		{
			m_columnHasBreakItem.Clear();
			m_columnHasBreakItem = null;
		}
		if (m_columnHeight != null)
		{
			m_columnHeight.Clear();
			m_columnHeight = null;
		}
		if (m_lineHeights != null)
		{
			m_lineHeights.Clear();
			m_lineHeights = null;
		}
		if (m_pages != null)
		{
			m_pages.Clear();
			m_pages = null;
		}
		if (m_bkStartPageNumbers != null)
		{
			m_bkStartPageNumbers.Clear();
			m_bkStartPageNumbers = null;
		}
		if (m_currPage != null && m_currPage.TrackChangesMarkups != null)
		{
			m_currPage.TrackChangesMarkups.Clear();
			m_currPage.TrackChangesMarkups = null;
		}
		if (m_currPage != null)
		{
			m_currPage = null;
		}
		if (m_docWidget != null)
		{
			m_docWidget = null;
		}
		if (m_currSection != null)
		{
			m_currSection = null;
		}
		if (m_prevSection != null)
		{
			m_prevSection = null;
		}
		if (m_headerLPHandler != null)
		{
			m_headerLPHandler = null;
		}
		if (m_footerLPHandler != null)
		{
			m_footerLPHandler = null;
		}
		if (m_pageResult != null)
		{
			m_pageResult = null;
		}
		if (m_notFittedfloatingItems != null)
		{
			m_notFittedfloatingItems.Clear();
			m_notFittedfloatingItems = null;
		}
		if (m_headerFooterNeedsRelayout != null)
		{
			m_headerFooterNeedsRelayout.Clear();
			m_headerFooterNeedsRelayout = null;
		}
		m_sectionNumPages = null;
	}

	internal Stream[] DrawToImage(int startPageIndex, int noOfPages, ExportImageFormat imageType)
	{
		if (m_pages.Count < startPageIndex || startPageIndex < 0)
		{
			return null;
		}
		if (m_pages.Count < startPageIndex + noOfPages)
		{
			noOfPages = m_pages.Count - startPageIndex;
		}
		int num = ((noOfPages == -1) ? m_pages.Count : (startPageIndex + noOfPages));
		Stream[] array = new Stream[num];
		for (int i = startPageIndex; i < num; i++)
		{
			IImage image = CreateImage(m_pages[i].Setup);
			MemoryStream memoryStream = new MemoryStream();
			PageNumber = i + 1;
			using (IGraphics graphics = WordDocument.RenderHelper.GetGraphics(image))
			{
				graphics.PageUnit = GraphicsUnit.Point;
				graphics.FillRectangle(WordDocument.RenderHelper.GetSolidBrush(Color.White), new Rectangle(0, 0, (int)m_pages[i].Setup.PageSize.Width, (int)m_pages[i].Setup.PageSize.Height));
				DrawingContext drawingContext = new DrawingContext(graphics, GraphicsUnit.Point);
				if (m_docWidget is WordDocument)
				{
					if ((m_docWidget as WordDocument).FontSettings.FallbackFonts.Count > 0)
					{
						drawingContext.FallbackFonts = (m_docWidget as WordDocument).FontSettings.FallbackFonts;
					}
					drawingContext.FontStreams = (m_docWidget as WordDocument).FontSettings.FontStreams;
				}
				bool num2 = !CurrentSection.PageSetup.IsFrontPageBorder;
				if (num2)
				{
					drawingContext.DrawPageBorder(i, m_pages);
				}
				drawingContext.Draw(m_pages[i]);
				if (!num2)
				{
					drawingContext.DrawPageBorder(i, m_pages);
				}
				ImageFormat imageFormat = GetImageFormat(imageType);
				graphics.ExportAsImage(imageFormat, memoryStream);
				graphics.Dispose();
				memoryStream.Position = 0L;
			}
			array[i] = memoryStream;
		}
		InitLayoutInfo();
		return array;
	}

	internal ImageFormat GetImageFormat(ExportImageFormat imageFormat)
	{
		return imageFormat switch
		{
			ExportImageFormat.Jpeg => ImageFormat.Jpeg, 
			ExportImageFormat.Png => ImageFormat.Png, 
			_ => ImageFormat.Png, 
		};
	}

	internal IImage CreateImage(WPageSetup pageSetup)
	{
		int width = (int)UnitsConvertor.Instance.ConvertToPixels(pageSetup.PageSize.Width, PrintUnits.Point);
		int height = (int)UnitsConvertor.Instance.ConvertToPixels(pageSetup.PageSize.Height, PrintUnits.Point);
		return WordDocument.RenderHelper.GetBitmap(width, height);
	}

	private bool LayoutPages()
	{
		m_bDirty = false;
		m_pages.Clear();
		BookmarkHyperlinks.Clear();
		Bookmarks.Clear();
		m_nextPageIndex = 0;
		CreateNewPage(ref m_docWidget);
		DrawingContext drawingContext = new DrawingContext();
		drawingContext.IntializeGraphics(m_currPage.Setup);
		drawingContext.FillRectangle(Color.White, new Rectangle(0, 0, (int)m_currPage.Setup.PageSize.Width, (int)m_currPage.Setup.PageSize.Height));
		Dictionary<string, Stream> fontStreams = m_dc.FontStreams;
		m_dc = drawingContext;
		m_dc.FontStreams = fontStreams;
		LayoutHeaderFooter();
		ClearFields();
		if (m_docWidget is WordDocument)
		{
			(m_docWidget as WordDocument).ClearLists();
		}
		m_bFirstPageForSection = false;
		Layouter layouter = new Layouter();
		layouter.LeafLayoutAfter += Layouter_LeafLayoutAfter;
		layouter.Layout(m_docWidget, this, m_dc);
		layouter.LeafLayoutAfter -= Layouter_LeafLayoutAfter;
		m_dc.DisposeGraphics();
		if (m_docWidget is WordDocument)
		{
			if (IsFirstLayouting && (m_docWidget as WordDocument).LastSection == CurrentSection && CurrentSection is WSection)
			{
				SectionNumPages[(CurrentSection as WSection).Index] = m_sectionPagesCount;
			}
			(m_docWidget as WordDocument).ClearLists();
		}
		return !m_bDirty;
	}

	internal byte[] ConvertAsImage(IWidget widget)
	{
		m_docWidget = (widget as Entity).Document;
		m_currSection = (widget as Entity).GetOwnerSection(widget as Entity) as WSection;
		m_currPage = new Page(m_currSection, 0);
		LayoutedWidget ltWidget = null;
		IImage image = CreateImage(m_currPage.Setup.PageSize.Width, m_currPage.Setup.PageSize.Height);
		using (IGraphics graphics = WordDocument.RenderHelper.GetGraphics(image))
		{
			graphics.PageUnit = GraphicsUnit.Point;
			graphics.FillRectangle(Color.White, new Rectangle(0, 0, (int)m_currPage.Setup.PageSize.Width, (int)m_currPage.Setup.PageSize.Height));
			DrawingContext drawingContext = (m_dc = new DrawingContext(graphics, GraphicsUnit.Point));
			if (m_docWidget is WordDocument)
			{
				m_dc.FontStreams = (m_docWidget as WordDocument).FontSettings.FontStreams;
			}
			Layouter layouter = new Layouter();
			layouter.LeafLayoutAfter += Layouter_LeafLayoutAfter;
			layouter.DrawingContext = drawingContext;
			RectangleF rect = (layouter.m_clientLayoutArea = new RectangleF(0f, 0f, CurrentPage.Setup.PageSize.Width, CurrentPage.Setup.PageSize.Height));
			ltWidget = LayoutContext.Create(widget, layouter, isForceFitLayout: true).Layout(rect);
			layouter.LeafLayoutAfter -= Layouter_LeafLayoutAfter;
		}
		return DrawAsImage(widget, ltWidget);
	}

	private byte[] DrawAsImage(IWidget widget, LayoutedWidget ltWidget)
	{
		float num = (float)Math.Ceiling(ltWidget.Bounds.Width);
		float num2 = (float)Math.Ceiling(ltWidget.Bounds.Height);
		IImage image = CreateImage(num + 2f, num2 + 2f);
		if (ltWidget.Bounds.X != 0f || ltWidget.Bounds.Y != 0f)
		{
			ltWidget.ShiftLocation(0f - ltWidget.Bounds.X, 0f - ltWidget.Bounds.Y, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
		}
		using (IGraphics graphics = WordDocument.RenderHelper.GetGraphics(image))
		{
			graphics.PageUnit = GraphicsUnit.Point;
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.FillRectangle(Color.Transparent, new Rectangle(0, 0, (int)num, (int)num2));
			DrawingContext drawingContext = new DrawingContext(graphics, GraphicsUnit.Point);
			if (m_docWidget is WordDocument && (m_docWidget as WordDocument).FontSettings.FallbackFonts.Count > 0)
			{
				drawingContext.FallbackFonts = (m_docWidget as WordDocument).FontSettings.FallbackFonts;
				drawingContext.FontStreams = (m_docWidget as WordDocument).FontSettings.FontStreams;
			}
			if (widget is WMath)
			{
				new MathRenderer(drawingContext).Draw(widget as WMath, ltWidget);
			}
			else
			{
				drawingContext.Draw(ltWidget, isHaveToInitLayoutInfo: true);
			}
		}
		byte[] result = ConvertToByteArray(image);
		image.Dispose();
		return result;
	}

	private byte[] ConvertToByteArray(IImage image)
	{
		using MemoryStream memoryStream = new MemoryStream();
		image.Save(memoryStream, ImageFormat.Png);
		return memoryStream.ToArray();
	}

	private IImage CreateImage(float pageWidth, float pageHeight)
	{
		int width = (int)UnitsConvertor.Instance.ConvertToPixels(pageWidth, PrintUnits.Point);
		int height = (int)UnitsConvertor.Instance.ConvertToPixels(pageHeight, PrintUnits.Point);
		return WordDocument.RenderHelper.GetBitmap(width, height);
	}

	private void CreateNewPage(ref IWidgetContainer curWidget)
	{
		if (MaintainltWidget.ChildWidgets.Count > 0 && (m_dynamicParagraph != null || m_dynamicTable != null))
		{
			LayoutedWidget layoutedWidget = MaintainltWidget;
			if (m_dynamicTable != null)
			{
				while (!(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is WTable))
				{
					layoutedWidget = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
				}
			}
			else
			{
				while (!(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is WParagraph))
				{
					layoutedWidget = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
				}
			}
			layoutedWidget.ChildWidgets.Remove(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1]);
			RecreateLayoutInfo();
			CurrentPage.PageWidgets.RemoveRange(2, CurrentPage.PageWidgets.Count - 2);
			for (int i = 0; i < MaintainltWidget.ChildWidgets.Count; i++)
			{
				CurrentPage.PageWidgets.Add(MaintainltWidget.ChildWidgets[i]);
			}
			m_dynamicParagraph = null;
			m_dynamicTable = null;
			curWidget = m_pageEndWidget;
		}
		m_currPage = new Page(CurrentSection, m_nextPageIndex);
		if (IsFirstLayouting)
		{
			m_sectionPagesCount++;
		}
		if (m_bisNeedToRestartFootnote)
		{
			m_footnoteIDRestartEachPage = 1;
		}
		if ((m_currSection.PageSetup.RestartPageNumbering && m_bFirstPageForSection) || (m_nextPageIndex == 0 && m_currSection.PageSetup.PageStartingNumber > 0))
		{
			m_currPage.Number = m_currSection.PageSetup.PageStartingNumber - 1;
			m_nextPageIndex = m_currPage.Number;
		}
		ResetNotAddedFloatingEntityProperty();
		if (m_FloatingItems.Count > 0)
		{
			ResetFloatingItemsProperties();
		}
		MaintainltWidget.ChildWidgets.Clear();
		InterSectingPoint = null;
		m_FloatingItems.Clear();
		m_notFittedfloatingItems.Clear();
		m_WrapFloatingItems.Clear();
		if (!IsUpdatingTOC)
		{
			m_pages.Add(m_currPage);
		}
		m_nextPageIndex++;
		IsForceFitLayout = true;
		PageNumber = m_nextPageIndex;
		m_columnsWidth = 0f;
		m_columnIndex = 0;
		IsEndPage = false;
	}

	private void RecreateLayoutInfo()
	{
		if (CurrentPage.PageWidgets.Count <= 2)
		{
			return;
		}
		LayoutedWidget layoutedWidget = CurrentPage.PageWidgets[CurrentPage.PageWidgets.Count - 1];
		while (layoutedWidget.ChildWidgets.Count != 0)
		{
			layoutedWidget = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
		}
		IWidget widget = layoutedWidget.Widget;
		WParagraph wParagraph = null;
		ParagraphItem paragraphItem = ((widget is SplitStringWidget) ? ((widget as SplitStringWidget).RealStringWidget as WTextRange) : (widget as ParagraphItem));
		if (paragraphItem != null)
		{
			wParagraph = paragraphItem.GetOwnerParagraphValue();
		}
		if (wParagraph == null || !(wParagraph.GetOwnerEntity() is WSection))
		{
			return;
		}
		for (int i = 0; i < wParagraph.ChildEntities.Count; i++)
		{
			Entity entity = wParagraph.ChildEntities[i];
			if (entity != null && (entity as WidgetBase).m_layoutInfo == null)
			{
				_ = (entity as IWidget).LayoutInfo;
			}
		}
	}

	private void ResetFloatingItemsProperties()
	{
		for (int i = 0; i < m_FloatingItems.Count; i++)
		{
			WParagraph ownerParagraph = m_FloatingItems[i].OwnerParagraph;
			if (ownerParagraph != null)
			{
				ownerParagraph.IsFloatingItemsLayouted = false;
				Entity floatingEntity = m_FloatingItems[i].FloatingEntity;
				if (floatingEntity is WTextBox && floatingEntity.IsFloatingItem(isTextWrapAround: true))
				{
					(floatingEntity as WTextBox).TextBoxFormat.IsWrappingBoundsAdded = false;
				}
				else if (floatingEntity is WPicture && floatingEntity.IsFloatingItem(isTextWrapAround: true))
				{
					(floatingEntity as WPicture).IsWrappingBoundsAdded = false;
				}
				else if (floatingEntity is Shape && floatingEntity.IsFloatingItem(isTextWrapAround: true))
				{
					(floatingEntity as Shape).WrapFormat.IsWrappingBoundsAdded = false;
				}
				else if (floatingEntity is WChart && floatingEntity.IsFloatingItem(isTextWrapAround: true))
				{
					(floatingEntity as WChart).WrapFormat.IsWrappingBoundsAdded = false;
				}
				else if (floatingEntity is GroupShape && floatingEntity.IsFloatingItem(isTextWrapAround: true))
				{
					(floatingEntity as GroupShape).WrapFormat.IsWrappingBoundsAdded = false;
				}
			}
		}
	}

	private void ResetNotAddedFloatingEntityProperty()
	{
		for (int i = 0; i < FloatingItems.Count; i++)
		{
			if (!FloatingItems[i].IsFloatingItemFit)
			{
				if (FloatingItems[i].FloatingEntity is WTextBox)
				{
					(FloatingItems[i].FloatingEntity as WTextBox).TextBoxFormat.IsWrappingBoundsAdded = false;
				}
				else if (FloatingItems[i].FloatingEntity is WPicture)
				{
					(FloatingItems[i].FloatingEntity as WPicture).IsWrappingBoundsAdded = false;
				}
				else if (FloatingItems[i].FloatingEntity is Shape)
				{
					(FloatingItems[i].FloatingEntity as Shape).WrapFormat.IsWrappingBoundsAdded = false;
				}
				else if (FloatingItems[i].FloatingEntity is GroupShape)
				{
					(FloatingItems[i].FloatingEntity as GroupShape).WrapFormat.IsWrappingBoundsAdded = false;
				}
			}
		}
	}

	private void CreateNewSection()
	{
		m_columnIndex = 0;
		m_columnsWidth = 0f;
		m_columnHeight.Clear();
		m_columnHasBreakItem.Clear();
		m_prevColumnsWidth.Clear();
	}

	private bool CheckSectionBreak(bool isFromDynmicLayout, IWidgetContainer curWidget)
	{
		bool result = false;
		if (m_bFirstPageForSection || (isFromDynmicLayout && CurrentPage.PageWidgets.Count > 2))
		{
			m_usedHeight += m_sectionFixedHeight;
			switch (CurrentSection.BreakCode)
			{
			case SectionBreakCode.NewColumn:
			case SectionBreakCode.NewPage:
				if (!isFromDynmicLayout)
				{
					result = true;
				}
				m_sectionFixedHeight = 0f;
				m_footnoteIDRestartEachSection = 1;
				break;
			case SectionBreakCode.Oddpage:
				if (!IsEvenPage && CurrentSection.PageSetup.PageStartingNumber == 0)
				{
					CreateNewPage(ref curWidget);
				}
				result = true;
				m_sectionFixedHeight = 0f;
				m_footnoteIDRestartEachSection = 1;
				break;
			case SectionBreakCode.EvenPage:
				if (IsEvenPage && CurrentSection.PageSetup.PageStartingNumber == 0)
				{
					CreateNewPage(ref curWidget);
				}
				result = true;
				m_sectionFixedHeight = 0f;
				m_footnoteIDRestartEachSection = 1;
				break;
			case SectionBreakCode.NoBreak:
			{
				int num = CurrentSection.Document.Sections.IndexOf(CurrentSection);
				WSection wSection = CurrentSection.PreviousSibling as WSection;
				if (CurrentSection.Body.ChildEntities.Count > 0)
				{
					if (CurrentSection.Body.ChildEntities[0] is WParagraph && (CurrentSection.Body.ChildEntities[0] as WParagraph).ParagraphFormat.PageBreakBefore)
					{
						m_createNewPage = true;
						MaintainltWidget.ChildWidgets.Clear();
					}
					else if (CurrentSection.Body.ChildEntities[0] is WTable && (CurrentSection.Body.ChildEntities[0] as WTable).Rows[0].Cells[0].Paragraphs.Count > 0 && (CurrentSection.Body.ChildEntities[0] as WTable).Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.PageBreakBefore)
					{
						m_createNewPage = true;
						MaintainltWidget.ChildWidgets.Clear();
					}
				}
				if (CurrentSection is WSection && (wSection == null || CurrentSection.PageSetup.Orientation == wSection.PageSetup.Orientation) && (CurrentPage.FootnoteWidgets.Count == 0 || CurrentSection.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 || CurrentSection.Document.DOP.Dop2000.Copts.FtnLayoutLikeWW8) && (wSection == null || (CurrentSection.PageSetup.PageSize.Height == wSection.PageSetup.PageSize.Height && CurrentSection.PageSetup.PageSize.Width == wSection.PageSetup.PageSize.Width)))
				{
					if (wSection != null && wSection.Columns.Count <= 1)
					{
						int count = CurrentPage.PageWidgets.Count;
						m_usedHeight = CurrentPage.PageWidgets[count - 1].Bounds.Bottom - m_pageTop;
						for (int i = 0; i < CurrentPage.EndnoteWidgets.Count; i++)
						{
							if (num - 1 == CurrentPage.EndNoteSectionIndex[i])
							{
								m_usedHeight += CurrentPage.EndnoteWidgets[i].Bounds.Height;
							}
						}
						m_sectionFixedHeight = m_usedHeight;
					}
					if (!m_createNewPage)
					{
						if (CurrentPage.FootnoteWidgets.Count > 0)
						{
							(CurrentSection as WSection).IsSectionFitInSamePage = true;
						}
						CreateNewSection();
						m_bFirstPageForSection = false;
						m_createNewPage = false;
						result = false;
					}
					else
					{
						result = true;
					}
				}
				else
				{
					result = true;
				}
				m_footnoteIDRestartEachSection = 1;
				break;
			}
			}
		}
		return result;
	}

	private bool IsLastTableHasKeepWithNext()
	{
		if (CurrentPage.PageWidgets.Count < 3)
		{
			return false;
		}
		bool result = false;
		LayoutedWidget layoutedWidget = CurrentPage.PageWidgets[CurrentPage.PageWidgets.Count - 1];
		if (layoutedWidget != null && layoutedWidget.ChildWidgets.Count > 0 && (layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is WSection || (layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is SplitWidgetContainer && (layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget as SplitWidgetContainer).RealWidgetContainer is WSection)))
		{
			LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
			if (layoutedWidget2 != null && layoutedWidget2.ChildWidgets.Count > 0 && (layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1].Widget is BlockContentControl || (layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1].Widget is SplitWidgetContainer && (layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1].Widget as SplitWidgetContainer).RealWidgetContainer is BlockContentControl)))
			{
				LayoutedWidget ltWidget = layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1];
				result = IsTableHasKeepWithNext(ltWidget);
			}
			else
			{
				result = IsTableHasKeepWithNext(layoutedWidget2);
			}
		}
		return result;
	}

	private bool IsTableHasKeepWithNext(LayoutedWidget ltWidget)
	{
		bool result = false;
		if ((ltWidget != null && ltWidget.ChildWidgets.Count > 0 && ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].Widget is WTable && !(ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].Widget as WTable).TableFormat.WrapTextAround) || (ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].Widget is SplitWidgetContainer && (ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].Widget as SplitWidgetContainer).RealWidgetContainer is WTable && !((ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].Widget as SplitWidgetContainer).RealWidgetContainer as WTable).TableFormat.WrapTextAround))
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1];
			int num = 0;
			while (layoutedWidget != null && layoutedWidget.ChildWidgets.Count > 0 && num < layoutedWidget.ChildWidgets.Count)
			{
				if (layoutedWidget.ChildWidgets[num].Widget is WTableRow && layoutedWidget.ChildWidgets[num].Widget.LayoutInfo.IsKeepWithNext)
				{
					result = true;
					num++;
					continue;
				}
				result = false;
				break;
			}
		}
		return result;
	}

	private void HandlePageBreak()
	{
		if (CurrentPage.PageWidgets.Count > 2)
		{
			int count = CurrentPage.PageWidgets.Count;
			LayoutedWidget layoutedWidget = CurrentPage.PageWidgets[count - 1];
			while (layoutedWidget.ChildWidgets.Count != 0)
			{
				layoutedWidget = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
			}
			if (layoutedWidget.Widget is Break && (layoutedWidget.Widget as Break).BreakType == BreakType.PageBreak)
			{
				m_createNewPage = true;
				MaintainltWidget.ChildWidgets.Clear();
			}
		}
	}

	private void LayoutHeaderFooter()
	{
		do
		{
			IsLayoutingHeaderFooter = true;
			IWidgetContainer currentHeader = GetCurrentHeader(CurrentSection);
			IWidgetContainer currentFooter = GetCurrentFooter();
			Layouter layouter = new Layouter();
			layouter.LeafLayoutAfter += Layouter_LeafLayoutAfter;
			layouter.Layout(currentHeader, m_headerLPHandler, m_dc);
			layouter.LeafLayoutAfter -= Layouter_LeafLayoutAfter;
			if (CurrentSection.PageSetup.Margins.Top < CurrentPage.PageWidgets[0].Bounds.Bottom && FloatingItems.Count > 0)
			{
				ShiftItemsForVerticalAlignment();
			}
			Layouter layouter2 = new Layouter();
			layouter2.LeafLayoutAfter += Layouter_LeafLayoutAfter;
			layouter2.Layout(currentFooter, m_footerLPHandler, m_dc);
			layouter2.LeafLayoutAfter -= Layouter_LeafLayoutAfter;
			IsLayoutingHeaderFooter = false;
			ResetFloatingItemsProperties();
		}
		while (IsNeedToRelayoutHeaderFooter());
	}

	private bool IsNeedToRelayoutHeaderFooter()
	{
		if (CurrentSection.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && HeaderFooterNeedsRelayout.Count > 0 && CurrentPage.PageWidgets.Count > 0)
		{
			string headerFooterType = GetHeaderFooterType(CurrentPage.PageWidgets[0].Widget);
			float word2013HeaderFooterHeight = GetWord2013HeaderFooterHeight();
			if ((!HeaderFooterNeedsRelayout.ContainsKey(headerFooterType) || !HeaderFooterNeedsRelayout[headerFooterType]) && CurrentPage.PageWidgets[0].Bounds.Height + CurrentPage.PageWidgets[1].Bounds.Height > word2013HeaderFooterHeight)
			{
				CurrentPage.PageWidgets.RemoveRange(0, CurrentPage.PageWidgets.Count);
				ResetNotAddedFloatingEntityProperty();
				m_FloatingItems.Clear();
				m_notFittedfloatingItems.Clear();
				m_WrapFloatingItems.Clear();
				CurrentPage.BehindWidgets.Clear();
				CurrentPage.NumberOfBehindWidgetsInHeader = 0;
				CurrentPage.NumberOfBehindWidgetsInFooter = 0;
				HeaderFooterNeedsRelayout[headerFooterType] = true;
				return true;
			}
		}
		return false;
	}

	internal float GetWord2013HeaderFooterHeight()
	{
		return CurrentSection.PageSetup.PageSize.Height * 0.8f;
	}

	internal string GetHeaderFooterType(IWidget widget)
	{
		HeaderFooterType type = (widget as HeaderFooter).Type;
		if (!type.ToString().Contains("Even"))
		{
			if (!type.ToString().Contains("FirstPage"))
			{
				return "Odd";
			}
			return "FirstPage";
		}
		return "Even";
	}

	private void ShiftItemsForVerticalAlignment()
	{
		for (int i = 0; i < FloatingItems.Count; i++)
		{
			FloatingItem floatingItem = FloatingItems[i];
			if (!(floatingItem.FloatingEntity is ParagraphItem))
			{
				continue;
			}
			VerticalOrigin verticalOrigin = (floatingItem.FloatingEntity as ParagraphItem).GetVerticalOrigin();
			WParagraph ownerParagraph = (floatingItem.FloatingEntity as ParagraphItem).OwnerParagraph;
			if (verticalOrigin != 0 || ownerParagraph.IsInCell || ownerParagraph.ParagraphFormat.IsFrame)
			{
				continue;
			}
			float verticalPosition = (floatingItem.FloatingEntity as ParagraphItem).GetVerticalPosition();
			if (verticalPosition == 0f)
			{
				continue;
			}
			verticalPosition += CurrentPage.PageWidgets[0].Bounds.Bottom;
			float num = verticalPosition - floatingItem.TextWrappingBounds.Y;
			floatingItem.TextWrappingBounds = new RectangleF(floatingItem.TextWrappingBounds.X, verticalPosition, floatingItem.TextWrappingBounds.Width, floatingItem.TextWrappingBounds.Height);
			LayoutedWidget layoutedWidget = CurrentPage.PageWidgets[0];
			for (int j = 0; j < layoutedWidget.ChildWidgets.Count; j++)
			{
				if (!(layoutedWidget.ChildWidgets[j].Widget is WParagraph))
				{
					continue;
				}
				foreach (LayoutedWidget childWidget in layoutedWidget.ChildWidgets[j].ChildWidgets)
				{
					foreach (LayoutedWidget childWidget2 in childWidget.ChildWidgets)
					{
						if (floatingItem.FloatingEntity == childWidget2.Widget)
						{
							childWidget2.ShiftLocation(0.0, num, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
							break;
						}
					}
				}
			}
		}
	}

	internal WTextBody GetCurrentHeader(IWSection section)
	{
		WHeadersFooters headersFooters = section.HeadersFooters;
		WSection wSection = null;
		WTextBody headerFooter = GetHeaderFooter(section, headersFooters.OddHeader);
		if (headersFooters.LinkToPrevious)
		{
			if (!(section.PreviousSibling is WSection))
			{
				return headerFooter;
			}
			wSection = section.PreviousSibling as WSection;
			if (section.PageSetup.DifferentOddAndEvenPages && IsEvenPage)
			{
				headerFooter = GetHeaderFooter(wSection, wSection.HeadersFooters.EvenHeader);
			}
			if (section.PageSetup.DifferentFirstPage && m_bFirstPageForSection)
			{
				headerFooter = GetHeaderFooter(wSection, wSection.HeadersFooters.FirstPageHeader);
			}
		}
		else
		{
			if (section.PageSetup.DifferentOddAndEvenPages && IsEvenPage)
			{
				headerFooter = GetHeaderFooter(section, headersFooters.EvenHeader);
			}
			if (section.PageSetup.DifferentFirstPage && m_bFirstPageForSection)
			{
				headerFooter = GetHeaderFooter(section, headersFooters.FirstPageHeader);
			}
		}
		return headerFooter;
	}

	private WTextBody GetCurrentFooter()
	{
		WHeadersFooters headersFooters = CurrentSection.HeadersFooters;
		WSection wSection = null;
		WTextBody headerFooter = GetHeaderFooter(CurrentSection, headersFooters.OddFooter);
		if (headersFooters.LinkToPrevious)
		{
			if (!(CurrentSection.PreviousSibling is WSection))
			{
				return headerFooter;
			}
			wSection = CurrentSection.PreviousSibling as WSection;
			if (CurrentSection.PageSetup.DifferentOddAndEvenPages && IsEvenPage)
			{
				headerFooter = GetHeaderFooter(wSection, wSection.HeadersFooters.EvenFooter);
			}
			if (CurrentSection.PageSetup.DifferentFirstPage && m_bFirstPageForSection)
			{
				headerFooter = GetHeaderFooter(wSection, wSection.HeadersFooters.FirstPageFooter);
			}
		}
		else
		{
			if (CurrentSection.PageSetup.DifferentOddAndEvenPages && IsEvenPage)
			{
				headerFooter = GetHeaderFooter(CurrentSection, headersFooters.EvenFooter);
			}
			if (CurrentSection.PageSetup.DifferentFirstPage && m_bFirstPageForSection)
			{
				headerFooter = GetHeaderFooter(CurrentSection, headersFooters.FirstPageFooter);
			}
		}
		return headerFooter;
	}

	private WTextBody GetHeaderFooter(IWSection section, HeaderFooter headerFooter)
	{
		WTextBody result = headerFooter;
		if (headerFooter.LinkToPrevious)
		{
			if (section is Entity && (section as Entity).Index > 0)
			{
				for (int num = (section as Entity).Index; num > 0; num--)
				{
					WSection wSection = section.Document.Sections[num - 1];
					result = wSection.HeadersFooters[headerFooter.Type];
					if (!wSection.HeadersFooters[headerFooter.Type].LinkToPrevious)
					{
						break;
					}
				}
			}
		}
		return result;
	}

	private void OnNextSection()
	{
		if (IsFirstLayouting)
		{
			m_sectionPagesCount = ((m_currSection.BreakCode == SectionBreakCode.NoBreak) ? 1 : 0);
		}
		m_bFirstPageForSection = true;
		m_sectionNewPage = false;
		m_isContinuousSectionLayouted = false;
		m_columnIndex = 0;
		m_columnsWidth = 0f;
		m_totalHeight = 0f;
	}

	internal Dictionary<Entity, int> GetTOCEntryPageNumbers(WordDocument doc)
	{
		m_docWidget = doc;
		m_currSection = doc.Sections[0];
		IsFirstLayouting = true;
		IsUpdatingTOC = true;
		LayoutPages();
		if (CurrentPage != null && CurrentPage.DocSection is WSection)
		{
			WParagraph lastLtParagraph = GetLastLtParagraph();
			int index = (CurrentPage.DocSection as WSection).Index;
			bool isLastTOCEntry = false;
			for (int i = 0; i <= index; i++)
			{
				doc.Sections[i].InitLayoutInfo(lastLtParagraph, ref isLastTOCEntry);
			}
		}
		m_footnoteId = 0;
		m_endnoteId = 0;
		IsUpdatingTOC = false;
		IsEndUpdateTOC = false;
		LastTocEntity = null;
		return TOCEntryPageNumbers;
	}

	private WParagraph GetLastLtParagraph()
	{
		if (CurrentPage.PageWidgets.Count < 3)
		{
			return null;
		}
		LayoutedWidget layoutedWidget = CurrentPage.PageWidgets[CurrentPage.PageWidgets.Count - 1];
		while (layoutedWidget != null)
		{
			layoutedWidget = ((layoutedWidget.ChildWidgets.Count > 0) ? layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1] : null);
			if (layoutedWidget == null)
			{
				break;
			}
			if (layoutedWidget.Widget is WParagraph)
			{
				return layoutedWidget.Widget as WParagraph;
			}
			if (layoutedWidget.Widget is SplitWidgetContainer && (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
			{
				return (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
			}
		}
		return null;
	}

	internal void UpdatePageFields(WordDocument doc, bool isUpdateFromWordToPDF)
	{
		m_docWidget = doc;
		m_currSection = doc.Sections[0];
		m_UpdatingPageFields = true;
		IsFirstLayouting = true;
		if (isUpdateFromWordToPDF)
		{
			Layout(doc);
		}
		else
		{
			LayoutPages();
		}
		m_UpdatingPageFields = false;
	}

	private WParagraph IsTOCParagraph(IWidget widget)
	{
		bool flag = false;
		WParagraph wParagraph = null;
		if ((widget is WParagraph && (!string.IsNullOrEmpty((widget as WParagraph).Text) || (widget as WParagraph).ListFormat.ListType == ListType.Numbered)) || (widget is WParagraph && (widget as WParagraph).IsContainsInLineImage()))
		{
			wParagraph = widget as WParagraph;
			flag = true;
		}
		else if (widget is SplitWidgetContainer && (widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
		{
			wParagraph = (widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
			if (!string.IsNullOrEmpty(wParagraph.Text) || wParagraph.IsContainsInLineImage())
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return null;
		}
		return wParagraph;
	}

	private void Layouter_LeafLayoutAfter(object sender, LayoutedWidget ltWidget, bool isFromTOCLinkStyle)
	{
		Entity entity;
		if (ltWidget.Widget is WField wField && (wField.FieldType == FieldType.FieldPage || wField.FieldType == FieldType.FieldNumPages || wField.FieldType == FieldType.FieldSectionPages || wField.FieldType == FieldType.FieldTOCEntry || wField.FieldType == FieldType.FieldPageRef || wField.FieldType == FieldType.FieldAutoNum || wField.FieldType == FieldType.FieldAutoNumLegal))
		{
			if (wField.FieldType == FieldType.FieldPage)
			{
				string text = "";
				bool pageFieldHasFormatText = false;
				string fieldCode = wField.FieldCode;
				if (fieldCode.Contains("\\"))
				{
					text = wField.UpdateTextFormat((CurrentPage.Number + 1).ToString(), fieldCode.Substring(fieldCode.IndexOf("\\")), ref pageFieldHasFormatText);
				}
				if (!pageFieldHasFormatText)
				{
					text = CurrentSection.PageSetup.GetNumberFormatValue((byte)CurrentSection.PageSetup.PageNumberStyle, CurrentPage.Number + 1);
				}
				ltWidget.TextTag = text;
				if (m_UpdatingPageFields)
				{
					wField.UpdateFieldResult(text);
				}
				WCharacterFormat characterFormatValue = wField.GetCharacterFormatValue();
				SizeF size = DrawingContext.MeasureString(ltWidget.TextTag, characterFormatValue.GetFontToRender(wField.ScriptType), null, characterFormatValue, isMeasureFromTabList: false, wField.ScriptType);
				ltWidget.Bounds = new RectangleF(ltWidget.Bounds.Location, size);
				if (m_UpdatingPageFields && wField.IsSkipFieldResult())
				{
					wField.SkipLayoutingFieldItems(isFieldCode: false);
				}
			}
			else if (wField.FieldType == FieldType.FieldAutoNum || wField.FieldType == FieldType.FieldAutoNumLegal)
			{
				ltWidget.TextTag = wField.Text;
				WCharacterFormat characterFormatValue2 = wField.GetCharacterFormatValue();
				SizeF size2 = DrawingContext.MeasureString(ltWidget.TextTag, characterFormatValue2.GetFontToRender(wField.ScriptType), null, characterFormatValue2, isMeasureFromTabList: false, wField.ScriptType);
				ltWidget.Bounds = new RectangleF(ltWidget.Bounds.Location, size2);
			}
			else if (wField.FieldType == FieldType.FieldSectionPages)
			{
				if (!IsFirstLayouting)
				{
					if (CurrentSection is WSection)
					{
						ltWidget.TextTag = SectionNumPages[(CurrentSection as WSection).Index].ToString();
					}
					WCharacterFormat characterFormatValue3 = wField.GetCharacterFormatValue();
					SizeF size3 = DrawingContext.MeasureString(ltWidget.TextTag, characterFormatValue3.GetFontToRender(wField.ScriptType), null, characterFormatValue3, isMeasureFromTabList: false, wField.ScriptType);
					ltWidget.Bounds = new RectangleF(ltWidget.Bounds.Location, size3);
				}
				else
				{
					m_bDirty = true;
				}
			}
			else if (wField.FieldType == FieldType.FieldNumPages)
			{
				CurrentPage.AddCachedFields(wField);
				m_bDirty = true;
			}
			else if (wField.FieldType == FieldType.FieldPageRef)
			{
				wField.FieldResult = (CurrentPage.Number + 1).ToString();
			}
			else if (wField.FieldType == FieldType.FieldTOCEntry && !IsLayoutingHeaderFooter && IsUpdatingTOC && UseTCFields)
			{
				if (TOCEntryPageNumbers.Count > 0 && TOCEntryPageNumbers.ContainsKey(wField))
				{
					TOCEntryPageNumbers[wField] = CurrentPage.Number + 1;
				}
				else
				{
					TOCEntryPageNumbers.Add(wField, CurrentPage.Number + 1);
				}
				if (wField == LastTocEntity)
				{
					IsEndUpdateTOC = true;
				}
			}
		}
		else if (IsUpdatingTOC && !IsLayoutingHeaderFooter && (entity = IsTOCParagraph(ltWidget.Widget)) != null)
		{
			if (TOCEntryPageNumbers.Count > 0 && TOCEntryPageNumbers.ContainsKey(entity))
			{
				TOCEntryPageNumbers[entity] = CurrentPage.Number + 1;
			}
			else
			{
				TOCEntryPageNumbers.Add(entity, CurrentPage.Number + 1);
			}
			if (entity == LastTocEntity && !ltWidget.Widget.LayoutInfo.IsKeepWithNext)
			{
				IsEndUpdateTOC = true;
			}
		}
		else if (IsUpdatingTOC && !IsLayoutingHeaderFooter && isFromTOCLinkStyle)
		{
			entity = ltWidget.Widget as Entity;
			if (ltWidget.Widget is SplitStringWidget)
			{
				entity = (ltWidget.Widget as SplitStringWidget).RealStringWidget as Entity;
			}
			if (TOCEntryPageNumbers != null && CurrentPage != null && entity != null)
			{
				if (TOCEntryPageNumbers.Count > 0 && TOCEntryPageNumbers.ContainsKey(entity))
				{
					TOCEntryPageNumbers[entity] = CurrentPage.Number + 1;
				}
				else
				{
					TOCEntryPageNumbers.Add(entity, CurrentPage.Number + 1);
				}
			}
			if (entity == LastTocEntity)
			{
				IsEndUpdateTOC = true;
			}
		}
		else
		{
			if (!(ltWidget.Widget is BookmarkStart))
			{
				return;
			}
			BookmarkStart bookmarkStart = ltWidget.Widget as BookmarkStart;
			WTextBody entityOwnerTextBody = bookmarkStart.Document.GetEntityOwnerTextBody(bookmarkStart.GetOwnerParagraphValue());
			if (entityOwnerTextBody is HeaderFooter || entityOwnerTextBody.Owner is WComment)
			{
				if (BookmarkStartPageNumbers.Count > 0 && BookmarkStartPageNumbers.ContainsKey(bookmarkStart))
				{
					BookmarkStartPageNumbers[bookmarkStart] = 1;
				}
				else
				{
					BookmarkStartPageNumbers.Add(bookmarkStart, 1);
				}
			}
			else if (BookmarkStartPageNumbers.Count > 0 && BookmarkStartPageNumbers.ContainsKey(bookmarkStart))
			{
				BookmarkStartPageNumbers[bookmarkStart] = CurrentPage.Number + 1;
			}
			else
			{
				BookmarkStartPageNumbers.Add(bookmarkStart, CurrentPage.Number + 1);
			}
		}
	}

	bool ILayoutProcessHandler.GetNextArea(out RectangleF area, ref int columnIndex, ref bool isContinuousSection, bool isSplittedWidget, ref float topMargin, bool isFromDynmicLayout, ref IWidgetContainer curWidget)
	{
		int count = CurrentSection.Columns.Count;
		if (!isSplittedWidget || isFromDynmicLayout)
		{
			if (isFromDynmicLayout)
			{
				LayoutedWidget layoutedWidget = MaintainltWidget.ChildWidgets[InterSectingPoint[0]].ChildWidgets[InterSectingPoint[1]];
				if (InterSectingPoint[0] != int.MinValue && layoutedWidget.Widget is WSection)
				{
					m_currSection = layoutedWidget.Widget as WSection;
				}
				if (m_currSection.PageSetup.Bidi && count > 1)
				{
					if (layoutedWidget.Bounds.X.Equals(Layouter.GetCurrentPageRightPosition(m_currSection as WSection) - m_currSection.Columns[0].Width))
					{
						m_columnIndex = 0;
						m_columnsWidth = 0f;
					}
					else
					{
						int i = 0;
						int num = 0;
						float num2 = 0f;
						for (; Math.Round(layoutedWidget.Bounds.X) != Math.Round(MaintainltWidget.ChildWidgets[i].Bounds.X); i++)
						{
							if (num2 != MaintainltWidget.ChildWidgets[i].Bounds.X)
							{
								num2 = MaintainltWidget.ChildWidgets[i].Bounds.X;
								num++;
							}
						}
						m_columnIndex = num;
						Column column = ((m_columnIndex != 0) ? m_currSection.Columns[m_columnIndex - 1] : null);
						if (column != null)
						{
							num2 -= column.Space;
						}
						m_columnsWidth = Layouter.GetCurrentPageRightPosition(m_currSection as WSection) - num2;
					}
				}
				else if (layoutedWidget.Bounds.X == Layouter.GetLeftMargin(m_currSection as WSection))
				{
					m_columnIndex = 0;
					m_columnsWidth = 0f;
				}
				else
				{
					int j = 0;
					int num3 = 0;
					float num4 = 0f;
					for (; Math.Round(layoutedWidget.Bounds.X) != Math.Round(MaintainltWidget.ChildWidgets[j].Bounds.X); j++)
					{
						if (num4 != MaintainltWidget.ChildWidgets[j].Bounds.Right)
						{
							num4 = MaintainltWidget.ChildWidgets[j].Bounds.Right;
							num3++;
						}
					}
					m_columnIndex = num3;
					Column column2 = ((m_currSection.Columns.Count > columnIndex) ? m_currSection.Columns[columnIndex] : null);
					if (column2 != null)
					{
						num4 += column2.Space;
					}
					m_columnsWidth = num4 - Layouter.GetLeftMargin(m_currSection as WSection);
				}
			}
			else
			{
				m_columnIndex = 0;
				m_columnsWidth = 0f;
			}
		}
		bool flag = (count == 0 && m_columnIndex > 0) || (count > 0 && m_columnIndex > count - 1);
		if (CurrentSection.Document.Sections.Count > 1)
		{
			bool flag2 = CheckSectionBreak(isFromDynmicLayout, curWidget);
			flag = flag || flag2;
		}
		if (!flag)
		{
			flag = CurrentSection.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && IsLastTableHasKeepWithNext();
		}
		HandlePageBreak();
		HandleDynamicRelayouting(isFromDynmicLayout);
		if ((flag && (m_usedHeight == m_clientHeight || !isContinuousSection)) || m_createNewPage || IsEndPage)
		{
			CreateNewPage(ref curWidget);
			LayoutHeaderFooter();
			ClearFields();
		}
		area = GetSectionClientArea(isSplittedWidget, curWidget);
		topMargin = m_pageTop;
		isContinuousSection = CheckNextSectionBreakType(isContinuousSection);
		if (isContinuousSection && !m_isContinuousSectionLayouted && CurrentSection.Columns.Count > 1 && m_columnIndex == 0 && isSplittedWidget && !CurrentSection.PageSetup.EqualColumnWidth && !IsEqualColumnWidth())
		{
			float num5 = 0f;
			for (int k = 0; k < CurrentSection.Columns.Count; k++)
			{
				num5 += CurrentSection.Columns[k].Width;
			}
			area.Width = num5;
			m_totalColumnWidth = num5;
		}
		if (m_isContinuousSectionLayouted)
		{
			if (m_columnIndex == 0)
			{
				m_sectionFixedHeight = GetRequiredHeightForContinuousSection();
			}
			isContinuousSection = false;
			if ((CurrentPage.PageWidgets[0].ChildWidgets.Count > 0 && CurrentPage.PageWidgets[0].Bounds.Height + CurrentSection.PageSetup.HeaderDistance > CurrentSection.PageSetup.Margins.Top) ? (Math.Round(area.Y, 2) == Math.Round(CurrentPage.PageWidgets[0].Bounds.Height + CurrentSection.PageSetup.HeaderDistance, 2)) : (area.Y == CurrentSection.PageSetup.Margins.Top))
			{
				IsForceFitLayout = true;
			}
			if (area.Height > m_sectionFixedHeight + RemovedWidgetsHeight)
			{
				area.Height = m_sectionFixedHeight + RemovedWidgetsHeight;
			}
			RemovedWidgetsHeight = 0f;
			m_firstParaHeight = 0f;
			lineHeigthCount = 0;
			m_footnoteHeight = 0f;
			m_lineHeights.Clear();
		}
		columnIndex = m_columnIndex;
		m_columnIndex++;
		return !area.Equals(RectangleF.Empty);
	}

	private void HandleDynamicRelayouting(bool isFromDynmicLayout)
	{
		if (!isFromDynmicLayout)
		{
			return;
		}
		LayoutedWidget layoutedWidget = ((InterSectingPoint[0] < MaintainltWidget.ChildWidgets.Count && InterSectingPoint[1] < MaintainltWidget.ChildWidgets[InterSectingPoint[0]].ChildWidgets.Count) ? MaintainltWidget.ChildWidgets[InterSectingPoint[0]].ChildWidgets[InterSectingPoint[1]] : null);
		LayoutedWidget layoutedWidget2 = layoutedWidget?.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
		LayoutedWidget layoutedWidget3 = ((layoutedWidget != null && InterSectingPoint[2] < layoutedWidget.ChildWidgets.Count) ? layoutedWidget.ChildWidgets[InterSectingPoint[2]] : null);
		if (layoutedWidget2 != null && layoutedWidget3 != null)
		{
			bool flag = IsIntersectedItem(layoutedWidget3, layoutedWidget2);
			float num = layoutedWidget3.Bounds.Y - MaintainltWidget.ChildWidgets[0].Bounds.Y;
			float num2 = CurrentPage.Setup.Margins.Top;
			float val = ((CurrentPage.PageWidgets[0].ChildWidgets.Count == 0 || num2 < 0f) ? 0f : (CurrentPage.PageWidgets[0].Bounds.Height + ((CurrentPage.Setup.HeaderDistance != -0.05f) ? CurrentPage.Setup.HeaderDistance : 36f)));
			float val2 = ((CurrentPage.PageWidgets[1].ChildWidgets.Count == 0 || CurrentPage.Setup.Margins.Bottom < 0f) ? 0f : (CurrentPage.PageWidgets[1].Bounds.Height + ((CurrentPage.Setup.FooterDistance != -0.05f) ? CurrentPage.Setup.FooterDistance : 36f)));
			if (CurrentPage.Setup.Margins.Gutter > 0f && CurrentPage.Setup.Document.DOP.GutterAtTop)
			{
				num2 += CurrentPage.Setup.Margins.Gutter;
			}
			float num3 = CurrentPage.Setup.PageSize.Height - (Math.Max(num2, val) + Math.Max(CurrentPage.Setup.Margins.Bottom, val2));
			num3 -= num;
			float intersectingHeight = GetIntersectingHeight(layoutedWidget, layoutedWidget3, flag);
			bool flag2 = layoutedWidget2.Bounds.Height + intersectingHeight <= num3;
			if (flag && !flag2 && layoutedWidget2.Bounds.Width >= m_currSection.PageSetup.ClientWidth && m_notFittedfloatingItems.Count == 0)
			{
				m_createNewPage = true;
			}
		}
	}

	private float GetIntersectingHeight(LayoutedWidget layoutedSectionWidget, LayoutedWidget intersectingItem, bool isItemIntersected)
	{
		float num = 0f;
		if (intersectingItem.Widget is WTable)
		{
			num = layoutedSectionWidget.ChildWidgets[layoutedSectionWidget.ChildWidgets.Count - 2].Bounds.Bottom - intersectingItem.Bounds.Y;
		}
		else
		{
			LayoutedWidget layoutedWidget = intersectingItem.ChildWidgets[InterSectingPoint[3]];
			num = (isItemIntersected ? (intersectingItem.ChildWidgets[intersectingItem.ChildWidgets.Count - 1].Bounds.Bottom - layoutedWidget.Bounds.Y) : num);
			int num2 = InterSectingPoint[2] + 1;
			if (InterSectingPoint[2] < layoutedSectionWidget.ChildWidgets.Count - 2 && num2 < layoutedSectionWidget.ChildWidgets.Count)
			{
				num += layoutedSectionWidget.ChildWidgets[layoutedSectionWidget.ChildWidgets.Count - 2].Bounds.Bottom - layoutedSectionWidget.ChildWidgets[num2].Bounds.Y;
			}
		}
		return num;
	}

	private bool IsIntersectedItem(LayoutedWidget intersectingItem, LayoutedWidget layoutedFloatingItem)
	{
		if (intersectingItem.Widget is WTable)
		{
			return true;
		}
		bool flag = false;
		for (int i = 0; i < intersectingItem.ChildWidgets.Count; i++)
		{
			if (flag)
			{
				break;
			}
			flag = layoutedFloatingItem.Bounds.IntersectsWith(intersectingItem.ChildWidgets[i].Bounds);
		}
		return flag;
	}

	private RectangleF GetSectionClientArea(bool isSplittedWidget, IWidgetContainer curWidget)
	{
		RectangleF result = GetColumnClientArea(isSplittedWidget);
		if (CurrentSection.BreakCode == SectionBreakCode.NoBreak)
		{
			float firstLineHeight = GetFirstLineHeight();
			if (result.Height < firstLineHeight)
			{
				if (CurrentSection.Body.ChildEntities.Count > 0 && CurrentSection.Body.ChildEntities[0] is WParagraph && (IsFirstItemBreakItems(CurrentSection.Body.ChildEntities[0] as WParagraph) || IsFirstInlineTextWrappingStyleItem(CurrentSection.Body.ChildEntities[0] as WParagraph)))
				{
					result = new RectangleF(result.X, result.Y, result.Width, firstLineHeight);
				}
				else if (m_currPage.PageWidgets.Count > 2)
				{
					CreateNewPage(ref curWidget);
					LayoutHeaderFooter();
					ClearFields();
					result = GetColumnClientArea(isSplittedWidget);
				}
			}
		}
		return result;
	}

	private bool IsFirstInlineTextWrappingStyleItem(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.ChildEntities.Count; i++)
		{
			Entity entity = paragraph.ChildEntities[i];
			if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity as ParagraphItem).ParaItemCharFormat.Hidden && (entity is WPicture || entity is WTextBox || entity is Shape || entity is GroupShape || entity is WChart) && (entity as ParagraphItem).GetTextWrappingStyle() == TextWrappingStyle.Inline)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsFirstItemBreakItems(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.ChildEntities.Count; i++)
		{
			Entity entity = paragraph.ChildEntities[i];
			if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity as ParagraphItem).ParaItemCharFormat.Hidden)
			{
				if (entity is Break && (entity as Break).BreakType != BreakType.LineBreak && (entity as Break).BreakType != BreakType.TextWrappingBreak)
				{
					return true;
				}
				return false;
			}
		}
		return true;
	}

	private RectangleF GetColumnClientArea(bool isSplittedWidget)
	{
		RectangleF empty = RectangleF.Empty;
		if (CurrentSection.BreakCode == SectionBreakCode.NoBreak && !m_isFirstPage)
		{
			if (m_sectionNewPage)
			{
				empty = CurrentPage.GetColumnArea(m_columnIndex, ref m_columnsWidth, m_isNeedtoAdjustFooter);
				m_pageTop = empty.Y;
				m_clientHeight = empty.Height;
			}
			else
			{
				empty = CurrentPage.GetSectionArea(m_columnIndex, ref m_columnsWidth, m_isContinuousSectionLayouted, isSplittedWidget);
			}
			empty.Y = m_pageTop + m_usedHeight;
			empty.Height = m_clientHeight - m_usedHeight;
		}
		else
		{
			empty = CurrentPage.GetColumnArea(m_columnIndex, ref m_columnsWidth, m_isNeedtoAdjustFooter);
			m_pageTop = empty.Y;
			m_sectionNewPage = false;
			m_isFirstPage = false;
			m_clientHeight = empty.Height;
			IsEndPage = false;
		}
		m_isNeedtoAdjustFooter = false;
		int num = CurrentSection.Document.Sections.IndexOf(CurrentSection);
		for (int i = 0; i < CurrentPage.EndnoteWidgets.Count; i++)
		{
			if (num - 1 == CurrentPage.EndNoteSectionIndex[i])
			{
				empty.Y = CurrentPage.EndnoteWidgets[i].Bounds.Bottom;
			}
		}
		return empty;
	}

	private void ClearFields()
	{
		m_columnHeight.Clear();
		m_lineHeights.Clear();
		m_columnHasBreakItem.Clear();
		m_prevColumnsWidth.Clear();
		m_totalHeight = 0f;
		m_usedHeight = 0f;
		m_sectionFixedHeight = 0f;
		m_sectionNewPage = true;
		m_createNewPage = false;
		m_isContinuousSectionLayouted = false;
		m_bFirstPageForSection = false;
		RemovedWidgetsHeight = 0f;
		m_footnoteHeight = 0f;
		m_firstParaHeight = 0f;
		lineHeigthCount = 0;
	}

	private float GetRequiredHeightForContinuousSection()
	{
		FloatingItems = m_prevFloatingItems;
		float floatingItemHeight = GetFloatingItemHeight(FloatingItems);
		ResetFloatingItemsProperties();
		m_prevFloatingItems.Clear();
		int count = CurrentSection.Columns.Count;
		float num = 0f;
		if (CurrentSection.PageSetup.EqualColumnWidth || IsEqualColumnWidth())
		{
			foreach (float lineHeight in m_lineHeights)
			{
				num += lineHeight;
				if (num >= (m_totalHeight + floatingItemHeight) / (float)(count - m_columnIndex))
				{
					break;
				}
			}
		}
		else
		{
			num = GetRequiredHeightForUnEqualColumns();
		}
		if (m_absolutePositionedTableHeights != null)
		{
			foreach (float absolutePositionedTableHeight in m_absolutePositionedTableHeights)
			{
				num += absolutePositionedTableHeight;
			}
			m_absolutePositionedTableHeights.Clear();
		}
		if (m_firstParaHeight > num && lineHeigthCount <= 3 && m_totalHeight > num)
		{
			return (float)Math.Ceiling(m_totalHeight + m_footnoteHeight);
		}
		return (float)Math.Ceiling(num + m_footnoteHeight);
	}

	private float GetFloatingItemHeight(List<FloatingItem> floatingItems)
	{
		foreach (FloatingItem floatingItem in floatingItems)
		{
			if (floatingItem.TextWrappingStyle != TextWrappingStyle.InFrontOfText && floatingItem.TextWrappingStyle != TextWrappingStyle.Behind && (floatingItem.TextWrappingStyle == TextWrappingStyle.TopAndBottom || floatingItem.TextWrappingBounds.Width >= CurrentSection.Columns[0].Width))
			{
				return floatingItem.TextWrappingBounds.Height;
			}
		}
		return 0f;
	}

	private float GetRequiredHeightForUnEqualColumns()
	{
		float num = 0f;
		int columnIndexForMinColumnWidth = GetColumnIndexForMinColumnWidth();
		int columnIndexForMaxColumnWidth = GetColumnIndexForMaxColumnWidth();
		float firstLineHeight = GetFirstLineHeight();
		float num2 = CurrentSection.Columns[columnIndexForMaxColumnWidth].Width - CurrentSection.Columns[columnIndexForMinColumnWidth].Width;
		if (num2 < CurrentSection.Columns[columnIndexForMinColumnWidth].Width)
		{
			if (CurrentSection.PageSetup.Orientation == PageOrientation.Landscape)
			{
				return m_totalColumnWidth * m_totalHeight / CurrentSection.Columns[columnIndexForMaxColumnWidth].Width - m_totalHeight;
			}
			float num3 = m_totalColumnWidth * m_totalHeight / CurrentSection.Columns[columnIndexForMinColumnWidth].Width - m_totalHeight;
			float num4 = 0f;
			for (int i = 0; i < CurrentSection.Columns.Count; i++)
			{
				num4 += (float)(2 * CurrentSection.Columns.Count - 1) * Math.Abs(CurrentSection.Columns[i].Width - CurrentSection.Columns[columnIndexForMinColumnWidth].Width);
			}
			return Math.Abs(num3 - (float)(CurrentSection.Columns.Count - 1) * num4);
		}
		num = ((columnIndexForMaxColumnWidth >= columnIndexForMinColumnWidth) ? (num2 * m_totalHeight / m_totalColumnWidth + num2 * CurrentSection.Columns[columnIndexForMinColumnWidth].Width / ((float)CurrentSection.Columns.Count * CurrentSection.Columns[columnIndexForMaxColumnWidth].Width)) : (num2 * m_totalHeight / m_totalColumnWidth + (float)CurrentSection.Columns.Count * CurrentSection.Columns[columnIndexForMaxColumnWidth].Width / (num2 * CurrentSection.Columns[columnIndexForMinColumnWidth].Width)));
		string[] array = (CurrentSection.Columns[columnIndexForMaxColumnWidth].Width / CurrentSection.Columns[columnIndexForMinColumnWidth].Width).ToString(CultureInfo.InvariantCulture).Split('.');
		float num5 = 0f;
		if (array.Length > 1)
		{
			num5 = (float)(Convert.ToDouble(array[1]) * (double)m_totalHeight) / ((float)CurrentSection.Columns.Count * (float)Math.Pow(10.0, array[1].Length));
		}
		if (columnIndexForMaxColumnWidth < columnIndexForMinColumnWidth)
		{
			num += num5;
			return num + firstLineHeight;
		}
		num -= num5;
		if (CurrentSection.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && CurrentSection.Body.Items.FirstItem is WParagraph && (CurrentSection.Body.Items.FirstItem as WParagraph).ParagraphFormat.HasValue(8))
		{
			firstLineHeight += (CurrentSection.Body.Items.FirstItem as WParagraph).ParagraphFormat.BeforeSpacing;
			if (firstLineHeight > num)
			{
				return firstLineHeight - num;
			}
			return num + firstLineHeight;
		}
		if (num > firstLineHeight)
		{
			return num - firstLineHeight;
		}
		return firstLineHeight;
	}

	private float GetFirstLineHeight()
	{
		float num = 0f;
		if (CurrentSection.Body.Items.FirstItem is WParagraph)
		{
			WParagraph wParagraph = CurrentSection.Body.Items.FirstItem as WParagraph;
			num = wParagraph.GetHeight(wParagraph, (wParagraph.ChildEntities.Count > 0) ? (wParagraph.ChildEntities[0] as ParagraphItem) : null);
		}
		else if (CurrentSection.Body.Items.FirstItem is WTable)
		{
			WTable wTable = CurrentSection.Body.Items.FirstItem as WTable;
			if (wTable.Rows[0].Cells[0].LastParagraph != null)
			{
				WParagraph wParagraph2 = wTable.Rows[0].Cells[0].LastParagraph as WParagraph;
				num = wParagraph2.GetHeight(wParagraph2, (wParagraph2.ChildEntities.Count > 0) ? (wParagraph2.ChildEntities[0] as ParagraphItem) : null);
			}
			float num2 = ((wTable.Rows[0].Height >= 0f) ? wTable.Rows[0].Height : (-1f * wTable.Rows[0].Height));
			if (wTable.Rows[0].HeightType == TableRowHeightType.Exactly && num2 < num)
			{
				num = num2;
			}
			else if (wTable.Rows[0].HeightType == TableRowHeightType.AtLeast && num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private bool IsEqualColumnWidth()
	{
		float width = CurrentSection.Columns[0].Width;
		bool result = true;
		foreach (Column column in CurrentSection.Columns)
		{
			if (width != column.Width)
			{
				result = false;
				break;
			}
			width = column.Width;
		}
		return result;
	}

	private bool CheckNextSectionBreakType(bool isContinuousSection)
	{
		int num = CurrentSection.Document.Sections.IndexOf(CurrentSection);
		if (CurrentSection.Document.Sections.Count - 1 > num)
		{
			if (((IWSection)CurrentSection.Document.Sections[num + 1]).BreakCode == SectionBreakCode.NoBreak && CurrentSection.Columns.Count > 1)
			{
				isContinuousSection = true;
				m_prevFloatingItems = FloatingItems;
				if (m_columnIndex == 0 && m_sectionIndex != num)
				{
					m_createNewPage = false;
					m_isContinuousSectionLayouted = false;
					m_sectionIndex = num;
				}
			}
			else
			{
				isContinuousSection = false;
				m_isContinuousSectionLayouted = false;
			}
		}
		else
		{
			isContinuousSection = false;
			m_isContinuousSectionLayouted = false;
		}
		return isContinuousSection;
	}

	void ILayoutProcessHandler.PushLayoutedWidget(LayoutedWidget ltWidget, RectangleF layoutArea, bool isNeedToRestartFootnote, bool isNeedToRestartEndnote, LayoutState state, bool isNeedToFindInterSectingPoint, bool isContinuousSection)
	{
		if (CurrentPage.FootnoteWidgets.Count > 0 && state != LayoutState.DynamicRelayout)
		{
			if (CurrentSection.PageSetup.FootnotePosition == FootnotePosition.PrintImmediatelyBeneathText)
			{
				FootnotePushLayoutedWidget(ltWidget.Bounds);
			}
			else
			{
				FootnotePushLayoutedWidget(layoutArea);
			}
		}
		if (!isContinuousSection)
		{
			if (CurrentPage.EndnoteWidgets.Count > 0)
			{
				EndnotePushLayoutedWidget(ltWidget.Bounds, ltWidget);
			}
			m_bisNeedToRestartFootnote = isNeedToRestartFootnote;
			m_bisNeedToRestartEndnote = isNeedToRestartFootnote;
			CurrentPage.PageWidgets.Add(ltWidget);
			if (state == LayoutState.DynamicRelayout && isNeedToFindInterSectingPoint)
			{
				FindIntersectPointAndRemovltWidget(ltWidget);
			}
		}
	}

	private void FindIntersectPointAndRemovltWidget(LayoutedWidget ltwidget)
	{
		while (!(ltwidget.ChildWidgets[ltwidget.ChildWidgets.Count - 1].Widget is ParagraphItem) && !(ltwidget.ChildWidgets[ltwidget.ChildWidgets.Count - 1].Widget is SplitStringWidget) && !(ltwidget.ChildWidgets[ltwidget.ChildWidgets.Count - 1].Widget is WTable))
		{
			int num = ltwidget.ChildWidgets.Count - 1;
			if (ltwidget.ChildWidgets.Count > 1 && ltwidget.ChildWidgets[num].ChildWidgets.Count == 0 && ltwidget.ChildWidgets[num].Widget is SplitWidgetContainer)
			{
				ltwidget.ChildWidgets.RemoveAt(num);
				num--;
			}
			ltwidget = ltwidget.ChildWidgets[num];
			if (ltwidget.ChildWidgets.Count <= 0)
			{
				break;
			}
		}
		for (int i = 2; i < CurrentPage.PageWidgets.Count; i++)
		{
			MaintainltWidget.ChildWidgets.Add(new LayoutedWidget(CurrentPage.PageWidgets[i]));
		}
		if (ltwidget.ChildWidgets.Count > 0 && ltwidget.ChildWidgets[ltwidget.ChildWidgets.Count - 1].Widget is WTable)
		{
			m_dynamicTable = ltwidget.ChildWidgets[ltwidget.ChildWidgets.Count - 1].Widget as WTable;
		}
		RectangleF rectangleF = ((ltwidget.ChildWidgets.Count > 0) ? ltwidget.ChildWidgets[ltwidget.ChildWidgets.Count - 1].Bounds : ltwidget.Bounds);
		float y = rectangleF.Y;
		float bottom = rectangleF.Bottom;
		float num2 = rectangleF.Right;
		if (ltwidget.Widget is WParagraph && (ltwidget.Widget as WParagraph).ParagraphFormat != null && (ltwidget.Widget as WParagraph).ParagraphFormat.IsFrame)
		{
			num2 = ltwidget.Bounds.X + (ltwidget.Widget as WParagraph).ParagraphFormat.FrameWidth;
		}
		int num3 = 0;
		for (int num4 = CurrentPage.PageWidgets.Count - 1; num4 >= 2; num4--)
		{
			if (bottom >= CurrentPage.PageWidgets[num4].Bounds.Y && num2 > CurrentPage.PageWidgets[num4].Bounds.X)
			{
				if (CurrentSection.Columns.Count <= 1)
				{
					m_usedHeight -= CurrentPage.PageWidgets[CurrentPage.PageWidgets.Count - 1].Bounds.Bottom - y;
					m_sectionFixedHeight = 0f;
				}
				InterSectingPoint[0] = num4 - 2;
				for (int j = num4; j < CurrentPage.PageWidgets.Count; j++)
				{
					CurrentPage.PageWidgets[j].Widget.InitLayoutInfo();
				}
				ltwidget = CurrentPage.PageWidgets[num4];
				CurrentPage.PageWidgets.RemoveRange(num4, CurrentPage.PageWidgets.Count - num4);
				num3++;
				break;
			}
		}
		if (InterSectingPoint[0] != int.MinValue)
		{
			for (int num5 = ltwidget.ChildWidgets.Count - 1; num5 >= 0; num5--)
			{
				if (m_dynamicTable != ltwidget.ChildWidgets[num5].Widget && !IsFloatingTextBodyItem(ltwidget.ChildWidgets[num5].Widget) && y >= ltwidget.ChildWidgets[num5].Bounds.Bottom && num2 > ltwidget.ChildWidgets[num5].Bounds.X && num3 < 4)
				{
					InterSectingPoint[num3] = num5;
					ltwidget = ltwidget.ChildWidgets[num5];
					num5 = ltwidget.ChildWidgets.Count;
					if (ltwidget.Widget is WTable || (ltwidget.ChildWidgets.Count > 0 && (ltwidget.ChildWidgets[0].Widget is ParagraphItem || ltwidget.ChildWidgets[0].Widget is SplitStringWidget)))
					{
						break;
					}
					num3++;
				}
			}
		}
		for (int k = 0; k < 4; k++)
		{
			if (InterSectingPoint[k] < 0)
			{
				InterSectingPoint[k] = 0;
			}
		}
		if (m_dynamicTable == null)
		{
			ltwidget = MaintainltWidget;
			while (!(ltwidget.Widget is WParagraph) && (!(ltwidget.Widget is SplitWidgetContainer) || !((ltwidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)))
			{
				ltwidget = ltwidget.ChildWidgets[ltwidget.ChildWidgets.Count - 1];
			}
			m_dynamicParagraph = ((ltwidget.Widget is WParagraph) ? (ltwidget.Widget as WParagraph) : ((ltwidget.Widget is SplitWidgetContainer && (ltwidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) ? ((ltwidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null));
			if (num3 == 0)
			{
				InterSectingPoint[0] = (InterSectingPoint[1] = (InterSectingPoint[2] = (InterSectingPoint[3] = 0)));
				m_usedHeight -= CurrentPage.PageWidgets[CurrentPage.PageWidgets.Count - 1].Bounds.Bottom - y;
				CurrentPage.PageWidgets[2].Widget.InitLayoutInfo();
				CurrentPage.PageWidgets.RemoveAt(2);
			}
		}
		if (FloatingItems.Count > 0)
		{
			for (int l = 0; l < FloatingItems.Count - 1; l++)
			{
				FloatingItem floatingItem = FloatingItems[l];
				if (floatingItem.FloatingEntity is ParagraphItem)
				{
					ParagraphItem paragraphItem = floatingItem.FloatingEntity as ParagraphItem;
					if ((floatingItem.FloatingEntity as ParagraphItem).GetVerticalOrigin() == VerticalOrigin.Paragraph && paragraphItem.IsWrappingBoundsAdded() && paragraphItem.GetVerticalPosition() >= 0f)
					{
						paragraphItem.SetIsWrappingBoundsAdded(boolean: false);
						paragraphItem.OwnerParagraph.IsFloatingItemsLayouted = false;
						FloatingItems.RemoveAt(l);
						l--;
					}
				}
			}
		}
		if (CurrentPage.TrackChangesMarkups.Count > 0)
		{
			for (int m = 0; m < CurrentPage.TrackChangesMarkups.Count; m++)
			{
				TrackChangesMarkups trackChangesMarkups = CurrentPage.TrackChangesMarkups[m];
				if (trackChangesMarkups is CommentsMarkups)
				{
					CommentsMarkups commentsMarkups = trackChangesMarkups as CommentsMarkups;
					if (commentsMarkups.Comment.OwnerParagraph == null || !commentsMarkups.Comment.OwnerParagraph.IsInCell || !(commentsMarkups.Comment.OwnerParagraph.GetOwnerEntity() is WTableCell { OwnerRow: not null } wTableCell) || wTableCell.OwnerRow.OwnerTable == null || !wTableCell.OwnerRow.OwnerTable.TableFormat.WrapTextAround)
					{
						CurrentPage.TrackChangesMarkups.RemoveAt(m);
						m--;
					}
				}
			}
		}
		if (CurrentPage.PageWidgets.Count == 2 && InterSectingPoint[1] == 0)
		{
			IsForceFitLayout = true;
		}
	}

	private bool IsFloatingTextBodyItem(IWidget widget)
	{
		if ((widget is WTable && (widget as WTable).TableFormat.WrapTextAround) || (widget is WParagraph && (widget as WParagraph).ParagraphFormat.IsFrame && (widget as WParagraph).ParagraphFormat.WrapFrameAround != FrameWrapMode.None))
		{
			return true;
		}
		return false;
	}

	bool ILayoutProcessHandler.HandleSplittedWidget(SplitWidgetContainer stWidgetContainer, LayoutState state, LayoutedWidget ltWidget, ref bool isLayoutedWidgetNeedToPushed)
	{
		if (stWidgetContainer == null)
		{
			throw new ArgumentNullException("stWidgetContainer");
		}
		if (stWidgetContainer.Count < 1)
		{
			throw new DLSException("Split widget container (document) must contains at last one child element!");
		}
		IWidget widget = stWidgetContainer[0];
		IWSection iWSection = widget as IWSection;
		if (iWSection == null && widget is SplitWidgetContainer)
		{
			iWSection = (widget as SplitWidgetContainer).m_currentChild as IWSection;
		}
		bool flag = IsContinueLayoutingNextSection(iWSection, ltWidget, isLayoutedWidgetNeedToPushed);
		if ((!flag && iWSection != null) & isLayoutedWidgetNeedToPushed)
		{
			return false;
		}
		if (iWSection == null)
		{
			for (SplitWidgetContainer splitWidgetContainer = widget as SplitWidgetContainer; splitWidgetContainer != null; splitWidgetContainer = splitWidgetContainer.m_currentChild as SplitWidgetContainer)
			{
				if (splitWidgetContainer.RealWidgetContainer is WSection)
				{
					iWSection = splitWidgetContainer.RealWidgetContainer as IWSection;
					break;
				}
			}
		}
		isLayoutedWidgetNeedToPushed = HandleColumnAndPageBreakInLayoutedWidget(ltWidget, isLayoutedWidgetNeedToPushed, flag);
		if (iWSection == null)
		{
			throw new DLSException("Child of SplitWidgetContainer object can't support ISecton interface!");
		}
		if (m_currSection != iWSection)
		{
			if (IsFirstLayouting && CurrentSection is WSection)
			{
				SectionNumPages[(CurrentSection as WSection).Index] = m_sectionPagesCount;
			}
			m_currSection = iWSection;
			OnNextSection();
		}
		return true;
	}

	private bool IsContinueLayoutingNextSection(IWSection nextSection, LayoutedWidget ltWidget, bool isLayoutedWidgetNeedToPushed)
	{
		bool flag = false;
		if (nextSection != null && isLayoutedWidgetNeedToPushed)
		{
			isLayoutedWidgetNeedToPushed = false;
			m_isContinuousSectionLayouted = true;
			float columnsWidth = m_columnsWidth;
			m_columnIndex = 0;
			m_columnsWidth = 0f;
			for (int i = 0; i < m_columnHasBreakItem.Count; i++)
			{
				if (m_columnHasBreakItem[i])
				{
					m_columnIndex = i + 1;
					m_columnsWidth = m_prevColumnsWidth[i];
					isLayoutedWidgetNeedToPushed = true;
					flag = true;
				}
			}
			if (flag)
			{
				m_columnHeight.Insert(m_columnHeight.Count, ltWidget.Bounds.Height);
				m_columnHasBreakItem.Insert(m_columnHasBreakItem.Count, item: false);
				m_prevColumnsWidth.Insert(m_prevColumnsWidth.Count, columnsWidth);
				UpdateSectionHeight(isLastcolumOfCurrentPage: false);
			}
		}
		else if ((m_columnIndex == CurrentSection.Columns.Count || IsEndPage) && isLayoutedWidgetNeedToPushed)
		{
			isLayoutedWidgetNeedToPushed = true;
			flag = true;
			float columnsWidth2 = m_columnsWidth;
			m_columnHeight.Insert(m_columnHeight.Count, ltWidget.Bounds.Height);
			m_columnHasBreakItem.Insert(m_columnHasBreakItem.Count, item: false);
			m_prevColumnsWidth.Insert(m_prevColumnsWidth.Count, columnsWidth2);
			UpdateSectionHeight(isLastcolumOfCurrentPage: true);
		}
		return flag;
	}

	private bool HandleColumnAndPageBreakInLayoutedWidget(LayoutedWidget ltWidget, bool isLayoutedWidgetNeedToPushed, bool isContinueNextSection)
	{
		if (CurrentPage.PageWidgets.Count >= 2 && isLayoutedWidgetNeedToPushed && !m_isContinuousSectionLayouted)
		{
			isLayoutedWidgetNeedToPushed = false;
			LayoutedWidget layoutedWidget = ltWidget;
			while (layoutedWidget.ChildWidgets.Count != 0)
			{
				layoutedWidget = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
			}
			if (layoutedWidget.Widget is Break)
			{
				if ((layoutedWidget.Widget as Break).BreakType == BreakType.PageBreak || (layoutedWidget.Widget as Break).BreakType == BreakType.ColumnBreak)
				{
					isLayoutedWidgetNeedToPushed = true;
					m_columnHeight.Insert(m_columnIndex - 1, ltWidget.Bounds.Height);
					m_columnHasBreakItem.Insert(m_columnIndex - 1, item: true);
					m_prevColumnsWidth.Insert(m_columnIndex - 1, m_columnsWidth);
					if ((layoutedWidget.Widget as Break).BreakType == BreakType.PageBreak)
					{
						m_createNewPage = true;
						MaintainltWidget.ChildWidgets.Clear();
					}
				}
			}
			else
			{
				m_columnHeight.Insert(m_columnIndex - 1, ltWidget.Bounds.Height);
				m_columnHasBreakItem.Insert(m_columnIndex - 1, item: false);
				m_prevColumnsWidth.Insert(m_columnIndex - 1, m_columnsWidth);
			}
		}
		else if (!isContinueNextSection)
		{
			isLayoutedWidgetNeedToPushed = false;
		}
		if (CurrentSection.Columns.Count > 1 && (m_columnIndex == CurrentSection.Columns.Count || IsEndPage))
		{
			isLayoutedWidgetNeedToPushed = true;
		}
		return isLayoutedWidgetNeedToPushed;
	}

	private void UpdateSectionHeight(bool isLastcolumOfCurrentPage)
	{
		m_sectionFixedHeight = 0f;
		foreach (float item in m_columnHeight)
		{
			m_sectionFixedHeight = Math.Max(m_sectionFixedHeight, Math.Min(m_clientHeight, item));
		}
		if (m_usedHeight + m_sectionFixedHeight + 10f >= m_clientHeight || isLastcolumOfCurrentPage)
		{
			m_createNewPage = true;
			MaintainltWidget.ChildWidgets.Clear();
		}
	}

	void ILayoutProcessHandler.HandleLayoutedWidget(LayoutedWidget ltWidget)
	{
		m_totalHeight += ltWidget.Bounds.Height;
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			GetLinesHeight(ltWidget.ChildWidgets[i]);
		}
	}

	private void GetLinesHeight(LayoutedWidget ltWidget)
	{
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[i];
			if (layoutedWidget.Widget is WParagraph || (i == 0 && layoutedWidget.Widget is SplitWidgetContainer && (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) || layoutedWidget.Widget is WTable)
			{
				if (layoutedWidget.Widget is WTable && (layoutedWidget.Widget as WTable).TableFormat.WrapTextAround)
				{
					float num = 0f;
					if (m_absolutePositionedTableHeights == null)
					{
						m_absolutePositionedTableHeights = new List<float>();
					}
					if ((layoutedWidget.Widget as WTable).TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph)
					{
						num = (layoutedWidget.Widget as WTable).TableFormat.Positioning.VertPosition;
					}
					m_absolutePositionedTableHeights.Add(layoutedWidget.Bounds.Height + num);
					continue;
				}
				for (int j = 0; j < layoutedWidget.ChildWidgets.Count; j++)
				{
					IWidget widget;
					if (!(layoutedWidget.Widget is SplitWidgetContainer))
					{
						widget = layoutedWidget.Widget;
					}
					else
					{
						IWidget realWidgetContainer = (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer;
						widget = realWidgetContainer;
					}
					IWidget widget2 = widget;
					float num2 = layoutedWidget.ChildWidgets[j].Bounds.Height;
					if (widget2 is WParagraph)
					{
						bool widowControl = (widget2 as WParagraph).ParagraphFormat.WidowControl;
						if (num2 == 0f && (widget2 as WParagraph).SectionEndMark && (widget2 as WParagraph).IsEmptyParagraph())
						{
							num2 = widget2.LayoutInfo.Size.Height;
						}
						if (widget2.LayoutInfo is ParagraphLayoutInfo)
						{
							if (j == 0)
							{
								num2 += (widget2.LayoutInfo as ParagraphLayoutInfo).Margins.Top;
							}
							if (j == layoutedWidget.ChildWidgets.Count - 1 && !(widget2 as WParagraph).IsEndOfSection)
							{
								num2 += (widget2.LayoutInfo as ParagraphLayoutInfo).Margins.Bottom;
							}
						}
						if (i == 0 && widowControl)
						{
							lineHeigthCount++;
							m_firstParaHeight += num2;
						}
						foreach (LayoutedWidget childWidget in layoutedWidget.ChildWidgets[j].ChildWidgets)
						{
							if (childWidget.Widget is WFootnote)
							{
								m_footnoteHeight += ((FootnoteLayoutInfo)childWidget.Widget.LayoutInfo).FootnoteHeight;
							}
						}
					}
					else if (layoutedWidget.Widget is WTable && layoutedWidget.ChildWidgets[j].Widget is WTableRow wTableRow && wTableRow.m_layoutInfo is RowLayoutInfo)
					{
						num2 += (wTableRow.m_layoutInfo as RowLayoutInfo).Paddings.Bottom;
					}
					m_lineHeights.Insert(m_lineHeights.Count, num2);
				}
			}
			else if (layoutedWidget.Widget is BlockContentControl || (layoutedWidget.Widget is SplitWidgetContainer && (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is BlockContentControl))
			{
				GetLinesHeight(layoutedWidget);
			}
		}
	}

	private int GetColumnIndexForMinColumnWidth()
	{
		int result = 0;
		float width = CurrentSection.Columns[0].Width;
		for (int i = 1; i < CurrentSection.Columns.Count; i++)
		{
			if (CurrentSection.Columns[i].Width < width)
			{
				result = i;
				width = CurrentSection.Columns[i].Width;
			}
		}
		return result;
	}

	private int GetColumnIndexForMaxColumnWidth()
	{
		int result = 0;
		float width = CurrentSection.Columns[0].Width;
		for (int i = 1; i < CurrentSection.Columns.Count; i++)
		{
			if (CurrentSection.Columns[i].Width > width)
			{
				result = i;
				width = CurrentSection.Columns[i].Width;
			}
		}
		return result;
	}

	internal bool HeaderGetNextArea(out RectangleF area)
	{
		area = ((CurrentPage.PageWidgets.Count != 0) ? RectangleF.Empty : CurrentPage.GetHeaderArea());
		return !area.Equals(RectangleF.Empty);
	}

	internal void HeaderPushLayoutedWidget(LayoutedWidget ltWidget)
	{
		float footerHeight = Math.Max(ltWidget.Bounds.Height + ((CurrentPage.Setup.HeaderDistance != -0.05f) ? CurrentPage.Setup.HeaderDistance : 36f), CurrentPage.Setup.Margins.Top);
		ltWidget.ShiftLocation(0.0, ltWidget.Bounds.Height, footerHeight, CurrentPage.Setup.PageSize.Height, isHeader: true, this);
		CurrentPage.PageWidgets.Add(ltWidget);
	}

	internal void FootnotePushLayoutedWidget(RectangleF layoutArea)
	{
		float num = CurrentPage.FootnoteWidgets[CurrentPage.FootnoteWidgets.Count - 1].Bounds.Bottom - layoutArea.Y;
		if (CurrentSection.PageSetup.FootnotePosition == FootnotePosition.PrintImmediatelyBeneathText)
		{
			num = 0f;
		}
		for (int i = m_footnoteCount; i < CurrentPage.FootnoteWidgets.Count; i++)
		{
			CurrentPage.FootnoteWidgets[i].ShiftLocation(0.0, layoutArea.Height - num, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
		}
		if (m_currSection.Columns.Count > 1)
		{
			m_footnoteCount = m_currPage.FootnoteWidgets.Count;
		}
		if (m_columnIndex == m_currSection.Columns.Count || m_currSection.Columns.Count == 1)
		{
			m_footnoteCount = 0;
		}
	}

	internal void EndnotePushLayoutedWidget(RectangleF layoutArea, LayoutedWidget ltWidget)
	{
		Entity entity = ltWidget.Widget as Entity;
		if (ltWidget.Widget is SplitWidgetContainer)
		{
			entity = (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as Entity;
		}
		float num = 0f;
		WSection section = ((ltWidget.ChildWidgets[0].Widget is SplitWidgetContainer) ? ((ltWidget.ChildWidgets[0].Widget as SplitWidgetContainer).RealWidgetContainer as WSection) : (ltWidget.ChildWidgets[0].Widget as WSection));
		int num2 = 0;
		if (entity != null)
		{
			num2 = entity.Document.Sections.IndexOf(section);
		}
		for (int i = 0; i < CurrentPage.FootnoteWidgets.Count; i++)
		{
			if (num2 == CurrentPage.FootNoteSectionIndex[i])
			{
				num += CurrentPage.FootnoteWidgets[i].Bounds.Height;
			}
		}
		for (int j = m_endnoteCount; j < CurrentPage.EndnoteWidgets.Count; j++)
		{
			if (entity != null && entity.Document.EndnotePosition == EndnotePosition.DisplayEndOfSection)
			{
				if (num2 == CurrentPage.EndNoteSectionIndex[j])
				{
					CurrentPage.EndnoteWidgets[j].ShiftLocation(0.0, layoutArea.Height + num, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
				}
			}
			else
			{
				CurrentPage.EndnoteWidgets[j].ShiftLocation(0.0, layoutArea.Height + num, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
			}
		}
		if (m_currSection.Columns.Count > 1)
		{
			m_endnoteCount = m_currPage.EndnoteWidgets.Count;
		}
		if (m_columnIndex == m_currSection.Columns.Count || m_currSection.Columns.Count == 1)
		{
			m_endnoteCount = 0;
		}
	}

	internal bool FooterGetNextArea(out RectangleF area)
	{
		area = ((CurrentPage.PageWidgets.Count != 1) ? RectangleF.Empty : CurrentPage.GetFooterArea());
		return !area.Equals(RectangleF.Empty);
	}

	internal void FooterPushLayoutedWidget(LayoutedWidget ltWidget)
	{
		float footerHeight = Math.Max(ltWidget.Bounds.Height + ((CurrentPage.Setup.FooterDistance != -0.05f) ? CurrentPage.Setup.FooterDistance : 36f), CurrentPage.Setup.Margins.Bottom);
		RectangleF bounds = ltWidget.Bounds;
		if (ltWidget.ChildWidgets.Count >= 1 && ltWidget.ChildWidgets[0].Widget is WTable && ltWidget.ChildWidgets[0].Bounds.Y >= ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].Bounds.Bottom && (ltWidget.ChildWidgets[0].Widget as WTable).Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
		{
			RowFormat tableFormat = (ltWidget.ChildWidgets[0].Widget as WTable).TableFormat;
			if (tableFormat.WrapTextAround && tableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph)
			{
				bounds.Height += ltWidget.ChildWidgets[0].Bounds.Height;
			}
		}
		ltWidget.ShiftLocation(0.0, 0f - bounds.Height, footerHeight, CurrentPage.Setup.PageSize.Height, isHeader: false, this);
		CurrentPage.PageWidgets.Add(ltWidget);
	}
}
