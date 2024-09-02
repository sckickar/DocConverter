using System;
using System.Globalization;
using System.Text;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tStringConstant)]
internal class StringConstantPtg : Ptg
{
	public string m_strValue = string.Empty;

	public byte m_compressed = 1;

	public string Value
	{
		get
		{
			return m_strValue;
		}
		set
		{
			if (value.Length > 255)
			{
				throw new ArgumentOutOfRangeException("value", "String is too long.");
			}
			m_strValue = value;
			m_compressed = 1;
		}
	}

	[Preserve]
	public StringConstantPtg()
	{
	}

	[Preserve]
	public StringConstantPtg(string value)
	{
		TokenCode = FormulaToken.tStringConstant;
		Value = value;
	}

	[Preserve]
	public StringConstantPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override int GetSize(OfficeVersion version)
	{
		if (m_compressed != 1)
		{
			return m_strValue.Length + 3;
		}
		return m_strValue.Length * 2 + 3;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		string text = m_strValue;
		if (text != null)
		{
			text = text.Replace("\"", "\"\"");
		}
		return "\"" + text + "\"";
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = ((m_compressed != 1) ? BiffRecordRaw.LatinEncoding.GetBytes(m_strValue) : Encoding.Unicode.GetBytes(m_strValue));
		byte[] array2 = new byte[array.Length + 3];
		array2[0] = (byte)TokenCode;
		array2[1] = (byte)m_strValue.Length;
		array2[2] = m_compressed;
		array.CopyTo(array2, 3);
		return array2;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_compressed = provider.ReadByte(offset + 1);
		m_strValue = provider.ReadString8Bit(offset, out var iFullLength);
		offset += iFullLength;
	}
}
