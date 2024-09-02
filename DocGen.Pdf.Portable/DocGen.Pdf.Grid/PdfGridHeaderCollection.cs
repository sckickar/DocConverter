using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Grid;

public class PdfGridHeaderCollection : IEnumerable
{
	private struct PdfGridHeaderRowEnumerator : IEnumerator
	{
		private PdfGridHeaderCollection m_headerRowCollection;

		private int m_currentIndex;

		public object Current
		{
			get
			{
				CheckIndex();
				return m_headerRowCollection[m_currentIndex];
			}
		}

		internal PdfGridHeaderRowEnumerator(PdfGridHeaderCollection rowCollection)
		{
			if (rowCollection == null)
			{
				throw new ArgumentNullException("rowCollection");
			}
			m_headerRowCollection = rowCollection;
			m_currentIndex = -1;
		}

		public bool MoveNext()
		{
			m_currentIndex++;
			return m_currentIndex < m_headerRowCollection.Count;
		}

		public void Reset()
		{
			m_currentIndex = -1;
		}

		private void CheckIndex()
		{
			if (m_currentIndex < 0 || m_currentIndex >= m_headerRowCollection.Count)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}

	internal PdfGrid m_grid;

	private List<PdfGridRow> m_rows = new List<PdfGridRow>();

	public PdfGridRow this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new IndexOutOfRangeException();
			}
			return m_rows[index];
		}
	}

	public int Count => m_rows.Count;

	public PdfGridHeaderCollection(PdfGrid grid)
	{
		m_grid = grid;
		m_rows = new List<PdfGridRow>();
	}

	internal void Add(PdfGridRow row)
	{
		row.IsHeaderRow = true;
		m_rows.Add(row);
	}

	public PdfGridRow[] Add(int count)
	{
		for (int i = 0; i < count; i++)
		{
			PdfGridRow pdfGridRow = new PdfGridRow(m_grid);
			for (int j = 0; j < m_grid.Columns.Count; j++)
			{
				pdfGridRow.Cells.Add(new PdfGridCell());
			}
			pdfGridRow.IsHeaderRow = true;
			m_rows.Add(pdfGridRow);
		}
		return m_rows.ToArray();
	}

	public void Clear()
	{
		m_rows.Clear();
	}

	public void ApplyStyle(PdfGridStyleBase style)
	{
		if (style is PdfGridCellStyle)
		{
			foreach (PdfGridRow header in m_grid.Headers)
			{
				foreach (PdfGridCell cell in header.Cells)
				{
					cell.Style = style as PdfGridCellStyle;
				}
			}
			return;
		}
		if (!(style is PdfGridRowStyle))
		{
			return;
		}
		foreach (PdfGridRow header2 in m_grid.Headers)
		{
			header2.Style = style as PdfGridRowStyle;
		}
	}

	internal int IndexOf(PdfGridRow row)
	{
		return m_rows.IndexOf(row);
	}

	public IEnumerator GetEnumerator()
	{
		return new PdfGridHeaderRowEnumerator(this);
	}
}
