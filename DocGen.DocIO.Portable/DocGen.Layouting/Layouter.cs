using System.Collections.Generic;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal class Layouter : ILCOperator
{
	public delegate void LeafLayoutEventHandler(object sender, LayoutedWidget ltWidget, bool isFromTOCLinkStyle);

	private DrawingContext m_drawingContext;

	internal RectangleF m_clientLayoutArea;

	private float m_pageTop;

	private List<SplitWidgetContainer> m_footnoteSplittedWidgets = new List<SplitWidgetContainer>();

	private List<SplitWidgetContainer> m_endnoteSplittedWidgets;

	private List<Entity> m_endnotesInstance;

	private TabsLayoutInfo.LayoutTab m_previousTab;

	private float m_previousTabWidth;

	private RectangleF m_frameLayoutArea;

	private RectangleF m_frameBounds;

	private float m_wrappingDifference = float.MinValue;

	private bool m_isLayoutingTableHeight;

	private bool m_isSkipBottomForFrame;

	private float m_rightPositionOfTabStopInterSectingFloattingItems = float.MinValue;

	private float m_maxRightPositionOfTabStopInterSectingFloattingItems = float.MinValue;

	private float m_frameHeight;

	internal float m_firstItemInPageYPosition;

	private ushort m_bFlags = 286;

	private byte m_byteFlag;

	private TableOfContent m_layoutingTOC;

	internal IEntity m_fieldentity;

	private int m_currentColumnIndex;

	private int m_countForConsecutiveLimit;

	private float m_ParagraphYPosition;

	internal List<float> m_lineSpaceWidths;

	internal float m_effectiveJustifyWidth;

	private WField m_unknownField;

	private bool m_isLayoutTrackChanges;

	private float m_hiddenLineBottom;

	internal float HiddenLineBottom
	{
		get
		{
			return m_hiddenLineBottom;
		}
		set
		{
			m_hiddenLineBottom = value;
		}
	}

	internal RectangleF FrameBounds
	{
		get
		{
			return m_frameBounds;
		}
		set
		{
			m_frameBounds = value;
		}
	}

	internal bool IsLayoutingVerticalMergeStartCell
	{
		get
		{
			return (m_byteFlag & 1) != 0;
		}
		set
		{
			m_byteFlag = (byte)((m_byteFlag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsNeedToRestartFootnote
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool m_canSplitbyCharacter
	{
		get
		{
			return (m_bFlags & 0x800) >> 11 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xF7FFu) | ((value ? 1u : 0u) << 11));
		}
	}

	internal bool m_canSplitByTab
	{
		get
		{
			return (m_bFlags & 0x1000) >> 12 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xEFFFu) | ((value ? 1u : 0u) << 12));
		}
	}

	internal bool IsNeedToRestartEndnote
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsNeedToRestartFootnoteID
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool IsNeedToRestartEndnoteID
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal int CurrPageIndex => (this.LeafLayoutAfter.Target as DocumentLayouter).Pages.Count;

	internal bool IsNeedToRelayout
	{
		get
		{
			return (m_byteFlag & 2) >> 1 != 0;
		}
		set
		{
			m_byteFlag = (byte)((m_byteFlag & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool IsWord2013WordFitLayout
	{
		get
		{
			return (m_byteFlag & 4) >> 2 != 0;
		}
		set
		{
			m_byteFlag = (byte)((m_byteFlag & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsNeedToRelayoutTable
	{
		get
		{
			return (m_byteFlag & 8) >> 3 != 0;
		}
		set
		{
			m_byteFlag = (byte)((m_byteFlag & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal int PageNumber => (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.Number;

	internal float RemovedWidgetsHeight
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).RemovedWidgetsHeight;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).RemovedWidgetsHeight = value;
		}
	}

	internal bool IsRowFitInSamePage
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsLayoutingHeaderRow
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool IsLayoutingTrackChanges
	{
		get
		{
			return m_isLayoutTrackChanges;
		}
		set
		{
			m_isLayoutTrackChanges = value;
		}
	}

	internal bool AtLeastOneChildFitted
	{
		get
		{
			return (m_byteFlag & 0x10) >> 4 != 0;
		}
		set
		{
			m_byteFlag = (byte)((m_byteFlag & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal IEntity FieldEntity
	{
		get
		{
			return m_fieldentity;
		}
		set
		{
			m_fieldentity = value;
		}
	}

	public DrawingContext DrawingContext
	{
		get
		{
			return m_drawingContext;
		}
		set
		{
			m_drawingContext = value;
		}
	}

	internal WField UnknownField
	{
		get
		{
			return m_unknownField;
		}
		set
		{
			m_unknownField = value;
		}
	}

	internal RectangleF ClientLayoutArea => m_clientLayoutArea;

	internal float PageTopMargin => m_pageTop;

	internal RectangleF FrameLayoutArea
	{
		get
		{
			_ = m_frameLayoutArea;
			return m_frameLayoutArea;
		}
		set
		{
			m_frameLayoutArea = value;
		}
	}

	internal float FrameHeight
	{
		get
		{
			return m_frameHeight;
		}
		set
		{
			m_frameHeight = value;
		}
	}

	internal bool IsSkipBottomForFrame
	{
		get
		{
			return m_isSkipBottomForFrame;
		}
		set
		{
			m_isSkipBottomForFrame = value;
		}
	}

	internal bool IsLayoutingHeaderFooter
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool IsLayoutingHeader
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFF7Fu) | (value ? 128u : 0u));
		}
	}

	internal bool IsFirstItemInLine
	{
		get
		{
			return (m_bFlags & 0x200) >> 9 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFDFFu) | ((value ? 1u : 0u) << 9));
		}
	}

	internal bool IsLayoutingTableHeight
	{
		get
		{
			return m_isLayoutingTableHeight;
		}
		set
		{
			m_isLayoutingTableHeight = value;
		}
	}

	internal bool IsLayoutingFootnote
	{
		get
		{
			return (m_bFlags & 0x400) >> 10 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFBFFu) | ((value ? 1u : 0u) << 10));
		}
	}

	internal float WrappingDifference
	{
		get
		{
			return m_wrappingDifference;
		}
		set
		{
			m_wrappingDifference = value;
		}
	}

	internal float MaxRightPositionOfTabStopInterSectingFloattingItems
	{
		get
		{
			return m_maxRightPositionOfTabStopInterSectingFloattingItems;
		}
		set
		{
			m_maxRightPositionOfTabStopInterSectingFloattingItems = value;
		}
	}

	internal float RightPositionOfTabStopInterSectingFloattingItems
	{
		get
		{
			return m_rightPositionOfTabStopInterSectingFloattingItems;
		}
		set
		{
			m_rightPositionOfTabStopInterSectingFloattingItems = value;
		}
	}

	internal Dictionary<Entity, int> TOCEntryPageNumbers => (this.LeafLayoutAfter.Target as DocumentLayouter).TOCEntryPageNumbers;

	internal List<ParagraphItem> tocParaItems => (this.LeafLayoutAfter.Target as DocumentLayouter).tocParaItems;

	internal List<WParagraph> HiddenParagraphCollection => (this.LeafLayoutAfter.Target as DocumentLayouter).m_hiddenParaCollection;

	internal WParagraph LastTOCParagraph => (this.LeafLayoutAfter.Target as DocumentLayouter).LastTocEntity as WParagraph;

	internal bool UpdatingPageFields => DocumentLayouter.m_UpdatingPageFields;

	internal bool IsNeedtoAdjustFooter
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).m_isNeedtoAdjustFooter;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).m_isNeedtoAdjustFooter = value;
		}
	}

	internal LayoutedWidgetList FootnoteWidgets => (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.FootnoteWidgets;

	internal int FootnoteCount => (this.LeafLayoutAfter.Target as DocumentLayouter).m_footnoteCount;

	internal LayoutedWidgetList LineNumberWidgets => (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.LineNumberWidgets;

	internal LayoutedWidgetList EndnoteWidgets => (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.EndnoteWidgets;

	internal List<TrackChangesMarkups> TrackChangesMarkups => (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.TrackChangesMarkups;

	internal LayoutedWidgetList BehindWidgets => (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.BehindWidgets;

	internal int NumberOfBehindWidgetsInHeader
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.NumberOfBehindWidgetsInHeader;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.NumberOfBehindWidgetsInHeader = value;
		}
	}

	internal int NumberOfBehindWidgetsInFooter
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.NumberOfBehindWidgetsInFooter;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.NumberOfBehindWidgetsInFooter = value;
		}
	}

	internal List<int> EndNoteSectionIndex => (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.EndNoteSectionIndex;

	internal IWSection CurrentSection => (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentSection;

	internal List<int> FootNoteSectionIndex => (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.FootNoteSectionIndex;

	internal List<SplitWidgetContainer> FootnoteSplittedWidgets
	{
		get
		{
			if (m_footnoteSplittedWidgets == null)
			{
				m_footnoteSplittedWidgets = new List<SplitWidgetContainer>();
			}
			return m_footnoteSplittedWidgets;
		}
		set
		{
			m_footnoteSplittedWidgets = value;
		}
	}

	internal List<SplitWidgetContainer> EndnoteSplittedWidgets
	{
		get
		{
			if (m_endnoteSplittedWidgets == null)
			{
				m_endnoteSplittedWidgets = new List<SplitWidgetContainer>();
			}
			return m_endnoteSplittedWidgets;
		}
		set
		{
			m_endnoteSplittedWidgets = value;
		}
	}

	internal List<Entity> EndnotesInstances
	{
		get
		{
			if (m_endnotesInstance == null)
			{
				m_endnotesInstance = new List<Entity>();
			}
			return m_endnotesInstance;
		}
		set
		{
			m_endnotesInstance = value;
		}
	}

	internal List<FloatingItem> FloatingItems => (this.LeafLayoutAfter.Target as DocumentLayouter).FloatingItems;

	internal List<FloatingItem> WrapFloatingItems => (this.LeafLayoutAfter.Target as DocumentLayouter).WrapFloatingItems;

	internal WParagraph DynamicParagraph
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).m_dynamicParagraph;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).m_dynamicParagraph = value;
		}
	}

	internal WTable DynamicTable
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).m_dynamicTable;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).m_dynamicTable = value;
		}
	}

	internal List<Entity> NotFittedFloatingItems
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).m_notFittedfloatingItems;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).m_notFittedfloatingItems = value;
		}
	}

	internal LayoutedWidget MaintainltWidget
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).MaintainltWidget;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).MaintainltWidget = value;
		}
	}

	internal int[] m_interSectingPoint
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).InterSectingPoint;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).InterSectingPoint = value;
		}
	}

	internal IWidgetContainer PageEndWidget
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).m_pageEndWidget;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).m_pageEndWidget = value;
		}
	}

	internal bool IsForceFitLayout
	{
		get
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).IsForceFitLayout;
		}
		set
		{
			(this.LeafLayoutAfter.Target as DocumentLayouter).IsForceFitLayout = value;
		}
	}

	internal TabsLayoutInfo.LayoutTab PreviousTab
	{
		get
		{
			if (m_previousTab == null)
			{
				m_previousTab = new TabsLayoutInfo.LayoutTab();
			}
			return m_previousTab;
		}
		set
		{
			m_previousTab = value;
		}
	}

	internal float PreviousTabWidth
	{
		get
		{
			return m_previousTabWidth;
		}
		set
		{
			m_previousTabWidth = value;
		}
	}

	internal bool IsTabWidthUpdatedBasedOnIndent
	{
		get
		{
			return (m_bFlags & 0x8000) >> 15 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0x7FFFu) | ((value ? 1u : 0u) << 15));
		}
	}

	internal TableOfContent LayoutingTOC
	{
		get
		{
			return m_layoutingTOC;
		}
		set
		{
			m_layoutingTOC = value;
		}
	}

	internal int CurrentColumnIndex
	{
		get
		{
			return m_currentColumnIndex;
		}
		set
		{
			m_currentColumnIndex = value;
		}
	}

	internal int CountForConsecutiveLimit
	{
		get
		{
			return m_countForConsecutiveLimit;
		}
		set
		{
			m_countForConsecutiveLimit = value;
		}
	}

	internal float ParagraphYPosition
	{
		get
		{
			return m_ParagraphYPosition;
		}
		set
		{
			m_ParagraphYPosition = value;
		}
	}

	internal bool IsTwoLinesLayouted
	{
		get
		{
			return (m_bFlags & 0x2000) >> 13 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xDFFFu) | ((value ? 1u : 0u) << 13));
		}
	}

	internal bool IsFootnoteHeightAdjusted
	{
		get
		{
			return (m_bFlags & 0x4000) >> 14 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xBFFFu) | ((value ? 1u : 0u) << 14));
		}
	}

	internal List<float> LineSpaceWidths
	{
		get
		{
			if (m_lineSpaceWidths == null)
			{
				m_lineSpaceWidths = new List<float>();
			}
			return m_lineSpaceWidths;
		}
	}

	public event LeafLayoutEventHandler LeafLayoutAfter;

	public void Layout(IWidgetContainer widget, ILayoutProcessHandler handler, DrawingContext dc)
	{
		IsLayoutingHeaderFooter = widget is HeaderFooter;
		IsLayoutingHeader = IsLayoutingHeaderFooter && (widget as HeaderFooter).Type.ToString().Contains("Header");
		bool isContinuousSection = false;
		bool isSplittedWidget = true;
		IWidgetContainer curWidget = widget;
		List<IWidgetContainer> list = new List<IWidgetContainer>();
		List<LayoutedWidget> list2 = new List<LayoutedWidget>();
		bool isCurrentWidgetNeedToLayout = false;
		int columnIndex = 0;
		bool flag = false;
		m_drawingContext = dc;
		RectangleF rect;
		while (handler.GetNextArea(out rect, ref columnIndex, ref isContinuousSection, isSplittedWidget, ref m_pageTop, flag, ref curWidget))
		{
			CurrentColumnIndex = columnIndex;
			if (rect.Equals(RectangleF.Empty))
			{
				break;
			}
			m_clientLayoutArea = rect;
			m_wrappingDifference = float.MinValue;
			IsNeedToRestartFootnote = true;
			IsNeedToRestartFootnoteID = true;
			IsNeedToRestartEndnote = true;
			IsNeedToRestartEndnoteID = true;
			if (CurrentSection is WSection && (CurrentSection as WSection).IsSectionFitInSamePage)
			{
				IsNeedToRestartFootnote = false;
			}
			else if (columnIndex == 0)
			{
				if (flag && m_interSectingPoint[2] > 0)
				{
					IsNeedToRestartFootnote = false;
				}
				else
				{
					FootnoteWidgets.Clear();
					FootNoteSectionIndex.Clear();
				}
			}
			flag = false;
			LayoutContext layoutContext = LayoutContext.Create(curWidget, this, IsForceFitLayout);
			if (!IsLayoutingHeaderFooter)
			{
				IsForceFitLayout = false;
			}
			layoutContext.ClientLayoutAreaRight = rect.Width;
			LayoutedWidget layoutedWidget = layoutContext.Layout(rect);
			if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
			{
				break;
			}
			if (FootnoteSplittedWidgets.Count == 0)
			{
				IsNeedToRestartFootnoteID = true;
			}
			if (EndnoteSplittedWidgets.Count == 0)
			{
				IsNeedToRestartEndnoteID = true;
			}
			if (layoutContext.State != LayoutState.DynamicRelayout)
			{
				if (!IsLayoutingHeaderFooter && curWidget is SplitWidgetContainer)
				{
					RemoveSplitStringWidget(layoutedWidget);
				}
				IsCurrentWidgetNeedToLayout(layoutContext, columnIndex, ref isSplittedWidget, ref isCurrentWidgetNeedToLayout, isContinuousSection);
				if (isCurrentWidgetNeedToLayout)
				{
					continue;
				}
			}
			if (!IsLayoutingHeaderFooter)
			{
				LayoutTrackChangesBalloon(this);
			}
			if (!isContinuousSection)
			{
				list.Clear();
				list2.Clear();
				handler.PushLayoutedWidget(layoutedWidget, m_clientLayoutArea, IsNeedToRestartFootnoteID, IsNeedToRestartEndnoteID, layoutContext.State, isNeedToFindInterSectingPoint: true, isContinuousSection: false);
			}
			else
			{
				list.Insert(list.Count, curWidget);
				list2.Insert(list2.Count, layoutedWidget);
				if (layoutContext.State == LayoutState.DynamicRelayout)
				{
					int count = list2.Count;
					for (int i = 0; i < count; i++)
					{
						handler.PushLayoutedWidget(list2[0], m_clientLayoutArea, IsNeedToRestartFootnoteID, IsNeedToRestartEndnoteID, layoutContext.State, i == count - 1, isContinuousSection: false);
						list.RemoveAt(0);
						list2.RemoveAt(0);
					}
				}
			}
			if (layoutContext.State == LayoutState.Splitted && isContinuousSection)
			{
				handler.PushLayoutedWidget(list2[0], m_clientLayoutArea, IsNeedToRestartFootnoteID, IsNeedToRestartEndnoteID, layoutContext.State, isNeedToFindInterSectingPoint: true, isContinuousSection: true);
			}
			if ((!layoutContext.IsEnsureSplitted() && (layoutContext.State != LayoutState.NotFitted || layoutContext.SplittedWidget == null || !(layoutContext.SplittedWidget is SplitWidgetContainer) || (!((layoutContext.SplittedWidget as SplitWidgetContainer)[0] is WSection) && (!((layoutContext.SplittedWidget as SplitWidgetContainer)[0] is SplitWidgetContainer) || !(((layoutContext.SplittedWidget as SplitWidgetContainer)[0] as SplitWidgetContainer).RealWidgetContainer is WSection)))) && layoutContext.State != LayoutState.DynamicRelayout) || IsLayoutingHeaderFooter)
			{
				break;
			}
			if (layoutContext.m_ltState == LayoutState.DynamicRelayout && m_interSectingPoint[0] != int.MinValue)
			{
				if (m_interSectingPoint[0] == 0)
				{
					curWidget = MaintainltWidget.ChildWidgets[m_interSectingPoint[0]].Widget as IWidgetContainer;
				}
				else if (m_interSectingPoint[0] != int.MinValue)
				{
					curWidget.InitLayoutInfo(MaintainltWidget.ChildWidgets[m_interSectingPoint[0]].ChildWidgets[m_interSectingPoint[1]].Widget);
					curWidget = ((curWidget is SplitWidgetContainer && (curWidget as SplitWidgetContainer).m_currentChild is SplitWidgetContainer && ((curWidget as SplitWidgetContainer).m_currentChild as SplitWidgetContainer).RealWidgetContainer is WordDocument) ? ((curWidget as SplitWidgetContainer).m_currentChild as IWidgetContainer) : curWidget);
				}
				PageEndWidget = layoutContext.SplittedWidget as IWidgetContainer;
				flag = true;
				RemoveLayoutedFootnoteWidget();
				(this.LeafLayoutAfter.Target as DocumentLayouter).IsCreateNewPage = false;
				layoutContext.m_ltState = LayoutState.Unknown;
				(this.LeafLayoutAfter.Target as DocumentLayouter).ResetTheLayoutedFootnotes(FootnoteWidgets, EndnoteWidgets);
				continue;
			}
			if (layoutContext.m_ltState == LayoutState.DynamicRelayout)
			{
				(this.LeafLayoutAfter.Target as DocumentLayouter).ResetTheLayoutedFootnotes(FootnoteWidgets, EndnoteWidgets);
				layoutContext.m_ltState = LayoutState.Unknown;
			}
			if (layoutContext.State == LayoutState.NotFitted && layoutContext.SplittedWidget != null)
			{
				(this.LeafLayoutAfter.Target as DocumentLayouter).IsCreateNewPage = true;
			}
			SplitWidgetContainer splitWidgetContainer = layoutContext.SplittedWidget as SplitWidgetContainer;
			bool isLayoutedWidgetNeedToPushed = isContinuousSection;
			if (handler.HandleSplittedWidget(splitWidgetContainer, layoutContext.State, layoutedWidget, ref isLayoutedWidgetNeedToPushed))
			{
				if (isLayoutedWidgetNeedToPushed && isContinuousSection)
				{
					int count2 = list2.Count;
					for (int j = 0; j < count2; j++)
					{
						handler.PushLayoutedWidget(list2[0], m_clientLayoutArea, IsNeedToRestartFootnoteID, IsNeedToRestartEndnoteID, layoutContext.State, isNeedToFindInterSectingPoint: true, isContinuousSection: false);
						list.RemoveAt(0);
						list2.RemoveAt(0);
					}
				}
				else if (isContinuousSection)
				{
					handler.HandleLayoutedWidget(layoutedWidget);
				}
				curWidget = splitWidgetContainer;
			}
			else
			{
				handler.HandleLayoutedWidget(layoutedWidget);
				curWidget = list[0];
			}
		}
	}

	private void RemoveLayoutedFootnoteWidget()
	{
		if (m_interSectingPoint[0] == int.MinValue || m_interSectingPoint[1] == int.MinValue || m_interSectingPoint[2] == int.MinValue || m_interSectingPoint[3] == int.MinValue)
		{
			return;
		}
		LayoutedWidget layoutedWidget = ((m_interSectingPoint[0] < MaintainltWidget.ChildWidgets.Count && m_interSectingPoint[1] < MaintainltWidget.ChildWidgets[m_interSectingPoint[0]].ChildWidgets.Count) ? MaintainltWidget.ChildWidgets[m_interSectingPoint[0]].ChildWidgets[m_interSectingPoint[1]] : null);
		LayoutedWidget layoutedWidget2 = ((layoutedWidget != null && m_interSectingPoint[2] < layoutedWidget.ChildWidgets.Count) ? layoutedWidget.ChildWidgets[m_interSectingPoint[2]] : null);
		if (FootnoteWidgets.Count <= 0 || layoutedWidget == null || layoutedWidget2 == null || m_interSectingPoint[2] <= 0)
		{
			return;
		}
		IWidget widget;
		if (!(layoutedWidget2.Widget is SplitWidgetContainer))
		{
			if (!(layoutedWidget2.Widget is SplitTableWidget))
			{
				widget = layoutedWidget2.Widget;
			}
			else
			{
				IWidget tableWidget = (layoutedWidget2.Widget as SplitTableWidget).TableWidget;
				widget = tableWidget;
			}
		}
		else
		{
			IWidget tableWidget = (layoutedWidget2.Widget as SplitWidgetContainer).RealWidgetContainer;
			widget = tableWidget;
		}
		if (!(widget is Entity entity))
		{
			return;
		}
		WSection wSection = ((entity.GetOwnerSection(entity) is WSection) ? (entity.GetOwnerSection(entity) as WSection) : null);
		int index = entity.Index;
		for (int num = FootnoteWidgets.Count - 1; num >= 0; num--)
		{
			WTextBody wTextBody = ((FootnoteWidgets[num].Widget is WTextBody) ? (FootnoteWidgets[num].Widget as WTextBody) : ((FootnoteWidgets[num].Widget as SplitWidgetContainer).RealWidgetContainer as WTextBody));
			Entity entity2 = wTextBody.Owner as WFootnote;
			int num2 = int.MinValue;
			while (entity2 != null && !(entity2 is WSection))
			{
				if (!(entity2 is WTextBody))
				{
					num2 = entity2.Index;
				}
				entity2 = entity2.Owner;
			}
			if (entity2 is WSection && (wSection != entity2 as WSection || (num2 != int.MinValue && num2 < index && wSection == entity2 as WSection)))
			{
				break;
			}
			if (entity2 is WSection && num2 != int.MinValue && num2 > index && wSection == entity2 as WSection)
			{
				FootnoteWidgets.RemoveAt(num);
				(wTextBody.Owner as WFootnote).IsLayouted = false;
			}
			else if (entity2 is WSection && num2 != int.MinValue && num2 == index && wSection == entity2 as WSection)
			{
				for (int i = 0; i < layoutedWidget2.ChildWidgets.Count; i++)
				{
					LayoutedWidget layoutedWidget3 = layoutedWidget2.ChildWidgets[i];
					for (int j = 0; j < layoutedWidget3.ChildWidgets.Count; j++)
					{
						Entity entity3 = layoutedWidget3.ChildWidgets[j].Widget as Entity;
						if (entity3 is WFootnote && entity3 as WFootnote == wTextBody.Owner as WFootnote && i >= m_interSectingPoint[3])
						{
							FootnoteWidgets.RemoveAt(num);
							(wTextBody.Owner as WFootnote).IsLayouted = false;
						}
					}
				}
			}
		}
	}

	private void RemoveSplitStringWidget(LayoutedWidget ltwidget)
	{
		LayoutedWidget layoutedWidget = ltwidget;
		LayoutedWidget layoutedWidget2 = ltwidget;
		while (layoutedWidget.ChildWidgets.Count > 0 && !(layoutedWidget.Widget is WParagraph) && (!(layoutedWidget.Widget is SplitWidgetContainer) || !((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)))
		{
			layoutedWidget = layoutedWidget.ChildWidgets[0];
		}
		WParagraph wParagraph = ((layoutedWidget.Widget is WParagraph) ? (layoutedWidget.Widget as WParagraph) : ((layoutedWidget.Widget is SplitWidgetContainer && (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) ? ((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null));
		while (layoutedWidget2.ChildWidgets.Count > 0 && !(layoutedWidget2.Widget is WParagraph) && (!(layoutedWidget2.Widget is SplitWidgetContainer) || !((layoutedWidget2.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)))
		{
			layoutedWidget2 = layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1];
		}
		WParagraph wParagraph2 = ((layoutedWidget2.Widget is WParagraph) ? (layoutedWidget2.Widget as WParagraph) : ((layoutedWidget2.Widget is SplitWidgetContainer && (layoutedWidget2.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) ? ((layoutedWidget2.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null));
		if (wParagraph != null && wParagraph2 != null && wParagraph != wParagraph2 && wParagraph.GetOwnerEntity() is WSection)
		{
			wParagraph.RemoveSplitStringWidget();
		}
	}

	private void LayoutTrackChangesBalloon(Layouter layouter)
	{
		if (layouter.TrackChangesMarkups.Count <= 0)
		{
			return;
		}
		List<FloatingItem> list = null;
		if (layouter.FloatingItems.Count > 0)
		{
			list = new List<FloatingItem>();
			foreach (FloatingItem floatingItem in layouter.FloatingItems)
			{
				list.Add(floatingItem);
			}
			layouter.FloatingItems.Clear();
		}
		layouter.IsLayoutingTrackChanges = true;
		List<TrackChangesMarkups> list2 = new List<TrackChangesMarkups>();
		for (int i = 0; i < layouter.TrackChangesMarkups.Count; i++)
		{
			TrackChangesMarkups trackChangesMarkups = layouter.TrackChangesMarkups[i];
			trackChangesMarkups.LtWidget = GetBalloonLayoutedWidget(trackChangesMarkups.ChangedValue, trackChangesMarkups.BallonYPosition, list2, layouter);
			trackChangesMarkups.LtWidget.Bounds = new RectangleF(trackChangesMarkups.LtWidget.Bounds.X, trackChangesMarkups.LtWidget.Bounds.Y, trackChangesMarkups.LtWidget.Bounds.Width, trackChangesMarkups.LtWidget.Bounds.Height + 1f);
			AdjustBalloonPosition(trackChangesMarkups, list2, trackChangesMarkups.BallonYPosition, layouter);
			list2.Add(trackChangesMarkups);
		}
		list2.Clear();
		list2 = null;
		layouter.IsLayoutingTrackChanges = false;
		if (list == null)
		{
			return;
		}
		foreach (FloatingItem item in list)
		{
			layouter.FloatingItems.Add(item);
		}
		list.Clear();
		list = null;
	}

	private LayoutedWidget GetBalloonLayoutedWidget(WTextBody changedText, float yPosition, List<TrackChangesMarkups> updatedTrackChangesMarkups, Layouter layouter)
	{
		float num = ((updatedTrackChangesMarkups.Count < 1) ? yPosition : GetBalloonYposition(yPosition, updatedTrackChangesMarkups, layouter));
		if (updatedTrackChangesMarkups.Count > 0)
		{
			num += 3f;
		}
		RectangleF rect = new RectangleF(layouter.CurrentSection.PageSetup.PageSize.Width - layouter.CurrentSection.PageSetup.Margins.Right + 32f, num, 200f, layouter.ClientLayoutArea.Height);
		bool canSplitbyCharacter = layouter.m_canSplitbyCharacter;
		bool canSplitByTab = layouter.m_canSplitByTab;
		bool isFirstItemInLine = layouter.IsFirstItemInLine;
		TabsLayoutInfo.LayoutTab previousTab = layouter.PreviousTab;
		float previousTabWidth = layouter.PreviousTabWidth;
		LayoutContext layoutContext = LayoutContext.Create(changedText, layouter, IsForceFitLayout);
		layoutContext.ClientLayoutAreaRight = rect.Width;
		float paragraphYPosition = layouter.ParagraphYPosition;
		LayoutedWidget result = layoutContext.Layout(rect);
		layouter.ParagraphYPosition = paragraphYPosition;
		layouter.PreviousTab = previousTab;
		layouter.PreviousTabWidth = previousTabWidth;
		layouter.m_canSplitbyCharacter = canSplitbyCharacter;
		layouter.m_canSplitByTab = canSplitByTab;
		layouter.IsFirstItemInLine = isFirstItemInLine;
		return result;
	}

	private float GetBalloonYposition(float yPosition, List<TrackChangesMarkups> updatedTrackChangesMarkups, Layouter layouter)
	{
		TrackChangesMarkups trackChangesMarkups = updatedTrackChangesMarkups[updatedTrackChangesMarkups.Count - 1];
		if (yPosition < trackChangesMarkups.LtWidget.Bounds.Bottom)
		{
			if (layouter.ClientLayoutArea.Bottom > yPosition)
			{
				yPosition = trackChangesMarkups.LtWidget.Bounds.Bottom;
			}
			else
			{
				float num = trackChangesMarkups.LtWidget.Bounds.Bottom - yPosition;
				for (int num2 = updatedTrackChangesMarkups.Count - 1; num2 >= 0; num2--)
				{
					trackChangesMarkups = updatedTrackChangesMarkups[num2];
					if (trackChangesMarkups.EmptySpace == float.MinValue)
					{
						yPosition = trackChangesMarkups.LtWidget.Bounds.Bottom;
						break;
					}
					if (trackChangesMarkups.EmptySpace > 0f)
					{
						float num3 = ((num < trackChangesMarkups.EmptySpace) ? num : trackChangesMarkups.EmptySpace);
						num -= num3;
						for (int i = num2; i < updatedTrackChangesMarkups.Count; i++)
						{
							LayoutedWidget ltWidget = updatedTrackChangesMarkups[i].LtWidget;
							ltWidget.ShiftLocation(0.0, 0f - num3, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: false);
							updatedTrackChangesMarkups[i].LtWidget = ltWidget;
							updatedTrackChangesMarkups[i].EmptySpace -= num3;
						}
					}
					if (num == 0f)
					{
						break;
					}
				}
				yPosition += num;
			}
		}
		return yPosition;
	}

	private void AdjustBalloonPosition(TrackChangesMarkups trackChangesMarkups, List<TrackChangesMarkups> updatedTrackChangesMarkups, float clientAreaY, Layouter layouter)
	{
		if (updatedTrackChangesMarkups.Count == 0)
		{
			if (layouter.ClientLayoutArea.Bottom < trackChangesMarkups.LtWidget.Bounds.Bottom)
			{
				float num = trackChangesMarkups.LtWidget.Bounds.Bottom - layouter.ClientLayoutArea.Top;
				float num2 = trackChangesMarkups.LtWidget.Bounds.Top - layouter.ClientLayoutArea.Top;
				num = ((num < num2) ? num : num2);
				trackChangesMarkups.LtWidget.ShiftLocation(0.0, 0f - num, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: false);
			}
			float num3 = clientAreaY - layouter.ClientLayoutArea.Y;
			if (num3 == 0f)
			{
				trackChangesMarkups.EmptySpace = float.MinValue;
			}
			else
			{
				trackChangesMarkups.EmptySpace = num3;
			}
		}
		else
		{
			TrackChangesMarkups trackChangesMarkups2 = updatedTrackChangesMarkups[updatedTrackChangesMarkups.Count - 1];
			float num4 = clientAreaY - trackChangesMarkups2.LtWidget.Bounds.Bottom;
			if (num4 == 0f && trackChangesMarkups2.EmptySpace == float.MinValue)
			{
				trackChangesMarkups.EmptySpace = float.MinValue;
			}
			else
			{
				trackChangesMarkups.EmptySpace = num4;
			}
		}
	}

	private void IsCurrentWidgetNeedToLayout(LayoutContext lc, int columnIndex, ref bool isSplittedWidget, ref bool isCurrentWidgetNeedToLayout, bool isContinuousSection)
	{
		if (lc.IsEnsureSplitted() && !isCurrentWidgetNeedToLayout && isContinuousSection && columnIndex == 0 && lc.SplittedWidget is SplitWidgetContainer && !((lc.SplittedWidget as SplitWidgetContainer)[0] is WSection) && (lc.SplittedWidget as SplitWidgetContainer)[0] is SplitWidgetContainer && ((lc.SplittedWidget as SplitWidgetContainer)[0] as SplitWidgetContainer).RealWidgetContainer is WSection)
		{
			WSection wSection = ((lc.SplittedWidget as SplitWidgetContainer)[0] as SplitWidgetContainer).RealWidgetContainer as WSection;
			if (wSection.Columns.Count <= 1)
			{
				return;
			}
			float width = wSection.Columns[0].Width;
			bool flag = true;
			foreach (Column column in wSection.Columns)
			{
				if (width != column.Width)
				{
					flag = false;
					break;
				}
				width = column.Width;
			}
			if (!flag && !wSection.PageSetup.EqualColumnWidth)
			{
				isSplittedWidget = false;
				isCurrentWidgetNeedToLayout = true;
			}
		}
		else
		{
			isSplittedWidget = true;
			isCurrentWidgetNeedToLayout = false;
		}
	}

	void ILCOperator.SendLeafLayoutAfter(LayoutedWidget ltWidget, bool isFromTOCLinkStyle)
	{
		if (this.LeafLayoutAfter != null)
		{
			this.LeafLayoutAfter(this, ltWidget, isFromTOCLinkStyle);
		}
	}

	internal void ResetWordLayoutingFlags(bool canSplitByCharacter, bool canSplitByTab, bool isFirstItemInLine, List<float> lineSpaceWidths, float width)
	{
		m_canSplitbyCharacter = canSplitByCharacter;
		m_canSplitByTab = canSplitByTab;
		IsFirstItemInLine = isFirstItemInLine;
		m_lineSpaceWidths = lineSpaceWidths;
		m_effectiveJustifyWidth = width;
	}

	internal static float GetLeftMargin(WSection section)
	{
		float left = section.PageSetup.Margins.Left;
		if (section.Document.DOP.MirrorMargins && ((section.Document.DOP.GutterAtTop && section.PageSetup.Orientation != section.Document.Sections[0].PageSetup.Orientation) || (!section.Document.DOP.GutterAtTop && section.PageSetup.Orientation == section.Document.Sections[0].PageSetup.Orientation)) && DocumentLayouter.PageNumber % 2 == 0)
		{
			return section.PageSetup.Margins.Right;
		}
		if (!section.Document.DOP.GutterAtTop)
		{
			return left + section.PageSetup.Margins.Gutter;
		}
		return left;
	}

	internal static float GetRightMargin(WSection section)
	{
		float right = section.PageSetup.Margins.Right;
		if (section.Document.DOP.MirrorMargins && ((section.Document.DOP.GutterAtTop && section.PageSetup.Orientation != section.Document.Sections[0].PageSetup.Orientation) || (!section.Document.DOP.GutterAtTop && section.PageSetup.Orientation == section.Document.Sections[0].PageSetup.Orientation)) && DocumentLayouter.PageNumber % 2 == 0)
		{
			right = section.PageSetup.Margins.Left;
			if (!section.Document.DOP.GutterAtTop)
			{
				return right + section.PageSetup.Margins.Gutter;
			}
			return right;
		}
		return right;
	}

	internal float GetCurrentPageHeaderHeight()
	{
		if ((this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.PageWidgets.Count > 0)
		{
			return (this.LeafLayoutAfter.Target as DocumentLayouter).CurrentPage.PageWidgets[0].Bounds.Bottom;
		}
		return 0f;
	}

	internal static float GetCurrentPageRightPosition(WSection section)
	{
		return section.PageSetup.PageSize.Width - GetRightMargin(section);
	}
}
