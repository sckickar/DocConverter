using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tMemErr1)]
[Token(FormulaToken.tMemErr2)]
[Token(FormulaToken.tMemErr3)]
internal class MemErrPtg : Ptg
{
	private const int SIZE = 7;

	private byte[] m_arrData = new byte[6];

	[Preserve]
	public MemErrPtg()
	{
	}

	[Preserve]
	public MemErrPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return "(MemErr not implemented.)";
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		m_arrData.CopyTo(array, 1);
		return array;
	}

	public override int GetSize(OfficeVersion version)
	{
		return 7;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		provider.CopyTo(offset, m_arrData, 0, m_arrData.Length);
		offset += m_arrData.Length;
	}
}
