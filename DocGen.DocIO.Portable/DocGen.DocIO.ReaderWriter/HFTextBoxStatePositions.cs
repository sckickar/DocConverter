using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class HFTextBoxStatePositions : TextBoxStatePositions
{
	internal HFTextBoxStatePositions(WordFKPData fkp)
		: base(fkp)
	{
	}

	internal override int MoveToItem(int itemIndex)
	{
		m_iItemIndex = itemIndex;
		int num = m_fkp.Fib.CcpText + m_fkp.Fib.CcpFtn + m_fkp.Fib.CcpAtn + m_fkp.Fib.CcpHdd + m_fkp.Fib.CcpEdn + m_fkp.Fib.CcpTxbx;
		if (m_iStartItemPos == 0)
		{
			m_iStartItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)num);
			m_iEndText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Fib.CcpHdrTxbx));
		}
		m_iStartText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.ArtObj.GetTxbxPosition(isHdrTxbx: true, m_iItemIndex)));
		m_iEndItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.ArtObj.GetTxbxPosition(isHdrTxbx: true, m_iItemIndex + 1)));
		MoveToCurrentChpxPapx();
		return m_iStartText;
	}
}
