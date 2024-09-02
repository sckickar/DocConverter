using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class AtnStatePositions : StatePositionsBase
{
	internal AtnStatePositions(WordFKPData fkp)
		: base(fkp)
	{
	}

	internal override int MoveToItem(int itemIndex)
	{
		m_iItemIndex = itemIndex;
		int num = m_fkp.Fib.CcpText + m_fkp.Fib.CcpFtn + m_fkp.Fib.CcpHdd;
		if (m_iStartItemPos == 0)
		{
			m_iStartItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)num);
			m_iEndText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Fib.CcpAtn));
		}
		m_iStartText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.Annotations.GetTxtPosition(m_iItemIndex)));
		m_iEndItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.Annotations.GetTxtPosition(m_iItemIndex + 1)));
		MoveToCurrentChpxPapx();
		return m_iStartText;
	}

	internal override bool UpdateItemEndPos(long iEndPos)
	{
		if (iEndPos >= m_iEndItemPos)
		{
			uint num = (uint)(m_fkp.Fib.CcpText + m_fkp.Fib.CcpFtn + m_fkp.Fib.CcpHdd);
			m_iItemIndex++;
			m_iEndItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.Annotations.GetTxtPosition(m_iItemIndex + 1)));
			return true;
		}
		return false;
	}
}
