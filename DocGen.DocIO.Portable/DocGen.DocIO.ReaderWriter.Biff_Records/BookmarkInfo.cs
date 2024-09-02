namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class BookmarkInfo
{
	private string m_strName;

	private int m_iStartPos;

	private int m_iEndPos;

	private bool m_isCellGroup;

	private short m_startCell = -1;

	private short m_endCell = -1;

	private int m_bookmarkIndex;

	internal string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
		}
	}

	internal int EndPos
	{
		get
		{
			return m_iEndPos;
		}
		set
		{
			m_iEndPos = value;
		}
	}

	internal int StartPos
	{
		get
		{
			return m_iStartPos;
		}
		set
		{
			m_iStartPos = value;
		}
	}

	internal bool IsCellGroupBookmark => m_isCellGroup;

	internal short StartCellIndex
	{
		get
		{
			return m_startCell;
		}
		set
		{
			m_startCell = value;
		}
	}

	internal short EndCellIndex
	{
		get
		{
			return m_endCell;
		}
		set
		{
			m_endCell = value;
		}
	}

	internal int Index
	{
		get
		{
			return m_bookmarkIndex;
		}
		set
		{
			m_bookmarkIndex = value;
		}
	}

	internal BookmarkInfo(string name, int startPos, int endPos, bool isCellGroup, short startCellIndex, short endCellIndex)
	{
		m_strName = name;
		m_iStartPos = startPos;
		m_iEndPos = endPos;
		if (isCellGroup)
		{
			m_isCellGroup = true;
			m_startCell = startCellIndex;
			m_endCell = (short)(endCellIndex - 1);
		}
	}

	internal BookmarkInfo Clone()
	{
		return (BookmarkInfo)MemberwiseClone();
	}
}
