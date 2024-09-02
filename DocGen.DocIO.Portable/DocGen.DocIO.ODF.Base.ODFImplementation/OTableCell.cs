using System;
using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTableCell
{
	private object m_value;

	private string m_value2;

	private bool m_booleanValue;

	private string m_currency;

	private DateTime m_dateValue;

	private TimeSpan m_timeValue;

	private string m_formula;

	private int m_columnsSpanned;

	private int m_rowsSpanned;

	private int m_matrixColunsSpanned;

	private int m_matrixRowsSpanned;

	private CellValueType m_type;

	private string m_styleName;

	private int m_columnsRepeated;

	private string m_tableFormula;

	private OParagraph m_paragraph;

	private bool m_isBlank;

	private List<OTextBodyItem> m_textBodyIetm;

	private float m_cellWidth;

	internal float CellWidth
	{
		get
		{
			return m_cellWidth;
		}
		set
		{
			m_cellWidth = value;
		}
	}

	internal List<OTextBodyItem> TextBodyIetm
	{
		get
		{
			if (m_textBodyIetm == null)
			{
				m_textBodyIetm = new List<OTextBodyItem>();
			}
			return m_textBodyIetm;
		}
		set
		{
			m_textBodyIetm = value;
		}
	}

	internal object Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	internal string Value2
	{
		get
		{
			return m_value2;
		}
		set
		{
			m_value2 = value;
		}
	}

	internal CellValueType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	internal string StyleName
	{
		get
		{
			return m_styleName;
		}
		set
		{
			m_styleName = value;
		}
	}

	internal int ColumnsRepeated
	{
		get
		{
			return m_columnsRepeated;
		}
		set
		{
			m_columnsRepeated = value;
		}
	}

	internal string TableFormula
	{
		get
		{
			return m_tableFormula;
		}
		set
		{
			m_tableFormula = value;
		}
	}

	internal OParagraph Paragraph
	{
		get
		{
			return m_paragraph;
		}
		set
		{
			m_paragraph = value;
		}
	}

	internal bool BooleanValue
	{
		get
		{
			return m_booleanValue;
		}
		set
		{
			m_booleanValue = value;
		}
	}

	internal string Currency
	{
		get
		{
			return m_currency;
		}
		set
		{
			m_currency = value;
		}
	}

	internal DateTime DateValue
	{
		get
		{
			return m_dateValue;
		}
		set
		{
			m_dateValue = value;
		}
	}

	internal TimeSpan TimeValue
	{
		get
		{
			return m_timeValue;
		}
		set
		{
			m_timeValue = value;
		}
	}

	internal int ColumnsSpanned
	{
		get
		{
			return m_columnsSpanned;
		}
		set
		{
			m_columnsSpanned = value;
		}
	}

	internal int RowsSpanned
	{
		get
		{
			return m_rowsSpanned;
		}
		set
		{
			m_rowsSpanned = value;
		}
	}

	internal int MatrixColunsSpanned
	{
		get
		{
			return m_matrixColunsSpanned;
		}
		set
		{
			m_matrixColunsSpanned = value;
		}
	}

	internal int MatrixRowsSpanned
	{
		get
		{
			return m_matrixRowsSpanned;
		}
		set
		{
			m_matrixRowsSpanned = value;
		}
	}

	internal bool IsBlank
	{
		get
		{
			return m_isBlank;
		}
		set
		{
			m_isBlank = value;
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is OTableCell oTableCell))
		{
			return false;
		}
		if (IsBlank)
		{
			return oTableCell.IsBlank;
		}
		return false;
	}

	internal void Dispose()
	{
		if (m_paragraph != null)
		{
			m_paragraph.Dispose();
			m_paragraph = null;
		}
		if (m_textBodyIetm != null)
		{
			for (int i = 0; i < m_textBodyIetm.Count; i++)
			{
				m_textBodyIetm[i] = null;
			}
			m_textBodyIetm.Clear();
			m_textBodyIetm = null;
		}
	}
}
