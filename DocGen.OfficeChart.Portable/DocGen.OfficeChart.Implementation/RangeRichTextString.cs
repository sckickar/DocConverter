using System;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class RangeRichTextString : RichTextString, IRTFWrapper, IDisposable, IRichTextString, IParentApplication, IOptimizedUpdate
{
	private WorksheetImpl m_worksheet;

	private long m_lCellIndex;

	public override FontImpl DefaultFont
	{
		get
		{
			int fontIndex = m_worksheet.GetExtendedFormat(m_lCellIndex).FontIndex;
			return m_book.InnerFonts[fontIndex] as FontImpl;
		}
		internal set
		{
			int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(m_lCellIndex);
			int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(m_lCellIndex);
			IInternalFont internalFont = m_book.AddFont(value) as IInternalFont;
			if (rowFromCellIndex != 0 || columnFromCellIndex != 0)
			{
				(m_worksheet[rowFromCellIndex, columnFromCellIndex].CellStyle as ExtendedFormatWrapper).FontIndex = internalFont.Index;
			}
		}
	}

	public int Index => m_worksheet.GetStringIndex(m_lCellIndex);

	public RangeRichTextString(IApplication application, object parent, int row, int column)
		: this(application, parent, RangeImpl.GetCellIndex(column, row))
	{
	}

	public RangeRichTextString(IApplication application, object parent, long cellIndex)
		: base(application, ((WorksheetImpl)parent).ParentWorkbook)
	{
		m_worksheet = (WorksheetImpl)parent;
		if (cellIndex != -1)
		{
			m_lCellIndex = cellIndex;
			m_text = m_worksheet.GetTextWithFormat(m_lCellIndex);
		}
		else
		{
			m_text = m_worksheet.GetTextWithFormat(-1L);
		}
		if (m_text != null)
		{
			m_text = m_text.TypedClone();
		}
	}

	public RangeRichTextString(IApplication application, object parent, long cellIndex, TextWithFormat text)
		: base(application, ((WorksheetImpl)parent).ParentWorkbook, isReadOnly: true)
	{
		m_worksheet = (WorksheetImpl)parent;
		m_lCellIndex = cellIndex;
		m_text = text;
	}

	public override void BeginUpdate()
	{
		if (base.BeginCallsCount == 0)
		{
			if (m_text != null)
			{
				SSTDictionary innerSST = m_book.InnerSST;
				innerSST.Parse();
				int stringIndex = m_worksheet.GetStringIndex(m_lCellIndex);
				if (innerSST.GetStringCount(stringIndex) != 1)
				{
					m_text = m_text.TypedClone();
					if (stringIndex != -1)
					{
						innerSST.RemoveDecrease(stringIndex);
					}
				}
				else
				{
					innerSST.RemoveDecrease(stringIndex);
				}
			}
			else
			{
				m_text = new TextWithFormat();
			}
		}
		base.BeginUpdate();
	}

	public override void EndUpdate()
	{
		base.EndUpdate();
		if (base.BeginCallsCount == 0)
		{
			SSTDictionary innerSST = m_book.InnerSST;
			object key = ((m_text.FormattingRunsCount > 0) ? m_text : m_text.Text);
			int iSSTIndex = innerSST.AddIncrease(key);
			m_worksheet.SetLabelSSTIndex(m_lCellIndex, iSSTIndex);
		}
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}
}
