namespace DocGen.Pdf.Grid;

public class PdfGridBuiltinStyleSettings
{
	private bool m_bandedColumns;

	private bool m_bandedRows;

	private bool m_firstColumn;

	private bool m_lastColumn;

	private bool m_headerRow;

	private bool m_lastRow;

	public bool ApplyStyleForBandedColumns
	{
		get
		{
			return m_bandedColumns;
		}
		set
		{
			m_bandedColumns = value;
		}
	}

	public bool ApplyStyleForBandedRows
	{
		get
		{
			return m_bandedRows;
		}
		set
		{
			m_bandedRows = value;
		}
	}

	public bool ApplyStyleForFirstColumn
	{
		get
		{
			return m_firstColumn;
		}
		set
		{
			m_firstColumn = value;
		}
	}

	public bool ApplyStyleForHeaderRow
	{
		get
		{
			return m_headerRow;
		}
		set
		{
			m_headerRow = value;
		}
	}

	public bool ApplyStyleForLastColumn
	{
		get
		{
			return m_lastColumn;
		}
		set
		{
			m_lastColumn = value;
		}
	}

	public bool ApplyStyleForLastRow
	{
		get
		{
			return m_lastRow;
		}
		set
		{
			m_lastRow = value;
		}
	}
}
