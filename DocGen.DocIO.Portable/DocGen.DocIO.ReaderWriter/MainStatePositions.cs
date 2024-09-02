using System;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class MainStatePositions : StatePositionsBase
{
	private int m_iEndSecPos;

	private int m_iCurrentSepxIndex;

	internal int SectionIndex => m_iCurrentSepxIndex;

	internal SectionPropertyException CurrentSepx => m_fkp.GetSepx(m_iCurrentSepxIndex);

	internal MainStatePositions(WordFKPData fkp)
		: base(fkp)
	{
	}

	internal bool NextSepx(out int iEndPos)
	{
		bool result = true;
		if (m_iCurrentSepxIndex < base.Tables.SectionsTable.Positions.Length - 2)
		{
			m_iCurrentSepxIndex++;
		}
		else
		{
			result = false;
		}
		iEndPos = base.Tables.SectionsTable.Positions[m_iCurrentSepxIndex + 1];
		return result;
	}

	internal bool UpdateSepxEndPos(long iEndPos)
	{
		bool result = false;
		if (iEndPos >= m_iEndSecPos)
		{
			if (NextSepx(out var iEndPos2))
			{
				m_iEndSecPos = (int)base.Tables.ConvertCharPosToFileCharPos((uint)iEndPos2);
				result = true;
			}
			else
			{
				m_iEndSecPos = -1;
			}
		}
		return result;
	}

	internal override void InitStartEndPos()
	{
		base.InitStartEndPos();
		m_iEndSecPos = (int)base.Tables.ConvertCharPosToFileCharPos((uint)base.Tables.SectionsTable.Positions[1]);
	}

	internal override long GetMinEndPos(long curPos)
	{
		return Math.Min(base.GetMinEndPos(curPos), m_iEndSecPos);
	}
}
