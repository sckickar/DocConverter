using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tArray1)]
[Token(FormulaToken.tArray2)]
[Token(FormulaToken.tArray3)]
internal class ArrayPtg : Ptg, IAdditionalData, ICloneable
{
	public const byte DOUBLEVALUE = 1;

	public const byte STRINGVALUE = 2;

	public const byte BOOLEANVALUE = 4;

	public const byte ERRORCODEVALUE = 16;

	public static readonly string RowSeparator = ";";

	public static readonly string ColSeparator = ",";

	public const byte NilValue = 0;

	private byte m_ColumnNumber;

	private ushort m_usRowNumber;

	private object[,] m_arrCachedValue;

	public int AdditionalDataSize
	{
		get
		{
			int num = 0;
			if (m_arrCachedValue != null)
			{
				num += 3;
				object[,] arrCachedValue = m_arrCachedValue;
				foreach (object obj in arrCachedValue)
				{
					if (obj is double || obj is bool || obj is byte || obj == null)
					{
						num += 9;
						continue;
					}
					if (obj is string)
					{
						num += Encoding.Unicode.GetByteCount((string)obj) + 4;
						continue;
					}
					throw new ArrayTypeMismatchException("Unexpected type in tArray.");
				}
			}
			return num;
		}
	}

	[Preserve]
	public ArrayPtg()
	{
	}

