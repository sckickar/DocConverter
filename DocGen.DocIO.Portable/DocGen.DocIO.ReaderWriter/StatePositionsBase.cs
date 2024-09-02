using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal abstract class StatePositionsBase
{
	protected int m_iStartItemPos;

	protected int m_iEndItemPos;

	protected int m_iItemIndex;

	protected long m_iEndCHPxPos;

	internal long m_iEndPAPxPos;

	internal long m_iEndPieceTablePos;

	internal long m_iStartPieceTablePos;

	protected int m_iStartText;

	protected int m_iEndText;

	protected WordFKPData m_fkp;

	protected BookmarkInfo[] m_bookmarks;

	private int m_curTextPosition;

	private int m_iCurrentPapxFKPIndex;

	private int m_iCurrentChpxFKPIndex;

	private int m_iCurrentPapxIndex;

	private int m_iCurrentChpxIndex;

	internal int StartItemPos => m_iStartItemPos;

	internal int EndItemPos => m_iEndItemPos;

	internal int ItemIndex
	{
		get
		{
			return m_iItemIndex;
		}
		set
		{
			m_iItemIndex = value;
		}
	}

	internal int StartText
	{
		get
		{
			return m_iStartText;
		}
		set
		{
			m_iStartText = value;
		}
	}

	internal CharacterPropertyException CurrentChpx => m_fkp.GetChpxPage(m_iCurrentChpxFKPIndex).CharacterProperties[m_iCurrentChpxIndex];

	internal ParagraphPropertyException CurrentPapx => m_fkp.GetPapxPage(m_iCurrentPapxFKPIndex).ParagraphProperties[m_iCurrentPapxIndex];

	internal int CurrentTextPosition
	{
		get
		{
			return m_curTextPosition;
		}
		set
		{
			m_curTextPosition = value;
		}
	}

	protected WPTablesData Tables => m_fkp.Tables;

	protected bool IsCurrentPapxPosition
	{
		get
		{
			uint num = CurrentPapxPage.FileCharPos[m_iCurrentPapxIndex];
			uint num2 = CurrentPapxPage.FileCharPos[m_iCurrentPapxIndex + 1];
			if (m_iStartText >= num && m_iStartText <= num2)
			{
				return true;
			}
			return false;
		}
	}

	protected bool IsCurrentChpxPosition
	{
		get
		{
			uint num = CurrentChpxPage.FileCharPos[m_iCurrentChpxIndex];
			uint num2 = CurrentChpxPage.FileCharPos[m_iCurrentChpxIndex + 1];
			if (m_iStartText >= num && m_iStartText <= num2)
			{
				return true;
			}
			return false;
		}
	}

	private CharacterPropertiesPage CurrentChpxPage => m_fkp.GetChpxPage(m_iCurrentChpxFKPIndex);

	private ParagraphPropertiesPage CurrentPapxPage => m_fkp.GetPapxPage(m_iCurrentPapxFKPIndex);

	internal StatePositionsBase(WordFKPData fkp)
	{
		m_fkp = fkp;
	}

	internal virtual void InitStartEndPos()
	{
		m_iStartText = (int)Tables.ConvertCharPosToFileCharPos(0u);
		m_iEndText = (int)Tables.ConvertCharPosToFileCharPos((uint)m_fkp.Fib.CcpText);
		m_iEndPAPxPos = (int)m_fkp.GetPapxPage(0).FileCharPos[1];
		m_iEndCHPxPos = (int)m_fkp.GetChpxPage(0).FileCharPos[1];
	}

	internal bool NextChpx()
	{
		CharacterPropertiesPage currentChpxPage = CurrentChpxPage;
		if (m_iCurrentChpxIndex < currentChpxPage.RunsCount - 1)
		{
			m_iCurrentChpxIndex++;
		}
		else
		{
			if (m_iCurrentChpxFKPIndex + 1 > m_fkp.Tables.CHPXBinaryTable.EntriesCount - 1)
			{
				m_iEndCHPxPos = -1L;
				return false;
			}
			m_iCurrentChpxFKPIndex++;
			m_iCurrentChpxIndex = 0;
			currentChpxPage = CurrentChpxPage;
		}
		m_iEndCHPxPos = (int)currentChpxPage.FileCharPos[m_iCurrentChpxIndex + 1];
		return true;
	}

	internal bool NextPapx()
	{
		ParagraphPropertiesPage currentPapxPage = CurrentPapxPage;
		if (m_iCurrentPapxIndex < currentPapxPage.RunsCount - 1)
		{
			m_iCurrentPapxIndex++;
		}
		else
		{
			if (m_iCurrentPapxFKPIndex + 1 > m_fkp.Tables.PAPXBinaryTable.EntriesCount - 1)
			{
				m_iEndPAPxPos = -1L;
				return false;
			}
			m_iCurrentPapxFKPIndex++;
			m_iCurrentPapxIndex = 0;
			currentPapxPage = CurrentPapxPage;
		}
		m_iEndPAPxPos = (int)currentPapxPage.FileCharPos[m_iCurrentPapxIndex + 1];
		return true;
	}

	internal virtual long GetMinEndPos(long curPos)
	{
		int num = ((m_iEndPieceTablePos != 0L) ? Tables.PieceTablePositions.IndexOf((uint)m_iEndPieceTablePos) : 0);
		long num2 = ((num > 0) ? ((Tables.m_pieceTable.FileCharacterPos[num] - Tables.m_pieceTable.FileCharacterPos[num - 1]) * Tables.m_pieceTableEncodings[num - 1].GetByteCount("a")) : 0);
		if (m_iEndPieceTablePos <= curPos || curPos == m_iStartPieceTablePos + num2)
		{
			List<uint> pieceTablePositions = Tables.PieceTablePositions;
			int i = num;
			for (int num3 = pieceTablePositions.Count - 1; i < num3; i++)
			{
				uint num4 = pieceTablePositions[i];
				uint num5 = pieceTablePositions[i + 1];
				if (curPos < num5)
				{
					m_iStartPieceTablePos = num4;
					m_iEndPieceTablePos = (int)num5;
					num++;
					break;
				}
			}
		}
		long num6 = (((m_iEndCHPxPos == -1 || m_iStartPieceTablePos <= m_iEndCHPxPos) && (m_iEndPAPxPos == -1 || m_iStartPieceTablePos <= m_iEndPAPxPos)) ? Math.Min(Math.Min(m_iEndCHPxPos, m_iEndPAPxPos), m_iEndPieceTablePos) : Math.Min(m_iStartPieceTablePos, m_iEndPieceTablePos));
		num2 = ((num > 0) ? ((Tables.m_pieceTable.FileCharacterPos[num] - Tables.m_pieceTable.FileCharacterPos[num - 1]) * Tables.m_pieceTableEncodings[num - 1].GetByteCount("a")) : 0);
		long num7 = m_iStartPieceTablePos + num2;
		if (num6 > num7 && curPos < num7)
		{
			num6 = num7;
		}
		return num6;
	}

	internal bool UpdateCHPxEndPos(long iEndPos)
	{
		if (iEndPos >= m_iEndCHPxPos)
		{
			return NextChpx();
		}
		return false;
	}

	internal bool UpdatePAPxEndPos(long iEndPos)
	{
		if (iEndPos >= m_iEndPAPxPos)
		{
			return NextPapx();
		}
		return false;
	}

	internal bool IsFirstPass(long iPos)
	{
		return iPos < m_iStartText;
	}

	internal bool IsEndOfText(long iPos)
	{
		return iPos >= m_iEndText;
	}

	internal virtual bool UpdateItemEndPos(long iEndPos)
	{
		return true;
	}

	internal virtual int MoveToItem(int itemIndex)
	{
		return 0;
	}

	internal virtual bool IsEndOfSubdocItemText(long iPos)
	{
		return false;
	}

	protected void MoveToCurrentChpxPapx()
	{
		if (!IsCurrentPapxPosition)
		{
			while (m_iStartText >= m_iEndPAPxPos && NextPapx())
			{
			}
		}
		if (!IsCurrentChpxPosition)
		{
			while (m_iStartText >= m_iEndCHPxPos && NextChpx())
			{
			}
		}
	}
}
