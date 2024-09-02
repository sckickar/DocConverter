using System;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tInteger)]
[CLSCompliant(false)]
internal class IntegerPtg : Ptg
{
	public ushort m_usValue;

	public ushort Value
	{
		get
		{
			return m_usValue;
		}
		set
		{
			m_usValue = value;
		}
	}

	[Preserve]
	public IntegerPtg()
	{
	}

	[Preserve]
	public IntegerPtg(ushort value)
	{
		TokenCode = FormulaToken.tInteger;
		Value = value;
	}

	[Preserve]
	public IntegerPtg(string value)
		: this(ushort.Parse(value))
	{
	}

	[Preserve]
	public IntegerPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override int GetSize(OfficeVersion version)
	{
		return 3;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return m_usValue.ToString();
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		BitConverter.GetBytes(m_usValue).CopyTo(array, 1);
		return array;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_usValue = provider.ReadUInt16(offset);
		offset += 2;
	}
}