	[Preserve]
	public ArrayPtg(string strFormula, FormulaUtil formulaParser)
	{
		strFormula = strFormula.Substring(1, strFormula.Length - 2);
		List<string> list = formulaParser.SplitArray(strFormula, formulaParser.ArrayRowSeparator);
		List<string>[] array = new List<string>[list.Count];
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			string strFormula2 = list[i];
			array[i] = formulaParser.SplitArray(strFormula2, formulaParser.OperandsSeparator);
			if (i > 0 && array[i].Count != array[i - 1].Count)
			{
				throw new ArgumentException("Each row in the tArray must have the same column number.");
			}
		}
		FillList(array, formulaParser);
		SetReferenceIndex(2);
	}

	[Preserve]
	public ArrayPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public int ReadArray(DataProvider provider, int offset)
	{
		m_ColumnNumber = provider.ReadByte(offset);
		m_usRowNumber = (ushort)provider.ReadInt16(offset + 1);
		return FillList(provider, offset + 3, m_ColumnNumber + 1, m_usRowNumber + 1);
	}

	private int FillList(DataProvider provider, int offset, int ColumnNumber, int RowNumber)
	{
		m_arrCachedValue = new object[RowNumber, ColumnNumber];
		for (int i = 0; i < RowNumber; i++)
		{
			for (int j = 0; j < ColumnNumber; j++)
			{
				byte b = provider.ReadByte(offset++);
				switch (b)
				{
				case 1:
				{
					double num = provider.ReadDouble(offset);
					m_arrCachedValue[i, j] = num;
					offset += 8;
					break;
				}
				case 2:
				{
					int iFullLength;
					string text = provider.ReadString16Bit(offset, out iFullLength);
					m_arrCachedValue[i, j] = text;
					offset += iFullLength;
					break;
				}
				case 4:
				{
					bool flag = provider.ReadBoolean(offset);
					m_arrCachedValue[i, j] = flag;
					offset += 8;
					break;
				}
				case 16:
					m_arrCachedValue[i, j] = provider.ReadByte(offset);
					offset += 8;
					break;
				case 0:
					m_arrCachedValue[i, j] = null;
					offset += 8;
					break;
				default:
					throw new ArgumentOutOfRangeException("Unknown type in tArray: " + b);
				}
			}
		}
		return offset;
	}

	private int FillList(byte[] data, int offset, int ColumnNumber, int RowNumber)
	{
		m_arrCachedValue = new object[RowNumber, ColumnNumber];
		for (int i = 0; i < RowNumber; i++)
		{
			for (int j = 0; j < ColumnNumber; j++)
			{
				byte b = data[offset++];
				switch (b)
				{
				case 1:
				{
					if (offset + 8 > data.Length)
					{
						throw new ArgumentOutOfRangeException("FillList: data array too small.");
					}
					double num = BitConverter.ToDouble(data, offset);
					m_arrCachedValue[i, j] = num;
					offset += 8;
					break;
				}
				case 2:
				{
					int iFullLength;
					string string16Bit = Ptg.GetString16Bit(data, offset, out iFullLength);
					m_arrCachedValue[i, j] = string16Bit;
					offset += iFullLength;
					break;
				}
				case 4:
				{
					if (offset + 8 > data.Length)
					{
						throw new ArgumentOutOfRangeException("FillList: data array too small.");
					}
					bool flag = BitConverter.ToBoolean(data, offset);
					m_arrCachedValue[i, j] = flag;
					offset += 8;
					break;
				}
				case 16:
					if (offset + 8 > data.Length)
					{
						throw new ArgumentOutOfRangeException("FillList: data array too small.");
					}
					m_arrCachedValue[i, j] = data[offset];
					offset += 8;
					break;
				default:
					throw new ArgumentOutOfRangeException("Unknown type in tArray: " + b);
				}
			}
		}
		return offset;
	}

	private void FillList(List<string>[] arrValues, FormulaUtil formulaParser)
	{
		int num = arrValues.Length;
		int count = arrValues[0].Count;
		m_ColumnNumber = (byte)(count - 1);
		m_usRowNumber = (ushort)(num - 1);
		m_arrCachedValue = new object[num, count];
		for (int i = 0; i < num; i++)
		{
			if (arrValues[i].Count != count)
			{
				m_arrCachedValue = null;
				throw new ArgumentException("Each row in the tArray must have same number of columns.");
			}
			for (int j = 0; j < count; j++)
			{
				m_arrCachedValue[i, j] = ParseConstant(arrValues[i][j], formulaParser);
			}
		}
	}

	private object ParseConstant(string value, FormulaUtil formulaParser)
	{
		if (value.Length == 0)
		{
			throw new ArgumentException("Constant string can't be empty.");
		}
		if (double.TryParse(value, NumberStyles.Any, formulaParser.NumberFormat, out var result))
		{
			return result;
		}
		try
		{
			if (value.ToLower() == "true" || value.ToLower() == "false")
			{
				return bool.Parse(value.ToLower());
			}
		}
		catch (FormatException)
		{
		}
		if (value[0] == '"' && '"' == value[value.Length - 1])
		{
			return value.Substring(1, value.Length - 2);
		}
		Ptg[] array = formulaParser.ParseString(value);
		if (array != null && array.Length == 1 && array[0] is ErrorPtg errorPtg)
		{
			return errorPtg.ErrorCode;
		}
		return null;
	}

	public BytesList GetListBytes()
	{
		BytesList bytesList = new BytesList(bExactSize: false);
		bytesList.Add(m_ColumnNumber);
		bytesList.AddRange(BitConverter.GetBytes(m_usRowNumber));
		object[,] arrCachedValue = m_arrCachedValue;
		foreach (object obj in arrCachedValue)
		{
			if (obj is double)
			{
				bytesList.Add(1);
				bytesList.AddRange(GetDoubleBytes((double)obj));
				continue;
			}
			if (obj is string)
			{
				bytesList.Add(2);
				bytesList.AddRange(GetStringBytes((string)obj));
				continue;
			}
			if (obj is bool)
			{
				bytesList.Add(4);
				bytesList.AddRange(GetBoolBytes((bool)obj));
				continue;
			}
			if (obj is byte)
			{
				bytesList.Add(16);
				bytesList.AddRange(GetErrorCodeBytes((byte)obj));
				continue;
			}
			if (obj == null)
			{
				bytesList.Add(0);
				bytesList.AddRange(GetNilBytes());
				continue;
			}
			throw new ArrayTypeMismatchException("Unexpected type in tArray.");
		}
		return bytesList;
	}

	private byte[] GetBoolBytes(bool value)
	{
		byte[] array = new byte[8];
		if (value)
		{
			array[0] = 1;
		}
		return array;
	}

	private byte[] GetErrorCodeBytes(byte value)
	{
		return new byte[8] { value, 0, 0, 0, 0, 0, 0, 0 };
	}

	private byte[] GetNilBytes()
	{
		return new byte[8];
	}

	private byte[] GetDoubleBytes(double value)
	{
		return BitConverter.GetBytes(value);
	}

	private byte[] GetStringBytes(string value)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(value);
		byte[] array = new byte[bytes.Length + 3];
		bytes.CopyTo(array, 3);
		BitConverter.GetBytes((ushort)value.Length).CopyTo(array, 0);
		array[2] = 1;
		return array;
	}

	private void SetReferenceIndex(int referenceIndex)
	{
		switch (referenceIndex)
		{
		case 1:
			TokenCode = FormulaToken.tArray1;
			break;
		case 2:
			TokenCode = FormulaToken.tArray2;
			break;
		case 3:
			TokenCode = FormulaToken.tArray3;
			break;
		default:
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public override int GetSize(OfficeVersion version)
	{
		return 8;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		string empty = string.Empty;
		empty += "{";
		string text = ColSeparator;
		string text2 = RowSeparator;
		if (formulaUtil != null)
		{
			text = formulaUtil.OperandsSeparator;
			text2 = formulaUtil.ArrayRowSeparator;
		}
		for (int i = 0; i <= m_usRowNumber; i++)
		{
			int j;
			for (j = 0; j < m_ColumnNumber; j++)
			{
				empty += ((m_arrCachedValue[i, j] is string) ? ("\"" + m_arrCachedValue[i, j].ToString() + "\"") : m_arrCachedValue[i, j]);
				empty += text;
			}
			empty += ((m_arrCachedValue[i, j] is string) ? ("\"" + m_arrCachedValue[i, j].ToString() + "\"") : m_arrCachedValue[i, m_ColumnNumber]);
			if (i != m_usRowNumber)
			{
				empty += text2;
			}
		}
		return empty + "}";
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		array[1] = 0;
		return array;
	}

	public static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tArray1, 
			2 => FormulaToken.tArray2, 
			3 => FormulaToken.tArray3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public new object Clone()
	{
		object result = base.Clone();
		object[,] arrCachedValue = m_arrCachedValue;
		if (m_arrCachedValue != null)
		{
			int length = arrCachedValue.GetLength(0);
			int length2 = arrCachedValue.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					m_arrCachedValue[i, j] = arrCachedValue[i, j];
				}
			}
		}
		return result;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		offset += GetSize(version) - 1;
	}
}
