using System;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tMemFunc1)]
[Token(FormulaToken.tMemFunc2)]
[Token(FormulaToken.tMemFunc3)]
[CLSCompliant(false)]
internal class MemFuncPtg : Ptg
{
	private const int SIZE = 3;

	private ushort m_usSize;

	private byte[] m_arrData = new byte[2];

	public ushort SubExpressionLength
	{
		get
		{
			return m_usSize;
		}
		set
		{
			m_usSize = value;
		}
	}

	[Preserve]
	public MemFuncPtg()
	{
	}

	[Preserve]
	public MemFuncPtg(int size)
	{
		if (size < 0 || size > 65535)
		{
			throw new ArgumentOutOfRangeException("size", "Value cannot be less than 0 or greater than ushort.MaxValue");
		}
		TokenCode = FormulaToken.tMemFunc1;
		m_usSize = (ushort)size;
	}

	[Preserve]
	public MemFuncPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override int GetSize(OfficeVersion version)
	{
		return 3;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return "( MemFunc not implemented ) Size is " + m_usSize;
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		BitConverter.GetBytes(m_usSize).CopyTo(array, 1);
		return array;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_usSize = provider.ReadUInt16(offset);
		offset += 2;
	}
}
