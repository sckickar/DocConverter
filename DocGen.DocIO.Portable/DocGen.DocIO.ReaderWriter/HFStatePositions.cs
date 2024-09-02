using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class HFStatePositions : StatePositionsBase
{
	private int m_iSectionIndex;

	private bool m_isNextItemText;

	internal int SectionIndex
	{
		get
		{
			return m_iSectionIndex;
		}
		set
		{
			m_iSectionIndex = value;
		}
	}

	internal HFStatePositions(WordFKPData fkp)
		: base(fkp)
	{
	}

	internal override int MoveToItem(int itemIndex)
	{
		m_iItemIndex = m_iSectionIndex * 6 + itemIndex;
		int num = m_fkp.Fib.CcpText + m_fkp.Fib.CcpFtn;
		if (m_iStartItemPos == 0)
		{
			m_iStartItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)num);
			m_iEndText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Fib.CcpHdd));
		}
		m_iStartText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.HeaderFooterCharPosTable.Positions[m_iItemIndex]));
		m_iEndItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.HeaderFooterCharPosTable.Positions[m_iItemIndex + 1]));
		MoveToCurrentChpxPapx();
		return m_iStartText;
	}

	internal void MoveToNextHeaderPos()
	{
		m_iItemIndex++;
		int num = m_fkp.Fib.CcpText + m_fkp.Fib.CcpFtn;
		m_iEndItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.HeaderFooterCharPosTable.Positions[m_iItemIndex + 1]));
	}

	internal bool UpdateHeaderEndPos(long iEndPos, HeaderType headerType)
	{
		m_isNextItemText = ((iEndPos >= m_iEndItemPos && headerType < HeaderType.FirstPageFooter) ? true : false);
		if (m_isNextItemText)
		{
			headerType++;
			MoveToNextHeaderPos();
		}
		else if (iEndPos >= m_iEndItemPos)
		{
			m_iEndItemPos = -1;
		}
		return m_isNextItemText;
	}

	internal override bool IsEndOfSubdocItemText(long iPos)
	{
		return m_isNextItemText;
	}
}
