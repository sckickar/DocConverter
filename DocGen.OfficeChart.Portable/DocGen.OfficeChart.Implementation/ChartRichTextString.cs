using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation;

internal class ChartRichTextString : CommonWrapper, IOfficeChartRichTextString, IParentApplication, IOptimizedUpdate
{
	private string m_text;

	protected WorkbookImpl m_book;

	private bool m_bIsReadOnly;

	private object m_parent;

	private ChartTextAreaImpl m_textArea;

	public ChartAlrunsRecord.TRuns[] FormattingRuns
	{
		get
		{
			if (TextArea != null && TextArea.ChartAlRuns != null)
			{
				return TextArea.ChartAlRuns.Runs;
			}
			return null;
		}
	}

	private ChartTextAreaImpl TextArea
	{
		get
		{
			return m_textArea;
		}
		set
		{
			m_textArea = value;
		}
	}

	public string Text
	{
		get
		{
			if (Parent is ChartTextAreaImpl chartTextAreaImpl)
			{
				m_text = chartTextAreaImpl.Text;
			}
			return m_text;
		}
	}

	public object Parent => m_parent;

	public IApplication Application => m_book.Application;

	public ChartRichTextString(IApplication application, object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		m_parent = parent;
		SetParents();
		if (m_parent is ChartTextAreaImpl)
		{
			m_textArea = m_parent as ChartTextAreaImpl;
		}
	}

	public ChartRichTextString(IApplication application, object parent, bool isReadOnly)
		: this(application, parent, isReadOnly, bCreateText: false)
	{
	}

	public ChartRichTextString(IApplication application, object parent, bool isReadOnly, bool bCreateText)
		: this(application, parent)
	{
		m_bIsReadOnly = isReadOnly;
		if (bCreateText)
		{
			m_text = new TextWithFormat();
		}
	}

	public ChartRichTextString(IApplication application, object parent, TextWithFormat text)
		: this(application, parent)
	{
		m_text = text;
	}

	protected virtual void SetParents()
	{
		m_book = CommonObject.FindParent(m_parent, typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}

	public void SetFont(int iStartPos, int iEndPos, IOfficeFont font)
	{
		BeginUpdate();
		ushort fontIndex = (ushort)AddFont(font);
		TextArea = Parent as ChartTextAreaImpl;
		if (TextArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (TextArea.Text == null)
		{
			throw new ArgumentNullException("Does not support rich-text for empty string");
		}
		if (TextArea.Text != null && TextArea.Text.Length > 0 && TextArea.ChartAlRuns != null)
		{
			List<ChartAlrunsRecord.TRuns> list = new List<ChartAlrunsRecord.TRuns>(TextArea.ChartAlRuns.Runs);
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].FirstCharIndex == iStartPos || (flag && list[i].FirstCharIndex <= iEndPos))
				{
					list[i].FontIndex = fontIndex;
					TextArea.ChartAlRuns.Runs = list.ToArray();
					flag = true;
				}
			}
			if (!flag)
			{
				if (iStartPos > 0)
				{
					for (int j = 0; j < iStartPos; j++)
					{
						list.Add(new ChartAlrunsRecord.TRuns((ushort)j, 0));
						TextArea.ChartAlRuns.Runs = list.ToArray();
					}
				}
				for (int k = iStartPos; k <= iEndPos; k++)
				{
					list.Add(new ChartAlrunsRecord.TRuns((ushort)k, fontIndex));
					TextArea.ChartAlRuns.Runs = list.ToArray();
				}
				for (int l = iEndPos + 1; l <= TextArea.Text.Length; l++)
				{
					list.Add(new ChartAlrunsRecord.TRuns((ushort)l, 0));
					TextArea.ChartAlRuns.Runs = list.ToArray();
				}
			}
			TextArea.m_chartText.IsAutoColor = false;
		}
		EndUpdate();
	}

	public IOfficeFont GetFont(ChartAlrunsRecord.TRuns tRuns)
	{
		FontsCollection innerFonts = m_book.InnerFonts;
		IOfficeFont result = null;
		foreach (FontImpl item in innerFonts)
		{
			if (item.Index == tRuns.FontIndex)
			{
				result = item;
			}
		}
		return result;
	}

	protected virtual int AddFont(IOfficeFont font)
	{
		FontImpl font2 = ((IInternalFont)font).Font;
		font2 = m_book.InnerFonts.Add(font2) as FontImpl;
		return font2.Index;
	}
}
