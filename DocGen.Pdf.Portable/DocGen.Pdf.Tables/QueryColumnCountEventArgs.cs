using System;

namespace DocGen.Pdf.Tables;

public class QueryColumnCountEventArgs : EventArgs
{
	private int m_columnCount;

	public int ColumnCount
	{
		get
		{
			return m_columnCount;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("ColumnNumber");
			}
			m_columnCount = value;
		}
	}

	internal QueryColumnCountEventArgs()
	{
	}
}
