using System.Collections.Generic;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[ErrorCode("#NULL!", 0)]
[ErrorCode("#DIV/0!", 7)]
[ErrorCode("#VALUE!", 15)]
[ErrorCode("#NAME?", 29)]
[ErrorCode("#NUM!", 36)]
[ErrorCode("#N/A", 42)]
[Token(FormulaToken.tError)]
internal class ErrorPtg : Ptg
{
	public static readonly Dictionary<string, int> ErrorNameToCode;

	public static readonly Dictionary<int, string> ErrorCodeToName;

	public const string DEF_ERROR_NAME = "#N/A";

	private byte m_errorCode;

	public byte ErrorCode
	{
		get
		{
			return m_errorCode;
		}
		set
		{
			m_errorCode = value;
		}
	}

	[Preserve]
	static ErrorPtg()
	{
		ErrorNameToCode = new Dictionary<string, int>(6);
		ErrorCodeToName = new Dictionary<int, string>(6);
		ErrorCodeAttribute[] array = new ErrorCodeAttribute[6]
		{
			new ErrorCodeAttribute("#NULL!", 0),
			new ErrorCodeAttribute("#DIV/0!", 7),
			new ErrorCodeAttribute("#VALUE!", 15),
			new ErrorCodeAttribute("#NAME?", 29),
			new ErrorCodeAttribute("#NUM!", 36),
			new ErrorCodeAttribute("#N/A", 42)
		};
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			ErrorCodeAttribute errorCodeAttribute = array[i];
			ErrorNameToCode.Add(errorCodeAttribute.StringValue, errorCodeAttribute.ErrorCode);
			ErrorCodeToName.Add(errorCodeAttribute.ErrorCode, errorCodeAttribute.StringValue);
		}
	}

	[Preserve]
	public ErrorPtg()
	{
	}

	[Preserve]
	public ErrorPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public ErrorPtg(int errorCode)
	{
		TokenCode = FormulaToken.tError;
		m_errorCode = (byte)errorCode;
	}

	[Preserve]
	public ErrorPtg(string errorName)
		: this(ErrorNameToCode[errorName])
	{
	}

	public override int GetSize(OfficeVersion version)
	{
		return 2;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		if (!ErrorCodeToName.TryGetValue(m_errorCode, out var value))
		{
			return "#N/A";
		}
		return value;
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		array[1] = m_errorCode;
		return array;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_errorCode = provider.ReadByte(offset++);
	}
}
