using System;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tBoolean)]
internal class BooleanPtg : Ptg
{
	private bool m_bValue;

	public bool Value
	{
		get
		{
			return m_bValue;
		}
		set
		{
			m_bValue = value;
		}
	}

	[Preserve]
	public BooleanPtg()
	{
	}

	[Preserve]
	public BooleanPtg(bool value)
	{
		TokenCode = FormulaToken.tBoolean;
		Value = value;
	}

	[Preserve]
	public BooleanPtg(string value)
		: this(bool.Parse(value))
	{
	}

	[Preserve]
	public BooleanPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override int GetSize(OfficeVersion version)
	{
		return 2;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return m_bValue.ToString();
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		BitConverter.GetBytes(m_bValue).CopyTo(array, 1);
		return array;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_bValue = provider.ReadBoolean(offset++);
	}
}
