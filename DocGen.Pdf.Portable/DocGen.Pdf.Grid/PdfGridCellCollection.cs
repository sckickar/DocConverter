using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Grid;

public class PdfGridCellCollection : IEnumerable
{
	private struct PdfGridCellEnumerator : IEnumerator
	{
		private PdfGridCellCollection m_cellCollection;

		private int m_currentIndex;

		public object Current
		{
			get
			{
				CheckIndex();
				return m_cellCollection[m_currentIndex];
			}
		}

		internal PdfGridCellEnumerator(PdfGridCellCollection columnCollection)
		{
			if (columnCollection == null)
			{
				throw new ArgumentNullException("columnCollection");
			}
			m_cellCollection = columnCollection;
			m_currentIndex = -1;
		}

		public bool MoveNext()
		{
			m_currentIndex++;
			return m_currentIndex < m_cellCollection.Count;
		}

		public void Reset()
		{
			m_currentIndex = -1;
		}

		private void CheckIndex()
		{
			if (m_currentIndex < 0 || m_currentIndex >= m_cellCollection.Count)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}

	private PdfGridRow m_row;

	private List<PdfGridCell> m_cells = new List<PdfGridCell>();

	public PdfGridCell this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new IndexOutOfRangeException();
			}
			return m_cells[index];
		}
	}

	public int Count => m_cells.Count;

	internal PdfGridCellCollection(PdfGridRow row)
	{
		m_row = row;
	}

	internal PdfGridCell Add()
	{
		PdfGridCell pdfGridCell = new PdfGridCell();
		pdfGridCell.Style = m_row.Style as PdfGridCellStyle;
		Add(pdfGridCell);
		return pdfGridCell;
	}

	internal void Add(PdfGridCell cell)
	{
		if (cell.Style == null)
		{
			cell.Style = m_row.Style as PdfGridCellStyle;
		}
		cell.Row = m_row;
		m_cells.Add(cell);
	}

	public int IndexOf(PdfGridCell cell)
	{
		return m_cells.IndexOf(cell);
	}

	public IEnumerator GetEnumerator()
	{
		return new PdfGridCellEnumerator(this);
	}
}
