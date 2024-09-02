using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS.Rendering;

internal class Page
{
	private LayoutedWidgetList m_pageWidgets = new LayoutedWidgetList();

	private LayoutedWidgetList m_footnoteWidgets = new LayoutedWidgetList();

	private LayoutedWidgetList m_lineNumberWidgets = new LayoutedWidgetList();

	private LayoutedWidgetList m_endnoteWidgets = new LayoutedWidgetList();

	private List<TrackChangesMarkups> m_trackChangesMarkups = new List<TrackChangesMarkups>();

	private List<int> m_endNotesectionIndex = new List<int>();

	private List<int> m_footNotesectionIndex = new List<int>();

	private WPageSetup m_pageSetup;

	private WHeadersFooters m_headersFooters;

	private IWSection m_docSection;

	private int m_iNumber;

	private byte[] m_backgroundImage;

	private Color m_backgroundColor;

	private List<IWField> m_cachedFields = new List<IWField>();

	private LayoutedWidgetList m_behindWidgets;

	private int m_numberOfBehindShapeWidgetsInHeader;

	private int m_numberOfBehindShapeWidgetsInFooter;

	public LayoutedWidgetList PageWidgets => m_pageWidgets;

	internal IWSection DocSection => m_docSection;

	internal byte[] BackgroundImage => m_backgroundImage;

	internal Color BackgroundColor => m_backgroundColor;

	internal LayoutedWidgetList FootnoteWidgets => m_footnoteWidgets;

	internal LayoutedWidgetList LineNumberWidgets => m_lineNumberWidgets;

	internal LayoutedWidgetList EndnoteWidgets => m_endnoteWidgets;

	internal List<TrackChangesMarkups> TrackChangesMarkups
	{
		get
		{
			if (m_trackChangesMarkups == null)
			{
				m_trackChangesMarkups = new List<TrackChangesMarkups>();
			}
			return m_trackChangesMarkups;
		}
		set
		{
			m_trackChangesMarkups = value;
		}
	}

	internal LayoutedWidgetList BehindWidgets
	{
		get
		{
			if (m_behindWidgets == null)
			{
				m_behindWidgets = new LayoutedWidgetList();
			}
			return m_behindWidgets;
		}
	}

	internal List<int> EndNoteSectionIndex => m_endNotesectionIndex;

	internal List<int> FootNoteSectionIndex => m_footNotesectionIndex;

	public WPageSetup Setup => m_pageSetup;

	public int Number
	{
		get
		{
			return m_iNumber;
		}
		set
		{
			m_iNumber = value;
		}
	}

	internal int NumberOfBehindWidgetsInHeader
	{
		get
		{
			return m_numberOfBehindShapeWidgetsInHeader;
		}
		set
		{
			m_numberOfBehindShapeWidgetsInHeader = value;
		}
	}

	internal int NumberOfBehindWidgetsInFooter
	{
		get
		{
			return m_numberOfBehindShapeWidgetsInFooter;
		}
		set
		{
			m_numberOfBehindShapeWidgetsInFooter = value;
		}
	}

	internal bool SwapMargins
	{
		get
		{
			if (m_docSection.Document.DOP.MirrorMargins && ((m_docSection.Document.DOP.GutterAtTop && m_docSection.PageSetup.Orientation != m_docSection.Document.Sections[0].PageSetup.Orientation) || (!m_docSection.Document.DOP.GutterAtTop && m_docSection.PageSetup.Orientation == m_docSection.Document.Sections[0].PageSetup.Orientation)))
			{
				return DocumentLayouter.PageNumber % 2 == 0;
			}
			return false;
		}
	}

	public Page(IWSection section, int iNumber)
	{
		m_docSection = section;
		m_pageSetup = section.PageSetup;
		m_headersFooters = section.HeadersFooters;
		m_iNumber = iNumber;
		IWordDocument document = section.Document;
		m_backgroundImage = document.BackgroundImage;
		m_backgroundColor = document.Background.Color;
	}

