using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Grid;

public class PdfGridColumnCollection : IEnumerable
{
	private struct PdfGridColumnEnumerator : IEnumerator
	{
		private PdfGridColumnCollection m_columnCollection;

		private int m_currentIndex;

		public object Current
		{
			get
			{
				CheckIndex();
				return m_columnCollection[m_currentIndex];
			}
		}

		internal PdfGridColumnEnumerator(PdfGridColumnCollection columnCollection)
		{
			if (columnCollection == null)
			{
				throw new ArgumentNullException("columnCollection");
			}
			m_columnCollection = columnCollection;
			m_currentIndex = -1;
		}

		public bool MoveNext()
		{
			m_currentIndex++;
			return m_currentIndex < m_columnCollection.Count;
		}

		public void Reset()
		{
			m_currentIndex = -1;
		}

		private void CheckIndex()
		{
			if (m_currentIndex < 0 || m_currentIndex >= m_columnCollection.Count)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}

	private PdfGrid m_grid;

	private List<PdfGridColumn> m_columns = new List<PdfGridColumn>();

	private float m_width = float.MinValue;

	private float m_previousCellsCount;

	public PdfGridColumn this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new IndexOutOfRangeException();
			}
			return m_columns[index];
		}
	}

	public int Count => m_columns.Count;

	internal float Width
	{
		get
		{
			if (m_width == float.MinValue)
			{
				m_width = MeasureColumnsWidth();
			}
			if (m_grid.InitialWidth != 0f && m_width != m_grid.InitialWidth && !m_grid.Style.AllowHorizontalOverflow)
			{
				m_width = m_grid.InitialWidth;
				m_grid.IsPageWidth = true;
			}
			return m_width;
		}
	}

	public PdfGridColumnCollection(PdfGrid grid)
	{
		m_grid = grid;
		m_columns = new List<PdfGridColumn>();
	}

	internal void Clear()
	{
		m_columns.Clear();
	}

	public PdfGridColumn Add()
	{
		PdfGridColumn pdfGridColumn = new PdfGridColumn(m_grid);
		m_columns.Add(pdfGridColumn);
		return pdfGridColumn;
	}

	public void Add(int count)
	{
		for (int i = 0; i < count; i++)
		{
			m_columns.Add(new PdfGridColumn(m_grid));
			foreach (PdfGridRow row in m_grid.Rows)
			{
				PdfGridCell cell = new PdfGridCell
				{
					Value = ""
				};
				row.Cells.Add(cell);
			}
		}
	}

	public void Add(PdfGridColumn column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("column");
		}
		m_columns.Add(column);
	}

	internal void AddColumns(int count)
	{
		if (m_previousCellsCount != (float)count)
		{
			for (int i = count - 1; i < count; i++)
			{
				PdfGridColumn item = new PdfGridColumn(m_grid);
				m_columns.Add(item);
			}
			m_previousCellsCount = count;
		}
	}

	internal float MeasureColumnsWidth()
	{
		float num = 0f;
		m_grid.MeasureColumnsWidth();
		int i = 0;
		for (int count = m_columns.Count; i < count; i++)
		{
			num += m_columns[i].Width;
		}
		return num;
	}

	internal float[] GetDefaultWidths(float totalWidth)
	{
		float[] array = new float[Count];
		int num = Count;
		for (int i = 0; i < Count; i++)
		{
			if (m_grid.IsPageWidth && totalWidth >= 0f && !m_columns[i].isCustomWidth)
			{
				m_columns[i].Width = float.MinValue;
				continue;
			}
			array[i] = m_columns[i].Width;
			if (m_columns[i].Width > 0f && m_columns[i].isCustomWidth)
			{
				totalWidth -= m_columns[i].Width;
				num--;
			}
			else
			{
				array[i] = float.MinValue;
			}
		}
		for (int j = 0; j < Count; j++)
		{
			float num2 = totalWidth / (float)num;
			if (array[j] <= 0f)
			{
				array[j] = num2;
			}
		}
		return array;
	}

	public IEnumerator GetEnumerator()
	{
		return new PdfGridColumnEnumerator(this);
	}
}
