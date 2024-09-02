using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class CFVO
{
	private const ushort DEF_MINIMUM_SIZE = 3;

	private byte m_cfvoType = 1;

	private ushort m_formulaLength;

	private byte[] m_arrFormula = new byte[0];

	private Ptg[] m_arrFormulaParsed;

	private double m_numValue;

	private string m_value;

	public ushort FormulaSize => m_formulaLength;

	public Ptg[] FormulaPtgs
	{
		get
		{
			return m_arrFormulaParsed;
		}
		set
		{
			m_arrFormula = FormulaUtil.PtgArrayToByteArray(value, OfficeVersion.Excel2007);
			m_arrFormulaParsed = value;
			m_formulaLength = (ushort)m_arrFormula.Length;
		}
	}

	public byte[] FormulaBytes => m_arrFormula;

	public double NumValue
	{
		get
		{
			return m_numValue;
		}
		set
		{
			m_numValue = value;
		}
	}

	public string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	public int ParseCFVO(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_cfvoType = provider.ReadByte(iOffset);
		iOffset++;
		m_formulaLength = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_arrFormula = new byte[m_formulaLength];
		provider.ReadArray(iOffset, m_arrFormula);
		iOffset += m_formulaLength;
		m_arrFormulaParsed = FormulaUtil.ParseExpression(new ByteArrayDataProvider(m_arrFormula), m_formulaLength, version);
		if (version != OfficeVersion.Excel2007 && m_formulaLength > 0)
		{
			m_arrFormula = FormulaUtil.PtgArrayToByteArray(m_arrFormulaParsed, OfficeVersion.Excel2007);
			m_formulaLength = (ushort)m_arrFormula.Length;
		}
		return iOffset;
	}

	public int SerializeCFVO(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (m_arrFormulaParsed != null && m_arrFormulaParsed.Length != 0)
		{
			m_arrFormula = FormulaUtil.PtgArrayToByteArray(m_arrFormulaParsed, version);
			m_formulaLength = (ushort)m_arrFormula.Length;
		}
		else
		{
			m_arrFormula = null;
			m_formulaLength = 0;
		}
		provider.WriteByte(iOffset, m_cfvoType);
		iOffset++;
		provider.WriteUInt16(iOffset, m_formulaLength);
		iOffset += 2;
		provider.WriteBytes(iOffset, m_arrFormula, 0, m_formulaLength);
		iOffset += m_formulaLength;
		return iOffset;
	}

	public int GetStoreSize(OfficeVersion version)
	{
		return 3 + m_formulaLength;
	}

	internal void ClearAll()
	{
		m_arrFormula = null;
		m_arrFormulaParsed = null;
	}
}
