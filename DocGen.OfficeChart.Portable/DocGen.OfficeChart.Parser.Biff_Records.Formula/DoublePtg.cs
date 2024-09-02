using System;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tNumber)]
internal class DoublePtg : Ptg
{
	private double m_value;

	public double Value
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

	[Preserve]
	public DoublePtg()
	{
	}

	[Preserve]
	public DoublePtg(double value)
	{
		TokenCode = FormulaToken.tNumber;
		Value = value;
	}

	[Preserve]
	public DoublePtg(string value)
		: this(value, null)
	{
	}

	[Preserve]
	public DoublePtg(string value, NumberFormatInfo numberInfo)
	{
		double value2 = ((numberInfo == null) ? double.Parse(value) : double.Parse(value, numberInfo));
		TokenCode = FormulaToken.tNumber;
		Value = value2;
	}

	[Preserve]
	public DoublePtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override int GetSize(OfficeVersion version)
	{
		return 9;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		if (numberFormat == null)
		{
			return m_value.ToString();
		}
		return m_value.ToString(numberFormat);
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberInfo)
	{
		return ToString(formulaUtil, iRow, iColumn, bR1C1, numberInfo, isForSerialization: false);
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		BitConverter.GetBytes(m_value).CopyTo(array, 1);
		return array;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_value = provider.ReadDouble(offset);
		offset += 8;
	}
}
