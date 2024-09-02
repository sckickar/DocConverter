using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class TextBoxStatePositions : StatePositionsBase
{
	internal TextBoxStatePositions(WordFKPData fkp)
		: base(fkp)
	{
	}

	internal override int MoveToItem(int itemIndex)
	{
		m_iItemIndex = itemIndex;
		uint num = (uint)(m_fkp.Fib.CcpText + m_fkp.Fib.CcpFtn + m_fkp.Fib.CcpAtn + m_fkp.Fib.CcpHdd + m_fkp.Fib.CcpEdn);
		if (m_iStartItemPos == 0)
		{
			m_iStartItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos(num);
			m_iEndText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Fib.CcpTxbx));
		}
		m_iStartText = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.ArtObj.GetTxbxPosition(isHdrTxbx: false, m_iItemIndex)));
		m_iEndItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.ArtObj.GetTxbxPosition(isHdrTxbx: false, m_iItemIndex + 1)));
		MoveToCurrentChpxPapx();
		return m_iStartText;
	}

	internal override bool UpdateItemEndPos(long iEndPos)
	{
		if (iEndPos >= m_iEndItemPos)
		{
			uint num = (uint)(m_fkp.Fib.CcpText + m_fkp.Fib.CcpFtn + m_fkp.Fib.CcpAtn + m_fkp.Fib.CcpHdd + m_fkp.Fib.CcpEdn);
			bool flag = this is HFTextBoxStatePositions;
			if (flag)
			{
				num += (uint)m_fkp.Fib.CcpTxbx;
			}
			m_iItemIndex++;
			m_iEndItemPos = (int)m_fkp.Tables.ConvertCharPosToFileCharPos((uint)(num + m_fkp.Tables.ArtObj.GetTxbxPosition(flag, m_iItemIndex + 1)));
			return true;
		}
		return false;
	}
}
