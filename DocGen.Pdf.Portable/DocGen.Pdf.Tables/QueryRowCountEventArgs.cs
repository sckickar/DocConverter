using System;

namespace DocGen.Pdf.Tables;

public class QueryRowCountEventArgs : EventArgs
{
	private int m_rowCount;

	public int RowCount
	{
		get
		{
			return m_rowCount;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("RowNumber");
			}
			m_rowCount = value;
		}
	}

	internal QueryRowCountEventArgs()
	{
	}
}
