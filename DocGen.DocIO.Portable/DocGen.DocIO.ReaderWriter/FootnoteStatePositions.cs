using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class FootnoteStatePositions : StatePositionsBase
{
	internal FootnoteStatePositions(WordFKPData fkp)
		: base(fkp)
	{
	}

	internal override int MoveToItem(int itemIndex)
	{
		m_iItemIndex = itemIndex;
		int ccpText = m_fkp.Fib.CcpText;
		if (m_iStartItemPos == 0)
		{
			m_iStartItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)ccpText);
			m_iEndText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(ccpText + m_fkp.Fib.CcpFtn));
		}
		m_iStartText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(ccpText + m_fkp.Tables.Footnotes.GetTxtPosition(m_iItemIndex)));
		m_iEndItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(ccpText + m_fkp.Tables.Footnotes.GetTxtPosition(m_iItemIndex + 1)));
		MoveToCurrentChpxPapx();
		return m_iStartText;
	}

	internal override bool UpdateItemEndPos(long iEndPos)
	{
		if (iEndPos >= m_iEndItemPos)
		{
			uint ccpText = (uint)m_fkp.Fib.CcpText;
			m_iItemIndex++;
			m_iEndItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(ccpText + m_fkp.Tables.Footnotes.GetTxtPosition(m_iItemIndex + 1)));
			return true;
		}
		return false;
	}
}
