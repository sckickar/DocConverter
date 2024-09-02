using System.Collections.Generic;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class CFIconSet
{
	private const ushort DEF_MINIMUM_SIZE = 6;

	private ushort m_undefined;

	private byte m_iconStates;

	private byte m_iconSet;

	private bool m_isIconOnly;

	private bool m_iconIsReversed;

	private List<CFIconMultiState> m_arrMultistate = new List<CFIconMultiState>();

	public ExcelIconSetType IconSetType
	{
		get
		{
			return (ExcelIconSetType)m_iconSet;
		}
		set
		{
			m_iconSet = (byte)value;
		}
	}

	public List<CFIconMultiState> ListCFIconSet
	{
		get
		{
			return m_arrMultistate;
		}
		set
		{
			m_arrMultistate = value;
		}
	}

	public ushort DefaultRecordSize => 6;

	public CFIconSet()
	{
		m_arrMultistate = new List<CFIconMultiState>();
	}

	private void CopyIconSet()
	{
	}

	public int ParseIconSet(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_undefined = provider.ReadUInt16(iOffset);
		iOffset += 2;
		provider.ReadByte(iOffset);
		iOffset++;
		m_iconStates = provider.ReadByte(iOffset);
		iOffset++;
		m_iconSet = provider.ReadByte(iOffset);
		iOffset++;
		m_isIconOnly = provider.ReadBit(iOffset, 0);
		provider.ReadBit(iOffset, 1);
		m_iconIsReversed = provider.ReadBit(iOffset, 2);
		iOffset++;
		for (int i = 0; i < m_iconStates; i++)
		{
			CFIconMultiState cFIconMultiState = new CFIconMultiState();
			iOffset = cFIconMultiState.ParseCFIconMultistate(provider, iOffset, version);
			m_arrMultistate.Add(cFIconMultiState);
		}
		CopyIconSet();
		return iOffset;
	}

	private byte CalculateIconOnlyAndReverseOrder()
	{
		byte result = 0;
		if (m_iconIsReversed && m_isIconOnly)
		{
			result = 5;
		}
		if (m_iconIsReversed && !m_isIconOnly)
		{
			result = 4;
		}
		if (!m_iconIsReversed && m_isIconOnly)
		{
			result = 1;
		}
		return result;
	}

	public int GetStoreSize(OfficeVersion version)
	{
		int num = 0;
		foreach (CFIconMultiState item in m_arrMultistate)
		{
			num += item.GetStoreSize(version);
		}
		return 6 + num;
	}

	internal void ClearAll()
	{
		foreach (CFIconMultiState item in m_arrMultistate)
		{
			item.ClearAll();
		}
		m_arrMultistate.Clear();
		m_arrMultistate = null;
	}
}