	public void InitLayoutInfo()
	{
		for (int i = 0; i < m_pageWidgets.Count; i++)
		{
			m_pageWidgets[i].InitLayoutInfoAll();
		}
		for (int j = 0; j < m_footnoteWidgets.Count; j++)
		{
			LayoutedWidget layoutedWidget = m_footnoteWidgets[j];
			WTextBody wTextBody = ((layoutedWidget.Widget is WTextBody) ? (layoutedWidget.Widget as WTextBody) : ((layoutedWidget.Widget is SplitWidgetContainer) ? ((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WTextBody) : null));
			if (wTextBody != null && wTextBody.Owner is WFootnote)
			{
				(wTextBody.Owner as WFootnote).IsLayouted = false;
			}
			layoutedWidget.InitLayoutInfoAll();
		}
		for (int k = 0; k < m_endnoteWidgets.Count; k++)
		{
			LayoutedWidget layoutedWidget2 = m_endnoteWidgets[k];
			WTextBody wTextBody2 = ((layoutedWidget2.Widget is WTextBody) ? (layoutedWidget2.Widget as WTextBody) : ((layoutedWidget2.Widget is SplitWidgetContainer) ? ((layoutedWidget2.Widget as SplitWidgetContainer).RealWidgetContainer as WTextBody) : null));
			if (wTextBody2 != null && wTextBody2.Owner is WFootnote)
			{
				(wTextBody2.Owner as WFootnote).IsLayouted = false;
			}
			layoutedWidget2.InitLayoutInfoAll();
		}
		if (m_pageWidgets != null)
		{
			m_pageWidgets.Clear();
			m_pageWidgets = null;
		}
		if (m_footnoteWidgets != null)
		{
			m_footnoteWidgets.Clear();
			m_footnoteWidgets = null;
		}
		if (m_endnoteWidgets != null)
		{
			m_endnoteWidgets.Clear();
			m_endnoteWidgets = null;
		}
		if (m_endNotesectionIndex != null)
		{
			m_endNotesectionIndex.Clear();
			m_endNotesectionIndex = null;
		}
		if (m_footNotesectionIndex != null)
		{
			m_footNotesectionIndex.Clear();
			m_footNotesectionIndex = null;
		}
		if (m_cachedFields != null)
		{
			m_cachedFields.Clear();
			m_cachedFields = null;
		}
		if (m_behindWidgets != null)
		{
			m_behindWidgets.Clear();
			m_behindWidgets = null;
		}
	}

	public void UpdateFieldsNumPages(int numPages)
	{
		for (int i = 0; i < m_cachedFields.Count; i++)
		{
			IWField iWField = m_cachedFields[i];
			if (iWField is WField && iWField.FieldType == FieldType.FieldNumPages)
			{
				(iWField as WField).FieldResult = numPages.ToString();
			}
		}
	}

	public void AddCachedFields(IWField field)
	{
		m_cachedFields.Add(field);
	}

	protected internal RectangleF GetHeaderArea()
	{
		float leftMargin = ((m_pageSetup.Margins.Left != -0.05f) ? m_pageSetup.Margins.Left : 0f);
		float rightMargin = ((m_pageSetup.Margins.Right != -0.05f) ? m_pageSetup.Margins.Right : 0f);
		float y = ((m_pageSetup.HeaderDistance != -0.05f) ? m_pageSetup.HeaderDistance : 36f);
		float width = m_pageSetup.PageSize.Width;
		float height = m_pageSetup.PageSize.Height;
		float height2 = ((DocSection.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? height : (height / 2f));
		if (SwapMargins)
		{
			UpdateMirrorMargins(ref leftMargin, ref rightMargin);
		}
		if ((m_pageSetup.Margins.Gutter > 0f && m_pageSetup.Document.DOP.GutterAtTop && m_docSection.PageSetup.Orientation != m_pageSetup.Document.Sections[0].PageSetup.Orientation) || (!m_pageSetup.Document.DOP.GutterAtTop && m_docSection.PageSetup.Orientation == m_pageSetup.Document.Sections[0].PageSetup.Orientation))
		{
			if (SwapMargins)
			{
				rightMargin += m_pageSetup.Margins.Gutter;
			}
			else
			{
				leftMargin += m_pageSetup.Margins.Gutter;
			}
		}
		return new RectangleF(leftMargin, y, width - (leftMargin + rightMargin), height2);
	}

	protected internal RectangleF GetFooterArea()
	{
		float leftMargin = ((m_pageSetup.Margins.Left != -0.05f) ? m_pageSetup.Margins.Left : 0f);
		float rightMargin = ((m_pageSetup.Margins.Right != -0.05f) ? m_pageSetup.Margins.Right : 0f);
		float num = ((m_pageSetup.FooterDistance != -0.05f) ? m_pageSetup.FooterDistance : 36f);
		float width = m_pageSetup.PageSize.Width;
		float height = m_pageSetup.PageSize.Height;
		float height2 = ((DocSection.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? height : (height / 2f));
		if (SwapMargins)
		{
			UpdateMirrorMargins(ref leftMargin, ref rightMargin);
		}
		if ((m_pageSetup.Margins.Gutter > 0f && m_pageSetup.Document.DOP.GutterAtTop && m_docSection.PageSetup.Orientation != m_pageSetup.Document.Sections[0].PageSetup.Orientation) || (!m_pageSetup.Document.DOP.GutterAtTop && m_docSection.PageSetup.Orientation == m_pageSetup.Document.Sections[0].PageSetup.Orientation))
		{
			if (SwapMargins)
			{
				rightMargin += m_pageSetup.Margins.Gutter;
			}
			else
			{
				leftMargin += m_pageSetup.Margins.Gutter;
			}
		}
		return new RectangleF(leftMargin, height - num, width - (leftMargin + rightMargin), height2);
	}

	private void UpdateGutterValue(ref float margin, Column column)
	{
		if (m_docSection.Columns[0] == column)
		{
			margin += m_pageSetup.Margins.Gutter;
		}
		else
		{
			margin += m_pageSetup.Margins.Gutter / (float)m_docSection.Columns.Count;
		}
	}

	private void UpdateMirrorMargins(ref float leftMargin, ref float rightMargin)
	{
		MarginsF margins = m_docSection.PageSetup.Margins;
		leftMargin = ((margins.Right != -0.05f) ? margins.Right : 0f);
		rightMargin = ((margins.Left != -0.05f) ? margins.Left : 0f);
	}

	protected internal RectangleF GetColumnArea(Column column, float prevWidth, bool isNeedtoAdjustFooter)
	{
		MarginsF margins = m_pageSetup.Margins;
		float width = m_pageSetup.PageSize.Width;
		float height = m_pageSetup.PageSize.Height;
		float num = Math.Abs((margins.Top != -0.05f) ? margins.Top : 0f);
		float leftMargin = ((margins.Left != -0.05f) ? margins.Left : 0f);
		float rightMargin = ((margins.Right != -0.05f) ? margins.Right : 0f);
		float num2 = Math.Abs((margins.Bottom != -0.05f) ? margins.Bottom : 0f);
		if (SwapMargins)
		{
			UpdateMirrorMargins(ref leftMargin, ref rightMargin);
		}
		if (SwapMargins)
		{
			UpdateMirrorMargins(ref leftMargin, ref rightMargin);
		}
		float val = ((m_pageWidgets[0].ChildWidgets.Count == 0 || (margins.Top < 0f && num > 0f)) ? 0f : (m_pageWidgets[0].Bounds.Height + ((m_pageSetup.HeaderDistance != -0.05f) ? m_pageSetup.HeaderDistance : 36f)));
		float num3 = 0f;
		num3 = ((!isNeedtoAdjustFooter) ? ((m_pageWidgets[1].ChildWidgets.Count == 0 || (margins.Bottom < 0f && num2 > 0f)) ? 0f : (m_pageWidgets[1].Bounds.Height + ((m_pageSetup.FooterDistance != -0.05f) ? m_pageSetup.FooterDistance : 36f))) : ((m_pageWidgets[1].ChildWidgets.Count == 0 || (margins.Bottom < 0f && num2 > 0f)) ? 0f : m_pageWidgets[1].Bounds.Height));
		float num4 = ((m_docSection.Columns.Count <= 1) ? m_docSection.PageSetup.ClientWidth : (column?.Width ?? (width - (leftMargin + rightMargin))));
		if (m_pageSetup.Margins.Gutter > 0f && m_docSection.Columns.Count > 0 && m_docSection.PageSetup.Orientation != m_pageSetup.Document.Sections[0].PageSetup.Orientation)
		{
			if (m_pageSetup.Document.DOP.GutterAtTop)
			{
				if (SwapMargins)
				{
					UpdateGutterValue(ref rightMargin, column);
				}
				else
				{
					UpdateGutterValue(ref leftMargin, column);
				}
				num4 -= m_pageSetup.Margins.Gutter / (float)m_docSection.Columns.Count;
			}
			else
			{
				num2 += m_pageSetup.Margins.Gutter;
			}
		}
		else if (m_pageSetup.Margins.Gutter > 0f && m_pageSetup.Document.DOP.GutterAtTop)
		{
			num += m_pageSetup.Margins.Gutter;
		}
		else if (m_pageSetup.Margins.Gutter > 0f && m_docSection.Columns.Count > 0)
		{
			if (SwapMargins)
			{
				UpdateGutterValue(ref rightMargin, column);
			}
			else
			{
				UpdateGutterValue(ref leftMargin, column);
			}
			num4 -= m_pageSetup.Margins.Gutter / (float)m_docSection.Columns.Count;
		}
		if (m_pageSetup.Bidi && m_docSection.Columns.Count > 1)
		{
			return new RectangleF(leftMargin + m_pageSetup.ClientWidth - prevWidth - num4, Math.Max(num, val), num4, height - (Math.Max(num, val) + Math.Max(num2, num3)));
		}
		return new RectangleF(leftMargin + prevWidth, Math.Max(num, val), num4, height - (Math.Max(num, val) + Math.Max(num2, num3)));
	}

	protected internal RectangleF GetColumnArea(int columnIndex, ref float prevColumnsWidth, bool isNeedtoAdjustFooter)
	{
		Column column = ((m_docSection.Columns.Count > columnIndex) ? m_docSection.Columns[columnIndex] : null);
		RectangleF columnArea = GetColumnArea(column, prevColumnsWidth, isNeedtoAdjustFooter);
		if (column != null)
		{
			prevColumnsWidth += column.Width + column.Space;
		}
		return columnArea;
	}

	protected internal RectangleF GetSectionArea(Column column, float prevWidth)
	{
		MarginsF margins = m_docSection.PageSetup.Margins;
		float width = m_docSection.PageSetup.PageSize.Width;
		float height = m_docSection.PageSetup.PageSize.Height;
		float num = Math.Abs((margins.Top != -0.05f) ? margins.Top : 0f);
		float leftMargin = ((margins.Left != -0.05f) ? margins.Left : 0f);
		float rightMargin = ((margins.Right != -0.05f) ? margins.Right : 0f);
		float num2 = Math.Abs((margins.Bottom != -0.05f) ? margins.Bottom : 0f);
		if (SwapMargins)
		{
			UpdateMirrorMargins(ref leftMargin, ref rightMargin);
		}
		if (SwapMargins)
		{
			UpdateMirrorMargins(ref leftMargin, ref rightMargin);
		}
		float num3 = ((m_docSection.Columns.Count <= 1) ? m_docSection.PageSetup.ClientWidth : (column?.Width ?? (width - (leftMargin + rightMargin))));
		if (m_pageSetup.Margins.Gutter > 0f && m_docSection.PageSetup.Orientation != m_pageSetup.Document.Sections[0].PageSetup.Orientation)
		{
			if (m_pageSetup.Document.DOP.GutterAtTop)
			{
				if (SwapMargins)
				{
					rightMargin += m_pageSetup.Margins.Gutter;
				}
				else
				{
					leftMargin += m_pageSetup.Margins.Gutter;
				}
				num3 -= margins.Gutter;
			}
			else
			{
				num2 += margins.Gutter;
			}
		}
		else if (m_docSection.Document.DOP.GutterAtTop)
		{
			num += margins.Gutter;
		}
		else
		{
			if (SwapMargins)
			{
				rightMargin += m_pageSetup.Margins.Gutter;
			}
			else
			{
				leftMargin += m_pageSetup.Margins.Gutter;
			}
			num3 -= margins.Gutter;
		}
		if (m_docSection.PageSetup.Bidi && m_docSection.Columns.Count > 1)
		{
			return new RectangleF(leftMargin + m_docSection.PageSetup.ClientWidth - prevWidth - num3, num, num3, height - (num + num2));
		}
		return new RectangleF(leftMargin + prevWidth, num, num3, height - (num + num2));
	}

	protected internal RectangleF GetSectionArea(int columnIndex, ref float prevColumnsWidth, bool isNextSection, bool isSplittedWidget)
	{
		int num = m_docSection.Document.Sections.IndexOf(m_docSection);
		if (!isSplittedWidget)
		{
			num--;
		}
		if (m_docSection.Document.Sections.Count - 1 > num && columnIndex == 0 && !isNextSection)
		{
			m_docSection = m_docSection.Document.Sections[num + 1];
		}
		Column column = ((m_docSection.Columns.Count > columnIndex) ? m_docSection.Columns[columnIndex] : null);
		RectangleF sectionArea = GetSectionArea(column, prevColumnsWidth);
		if (column != null)
		{
			prevColumnsWidth += column.Width + column.Space;
		}
		return sectionArea;
	}
}
